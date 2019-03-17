using Common.Logging;
using PIEPER_JONES.TEST.LENDING.Api.Tests.Framework;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.DataAccess.EmailDataAccess;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows.PreSubmitWorkflows;

namespace PIEPER_JONES.TEST.LENDING.Api.Tests
{
    [TestFixture("QA")]
    [Category("Core")]
    [Category("Pipeline")]
    [Category("LoanApplicationStatus")]
    
    class PipelineLoanStatusTests : TestBase
    {
        private static readonly ILog log = LoanDepotLogManager.GetLogger();
        private static LoanApplicationClient loanApplicationClient = new LoanApplicationClient();
        private static BorrowersClient borrowersClient;
        private static RegisterBorrowerRequest borrowerRegistration;
        private static string emailAddress;
        private static string testEnvironment;
        public PipelineLoanStatusTests(string environment) : base(environment)
        {
            borrowersClient = new BorrowersClient();
            testEnvironment = environment;
        }

        [SetUp]
        public void SetUp()
        {
            borrowerRegistration = CoreModelConstructors.CreateRegisterBorrowerRequest();
            emailAddress = borrowerRegistration.Borrower.EmailAddress;
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateRegistering()
        {
            var partialBorrowerResult = borrowersClient.PartialRegistration(borrowerRegistration);
            if (partialBorrowerResult == null)
                Assert.Fail($"Could not create a partially registered borrower for email {emailAddress}.  Failing Test.");
            var stateResult = loanApplicationClient.Status(partialBorrowerResult.content.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Registering);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.RegistrationPending);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateQuoting()
        {
            var borrowerState = new LoanApplicationCoreStateObject();
            borrowerState = RegisterBorrower(borrowerRegistration);
            borrowerState = GetBorrowerToQuoting(borrowerRegistration, (Guid)borrowerState.LoanApplicationGuid);
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Quoting);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.OfferSelectPending);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateApplying()
        {
            var borrowerState = RegisterBorrower(borrowerRegistration);
            borrowerState = GetPrimaryOffer(borrowerRegistration, (Guid)borrowerState.LoanApplicationGuid);
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.AppStipsPending);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateAllStipsButEmailVerificationAndBankLinking()
        {
            var statusUpdateClient = new LoanApplicationStatusClient();
            var borrowerState = GetBorrowerWithAllStipsButEmailAndBankverification(borrowerRegistration);

            //All Stips are completed now besides Email and BankLinking...now fill out bank linking...
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.AppStipsPending);

            //Link Bank Account
            var request = CoreModelConstructors.CreateCoreLinkBankAccountRequest("wells", (Guid)borrowerState.LoanApplicationGuid, "plaid_test", "plaid_good", PIEPER_JONES.Common.Enums.BankProvider.Plaid);
            BankLinking(borrowerState.LoanApplicationGuid.ToString(), (Guid)borrowerState.BorrowerGuid, borrowerRegistration, request);
            //All Stips but EmailVerification let's verifyEmail
            var statusUpdateResponse = statusUpdateClient.UpdateStatusJob();
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);

            //Now that we are down to one REAL stip, we check for EmailOnlyPending
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.EmailOnlyPending);

            //Verify The Email Address and check status again
            EmailVerification(borrowerState.LoanApplicationGuid.ToString(), borrowerRegistration.Borrower.EmailAddress);

            //We should now be ready for the AppSubmitPending status
            statusUpdateResponse = statusUpdateClient.UpdateStatusJob();
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.AppSubmitPending);

            //Ready to Submit the LoanApplication now
            SubmitLoanApplication(borrowerState.LoanApplicationGuid.ToString(), borrowerRegistration);
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            statusUpdateResponse = statusUpdateClient.UpdateStatusJob();

            //Now to move to Review
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Reviewing);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.NewAppNeedInitialReview, $"Did not successfully transition to status NewAppNeedInitialReview, status is {stateResult.content.LoanApplicationStatus}.");
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateTransitionToDecline()
        {
            var borrowerState = TransitionToReviewingState(borrowerRegistration);
            var stateClient = new LoanApplicationStateClient();
            var noContent = new ResponseObject<EmptyResult>();

            noContent = stateClient.ChangeStateToDecline(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            List<string> emails = GetAdverseActionsEmails(emailAddress);
            Assert.That(emails.Count > 0);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateCoreTransitionToDecline()
        {
            var borrowerState = TransitionToReviewingState(borrowerRegistration);
            var stateClient = new LoanApplicationClient();
            var noContent = new ResponseObject<EmptyResult>();

            noContent = stateClient.ChangeStateToDecline(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            List<string> emails = GetAdverseActionsEmails(emailAddress);
            Assert.That(emails.Count > 0);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateTransitionToAccepting()
        {
            LoanApplicationCoreStateObject borrowerState = TransitionToReviewingState(borrowerRegistration);
            int emailCount = GetEmails(emailAddress).Count;
            var stateClient = new LoanApplicationStateClient();
            var noContent = new ResponseObject<EmptyResult>();

            noContent = loanApplicationClient
                .WorkflowAction(borrowerState.LoanApplicationGuid.ToString(), LoanApplicationWorkflowAction.Counter);
            noContent = loanApplicationClient
                .SetLoanAppToAccept(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Accepting);

            List<string> emails = GetEmails(emailAddress);
            Assert.That(emails.Count > emailCount);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateCoreTransitionToAccepting()
        {
            var borrowerState = TransitionToReviewingState(borrowerRegistration);
            int emailCount = GetEmails(emailAddress).Count;
            var stateClient = new LoanApplicationClient();
            var noContent = new ResponseObject<EmptyResult>();

            noContent = loanApplicationClient.SetLoanAppToAccept(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Accepting);

            List<string> emails = GetEmails(emailAddress);
            Assert.That(emails.Count > emailCount);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateTransitionRegisteringToWithdrawn()
        {
            var partialBorrowerResult = borrowersClient.PartialRegistration(borrowerRegistration);
            if (partialBorrowerResult == null)
            {
                Assert.Fail($"Could not create a partially registered borrower for email {emailAddress}.  Failing Test.");
            }

            var stateResult = loanApplicationClient.GetLoanApplication(partialBorrowerResult.content.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Registering);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.RegistrationPending);

            int emailCount = GetEmails(emailAddress).Count;
            var stateClient = new LoanApplicationStateClient();
            var noContent = new ResponseObject<EmptyResult>();
            noContent = stateClient.ChangeStateToWithdraw(partialBorrowerResult.content.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            stateResult = loanApplicationClient.GetLoanApplication(partialBorrowerResult.content.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Withdrawn);

            List<string> emails = GetEmails(emailAddress);
            Assert.That(emails.Count > emailCount);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateTransitionApplyingToWithdrawn()
        {
            var borrowerState = RegisterBorrower(borrowerRegistration);
            borrowerState = GetPrimaryOffer(borrowerRegistration, (Guid)borrowerState.LoanApplicationGuid);
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.AppStipsPending);

            int emailCount = GetEmails(emailAddress).Count;
            var stateClient = new LoanApplicationStateClient();
            var noContent = new ResponseObject<EmptyResult>();
            noContent = stateClient.ChangeStateToWithdraw(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Withdrawn);
            Assert.That(stateResult.content.LoanApplicationStatus == LoanApplicationStatus.WithdrawnIncomplete);

            List<string> emails = GetEmails(emailAddress);
            Assert.That(emails.Count > emailCount);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateTransitionReviewingToWithdrawn()
        {
            var borrowerState = TransitionToReviewingState(borrowerRegistration);
            int emailCount = GetEmails(emailAddress).Count;
            var stateClient = new LoanApplicationStateClient();
            var noContent = new ResponseObject<EmptyResult>();
            noContent = stateClient.ChangeStateToWithdraw(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Withdrawn);

            List<string> emails = GetEmails(emailAddress);
            Assert.That(emails.Count > emailCount);
        }

        [Test]
        [Category("Functional")]
        [Category("Transitions")]
        public void PipelineLoanStateTransitionAcceptingToWithdrawn()
        {
            var borrowerState = TransitionToReviewingState(borrowerRegistration);
            int emailCount = GetEmails(emailAddress).Count;
            var stateClient = new LoanApplicationStateClient();
            var noContent = new ResponseObject<EmptyResult>();
            noContent = loanApplicationClient.SetLoanAppToAccept(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Accepting);

            List<string> emails = GetEmails(emailAddress);
            emailCount = GetEmails(emailAddress).Count;
            noContent = stateClient.ChangeStateToWithdraw(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(noContent.statusCode == HttpStatusCode.OK);

            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Withdrawn);

            emails = GetEmails(emailAddress);
            Assert.That(emails.Count > emailCount);
        }
    }
}
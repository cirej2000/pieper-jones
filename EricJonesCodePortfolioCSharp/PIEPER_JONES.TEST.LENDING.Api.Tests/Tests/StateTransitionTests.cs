using Common.Logging;
using PIEPER_JONES.TEST.LENDING.Api.Tests.Framework;
using PIEPER_JONES.TEST.LENDING.Api.Tests.Tests.Parameters;
using NUnit.Framework;
using System;
using System.Net;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Utilities.AgentAssignments;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows.ApprovalWorkflow;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows.PreSubmitWorkflows;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows.StateTransitionWorkflows;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Tests.Validations;

namespace PIEPER_JONES.TEST.LENDING.Api.Tests
{
    [TestFixture("QA")]
    [Category("Core")]
    [Category("Transitions")]
    [Category("Functional")]

    
    class StateTransitionTests : TestBase
    {
        private static readonly ILog log = LoanDepotLogManager.GetLogger();
        private static LoanApplicationStatusClient coreLoanApplicationStatusClient;
        private static LoanApplicationStateClient loanApplicationStateClient;
        private static LoanApplicationClient loanApplicationClient;
        private static LoanApplicationBcdClient bcdClient;
        private static RegisterBorrowerRequest borrowerRegistration;
        private static string env;
        public StateTransitionTests(string environment) : base(environment)
        {
            env = environment;
            borrowerRegistration = CoreModelConstructors.CreateRegisterBorrowerRequest();
            loanApplicationClient = new LoanApplicationClient();
            bcdClient = new LoanApplicationBcdClient();
            coreLoanApplicationStatusClient = new LoanApplicationStatusClient();
            loanApplicationStateClient = new LoanApplicationStateClient();
        }

        [Test]
        public void NewAppNeedInitialToInReviewNeedUwDecision()
        {
            LoanApplicationCoreStateObject loanApplicationInfo = TransitionToReviewingState(borrowerRegistration);
            ResponseObject<GetLoanApplicationStatusResponse> response = coreLoanApplicationStatusClient
                .GetStatusByLoanApplication(loanApplicationInfo.LoanApplicationGuid.ToString());
            if (response.content == null 
                || response.content.LoanApplicationState != LoanApplicationState.Reviewing 
                || response.content.LoanApplicationStatus != LoanApplicationStatus.NewAppNeedInitialReview)
            {
                var message = response.content == null ? "No response was returned, null content object." : $"State = {response.content.LoanApplicationState.ToString()}, Status = {response.content.LoanApplicationStatus.ToString()}";
                log.Error("Failing test due to invalid setup state for our loan application...");
                Assert.Fail($"Loan Application State or Status not in the valid state or status; unable to continue - {message}");
            }

            AssignAgentToLoanApplication((Guid)loanApplicationInfo.LoanApplicationGuid, AgentRole.UW, "clpuw");
            var coreLoanAppClient = new LoanApplicationClient();
            var workflowResponse = coreLoanAppClient.WorkflowAction(loanApplicationInfo.LoanApplicationGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
            Assert.That(workflowResponse.statusCode == HttpStatusCode.OK, $"Invalid status code returned when attempting workflow transition:  {workflowResponse.statusCode}");
            log.Info("Checking the actual status of our loan application after transitioning to SendToUW");

            var statusResponse = coreLoanApplicationStatusClient.GetStatusByLoanApplication(loanApplicationInfo.LoanApplicationGuid.ToString());
            log.Info("transitioning the status...");

            coreLoanApplicationStatusClient
                .UpdateStatusByLoanApplication(loanApplicationInfo.LoanApplicationGuid.ToString());
            Assert.That(statusResponse.statusCode == HttpStatusCode.OK, $"We received an invalid status code from our core loanapplication status request:  {statusResponse.statusCode}.");
            Assert.NotNull(statusResponse.content, "We have a response but the response content is null for our status request");
            ValidateApplicationState(statusResponse.content, LoanApplicationState.Reviewing, LoanApplicationStatus.InReviewNeedDecision);
            log.Info("We have successfully transitioned our loan application into the Reviewing/InReviewNeedDecision.  Now we must check to see if we have an Agent assigned.");
        }

        [TestCaseSource(typeof(ReviewingStateTransitionsCSVParameters), "GetStatusScenarios")]
        public void ReviewingTransitionStatuses(string testcaseName, LoanApplicationStatus startingStatus, LoanApplicationWorkflowAction action, LoanApplicationState expectedState, LoanApplicationStatus expectedStatus, string agentName, AgentRole agentRole)
        {
            var loanApplicationInfo = TransitionToReviewingState(borrowerRegistration);
            // If we're already are at InReviewNeedDecision, then we don't need to do any further transitioning for our stating status.
            if (startingStatus != LoanApplicationStatus.NewAppNeedInitialReview)
                if (!ReviewStatusTransitions(loanApplicationInfo.LoanApplicationGuid, startingStatus))
                    Assert.Fail($"Unable to transition to the desired loan status of {startingStatus.ToString()}...stopping test.");
            log.Info($"We're in the expected status of {startingStatus.ToString()}.  Transitioning to {expectedStatus.ToString()}.");

            //If we provide an agentname, that means we need to add a new agent with the given role to the BCD
            if (!String.IsNullOrEmpty(agentName))
                AssignAgentToLoanApplication((Guid)loanApplicationInfo.LoanApplicationGuid, AgentRole.UWM, "clpuwm");

            var workflowResponse = loanApplicationClient.WorkflowAction(loanApplicationInfo.LoanApplicationGuid.ToString(), action);
            Assert.That(workflowResponse.statusCode == HttpStatusCode.OK, $"We weren't able to successfully run the workflow transition request successfully.  Status code {workflowResponse.statusCode}");
            log.Info($"Checking to see that our loan application in the expected {expectedState}/{expectedStatus} state/status.");

            coreLoanApplicationStatusClient
                .UpdateStatusByLoanApplication(loanApplicationInfo.LoanApplicationGuid.ToString());

            var statusResponse = coreLoanApplicationStatusClient
                .GetStatusByLoanApplication(loanApplicationInfo
                .LoanApplicationGuid.ToString());

            Assert.That(statusResponse.statusCode == HttpStatusCode.OK, $"We weren't able to get the status of our loanapplication successfully.  Status Code {statusResponse.statusCode.ToString()}");
            Assert.NotNull(statusResponse.content, "We received no content for our status request.");
            ValidateApplicationState(statusResponse.content, expectedState, expectedStatus);
            log.Info($"Validated the successful transition from {startingStatus} to {expectedStatus} loan application guid:  {loanApplicationInfo.LoanApplicationGuid}");
        }

        [TestCaseSource(typeof(ReviewingStateTransitionsCSVParameters), "GetStateScenarios")]
        public void ReviewingTransitionStates(string testcaseName, LoanApplicationStatus startingStatus,
            LoanApplicationState toState, LoanApplicationState expectedState, LoanApplicationStatus expectedStatus)
        {
            var loanApplicationInfo = TransitionToReviewingState(borrowerRegistration);
            var loanApplicationGuid = loanApplicationInfo.LoanApplicationGuid.ToString();

            // If we're already are at InReviewNeedDecision, then we don't need to do any further transitioning for our stating status.
            if (startingStatus != LoanApplicationStatus.NewAppNeedInitialReview)
            {
                if (!ReviewStatusTransitions(loanApplicationInfo.LoanApplicationGuid, startingStatus))
                {
                    Assert.Fail($"Unable to transition to the desired loan status of {startingStatus.ToString()}...stopping test.");
                }
            }
            log.Info($"We're in the expected status of {startingStatus.ToString()}.  Transitioning to {expectedState}...");

            var stateUpdateResponse = new ResponseObject<EmptyResult>();

            switch (toState)
            {
                case LoanApplicationState.Accepting:
                case LoanApplicationState.Finalizing:
                    stateUpdateResponse = loanApplicationClient.SetLoanAppToAccept(loanApplicationGuid);
                    break;
                case LoanApplicationState.Expired:
                    stateUpdateResponse = loanApplicationClient.SetLoanAppToExpire(loanApplicationGuid);
                    break;
                case LoanApplicationState.Declined:
                    stateUpdateResponse = loanApplicationStateClient.ChangeStateToDecline(loanApplicationGuid);
                    break;
                case LoanApplicationState.Withdrawn:
                    stateUpdateResponse = loanApplicationStateClient.ChangeStateToWithdraw(loanApplicationGuid);
                    break;
                default:
                    Assert.Fail($"{toState.ToString()} is in invalid argument for this test...failing.");
                    break;
            }

            Assert.That(stateUpdateResponse.statusCode == HttpStatusCode.OK 
                        || (stateUpdateResponse.statusCode == HttpStatusCode.NoContent), 
                        $"We weren't able to successfully run the workflow transition request successfully.  Status code {stateUpdateResponse.statusCode}");
            log.Info($"Checking to see that our loan application in the expected Declined/Declined state/status.");

            coreLoanApplicationStatusClient
                .UpdateStatusByLoanApplication(loanApplicationInfo.LoanApplicationGuid.ToString());
            var statusResponse = coreLoanApplicationStatusClient
                .GetStatusByLoanApplication(loanApplicationInfo.LoanApplicationGuid.ToString());
            Assert.That(statusResponse.statusCode == HttpStatusCode.OK, $"We weren't able to get the status of our loanapplication successfully.  Status Code {statusResponse.statusCode.ToString()}");
            Assert.NotNull(statusResponse.content, "We received no content for our status request.");

            ValidateApplicationState(statusResponse.content, expectedState, expectedStatus);
            log.Info($"Validated the successful transition from {startingStatus} to {expectedState}/{expectedStatus} for loan application guid:  {loanApplicationInfo.LoanApplicationGuid}");
        }

        [Test]
        public void EscalatedNeedUWManagerReviewToEditRequestedNeedUWReview()
        {
            var loanApplicationInfo = TransitionToReviewingState(borrowerRegistration);
            var loanApplicationGuid = loanApplicationInfo.LoanApplicationGuid.ToString();

            // If we're already are at InReviewNeedDecision, then we don't need to do any further transitioning for our stating status.
            if (!ReviewStatusTransitions(loanApplicationInfo.LoanApplicationGuid, LoanApplicationStatus.EscalatedNeedUWManagerReview))
            {
                Assert.Fail($"Unable to transition to the desired loan status of {LoanApplicationStatus.EscalatedNeedUWManagerReview.ToString()}...stopping test.");
            }
            log.Info($"We're in the expected status of {LoanApplicationStatus.EscalatedNeedUWManagerReview.ToString()}.  Transitioning to {LoanApplicationStatus.CounteredPendingAccept}...");


            UpdateLoanApplicationPassStips(loanApplicationInfo.LoanApplicationGuid.ToString());
            DispositionOfUploadedFiles(loanApplicationInfo.LoanApplicationGuid.ToString());
            var workflowResponse = loanApplicationClient.WorkflowAction(loanApplicationGuid, LoanApplicationWorkflowAction.Approve);
            Assert.That(workflowResponse.statusCode == HttpStatusCode.OK
                    || (workflowResponse.statusCode == HttpStatusCode.NoContent),
                    $"Invalid status code returned when attempting workflow transition:  {workflowResponse.statusCode}");
            log.Info("Checking the actual status of our loan application after transitioning to Counter");

            var statusResponse = coreLoanApplicationStatusClient.GetStatusByLoanApplication(loanApplicationGuid);
            Assert.That(statusResponse.statusCode == HttpStatusCode.OK
                    || (statusResponse.statusCode == HttpStatusCode.NoContent), 
            $"We weren't able to get the status of our loanapplication successfully.  Status Code {statusResponse.statusCode.ToString()}");
            Assert.NotNull(statusResponse.content, "We received no content for our status request.");
            ValidateApplicationState(statusResponse.content, LoanApplicationState.Accepting, LoanApplicationStatus.ApprovedPendingNote);
            log.Info("We are now in a state of Accepting with a status of CounteredPendingAccept.  Now resubmit to UW.");

            workflowResponse = loanApplicationClient.WorkflowAction(loanApplicationGuid, LoanApplicationWorkflowAction.ReturnToUW);
            Assert.That(statusResponse.statusCode == HttpStatusCode.OK
                    || (statusResponse.statusCode == HttpStatusCode.NoContent), 
                    $"Invalid status code returned when attempting workflow transition:  {workflowResponse.statusCode}");
            log.Info("Checking the actual status of our loan application after peforming the ReturnToUW workflow Action...checking loanapp status...");

            coreLoanApplicationStatusClient.UpdateStatusByLoanApplication(loanApplicationGuid);
            statusResponse = coreLoanApplicationStatusClient.GetStatusByLoanApplication(loanApplicationGuid);
            Assert.That(statusResponse.statusCode == HttpStatusCode.OK
                    || (statusResponse.statusCode == HttpStatusCode.NoContent),
                    $"We weren't able to get the status of our loanapplication successfully.  Status Code {statusResponse.statusCode.ToString()}");
            Assert.NotNull(statusResponse.content, "We received no content for our status request.");
            ValidateApplicationState(statusResponse.content, LoanApplicationState.Reviewing, LoanApplicationStatus.EditRequestNeedUWReview);
            log.Info($"Validated the successful transition from EscalatedNeedUWManagerReview to Reviwing/EditRequestedNeedUWReview for loan application guid:  {loanApplicationGuid}");
        }
    }
}
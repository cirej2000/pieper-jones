using Common.Logging;
using PIEPER_JONES.TEST.LENDING.Api.Tests.Framework;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.CoreModelConstructors;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.DataAccess.AgentDataAccess;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows.PreSubmitWorkflows;

namespace PIEPER_JONES.TEST.LENDING.Api.Tests
{
    [TestFixture("QA")]
    [Category("Core")]
    [Category("Pipeline")]
    [Category("Functional")]

    class PipelineAgentStatusAccessTests : TestBase
    {
        private static readonly ILog log = LoanDepotLogManager.GetLogger();
        private static LoanApplicationClient loanApplicationClient = new LoanApplicationClient();
        private static AgentsClient coreAgentClient;
        private static RegisterBorrowerRequest borrowerRegistration;
        private static string emailAddress;
        private static string env;

        public PipelineAgentStatusAccessTests(string environment) : base(environment)
        {
            coreAgentClient = new AgentsClient();
            borrowerRegistration = CoreModelConstructors.CreateRegisterBorrowerRequest();
            env = environment;
        }

        [Test]
        public void AgentSearchRegistering()
        {
            List<AgentIdObject> agents = GetActiveAgents();
            AgentIdObject agent = agents[0];
            //Let's make this agent an MLO
            var agentRequest = new SetAgentRolesRequest();
            agentRequest.Roles = new List<string>{"MLO"};
            var agentUpdate = coreAgentClient.SetAgentRoles(agentRequest, agent.agentGuid);

            //Partially Register a Borrower
            var request = CreateRegisterBorrowerRequest();
            emailAddress = request.Borrower.EmailAddress;
            var partialBorrowerResult = new BorrowersClient().PartialRegistration(request);

            if (partialBorrowerResult == null)
            {
                Assert.Fail($"Could not create a partially registered borrower for email {emailAddress}.  Failing Test.");
            }
        }

        [Test]
        public void AgentSearchQuoting()
        {
            emailAddress = GetRandomEmail();
            borrowerRegistration.Borrower.EmailAddress = emailAddress;
            var borrowerState = new LoanApplicationCoreStateObject();
            borrowerState = RegisterBorrower(borrowerRegistration);
            borrowerState = GetBorrowerToQuoting(borrowerRegistration, (Guid)borrowerState.LoanApplicationGuid);
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Quoting);
        }

        [Test]
        [Category("Functional")]
        public void AgentSearchApplying()
        {
            emailAddress = GetRandomEmail();
            borrowerRegistration.Borrower.EmailAddress = emailAddress;
            var borrowerState = RegisterBorrower(borrowerRegistration);
            borrowerState = GetPrimaryOffer(borrowerRegistration, (Guid)borrowerState.LoanApplicationGuid);
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);
        }

        [Test]
        public void AgentSearchReviewing()
        {
            emailAddress = GetRandomEmail();
            borrowerRegistration.Borrower.EmailAddress = emailAddress;
            var borrowerState = GetBorrowerWithAllStipsButEmailAndBankverification(borrowerRegistration);
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);

            EmailVerification(borrowerState.LoanApplicationGuid.ToString(), borrowerRegistration.Borrower.EmailAddress);
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);

            var request = CoreModelConstructors.CreateCoreLinkBankAccountRequest("wells", (Guid)borrowerState.LoanApplicationGuid, "plaid_test", "plaid_good", PIEPER_JONES.Common.Enums.BankProvider.Plaid);
            BankLinking(borrowerState.LoanApplicationGuid.ToString(), (Guid)borrowerState.BorrowerGuid, borrowerRegistration, request);
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Applying);

            SubmitLoanApplication(borrowerState.LoanApplicationGuid.ToString(), borrowerRegistration);
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            Assert.That(stateResult.content.LoanApplicationState == LoanApplicationState.Reviewing);
        }


    }
}
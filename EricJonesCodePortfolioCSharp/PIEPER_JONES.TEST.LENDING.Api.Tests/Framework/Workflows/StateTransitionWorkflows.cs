using Common.Logging;
using PIEPER_JONES.Common.Enums;
using PIEPER_JONES.Core.Api.Models.LoanApplications;
using PIEPER_JONES.TEST.LENDING.Api.Common.Core.ClientObjects;
using PIEPER_JONES.TEST.Common.API.Rest;
using System;
using System.Net;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Utilities.AgentAssignments;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows.ApprovalWorkflow;

namespace PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows
{
    public static class StateTransitionWorkflows
    {
        private static readonly ILog log = LoanDepotLogManager.GetLogger();

        public static bool ReviewStatusTransitions(Guid? loanAppInfoGuid, LoanApplicationStatus targetStatus)
        {
            var client = new LoanApplicationClient();

            switch (targetStatus)
            {
                case (LoanApplicationStatus.InReviewNeedDecision):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    break;
                case (LoanApplicationStatus.EscalatedNeedFAReview):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FA, "clpfa");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.EscalateToFA);
                    break;
                case (LoanApplicationStatus.EscalatedNeedFAManagerReview):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FA, "clpfa");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.EscalateToFA);
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FAM, "clpfam");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.EscalateToManager);
                    break;
                case (LoanApplicationStatus.EscalatedNeedUWManagerReview):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UWM, "clpuwm");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.EscalateToManager);
                    break;
                case (LoanApplicationStatus.EditRequestNeedUWReview):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    UpdateLoanApplicationPassStips(loanAppInfoGuid.ToString());
                    DispositionOfUploadedFiles(loanAppInfoGuid.ToString(), "Approve");
                    SignPromissoryAndFTIL(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Counter);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Approve);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    break;
                case (LoanApplicationStatus.ReturnedfromVerbalNeedUWReview):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FA, "clpfa");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    UpdateLoanApplicationPassStips(loanAppInfoGuid.ToString());
                    DispositionOfUploadedFiles(loanAppInfoGuid.ToString());
                    SignPromissoryAndFTIL(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Counter);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Approve);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Escalate);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    break;
                case (LoanApplicationStatus.ReturnedfromFundingNeedUWManagerReview):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UWM, "clpuwm");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    UpdateLoanApplicationPassStips(loanAppInfoGuid.ToString());
                    DispositionOfUploadedFiles(loanAppInfoGuid.ToString());
                    SignPromissoryAndFTIL(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Counter);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Approve);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.VerbalQueue);
                    VerbalVerification(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.PassVerbal);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.ReturnToUW);
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.EscalateToManager);
                    break;
                case (LoanApplicationStatus.ApprovedPendingNote):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FA, "clpfa");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    UpdateLoanApplicationPassStips(loanAppInfoGuid.ToString());
                    DispositionOfUploadedFiles(loanAppInfoGuid.ToString());
                    SignPromissoryAndFTIL(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Approve);
                    break;
                case (LoanApplicationStatus.NoteSignedNeedVerbal):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FA, "clpfa");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    UpdateLoanApplicationPassStips(loanAppInfoGuid.ToString());
                    DispositionOfUploadedFiles(loanAppInfoGuid.ToString());
                    SignPromissoryAndFTIL(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Counter);
                    SignLoanPacket(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.VerbalQueue);
                    break;
                case (LoanApplicationStatus.VerbalVerifiedNeedFunding):
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.UW, "clpuw");
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FA, "clpfa");
                    AssignAgentToLoanApplication((Guid)loanAppInfoGuid, AgentRole.FAM, "clpfam");
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.SendToUW);
                    UpdateLoanApplicationPassStips(loanAppInfoGuid.ToString());
                    DispositionOfUploadedFiles(loanAppInfoGuid.ToString());
                    SignPromissoryAndFTIL(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.Counter);
                    SignLoanPacket(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.VerbalQueue);
                    VerbalVerification(loanAppInfoGuid.ToString());
                    client.WorkflowAction(loanAppInfoGuid.ToString(), LoanApplicationWorkflowAction.PassVerbal);
                    break;
                default:
                    log.Error($"Functionality for LoanApplicationStatus {targetStatus.ToString()} is Not Available for this method");
                    return false;
            }

            var statusClient = new LoanApplicationStatusClient();
            statusClient.UpdateStatusByLoanApplication(loanAppInfoGuid.ToString());
            var statusResponse = statusClient.GetStatusByLoanApplication(loanAppInfoGuid.ToString());

            if (statusResponse.statusCode != HttpStatusCode.OK || statusResponse.content == null)
            {
                log.Error($"Error in checking the response of our status request (null content or invalid statuscode).");
                return false;
            }
            else
            {
                if (statusResponse.content.LoanApplicationStatus == targetStatus)
                    return true;
                else
                {
                    log.Error($"Wrong status generated for our request to transition to {targetStatus.ToString()} we received {statusResponse.content.LoanApplicationStatus.ToString()}.");
                    return false;
                }
            }
        }

    }
}

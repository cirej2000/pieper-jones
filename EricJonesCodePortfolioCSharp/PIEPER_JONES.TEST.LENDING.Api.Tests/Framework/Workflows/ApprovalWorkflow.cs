using Common.Logging;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.CoreModelConstructors;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.DataAccess.AgentDataAccess;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.DataAccess.LoanApplicationDataAccess;


namespace PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows
{
    public class ApprovalWorkflow
    {
        private static readonly ILog log = LoanDepotLogManager.GetLogger();

        public static void SignPromissoryAndFTIL(string loanApplicationGuid)
        {
            var request = new DocumentSigningByTypeRequest();
            request.GeneratedDocumentTypes = new List<GeneratedDocumentType>
                    { GeneratedDocumentType.FinalTIL, GeneratedDocumentType.PromissoryNote, GeneratedDocumentType.EFTAgreement };
            request.IpAddress = "127.0.0.1";
            Dictionary<string, string> headers = new Dictionary<string, string>() { { "Authorization", CLPCoreUWAuth } };
            /*      ResponseObject<EmptyResult> response = new VanillaRestClient(RestClientBase.environment).VanillaRequest<EmptyResult>(CLPCoreApiHost,
                                                            $"{ApiBasePath}/loanapplications/{loanApplicationGuid}/generateddocuments/signbytype",
                                                            RestSharp.Method.PUT, headers, new System.Collections.Specialized.NameValueCollection(), request);*/

            var response = new LoanAppGenDocClient().SignApplicationDocs(loanApplicationGuid, request);
        }

        public static void VerbalVerification(string applicationGuid)
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Resources\\";
            var loanApplicationTasksClient = new LoanApplicationTasksClient();
            var resp = loanApplicationTasksClient.GetTasksList(applicationGuid);
            var uploadClient = new LoanApplicationUploadedDocumentsClient();
            var request = new CreateUploadedDocumentRequest();

            request.Category = UploadedDocumentCategory.VerbalVerify;
            request.LoanApplicationGuid = (Guid?)new Guid(applicationGuid);
            request.Type = UploadedDocumentType.VerbalVerificationRecording;
            request.Description = "Verbal Verify Recording";
            request.Name = UploadedDocumentType.VerbalVerificationRecording.ToString();

            var uploadResponse = uploadClient.UploadDocument(request, "VerbalVerification.wav", filePath, "multipart/form-data");
            var note = BuildNoteRequest(EntityType.UploadedDocument, uploadResponse.content.Guid, UploadedDocumentType.VerbalVerificationRecording.ToString(), true,
                            $"{UploadedDocumentType.VerbalVerificationRecording} Doc uploaded", AgentNoteCategory.DocumentUpload, AgentRole.UW,
                            $"{UploadedDocumentType.VerbalVerificationRecording.ToString()} Document uploaded.");
            var agents = GetAgentsByRole(note.CreatorRole.ToString());
            note.CreatedBy = (Guid)agents[new Random().Next(0, agents.Count)].agentGuid;
            var noteCreated = new LoanApplicationNotesClient().PostNote(note, applicationGuid);
            var disposition = new UploadedDocumentsClient().DocumentDisposition(uploadResponse.content.Guid.ToString(), "Reviewed");

            request.Type = UploadedDocumentType.VerbalVerification;
            request.Description = "Verbal Verification";
            request.Name = UploadedDocumentType.VerbalVerification.ToString();
            uploadResponse = uploadClient.UploadDocument(request, "DivorceDecree.pdf", filePath, "multipart/form-data");
            note = BuildNoteRequest(EntityType.UploadedDocument, uploadResponse.content.Guid, UploadedDocumentType.VerbalVerification.ToString(), true,
                            $"{UploadedDocumentType.VerbalVerification} Doc uploaded", AgentNoteCategory.DocumentUpload, AgentRole.UW,
                            $"{UploadedDocumentType.VerbalVerification.ToString()} Document uploaded.");
            agents = GetAgentsByRole(note.CreatorRole.ToString());
            note.CreatedBy = (Guid)agents[new Random().Next(0, agents.Count)].agentGuid;
            noteCreated = new LoanApplicationNotesClient().PostNote(note, applicationGuid);
            disposition = new UploadedDocumentsClient().DocumentDisposition(uploadResponse.content.Guid.ToString(), "Pass");
        }

        public static void DispositionOfUploadedFiles(string loanApplicationGuid, string disposition = null)
        {
            var dispo = new ResponseObject<EmptyResult>();
            var uploadClient = new UploadedDocumentsClient();
            var tasks = new LoanApplicationTasksClient().GetTasksList(loanApplicationGuid);
            var uploadedDocsList = GetFileUploadGuids(loanApplicationGuid);
            foreach (var g in uploadedDocsList)
            {
                dispo = uploadClient.DocumentDisposition(g, (String.IsNullOrEmpty(disposition)
                                                              ? "Verified" : disposition));
            }
        }

        public static ResponseObject<GetOffersResponse> UpdateLoanApplicationPassStips(string loanApplicationGuid)
        {
            var loanAppClient = new LoanApplicationClient();
            var loanApp = loanAppClient.GetLoanApplication(loanApplicationGuid);
            var saveLoanAppRequest = new UpdateLoanApplicationRequest();
            saveLoanAppRequest.LoanApplication = loanApp.content;

            saveLoanAppRequest.LoanApplicationVerificationFlags = new LoanApplicationVerificationFlags();
            saveLoanAppRequest.LoanApplicationVerificationFlags.AccountHolderVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.AccountNumberVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.AddressVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.DobVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.EmailVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.EmployerNameVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.EmploymentStatusVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.NameVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.OtherContactNumberVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.OwnRentVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.PrimaryPhoneNumberVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.SsnVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.WorkAddressVerified = true;
            saveLoanAppRequest.LoanApplicationVerificationFlags.WorkPhoneNumberVerified = true;
            saveLoanAppRequest.VerifiedBorrowerIncome = loanApp.content.LoanFile.Borrowers[0].AnnualIncome;
            saveLoanAppRequest.VerifiedBorrowerDebt = loanApp.content.LoanFile.Debt[0].Amount;
            //saveLoanAppRequest.VerifiedBorrowerIncome
            //Dispositions for uploaded files
            var docGuids = GetFileUploadGuids(loanApplicationGuid);

            foreach (string dg in docGuids)
            {
                ResponseObject<EmptyResult> disposition = new UploadedDocumentsClient().DocumentDisposition(dg, "Pass");
            }

            var findAlertsRequest = new SearchAlertFlagsRequest();
            findAlertsRequest.LoanApplicationGuid = new Guid(loanApplicationGuid);
            var alertsClient = new AlertFlagClient();
            var alerts = alertsClient.AlertFlagSearch(findAlertsRequest);
            var agentGuid = (Guid)GetAgentByUserName("uw").agentGuid;
            var agent = GetAgentByUserName("uw");

            if (ClearAlerts(alerts.content, agent) == false)
                Assert.Fail("Cannot continue because alerts could not be cleared.");

            var findDataObjectsRequest = new SearchDataObjectRequest();
            findDataObjectsRequest.LoanApplicationGuid = new Guid(loanApplicationGuid);
            var dataObjectClient = new DataObjectClient();
            var dataObjects = dataObjectClient.DataObjectSearch(findDataObjectsRequest);

            if (DispositionDataObjects(dataObjects.content, agent, Disposition.FalsePositive) == false)
                Assert.Fail("Cannot continue because data object could not be dispositioned.");

            var response = loanAppClient.SaveLoanApplicationUW(saveLoanAppRequest);
            var hardPullResponse = new LoanApplicationThirdPartyClient().RunHardPull(loanApplicationGuid);
            var reprice = new LoanApplicationOffersClient().Reprice(loanApplicationGuid);

            return reprice;
        }
        public static ResponseObject<EmptyResult> SignLoanPacket(string loanApplicationGuid)
        {
            var response = new ResponseObject<EmptyResult>();
            var headers = new Dictionary<string, string>() { { "Authorization", CLPCoreUWAuth } };
            response = new VanillaRestClient("").VanillaRequest<EmptyResult>(
                                               CLPCoreApiHost, $"{ApiBasePath}/loanapplications/{loanApplicationGuid}/accept",
                                               Method.PUT, headers, null, null);
            return response;
        }

        public static bool ClearAlerts(List<AlertFlag> alerts, AgentIdObject agent)
        {
            if (alerts.Count > 0)
            {
                var alertsClient = new AlertFlagClient();
                foreach (AlertFlag a in alerts)
                {
                    ResolveAlertFlagRequest req = new ResolveAlertFlagRequest();
                    req.AlertFlagGuid = (Guid)a.Guid;
                    req.ResolvedByRole = (AgentRole)Enum.Parse(typeof(AgentRole), agent.agentRoleNames[0]);
                    req.ResolvedBy = (Guid)agent.agentGuid;
                    ResponseObject<EmptyResult> clearAlert = alertsClient.ResolveAlertFlag(req);
                    if (clearAlert.statusCode != System.Net.HttpStatusCode.OK || clearAlert.statusCode != System.Net.HttpStatusCode.NoContent)
                    {
                        log.Error($"We could not successfully clear alerts for alert flag with guid {a.Guid.ToString()} on loanapplication {a.LoanApplicationGuid.ToString()}.");
                        return false;
                    }
                }
                return true;
            }
            else return false;
        }

        public static bool DispositionDataObjects(List<DataObject> dataObjects, AgentIdObject agent, Disposition? disposition)
        {
            var dataClient = new DataObjectClient();
            if (dataObjects.Count > 0)
            {
                var alertsClient = new AlertFlagClient();
                foreach (DataObject d in dataObjects)
                {
                    DataObjectDispositionRequest req = new DataObjectDispositionRequest();
                    req.DataObjectGuid = (Guid)d.DataObjectGuid;
                    req.DispositionedByRole = (AgentRole)Enum.Parse(typeof(AgentRole), agent.agentRoleNames[0]);
                    req.DispositionedBy = (Guid)agent.agentGuid;
                    req.Disposition = disposition == null ? Disposition.FalsePositive  : (Disposition)disposition;
                    req.DispositionDate = DateTime.Now;
                    ResponseObject<EmptyResult> dispositionDataResponse = dataClient.Disposition(req);
                    if (dispositionDataResponse.statusCode != System.Net.HttpStatusCode.OK || dispositionDataResponse.statusCode != System.Net.HttpStatusCode.NoContent)
                    {
                        log.Error($"We could not successfully clear alerts for alert flag with guid {d.DataObjectGuid.ToString()} on loanapplication {d.LoanApplicationGuid.ToString()}.");
                        return false;
                    }
                }
                return true;
            }
            else return false;
        }

    }
}

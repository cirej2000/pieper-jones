using Common.Logging;
using LD.Common.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.CoreModelConstructors;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.DataAccess.AgentDataAccess;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.DataAccess.EmailDataAccess;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Utilities.AgentAssignments;
using static PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Utilities.DocumentUploads;

//TODO - Split this class into functional groups...gotten to be a bit of a dumping ground
namespace PIEPER_JONES.TEST.LENDING.Api.Tests.Framework.Workflows
{
    public static class PreSubmitWorkflows
    {
        private static readonly ILog log = LoanDepotLogManager.GetLogger();
        public static string fileUploadPayload(string documentType, string documentName, string description) => $"{{\"type\": \"{documentType}\", \"name\": \"{documentName}\", \"description\": \"{description}\"}}";

        //Take the loan application to the point where we have all stipulations completed, besides email verification and banklinking
        public static LoanApplicationCoreStateObject GetBorrowerWithAllStipsButEmailAndBankverification(RegisterBorrowerRequest borrower)
        {
            var statusClient = new LoanApplicationStatusClient();
            var filePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Resources\\";
            var borrowerState = RegisterBorrower(borrower);
            borrowerState = GetPrimaryOffer(borrowerState.Borrower, (Guid)borrowerState.LoanApplicationGuid, borrowerState.BorrowerGuid.ToString());
            var employmentUpdate = CoreModelConstructors.CreateUpdateEmploymentRequestAuto();
            EmploymentInformation(employmentUpdate, (Guid)borrowerState.BorrowerGuid);
            borrowerState.LoanApplicationStipsCompleted++;
            var statusNoContent = statusClient.UpdateStatusJob();

            //ID
            IdentityVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, "PhotoID.pdf", null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            IncomeVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, "W2.pdf", null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            BankStatementVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, "BankStatement.pdf", null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            UpdateHomePayment(1700, GetRandomEnumValue<OccupancyStatus>(), borrowerState.BorrowerGuid.ToString());
            borrowerState.LoanApplicationStipsCompleted++;
            //Get all of these updated via the "batch job" 
            statusNoContent = statusClient.UpdateStatusJob();
            return borrowerState;
        }

        public static LoanApplicationCoreStateObject GetBorrowerWithAllStipsButEmailAndBankverificationAuto(RegisterBorrowerRequest borrower, string address1, string address2, string city, string state, string zipCode, string currentEmployer, int lengthYears, string phoneNumber, string extension, string jobTitle, DateTime startDate, EmploymentStatus status)
        {
            var statusClient = new LoanApplicationStatusClient();
            var filePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Resources\\";
            var borrowerState = RegisterBorrower(borrower);
            borrowerState = GetPrimaryOffer(borrowerState.Borrower, (Guid)borrowerState.LoanApplicationGuid, borrowerState.BorrowerGuid.ToString());
            var employmentUpdate = CoreModelConstructors.CreateUpdateEmploymentRequestAuto();
            EmploymentInformation(employmentUpdate, (Guid)borrowerState.BorrowerGuid);
            borrowerState.LoanApplicationStipsCompleted++;
            var statusNoContent = statusClient.UpdateStatusJob();
            IdentityVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, "PhotoID.pdf", null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            IncomeVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, "W2.pdf", null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            BankStatementVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, "BankStatement.pdf", null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            UpdateHomePayment(1700, GetRandomEnumValue<OccupancyStatus>(), borrowerState.BorrowerGuid.ToString());
            borrowerState.LoanApplicationStipsCompleted++;
            //Get all of these updated via the "batch job" 
            statusNoContent = statusClient.UpdateStatusJob();
            return borrowerState;
        }

        public static LoanApplicationCoreStateObject GetBorrowerWithAllStipsButIDVerification(RegisterBorrowerRequest borrower, string employerAddress1, string employerAddress2, string employerCity, string employerState, string employerZipCode,
                                                     string currentEmployer, int lengthYears, string phoneNumber, string phoneExtension, string jobTitle, DateTime startDate, EmploymentStatus status)
        {
            var statusClient = new LoanApplicationStatusClient();
            var filePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "Resources\\";
            var fileName = "logo.jpg";
            var borrowerState = RegisterBorrower(borrower);

            borrowerState = GetPrimaryOffer(borrowerState.Borrower, (Guid)borrowerState.LoanApplicationGuid, borrowerState.BorrowerGuid.ToString());
            var employmentUpdate = CoreModelConstructors.CreateUpdateEmploymentRequestAuto();
            EmploymentInformation(employmentUpdate, (Guid)borrowerState.BorrowerGuid);
            borrowerState.LoanApplicationStipsCompleted++;
            var statusNoContent = statusClient.UpdateStatusJob();
            //ID
            EmailVerification(borrowerState.BorrowerGuid.ToString(), borrowerState.Borrower.Borrower.EmailAddress);
            var request = CoreModelConstructors.CreateCoreLinkBankAccountRequest("wells", (Guid)borrowerState.LoanApplicationGuid, "plaid_test", "plaid_good", PIEPER_JONES.Common.Enums.BankProvider.Plaid);

            statusNoContent = statusClient.UpdateStatusJob();
            IncomeVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, fileName, null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            BankStatementVerification(borrowerState.LoanApplicationGuid.ToString(), filePath, fileName, null);
            borrowerState.LoanApplicationStipsCompleted++;
            statusNoContent = statusClient.UpdateStatusJob();
            UpdateHomePayment(1700, GetRandomEnumValue<OccupancyStatus>(), borrowerState.BorrowerGuid.ToString());
            borrowerState.LoanApplicationStipsCompleted++;
            //Get all of these updated via the "batch job" 
            statusNoContent = statusClient.UpdateStatusJob();
            return borrowerState;
        }

        public static LoanApplicationCoreStateObject GetBorrowerWithAllStipsButBankverification(RegisterBorrowerRequest borrower)
        {
            var borrowerState = GetBorrowerWithAllStipsButEmailAndBankverification(borrower);
            EmailVerification(borrowerState.LoanApplicationGuid.ToString(), borrowerState.Borrower.Borrower.EmailAddress);
            //Get all of these updated via the "30 second job" 
            borrowerState.LoanApplicationStipsCompleted++;
            return borrowerState;
        }

        // return a borrower that has registered and accepted the primary offer
        public static LoanApplicationCoreStateObject GetPrimaryOffer(RegisterBorrowerRequest borrower, Guid applicationGuid, string borrowerGuid = null)
        {
            var loanApplicationOffersClient = new LoanApplicationOffersClient();
            var offerJson = new GenerateOffersRequest();
            offerJson.LoanAmount = borrower.LoanApplication.RequestedLoanAmount;
            var result = loanApplicationOffersClient.GenerateOffers(offerJson, applicationGuid.ToString());
            result = loanApplicationOffersClient.GetOffer(applicationGuid.ToString());
            var offerGuid = result.content.LoanOfferGroup.LoanApplicationOffers[0].Guid.ToString();
            var offersClient = new OffersClient();
            var noResult = offersClient.SelectOffer(offerGuid);
            var stateObject = new LoanApplicationCoreStateObject();

            stateObject.Borrower = borrower;
            if (borrowerGuid != null)
                stateObject.BorrowerGuid = new Guid(borrowerGuid);
            stateObject.OfferGuid = new Guid(offerGuid);
            stateObject.LoanApplicationGuid = applicationGuid;
            return stateObject;
        }

        public static LoanApplicationCoreStateObject GetBorrowerToQuoting(RegisterBorrowerRequest borrower, Guid applicationGuid)
        {
            var loanApplicationOffersClient = new LoanApplicationOffersClient();
            var offerJson = new GenerateOffersRequest();
            offerJson.LoanAmount = borrower.LoanApplication.RequestedLoanAmount;
            var result = loanApplicationOffersClient.GenerateOffers(offerJson, applicationGuid.ToString());
            var stateObject = new LoanApplicationCoreStateObject();
            stateObject.Borrower = borrower;
            stateObject.LoanApplicationGuid = applicationGuid;
            return stateObject;
        }

        public static LoanApplicationCoreStateObject TransitionToReviewingState(RegisterBorrowerRequest borrowerRegistration)
        {
            var loanApplicationClient = new LoanApplicationClient();
            var statusUpdateClient = new LoanApplicationStatusClient();
            var borrowerState = GetBorrowerWithAllStipsButEmailAndBankverification(borrowerRegistration);
            var stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            EmailVerification(borrowerState.LoanApplicationGuid.ToString(), borrowerRegistration.Borrower.EmailAddress);

            var statusUpdateResponse = statusUpdateClient.UpdateStatusJob();
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());

            var request = CoreModelConstructors.CreateCoreLinkBankAccountRequest("wells", (Guid)borrowerState.LoanApplicationGuid, "plaid_test", "plaid_good", PIEPER_JONES.Common.Enums.BankProvider.Plaid);
            BankLinking(borrowerState.LoanApplicationGuid.ToString(), (Guid)borrowerState.BorrowerGuid, borrowerRegistration, request);
            statusUpdateResponse = statusUpdateClient.UpdateStatusJob();
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            borrowerState = SubmitLoanApplication(borrowerState.LoanApplicationGuid.ToString(), borrowerRegistration);
            stateResult = loanApplicationClient.Status(borrowerState.LoanApplicationGuid.ToString());
            return borrowerState;
        }

        public static LoanApplicationCoreStateObject NewAppToAssignBCD(RegisterBorrowerRequest borrowerRegistration, AgentRole role, string employmentAddress1, string employmentAddress2, string employmentCity, string employmentState, string employmentZipCode, int yearsEmployed, string phoneNumber, string phoneExtension)
        {
            var loanInfoObject = TransitionToReviewingState(borrowerRegistration);
            AssignAgentToLoanApplication((Guid)loanInfoObject.LoanApplicationGuid, role);
            loanInfoObject.ApplicaiontStatus = LoanApplicationStatus.NewAppNeedInitialReview;
            loanInfoObject.ApplicationState = LoanApplicationState.Reviewing;
            return loanInfoObject;
        }





        // Register N borrowers
        public static void CreateNBorrowerApplicationsRandomSource(int number)
        {
            RegisterBorrowerRequest borrower;
            for (int x = 0; x < number; x++)
            {
                borrower = CoreModelConstructors.CreateRegisterBorrowerRequest();
                RegisterBorrower(borrower);
            }
        }

        public static bool? ValidateEmailAvailability(string emailAddress)
        {
            var borrowersClient = new BorrowersClient();
            var result = borrowersClient.IsEmailAvailable(emailAddress);

            if (result.content == null)
            {
                throw new NullReferenceException($"response content for email availability request against email {emailAddress} was null.");
            }
            else
            {
                return result.content.IsAvailable;
            }
        }

        public static LoanApplicationCoreStateObject RegisterBorrower(RegisterBorrowerRequest borrower = null)
        {
            if (borrower == null)
            {
                borrower = CoreModelConstructors.CreateRegisterBorrowerRequest();
            }

            var borrowersClient = new BorrowersClient();
            var result = borrowersClient.BorrowerRegistration(borrower);
            var borrowerGuid = (Guid)result.content.BorrowerGuid;
            Guid? applicationGuid = result.content.LoanApplicationGuid;
            var returnObject = new LoanApplicationCoreStateObject();
            returnObject.Borrower = borrower;
            returnObject.BorrowerGuid = borrowerGuid;
            returnObject.LoanApplicationGuid = applicationGuid;
            returnObject.ApplicaiontStatus = LoanApplicationStatus.OfferSelectPending;
            returnObject.ApplicationState = LoanApplicationState.Quoting;
            return returnObject;
        }


        public static GetOffersResponse GenerateOffers(GenerateOffersRequest offerJson, Guid loanApplicationGuid)
        {
            var loanApplicationOffersClient = new LoanApplicationOffersClient();
            var result = loanApplicationOffersClient.GenerateOffers(offerJson, loanApplicationGuid.ToString());
            return result.content;
        }

        public static Guid? GetPrimaryOffer(Guid loanApplicationGuid)
        {
            var loanApplicationOffersClient = new LoanApplicationOffersClient();
            var result = loanApplicationOffersClient.GetOffer(loanApplicationGuid.ToString());
            return result.content.LoanOfferGroup.LoanApplicationOffers[0].Guid;
        }

        public static Guid? GetAlternativeOffer(Guid loanApplicationGuid, int offerIndex)
        {
            var loanApplicationOffersClient = new LoanApplicationOffersClient();
            var result = loanApplicationOffersClient.GetOffer(loanApplicationGuid.ToString());
            return result.content.LoanOfferGroup.LoanApplicationOffers[offerIndex + 1].Guid;
        }

        public static void ChooseOffer(Guid offerGuid)
        {
            var offersClient = new OffersClient();
            var result = offersClient.SelectOffer(offerGuid.ToString());
        }
      
        public static LoanApplicationCoreStateObject LoanApplicationInApplyingState(RegisterBorrowerRequest borrower = null)
        {
            var result = RegisterBorrower(borrower);
            var genOffers = new GenerateOffersRequest();
            genOffers.LoanAmount = result.Borrower.LoanApplication.RequestedLoanAmount == null ?
                                    (decimal?)(new Random().Next(5000, 35001)) : result.Borrower.LoanApplication.RequestedLoanAmount;
            var offers = GenerateOffers(genOffers, (Guid)result.LoanApplicationGuid);

            ChooseOffer((Guid)offers.LoanOfferGroup.LoanApplicationOffers[0].Guid);
            result.OfferGuid = offers.LoanOfferGroup.LoanApplicationOffers[0].Guid;
            result.ApplicationState = LoanApplicationState.Applying;
            result.ApplicaiontStatus = LoanApplicationStatus.AppStipsPending;
            return result;
        }

        public static List<LoanApplicationTask> GetTasksList(string loanApplicationGuid)
        {
            var loanApplicationTasksClient = new LoanApplicationTasksClient();
            var resp = loanApplicationTasksClient.GetTasksList(loanApplicationGuid);
            List<LoanApplicationTask> taskList = resp.content;
            return taskList;
        }

        public static void IncomeVerification(string applicationGuid, string filePath, string fileName, UploadedDocumentCategory? uploadCategory)
        {
            var loanApplicationTasksClient = new LoanApplicationTasksClient();
            var docClient = new LoanApplicationUploadedDocumentsClient();
            var resp = loanApplicationTasksClient.GetTasksList(applicationGuid);
            List<LoanApplicationTask> taskList = resp.content;
            List<LoanApplicationTask> incomeVerification = taskList.Where(task => 
                                                                task.Title == "W-2" ||
                                                                task.Title == "Pay Stub")
                                                                .ToList();
            UploadedDocumentType type;       

            foreach (LoanApplicationTask l in incomeVerification)
            {
                type = l.Title.ToLower().Contains("w-2") ? UploadedDocumentType.W2 : UploadedDocumentType.PayStub;
                fileName = type == UploadedDocumentType.W2 ? "W2.pdf" : "PayStub.pdf";
                var docUploadResponse = UploadDocument(l, filePath, fileName, UploadedDocumentCategory.Borrower, type);

                var note = BuildNoteRequest(EntityType.UploadedDocument, docUploadResponse.content.Guid, type.ToString(), true,
                            $"{type} Doc uploaded", AgentNoteCategory.DocumentUpload, AgentRole.LLO,
                            $"{type.ToString()} Document uploaded.");
                var agents = GetAgentsByRole(note.CreatorRole.ToString());
                note.CreatedBy = (Guid)agents[new Random().Next(0, agents.Count)].agentGuid;
                CompleteDocUpload(note, l.LoanApplicationGuid.ToString(), l.Guid.ToString());
            }            
        }

        public static void EmailVerification(string applicationGuid, string emailAddress)
        {
            var emailsClient = new EmailsClient();
            var loanApplicationTasksClient = new LoanApplicationTasksClient();
            Dictionary<string, string> emailVerificationDetails = GetEmailVerificationDetails(emailAddress);
            var verificationGuid = emailVerificationDetails["guid"];
            var result = emailsClient.VerifyEmail(verificationGuid);
            var resp2 = loanApplicationTasksClient.GetTasksList(applicationGuid);
            var taskList2 = resp2.content;
        }

        public static void BankLinking(string applicationGuid, Guid borrowerGuid, RegisterBorrowerRequest borrower, LinkBankAccount.Request request)
        {
            var req = new LinkBankAccount.Request();
            var bank = new Bank();

            bank.Name = "Wells Fargo Bank";
            bank.Id = "wells";
            bank.Provider = BankProvider.Plaid;
            req.LoanApplicationGuid = new Guid(applicationGuid);
            req.Username = "plaid_test";
            req.Password = "plaid_good";
            req.Bank = bank;
            var client = new BankVerificationClient();
            var response = client.BankLinking(applicationGuid, req);
            var account = response.content.Accounts[0];
            var sBank = new SetBankAccount.Request();
            sBank.BankAccountGuid = (Guid)account.Guid;
            var bClient = new BorrowerBankAccountsClient();
            bClient.CreateBankAccount(sBank, applicationGuid);
        }

        public static ResponseObject<EmptyResult> EmploymentInformation(UpdateBorrowerEmploymentRequest request, Guid borrowerGuid)
        {
            var borrowersClient = new BorrowersClient();
            return borrowersClient.UpdateEmployment(borrowerGuid.ToString(), request);
        }

        public static void IdentityVerification(string applicationGuid, string filePath, string fileName, UploadedDocumentCategory? uploadCategory)
        {
            var loanApplicationTasksClient = new LoanApplicationTasksClient();
            var resp = loanApplicationTasksClient.GetTasksList(applicationGuid);
            var taskList = resp.content;        
            var identifyVerification = taskList.Where(task => task.Title == "Driver License").ToList()[0];
            var resp2 = UploadDocument(identifyVerification, filePath, "PhotoId.pdf", UploadedDocumentCategory.Borrower, UploadedDocumentType.PhotoId);
            var note = BuildNoteRequest(EntityType.UploadedDocument, resp2.content.Guid, UploadedDocumentType.PhotoId.ToString(), true,
                                        $"{UploadedDocumentType.PhotoId} Doc uploaded", AgentNoteCategory.DocumentUpload, AgentRole.LLO,
                                        $"{UploadedDocumentType.PhotoId.ToString()} Document uploaded.");
            var agents = GetAgentsByRole(note.CreatorRole.ToString());
            note.CreatedBy = (Guid)agents[new Random().Next(0, agents.Count)].agentGuid;
            CompleteDocUpload(note, identifyVerification.LoanApplicationGuid.ToString(), identifyVerification.Guid.ToString());
        }



        public static ResponseObject<EntityCreationResponse> UploadDocument(LoanApplicationTask task, string filePath, 
                                            string fileName, UploadedDocumentCategory? uploadCategory, 
                                            UploadedDocumentType docType)
        {
            var request = new CreateUploadedDocumentRequest();
            request.Description = task.RequestMessage;
            request.Name = task.Title;
            request.Type = docType;
            request.Category = uploadCategory;
            request.TaskGuid = task.Guid;
            request.LoanApplicationGuid = task.LoanApplicationGuid;
            return new TasksClient().UploadDocument(request, fileName, filePath, "multipart/form-data");
        }

        //Todo - Move this to the model constructor class
                              

        public static void BankStatementVerification(string applicationGuid, string filePath, string fileName, UploadedDocumentCategory? uploadCategory)
        {
            var loanApplicationTasksClient = new LoanApplicationTasksClient();
            var resp = loanApplicationTasksClient.GetTasksList(applicationGuid);
            List<LoanApplicationTask> taskList = resp.content;
            var bankStatementVerification = taskList.Where(task => task.Title == "Bank Statement").ToList()[0];
            var resp2 = UploadDocument(bankStatementVerification, filePath, fileName, UploadedDocumentCategory.Borrower, UploadedDocumentType.BankStatement);
            var note = BuildNoteRequest(EntityType.UploadedDocument, resp2.content.Guid, UploadedDocumentType.BankStatement.ToString(), true,
                                        $"{UploadedDocumentType.PhotoId} Doc uploaded", AgentNoteCategory.DocumentUpload, AgentRole.LLO,
                                        $"{UploadedDocumentType.PhotoId.ToString()} Document uploaded.");
            var agents = GetAgentsByRole(note.CreatorRole.ToString());
            note.CreatedBy = (Guid)agents[new Random().Next(0, agents.Count)].agentGuid;
            CompleteDocUpload(note, bankStatementVerification.LoanApplicationGuid.ToString(), bankStatementVerification.Guid.ToString());
        }

        public static ResponseObject<EmptyResult> UpdateHomePayment(int monthlyPayment, OccupancyStatus status, string borrowerGuid)
        {
            var borrowersClient = new BorrowersClient();
            var request = new UpdateBorrowerHomePaymentRequest();
            request.HomeMonthlyPayment = monthlyPayment;
            request.OccupancyStatus = status;
            return borrowersClient.UpdateHomePayment(borrowerGuid.ToString(), request);
        }

        public static LoanApplicationCoreStateObject SubmitLoanApplication(string applicationGuid, RegisterBorrowerRequest borrower)
        {
            var loanApplicationClient = new LoanApplicationClient();
            var devinfo = new DeviceInfo();
            devinfo.BlackBoxId = "BlackBoxId".GetAppSetting();
            devinfo.IpAddress = "127.0.0.1";   
            SignAllPreLoanAppSubmitDocs(applicationGuid);
            var request = CoreModelConstructors.CreateLoanApplicationSubmitRequest(devinfo, 1, 1, 1, 1, borrower.Borrower.LastName);
            LoanApplicationCoreStateObject result = new LoanApplicationCoreStateObject();

            loanApplicationClient.SubmitLoanApplication(applicationGuid, request);
            result.ApplicaiontStatus = LoanApplicationStatus.NewAppNeedInitialReview;
            result.ApplicationState = LoanApplicationState.Reviewing;
            result.Borrower = borrower;
            result.LoanApplicationGuid = new Guid(applicationGuid);
            return result;
        }








        public static void SignAllPreLoanAppSubmitDocs(string loanApplicationGuid, List<GeneratedDocumentType> docTypes = null)
        {
            var listOfDocTypes;

            if (docTypes == null || docTypes.Count == 0)
            {
                listOfDocTypes = new List<GeneratedDocumentType>() {
                GeneratedDocumentType.ESignConsent, GeneratedDocumentType.PatriotActDisclosure,
                                                    GeneratedDocumentType.CreditAuthorizationDisclosure, GeneratedDocumentType.CreditScoreNotice, GeneratedDocumentType.TCPADisclosure,
                                                    GeneratedDocumentType.TermsOfUse, GeneratedDocumentType.BorrowerAgreement, GeneratedDocumentType.RegisterBorrowerAuthorization,
                                                    GeneratedDocumentType.PrivacyPolicy, GeneratedDocumentType.AdverseActionNotice, GeneratedDocumentType.CancelAnyTimeNotice,
                                                    GeneratedDocumentType.CRBPrivacyPolicy, GeneratedDocumentType.PTIL};
            }
            else
            {
                listOfDocTypes = docTypes;
            }

            var request = new DocumentSigningByTypeRequest();
            request.GeneratedDocumentTypes = listOfDocTypes;
            request.IpAddress = "127.0.0.1";
            var result = new LoanAppGenDocClient().SignApplicationDocs(loanApplicationGuid, request);
        }
    }
}
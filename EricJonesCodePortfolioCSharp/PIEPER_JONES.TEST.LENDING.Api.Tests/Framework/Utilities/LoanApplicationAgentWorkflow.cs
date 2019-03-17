
using Common.Logging;
using PIEPER_JONES.Internal.Api.Models;
using PIEPER_JONES.Internal.Api.Models.Shared;
using PIEPER_JONES.TEST.LENDING.Api.Common.Internal.ClientObjects;
using PIEPER_JONES.TEST.Common.API.Rest;
using System;
using System.Collections.Generic;
using System.Net;
namespace PIEPER_JONES.TEST.LENDING.Api.Tests.Framework
{
    public static class LoanApplicationAgentWorkflow
    {
        private static readonly ILog log = LoanDepotLogManager.GetLogger();

        public static LoanApplicationAgentStateObject RegisteredBorrower(RegisterBorrower.Request borrower)
        {
            // check email availability
            var Client = new BorrowersClient();                
            var registerResponse = Client.RegisterBorrower(borrower);
            var result = new LoanApplicationAgentStateObject();

            if (registerResponse.statusCode != HttpStatusCode.Created || registerResponse.content == null)
            {
                log.Info($"Borrower Registration Failed.");
                return null;
            }

            result.Borrower = borrower;
            result.BorrowerGuid = (Guid)registerResponse.content.BorrowerGuid;
            result.LoanApplicationGuid = (Guid)registerResponse.content.LoanApplicationGuid;

            return result;
        }

        // return a borrower that has registered and accepted the primary offer
        public static LoanApplicationAgentStateObject GetPrimaryOffer(RegisterBorrower.Request borrower, Guid applicationGuid, string borrowerGuid = null)
        {
            var loanApplicationOffersClient = new LoanApplicationOffersClient();
            var offerJson = new GetOffers.GenerateRequest();

            offerJson.LoanAmount = borrower.RequestedLoanAmount;

            var result = loanApplicationOffersClient.GenerateOffers(offerJson, applicationGuid.ToString());

            result = loanApplicationOffersClient.GetOffer(applicationGuid.ToString());

            var offerGuid = result.content.LoanOfferGroup.LoanApplicationOffers[0].Guid.ToString();
            var offersClient = new OffersClient();
            var noResult = offersClient.SelectOffer(offerGuid);
            var stateObject = new LoanApplicationAgentStateObject();

            stateObject.Borrower = borrower;

            if (borrowerGuid != null)
            {
                stateObject.BorrowerGuid = new Guid(borrowerGuid);
            }

            stateObject.OfferGuid = new Guid(offerGuid);
            stateObject.LoanApplicationGuid = applicationGuid;
            return stateObject;
        }

        public static LoanApplicationAgentStateObject GetBorrowerToQuoting(RegisterBorrower.Request borrower, Guid applicationGuid)
        {
            var loanApplicationOffersClient = new LoanApplicationOffersClient();
            var offerJson = new GetOffers.GenerateRequest();

            offerJson.LoanAmount = borrower.RequestedLoanAmount;

            var result = loanApplicationOffersClient.GenerateOffers(offerJson, applicationGuid.ToString());
            var stateObject = new LoanApplicationAgentStateObject();

            stateObject.Borrower = borrower;
            stateObject.LoanApplicationGuid = applicationGuid;
            return stateObject;
        }

        public static bool? ValidateEmailAvailability(string emailAddress)
        {
            var borrowersClient = new BorrowersClient();
            var request = new EmailAvailability.Request();
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


         public static GetOffers.Response GenerateOffers(GetOffers.GenerateRequest offerJson, Guid loanApplicationGuid)
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
            var offers = result.content.LoanOfferGroup.LoanApplicationOffers;
            return offers.GetRange(1,offers.Count)[offerIndex].Guid;
        }


        public static void ChooseOffer(Guid offerGuid)
        {
            var offersClient = new OffersClient();
            var result = offersClient.SelectOffer(offerGuid.ToString());
        }
    }
}

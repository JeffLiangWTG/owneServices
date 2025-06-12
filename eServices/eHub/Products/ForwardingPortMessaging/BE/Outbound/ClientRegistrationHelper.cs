using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Core.Logging;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
    public static class ClientRegistrationHelper
    {


        public static ClientRegistrationItem GetEHubClientRegistrationItemWithRetries(string clientID, string branchCode, string rtID, OrchestrationLogger logger)
        {
            var clientRegistrationItem = DatabaseAccessHelpers.AccessDatabaseWithRetries<ClientRegistrationItem>(() =>
            GetEHubClientRegistrationItem(clientID, branchCode, rtID));
			if (clientRegistrationItem == null)
			{
				logger.Log.Debug(String.Format("Cannot find ClientRegistration with branch {0}, will get fallback client registration", branchCode));
				clientRegistrationItem = DatabaseAccessHelpers.AccessDatabaseWithRetries<ClientRegistrationItem>(() =>
				GetEHubClientRegistrationItem(clientID, null, rtID));
			}
			logger.Log.Debug(String.Format("Client Code: {0}", clientRegistrationItem?.Code));
			return clientRegistrationItem;
		}

        public static ClientRegistrationItem GetEHubClientRegistrationItem(string clientID, string branchCode, string rtID)
        {
            using (var context = GetDbContext())
            {
                var clientRegistration = context.eHubClientRegistrations
                    .FirstOrDefault( x => x.eHubRegistrationType.RT_ID == rtID
                    && x.eHubClient.CC_ID == clientID 
                    && x.CX_Qualifier == branchCode);
                if (clientRegistration == null)
                {
                    return null;
                }
                return new ClientRegistrationItem(clientRegistration);
            }
        }
        internal static Func<eHubTransactionsContext> GetDbContext = () => new eHubTransactionsContext();

    }
}

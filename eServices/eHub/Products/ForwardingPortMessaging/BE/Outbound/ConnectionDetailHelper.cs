using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;


namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
    public static class ConnectionDetailHelper
    {

        public static ConnectionDetailItem GetConnectionDetailItemWithRetries()
        {
            return DatabaseAccessHelpers.AccessDatabaseWithRetries<ConnectionDetailItem>(() => GetConnectionDetails());
        }

        public static ConnectionDetailItem GetConnectionDetails()
        {
            try
            {
                using (var context = GetDbContext())
                {
                    var eHubCodeValues = context.eHubCodeMapValues
                            .Where(v => v.eHubCodeSetResult.eHubCodeSet.eHubTransformationSet.TS_Name == "NXPORT System Configuration"
                            && v.eHubCodeSetResult.eHubCodeSet.CS_Name == "Connection Details");
         
                    var connectionDetailItem = new ConnectionDetailItem();
                    connectionDetailItem.ClientId = eHubCodeValues
                        .Where(v => v.eHubCodeSetResult.CR_Name == "ClientId"
                        && v.CV_OutputCode != "")
                        .FirstOrDefault().CV_OutputCode;

                    connectionDetailItem.Secret = eHubCodeValues
                        .Where(v => v.eHubCodeSetResult.CR_Name == "Secret"
                        && v.CV_OutputCode != "")
                        .FirstOrDefault().CV_OutputCode;
                    return connectionDetailItem;
                }
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }


        internal static Func<eHubTransactionsContext> GetDbContext = () => new eHubTransactionsContext();

    }
}

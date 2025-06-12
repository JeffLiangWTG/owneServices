using System;
using System.Linq;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Helpers
{
    public class DbHelpers
    {
        internal static Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();

        public static string GetClientPk(string clientId)
        {
            using (var context = ContextFactory())
            {
                return context.eHubClients.Where(c => c.CC_ID == clientId).Select(c => c.CC_PK.ToString())
                           .FirstOrDefault() ?? string.Empty;
            }
        }

        public static string GetSubscriptionTypePk(string subscriptionTypeId)
        {
            using (var context = ContextFactory())
            {
                return context.eHubSubscriptionTypes.Where(s => s.ST_ID == subscriptionTypeId).Select(s => s.ST_PK.ToString())
                           .FirstOrDefault() ?? string.Empty;
            }
        }

		public static string GetMessageTypePK(string messageType)
		{
			using (var context = ContextFactory())
			{
				return context.eHubMessageTypes.Where(d => d.DT_Code == messageType).Select(d => d.DT_PK.ToString())
						   .FirstOrDefault() ?? string.Empty;
			}
		}
    }
}

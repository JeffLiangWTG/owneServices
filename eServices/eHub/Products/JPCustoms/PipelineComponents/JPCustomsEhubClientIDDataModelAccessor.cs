using System.Data.Entity;
using System.Linq;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Core.Transforms.Helper
{
    public class JPCustomsEhubClientIDDataModelAccessor
    {
        const string ClientRegistrationType = "JPCustomsAccount_ClientLevel";
        const string SystemRegistrationType = "JPCustomsAccount_SystemLevel";
        private readonly ContextFactory<eHubTransactionsContext> contextFactory;

        public JPCustomsEhubClientIDDataModelAccessor() : this(new ContextFactory<eHubTransactionsContext>()) { }

        internal JPCustomsEhubClientIDDataModelAccessor(ContextFactory<eHubTransactionsContext> contextFactory)
        {
            this.contextFactory = contextFactory;
            Database.SetInitializer<eHubTransactionsContext>(null);
        }

        public virtual string GetEHubClientIDFromCredentials(string username)
        {
            using (var context = contextFactory.CreateContext())
            {
                var clientOfClientRegistration = context.eHubClientRegistrations.Where(r => r.CX_Code == username && r.eHubRegistrationType.RT_ID == ClientRegistrationType).Select(x => x.eHubClient.CC_ID).FirstOrDefault();
                if (clientOfClientRegistration != null)
                    return clientOfClientRegistration;
                var systemRegistration = context.eHubClientSystemRegistrations.FirstOrDefault(r => r.CD_Code == username && r.eHubRegistrationType.RT_ID == SystemRegistrationType);
                if (systemRegistration == null)
                    return null;
                var systemID = systemRegistration.eHubClientSystem.EH_ID;
                var enterpriseCode = systemID.Substring(0, 3);
                var serverCode = systemID.Substring(3, 3);
                var client = context.eHubClients.FirstOrDefault(x =>
                    x.CC_ID.StartsWith(enterpriseCode) && x.CC_ID.EndsWith(serverCode));
                return client == null ? null : client.CC_ID;
            }
        }
    }
}

using System.Data.Entity;
using CargoWise.eHub.DataModel.Accessors;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.AUCustomsNEXDOC.Transforms.Helper
{
    public class NEXDOCDataModelAccessor
    {
        private readonly ContextFactory<eHubTransactionsContext> contextFactory;
		public const string NEXDOC_ClientGroup = "NEXDOC_ClientGroup";
		public const string NEXDOC_Client = "NEXDOC_Client";

        public NEXDOCDataModelAccessor() : this(new ContextFactory<eHubTransactionsContext>()) { }

		internal NEXDOCDataModelAccessor(ContextFactory<eHubTransactionsContext> contextFactory)
		{
			this.contextFactory = contextFactory;
			Database.SetInitializer<eHubTransactionsContext>(null);
		}

		public virtual string GetVendorToken(string recipientID)
		{
			return eHubTransactionsAccessor.GetCodeMappedValue_WithRetries(recipientID, recipientID, "NEXDOCS Credentials", "Default", "Vendor Token", null, null, null, null, null);
		}

		public virtual string GetInstallationToken(string recipientID)
		{
			return eHubTransactionsAccessor.GetCodeMappedValue_WithRetries(recipientID, recipientID, "NEXDOCS Credentials", "Default", "Installation Token", null, null, null, null, null);
		}

		public virtual string GetInstallationPassword(string recipientID)
		{
			return eHubTransactionsAccessor.GetCodeMappedValue_WithRetries(recipientID, recipientID, "NEXDOCS Credentials", "Default", "Password", null, null, null, null, null);
		}
    }
}

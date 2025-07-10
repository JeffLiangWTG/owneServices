using Enterprise.DocumentEngineCore.Registry;

namespace Enterprise.MasterFiles.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded menu item constants")]
	public class JobInvoicingEDocsProviderSupporter : EDocsProviderSupporter
	{
		public JobInvoicingEDocsProviderSupporter(IEDocsProvider eDocsProvider)
			: base(eDocsProvider)
		{
			if (!DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.Value)
			{
				AddConsumer(new MenuItemIdentifier(CargoWise.Definitions.BusinessContext.ARInvoice, OldStyleInvoiceName));
			}
		}

		public const string DocBuilderInvoiceName = "DocBuilder Invoice";
		public const string OldStyleInvoiceName = "Invoice";
		public const string ClassAInvoiceName = "Class A Invoice Preprinted";
		public const string CostConfirmationDocument = "Cost Confirmation Document";
		public const string CostConfirmationSummary = "Cost Confirmation Summary";
		public const string SelfBillingInvoice = "Self Billing Invoice";
		public const string ITAutofattura = "Autofattura (IT)";
	}
}

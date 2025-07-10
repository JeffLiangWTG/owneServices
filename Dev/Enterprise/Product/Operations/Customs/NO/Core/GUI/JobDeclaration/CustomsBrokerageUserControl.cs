using Enterprise.Customs.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InvoiceGroupingTabPage.TabRelevant = false;
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new JobDeclarationUserControl();

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			BaseInvoiceLineUserControl result;
			var declaration = JobDeclaration;
			if (declaration.IsExport)
			{
				result = new ExportInvoiceLineUserControl();
			}
			else
			{
				result = new ImportInvoiceLineUserControl();
			}
			return result;
		}

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl() => new InvoiceHeaderUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new MessageUserControl();

		public override bool EntryInstructionsTabVisibleForCountry => true;
	}
}

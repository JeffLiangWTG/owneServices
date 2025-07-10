using System;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override void JobDeclaration_JE_ApplicationCodeChanged(object sender, EventArgs e)
		{
			base.JobDeclaration_JE_ApplicationCodeChanged(sender, e);
			RemoveUserControlOfEachTabPage();
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControl();

		public override bool EntryInstructionsTabVisibleForCountry => true;

		protected override BaseInvoiceGroupingUserControl GetInvoiceGroupingUserControl() => new InvoiceGroupingUserControl();

		protected override BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl() => new ZAInvoiceHeaderUserControl();

		protected override BaseInvoiceLineUserControl GetInvoiceLinesUserControl() => new InvoiceLinesUserControl();

		protected override BaseCustomsEntryUserControl GetMessageUserControl() => new CustomsEntryAndDiscardedMessagesUserControl();

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl() => new ZADeclarationUserControl();

		protected override BaseMiscOptionsUserControl GetMiscOptionsUserControl() => new MiscOptionsUserControl();

		protected override IBasePackingControl GetPackingUserControl() => new BasePackingControl();
	}
}

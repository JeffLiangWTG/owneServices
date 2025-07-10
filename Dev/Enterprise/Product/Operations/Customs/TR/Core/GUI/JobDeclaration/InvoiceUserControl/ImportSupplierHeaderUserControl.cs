using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.TR.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ImportSupplierHeaderUserControl : EUNonLayoutImportSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			ReorderTabPages();

			LocalChargesGroupBox.AllowOutsideOfParent();
			ForeignChargesGroupBox.AllowOutsideOfParent();
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(AutoJobComInvoiceHeader.Schema.JZ_IncoTermPlace);
			}
		}

		protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControl);

		void ReorderTabPages()
		{
			ChargesTabControl.SuspendLayout();

			ChargesTabControl.TabPages.Remove(ApportionedTabPage);
			ChargesTabControl.TabPages.Insert(ApportionedTabPage, 0);
			ChargesTabControl.TabPages.Remove(ForeignChargesTabPage);
			ChargesTabControl.TabPages.Insert(ForeignChargesTabPage, 0);
			ChargesTabControl.TabPages.Remove(LocalChargesTabPage);
			ChargesTabControl.TabPages.Insert(LocalChargesTabPage, 0);
			ChargesTabControl.TabPages.Remove(InvoiceChargesTabPage);
			ChargesTabControl.TabPages.Insert(InvoiceChargesTabPage, 0);

			ChargesTabControl.ResumeLayout(false);
			ChargesTabControl.PerformLayout();
		}
	}
}

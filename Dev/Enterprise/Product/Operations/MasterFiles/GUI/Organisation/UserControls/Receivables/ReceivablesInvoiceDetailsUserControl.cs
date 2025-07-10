using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ReceivablesInvoiceDetailsUserControl : OrganisationContainerControl
	{
		public ReceivablesInvoiceDetailsUserControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				ARInvoiceTemplateTabPage.TabVisible = GlbCompany.CurrentCompany.IsTemplateFileConfigurationsEnabled;
				ARCashAdvanceTabPage.TabVisible = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>().IsReceivablesCashAdvanceFunctionalityEnabled;
			}
		}

		public void SetClientOverrideExchageRatesGroupBoxVisibility(bool exchangeRatesGridShouldBeVisible)
		{
			ClientOverrideExchageRatesGroupBox.Visible = exchangeRatesGridShouldBeVisible;
		}

		public void SetControlsReadOnlyForSecurity(bool invoiceBatchingGridShouldBeReadOnly, bool exchangeRatesGridShouldBeReadOnly)
		{
			InvoiceBatchingGrid.ReadOnly = invoiceBatchingGridShouldBeReadOnly;
			ExchangeRatesGrid.ReadOnly = exchangeRatesGridShouldBeReadOnly;
		}

		internal void WarehouseOptionsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var form = ParentForm as ZOrganisationsForm;
			if (form != null)
			{
				if (form.OrganisationsTabControl != null)
				{
					form.OrganisationsTabControl.SelectedTab = form.WhsFacilityTabPage;
				}
				if (form.WhsFacilityUserControl != null && form.WhsFacilityUserControl.WhsFacilityTabControl != null)
				{
					form.WhsFacilityUserControl.WhsFacilityTabControl.SelectedTab = form.WhsFacilityUserControl.ProductWarehouseTabPage;
				}
				if (form.WhsFacilityUserControl.WarehouseUserControl != null && form.WhsFacilityUserControl.WhsFacilityTabControl != null)
				{
					form.WhsFacilityUserControl.WarehouseUserControl.WarehouseTabControl.SelectedTab = form.WhsFacilityUserControl.WarehouseUserControl.InvoicingTabPage;
				}
			}
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}

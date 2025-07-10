using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class USInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public USInvoiceLineUserControl()
		{
			InitializeComponent();
			ChangeControlVisibility();
		}

		public new IInvoicesProvider CurrentDataItem
		{
			get { return (IInvoicesProvider)base.CurrentDataItem; }
		}

		protected override bool UseUniversalTariff
		{
			get { return false; }
		}

		protected override bool SupportsExtraPhysicalQuantitiesOnC2Pivot => true;

		protected JobComInvoiceLine currentInvoiceLine => CurrentInvoiceLine as JobComInvoiceLine;

		void ChangeControlVisibility()
		{
			AddColumnsToGrid();

			JI_Calc_GSTConvertToLocalCurrencyControl.Visible = false;
			JI_Calc_CIFConvertToLocalCurrencyControl.Visible = false;

			InvoiceLineCharges.ChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable, USCustomsSupplierHeaderUserControl.IsCIFComponent);
			InvoiceLineCharges.ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable, USCustomsSupplierHeaderUserControl.IsCIFComponent);
		}

		void AddColumnsToGrid()
		{
			TariffColumnStyleInfo tariffColumnStyleInfo1 = new TariffColumnStyleInfo();
			tariffColumnStyleInfo1.Caption = GetTariffCaption();
			tariffColumnStyleInfo1.ColumnName = JobComInvoiceLine.Schema.JI_FormattedTariff;
			tariffColumnStyleInfo1.IsMandatory = true;
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.ContainerControlToolTip.SetToolTip(this.CustomsInvoiceLinesBoundGrid, "-");
		}

		protected virtual string GetTariffCaption()
		{
			return "Tariff";
		}

		protected override InvoiceLineFilterBusinessObject CreateFilterBusinessObject(Func<Customs.Business.IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
		{
			return this.IsDesignMode() ? null : new USInvoiceLineFilterBusinessObject(getInvoicesProvider, isColumnAvailable);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (CurrentDataItem != null)
			{
				UpdateCurrentInvoiceLineAndRehookEvents();
			}
		}

		protected override void HookInvoiceLineEvents(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.HookInvoiceLineEvents(baseInvoiceLine);
			HookInvoiceLineEvents(baseInvoiceLine as JobComInvoiceLine);
		}

		protected virtual void HookInvoiceLineEvents(JobComInvoiceLine invoiceLine) { }

		protected override void UnHookInvoiceLineEvents(Customs.Business.BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.UnHookInvoiceLineEvents(baseInvoiceLine);
			UnHookInvoiceLineEvents(baseInvoiceLine as JobComInvoiceLine);
		}

		protected virtual void UnHookInvoiceLineEvents(JobComInvoiceLine invoiceLine) { }
	}
}

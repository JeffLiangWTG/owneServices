using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ExportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}

		protected override string ColumnTitleForGSTApplies => string.Empty;

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			var invoiceChargesIsGSTApplicableColumnInfo = InvoiceChargesGrid.GetColumnStyle(BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
			invoiceChargesIsGSTApplicableColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("0CEA4DF3-3E95-43E3-874E-AA2BD9FCB882", "Incl. in Total Inv. Amt. (16)", "Included in Declaration Total Invoice Amount (16)", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.");
			invoiceChargesIsGSTApplicableColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);

			var baseGroupChargesGridIsGSTApplicableColumnInfo = BaseGroupChargesGrid.GetColumnStyle(BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
			baseGroupChargesGridIsGSTApplicableColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("8FC020D3-3B17-49E4-9AC9-8C3D90AFD166", "Incl. in Total Inv. Amt. (16)", "Included in Declaration Total Invoice Amount (16)", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.");
			baseGroupChargesGridIsGSTApplicableColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);

			var apportionedChargesGridIsGSTApplicableColumnInfo = ApportionedChargesGrid.GetColumnStyle(BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
			apportionedChargesGridIsGSTApplicableColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("C23BB245-D31B-42B9-AA96-47576D78F12F", "Incl. in Total Inv. Amt. (16)", "Included in Declaration Total Invoice Amount (16)", "It indicates whether the charge is included in the invoice total amount. The Incoterm and charge code determine whether the charge is included by default.");
			apportionedChargesGridIsGSTApplicableColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);

			ResetInvoiceChargesGridColumnOrder();
		}

		#region The Columns of InvoiceChargesGrid (Order, DisplayHidden)

		void ResetInvoiceChargesGridColumnOrder()
		{
			InvoiceChargesGrid.ReOrderColumns(ColumnNamesInSortOrder);
			InvoiceChargesGrid.SetAllColumnsVisible(false);
			InvoiceChargesGrid.SetColumnVisible(true, defaultColumnsForGrid);
		}

		readonly string[] defaultColumnsForGrid = new string[]
		{
			BaseJobComInvHeaderCharge.Schema.J7_ChargeType,
			BaseJobComInvHeaderCharge.Schema.J7_ChargeDescription,
			BaseJobComInvHeaderCharge.Schema.J7_Amount,
			BaseJobComInvHeaderCharge.Schema.J7_RX_NKCurrency,
			BaseJobComInvHeaderCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
			BaseJobComInvHeaderCharge.Schema.J7_IsIncludedInITOT,
			BaseJobComInvHeaderCharge.Schema.J7_IsDutiable,
			BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable,
			BaseJobComInvHeaderCharge.Schema.J7_DistributeBy
		};

		string[] fColumnNamesInSortOrder;
		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (fColumnNamesInSortOrder == null)
				{
					var result = new List<string>(defaultColumnsForGrid);
					result.Add(BaseJobComInvHeaderCharge.Schema.IsJ7_ExchangeRateUserEnterable);
					result.Add(BaseJobComInvHeaderCharge.Schema.J7_ExchangeRate);
					result.Add(BaseJobComInvHeaderCharge.Schema.J7_PrepaidCollect);
					result.Add(BaseJobComInvHeaderCharge.Schema.J7_Percentage);
					fColumnNamesInSortOrder = result.ToArray();
				}
				return fColumnNamesInSortOrder;
			}
		}

		#endregion

		protected override string ColumnTitleWhenExportForDutiable => Res.GetData("25d8ef7a-e871-416c-9e6f-70984ef2210c", "Incl. in FOB", "It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.").Caption;
	}
}

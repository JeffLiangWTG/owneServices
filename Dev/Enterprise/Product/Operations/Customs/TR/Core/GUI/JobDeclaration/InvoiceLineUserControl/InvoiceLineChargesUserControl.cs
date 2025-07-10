using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TR.GUI
{
	public partial class InvoiceLineChargesUserControl : EU.GUI.InvoiceLineChargesUserControl
	{
		public InvoiceLineChargesUserControl()
		{
			InitializeChargesGridLayout();
		}

		public void InitializeChargesGridLayout()
		{
			using (ChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				AddColumnStyles();
				ReOrderGridColumns();
			}
		}

		void AddColumnStyles()
		{
			ChargesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("444324DE-1BD2-422C-9E92-35333784A75E", "Explanation"),
				ColumnName = InvoiceLineCharge.Schema.Explanation,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130)
			});
		}

		void ReOrderGridColumns()
		{
			ChargesGrid.ReOrderColumns(
				[
					InvoiceLineCharge.Schema.J7_ChargeType,
					InvoiceLineCharge.Schema.ChargeCodeDescription,
					InvoiceLineCharge.Schema.J7_Amount,
					InvoiceLineCharge.Schema.J7_RX_NKCurrency,
					InvoiceLineCharge.Schema.Explanation,
					InvoiceLineCharge.Schema.J7_IsDutiable,
					InvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable,
					InvoiceLineCharge.Schema.J7_IsGSTApplicable,
					InvoiceLineCharge.Schema.J7_Percentage,
					InvoiceLineCharge.Schema.J7_IsIncludedInITOT,
					InvoiceLineCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
					JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable,
					InvoiceLineCharge.Schema.J7_ExchangeRate
				]);
		}
	}
}

using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class InvoiceLineChargesUserControlTest : TestCaseWithFactory
	{
		public void TestColumnsReadonly()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var apportionedChargesGrid = control.ApportionedChargesGrid;
				AssertEquals("J7_Amount column ReadOnly.", true, apportionedChargesGrid.GetColumnStyle(InvoiceLineApportionCharge.Schema.J7_Amount).IsReadOnly);
				AssertEquals("J7_RX_NKCurrency column ReadOnly.", true, apportionedChargesGrid.GetColumnStyle(InvoiceLineApportionCharge.Schema.J7_RX_NKCurrency).IsReadOnly);
			}
		}

		public void TestExplanationColumnStyles()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var chargesGrid = control.ChargesGrid;

				var explanationColumnStyle = chargesGrid.GetColumnStyle(InvoiceLineCharge.Schema.Explanation);
				AssertNotNull("Explanation column should be available in the grid.", explanationColumnStyle);
				AssertEquals("Explanation column width should be set correctly.", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130), explanationColumnStyle.Width);
			}
		}

		public void TestReOrderGridColumns()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				control.InitializeChargesGridLayout();
				var invoiceChargesGridColumnStyle = control.ChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

				int index = 0;
				AssertEquals("Column order 1 => J7_ChargeType", InvoiceLineCharge.Schema.J7_ChargeType, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 2 => ChargeCodeDescription", InvoiceLineCharge.Schema.ChargeCodeDescription, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 3 => J7_Amount", InvoiceLineCharge.Schema.J7_Amount, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 4 => J7_RX_NKCurrency", InvoiceLineCharge.Schema.J7_RX_NKCurrency, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 5 => Explanation", InvoiceLineCharge.Schema.Explanation, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 6 => J7_IsDutiable", InvoiceLineCharge.Schema.J7_IsDutiable, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 7 => J7_IsStatisticalValueApplicable", InvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 8 => J7_IsGSTApplicable", InvoiceLineCharge.Schema.J7_IsGSTApplicable, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 9 => J7_Percentage", InvoiceLineCharge.Schema.J7_Percentage, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 10 => J7_IsIncludedInITOT", InvoiceLineCharge.Schema.J7_IsIncludedInITOT, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 11 => J7_Calc_IsIncludedInInvoiceAmount", InvoiceLineCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 12 => IsJ7_ExchangeRateUserEnterable", JobComInvCharge.Schema.IsJ7_ExchangeRateUserEnterable, invoiceChargesGridColumnStyle[index++].ColumnName);
				AssertEquals("Column order 13 => J7_ExchangeRate", InvoiceLineCharge.Schema.J7_ExchangeRate, invoiceChargesGridColumnStyle[index++].ColumnName);
			}
		}
	}
}

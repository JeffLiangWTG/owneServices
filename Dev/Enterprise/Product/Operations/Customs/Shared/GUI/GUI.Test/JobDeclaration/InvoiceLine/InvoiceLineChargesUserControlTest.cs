using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceLineChargesUserControlBaseOnlyTest : TestCaseWithFactory
	{
		public void TestChargesGridColumnWidth()
		{
			using (var control = new InvoiceLineChargesUserControl())
			{
				var chargesGrid = control.ChargesGrid;
				CombineAssertions(() =>
				{
					AssertEquals("J7_ChargeType", 47, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_ChargeType).Width);
					AssertEquals("ChargeCodeDescription", 46, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.ChargeCodeDescription).Width);
					AssertEquals("J7_Amount", 68, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_Amount).Width);
					AssertEquals("J7_RX_NKCurrency", 47, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_RX_NKCurrency).Width);
					AssertEquals("J7_IsDutiable", 63, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsDutiable).Width);
					AssertEquals("J7_IsGSTApplicable", 97, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsGSTApplicable).Width);
					AssertEquals("J7_Percentage", 100, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_Percentage).Width);
					AssertEquals("J7_IsIncludedInITOT", 102, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_IsIncludedInITOT).Width);
					AssertEquals("J7_Calc_IsIncludedInInvoiceAmount", 117, chargesGrid.GetColumnStyle(JobComInvCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Width);
				});
			}
		}
	}
}

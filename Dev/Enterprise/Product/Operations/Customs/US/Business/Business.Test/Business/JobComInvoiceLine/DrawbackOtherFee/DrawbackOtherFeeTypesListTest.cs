using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DrawbackOtherFeeTypesListTest : TestCase
	{
		public void TestHasNoDefaultAccountingClassCodes()
		{
			var feeTypesList = new DrawbackOtherFeeTypesList();
			AssertEquals(false, feeTypesList.ContainsCode(DrawbackOtherFeeTypesList.Codes.DrawbackDuty));
			AssertEquals(false, feeTypesList.ContainsCode(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes));
			AssertEquals(false, feeTypesList.ContainsCode(DrawbackOtherFeeTypesList.Codes.DrawbackHMF));
			AssertEquals(false, feeTypesList.ContainsCode(DrawbackOtherFeeTypesList.Codes.DrawbackMPF));
		}

		public void TestIsGrandTotalDutyAmountFee()
		{
			AssertEquals(true, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackDuty));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackHMF));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackMPF));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.OilSpillTax));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.DomesticTax));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackSuperfundTax));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(DrawbackOtherFeeTypesList.Codes.CottonFee));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee("X"));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalDutyAmountFee(""));
		}

		public void TestIsGrandTotalIRTaxAmountFee()
		{
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackDuty));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackHMF));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackMPF));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.OilSpillTax));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.DomesticTax));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.DrawbackSuperfundTax));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(DrawbackOtherFeeTypesList.Codes.CottonFee));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee("X"));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsGrandTotalIRTaxAmountFee(""));
		}

		public void TestIsOtherFee()
		{
			AssertEquals(false, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.DrawbackDuty));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.DrawbackHMF));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.DrawbackMPF));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.OilSpillTax));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.DomesticTax));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.DrawbackSuperfundTax));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsOtherFee(DrawbackOtherFeeTypesList.Codes.CottonFee));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsOtherFee("X"));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsOtherFee(""));
		}

		public void TestIsPuertoRicoFee()
		{
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.DrawbackDuty));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.DrawbackTaxes));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.DrawbackHMF));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.DrawbackMPF));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.OilSpillTax));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.DomesticTax));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.DrawbackSuperfundTax));
			AssertEquals(true, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.PRDrawbackDuty));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(DrawbackOtherFeeTypesList.Codes.CottonFee));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee("X"));
			AssertEquals(false, DrawbackOtherFeeTypesList.IsPuertoRicoFee(""));
		}
	}
}

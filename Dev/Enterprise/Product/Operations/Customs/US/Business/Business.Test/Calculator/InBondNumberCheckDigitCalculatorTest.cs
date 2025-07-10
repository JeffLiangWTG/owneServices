using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InBondNumberCheckDigitCalculatorTest : TestCase
	{
		public void TestCheckDigit()
		{
			AssertEquals("CheckDigit", "2", InBondNumberCheckDigitCalculator.GetCheckDigit("25770005"));
			AssertEquals("CheckDigit", "4", InBondNumberCheckDigitCalculator.GetCheckDigit("25770014"));
			AssertEquals("CheckDigit", "5", InBondNumberCheckDigitCalculator.GetCheckDigit("25770015"));
			AssertEquals("CheckDigit", "6", InBondNumberCheckDigitCalculator.GetCheckDigit("25770016"));
			AssertEquals("CheckDigit", "0", InBondNumberCheckDigitCalculator.GetCheckDigit("25770017"));
			AssertEquals("CheckDigit", "1", InBondNumberCheckDigitCalculator.GetCheckDigit("25770018"));
			AssertEquals("CheckDigit", "2", InBondNumberCheckDigitCalculator.GetCheckDigit("25770019"));
			AssertEquals("CheckDigit", "3", InBondNumberCheckDigitCalculator.GetCheckDigit("25770020"));
		}

		public void TestCalculatePaperlessITNoCheckDigit()
		{
			AssertEquals("CheckDigit", 0, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("v7210045050"));
			AssertEquals("CheckDigit", 0, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V7210045050"));
			AssertEquals("CheckDigit", 5, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V7603245275"));
			AssertEquals("CheckDigit", 5, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("v7603245275"));
			AssertEquals("CheckDigit", 5, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V7603245273"));
			AssertEquals("CheckDigit", 7, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("VF603245273"));
			AssertEquals("CheckDigit", 1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("VFK03245273"));
			AssertEquals("CheckDigit", 1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V2J03245273"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V1"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V12"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V123"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("1"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("12"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("123"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("1234"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("12345"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("123456"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("1234567"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("12345678"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("123456789"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("1234567890"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("12345678901"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("123456789012"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("1234567890123"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("12345678901234"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("123456789012345"));
			AssertEquals("CheckDigit", -1, InBondNumberCheckDigitCalculator.CalculatePaperlessITNoCheckDigit("V2J0324A2D3"));
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EntryNumberCheckDigitCalculatorTest : TestCaseWithFactory
	{
		public void TestCheckDigit()
		{
			AssertEquals(8, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 0));
			AssertEquals(9, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 1));
			AssertEquals(3, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 5));
			AssertEquals(7, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 9));
			AssertEquals(8, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 0));
			AssertEquals(9, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 1));
			AssertEquals(3, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 5));
			AssertEquals(7, EntryNumberCheckDigitCalculator.GetCheckDigit("B76", "0324527", 9));
		}
	}
}

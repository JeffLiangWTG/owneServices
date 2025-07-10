using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ITCINValidatorTest : TestCaseWithFactory
	{
		public void TestCalculateCheckDigitAgainstSixCharLengthString()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentOutOfRangeException>(() => { ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("1234567"); });
				AssertEquals("Check digit of an empty string", "", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString(""));
				AssertEquals("000000", "J", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("000000"));
				AssertEquals("11942", "Q", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("11942"));
				AssertEquals("13122", "C", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("13122"));
				AssertEquals("18517", "M", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("18517"));
				AssertEquals("19007", "T", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("19007"));
				AssertEquals("22134", "H", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("22134"));
				AssertEquals("9465", "H", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("9465"));
				AssertEquals("11367", "X", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("11367"));
				AssertEquals("000011367", "X", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("000011367"));
				AssertEquals("113678", "A", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString("113678"));
				AssertEquals(" 0113678", "A", ITCINValidator.CalculateCheckDigitAgainstSixCharLengthString(" 0113678"));
			});
		}

		public void TestCalculateCheckDigitAgainstEightCharLengthString()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentOutOfRangeException>(() => { ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString("123456789"); });
				AssertEquals("Check digit of an empty string", "", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString(""));
				AssertEquals("9041", "F", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString("9041"));
				AssertEquals("3227", "K", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString("3227"));
				AssertEquals("322182", "U", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString("322182"));
				AssertEquals("322173", "H", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString("322173"));
				AssertEquals("13540", "B", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString("13540"));
				AssertEquals(" 013540", "B", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString(" 013540"));
				AssertEquals("000013540", "B", ITCINValidator.CalculateCheckDigitAgainstEightCharLengthString("000013540"));
			});
		}
	}
}

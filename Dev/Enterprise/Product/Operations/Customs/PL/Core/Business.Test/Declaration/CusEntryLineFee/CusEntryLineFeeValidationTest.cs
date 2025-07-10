using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business.Testing;

class CusEntryLineFeeValidationTest : EU.Business.Declaration.Testing.EUUniversalCusEntryLineFeeValidationTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	public void TestCheckCF_BaseValueDecimalPrecision()
	{
		var (_, _, entryLineFee) = SetEntryLineFeeData();
		entryLineFee.CF_BaseValue = 3.1M;

		CombineAssertions(() =>
		{
			AssertHasError("Value with decimal places", entryLineFee.CF_BaseValueInfo, "Base Amount allows no decimal place values.");
			entryLineFee.CF_BaseValue = 3M;
			AssertNoError("Value without decimal places", entryLineFee.CF_BaseValueInfo, "Base Amount allows no decimal place values.");
		});
	}

	public void TestCheckCF_BaseValueForDisplay()
	{
		var (_, _, entryLineFee) = SetEntryLineFeeData();
		entryLineFee.CF_BaseValueForDisplay = "3,1";

		CombineAssertions(() =>
		{
			AssertHasError("Value with decimal places", entryLineFee.CF_BaseValueForDisplayInfo, "Base Amount allows no decimal place values.");
			entryLineFee.CF_BaseValueForDisplay = "3";
			AssertNoError("Value without decimal places", entryLineFee.CF_BaseValueForDisplayInfo, "Base Amount allows no decimal place values.");
		});
	}

	public void TestCheckCF_RateForDisplay()
	{
		var (_, _, entryLineFee) = SetEntryLineFeeData();
		entryLineFee.CF_RateForDisplay = "3,1234567";
		var messageError = "Tax Rate allows only " + CusEntryLineFee.TaxRateDecimalPrecision + " decimal places.";

		CombineAssertions(() =>
		{
			AssertHasMessageError("Decimal places more than allowed", entryLineFee.CF_RateForDisplayInfo, messageError);
			entryLineFee.CF_RateForDisplay = "3,1324";
			AssertNoMessageError("Decimal places less than allowed", entryLineFee.CF_RateForDisplayInfo, messageError);
		});
	}

	protected override void AssertNotificationsForEmptyMethodOfPayment(EU.Business.Declaration.CusEntryLineFee entryLineFee)
	{
		AssertHasMessageError("Empty Method of Payment", entryLineFee.CF_MethodOfPaymentInfo, "You have not entered a value.");
	}

	protected override string InvalidMethodOfPaymentForListValidationTest => "I";
}

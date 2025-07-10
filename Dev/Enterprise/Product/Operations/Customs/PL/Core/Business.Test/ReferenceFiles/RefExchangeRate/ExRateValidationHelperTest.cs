using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class ExRateValidationHelperTest : TestCaseWithFactory
{
	public void TestCheckIfCorrectMultiplierUsed()
	{
		var exRate = RefExchangeRate.New(Factory);
		exRate.Company.SetCountry(Core.Constants.CountryCodes.Poland);

		var errorMessage = "Invalid value. Please insert correct decimal value or leave this field empty.\r\nMake sure using ',' (COMMA) instead of '.' (DOT) for decimal separator.";
		var warningMessage = "Invalid As Published value. Correct value should contain multiplied exchange rate by 1/100/10000";

		exRate.RE_SellRate = 1.1m;

		exRate.RE_AsPublished = "1,1";
		exRate.Validation.ValidateRE_AsPublished();
		AssertNoNotifications(exRate.RE_AsPublishedInfo);
		exRate.RE_AsPublished = "110,0";
		exRate.Validation.ValidateRE_AsPublished();
		AssertNoNotifications(exRate.RE_AsPublishedInfo);
		exRate.RE_AsPublished = "11000,0";
		exRate.Validation.ValidateRE_AsPublished();
		AssertNoNotifications(exRate.RE_AsPublishedInfo);

		exRate.RE_AsPublished = "0,11";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasWarnings(warningMessage, exRate.RE_AsPublishedInfo);
		exRate.RE_AsPublished = "11,0";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasWarnings(warningMessage, exRate.RE_AsPublishedInfo);
		exRate.RE_AsPublished = "1100,00";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasWarnings(warningMessage, exRate.RE_AsPublishedInfo);

		exRate.RE_SellRate = 1.2m;

		exRate.RE_AsPublished = "1,2";
		exRate.Validation.ValidateRE_AsPublished();
		AssertNoNotifications(exRate.RE_AsPublishedInfo);

		exRate.RE_AsPublished = "1.2";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasMessageErrors(errorMessage, exRate.RE_AsPublishedInfo);

		exRate.RE_AsPublished = "0,12";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasWarnings(warningMessage, exRate.RE_AsPublishedInfo);

		exRate.RE_AsPublished = "1,20001";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasWarnings(warningMessage, exRate.RE_AsPublishedInfo);

		exRate.RE_AsPublished = "1,19999";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasWarnings(warningMessage, exRate.RE_AsPublishedInfo);

		exRate.RE_AsPublished = "1,20001";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasWarnings(warningMessage, exRate.RE_AsPublishedInfo);

		exRate.RE_AsPublished = "ABCD";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasMessageErrors(errorMessage, exRate.RE_AsPublishedInfo);
		exRate.RE_AsPublished = "A,2";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasMessageErrors(errorMessage, exRate.RE_AsPublishedInfo);
		exRate.RE_AsPublished = "1,2A";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasMessageErrors(errorMessage, exRate.RE_AsPublishedInfo);
		exRate.RE_AsPublished = "1,A2";
		exRate.Validation.ValidateRE_AsPublished();
		AssertHasMessageErrors(errorMessage, exRate.RE_AsPublishedInfo);
	}
}

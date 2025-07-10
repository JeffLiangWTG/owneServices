using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefRateCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCR7_RateType()
		{
			var testItem = Factory.NewWithValidTestData<CusRefRateCode>();
			testItem.CR7_RateType = "XXX";
			AssertHasErrorContaining(testItem.CR7_RateTypeInfo, ListValidation.InvalidCodeError);
			testItem.CR7_RateType = "OTH";
			AssertNoErrors(testItem.CR7_RateTypeInfo);
		}

		public void TestCheckCR7_RateCode()
		{
			var testItem = Factory.New<CusRefRateCode>();
			testItem.Validation.ValidateCR7_RateCode();
			AssertHasErrorContaining(testItem.CR7_RateCodeInfo, MandatoryValidation.MustBeEntered);
			testItem.CR7_RateCode = "CD1";
			AssertNoErrors(testItem.CR7_RateCodeInfo);
		}

		public void TestCheckCR7_Description()
		{
			var testItem = Factory.New<CusRefRateCode>();
			testItem.Validation.ValidateCR7_Description();
			AssertHasErrorContaining(testItem.CR7_DescriptionInfo, MandatoryValidation.MustBeEntered);
			testItem.CR7_Description = "DESC";
			AssertNoErrors(testItem.CR7_DescriptionInfo);
		}

		public void TestCheckCR7_RN_NKCountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion);
			Factory.Save();
			var testItem = Factory.New<CusRefRateCode>();
			var targetInfo = testItem.CR7_RN_NKCountryCodeInfo;
			testItem.Validation.ValidateCR7_RN_NKCountryCode();
			AssertNoErrors(targetInfo);
			testItem.CR7_RN_NKCountryCode = "XX";
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			testItem.CR7_RN_NKCountryCode = "ER";
			AssertNoErrors(targetInfo);
		}
	}
}

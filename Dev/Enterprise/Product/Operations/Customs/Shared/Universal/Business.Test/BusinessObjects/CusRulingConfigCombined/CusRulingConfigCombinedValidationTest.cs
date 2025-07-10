using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.Universal.RefCusRulingConfigCategories.Codes;
using static Enterprise.Customs.Universal.RefCusRulingConfigTypes.Codes;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRulingConfigCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZY_Category()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			rulingConfig.Validation.ValidateZZY_Category();
			AssertHasError(rulingConfig.ZZY_CategoryInfo, "Please enter a value.");
			rulingConfig.ZZY_Category = "A";
			AssertHasError(rulingConfig.ZZY_CategoryInfo, "Enter a valid selection.");
			rulingConfig.ZZY_Category = DTY;
			AssertNoErrors(rulingConfig.ZZY_CategoryInfo);
			rulingConfig.ZZY_Category = DAT;
			AssertNoErrors(rulingConfig.ZZY_CategoryInfo);
			rulingConfig.ZZY_Category = EXD;
			AssertNoErrors(rulingConfig.ZZY_CategoryInfo);
		}

		public void TestCheckZZY_Type()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			rulingConfig.Validation.ValidateZZY_Type();
			AssertHasError(rulingConfig.ZZY_TypeInfo, "Please enter a value.");
			rulingConfig.ZZY_Type = "A";
			AssertHasError(rulingConfig.ZZY_TypeInfo, "Enter a valid selection.");
			rulingConfig.ZZY_Category = DTY;
			rulingConfig.ZZY_Type = AdValorem;
			AssertNoErrors(rulingConfig.ZZY_TypeInfo);
		}

		public void TestCheckZZY_Rate()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			rulingConfig.ZZY_Category = DTY;
			rulingConfig.ZZY_Type = AcceptAmount;
			rulingConfig.Validation.ValidateZZY_Rate();
			AssertHasError(rulingConfig.ZZY_RateInfo, "Please enter a value.");
			rulingConfig.ZZY_Rate = 1234;
			AssertNoErrors(rulingConfig.ZZY_RateInfo);
		}
	}
}

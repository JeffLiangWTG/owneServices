using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefPreferenceViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZS_ZZZ_NKDataGrouping()
		{
			var testItem = Factory.New<CusRefPreferenceView>();
			var targetInfo = testItem.ZZS_ZZZ_NKDataGroupingInfo;
			testItem.ZZS_ZZZ_NKDataGrouping = "EUN";
			AssertHasErrorContaining(targetInfo, "Country/Region Code for non-system defined Preference cannot be longer than 2 characters.");
			testItem.ZZS_ZZZ_NKDataGrouping = "CN";
			AssertNoErrors(targetInfo);
		}
	}
}

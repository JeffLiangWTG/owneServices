using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRulingConfigCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRulingConfigCategoryList()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			AssertEquals(typeof(RefCusRulingConfigCategories), rulingConfig.Lookups.RulingConfigCategoryList.GetType());
			AssertEquals(6, rulingConfig.Lookups.RulingConfigCategoryList.Count);
			AssertSame(Factory.GetCachedValue<RefCusRulingConfigCategories>(), rulingConfig.Lookups.RulingConfigCategoryList);
		}

		public void TestRulingConfigTypeList()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			AssertEquals(typeof(RefCusRulingConfigTypes), rulingConfig.Lookups.RulingConfigTypeList.GetType());
			AssertEquals(13, rulingConfig.Lookups.RulingConfigTypeList.Count);
			AssertSame(Factory.GetCachedValue<RefCusRulingConfigTypes>(), rulingConfig.Lookups.RulingConfigTypeList);
		}

		public void TestRulingConfigValueList()
		{
			var ruling = Factory.NewWithValidTestData<ZZRefCusRulingCombined>();
			var rulingConfig = ruling.Configurations.AddNew();
			AssertEquals(typeof(RefCusRulingConfigValuesForAcceptType), rulingConfig.Lookups.RulingConfigValueList.GetType());
			AssertEquals(2, rulingConfig.Lookups.RulingConfigValueList.Count);
			AssertSame(Factory.GetCachedValue<RefCusRulingConfigValuesForAcceptType>(), rulingConfig.Lookups.RulingConfigValueList);
		}
	}
}

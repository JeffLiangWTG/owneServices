using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCountryFilterBusinessObject))]
	sealed class RefCountryFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestEconomicGroupFilter()
		{
			AddCountryWithEconomicGroupForTest("XX", "");
			AddCountryWithEconomicGroupForTest("A1", EconomicGroupList.Codes.ASEAN);
			AddCountryWithEconomicGroupForTest("A2", EconomicGroupList.Codes.ASEAN);
			AddCountryWithEconomicGroupForTest("E1", EconomicGroupList.Codes.EuropeanUnion);
			AddCountryWithEconomicGroupForTest("N1", EconomicGroupList.Codes.NAFTA);
			AddCountryWithEconomicGroupForTest("N2", EconomicGroupList.Codes.NAFTA);

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var economicGroupFilter = (ModuleTextFilter)filterBizObj["Economic Group"];
			economicGroupFilter.IsActive = true;
			economicGroupFilter.Property = "";
			AssertContainsExactElementsInAnyOrder(
				new[] { "XX", "A1", "A2", "E1", "N1", "N2" },
				Factory.Load<RefCountry>(filterBizObj.Filter).Select(x => x.RN_Code.ToString()));

			economicGroupFilter.Property = EconomicGroupList.Codes.ASEAN;
			AssertContainsExactElementsInAnyOrder(
				new[] { "A1", "A2" },
				Factory.Load<RefCountry>(filterBizObj.Filter).Select(x => x.RN_Code.ToString()));

			economicGroupFilter.Property = EconomicGroupList.Codes.EuropeanUnion;
			AssertContainsExactElementsInAnyOrder(
				new[] { "E1" },
				Factory.Load<RefCountry>(filterBizObj.Filter).Select(x => x.RN_Code.ToString()));
		}

		#region Implementation

		void AddCountryWithEconomicGroupForTest(string countryCode, string economicGroup)
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode) ?? Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = countryCode;
			country.RN_EconomicGrouping = economicGroup;
			country.RN_Desc = RefCountryFilterBusinessObjectForTest.CountryDescForTest;
		}

		RefCountryFilterBusinessObject GetFilterBizObj()
		{
			return new RefCountryFilterBusinessObjectForTest();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCountryFilterBusinessObject();
		}

		#endregion
	}
}

using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCommodityCodeMapLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLocalCodeProviderList_WhenCountryIsGermany_ThenOnlyGermanyShown()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();
			commodityCodeMap.LC_RN_NKCountry = CountryCodes.Germany;

			AssertExpectResult(new string[] { "DBH" }, commodityCodeMap);
		}

		public void TestLocalCodeProviderList_WhenCountryIsNotGermany_ThenNoResultsShown()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();
			commodityCodeMap.LC_RN_NKCountry = CountryCodes.Australia;

			AssertExpectResult(System.Array.Empty<string>(), commodityCodeMap);
		}

		public void TestLocalCodeProviderList_WhenCountryIsInvalid_ThenNoResultsShown()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();
			commodityCodeMap.LC_RN_NKCountry = CountryCodes.EuropeanUnion;

			AssertExpectResult(System.Array.Empty<string>(), commodityCodeMap);
		}

		public void TestLocalCodeProviderList_WhenCountryIsBlank_ThenOnlyRATShown()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();
			commodityCodeMap.LC_RN_NKCountry = ZString.Empty;

			AssertExpectResult(new string[] { "RAT" }, commodityCodeMap);
		}

		public void TestLocalCodeProviderList_AllCountries()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();

			AssertExpectResult(new string[] { "RAT", "DBH" }, commodityCodeMap.Lookups.AllProviders);
		}

		static void AssertExpectResult(string[] expectedCodes, CodeDescriptionPairList pairs)
		{
			var results =
				pairs
				.ToList<CodeDescriptionPair>().Select(x => x.Code);
			AssertContainsExactElementsInAnyOrder(expectedCodes, results);
		}

		static void AssertExpectResult(string[] expectedCodes, RefCommodityCodeMap commodityCodeMap )
		{
			AssertExpectResult(expectedCodes, commodityCodeMap.Lookups.LocalProviders);
		}
	}
}

using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using CountryCodes = Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCommodityCodeMapValidationTest : BusinessObjectValidationTestCase
	{
		#region Local Code Provider

		public void TestLocalCodeProvider()
		{
			var aluminiumCommodity = GetCommodity("ALUM");
			var commodityCodeMap = CreateCommodityCodeMap(aluminiumCommodity, ZString.Empty);
			AssertLocalCodeProvider(
				commodityCodeMap,
				expectedValidProviders: new[] { GlobalCommodityCodeProviderList.Codes.Rating },
				expectedInvalidProviders: new[] { DELocalCommodityCodeProviderList.Codes.DE_DBH });
			AssertLocalCodeProviderDuplicate(aluminiumCommodity, GlobalCommodityCodeProviderList.Codes.Rating);

			var beverageCommodity = GetCommodity("BVRG");
			commodityCodeMap = CreateCommodityCodeMap(beverageCommodity, CountryCodes.Germany);
			AssertLocalCodeProvider(
				commodityCodeMap,
				expectedValidProviders: new[] { DELocalCommodityCodeProviderList.Codes.DE_DBH },
				expectedInvalidProviders: new[] { GlobalCommodityCodeProviderList.Codes.Rating });
			AssertLocalCodeProviderDuplicate(beverageCommodity, DELocalCommodityCodeProviderList.Codes.DE_DBH);

			commodityCodeMap = CreateCommodityCodeMap(GetCommodity("IRON"), CountryCodes.Australia);
			AssertLocalCodeProvider(
				commodityCodeMap,
				expectedValidProviders: Array.Empty<string>(),
				expectedInvalidProviders: new[]
				{
						GlobalCommodityCodeProviderList.Codes.Rating,
						DELocalCommodityCodeProviderList.Codes.DE_DBH
				});
		}

		void AssertLocalCodeProvider(RefCommodityCodeMap commodityCodeMap, string[] expectedValidProviders, string[] expectedInvalidProviders)
		{
			commodityCodeMap.LC_LocalCodeProvider = ZString.Empty;
			AssertHasError("empty LocalCodeProvider", commodityCodeMap.LC_LocalCodeProviderInfo, "Please enter a value.");

			foreach (var validProvider in expectedValidProviders)
			{
				commodityCodeMap.LC_LocalCodeProvider = validProvider;
				AssertNoErrors($"valid LocalCodeProvider {validProvider}", commodityCodeMap.LC_LocalCodeProviderInfo);
			}

			var expectedInvalidErrorMessage = $"The Usage code is not supported for the Country/Region";
			foreach (var invalidProvider in expectedInvalidProviders)
			{
				commodityCodeMap.LC_LocalCodeProvider = invalidProvider;
				AssertHasErrorContaining("invalid LocalCodeProvider", commodityCodeMap.LC_LocalCodeProviderInfo, expectedInvalidErrorMessage);
			}

			commodityCodeMap.LC_LocalCodeProvider = "XXX";
			AssertHasErrorContaining("invalid LocalCodeProvider 'XXX'", commodityCodeMap.LC_LocalCodeProviderInfo, expectedInvalidErrorMessage);
		}

		void AssertLocalCodeProviderDuplicate(RefCommodityCode commodity, string localCodeProvider)
		{
			var commodityCodeMap = commodity.RefCommodityCodeMaps.Single();
			commodityCodeMap.LC_LocalCodeProvider = localCodeProvider;

			var commodityCodeMapDuplicate = commodity.RefCommodityCodeMaps.AddNew();
			commodityCodeMapDuplicate.LC_RN_NKCountry = commodityCodeMap.LC_RN_NKCountry;
			commodityCodeMapDuplicate.LC_LocalCodeProvider = localCodeProvider;

			AssertNoErrors(commodityCodeMap.LC_LocalCodeProviderInfo);
			AssertHasError(commodityCodeMapDuplicate.LC_LocalCodeProviderInfo, "Only one local code can be configured for each unique Country/Region & Usage pair.");
		}

		RefCommodityCode GetCommodity(string code)
			=> Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, code);

		RefCommodityCodeMap CreateCommodityCodeMap(RefCommodityCode commodity, string countryCode)
		{
			var commodityCodeMap = commodity.RefCommodityCodeMaps.AddNew();
			commodityCodeMap.LC_RH_NKCommodityCode = commodity.RH_Code;
			commodityCodeMap.LC_LocalCode = "XXXX";
			commodityCodeMap.LC_RN_NKCountry = countryCode;
			return commodityCodeMap;
		}

		#endregion

		public void TestLocalCode()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();

			commodityCodeMap.LC_LocalCode = ZString.Empty;
			AssertHasError(commodityCodeMap.LC_LocalCodeInfo, "Please enter a value.");

			commodityCodeMap.LC_LocalCode = "E";
			AssertNoErrors(commodityCodeMap.LC_LocalCodeInfo);

			commodityCodeMap.LC_LocalCode = "12345678901234567890";
			AssertNoErrors(commodityCodeMap.LC_LocalCodeInfo);

			AssertExceptionThrown<MaxLengthExceededException>(() => commodityCodeMap.LC_LocalCode = "123456789012345678901");
			ErrorReporter.Clear();
		}

		public void TestCountry()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();

			commodityCodeMap.LC_RN_NKCountry = ZString.Empty;
			AssertNoErrors(commodityCodeMap.LC_RN_NKCountryInfo);

			commodityCodeMap.LC_RN_NKCountry = CountryCodes.Germany;
			AssertNoErrors("Germany", commodityCodeMap.LC_RN_NKCountryInfo);

			commodityCodeMap.LC_RN_NKCountry = CountryCodes.Australia;
			AssertNoErrors("Australia", commodityCodeMap.LC_RN_NKCountryInfo);

			// Although EuropeanUnion is a Country (as far as CountryCodes is concerned),
			// it is not part of the RefCountryCollection in the default Odyssey database.
			commodityCodeMap.LC_RN_NKCountry = CountryCodes.EuropeanUnion;
			AssertHasError("European Union", commodityCodeMap.LC_RN_NKCountryInfo, "Enter a valid selection.");

			commodityCodeMap.LC_RN_NKCountry = "S";
			AssertHasError("Invalid country", commodityCodeMap.LC_RN_NKCountryInfo, "Enter a valid selection.");
		}

		public void TestCommodityCode()
		{
			var commodityCodeMap = Factory.New<RefCommodityCodeMap>();

			commodityCodeMap.LC_RH_NKCommodityCode = ZString.Empty;
			AssertHasError("empty commodity", commodityCodeMap.LC_RH_NKCommodityCodeInfo, "Please enter a value.");

			commodityCodeMap.LC_RH_NKCommodityCode = "C";
			AssertHasError("invalid commodity", commodityCodeMap.LC_RH_NKCommodityCodeInfo, "Enter a valid selection.");

			commodityCodeMap.LC_RH_NKCommodityCode = "CHEM";
			AssertNoErrors("valid commodity", commodityCodeMap.LC_RH_NKCommodityCodeInfo);
		}
	}
}

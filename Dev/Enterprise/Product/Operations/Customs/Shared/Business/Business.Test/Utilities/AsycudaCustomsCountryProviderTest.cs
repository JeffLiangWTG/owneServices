using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	[AsycudaCustomsCountries("NA", "LS", "BW", "SZ")]
	sealed class AsycudaCustomsCountryProviderTest : BusinessObjectLookupsTestCase
	{
		public void TestGetAsycudaCustomsCountryCodes()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "NA", "LS", "BW", "SZ" }, new AsycudaCustomsCountryProvider().GetAsycudaCustomsCountryCodes());
		}

		public void TestGetAsycudaXMLCountryCodes()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda,
				"NA",
				RefCountry.LoadFromCountryCode(factory, "NA").RN_Desc,
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codeList.PK, "AsycudaXML", "Y");
			factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { "NA" }, new AsycudaCustomsCountryProvider().GetAsycudaXMLCountryCodes());
		}

		[SelfManagedTariffCountries("CD", "CI", "CG")]
		public void TestIsSelfManagedTariffCountry()
		{
			AssertEquals("Self managed tariff country", true, new AsycudaCustomsCountryProvider().IsSelfManagedTariffCountry("CD"));
			AssertEquals("Not self managed tariff country", false, new AsycudaCustomsCountryProvider().IsSelfManagedTariffCountry("AU"));
		}

		[SelfManagedTariffCountries("CD", "CI", "CG")]
		public void TestGetSelfManagedTariffCountryCodes()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "CD", "CI", "CG" }, new AsycudaCustomsCountryProvider().GetSelfManagedTariffCountryCodes());
		}
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWAdditionalAttributeInformationProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDescription()
		{
			var tariffTypeForTW = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "222");
			Factory.Save();
			Helper.CreateCusCodeType("TWCR", "Taiwan Customs Requirements");
			Helper.CreateCusCodeType("TWIR", "Taiwan Import Regulations");
			Helper.CreateCusCodeType("TWER", "Taiwan Export Regulations");
			Helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWCR", "AAA", "AAA Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWIR", "BBB", "BBB Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, "TWER", "CCC", "CCC Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffViewForTW = Helper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeForTW.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Tariff Description");
			var tariffAttributeViewForCustomsRequirements = Helper.CreateTariffAttribute("AAA", "CustomsRequirements", tariffViewForTW);
			var tariffAttributeViewForImportRegulations = Helper.CreateTariffAttribute("BBB", "ImportRegulations", tariffViewForTW);
			var tariffAttributeViewForExportRegulations = Helper.CreateTariffAttribute("CCC", "ExportRegulations", tariffViewForTW);
			Factory.Save();
			var providerForTW = new TWAdditionalAttributeInformationProvider(Factory);
			AssertEquals("AAA Description", providerForTW.AdditionalDescription("CustomsRequirements", "AAA"));
			AssertEquals("BBB Description", providerForTW.AdditionalDescription("ImportRegulations", "BBB"));
			AssertEquals("CCC Description", providerForTW.AdditionalDescription("ExportRegulations", "CCC"));
			AssertEquals("", providerForTW.AdditionalDescription("TEST", "DDD"));
		}

		public void TestAdditionalDescriptionVisible()
		{
			AssertEquals(true, new TWAdditionalAttributeInformationProvider(Factory).AdditionalDescriptionVisible);
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}

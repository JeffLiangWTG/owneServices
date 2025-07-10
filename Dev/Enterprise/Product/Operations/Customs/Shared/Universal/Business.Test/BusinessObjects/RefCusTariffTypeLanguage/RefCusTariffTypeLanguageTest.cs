using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffTypeLanguage))]
	class RefCusTariffTypeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateRefCusTariffTypeLanguage(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateRefCusTariffTypeLanguage(Factory);
		}

		RefCusTariffTypeLanguage CreateRefCusTariffTypeLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Taiwan);
			helper.CreateOrGetLanguage("ZHT", "Chinese Traditional");
			Factory.Save();

			var tariffTypeAT = helper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			tariffTypeAT.ZZI_Description = "Alcohol Tax";
			return helper.CreateRefCusTariffTypeLanguage(tariffTypeAT, "ZHT", "酒稅");
		}
	}
}

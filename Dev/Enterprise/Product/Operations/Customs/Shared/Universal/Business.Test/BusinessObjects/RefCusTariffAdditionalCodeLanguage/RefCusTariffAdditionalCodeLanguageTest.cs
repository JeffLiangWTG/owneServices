using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffAdditionalCodeLanguage))]
	public class RefCusTariffAdditionalCodeLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateNewRefCusTariffAdditionalCodeLanguage(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateNewRefCusTariffAdditionalCodeLanguage(Factory);
		}

		RefCusTariffAdditionalCodeLanguage CreateNewRefCusTariffAdditionalCodeLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("EUN", "T1T");
			helper.CreateOrGetLanguage("ENG", "English");
			factory.Save();
			var tariff = helper.LoadOrCreateNewTariff("EUN", tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			var tariffAdditionalCodeView = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "T2T", "C2T");
			factory.Save();
			var tariffAdditionalCodeLanguage = factory.New<RefCusTariffAdditionalCodeLanguage>();
			tariffAdditionalCodeLanguage.ZY4_ZY2_TariffAdditionalCode = tariffAdditionalCodeView.PK;
			tariffAdditionalCodeLanguage.ZY4_Description = "Test Description With Language";
			tariffAdditionalCodeLanguage.ZY4_ZX6_NKLanguage = "ENG";
			return tariffAdditionalCodeLanguage;
		}
	}
}

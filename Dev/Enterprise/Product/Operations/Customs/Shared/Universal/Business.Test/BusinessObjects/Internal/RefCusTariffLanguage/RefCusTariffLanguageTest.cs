using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffLanguage))]
	internal class RefCusTariffLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateNewCusTariffLanguage(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateNewCusTariffLanguage(Factory);
		}

		RefCusTariffLanguage CreateNewCusTariffLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			helper.LoadOrCreateNewCusRateCode(factory, "DJC", rateType.PK);
			helper.CreateOrGetLanguage("ENG", "English");
			factory.Save();
			var tariffName = ZGuid.NewZGuid().ToString().Replace("-", string.Empty);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, tariffName, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Desc");
			var view = factory.Load<TariffView>(tariff.PK);
			var lanuage = factory.New<RefCusTariffLanguage>();
			lanuage.ZX7_ZZ1_Tariff = view.PK;
			lanuage.ZX7_ZX6_NKLanguage = "ENG";
			lanuage.ZX7_Description = "Test Description";
			return lanuage;
		}
	}
}

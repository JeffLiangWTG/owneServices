using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusNomenclatureLanguage))]
	public class RefCusNomenclatureLanguageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return CreateNewCodeListLanguage(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreateNewCodeListLanguage(factory);
		}

		RefCusNomenclatureLanguage CreateNewCodeListLanguage(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("ENG", "English");
			var group = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "100", ZDateTime.BrettsBirthday, ZDateTime.Today, "Test Description", "99...99.99", nomenclatureGroupType: "1P1");
			factory.Save();
			var language = factory.New<RefCusNomenclatureLanguage>();
			language.ZX8_ZZ5_NomenclatureGroup = group.PK;
			language.ZX8_ZX6_NKLanguage = "ENG";
			language.ZX8_Description = "Test Description";
			return language;
		}

		public void TestGetFullDescriptionForCompositeKey()
		{
			var startDate = new ZDateTime(1900, 01, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var currentCountryCode = Core.Constants.CountryCodes.Italy;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ITA", "Italian");
			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "Current Country Description", parent: eunDataGrouping);
			Factory.Save();

			var group1 = helper.CreateNomenclatureGroup("EUN", "01", startDate, endDate, "LIVE ANIMALS", compositeKey: "01.01", nomenclatureGroupType: "CN");
			var group2 = helper.CreateNomenclatureGroup(currentCountryCode, "0105", startDate, endDate, "Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls", compositeKey: "01.01..05", nomenclatureGroupType: "CN");
			var group3 = helper.CreateNomenclatureGroup(currentCountryCode, "010511", startDate, endDate, "Fowls of the species Gallus domesticus", compositeKey: "01.01..05.1.1", nomenclatureGroupType: "CN");

			AssertEquals("No description for language ITA", "", RefCusNomenclatureLanguage.GetFullDescriptionForCompositeKey(Factory, "01.01..05.1.1", currentCountryCode, "CN", ZDateTime.Today, false, true, "ITA"));

			helper.CreateNomenclatureGroupLanguage(group1.PK, "ITA", "ANIMALI VIVI");
			helper.CreateNomenclatureGroupLanguage(group2.PK, "ITA", "Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone");
			Factory.Save();

			AssertEquals("Partial description for language ITA", "", RefCusNomenclatureLanguage.GetFullDescriptionForCompositeKey(Factory, "01.01..05.1.1", currentCountryCode, "CN", ZDateTime.Today, false, true, "ITA"));

			helper.CreateNomenclatureGroupLanguage(group3.PK, "ITA", "Galli e galline della specie Gallus domesticus");
			Factory.Save();

			var expectedDescription = "ANIMALI VIVI Pollame vivo, vale a dire galli e galline della specie Gallus domesticus, anatre, oche, tacchini, tacchine e faraone Galli e galline della specie Gallus domesticus";
			AssertEquals("Full description for language ITA", expectedDescription, RefCusNomenclatureLanguage.GetFullDescriptionForCompositeKey(Factory, "01.01..05.1.1", currentCountryCode, "CN", ZDateTime.Today, false, true, "ITA"));
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffViewWrapper))]
	class TariffViewWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		[TestDate(2019, 10, 23)]
		public void TestProperties()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			var wrapper = tariff.Wrapper;
			AssertEquals("EffectiveDataGrouping", ZString.Empty, wrapper.EffectiveDataGrouping);
			AssertEquals("EffectiveDate", new ZDate(2019, 10, 23), wrapper.EffectiveDate);
			AssertEquals("SelectedNomenclatureDescription", ZString.Empty, wrapper.SelectedNomenclatureDescription);
			AssertEquals("SelectedNomenclatureAlternateLanguageDescription", ZString.Empty, wrapper.SelectedNomenclatureAlternateLanguageDescription);
			tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF2", new ZDate(2019, 11, 24), new ZDateTime(2079, 06, 06), "dummy Description 0");
			wrapper = tariff.Wrapper;
			AssertEquals("EffectiveDataGrouping", ZString.Empty, wrapper.EffectiveDataGrouping);
			AssertEquals("EffectiveDate", new ZDate(2019, 11, 24), wrapper.EffectiveDate);
			AssertEquals("SelectedNomenclatureDescription", ZString.Empty, wrapper.SelectedNomenclatureDescription);
			AssertEquals("SelectedNomenclatureAlternateLanguageDescription", ZString.Empty, wrapper.SelectedNomenclatureAlternateLanguageDescription);
			tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF3", new ZDateTime(2010, 12, 10), new ZDate(2019, 9, 22), "dummy Description 0");
			wrapper = tariff.Wrapper;
			AssertEquals("EffectiveDataGrouping", ZString.Empty, wrapper.EffectiveDataGrouping);
			AssertEquals("EffectiveDate", new ZDate(2019, 9, 22), wrapper.EffectiveDate);
			AssertEquals("SelectedNomenclatureDescription", ZString.Empty, wrapper.SelectedNomenclatureDescription);
			AssertEquals("SelectedNomenclatureAlternateLanguageDescription", ZString.Empty, wrapper.SelectedNomenclatureAlternateLanguageDescription);
		}

		public void TestMatchEffectiveDataGrouping()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			var wrapper = tariff.Wrapper;
			AssertEquals(true, wrapper.MatchEffectiveDataGrouping(ZString.Empty));
			AssertEquals(true, wrapper.MatchEffectiveDataGrouping("ABC"));
			wrapper.EffectiveDataGrouping = "DEF";
			AssertEquals(true, wrapper.MatchEffectiveDataGrouping(ZString.Empty));
			AssertEquals(false, wrapper.MatchEffectiveDataGrouping("ABC"));
			AssertEquals(true, wrapper.MatchEffectiveDataGrouping("DEF"));
		}

		[TestDate(2011, 1, 1)]
		public void TestIsWithInEffectiveDate()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var wrapper = tariff.Wrapper;
			AssertEquals(true, wrapper.IsWithInEffectiveDate(new ZDate(2010, 12, 10), new ZDate(2011, 06, 06)));
			wrapper.EffectiveDate = new ZDate(2010, 12, 9);
			AssertEquals(false, wrapper.IsWithInEffectiveDate(new ZDate(2010, 12, 10), new ZDate(2011, 06, 06)));
			AssertEquals(true, wrapper.IsWithInEffectiveDate(new ZDateTime(2010, 12, 9, 23, 59, 59), new ZDate(2011, 06, 06)));
			wrapper.EffectiveDate = new ZDate(2010, 12, 10);
			AssertEquals(true, wrapper.IsWithInEffectiveDate(new ZDate(2010, 12, 10), new ZDate(2011, 06, 06)));
			wrapper.EffectiveDate = new ZDate(2011, 06, 07);
			AssertEquals(false, wrapper.IsWithInEffectiveDate(new ZDate(2010, 12, 10), new ZDate(2011, 06, 06)));
			wrapper.EffectiveDate = new ZDate(2011, 06, 06);
			AssertEquals(true, wrapper.IsWithInEffectiveDate(new ZDate(2010, 12, 10), new ZDate(2011, 06, 06)));
			AssertEquals(true, wrapper.IsWithInEffectiveDate(new ZDateTime(2010, 12, 10), new ZDateTime(2011, 06, 06, 23, 59, 59)));
		}

		[TestDate(2019, 10, 15)]
		public void TestNomenclatureGroups()
		{
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "TT1", "NGT", ensureDataGroupingExists: false);
			Factory.Save();
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", compositeKey: "10.20.30", ensureDataGroupingExists: false);
			var nomenclatureGroup1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1021", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 1", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "12", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 2", "10", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup3 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1023", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 3", "10.20", "NG2", ensureDataGroupingExists: false);
			var nomenclatureGroup4 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1024", new ZDateTime(2019, 10, 16), new ZDateTime(2020, 11, 16), "DESC GROUP 4", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup5 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1025", new ZDateTime(2019, 09, 14), new ZDateTime(2019, 10, 14), "DESC GROUP 5", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup6 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "1026", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 6", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroups = cusTariff.Wrapper.NomenclatureGroups;
			AssertEquals("nomenclatureGroups.Count", 2, nomenclatureGroups.Count);
			AssertCollectionContains(nomenclatureGroup1, nomenclatureGroups);
			AssertCollectionContains(nomenclatureGroup2, nomenclatureGroups);
		}

		public void TestRatesApplicabilityCountryMaxLength()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			AssertEquals(2, tariff.Wrapper.RatesApplyToCountryInfo.MaxLength);
		}

		public void TestRatesApplyToCountryList()
		{
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			AssertType<RefCountryCollection>(tariff.Wrapper.RatesApplyToCountryList);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return tariff.Wrapper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			s1p1TariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		UniversalReferenceTestDataHelper helper;
	}
}

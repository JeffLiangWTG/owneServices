using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusNomenclatureGroupCollection))]
	class RefCusNomenclatureGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusNomenclatureGroupCollection>
	{
		[TestDate(2019, 10, 15)]
		public void TestCorrectDataIsLoading()
		{
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "TT1", "NGT", ensureDataGroupingExists: false);
			Factory.Save();
			var endDate = new ZDateTime(2079, 06, 06);
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "1020304050", new ZDateTime(2010, 12, 10), endDate, "dummy Description 0", compositeKey: "10.20.30", ensureDataGroupingExists: false);
			var nomenclatureGroup1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1021", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 1", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "12", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 2", "10", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup3 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1023", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 3", "10.20", "NG2", ensureDataGroupingExists: false);
			var nomenclatureGroup4 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1024", new ZDateTime(2019, 10, 16), new ZDateTime(2020, 11, 16), "DESC GROUP 4", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup5 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1025", new ZDateTime(2019, 09, 14), new ZDateTime(2019, 10, 14), "DESC GROUP 5", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroup6 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "1026", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 6", "10.20", "NGT", ensureDataGroupingExists: false);
			var nomenclatureGroups = new RefCusNomenclatureGroupCollection(tariff);
			AssertEquals("nomenclatureGroups.CompleteFilter.IsNoResultQuery", false, nomenclatureGroups.CompleteFilter.IsNoResultQuery);
			AssertEquals("nomenclatureGroups.Count", 2, nomenclatureGroups.Count);
			AssertCollectionContains(nomenclatureGroup1, nomenclatureGroups);
			AssertCollectionContains(nomenclatureGroup2, nomenclatureGroups);
			tariff.ZZ1_EndDate = ZDate.Today.AddDays(-1);
			nomenclatureGroups = new RefCusNomenclatureGroupCollection(tariff);
			AssertEquals("nomenclatureGroups.CompleteFilter.IsNoResultQuery", true, nomenclatureGroups.CompleteFilter.IsNoResultQuery);
			AssertEquals("nomenclatureGroups.Count", 0, nomenclatureGroups.Count);
			tariff.ZZ1_EndDate = endDate;
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = ZString.Empty;
			nomenclatureGroups = new RefCusNomenclatureGroupCollection(tariff);
			AssertEquals("nomenclatureGroups.CompleteFilter.IsNoResultQuery", true, nomenclatureGroups.CompleteFilter.IsNoResultQuery);
			AssertEquals("nomenclatureGroups.Count", 0, nomenclatureGroups.Count);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "NGT";
			tariff.ZZ1_CompositeKeyOnZZ5 = "11.20.30";
			nomenclatureGroups = new RefCusNomenclatureGroupCollection(tariff);
			AssertEquals("nomenclatureGroups.CompleteFilter.IsNoResultQuery", false, nomenclatureGroups.CompleteFilter.IsNoResultQuery);
			AssertEquals("nomenclatureGroups.Count", 0, nomenclatureGroups.Count);
			tariff.ZZ1_CompositeKeyOnZZ5 = ZString.Empty;
			nomenclatureGroups = new RefCusNomenclatureGroupCollection(tariff);
			AssertEquals("nomenclatureGroups.CompleteFilter.IsNoResultQuery", true, nomenclatureGroups.CompleteFilter.IsNoResultQuery);
			AssertEquals("nomenclatureGroups.Count", 0, nomenclatureGroups.Count);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var nomenclatureGroup = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1020", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 1", "10.20", "NGT");
			return nomenclatureGroup;
		}

		protected override RefCusNomenclatureGroupCollection GetCollectionToTest()
		{
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", compositeKey: "10.20.30");
			return new RefCusNomenclatureGroupCollection(cusTariff);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			s1p1TariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "1P1", "NGT", ensureDataGroupingExists: false);
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		UniversalReferenceTestDataHelper helper;
	}
}

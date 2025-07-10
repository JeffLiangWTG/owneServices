using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(FilteredTariffUOMViewCollection))]
	sealed class FilteredTariffUOMViewCollectionTest : ActiveBusinessObjectCollectionTestCase<FilteredTariffUOMViewCollection>
	{
		public void TestEnableEffectiveData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			helper.CreateNewOrGetExistingDataGrouping(CountryCodes.SouthAfrica);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			Factory.Save();

			var tariff = helper.CreateTariff(CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var uom1 = helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM", dataGrouping: CountryCodes.Eritrea);
			var uom2 = helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.AdditionalUOMType, "LTR", dataGrouping: CountryCodes.SouthAfrica);
			var uom3 = helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "BG", dataGrouping: "DG1");
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var tariffReloaded = factory.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = new FilteredTariffUOMViewCollection(tariffReloaded, true);
			var notFilteredCollection = new TariffUOMViewCollection(tariffReloaded);
			CombineAssertions(() =>
			{
				AssertEquals("filteredCollection", 2, filteredCollection.Count);
				AssertNotNull("1 filteredCollection.uom1", filteredCollection.FindByPK(uom1.PK));
				AssertNotNull("1 filteredCollection.uom3", filteredCollection.FindByPK(uom3.PK));
				AssertEquals("1 notFilteredCollection", 3, notFilteredCollection.Count);
				AssertNotNull("1 notFilteredCollection.uom1", notFilteredCollection.FindByPK(uom1.PK));
				AssertNotNull("1 notFilteredCollection.uom2", notFilteredCollection.FindByPK(uom2.PK));
				AssertNotNull("1 notFilteredCollection.uom3", notFilteredCollection.FindByPK(uom3.PK));
				wrapper.EffectiveDataGrouping = CountryCodes.Eritrea;
				AssertEquals("2 filteredCollection", 1, filteredCollection.Count);
				AssertNotNull("2 filteredCollection.uom1", filteredCollection.FindByPK(uom1.PK));
				AssertEquals("2 notFilteredCollection", 3, notFilteredCollection.Count);
				AssertNotNull("2 notFilteredCollection.uom1", notFilteredCollection.FindByPK(uom1.PK));
				AssertNotNull("2 notFilteredCollection.uom2", notFilteredCollection.FindByPK(uom2.PK));
				AssertNotNull("2 notFilteredCollection.uom3", notFilteredCollection.FindByPK(uom3.PK));
				wrapper.EffectiveDataGrouping = "DG1";
				AssertEquals("3 filteredCollection", 1, filteredCollection.Count);
				AssertNotNull("3 filteredCollection.uom3", filteredCollection.FindByPK(uom3.PK));
				AssertEquals("3 notFilteredCollection", 3, notFilteredCollection.Count);
				AssertNotNull("3 notFilteredCollection.uom1", notFilteredCollection.FindByPK(uom1.PK));
				AssertNotNull("3 notFilteredCollection.uom2", notFilteredCollection.FindByPK(uom2.PK));
				AssertNotNull("3 notFilteredCollection.uom3", notFilteredCollection.FindByPK(uom3.PK));
			});
		}

		public void TestCollectionFilteringByRatesApplyToCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupES = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including only ES");
			helper.AddCountry(tradeGroupES, CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tradeGroupIT = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including only IT");
			helper.AddCountry(tradeGroupIT, CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "DUMMYTRF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var uomTradeGroupES = helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM", tradeGroup: tradeGroupES);
			var uomTradeGroupIT = helper.CreateTariffUOM(tariff, Constants.UnitOfMeasureTypes.AdditionalUOMType, "LTR", tradeGroup: tradeGroupIT);
			var allUOM = new[] { uomTradeGroupES, uomTradeGroupIT };
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var tariffReloaded = factory.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = new FilteredTariffUOMViewCollection(tariffReloaded, true);
			var notFilteredCollection = new TariffUOMViewCollection(tariffReloaded);

			CombineAssertions(() =>
			{
				AssertFilter("Filter inactive", ZString.Empty, allUOM);
				AssertFilter("Italy", CountryCodes.Italy, uomTradeGroupIT);
				AssertFilter("Spain", CountryCodes.Spain, uomTradeGroupES);
				AssertFilter("United States", CountryCodes.UnitedStates);
			});

			void AssertFilter(string message, ZString ratesApplyToCountry, params TariffUOMView[] expectedTradeGroups)
			{
				wrapper.RatesApplyToCountry = ratesApplyToCountry;
				AssertContainsExactElementsInAnyOrder(message + " filtered", expectedTradeGroups.Select(x => x.PK), filteredCollection.Select(x => x.PK));
				AssertContainsExactElementsInAnyOrder(message + " not filtered", allUOM.Select(x => x.PK), notFilteredCollection.Select(x => x.PK));
			}
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, ((IBindingList)collection).AllowNew);
		}

		protected override FilteredTariffUOMViewCollection GetCollectionToTest() => new FilteredTariffUOMViewCollection(Tariff, true);

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
		RefCusTariffType TariffType
		{
			get
			{
				if (tariffType == null)
				{
					tariffType = Helper.CreateNewOrGetExistingTariffType(CountryCodes.SouthAfrica, "1P1");
					Factory.Save();
				}

				return tariffType;
			}
		}

		RefCusTariffType tariffType;
		TariffView Tariff => tariff ?? (tariff = Helper.CreateTariff(CountryCodes.SouthAfrica, TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false));
		TariffView tariff;
	}
}

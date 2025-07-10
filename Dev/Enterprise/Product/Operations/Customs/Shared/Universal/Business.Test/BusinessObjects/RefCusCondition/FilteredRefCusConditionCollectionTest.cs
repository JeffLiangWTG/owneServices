using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(FilteredRefCusConditionCollection))]
	sealed class FilteredRefCusConditionCollectionTest : ActiveBusinessObjectCollectionTestCase<FilteredRefCusConditionCollection>
	{
		public void TestEnableEffectiveData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var condType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.Eritrea, "CTRL", "724");
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.Eritrea, condType.PK, tariff.PK, "Import control", true, false, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31));
			var condition2 = helper.CreateOrGetExistingRefCusCondition("DG1", condType.PK, tariff.PK, "Import control", true, false, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31));
			Factory.Save();
			var filteredCollection = new FilteredRefCusConditionCollection(tariff, true);
			var wrapper = tariff.Wrapper;
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.condition", filteredCollection.FindByPK(condition.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals("filteredCollection", 0, filteredCollection.Count);
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			wrapper.EffectiveDate = new ZDate(2013, 1, 1);
			AssertEquals("filteredCollection", 0, filteredCollection.Count);
			filteredCollection = new FilteredRefCusConditionCollection(tariff, false);
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.condition", filteredCollection.FindByPK(condition.PK));
			AssertNotNull("filteredCollection.condition2", filteredCollection.FindByPK(condition2.PK));
		}

		public void TestCollectionFilteringByRatesApplyToCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: eun);
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "All", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including IT and ES");
			helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tradeGroupIT = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including only IT");
			helper.AddCountry(tradeGroupIT, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "DUMMYTRF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CTRL", "724");
			var condition1 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, condType.PK, tariff.PK, "Import control", true, false, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31));
			var condition2 = helper.CreateOrGetExistingRefCusCondition(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, condType.PK, tariff.PK, "Import rate", false, true, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31));
			helper.CreateCusApplicabilityInternal(condition1, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicabilityInternal(condition2, tradeGroupIT, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariffReloaded = Factory.Load<TariffView>(tariff.PK);
			var wrapper = tariff.Wrapper;
			wrapper.EffectiveDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			var filteredRateCollection = new FilteredRefCusConditionCollection(tariffReloaded, true);
			CombineAssertions("RatesApplyToCountry filter", () =>
			{
				tariffReloaded.Wrapper.RatesApplyToCountry = ZString.Empty;
				AssertContainsExactElementsInAnyOrder("RatesApplyToCountry ALL", new[] { condition1.PK, condition2.PK }, filteredRateCollection.Select(x => x.PK));

				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Italy;
				AssertContainsExactElementsInAnyOrder("RatesApplyToCountry IT", new[] { condition1.PK, condition2.PK }, filteredRateCollection.Select(x => x.PK));

				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Spain;
				AssertContainsExactElementsInAnyOrder("RatesApplyToCountry ES", new[] { condition1.PK }, filteredRateCollection.Select(x => x.PK));

				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("RatesApplyToCountry = US", 0, filteredRateCollection.Count);

				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Italy;
				AssertContainsExactElementsInAnyOrder("Setting again RatesApplyToCountry IT", new[] { condition1.PK, condition2.PK }, filteredRateCollection.Select(x => x.PK));
			});
		}

		protected override FilteredRefCusConditionCollection GetCollectionToTest()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_StartDate = ZDateTime.Empty;
			tariff.ZZ1_EndDate = ZDateTime.Empty;
			return new FilteredRefCusConditionCollection(tariff, true);
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(FilteredCusRefApplicabilityViewCollection))]
	public class FilteredRefCusApplicabilityCollection_Test : ActiveBusinessObjectCollectionTestCase<FilteredCusRefApplicabilityViewCollection>
	{
		public void TestEnableEffectiveData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var zaTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "ZATRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			var erTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Eritrea, "ERTRADE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			var tg12 = helper.CreateTradeGroup("DG1", "TG12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, ensureDataGroupingExists: false);
			var rate = helper.CreateRate(tariff, rateCode.PK, new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "0");
			var applic1 = helper.CreateCusApplicabilityInternal(rate, erTradeGroup, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31), "DJC", "O");
			var applic2 = helper.CreateCusApplicabilityInternal(rate, zaTradeGroup, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06), "LSC", "P");
			var applic3 = helper.CreateCusApplicabilityInternal(rate, tg12, new ZDateTime(2012, 12, 10), new ZDateTime(2079, 06, 06), "CA1", "G");
			Factory.Save();
			var f = new BusinessObjectFactory();
			var rateReloaded = f.Load<RateView>(rate.PK);
			var wrapper = rateReloaded.CusTariff.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = new FilteredCusRefApplicabilityViewCollection(rateReloaded, true);
			var collection = new CusRefApplicabilityViewCollection(rateReloaded);
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic1", filteredCollection.FindByPK(applic1.PK));
			AssertNotNull("filteredCollection.applic3", filteredCollection.FindByPK(applic3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic1", filteredCollection.FindByPK(applic1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic3", filteredCollection.FindByPK(applic3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic1", filteredCollection.FindByPK(applic1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			wrapper.EffectiveDate = new ZDate(2013, 1, 1);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic3", filteredCollection.FindByPK(applic3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.applic1", collection.FindByPK(applic1.PK));
			AssertNotNull("collection.applic2", collection.FindByPK(applic2.PK));
			AssertNotNull("collection.applic3", collection.FindByPK(applic3.PK));
			var condType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.Eritrea, "CTRL", "724");
			condType.Factory.Save();
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.Eritrea, condType.PK, tariff.PK, "Import control", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var applic4 = helper.CreateCusApplicabilityInternal(condition, erTradeGroup, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31), "DJC", "O");
			Factory.Save();
			filteredCollection = new FilteredCusRefApplicabilityViewCollection(condition, true);
			wrapper = condition.CusTariff.Wrapper;
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			wrapper.EffectiveDate = new ZDate(2012, 12, 9);
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.applic4", filteredCollection.FindByPK(applic4.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 0, filteredCollection.Count);
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			wrapper.EffectiveDate = new ZDate(2013, 1, 1);
			AssertEquals("filteredCollection", 0, filteredCollection.Count);
		}

		public void TestCollectionFilteringByRatesApplyToCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupES = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including only ES");
			var tradeGroupCountryES = helper.AddCountry(tradeGroupES, Core.Constants.CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tradeGroupIT = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including only IT");
			var tradeGroupCountryIT = helper.AddCountry(tradeGroupIT, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.CreateCusRateCode(Factory, "A00", rateType.PK);
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "DUMMYTRF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate1 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var rate1ApplicabilityTradeGroupES = helper.CreateCusApplicabilityInternal(rate1, tradeGroupES, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate1ApplicabilityTradeGroupIT = helper.CreateCusApplicabilityInternal(rate1, tradeGroupIT, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var rate1Reloaded = newFactory.Load<RateView>(rate1.PK);
			var filteredCusApplicabilityCollection = new FilteredCusRefApplicabilityViewCollection(rate1Reloaded, true);
			CombineAssertions("RatesApplyToCountry filter inactive", () =>
			{
				rate1Reloaded.CusTariff.Wrapper.RatesApplyToCountry = ZString.Empty;
				AssertEquals("Expected count", 2, filteredCusApplicabilityCollection.Count);
				AssertNotNull($"{nameof(rate1ApplicabilityTradeGroupES)} not null", filteredCusApplicabilityCollection.FindByPK(rate1ApplicabilityTradeGroupES.PK));
				AssertNotNull($"{nameof(rate1ApplicabilityTradeGroupIT)} not null", filteredCusApplicabilityCollection.FindByPK(rate1ApplicabilityTradeGroupIT.PK));
			}

			);
			CombineAssertions("RatesApplyToCountry = IT", () =>
			{
				rate1Reloaded.CusTariff.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Italy;
				AssertEquals("Expected count", 1, filteredCusApplicabilityCollection.Count);
				AssertNotNull($"{nameof(rate1ApplicabilityTradeGroupIT)} not null", filteredCusApplicabilityCollection.FindByPK(rate1ApplicabilityTradeGroupIT.PK));
			}

			);
			CombineAssertions("RatesApplyToCountry = ES", () =>
			{
				rate1Reloaded.CusTariff.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Spain;
				AssertEquals("Expected count", 1, filteredCusApplicabilityCollection.Count);
				AssertNotNull($"{nameof(rate1ApplicabilityTradeGroupES)} not null", filteredCusApplicabilityCollection.FindByPK(rate1ApplicabilityTradeGroupES.PK));
			}

			);
			CombineAssertions("RatesApplyToCountry = US", () =>
			{
				rate1Reloaded.CusTariff.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("Expected count", 0, filteredCusApplicabilityCollection.Count);
			}

			);
		}

		public void TestSetDefaultsForNewElement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "ADD");
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2021, 01, 01), new ZDateTime(2022, 01, 01));
			Factory.Save();
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RATE0", rateType.PK, isSystem: true);
			Factory.Save();
			var rateView = helper.CreateRate(cusTariff, rateCode.PK, new ZDateTime(2021, 02, 23), new ZDateTime(2022, 02, 23), "DUMMYFORMULA", dataGrouping: Core.Constants.CountryCodes.Eritrea, isSystem: true);
			var collection = new FilteredCusRefApplicabilityViewCollection(rateView, true);
			var applicability = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("ZZT_StartDate", new ZDateTime(2021, 02, 23), applicability.ZZT_StartDate);
				AssertEquals("ZZT_EndDate", new ZDateTime(2022, 02, 23), applicability.ZZT_EndDate);
			}

			);
		}

		protected override FilteredCusRefApplicabilityViewCollection GetCollectionToTest()
		{
			return new FilteredCusRefApplicabilityViewCollection(Factory.New<RateView>(), true);
		}
	}
}

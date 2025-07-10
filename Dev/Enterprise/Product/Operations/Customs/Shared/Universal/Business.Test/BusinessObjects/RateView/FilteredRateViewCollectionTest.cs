using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(FilteredRateViewCollection))]
	class FilteredRateViewCollectionTest : ActiveBusinessObjectCollectionTestCase<FilteredRateViewCollection>
	{
		public void TestEnableEffectiveData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var dg1 = helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			var za = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), ZDateTime.MaxSmallDateTimeValue, "dummy Description 0", ensureDataGroupingExists: false);
			var rate1 = helper.CreateRate(tariff, rateCode.PK, new ZDateTime(2011, 1, 1), new ZDateTime(2012, 12, 31), "0", dataGrouping: Core.Constants.CountryCodes.Eritrea);
			helper.CreateCusApplicability(rate1, null, new ZDateTime(2011, 1, 1), ZDateTime.MaxSmallDateTimeValue);
			var rate2 = helper.CreateRate(tariff, rateCode.PK, new ZDateTime(2012, 12, 10), ZDateTime.MaxSmallDateTimeValue, "0", dataGrouping: Core.Constants.CountryCodes.SouthAfrica);
			var rate3 = helper.CreateRate(tariff, rateCode.PK, new ZDateTime(2012, 12, 10), ZDateTime.MaxSmallDateTimeValue, "0", dataGrouping: "DG1");
			var rate4 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", dataGrouping: Core.Constants.CountryCodes.Eritrea);
			helper.CreateCusApplicability(rate4, null, new ZDateTime(2013, 1, 1), ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			var filteredRates = new FilteredRateViewCollection(tariffReloaded, true);
			var rates = new RateViewCollection(tariffReloaded);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(rates.GetPKs(), [rate1.PK, rate2.PK, rate3.PK, rate4.PK]);

				wrapper.EffectiveDataGrouping = ZString.Empty;
				wrapper.EffectiveDate = ZDate.Empty;
				AssertContainsExactElementsInAnyOrder("Empty filter", filteredRates.GetPKs(), [rate1.PK, rate3.PK, rate4.PK]);

				wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
				AssertContainsExactElementsInAnyOrder("DataGrouping = 'ER'", filteredRates.GetPKs(), [rate1.PK, rate4.PK]);

				wrapper.EffectiveDataGrouping = "DG1";
				AssertContainsExactElementsInAnyOrder("DataGrouping = 'DG1'", filteredRates.GetPKs(), [rate3.PK]);

				wrapper.EffectiveDataGrouping = ZString.Empty;
				wrapper.EffectiveDate = new ZDate(2013, 1, 1);
				AssertContainsExactElementsInAnyOrder("Date = '2013-01-01'", filteredRates.GetPKs(), [rate3.PK, rate4.PK]);

				wrapper.EffectiveDate = new ZDate(2012, 12, 9);
				AssertContainsExactElementsInAnyOrder("Date = '2012-12-09'", filteredRates.GetPKs(), [rate1.PK]);
			});
		}

		public void TestCollectionFilteringByRatesApplyToCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroupAll = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "All", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including IT and ES");
			var tradeGroupAllCountryIT = helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tradeGroupAllCountryES = helper.AddCountry(tradeGroupAll, Core.Constants.CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tradeGroupIT = helper.CreateTradeGroup(Core.Constants.CountryCodes.Italy, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, description: "Trade Group including only IT");
			var tradeGroupCountryIT = helper.AddCountry(tradeGroupIT, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Constants.RateTypes.Duty, "Duty");
			var rateCode = helper.CreateCusRateCode(Factory, "A00", rateType.PK);
			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "DUMMYTRF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate1 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var rate1ApplicabilityTradeGroupAll = helper.CreateCusApplicabilityInternal(rate1, tradeGroupAll, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate2 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var rate2ApplicabilityTradeGroupIT = helper.CreateCusApplicabilityInternal(rate2, tradeGroupIT, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate3 = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0", dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var tariffReloaded = newFactory.Load<TariffView>(tariff.PK);
			var filteredRateCollection = new FilteredRateViewCollection(tariffReloaded, true);
			CombineAssertions("RatesApplyToCountry filter inactive", () =>
			{
				tariffReloaded.Wrapper.RatesApplyToCountry = ZString.Empty;
				AssertEquals("Expected count", 3, filteredRateCollection.Count);
				AssertNotNull($"{nameof(rate1)} not null", filteredRateCollection.FindByPK(rate1.PK));
				AssertNotNull($"{nameof(rate2)} not null", filteredRateCollection.FindByPK(rate2.PK));
				AssertNotNull($"{nameof(rate3)} not null", filteredRateCollection.FindByPK(rate3.PK));
			});
			CombineAssertions("RatesApplyToCountry = IT", () =>
			{
				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Italy;
				AssertEquals("Expected count", 2, filteredRateCollection.Count);
				AssertNotNull($"{nameof(rate1)} not null", filteredRateCollection.FindByPK(rate1.PK));
				AssertNotNull($"{nameof(rate2)} not null", filteredRateCollection.FindByPK(rate2.PK));
			});
			CombineAssertions("RatesApplyToCountry = ES", () =>
			{
				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Spain;
				AssertEquals("Expected count", 1, filteredRateCollection.Count);
				AssertNotNull($"{nameof(rate2)} not null", filteredRateCollection.FindByPK(rate1.PK));
			});
			CombineAssertions("RatesApplyToCountry = US", () =>
			{
				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("Expected count", 0, filteredRateCollection.Count);
			});
			CombineAssertions("Setting again RatesApplyToCountry = IT", () =>
			{
				tariffReloaded.Wrapper.RatesApplyToCountry = Core.Constants.CountryCodes.Italy;
				AssertEquals("Expected count", 2, filteredRateCollection.Count);
				AssertNotNull($"{nameof(rate1)} not null", filteredRateCollection.FindByPK(rate1.PK));
				AssertNotNull($"{nameof(rate2)} not null", filteredRateCollection.FindByPK(rate2.PK));
			});
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, ((IBindingList)collection).AllowNew);
		}

		public void TestSetDefaultsForNewElement()
		{
			var collection = GetCollectionToTest();
			var rate = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("ZZ2_ZZ1_ParentTariffOrNationalCode", Tariff.PK, rate.ZZ2_ZZ1_ParentTariffOrNationalCode);
				AssertEquals("ZZ2_StartDate", new ZDateTime(2010, 12, 10), rate.ZZ2_StartDate);
				AssertEquals("ZZ2_EndDate", ZDateTime.MaxSmallDateTimeValue, rate.ZZ2_EndDate);
			});
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var rate = Tariff.FilteredRates.AddNew();
			rate.ZZ2_ZZ1_ParentTariffOrNationalCode = Tariff.PK;
			rate.ZZ2_StartDate = new ZDateTime(2010, 12, 10);
			rate.ZZ2_EndDate = ZDateTime.MaxSmallDateTimeValue;
			rate.ZZ2_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			return rate;
		}

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;

		RefCusTariffType TariffType
		{
			get
			{
				if (tariffType == null)
				{
					tariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
					Factory.Save();
				}
				return tariffType;
			}
		}
		RefCusTariffType tariffType;

		TariffView Tariff => tariff ??= Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), ZDateTime.MaxSmallDateTimeValue, "dummy Description 0", ensureDataGroupingExists: false);
		TariffView tariff;

		protected override FilteredRateViewCollection GetCollectionToTest()
		{
			return new FilteredRateViewCollection(Tariff, true);
		}
	}
}

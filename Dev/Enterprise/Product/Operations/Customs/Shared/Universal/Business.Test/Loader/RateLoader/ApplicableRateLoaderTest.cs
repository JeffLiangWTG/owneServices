using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	class ApplicableRateLoaderTest : BaseApplicableRateLoaderAbstractTest
	{
		public void TestLoadRatesForSingleCriteriaSet()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType1 = helper.CreateCusRateType(EunGroupCode, "RT1");
			var rateType2 = helper.CreateCusRateType(EunGroupCode, "RT2");
			CreateRate(tariff, rateType1, "RC111");
			CreateRate(tariff, rateType1, "RC112");
			CreateRate(tariff, rateType2, "RC121");
			Factory.Save();

			var selectionCriteria = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var tariffCriteriaSet = new RateLoadTariffCriteriaSet(tariff, selectionCriteria);
			AssertEquals("[PRE-CONDITION] Is criteria set cached before 1st loading?", false, tariffCriteriaSet.IsCached());

			var rateLoader = new ApplicableRateLoader(Factory);
			var rates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet);

			CombineAssertions("[After Loading]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariffCriteriaSet, true);
				AssertEquals("Loaded rate count", 2, rates.Count());
				AssertLoadedRateCount(rates, "RC111", 1); // Matches a loading criteria
				AssertLoadedRateCount(rates, "RC112", 1); // Matches a loading criteria
				AssertLoadedRateCount(rates, "RC121", 0); // Does not match any loading criteria
			});
		}

		public void TestLoadRatesForSingleCriteriaSet_WithSecondTradeGroup()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var secondTradeGroup = helper.CreateTradeGroup(EunGroupCode, "SecondTG", startDate, endDate);
			var secondTradeGroup2 = helper.CreateTradeGroup(EunGroupCode, "SecondTG2", startDate, endDate);
			var rateType1 = helper.CreateCusRateType(EunGroupCode, "RT1");
			CreateRate(tariff, rateType1, "RC111");
			CreateRate(tariff, rateType1, "RC112", secondTradeGroup: secondTradeGroup);
			CreateRate(tariff, rateType1, "RC113", secondTradeGroup: secondTradeGroup2);
			Factory.Save();

			var selectionCriteria = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "", new HashSet<ZString> { "SecondTG", "SecondTG" });
			var tariffCriteriaSet = new RateLoadTariffCriteriaSet(tariff, selectionCriteria);

			var rateLoader = new ApplicableRateLoader(Factory);
			var rates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet);

			CombineAssertions("[After Loading]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariffCriteriaSet, true);
				AssertEquals("Loaded rate count", 2, rates.Count());
				AssertLoadedRateCount(rates, "RC111", 1); // ZZT_ZZA_SecondTradeGroup is null and Matches a loading criteria
				AssertLoadedRateCount(rates, "RC112", 1); // ZZT_ZZA_SecondTradeGroup is not null and secondTradeGroup Matched
				AssertLoadedRateCount(rates, "RC113", 0); // ZZT_ZZA_SecondTradeGroup is not null and secondTradeGroup not Matched
			});
		}

		public void TestLoadRatesForMultipleCriteriaSets()
		{
			var tariff1 = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var tariff2 = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC2", startDate, endDate);
			var rateType1 = helper.CreateCusRateType(EunGroupCode, "RT1");
			var rateType2 = helper.CreateCusRateType(EunGroupCode, "RT2");
			var rateType3 = helper.CreateCusRateType(EunGroupCode, "RT3");
			CreateRate(tariff1, rateType1, "RC111");
			CreateRate(tariff1, rateType1, "RC112");
			CreateRate(tariff1, rateType2, "RC121");
			CreateRate(tariff1, rateType3, "RC131");
			CreateRate(tariff1, rateType3, "RC132");
			CreateRate(tariff2, rateType1, "RC211");
			CreateRate(tariff2, rateType2, "RC221");
			CreateRate(tariff2, rateType2, "RC222");
			CreateRate(tariff2, rateType2, "RC223");
			CreateRate(tariff2, rateType3, "RC231");
			CreateRate(tariff2, rateType3, "RC232");
			Factory.Save();

			var selectionCriteriaAnyRate = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "");
			var selectionCriteriaRateType1 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var selectionCriteriaRateType2 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT2", "");
			var selectionCriteriaRateCode231 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "RC231");
			var tariff1CriteriaSetRateType1 = new RateLoadTariffCriteriaSet(tariff1, selectionCriteriaRateType1);
			var tariff1CriteriaSetRateType2 = new RateLoadTariffCriteriaSet(tariff1, selectionCriteriaRateType2);
			var tariff2CriteriaSetRateType1 = new RateLoadTariffCriteriaSet(tariff2, selectionCriteriaRateType1);
			var tariff2CriteriaSetRateType2 = new RateLoadTariffCriteriaSet(tariff2, selectionCriteriaRateType2);
			var tariff2CriteriaSetRateCode231 = new RateLoadTariffCriteriaSet(tariff2, selectionCriteriaRateCode231);
			var tariff1CriteriaSetAnyRate = new RateLoadTariffCriteriaSet(tariff1, selectionCriteriaAnyRate);
			var tariff2CriteriaSetAnyRate = new RateLoadTariffCriteriaSet(tariff2, selectionCriteriaAnyRate);
			CombineAssertions("[PRE-CONDITION] Before Loading", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateCode231", tariff2CriteriaSetRateCode231, false);
				AssertCriteriaSetCached("Tariff1 + Any Rate", tariff1CriteriaSetAnyRate, false);
				AssertCriteriaSetCached("Tariff2 + Any Rate", tariff2CriteriaSetAnyRate, false);
			});

			var rateLoader = new ApplicableRateLoader(Factory);
			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariff1CriteriaSetAnyRate, tariff2CriteriaSetAnyRate, });
			CombineAssertions("[After 1st Load]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateCode231", tariff2CriteriaSetRateCode231, false);
				AssertCriteriaSetCached("Tariff1 + Any Rate", tariff1CriteriaSetAnyRate, true);
				AssertCriteriaSetCached("Tariff2 + Any Rate", tariff2CriteriaSetAnyRate, true);
				AssertEquals("Loaded rate count", 11, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1);
				AssertLoadedRateCount(loadedRates, "RC112", 1);
				AssertLoadedRateCount(loadedRates, "RC121", 1);
				AssertLoadedRateCount(loadedRates, "RC131", 1);
				AssertLoadedRateCount(loadedRates, "RC132", 1);
				AssertLoadedRateCount(loadedRates, "RC211", 1);
				AssertLoadedRateCount(loadedRates, "RC221", 1);
				AssertLoadedRateCount(loadedRates, "RC222", 1);
				AssertLoadedRateCount(loadedRates, "RC223", 1);
				AssertLoadedRateCount(loadedRates, "RC231", 1);
				AssertLoadedRateCount(loadedRates, "RC232", 1);
			});

			loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariff2CriteriaSetRateType1, tariff2CriteriaSetRateType2, tariff2CriteriaSetRateCode231, });
			CombineAssertions("[After 2nd Load]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateCode231", tariff2CriteriaSetRateCode231, true);
				AssertCriteriaSetCached("Tariff1 + Any Rate", tariff1CriteriaSetAnyRate, true);
				AssertCriteriaSetCached("Tariff2 + Any Rate", tariff2CriteriaSetAnyRate, true);
				AssertEquals("Loaded rate count", 5, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC112", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC121", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC131", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC132", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC211", 1);
				AssertLoadedRateCount(loadedRates, "RC221", 1);
				AssertLoadedRateCount(loadedRates, "RC222", 1);
				AssertLoadedRateCount(loadedRates, "RC223", 1);
				AssertLoadedRateCount(loadedRates, "RC231", 1);
				AssertLoadedRateCount(loadedRates, "RC232", 0); // Does not match any loading criteria
			});

			// Add new rates to test caching
			CreateRate(tariff1, rateType1, "RC113");
			CreateRate(tariff1, rateType2, "RC122");
			CreateRate(tariff2, rateType2, "RC224");
			Factory.Save();

			loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariff1CriteriaSetRateType1, // this criteria should load all applicable rates as it wasn't cached yet
 tariff1CriteriaSetRateType2, // this criteria should load all applicable rates as it wasn't cached yet
 tariff2CriteriaSetRateType2, // this criteria should not load new applicable rates as it's already cached
 });

			CombineAssertions("[After 3rd Load]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateCode231", tariff2CriteriaSetRateCode231, true);
				AssertCriteriaSetCached("Tariff1 + Any Rate", tariff1CriteriaSetAnyRate, true);
				AssertCriteriaSetCached("Tariff2 + Any Rate", tariff2CriteriaSetAnyRate, true);
				AssertEquals("Loaded rate count", 8, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1); // Matches a loading criteria
				AssertLoadedRateCount(loadedRates, "RC112", 1); // Matches a loading criteria
				AssertLoadedRateCount(loadedRates, "RC113", 1); // New rate, matches a non previously cached criteria
				AssertLoadedRateCount(loadedRates, "RC121", 1); // Matches a loading criteria
				AssertLoadedRateCount(loadedRates, "RC122", 1); // New rate, matches a non previously cached criteria
				AssertLoadedRateCount(loadedRates, "RC131", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC132", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC211", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC221", 1); // Already cached for a loading criteria
				AssertLoadedRateCount(loadedRates, "RC222", 1); // Already cached for a loading criteria
				AssertLoadedRateCount(loadedRates, "RC223", 1); // Already cached for a loading criteria
				AssertLoadedRateCount(loadedRates, "RC224", 0); // New rate, matches a previously cached criteria
				AssertLoadedRateCount(loadedRates, "RC231", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC232", 0); // Does not match any loading criteria
			});
		}

		public void TestCacheRatesForMultipleCriteriaSets()
		{
			var tariff1 = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var tariff2 = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC2", startDate, endDate);
			var rateType1 = helper.CreateCusRateType(EunGroupCode, "RT1");
			var rateType2 = helper.CreateCusRateType(EunGroupCode, "RT2");
			CreateRate(tariff1, rateType1, "RC111");
			CreateRate(tariff1, rateType1, "RC112");
			CreateRate(tariff1, rateType2, "RC121");
			CreateRate(tariff2, rateType2, "RC221");
			CreateRate(tariff2, rateType2, "RC222");
			CreateRate(tariff2, rateType2, "RC223");
			Factory.Save();

			var selectionCriteriaRateType1 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");
			var selectionCriteriaRateType2 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT2", "");
			var tariff1CriteriaSetRateType1 = new RateLoadTariffCriteriaSet(tariff1, selectionCriteriaRateType1);
			var tariff1CriteriaSetRateType2 = new RateLoadTariffCriteriaSet(tariff1, selectionCriteriaRateType2);
			var tariff2CriteriaSetRateType1 = new RateLoadTariffCriteriaSet(tariff2, selectionCriteriaRateType1);
			var tariff2CriteriaSetRateType2 = new RateLoadTariffCriteriaSet(tariff2, selectionCriteriaRateType2);

			CombineAssertions("[PRE-CONDITION] Before Caching", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType2, false);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, false);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType2, false);
			});

			var rateLoader = new ApplicableRateLoader(Factory);
			rateLoader.CacheRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariff1CriteriaSetRateType1, tariff2CriteriaSetRateType1, });
			CombineAssertions("[After 1st Caching]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType2, false);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType2, false);
			});

			rateLoader.CacheRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariff1CriteriaSetRateType1, tariff1CriteriaSetRateType2, });
			CombineAssertions("[After 2nd Caching]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType2, true);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType2, false);
			});

			// Add new rates to test caching
			CreateRate(tariff1, rateType2, "RC122");
			CreateRate(tariff1, rateType2, "RC123");
			CreateRate(tariff2, rateType2, "RC224");
			Factory.Save();

			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] {
							tariff1CriteriaSetRateType2, // this criteria should not load new applicable rates as it's already cached
							tariff2CriteriaSetRateType2, // this criteria should load all applicable rates as it wasn't cached yet
							});

			CombineAssertions("[After Loading]", () =>
			{
				AssertCriteriaSetCached("Tariff1 + RateType1", tariff1CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff1 + RateType2", tariff1CriteriaSetRateType2, true);
				AssertCriteriaSetCached("Tariff2 + RateType1", tariff2CriteriaSetRateType1, true);
				AssertCriteriaSetCached("Tariff2 + RateType2", tariff2CriteriaSetRateType2, true);
				AssertEquals("Loaded rate count", 5, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC112", 0); // Does not match any loading criteria
				AssertLoadedRateCount(loadedRates, "RC121", 1); // Already cached for a loading criteria
				AssertLoadedRateCount(loadedRates, "RC122", 0); // New rate, matches a previously cached criteria
				AssertLoadedRateCount(loadedRates, "RC123", 0); // New rate, matches a previously cached criteria
				AssertLoadedRateCount(loadedRates, "RC221", 1); // Matches a loading criteria
				AssertLoadedRateCount(loadedRates, "RC222", 1); // Matches a loading criteria
				AssertLoadedRateCount(loadedRates, "RC223", 1); // Matches a loading criteria
				AssertLoadedRateCount(loadedRates, "RC224", 1); // New rate, matches a non previously cached criteria
			});
		}

		public void TestCachedRatesMatchingMultipleCriteriaAreLoadedOnlyOnce()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType = helper.CreateCusRateType(EunGroupCode, "RT1");
			CreateRate(tariff, rateType, "RC111");
			CreateRate(tariff, rateType, "RC112");
			Factory.Save();
			var tariffCriteriaSetAnyRate = new RateLoadTariffCriteriaSet(tariff, new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", ""));
			var tariffCriteriaSetRateCode111 = new RateLoadTariffCriteriaSet(tariff, new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "", "RC111"));

			// Crieria Sets not yet cached => Loading from database and caching results.
			var rateLoader = new ApplicableRateLoader(Factory);
			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetAnyRate, tariffCriteriaSetRateCode111, });
			CombineAssertions("[1st Load Results]", () =>
			{
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1); // Matches both criteria sets, but loaded only once
				AssertLoadedRateCount(loadedRates, "RC112", 1); // Matches one criteria set only
			});

			// Crieria Sets cached => Loading from cache.
			loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSetAnyRate, tariffCriteriaSetRateCode111, });
			CombineAssertions("[2nd Load Results]", () =>
			{
				AssertEquals("Loaded rate count", 2, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "RC111", 1); // Matches both criteria sets, but loaded only once
				AssertLoadedRateCount(loadedRates, "RC112", 1); // Matches one criteria set only
			});
		}

		public void TestLoadRatesForMultipleCriteriaSets_WithCountryOfOriginPreferenceOrderAndAdditionalCodes()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var rateType = helper.CreateCusRateType(EunGroupCode, "RT1");
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("PP1", "PREF1", EunGroupCode, "ZA");
			var preference2 = helper.CreatePreferenceForCountryAndGrouping("PP2", "PREF2", EunGroupCode, "ZA");
			helper.AddCountry(stdTradeGroup, "ZA");
			Factory.Save();
			CreateRate(tariff, rateType, "R111", preference1.PK, "ORD1", "ADD1");
			CreateRate(tariff, rateType, "R112", preference1.PK, "ORD1", "ADD2");
			CreateRate(tariff, rateType, "R121", preference1.PK, "ORD2", "ADD1");
			CreateRate(tariff, rateType, "R212", preference2.PK, "ORD1", "ADD2");
			CreateRate(tariff, rateType, "R221", preference2.PK, "ORD2", "ADD1");
			CreateRate(tariff, rateType, "R222", preference2.PK, "ORD2", "ADD2");
			Factory.Save();

			var selectionCriteria111 = new SpecificRateSelectionCriteria("ZA", EunGroupCode, "PP1", "ORD1", new HashSet<ZString>()
			{ "ADD1" }, testDate, "RT1", "");
			var selectionCriteria11X = new SpecificRateSelectionCriteria("ZA", EunGroupCode, "PP1", "ORD1", new HashSet<ZString>()
			{ "ADD1", "ADD2" }, testDate, "RT1", "");
			var selectionCriteria22X = new SpecificRateSelectionCriteria("ZA", EunGroupCode, "PP2", "ORD2", new HashSet<ZString>()
			{ "ADD1", "ADD2" }, testDate, "RT1", "");
			var tariffCriteriaSet111 = new RateLoadTariffCriteriaSet(tariff, selectionCriteria111);
			var tariffCriteriaSet11X = new RateLoadTariffCriteriaSet(tariff, selectionCriteria11X);
			var tariffCriteriaSet22X = new RateLoadTariffCriteriaSet(tariff, selectionCriteria22X);

			var rateLoader = new ApplicableRateLoader(Factory);
			var loadedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSet111, tariffCriteriaSet11X, tariffCriteriaSet22X, });

			CombineAssertions("[1st Loading Results]", () =>
			{
				AssertEquals("Loaded rate count", 4, loadedRates.Count());
				AssertLoadedRateCount(loadedRates, "R111", 1);
				AssertLoadedRateCount(loadedRates, "R112", 1);
				AssertLoadedRateCount(loadedRates, "R121", 0);
				AssertLoadedRateCount(loadedRates, "R212", 0);
				AssertLoadedRateCount(loadedRates, "R221", 1);
				AssertLoadedRateCount(loadedRates, "R222", 1);
			});

			var criteria111Rates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet111);
			CombineAssertions("Criteria [Preference: PP1, Order: ORD1, AdditionalCodes: (ADD1)]", () =>
			{
				AssertEquals("Loaded rate count", 1, criteria111Rates.Count());
				AssertLoadedRateCount(criteria111Rates, "R111", 1);
			});

			var criteria11XRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet11X);
			CombineAssertions("Criteria [Preference: PP1, Order: ORD1, AdditionalCodes: (ADD1, ADD2)]", () =>
			{
				AssertEquals("Loaded rate count", 2, criteria11XRates.Count());
				AssertLoadedRateCount(criteria11XRates, "R111", 1);
				AssertLoadedRateCount(criteria11XRates, "R112", 1);
			});

			var criteria22XRates = rateLoader.LoadRatesForSingleCriteriaSet(tariffCriteriaSet22X);
			CombineAssertions("Criteria [Preference: PP2, Order: ORD2, AdditionalCodes: (ADD1, ADD2)]", () =>
			{
				AssertEquals("Loaded rate count", 2, criteria22XRates.Count());
				AssertLoadedRateCount(criteria22XRates, "R221", 1);
				AssertLoadedRateCount(criteria22XRates, "R222", 1);
			});

			CombineAssertions("[Confirm Criteria Sets Are Cached]", () =>
			{
				AssertCriteriaSetCached("Preference: PP1, Order: ORD1, AdditionalCodes: (ADD1)", tariffCriteriaSet111, true);
				AssertCriteriaSetCached("Preference: PP1, Order: ORD1, AdditionalCodes: (ADD1, ADD2)", tariffCriteriaSet11X, true);
				AssertCriteriaSetCached("Preference: PP2, Order: ORD2, AdditionalCodes: (ADD1, ADD2)", tariffCriteriaSet22X, true);
			});

			var cachedRates = rateLoader.LoadRatesForMultipleCriteriaSets(new RateLoadTariffCriteriaSet[] { tariffCriteriaSet111, tariffCriteriaSet11X, tariffCriteriaSet22X, });
			CombineAssertions("[Result From Loading Cached Rates]", () =>
			{
				AssertEquals("Loaded rate count", 4, cachedRates.Count());
				AssertLoadedRateCount(cachedRates, "R111", 1);
				AssertLoadedRateCount(cachedRates, "R112", 1);
				AssertLoadedRateCount(cachedRates, "R121", 0);
				AssertLoadedRateCount(cachedRates, "R212", 0);
				AssertLoadedRateCount(cachedRates, "R221", 1);
				AssertLoadedRateCount(cachedRates, "R222", 1);
			});
		}

		public void TestLoadRatesForMultipleCriteriaSets_WithMultiSecondTradeGroup()
		{
			var tariff = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC1", startDate, endDate);
			var tariff2 = helper.CreateTariff(EunGroupCode, tariffType.PK, "TC2", startDate, endDate);
			var secondTradeGroup = helper.CreateTradeGroup(EunGroupCode, "SecondTG", startDate, endDate);
			var secondTradeGroup2 = helper.CreateTradeGroup(EunGroupCode, "SecondTG2", startDate, endDate);
			var rateType1 = helper.CreateCusRateType(EunGroupCode, "RT1");
			CreateRate(tariff, rateType1, "RC111");
			CreateRate(tariff2, rateType1, "RC112", secondTradeGroup: secondTradeGroup);
			CreateRate(tariff2, rateType1, "RC113", secondTradeGroup: secondTradeGroup2);
			Factory.Save();

			var selectionCriteria1 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "", new HashSet<ZString> { "SecondTG" });
			var selectionCriteria2 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "", new HashSet<ZString> { "SecondTG2" });
			var selectionCriteria3 = new SpecificRateSelectionCriteria("", EunGroupCode, "", "", new HashSet<ZString>(), testDate, "RT1", "");

			var tariffCriteriaSet1 = new RateLoadTariffCriteriaSet(tariff2, selectionCriteria1);
			var tariffCriteriaSet2 = new RateLoadTariffCriteriaSet(tariff2, selectionCriteria2);
			var tariffCriteriaSet3 = new RateLoadTariffCriteriaSet(tariff, selectionCriteria3);

			var rateLoader = new ApplicableRateLoader(Factory);
			var rates = rateLoader.LoadRatesForMultipleCriteriaSets(new[] { tariffCriteriaSet1, tariffCriteriaSet2, tariffCriteriaSet3 });

			CombineAssertions("[After Loading]", () =>
			{
				AssertEquals("Loaded rate count", 3, rates.Count());
				AssertLoadedRateCount(rates, "RC111", 1); // ZZT_ZZA_SecondTradeGroup is not null and Matches a loading criteria - tariffCriteriaSet1
				AssertLoadedRateCount(rates, "RC112", 1); // ZZT_ZZA_SecondTradeGroup is not null and Matches a loading criteria = tariffCriteriaSet2
				AssertLoadedRateCount(rates, "RC113", 1); // ZZT_ZZA_SecondTradeGroup is null and Match a loading criteria - tariffCriteriaSet3
			});
		}
	}
}

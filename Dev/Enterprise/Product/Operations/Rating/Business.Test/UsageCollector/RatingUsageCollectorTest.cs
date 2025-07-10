using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Registry.Business.RatingFeatureHelper.CarrierConnect;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingUsageCollectorTest : RatingTestCase
	{
		#region GetAutoRateScope

		public void TestGetAutoRateScope_IsRateSelector()
		{
			void TestCase(bool isRateSelector)
			{
				var context = new Mock<IRatingContext>();
				context.Setup(c => c.IsManualCostSelectMode).Returns(isRateSelector);

				var scope = RatingUsageCollector.GetAutoRateScope(
					context.Object,
					new TestRatingCriteria(),
					AutoRateOptions.AutorateCostsRevenue,
					CostSell.Cost);

				AssertCollectionContains(
					$"Expected scope to contain (UsageProperties.IsRateSelector, {isRateSelector})",
					(UsageProperties.IsRateSelector, (object)isRateSelector),
					scope);
			}

			TestCase(isRateSelector: true);
			TestCase(isRateSelector: false);
		}

		public void TestGetAutoRateScope_TriggerSource()
		{
			void TestCase(AutoRateTriggerSource triggerSource)
			{
				var scope = RatingUsageCollector.GetAutoRateScope(
					new Mock<IRatingContext>().Object,
					new TestRatingCriteria(),
					new AutoRateOptions(triggerSource: triggerSource),
					CostSell.Cost);

				AssertCollectionContains(
					(UsageProperties.TriggerSource, (object)triggerSource.ToString()),
					scope
				);
			}

			TestCase(AutoRateTriggerSource.Api);
			TestCase(AutoRateTriggerSource.Menu);
			TestCase(AutoRateTriggerSource.Workflow);
			TestCase(AutoRateTriggerSource.Unspecified);
			TestCase(AutoRateTriggerSource.PrintInvoicing);
			TestCase(AutoRateTriggerSource.GatewayBilling);
			TestCase(AutoRateTriggerSource.OperationalActions);
		}

		public void TestGetAutoRateScope_ContainerMode()
		{
			void TestCase(string containerMode)
			{
				var criteria = new TestRatingCriteria();
				criteria.ContainerMode = containerMode;

				var scope = RatingUsageCollector.GetAutoRateScope(
					new Mock<IRatingContext>().Object,
					criteria,
					AutoRateOptions.AutorateCostsRevenue,
					CostSell.Cost);

				AssertCollectionContains((UsageProperties.ContainerMode, (object)containerMode), scope);
			}

			TestCase("FCL");
			TestCase("LCL");
			TestCase("ULD");
			TestCase("LSE");
		}

		public void TestGetAutoRateScope_TransportMode()
		{
			void TestCase(FreightMode freightMode, string expectedTransportMode)
			{
				var criteria = new TestRatingCriteria();
				criteria.FreightMode = freightMode;

				var scope = RatingUsageCollector.GetAutoRateScope(
					new Mock<IRatingContext>().Object,
					criteria,
					AutoRateOptions.AutorateCostsRevenue,
					CostSell.Cost);

				AssertCollectionContains(
					(UsageProperties.TransportMode, (object)expectedTransportMode),
					scope
				);
			}

			TestCase(FreightMode.AIR, "AIR");
			TestCase(FreightMode.LSE, "AIR");
			TestCase(FreightMode.SEA, "SEA");
			TestCase(FreightMode.FCL, "SEA");
			TestCase(FreightMode.LCL, "SEA");
			TestCase(FreightMode.FRA, "RAI");
			TestCase(FreightMode.LRO, "ROA");
		}

		public void TestGetAutoRateScope_Mode()
		{
			void TestCase(CostSell costSell, bool isRebateMode, string expectedMode)
			{
				var context = new Mock<IRatingContext>();
				context.Setup(c => c.IsInRebateCalculationMode).Returns(isRebateMode);

				var scope = RatingUsageCollector.GetAutoRateScope(
					context.Object,
					new TestRatingCriteria(),
					AutoRateOptions.AutorateCostsRevenue,
					costSell);

				AssertCollectionContains(
					"Expected the scope to contain the correct mode.",
					(UsageProperties.Mode, (object)expectedMode),
					scope
				);
			}

			TestCase(costSell: CostSell.Cost, isRebateMode: false, "Cost");
			TestCase(costSell: CostSell.Revenue, isRebateMode: false, "Revenue");
			TestCase(costSell: CostSell.Revenue, isRebateMode: true, "Rebate");
		}

		public void TestGetAutoRateScope_JobType()
		{
			void TestCase(AdapterType adapterType, string expectedJobType)
			{
				var criteria = new TestRatingCriteria();
				criteria.AdapterType = adapterType;

				var scope = RatingUsageCollector.GetAutoRateScope(
					new Mock<IRatingContext>().Object,
					criteria,
					AutoRateOptions.AutorateCostsRevenue,
					CostSell.Cost);

				AssertCollectionContains(
					(UsageProperties.JobType, (object)expectedJobType),
					scope
				);
			}

			TestCase(AdapterType.Shipment, "Shipment");
			TestCase(AdapterType.Consolidation, "Consolidation");
		}

		public void TestGetAutoRateScope_JobId()
		{
			var criteria = new TestRatingCriteria();
			criteria.JobID = "666";

			var scope = RatingUsageCollector.GetAutoRateScope(
				new Mock<IRatingContext>().Object,
				criteria,
				AutoRateOptions.AutorateCostsRevenue,
				CostSell.Cost);

			AssertCollectionContains((UsageProperties.JobID, (object)"666"), scope);
		}

		public void TestGetAutoRateScope_RateSelectorType()
		{
			void TestCase(string rateSelectorType, bool carrierConnectEnabled)
			{
				var context = new Mock<IRatingContext>();
				context.Setup(c => c.IsManualCostSelectMode).Returns(true);

				using (ObjectFactory.Substitute(MockCarrierConnectFeatureControl(carrierConnectEnabled)))
				{
					var scope = RatingUsageCollector.GetAutoRateScope(
						context.Object,
						new TestRatingCriteria(),
						AutoRateOptions.AutorateCostsRevenue,
						CostSell.Cost);

					AssertCollectionContains(
						$"Expected scope to contain (UsageProperties.RateSelectorType, {rateSelectorType})",
						(UsageProperties.RateSelectorType, (object)rateSelectorType),
						scope);
				}
			}

			TestCase(RateSelectorConstants.CarrierConnect, true);
			TestCase(RateSelectorConstants.Legacy, false);
		}

		#endregion

		#region ReportAutoRate

		public void TestReportAutoRate_ShouldReportUsageEvent_WithRatesAppliedDetails()
		{
			var wiseRate1 = new Rate { Id = "Rate 1" };
			var wiseRate2 = new Rate { Id = "Rate 2" };
			var wiseRate3 = new Rate { Id = "Rate 3" };

			var wiseLine1 = new WiseLine(Factory, new Charge());
			var wiseLine2 = new WiseLine(Factory, new Charge());
			var wiseLine3 = new WiseLine(Factory, new Charge());
			var wiseLine4 = new WiseLine(Factory, new Charge());
			var wiseLine5 = new WiseLine(Factory, new Charge());

			wiseLine1.ParentRateEntry = new WiseEntry(wiseRate1, Factory) { RateProvider = WRConstants.RateProviders.CargoSphere }; // CargoSphere Rate 1
			wiseLine2.ParentRateEntry = new WiseEntry(wiseRate2, Factory) { RateProvider = WRConstants.RateProviders.CargoSphere }; // CargoSphere Rate 2
			wiseLine3.ParentRateEntry = new WiseEntry(wiseRate2, Factory) { RateProvider = WRConstants.RateProviders.CargoSphere }; // CargoSphere Rate 2
			wiseLine4.ParentRateEntry = new WiseEntry(wiseRate3, Factory) { RateProvider = WRConstants.RateProviders.CargoGuide }; // Cargoguide Rate 1
			wiseLine5.ParentRateEntry = new WiseEntry(wiseRate3, Factory) { RateProvider = WRConstants.RateProviders.CargoGuide }; // Cargoguide Rate 1

			var header = Factory.New<RatingHeader>();
			var rate1 = header.AddRateEntry("FCL");
			var rate2 = header.AddRateEntry("FCL");

			var line1 = rate1.AddRateLine("FRT", lineUnit: "KG"); // CW1 Rate 1
			var line2 = rate1.AddRateLine("FRT", lineUnit: "KG"); // CW1 Rate 1
			var line3 = rate2.AddRateLine("FRT", lineUnit: "KG"); // CW1 Rate 2
			var line4 = rate2.AddRateLine("FRT", lineUnit: "KG"); // CW1 Rate 2
			var line5 = rate2.AddRateLine("FRT", lineUnit: "KG"); // CW1 Rate 2

			var results = new RatesAdditionInfo
			{
				RatesFound = new[]
				{
					new ChargeInfo("FRT", new AutoRateInfo(Factory, wiseLine1)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, wiseLine2)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, wiseLine3)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, wiseLine4)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, wiseLine5)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, line1)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, line2)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, line3)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, line4)),
					new ChargeInfo("FRT", new AutoRateInfo(Factory, line5)),
				}
			};

			RatingUsageCollector.ReportAutoRate(Factory, results, TimeSpan.FromMilliseconds(666));

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(UsageFeatures.Codes.Autorate);
			var message = messages.First();

			AssertEquals(
				"The event should be saved in the passed factory (which we don't save) rather than in some internal one.",
				false,
				message.IsInDatabase
			);

			var actual = message.GetProperty<UsageRatesSearchResult>(UsageProperties.RatesSearchResult);
			AssertEquals(".CargoSphere.TotalRates", 2, actual.CargoSphere.TotalRates);
			AssertEquals(".CargoSphere.TotalCharges", 3, actual.CargoSphere.TotalCharges);
			AssertEquals(".Cargoguide.TotalRates", 1, actual.Cargoguide.TotalRates);
			AssertEquals(".Cargoguide.TotalCharges", 2, actual.Cargoguide.TotalCharges);
			AssertEquals(".CW1.TotalRates", 2, actual.CW1.TotalRates);
			AssertEquals(".CW1.TotalCharges", 5, actual.CW1.TotalCharges);
			AssertEquals(
				666,
				message.GetProperty<int>(UsageProperties.ElapsedTime)
			);
		}

		#endregion

		#region ReportRateSelector

		void AssertReportUsageEvent_WithRatesAppliedDetails(string rateSelector)
		{
			var ratesSearchResult = new UsageRatesSearchResult();
			ratesSearchResult.CargoSphere.TotalRates = 5;
			ratesSearchResult.Cargoguide.TotalRates = 7;
			ratesSearchResult.CW1.TotalRates = 4;

			RatingUsageCollector.ReportRateSelector(RatingUsageCollector.RateSelectorAction.Select, 666, RatingUsageCollector.RateProvider.Cargoguide, ratesSearchResult, 888);

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(UsageFeatures.Codes.RateSelector);
			var message = messages.First();

			AssertEquals(
				666,
				message.GetProperty<int>(UsageProperties.ElapsedTime)
			);

			AssertEquals(
				888,
				message.GetProperty<int>(UsageProperties.SessionTime)
			);

			var actualRatesSearchResult = message.GetProperty<UsageRatesSearchResult>(UsageProperties.RatesSearchResult);
			AssertEquals(5, actualRatesSearchResult.CargoSphere.TotalRates);
			AssertEquals(7, actualRatesSearchResult.Cargoguide.TotalRates);
			AssertEquals(4, actualRatesSearchResult.CW1.TotalRates);

			var actualAction = message.GetProperty<string>(UsageProperties.Action);
			AssertEquals("The action should be 'Select'.", "Select", actualAction);

			var actualSelectedProvider = message.GetProperty<string>(UsageProperties.SelectedProvider);
			AssertEquals("The selected provider should be 'Cargoguide'.", "Cargoguide", actualSelectedProvider);

			var rateSelectorType = message.GetProperty<string>(UsageProperties.RateSelectorType);
			AssertEquals($"The rate selector should be {rateSelector}", rateSelector, rateSelectorType);
		}

		public void TestReportRateSelector_ShouldReportUsageEvent_WithRatesAppliedDetailsLegacy()
		{
			using (ObjectFactory.Substitute(MockCarrierConnectFeatureControl(false)))
			{
				AssertReportUsageEvent_WithRatesAppliedDetails(RateSelectorConstants.Legacy);
			}
		}

		public void TestReportRateSelector_ShouldReportUsageEvent_WithRatesAppliedDetailsCarrierConnect()
		{
			using (ObjectFactory.Substitute(MockCarrierConnectFeatureControl(true)))
			{
				AssertReportUsageEvent_WithRatesAppliedDetails(RateSelectorConstants.CarrierConnect);
			}
		}

		#endregion

		#region ReportRateSelectorSearch

		void AssertReportUsageSearchEvent_WithRatesApplied(string rateSelector)
		{
			var ratesSearchResult = new UsageRatesSearchResult();
			ratesSearchResult.CargoSphere.TotalRates = 5;
			ratesSearchResult.CargoSphere.ErrorRates = 1;
			ratesSearchResult.CargoSphere.WarningRates = 3;
			ratesSearchResult.CargoSphere.ValidRates = 2;
			ratesSearchResult.Cargoguide.TotalRates = 7;
			ratesSearchResult.Cargoguide.ErrorRates = 3;
			ratesSearchResult.Cargoguide.WarningRates = 4;
			ratesSearchResult.Cargoguide.ValidRates = 3;
			ratesSearchResult.CW1.TotalRates = 4;
			ratesSearchResult.CW1.ValidRates = 4;
			ratesSearchResult.CargoSphere.ElapsedTime = 1234;
			ratesSearchResult.Cargoguide.ElapsedTime = 5678;
			ratesSearchResult.CW1.ElapsedTime = 9012;

			RatingUsageCollector.ReportRateSelectorSearch(ratesSearchResult);

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(UsageFeatures.Codes.RateSelectorSearch);
			var message = messages.First();

			var actual = message.GetProperty<UsageRatesSearchResult>(UsageProperties.RatesSearchResult);
			AssertEquals(5, actual.CargoSphere.TotalRates);
			AssertEquals(1, actual.CargoSphere.ErrorRates);
			AssertEquals(3, actual.CargoSphere.WarningRates);
			AssertEquals(2, actual.CargoSphere.ValidRates);
			AssertEquals(7, actual.Cargoguide.TotalRates);
			AssertEquals(3, actual.Cargoguide.ErrorRates);
			AssertEquals(4, actual.Cargoguide.WarningRates);
			AssertEquals(3, actual.Cargoguide.ValidRates);
			AssertEquals(4, actual.CW1.TotalRates);
			AssertEquals(4, actual.CW1.ValidRates);
			AssertEquals(1234, actual.CargoSphere.ElapsedTime);
			AssertEquals(5678, actual.Cargoguide.ElapsedTime);
			AssertEquals(9012, actual.CW1.ElapsedTime);

			var rateSelectorType = message.GetProperty<string>(UsageProperties.RateSelectorType);
			AssertEquals($"The rate selector should be {rateSelector}", rateSelector, rateSelectorType);
		}

		public void TestReportRateSelectorSearch_ShouldReportUsageEvent_WithRatesAppliedLegacy()
		{
			using (ObjectFactory.Substitute(MockCarrierConnectFeatureControl(false)))
			{
				AssertReportUsageSearchEvent_WithRatesApplied(RateSelectorConstants.Legacy);
			}
		}

		public void TestReportRateSelectorSearch_ShouldReportUsageEvent_WithRatesAppliedCarrierConnect()
		{
			using (ObjectFactory.Substitute(MockCarrierConnectFeatureControl(true)))
			{
				AssertReportUsageSearchEvent_WithRatesApplied(RateSelectorConstants.CarrierConnect);
			}
		}

		#endregion

		#region ReportRatesLoaded

		public void TestReportRatesLoaded_WithValidStats_ShouldReportUsageEvent()
		{
			var companytariff = Factory.New<CompanyTariff>();
			companytariff.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE");
			Factory.New<GlobalTariff>();
			Factory.Save();

			var stats = new RatesLoadedStats 
			{
				LoadedRatesCount = 250,
				FilteredRatesCount = 120
			};
			var totalElapsedTimeMs = 1500;
			RatingUsageCollector.ReportRatesLoaded(stats, totalElapsedTimeMs);

			var helper = new UsageCollectorTestHelper(Factory);
			var messages = helper.LoadUsageMessages(UsageFeatures.Codes.RatesLoaded);
			var message = messages.First();

			var actualStats = message.GetProperty<RatesLoadedStats>(UsageProperties.RatesLoadedStats);
			var elapsedTime = message.GetProperty<long>(UsageProperties.ElapsedTime);
			var dbStats = message.GetProperty<DbStats>(UsageProperties.DBStats);
			AssertNotNull("The RatesLoadedStats should be reported in the usage event.", actualStats);
			AssertNotNull("The DBStats should be reported in the usage event RLD.", dbStats);

			AssertEquals("LoadedRatesCount should match", stats.LoadedRatesCount, actualStats.LoadedRatesCount);
			AssertEquals("FilteredRatesCount should match", stats.FilteredRatesCount, actualStats.FilteredRatesCount);
			AssertEquals("TotalElapsedTimeMs should match", totalElapsedTimeMs, elapsedTime);
		}

		#endregion

		#region Helpers

		static IFeatureControlManager MockCarrierConnectFeatureControl(bool enabled)
		{
			var featureControlMock = new Mock<IFeatureControlManager>();

			var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = enabled };
			var featureDataMock = new Mock<IFeatureData>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			return featureControlMock.Object;
		}

		#endregion
		
		#region GetDbStats

		[TestDate(2025, 3, 12)]
		public void TestGetDbStats()
		{
			var costing1 = Helper.NewCosting(TransportProvider1);
			costing1.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 300m);
			var expiredRateEntry = costing1.AddRateEntryWithFlatRateLine("AIR", "LSE", "CN", "", "FRT", 300m);
			expiredRateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-3);
			expiredRateEntry.TI_RateEndDate = ZDate.Today.AddDays(-1);

			var costing2 = Helper.NewCosting(TransportProvider2);
			costing2.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 300m);

			var clientRate1 = Helper.NewClientRate(Consignor);
			clientRate1.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "", "FRT", 300m);

			var companytariff = Factory.New<CompanyTariff>();
			companytariff.AddRateEntry("AIR", "LSE", "USLAX", "AUBNE");
			Factory.New<GlobalTariff>();

			Helper.NewIntercompanyTariff(Helper.NewOrgHeader());

			Factory.Save();

			var firstResult = RatingUsageCollector.GetDbStats(new DbStatsRetriever());

			AssertEquals("ClientRate.RatingHeaders", (ZInt)1, firstResult.ClientRate.RatingHeaders);
			AssertEquals("Costing.RatingHeaders", (ZInt)2, firstResult.Costing.RatingHeaders);
			AssertEquals("CompanyTariff.RatingHeaders", (ZInt)2, firstResult.CompanyTariff.RatingHeaders);
			AssertEquals("InterCompanyTariff.RatingHeaders", (ZInt)1, firstResult.InterCompanyTariff.RatingHeaders);

			AssertEquals("ClientRate.RateEntries", (ZInt)1, firstResult.ClientRate.RateEntries);
			AssertEquals("Costing.RateEntries", (ZInt)3, firstResult.Costing.RateEntries);
			AssertEquals("CompanyTariff.RateEntries", (ZInt)1, firstResult.CompanyTariff.RateEntries);
			AssertEquals("InterCompanyTariff.RateEntries", (ZInt)0, firstResult.InterCompanyTariff.RateEntries);

			AssertEquals("ClientRate.ExpiredRateEntries", (ZInt)0, firstResult.ClientRate.ExpiredRateEntries);
			AssertEquals("Costing.ExpiredRateEntries", (ZInt)1, firstResult.Costing.ExpiredRateEntries);
			AssertEquals("CompanyTariff.ExpiredRateEntries", (ZInt)0, firstResult.CompanyTariff.ExpiredRateEntries);
			AssertEquals("InterCompanyTariff.ExpiredRateEntries", (ZInt)0, firstResult.InterCompanyTariff.ExpiredRateEntries);

			costing2.AddRateEntryWithFlatRateLine("AIR", "LSE", "US", "", "FRT", 300m);
			Factory.Save();

			var secondResult = RatingUsageCollector.GetDbStats(new DbStatsRetriever());
			AssertEquals("Second call should return cached instance", true, Object.ReferenceEquals(firstResult, secondResult));

			var cacheItemPolicy = new CacheItemPolicy
			{
				AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(-1)
			};
			MemoryCache.Default.Set("RatingDbStats", firstResult, cacheItemPolicy);

			var thirdResult = RatingUsageCollector.GetDbStats(new DbStatsRetriever());

			AssertEquals("Third call should not return cached instance", false, Object.ReferenceEquals(firstResult, thirdResult));
			AssertEquals("Costing.RateEntries", (ZInt)4, thirdResult.Costing.RateEntries);
		}
		
		[TestDate(2025, 3, 18)]
		public void TestGetDbStats_ReturnsEmptyStatsWhenRetrievalFailsAndNoCachedStats()
		{
			var mockRetriever = new Mock<IDbStatsRetriever>();
			mockRetriever.Setup(r => r.RetrieveDbStatsFromDatabase(ref It.Ref<DbStats>.IsAny))
				.Returns(false);
			
			var result = RatingUsageCollector.GetDbStats(mockRetriever.Object);
			
			AssertEquals("Should return empty client rate entries", (ZInt)0, result.ClientRate.RateEntries);
			AssertEquals("Should return empty costing entries", (ZInt)0, result.Costing.RateEntries);
			AssertEquals("Should return empty company tariff entries", (ZInt)0, result.CompanyTariff.RateEntries);
			AssertEquals("Should return empty intercompany tariff entries", (ZInt)0, result.InterCompanyTariff.RateEntries);
			
			// Check that the empty stats were cached with a 1-hour expiration
			var cachedItem = MemoryCache.Default.GetCacheItem("RatingDbStats");
			AssertNotNull("Empty stats should be cached", cachedItem);
		}

		#endregion

		[TestDate(2025, 3, 20)]
		public void TestGetDbStats_HandlesExceptions()
		{
			var mockRetriever = new Mock<IDbStatsRetriever>();
			mockRetriever.Setup(r => r.RetrieveDbStatsFromDatabase(ref It.Ref<DbStats>.IsAny))
				.Throws(new Exception("Database connection failure simulation"));
			
			var result = RatingUsageCollector.GetDbStats(mockRetriever.Object);

			AssertEquals("Should have an error report for the database failure", 1, ErrorReporter.TotalErrorCount);
			
			var expected = "Failed to retrieve database statistics for rating: Database connection failure simulation";
			AssertContains(expected, ErrorReporter.LastMessageReported);

			AssertEquals("Should return empty client rate entries when exception occurs", (ZInt)0, result.ClientRate.RateEntries);
			AssertEquals("Should return empty costing entries when exception occurs", (ZInt)0, result.Costing.RateEntries);
			AssertEquals("Should return empty company tariff entries when exception occurs", (ZInt)0, result.CompanyTariff.RateEntries);
			AssertEquals("Should return empty intercompany tariff entries when exception occurs", (ZInt)0, result.InterCompanyTariff.RateEntries);
			
			var cachedItem = MemoryCache.Default.GetCacheItem("RatingDbStats");
			AssertNotNull("Empty stats should be cached even after exception", cachedItem);
			ErrorReporter.Clear();
		}
	}
}

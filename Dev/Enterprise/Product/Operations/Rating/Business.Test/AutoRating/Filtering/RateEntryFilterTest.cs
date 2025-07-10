using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using WiseRatesApi = WiseRates.Api.Model;
using WiseRatesConsts = WiseRates.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateEntryFilterTest : RatingTestCase
	{
		public void TestFindRateEntries_GivenSCN_ThenShouldNotFilterRatesByContainer()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			TestCriteria.DebtorOrgs[RatingDebtorOrgTypes.CCUS] = controllingCustomer;

			var container = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", 1, container, NewClient);
			criteria.FreightMode = FreightMode.SEA | FreightMode.SCN;

			var clientRate = Helper.NewClientRate(controllingCustomer);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.SCN, "UAIEV", "AUSYD", "BAF", 90m, currency: "USD", container: "20GP");

			var rateEntries = Filter(new[] { clientRate }, criteria, true);
			AssertContainsExactElementsInAnyOrder
			(
				new[] { "DST-20GP-SCN" },
				rateEntries.Select(rateEntry => $"{rateEntry.TI_RateCategory}-{rateEntry.Container.RC_Code}-{rateEntry.TI_Mode}")
			);
		}

		public void TestFindRateEntries_BLK_BBK_ROR()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			TestCriteria.DebtorOrgs[RatingDebtorOrgTypes.CCUS] = controllingCustomer;

			var clientRate = Helper.NewClientRate(controllingCustomer);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.BBK, "UAIEV", "AUSYD", "BAF", 20m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.BLK, "UAIEV", "AUSYD", "BAF", 30m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.ROR, "UAIEV", "AUSYD", "BAF", 40m, currency: "USD");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 50m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.BBK, "UAIEV", "AUSYD", "BAF", 60m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.BLK, "UAIEV", "AUSYD", "BAF", 70m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.ROR, "UAIEV", "AUSYD", "BAF", 80m, currency: "USD");

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 90m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.BBK, "UAIEV", "AUSYD", "BAF", 100m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.BLK, "UAIEV", "AUSYD", "BAF", 110m, currency: "USD");
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.ROR, "UAIEV", "AUSYD", "BAF", 120m, currency: "USD");

			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.LCL, clientRate, isRegistryEnabled: false, expectedRateEntries: new[] { "LCL-LCL", "ORG-LCL", "DST-LCL" });
			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.LCL, clientRate, isRegistryEnabled: true, expectedRateEntries: new[] { "LCL-LCL", "ORG-LCL", "DST-LCL" });

			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.BLK, clientRate, isRegistryEnabled: false, expectedRateEntries: Array.Empty<string>());
			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.BLK, clientRate, isRegistryEnabled: true, expectedRateEntries: new[] { "LCL-BLK", "ORG-BLK", "DST-BLK" });

			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.BBK, clientRate, isRegistryEnabled: false, expectedRateEntries: Array.Empty<string>());
			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.BBK, clientRate, isRegistryEnabled: true, expectedRateEntries: new[] { "LCL-BBK", "ORG-BBK", "DST-BBK" });

			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.ROR, clientRate, isRegistryEnabled: false, expectedRateEntries: Array.Empty<string>());
			AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode.ROR, clientRate, isRegistryEnabled: true, expectedRateEntries: new[] { "LCL-ROR", "ORG-ROR", "DST-ROR" });
		}

		void AssertTestFindRateEntries_BLK_BBK_ROR(FreightMode freightMode, RatingHeader ratingHeader, bool isRegistryEnabled, string[] expectedRateEntries)
		{
			TestCriteria.FreightMode = freightMode;

			using (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled))
			{
				var rateEntries = Filter(new[] { ratingHeader }, TestCriteria, true);
				AssertContainsExactElementsInAnyOrder
				(
					$"GIVEN FreightMode={freightMode} AND AutorateByBBK_BLK_ROR_BCNContainerModes={isRegistryEnabled}",
					expectedRateEntries,
					rateEntries.Select(rateEntry => $"{rateEntry.TI_RateCategory}-{rateEntry.TI_Mode}")
				);
			}
		}

		public void TestFindRateEntries_CriteriaHasControllingCustomer_ReturnRatesWithMatchingControllingCustomer()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var rate = TestRates.First().AllEntries.First();
			rate.TI_OH_ControllingCustomer = controllingCustomer.PK;

			var entries = Filter(TestRates, TestCriteria, true);
			AssertEquals("No rates with matching controlling customer", 0, entries.Count);

			TestCriteria.DebtorOrgs[RatingDebtorOrgTypes.CCUS] = controllingCustomer;

			entries = Filter(TestRates, TestCriteria, true);
			var actual = entries.Select(e => e.PK).ToArray();
			var expected = new[] { rate.PK };
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestFindRateEntries_CriteriaHasNoControllingCustomer_ReturnRatesWithWithoutMatchingControllingCustomer()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var rate = TestRates.First().AllEntries.First();
			rate.TI_OH_ControllingCustomer = ZGuid.Empty;

			var entries = Filter(TestRates, TestCriteria, true);

			var actualPKs = entries.Select(e => e.PK).ToArray();
			var expectedPKs = new[] { rate.PK };

			AssertContainsExactElementsInAnyOrder(
				"Filtered entries should match the expected PKs without controlling customer",
				expectedPKs,
				actualPKs
			);

			TestCriteria.DebtorOrgs[RatingDebtorOrgTypes.CCUS] = controllingCustomer;

			entries = Filter(TestRates, TestCriteria, true);

			actualPKs = entries.Select(e => e.PK).ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Filtered entries should match the expected PKs with controlling customer",
				expectedPKs,
				actualPKs
			);
		}

		public void TestFindRateEntries_ConsignorConsigneeControllingCustomerFilter_DatabaseLoads()
		{
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.OH_IsConsignee = true;

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_IsConsignee = true;

			var consignee3 = Factory.NewWithValidTestData<OrgHeader>();
			consignee3.OH_IsConsignee = true;

			var costing = Helper.NewCosting(TransportProvider1);
			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "NZ");
			entry1.TI_OH_Consignee = consignee1.PK;
			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "NZ");
			entry2.TI_OH_Consignee = consignee2.PK;
			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "NZ");

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var costingInFactory2 = factory2.Load<Costing>(costing.PK);

			var refContainer20 = factory2.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var consignee3InFactory2 = factory2.Load<OrgHeader>(consignee3.PK);
			var criteria = new TestRatingCriteria("AUSYD", "NZAKL", 1, refContainer20, consignee3InFactory2);
			criteria.DeliveryAddress = consignee3InFactory2.MainAddress;
			var entries = Filter(new[] { costingInFactory2 }, criteria, true, factory2);

			var actualEntries = entries.Select(e => e.PK).ToArray();
			var expectedEntries = new[] { entry3.PK };
			AssertContainsExactElementsInAnyOrder(
				"Entries should only include the expected rate entry",
				expectedEntries,
				actualEntries
			);

			IBusinessObjectFactoryInternals factory2Internals = factory2;
			CombineAssertions(() =>
			{
				AssertEquals(
					"should not load consignee for unrelated rate",
					false,
					factory2Internals.AllBusinessObjects
						.OfType<OrgHeader>()
						.Any(x => x.PK == consignee1.PK || x.PK == consignee2.PK)
				);
				AssertEquals(
					"should not load consignee addresses for unrelated rate",
					false,
					factory2Internals.AllBusinessObjects
						.OfType<OrgAddress>()
						.Any(x => x.PK == consignee1.MainAddress.PK || x.PK == consignee2.MainAddress.PK)
				);
			});
		}

		public void TestGatewayServiceLevel()
		{
			var collection = new List<IRatingHeader>();
			var intercompanyTariff1 = Helper.NewIntercompanyTariff(NewClient);
			var intercompanyTariff2 = Helper.NewIntercompanyTariff(NewClient2);
			collection.Add(intercompanyTariff1);
			collection.Add(intercompanyTariff2);

			var entry11 = intercompanyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry11.TI_RS_NKGatewayServiceLevel = "DIR";
			var entry12 = intercompanyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry12.TI_RS_NKGatewayServiceLevel = "DEF";

			var entry21 = intercompanyTariff2.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry21.TI_RS_NKGatewayServiceLevel = "DIR";
			var entry22 = intercompanyTariff2.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry22.TI_RS_NKGatewayServiceLevel = "DEF";

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.overridenGatewayServiceLevelFilteredReason = (gatewayServiceLevel, gatewayAgentPk) =>
			{
				if ((gatewayServiceLevel == "DIR" && gatewayAgentPk == NewClient.PK) ||
					(gatewayServiceLevel == "DEF" && gatewayAgentPk == NewClient2.PK))
				{
					return ZString.Empty;
				}

				return "Error";
			};

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();

				var results = Filter(collection, testCriteria, false);

				AssertEquals("Count", 2, results.Count);
				Assert("Entry11 from intercompanyTariff1 (NewClient)", results.Contains(entry11));
				Assert("Entry22 from intercompanyTariff2 (NewClient2)", results.Contains(entry22));
			}
		}

		public void TestShipmentGatewayServiceLevel()
		{
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DIR").RS_IsGateway = true;
			Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "DEF").RS_IsGateway = true;

			var intercompanyTariff1 = Helper.NewIntercompanyTariff(NewClient);
			var collection = new List<IRatingHeader> { intercompanyTariff1 };

			var entry11 = intercompanyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry11.TI_RS_NKShipmentGatewayServiceLevel = "DIR";
			var entry12 = intercompanyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			entry12.TI_RS_NKShipmentGatewayServiceLevel = "DEF";

			var testCriteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 15M, 1M, NewClient);
			testCriteria.ShipmentGatewayServiceLevel = "DIR";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartSell();

				var results = Filter(collection, testCriteria, false);

				AssertEquals("Count", 1, results.Count);
				Assert("Entry11 with Gateway Service Level 'DIR' from intercompanyTariff1", results.Contains(entry11));
			}
		}

		[TestDate(2020, 04, 25)]
		public void TestFilter_ShouldDiscardWiseEntriesWithScheduleInformation()
		{
			var entries = new List<IRateEntry>();

			var rate1 = new WiseRatesApi.Rate
			{
				ProviderRateId = "R_123456",
				Carrier = "EMR",
				Origin = "UAIEV",
				Destination = "AUSYD",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				Provider = WiseRatesConsts.WRConstants.RateProviders.CargoSphere,
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2025, 01, 01),
				Charges = new[]
				{
					new WiseRatesApi.Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m },
				},
				BookingInfo = new WiseRatesApi.BookingInfo()
				{
					Schedule = new WiseRatesApi.Schedule()
					{
						DepartureDate = new DateTime(2020, 01, 01),
						ArrivalDate = new DateTime(2025, 01, 01),
						ScheduleDetails = new[]
						{
							new WiseRatesApi.ScheduleDetail()
							{
								Origin = "UAIEV",
								Destination = "AUSYD"
							},
						}
					}
				}
			};

			var rate2 = new WiseRatesApi.Rate
			{
				ProviderRateId = "Z_222333",
				Carrier = "EMR",
				Origin = "UAIEV",
				Destination = "AUSYD",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				Provider = WiseRatesConsts.WRConstants.RateProviders.CargoSphere,
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2025, 01, 01),
				Charges = new[]
				{
					new WiseRatesApi.Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m },
				},
			};

			var wiseEntry1 = new WiseEntry(rate1, Factory);
			wiseEntry1.TI_RateCategory = "FCL";
			wiseEntry1.TI_Mode = "SEA";
			wiseEntry1.TI_RateStartDate = new ZDate(2020, 01, 01);

			var wiseEntry2 = new WiseEntry(rate2, Factory);
			wiseEntry2.TI_RateStartDate = new ZDate(2020, 01, 01);
			wiseEntry2.TI_RateCategory = "FCL";
			wiseEntry2.TI_Mode = "SEA";

			TransportProvider1.OH_Code = "CARRIER01";

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = TransportProvider1.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry1, wiseEntry2 };

			entries.Add(wiseEntry1);
			entries.Add(wiseEntry2);

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.FCL, 100, 5, NewClient);
			var logger = new ElementaryLogger();
			var filteredEntries = RateEntryFilter.Filter(criteria, true, entries, Factory, logger);

			AssertEquals("Filtered entries count should be 1", 1, filteredEntries.Count());

			var matchedRate = (WiseEntry)filteredEntries.Single();
			AssertEquals("Filtered WiseEntry should have ProviderRateId 'Z_222333'", "Z_222333", matchedRate.WiseRate.ProviderRateId);

			AssertCollectionContains(
				"RateEntry Filtered Wise Costing CARRIER01 reason: Rate ID 'R_123456' discarded because it has schedule information and is considered as an spot rate.",
				logger.Infos
			);
		}

		public void TestFindRateEntries_TransportProviderFilter_PossibleCarriers()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsShippingLine = true;

			var rate = TestRates.First().AllEntries.First();
			rate.TI_OH_TransportProvider = carrier1.PK;
			TestCriteria.PossibleCarriers = new[] { carrier1, carrier2 };

			// carrier1 rate should not be removed
			var entries = Filter(TestRates, TestCriteria, true);
			var actualEntries1 = entries.Select(e => e.PK).ToArray();
			var expectedEntries1 = new[] { rate.PK };
			AssertContainsExactElementsInAnyOrder(
				"Carrier1 rate should not be removed",
				expectedEntries1,
				actualEntries1
			);

			// carrier2 rate should not be removed
			rate.TI_OH_TransportProvider = carrier2.PK;
			entries = Filter(TestRates, TestCriteria, true);
			var actualEntries2 = entries.Select(e => e.PK).ToArray();
			var expectedEntries2 = new[] { rate.PK };
			AssertContainsExactElementsInAnyOrder(
				"Carrier2 rate should not be removed",
				expectedEntries2,
				actualEntries2
			);

			// carrier3 rate should be removed
			rate.TI_OH_TransportProvider = carrier3.PK;
			entries = Filter(TestRates, TestCriteria, true);
			AssertEquals("Carrier3 rate should be removed", 0, entries.Count);
		}

		#region Container Quality Filter

		public void TestContainerQualityFilter_WhenContainerQualityListIsEmpty()
		{
			var rates = GetSampleRaresToTestContainerQualityFilter();
			SetAndAssertContainerQuality(rates.wiseEntry1, "GOH"); // It not applicable
			SetAndAssertContainerQuality(rates.wiseEntry2, "");
			var entries = new List<IRateEntry>() { rates.entry1, rates.entry2, rates.wiseEntry1, rates.wiseEntry2 };

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.FCL, 100, 5, NewClient);
			var logger = new ElementaryLogger();

			Assert("Prerequisites", !criteria.RateableMeasures.GetDistinctContainerQualities().Any());

			var filteredEntries = RateEntryFilter.Filter(criteria, true, entries, Factory, logger);

			AssertEquals(" Rates with specific ContainerQuality Shouold be removed when Container Quality list is Empty", filteredEntries.Count(), 3);
		}

		public void TestContainerQualityFilter_ShouldNotRemoveCW1Rate()
		{
			var rates = GetSampleRaresToTestContainerQualityFilter();
			var entries = new List<IRateEntry>() { rates.entry1, rates.entry2 };

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.FCL, 100, 5, NewClient);
			var logger = new ElementaryLogger();

			AddAndAssertContainerQuality(criteria, GP20, "GOH");

			var filteredEntries = RateEntryFilter.Filter(criteria, true, entries, Factory, logger);

			AssertEquals("CW1 Rates shouldn't be removed by Container Quality filter", filteredEntries.Count(), 2);
		}

		public void TestContainerQualityFilter_ShouldNotRemoveWiseRateWithEmptyContainerQuality()
		{
			var rates = GetSampleRaresToTestContainerQualityFilter();
			SetAndAssertContainerQuality(rates.wiseEntry1, "GOH");
			SetAndAssertContainerQuality(rates.wiseEntry2, "");
			var entries = new List<IRateEntry>() { rates.wiseEntry1, rates.wiseEntry2 };

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.FCL, 100, 5, NewClient);
			var logger = new ElementaryLogger();

			AddAndAssertContainerQuality(criteria, GP20, "GOH");

			var filteredEntries = RateEntryFilter.Filter(criteria, true, entries, Factory, logger);

			AssertEquals("Wise Rates with empty Container Quality shouldn't be removed by Container Quality filter", filteredEntries.Count(), 2);
		}

		public void TestContainerQualityFilter_ShouldRemoveWiseRateWithWrongContainerQuality()
		{
			var rates = GetSampleRaresToTestContainerQualityFilter();
			SetAndAssertContainerQuality(rates.wiseEntry1, "GOH");
			SetAndAssertContainerQuality(rates.wiseEntry2, "GAH");
			var entries = new List<IRateEntry>() { rates.wiseEntry1, rates.wiseEntry2 };

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.FCL, 100, 5, NewClient);
			var logger = new ElementaryLogger();

			AddAndAssertContainerQuality(criteria, GP20, "GOH");

			var filteredEntries = RateEntryFilter.Filter(criteria, true, entries, Factory, logger);

			AssertEquals(
				"Wise Rates with wrong Container Quality should be removed by Container Quality filter",
				1,
				filteredEntries.Count()
			);

			var matchedRate = (WiseEntry)filteredEntries.Single();
			AssertEquals("Matched rate ContainerQuality should be 'GOH'", "GOH", matchedRate.ContainerQuality);

			AssertCollectionContains(
				"RateEntry Filtered Wise Costing CARRIER01 reason: ContainerQuality didn't match job GOH.",
				logger.Infos
			);
		}

		void AddAndAssertContainerQuality(RatingCriteria criteria, RefContainer container, string quality)
		{
			criteria.RateableMeasures.AddContainerGroup(container.PK, new[] {
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345", containerQuality: quality)
			});

			Assert("Setup Container Quality List", criteria.RateableMeasures.GetDistinctContainerQualities().Contains(quality));
		}

		void SetAndAssertContainerQuality(WiseEntry wiseEntry, string quality)
		{
			var customFields =
				wiseEntry.CustomFields?.Where(x => x.Code != WiseRatesApi.Rate.CustomFields.CargoSphere.ContainerQuality).ToList()
				?? new List<WiseRatesApi.CustomField>();

			if (quality != null)
			{
				customFields.Add(new WiseRatesApi.CustomField
				{
					Code = WiseRatesApi.Rate.CustomFields.CargoSphere.ContainerQuality,
					Value = quality
				});
			}

			wiseEntry.CustomFields = customFields;

			AssertEquals("Setup wiseEntry.ContainerQuality", new ZString(quality), wiseEntry.ContainerQuality);
		}

		(RateEntry entry1, RateEntry entry2, WiseEntry wiseEntry1, WiseEntry wiseEntry2) GetSampleRaresToTestContainerQualityFilter()
		{
			TransportProvider1.OH_Code = "CARRIER01";

			#region CW1 Costing setup

			var costing = Helper.NewCosting(TransportProvider1);
			var entry1 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", "AUSYD", "USLAX", "FRT", 100m, container: "20GP");
			entry1.TI_ContractNumber = "123";
			var entry2 = costing.AddRateEntryWithFlatRateLine("FCL", "SEA", "AUSYD", "USLAX", "FRT", 200m, container: "20GP");
			entry2.TI_ContractNumber = "321";

			#endregion

			#region CS Rate/Cost setup

			var wiseRate1 = new WiseRatesApi.Rate
			{
				ProviderRateId = "R_123456",
				Carrier = "EMR",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				Provider = WiseRatesConsts.WRConstants.RateProviders.CargoSphere,
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2025, 01, 01),
				Charges = new[]
				{
					new WiseRatesApi.Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 1000m },
				},
				ProviderCustomFields = new[]
				{
					new WiseRatesApi.CustomField
					{
						Code = WiseRatesApi.Rate.CustomFields.CargoSphere.ContainerQuality,
						Value = "GOH",
					},
				},
			};

			var wiseRate2 = new WiseRatesApi.Rate
			{
				ProviderRateId = "Z_222333",
				Carrier = "EMR",
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = "SEA",
				ContainerMode = "FCL",
				Provider = WiseRatesConsts.WRConstants.RateProviders.CargoSphere,
				StartDate = new DateTime(2020, 01, 01),
				ExpiryDate = new DateTime(2025, 01, 01),
				Charges = new[]
				{
					new WiseRatesApi.Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 2000m },
				},
			};

			var frt = Helper.ChargeCodes["FRT"];

			var wiseEntry1 = new WiseEntry(wiseRate1, Factory);
			wiseEntry1.TI_RateCategory = "FCL";
			wiseEntry1.TI_Mode = "SEA";
			wiseEntry1.TI_RateStartDate = new ZDate(2020, 01, 01);
			wiseEntry1.ChildRateLines = new[]
			{
				new WiseLine(Factory, wiseRate1.Charges[0]) { TL_AC = frt.PK }
			};

			var wiseEntry2 = new WiseEntry(wiseRate2, Factory);
			wiseEntry2.TI_RateCategory = "FCL";
			wiseEntry2.TI_Mode = "SEA";
			wiseEntry2.TI_RateStartDate = new ZDate(2020, 01, 01);
			wiseEntry2.ChildRateLines = new[]
			{
				new WiseLine(Factory, wiseRate2.Charges[0]) { TL_AC = frt.PK }
			};

			var wiseHeader = new WiseHeader(Factory);
			wiseHeader.TH_OH = TransportProvider1.PK;
			wiseHeader.ChildRateEntries = new[] { wiseEntry1, wiseEntry2 };

			#endregion

			return (entry1, entry2, wiseEntry1, wiseEntry2);
		}

		#endregion

		public void TestFindRateEntries_ContractFilter_NoAdapterNumberFilter() =>
			TestFindRateEntries_ContractFilter_DoNotRemoveICTAndTACTRates(entryNumber: "123", shouldApplySpecificAdapterContractNumberFilter: false);

		public void TestFindRateEntries_ContractFilter_WithAdapterNumberFilter_EntryNumberEmpty() =>
			TestFindRateEntries_ContractFilter_DoNotRemoveICTAndTACTRates(entryNumber: "", shouldApplySpecificAdapterContractNumberFilter: true);

		public void TestFindRateEntries_ContractFilter_WithAdapterNumberFilter_EntryNumberNotEmpty() =>
			TestFindRateEntries_ContractFilter_DoNotRemoveICTAndTACTRates(entryNumber: "123", shouldApplySpecificAdapterContractNumberFilter: true);

		void TestFindRateEntries_ContractFilter_DoNotRemoveICTAndTACTRates(string entryNumber, bool shouldApplySpecificAdapterContractNumberFilter)
		{
			var standardCosting = Helper.NewCosting(null);
			var entryTACT = standardCosting.AddRateEntry("LCL", "LCL", "UAIEV", "AUSYD");
			entryTACT.TI_IsTact = true;

			var interCompanyTariff = Helper.NewIntercompanyTariff(NewClient);
			var entryICT = interCompanyTariff.AddRateEntry("LCL", "LCL", "UAIEV", "AUSYD");
			TestRates = new RatingHeader[] { interCompanyTariff, standardCosting };

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				// Only ICT entry can set contract number. TACT entry does not have this field.
				entryICT.TI_ContractNumber = entryNumber;
				var testRatingCriteria = TestCriteria;
				testRatingCriteria.SetCarrierContractNumber("ABC");
				testRatingCriteria.ShouldApplySpecificAdapterContractNumberFilter = shouldApplySpecificAdapterContractNumberFilter;

				var filteredEntries = Filter(TestRates, TestCriteria, true);

				var actualPks = filteredEntries.Select(x => x.PK).ToArray();
				var expectedPks = new[] { entryICT.PK, entryTACT.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should not filter ICT and TACT rates by contract numbers",
					expectedPks,
					actualPks
				);
			}
		}

		public void TestViaFilter_EmptyInCriteria()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_ViaLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_ViaLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetVia(null);

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);

				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK };
				AssertContainsExactElementsInAnyOrder("Should filter the entry with Via value.", expected, actual);
			}
		}

		public void TestViaFilter()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_ViaLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_ViaLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetVia(LocationHelper.GetCachedLocationFromString("AUMEL", Factory));

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK, entryAUMEL.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with wrong Via value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingLocation_FirstLoadFilter_EmptyInCriteria()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_FirstLoadLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_FirstLoadLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetFirstLoad(null);

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK };

				AssertContainsExactElementsInAnyOrder("Should filter the entry with FirstLoad value.", expected, actual);
			}
		}

		public void TestMatchingLocation_FirstLoadFilter()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_FirstLoadLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_FirstLoadLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetFirstLoad(LocationHelper.GetCachedLocationFromString("AUMEL", Factory));

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries
					.Select(x => x.PK)
					.ToArray();

				var expected = new[] { entryEmpty.PK, entryAUMEL.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with wrong FirstLoad value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingLocation_LastDischargeFilter_EmptyInCriteria()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_LastDischargeLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_LastDischargeLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetLastDischarge(null);

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with LastDischarge value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingLocation_LastDischargeFilter()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_LastDischargeLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_LastDischargeLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetLastDischarge(LocationHelper.GetCachedLocationFromString("AUMEL", Factory));

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK, entryAUMEL.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with wrong LastDischarge value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingLocation_FirstRouteSetLoadFilter_EmptyInCriteria()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_FirstRouteSetLoadPortLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_FirstRouteSetLoadPortLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetFirstRouteSetLoad(null);

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with FirstRouteSetLoad value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingLocation_FirstRouteSetLoadFilter()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_FirstRouteSetLoadPortLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_FirstRouteSetLoadPortLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetFirstRouteSetLoad(LocationHelper.GetCachedLocationFromString("AUMEL", Factory));

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);

				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK, entryAUMEL.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with wrong FirstRouteSetLoad value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingLocation_LastRouteSetDischargeFilter_EmptyInCriteria()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_LastRouteSetDischargePortLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_LastRouteSetDischargePortLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetLastRouteSetDischarge(null);

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actualResult = filteredEntries.Select(x => x.PK).ToArray();
				var expectedResult = new[] { entryEmpty.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with LastRouteSetDischarge value.",
					expectedResult,
					actualResult
				);
			}
		}

		#region Postcode and transport zone set 

		IDocAddress GetAddress(RateTransportProvider zoneSetProvider, string postcode)
		{
			var mock = new Mock<IDocAddress>();
			mock.SetupGet(x => x.E2_City).Returns("potato");
			mock.SetupGet(x => x.E2_State).Returns("potato");
			mock.SetupGet(x => x.E2_Postcode).Returns(postcode);

			return mock.Object;
		}

		RateTransportProvider CreateTransportZoneSet(int numCities, int postcodesPerCities, int zoneItemsPerZone)
		{
			var maxPostcodes = numCities * postcodesPerCities;
			var postcodePairs = GeneratePostcodePairs(numCities, postcodesPerCities).GetEnumerator();

			var jamaicaZoneSet = Helper.CreateRateTransportZoneSet(null, CountryCodes.Jamaica);
			for (int i = 0; i < maxPostcodes / zoneItemsPerZone / 2; i += zoneItemsPerZone)
			{
				var zone = jamaicaZoneSet.CreateRateTransportZoneForTest($"Zone name {i} > {i + zoneItemsPerZone - 1}");
				zone.TZ_IsActive = true;

				for (int j = 0; j < zoneItemsPerZone; j++)
				{
					if (postcodePairs.MoveNext())
					{
						var postcodePair = postcodePairs.Current;
						zone.CreateRateTransportZoneItemForTest(postcodePair.fromPostcode, postcodePair.destinationPostcode);
					}
				}
			}

			return jamaicaZoneSet;
		}

		IEnumerable<(RefPostCode fromPostcode, RefPostCode destinationPostcode)> GeneratePostcodePairs(int numberOfCities, int postcodesPerCity)
		{
			var sequence = GeneratePostcodes(numberOfCities, postcodesPerCity);
			var pairs = sequence.SelectSequencedPairs((a, b) => (a, b), withOverlap: false);
			return pairs;
		}

		IEnumerable<RefPostCode> GeneratePostcodes(int numberOfCities, int postcodesPerCity)
		{
			var cities = new List<RefCityTown>();
			int postcodeCounter = 0;
			int totalPostcodes = numberOfCities * postcodesPerCity;
			int digitsNeeded = (int)Math.Ceiling(Math.Log10(totalPostcodes));

			for (int i = 0; i < numberOfCities; i++)
			{
				var city = Factory.New<RefCityTown>();
				city.R9_RN_NKCountry = CountryCodes.Jamaica;
				city.R9_InternationalName = $"Test City {i}";

				for (int j = 0; j < postcodesPerCity; j++)
				{
					var postCode = Factory.New<RefPostCode>();
					postCode.RK_RN_NKCountry = CountryCodes.Jamaica;
					postCode.RK_CityTownPostCode = postcodeCounter.ToString();
					postcodeCounter++;
					city.PostCodes.Add(postCode);
					yield return postCode;
				}
			}
		}

		public void TestZoneFilterExecutedBeforeSupplyFilter()
		{
			var zoneSetProvider = CreateTransportZoneSet(3, 3, 1);
			var costOrg = Helper.NewOrgHeader();
			var supplierOrg = Helper.NewOrgHeader();
			var supplierOrg2 = Helper.NewOrgHeader();
			var rateHeader = Helper.NewCosting(costOrg);

			var rateEntry = rateHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "FRT", 100);
			rateEntry.TI_TZ_OriginZone = zoneSetProvider.Zones[0].PK;
			rateEntry.TI_TZ_DestinationZone = zoneSetProvider.Zones[1].PK;
			rateEntry.TI_OH_Supplier = supplierOrg.PK;
			rateEntry.AddFlatRateLine("BAF", 33);

			var rateEntry2 = rateHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "WAR", 100);
			rateEntry2.TI_TZ_OriginZone = zoneSetProvider.Zones[0].PK;
			rateEntry2.TI_TZ_DestinationZone = zoneSetProvider.Zones[1].PK;
			rateEntry2.TI_OH_Supplier = supplierOrg2.PK;
			rateEntry2.AddFlatRateLine("CAF", 33);

			var rateEntry3 = rateHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "FRT", 101);
			rateEntry3.TI_TZ_OriginZone = zoneSetProvider.Zones[1].PK;
			rateEntry3.TI_TZ_DestinationZone = zoneSetProvider.Zones[2].PK;
			rateEntry3.TI_OH_Supplier = supplierOrg.PK;
			rateEntry3.AddFlatRateLine("BAF", 33);

			var rateEntry4 = rateHeader.AddRateEntryWithFlatRateLine("FCL", "SEA", "AU", "JM", "WAR", 101);
			rateEntry4.TI_TZ_OriginZone = zoneSetProvider.Zones[2].PK;
			rateEntry4.TI_TZ_DestinationZone = zoneSetProvider.Zones[1].PK;
			rateEntry4.TI_OH_Supplier = supplierOrg2.PK;
			rateEntry4.AddFlatRateLine("CAF", 33);
			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "JMALP", FreightMode.FCL, 100, 5, costOrg);
			criteria.PickupAddress = GetAddress(zoneSetProvider, postcode: "1");
			criteria.DeliveryAddress = GetAddress(zoneSetProvider, postcode: "2");

			criteria.Creditors[Helper.ChargeCodes["BAF"].AC_ChargeGroup].Add(1, OrgWithSource.New(supplierOrg, new List<string>(new[] { "Test" })));
			criteria.Creditors[Helper.ChargeCodes["FRT"].AC_ChargeGroup].Add(1, OrgWithSource.New(supplierOrg, new List<string>(new[] { "Test" })));

			var logger = new LoggerDecorator();

			using (_Rating.Start(logger))
			{
				_Rating.StartCost();

				var filteredEntries = Filter(new[] { rateHeader }, criteria, true, logger: logger);
				AssertEquals("Exactly 1 rate should be found", 1, filteredEntries.Count);
				AssertEquals(
					"ServiceProvider filter should only filter 1 as the zone filter removed the rest",
					true,
					logger.GetLogs().Any(x => x == "Information: RateEntry Filtered Costing TESTORG1 reason: Service Provider didn't match job TESTORG2."));
			}
		}

		#endregion

		public void TestMatchingLocation_LastRouteSetDischargeFilter()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryAUMEL = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryAUMEL.TI_LastRouteSetDischargePortLRC = "AUMEL";

			var entryUSLAX = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryUSLAX.TI_LastRouteSetDischargePortLRC = "USLAX";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.SetLastRouteSetDischarge(LocationHelper.GetCachedLocationFromString("AUMEL", Factory));

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { entryEmpty.PK, entryAUMEL.PK };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with wrong LastRouteSetDischarge value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingReefer_IsNonOperatedReeferFilter()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryYes = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryYes.TI_IsNonOperatedReefer = "Y";

			var entryNo = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			entryNo.TI_IsNonOperatedReefer = "N";

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_IsNonOperativeReefer = true;

			var shipment = consol.Shipments.AddNew();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			var shipmentPackLine = shipment.OuterPackLines.AddNew();
			shipmentPackLine.JL_JC = container.PK;

			FreightRatingHelper.SetShippingContainers(TestCriteria.RateableMeasures, shipment.Containers, shipment);

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries
					.Select(x => GetYesNoBlank(x, empty: entryEmpty, yes: entryYes, no: entryNo))
					.ToArray();
				var expected = new[] { "Blank", "Yes" };

				AssertContainsExactElementsInAnyOrder(
					"Should filter the entry with wrong IsNonOperatedReefer container value.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingReefer_IsNonOperatedReeferFilterAllEntries()
		{
			var standardCosting = Helper.NewCosting(null);

			var entryEmpty = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");

			var entryYes = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 20m, currency: "USD");
			entryYes.TI_IsNonOperatedReefer = "Y";

			var entryNo = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 30m, currency: "USD");
			entryNo.TI_IsNonOperatedReefer = "N";

			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			container.JC_IsNonOperativeReefer = true;
			var container2 = consol.Containers.AddNew();
			container2.JC_IsNonOperativeReefer = false;

			var shipment = consol.Shipments.AddNew();
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			var shipmentPackLine = shipment.OuterPackLines.AddNew();
			shipmentPackLine.JL_JC = container.PK;
			var shipmentPackLine2 = shipment.OuterPackLines.AddNew();
			shipmentPackLine2.JL_JC = container2.PK;

			FreightRatingHelper.SetShippingContainers(TestCriteria.RateableMeasures, shipment.Containers, shipment);

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);

				var actual = filteredEntries
					.Select(x => GetYesNoBlank(x, empty: entryEmpty, yes: entryYes, no: entryNo))
					.ToArray();

				var expected = new[] { "Blank", "Yes", "No" };

				AssertContainsExactElementsInAnyOrder(
					"Should select all entries because all IsNonOperatedReefer container values used.",
					expected,
					actual
				);
			}
		}

		public void TestMatchingPostcode_ThreeAndMoreCharactersSubstring_Matched()
		{
			var standardCosting = Helper.NewCosting(null);
			var rate1 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate1.TI_CartageDeliveryAddressPostCode = "1";
			rate1.TI_CartagePickupAddressPostCode = "1";
			var rate2 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate2.TI_CartageDeliveryAddressPostCode = "12";
			rate2.TI_CartagePickupAddressPostCode = "12";
			var rate3 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate3.TI_CartageDeliveryAddressPostCode = "123";
			rate3.TI_CartagePickupAddressPostCode = "123";
			var rate4 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate4.TI_CartageDeliveryAddressPostCode = "1234";
			rate4.TI_CartagePickupAddressPostCode = "1234";
			var rate5 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate5.TI_CartageDeliveryAddressPostCode = "12345";
			rate5.TI_CartagePickupAddressPostCode = "12345";

			var anOrgWithAddress = Factory.NewWithValidTestData<OrgHeader>();
			anOrgWithAddress.MainAddress.Postcode = "12345";

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.PickupAddress = anOrgWithAddress.MainAddress;
				TestCriteria.DeliveryAddress = anOrgWithAddress.MainAddress;

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { rate3.PK, rate4.PK, rate5.PK };
				AssertContainsExactElementsInAnyOrder("only Zip3, Zip4 and Zip5 are acceptable values.", expected, actual);
			}
		}

		public void TestMatchingPostcode_ExactCharacters_Matched()
		{
			var standardCosting = Helper.NewCosting(null);
			var rate1 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate1.TI_CartageDeliveryAddressPostCode = "1";
			rate1.TI_CartagePickupAddressPostCode = "1";
			var rate2 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate2.TI_CartageDeliveryAddressPostCode = "12";
			rate2.TI_CartagePickupAddressPostCode = "12";
			var rate3 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate3.TI_CartageDeliveryAddressPostCode = "123";
			rate3.TI_CartagePickupAddressPostCode = "123";
			var rate5 = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rate5.TI_CartageDeliveryAddressPostCode = "12345";
			rate5.TI_CartagePickupAddressPostCode = "12345";

			var anOrgWithAddress = Factory.NewWithValidTestData<OrgHeader>();
			anOrgWithAddress.MainAddress.Postcode = "12";
			anOrgWithAddress.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Spain;

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.PickupAddress = anOrgWithAddress.MainAddress;
				TestCriteria.DeliveryAddress = anOrgWithAddress.MainAddress;

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var actual = filteredEntries.Select(x => x.PK).ToArray();
				var expected = new[] { rate2.PK };

				AssertContainsExactElementsInAnyOrder("should match exact post code.", expected, actual);
			}
		}

		public void TestMatchingPostcode_DoesNotStartWith_NotMatched()
		{
			var standardCosting = Helper.NewCosting(null);
			var rateMatchesEnd = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rateMatchesEnd.TI_CartageDeliveryAddressPostCode = "333";
			rateMatchesEnd.TI_CartagePickupAddressPostCode = "333";
			var rateMatchesStart = standardCosting.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UAIEV", "AUSYD", "BAF", 10m, currency: "USD");
			rateMatchesStart.TI_CartageDeliveryAddressPostCode = "123";
			rateMatchesStart.TI_CartagePickupAddressPostCode = "123";

			var anOrgWithAddress = Factory.NewWithValidTestData<OrgHeader>();
			anOrgWithAddress.MainAddress.Postcode = "12333";
			anOrgWithAddress.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.Spain;

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();

				TestCriteria.PickupAddress = anOrgWithAddress.MainAddress;
				TestCriteria.DeliveryAddress = anOrgWithAddress.MainAddress;

				var filteredEntries = Filter(new[] { standardCosting }, TestCriteria, false);
				var expected = new[] { rateMatchesStart.PK };
				var actual = filteredEntries.Select(x => x.PK).ToArray();

				AssertContainsExactElementsInAnyOrder("Should match start of post code.", expected, actual);
			}
		}

		static string GetYesNoBlank(IRateEntry entryToCheck, RateEntry empty = null, RateEntry yes = null, RateEntry no = null)
		{
			if (entryToCheck.PK == empty?.PK)
			{
				return "Blank";
			}
			if (entryToCheck.PK == yes?.PK)
			{
				return "Yes";
			}
			if (entryToCheck.PK == no?.PK)
			{
				return "No";
			}

			return "Unexpected value";
		}

		protected List<IRateEntry> Filter(
			IEnumerable<IRatingHeader> ratingHeaders,
			RatingCriteria criteria,
			bool isCosting,
			BusinessObjectFactory factory = null,
			ILogger logger = null,
			FreightAutoRater.RatesToFindEnum ratesToFind = FreightAutoRater.RatesToFindEnum.ActiveRates)
		{
			if (isCosting)
			{
				var loader = new CostRatesLoader(criteria.Factory, logger);
				var rates = loader.Load(criteria, ratingHeaders);
				return RateEntryFilter.Filter(criteria, true, rates, factory ?? Factory, logger ?? new DummyLogger(), ratesToFind).ToList();
			}
			else
			{
				var loader = new RevenueRatesLoader(criteria.Factory, logger);
				var rates = loader.Load(criteria, ratingHeaders);
				return RateEntryFilter.Filter(criteria, false, rates, factory ?? Factory, logger ?? new DummyLogger(), ratesToFind).ToList();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var rate = Helper.NewCosting(NewClient);
			rate.AddRateEntry("LCL", "LCL", "UAIEV", "AUSYD");

			TestRates = new[] { rate };
			TestCriteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LCL, 100, 5, NewClient);
		}

		IEnumerable<RatingHeader> TestRates;
		TestRatingCriteria TestCriteria;
	}
}

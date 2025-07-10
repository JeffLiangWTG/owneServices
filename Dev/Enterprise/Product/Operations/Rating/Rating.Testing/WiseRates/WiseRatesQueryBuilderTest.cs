using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.RatingTests.WiseRates;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Api.Model;
using static Enterprise.Core.Constants;
using MeasureType = Enterprise.Rating.Integration.MeasureType;
using RatesService = WiseRates.Api.Model;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Testing.WiseRates
{
	public class WiseRatesQueryBuilderTest : RatingTestCase
	{
		#region Origin / Destination

		public void TestBuild_Locations()
		{
			var criteria = ValidCriteria;
			criteria.Origin = LocationHelper.GetLocationFromString("AUSYD", Factory);
			criteria.Destination = LocationHelper.GetLocationFromString("UAIEV", Factory);

			var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

			var expectedQuery = new RatesService.RatesQuery
			{
				Origin = new[] { "AUSYD", "SYD", "AU", "OCEG" },
				Destination = new[] { "UAIEV", "IEV", "UA", "MEDG" }
			};

			AssertNullOrEmpty("error", error);

			AssertContainsExactElementsInAnyOrder(
				"Mismatch in the 'Origin' values",
				expectedQuery.Origin,
				query.Origin
			);

			AssertContainsExactElementsInAnyOrder(
				"Mismatch in the 'Destination' values",
				expectedQuery.Destination,
				query.Destination
			);
		}

		#endregion

		#region Containers

		public void TestBuild_CriteriaContainersWithRateClass_ConvertedToISOTypeGroups()
		{
			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container20GP.RC_ISOType = "20G0";
			container20GP.RC_HandlingRateClass = "20GP";
			container20GP.RC_FreightRateClass = "20TN";
			var expectedContainer20GP = new RatesService.RatesQueryContainer { Code = "20GP", ISOType = "20G0", ISOTypeGroups = new[] { "20TN", "20GP" } };

			var container20HC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC");
			container20HC.RC_ISOType = "22G0";
			container20HC.RC_HandlingRateClass = "AAAA";
			container20HC.RC_FreightRateClass = "BBBB";
			var expectedContainer20HC = new RatesService.RatesQueryContainer { Code = "20HC", ISOType = "22G0", ISOTypeGroups = null };

			var container20FR = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR");
			container20FR.RC_ISOType = "22G2";
			container20FR.RC_HandlingRateClass = "";
			container20FR.RC_FreightRateClass = "";
			var expectedContainer20FR = new RatesService.RatesQueryContainer { Code = "20FR", ISOType = "22G2", ISOTypeGroups = null };

			var billTo = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", 0, null, billTo);
			criteria.AdapterType = AdapterType.Consolidation;
			criteria.JobDatesProvider = new TestJobDatesProvider(Factory.NewWithValidTestData<DummyBusinessObject>(), new ZDateTime(2016, 08, 15), new ZDateTime(2016, 08, 04));
			criteria.OperationalJobCode = "S000234202";

			var testContainers = new TestContainers(Factory,
				container20GP.PK, 1,
				container20HC.PK, 1,
				container20FR.PK, 1
			);

			testContainers.PopulateContainerList(criteria.RateableMeasures);

			var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

			AssertNullOrEmpty("Error should be null or empty.", error);

			var expectedQueryContainers = new[]
			{
				$"{expectedContainer20GP.Code}|{expectedContainer20GP.ISOType}|{(expectedContainer20GP.ISOTypeGroups != null ? string.Join(",", expectedContainer20GP.ISOTypeGroups) : "")}",
				$"{expectedContainer20HC.Code}|{expectedContainer20HC.ISOType}|{(expectedContainer20HC.ISOTypeGroups != null ? string.Join(",", expectedContainer20HC.ISOTypeGroups) : "")}",
				$"{expectedContainer20FR.Code}|{expectedContainer20FR.ISOType}|{(expectedContainer20FR.ISOTypeGroups != null ? string.Join(",", expectedContainer20FR.ISOTypeGroups) : "")}",
			};

			var actualQueryContainers = query.Container
				.Select(c => $"{c.Code}|{c.ISOType}|{(c.ISOTypeGroups != null ? string.Join(",", c.ISOTypeGroups) : "")}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Query.Container should contain the expected containers.",
				expectedQueryContainers,
				actualQueryContainers
			);
		}

		public void TestBuild_CriteriaHasContainersWithoutISOTypesAndMappings_ReturnError()
		{
			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container20GP.RC_ISOType = null;

			var container40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container40GP.RC_ISOType = null;

			Factory.Save();

			var criteria = ValidCriteria;
			criteria.FreightMode = FreightMode.FCL;
			var measures = criteria.RateableMeasures;
			new TestContainers(Factory, "20GP", 3, "40GP", 2).PopulateContainerList(measures);

			var (_, ursQuery, legacyQuery, error) = GetQuery(criteria);

			AssertContains("None of Job's containers can be used for rates search", error);
		}

		public void TestBuild_CriteriaIsAirAndHasContainersWithoutISOTypesAndMappings_IncludeContainersCodeInTheQuery()
		{
			var container20GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container20GP.RC_ISOType = null;

			var container40GP = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container40GP.RC_ISOType = null;

			Factory.Save();

			var criteria = ValidCriteria;
			criteria.FreightMode = FreightMode.ULD;
			criteria.ContainerMode = "ULD";
			var measures = criteria.RateableMeasures;
			new TestContainers(Factory, "20GP", 3, "40GP", 2).PopulateContainerList(measures);

			var expectedQuery = new RatesService.RatesQuery
			{
				Container = new[]
				{
					new RatesService.RatesQueryContainer { Code = "20GP" },
					new RatesService.RatesQueryContainer { Code = "40GP" },
				}
			};

			var registryValue = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = TransportModes.Air,
					ContainerMode = ContainerModes.ULD,
					IsSubscriptionEnabled = true
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

				AssertNullOrEmpty("Error should be null or empty", error);

				var expectedContainers = expectedQuery.Container
					.Select(c => $"{c.Code}")
					.ToArray();

				var actualContainers = query.Container
					.Select(c => $"{c.Code}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder(
					"Query should contain the expected containers",
					expectedContainers,
					actualContainers
				);
			}
		}

		public void TestBuild_CriteriaHasNeitherAirNorSeaFreight_ReturnError()
		{
			var criteria = ValidCriteria;
			criteria.FreightMode = FreightMode.ROA;
			Assert(criteria.IsRoadFreight);

			var builder = new WiseRatesQueryBuilder(new TestLogger());
			var (_, error) = builder.Build(criteria);
			AssertContains("Job is neither Air Freight nor Sea Freight", error);
		}

		public void TestGetContainers_SeaTransportModeIsoCodeSpecified_ProduceContainersForQuery()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "20GP";
			container.RC_ISOType = "20G0";
			container.RC_IATARateClass = ZString.Empty;

			var actual = queryCreator.GetContainers(new[] { container }, new[] { TransportModes.Sea });
			var expected = new[] { "20GP|20G0" };

			var actualKeys = actual
				.Select(c => $"{c.Code}|{c.ISOType}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Sea TransportMode + container iso code is valid",
				expected,
				actualKeys
			);
		}

		public void TestGetContainers_SeaTransportModeIsoCodeUnspecified_ReturnError()
		{
			var logger = new TestLogger();
			var queryCreator = new WiseRatesQueryBuilder(logger);

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "20GP";
			container.RC_ISOType = null;

			var actual = queryCreator.GetContainers(new[] { container }, new[] { TransportModes.Sea });
			var expected = Array.Empty<RatesService.RatesQueryContainer>();

			AssertContainsExactElementsInAnyOrder(
				"Sea TransportMode with no container iso code is invalid",
				expected,
				actual
			);
		}

		public void TestGetContainers_AirTransportModeIataCodeSpecified_ProduceContainersForQuery()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "DPE";
			container.RC_IATARateClass = "8D";

			var actual = queryCreator.GetContainers(new[] { container }, new[] { TransportModes.Air });
			var expected = new[] { new RatesService.RatesQueryContainer { Code = "DPE", IATAULDRateClass = "8D" } };

			var actualConverted = actual
				.Select(c => $"{c.Code}|{c.IATAULDRateClass}")
				.ToArray();

			var expectedConverted = expected
				.Select(e => $"{e.Code}|{e.IATAULDRateClass}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("Air TransportMode + iata rate class is valid", expectedConverted, actualConverted);
		}

		public void TestGetContainers_AirTransportModeIataCodeUnspecified_ProduceContainersForQuery()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "DPE";
			container.RC_IATARateClass = null;

			var actual = queryCreator.GetContainers(new[] { container }, new[] { TransportModes.Air });
			var expected = new[] { "DPE" };

			var actualKeys = actual.Select(c => c.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Air TransportMode with no iata rate class is valid",
				expected,
				actualKeys
			);
		}

		public void TestGetContainers_TransportModeUnspecifiedIsoCodeSpecified_ProduceContainersForQuery()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "20GP";
			container.RC_ISOType = "20G0";
			container.RC_IATARateClass = ZString.Empty;

			var actual = queryCreator.GetContainers(new[] { container }, Array.Empty<string>())
				.Select(c => $"{c.Code}|{c.ISOType}")
				.ToArray();

			var expected = new[] { "20GP|20G0" };

			AssertContainsExactElementsInAnyOrder(
				"Unspecified TransportMode + isotype is valid",
				expected,
				actual
			);
		}

		public void TestGetContainers_TransportModeUnspecifiedIsoCodeUnspecified_ProduceContainersForQuery()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "DPE";
			container.RC_ISOType = null;
			container.RC_IATARateClass = ZString.Empty;

			var actual = queryCreator.GetContainers(new[] { container }, Array.Empty<string>())
				.Select(c => $"{c.Code}")
				.ToArray();
			var expected = new[] { "DPE" };

			AssertContainsExactElementsInAnyOrder(
				"Unspecified TransportMode with no isotype is valid",
				expected,
				actual
			);
		}

		public void TestGetContainers_SeaFreightIsoCodeSpecified_ProduceContainersForQuery()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "20GP";
			container.RC_ISOType = "20G0";
			container.RC_IATARateClass = ZString.Empty;

			var actual = queryCreator.GetContainers(new[] { container }, FreightMode.SEA);
			var expected = new[] { "20GP|20G0" };

			var actualKeys = actual
				.Select(c => $"{c.Code}|{c.ISOType}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Sea TransportMode + container iso code is valid",
				expected,
				actualKeys
			);
		}

		public void TestGetContainers_AirFreightIataCodeUnspecified_ProduceContainersForQuery()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "DPE";
			container.RC_IATARateClass = null;

			var actual = queryCreator.GetContainers(new[] { container }, FreightMode.AIR);
			var expected = new[] { new RatesService.RatesQueryContainer { Code = "DPE" } };

			var actualResult = actual.Select(c => $"{c.Code}").ToArray();
			var expectedResult = expected.Select(c => $"{c.Code}").ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Air TransportMode with no iata rate class is valid",
				expectedResult,
				actualResult
			);
		}

		public void TestGivenAirContainersWithISOType_WhenBuildQuery_ThenISOTypeShouldNotBeAddedToQueryMessage()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_ShippingMode = RefContainerLookups.ShippingModes.Air;
			container.RC_Code = "20GP";
			container.RC_ISOType = "20G0";
			container.RC_IATARateClass = ZString.Empty;

			var actual = queryCreator.GetContainers(new[] { container }, new[] { TransportModes.Air, TransportModes.Sea });
			var expected = new[] { "20GP|" };

			var actualKeys = actual
				.Select(c => $"{c.Code}|{c.ISOType}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"For Air Container, container ISOType should not be added into query content",
				expected,
				actualKeys
			);
		}

		public void TestGivenSeaContainersWithISOType_WhenBuildQuery_ThenISOTypeShouldBeAddedToQueryMessage()
		{
			var queryCreator = new WiseRatesQueryBuilder(new TestLogger());

			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;
			container.RC_Code = "20GP";
			container.RC_ISOType = "20G0";
			container.RC_IATARateClass = ZString.Empty;

			var actual = queryCreator.GetContainers(new[] { container }, new[] { TransportModes.Air, TransportModes.Sea })
				.Select(c => $"{c.Code}|{c.ISOType}")
				.ToArray();

			var expected = new[] { "20GP|20G0" };

			AssertContainsExactElementsInAnyOrder(
				"For Sea Container, container ISOType should be added into query content",
				expected,
				actual
			);
		}

		#endregion

		#region Carrier

		public void TestBuild_CriteriaHasCarriers_IncludeCarriersInTheQuery()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "EMR";
			carrier1.OH_FullName = "Emirates Airlines";
			carrier1.OH_IsShippingProvider = true;
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "EMMR";
			shippingLine1.RSL_CargoWiseOneCode = "xxxx";
			carrier1.OH_RSL_ShippingLine = shippingLine1.PK;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_FullName = "Qantas Airlines";
			carrier2.OH_IsShippingProvider = true;
			var shippingLine2 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine2.RSL_StandardCarrierAlphaCode = "";
			shippingLine2.RSL_CargoWiseOneCode = "C1QQ";
			carrier2.OH_RSL_ShippingLine = shippingLine2.PK;

			var sq = RefAirline.LoadFromAirline2LetterCode(Factory, "SQ");
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_Code = "SQ";
			carrier3.OH_FullName = "Singapore Airlines";
			carrier3.OH_IsShippingProvider = true;
			carrier3.MiscServ.OM_RM_Airline = sq.PK;

			var creditorNonCarrier = Factory.NewWithValidTestData<OrgHeader>();
			creditorNonCarrier.OH_Code = "NONCRED";
			creditorNonCarrier.OH_FullName = "Creditor";
			creditorNonCarrier.OH_IsShippingProvider = false;

			Factory.Save();

			var criteria = ValidCriteria;
			criteria.Creditors = Creditors.New(
				OrgWithSource.New(carrier1, new List<string> { "Consol" }),
				OrgWithSource.New(carrier2, new List<string> { "Consol" }),
				OrgWithSource.New(carrier3, new List<string> { "Consol" }),
				OrgWithSource.New(creditorNonCarrier, new List<string> { "Consol" }));

			var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

			var expectedQuery = new RatesService.RatesQuery
			{
				Carrier = new[]
				{
					new RatesService.RatesQueryCarrier { SCACCode = "EMMR", C1Code = "xxxx", Source = "Consol", Name = "Emirates Airlines" },
					new RatesService.RatesQueryCarrier { C1Code = "C1QQ", Source = "Consol", Name = "Qantas Airlines" },
					new RatesService.RatesQueryCarrier { Code = null,  IATACode = "SQ", Source = "Consol", Name = "Singapore Airlines" }
				}
			};

			AssertNullOrEmpty("Error should be null or empty if the query built successfully", error);

			var actualQueryAsString = query.Carrier?.Select(c => $"{c.SCACCode}|{c.C1Code}|{c.Code}|{c.IATACode}|{c.Source}|{c.Name}").ToArray();
			var expectedQueryAsString = expectedQuery.Carrier?.Select(c => $"{c.SCACCode}|{c.C1Code}|{c.Code}|{c.IATACode}|{c.Source}|{c.Name}").ToArray();

			AssertContainsExactElementsInAnyOrder(
				"The assembled query should match the expected carriers",
				expectedQueryAsString,
				actualQueryAsString
			);
		}

		public void TestBuild_CriteriaHasCarriersWithInsufficientInfo_ReturnError()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "EMR";
			carrier1.OH_FullName = "Emirates Airlines";
			carrier1.OH_IsShippingProvider = true;

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_FullName = "Qantas Airlines";
			carrier2.OH_IsShippingProvider = true;

			Factory.Save();

			var criteria = ValidCriteria;
			criteria.Creditors = Creditors.New(
				OrgWithSource.New(carrier1, new List<string> { "Consol" }),
				OrgWithSource.New(carrier2, new List<string> { "Consol" }));

			var (_, ursQuery, legacyQuery, error) = GetQuery(criteria);

			AssertContains(
				"None of Job's Service Providers/Creditors can be used for rates search",
				error
			);
		}

		#endregion

		#region Start/End Date

		public void TestBuild_CriteriaHasFreightDate_IncludeAsStartAndEndDate()
		{
			var arrivalDate = new DateTime(2020, 08, 15);
			var departureDate = new DateTime(2020, 08, 04);

			var jobDatesProviderMock = new Mock<IJobDatesProvider>();
			jobDatesProviderMock
				.Setup(p => p.GetJobDateByType(It.Is<ZString>(s => s == JobDateTypes.Codes.ArrivalDate), It.IsAny<string>()))
				.Returns(arrivalDate);
			jobDatesProviderMock
				.Setup(p => p.GetJobDateByType(It.Is<ZString>(s => s == JobDateTypes.Codes.DepartureDate), It.IsAny<string>()))
				.Returns(departureDate);

			var criteria = ValidCriteria;
			criteria.JobDatesProvider = jobDatesProviderMock.Object;

			using (AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				new AutoRateDateByChargeGroupConfiguration
				{
					FilterType = RatingDateFilterTypes.Codes.Arrival
				}))
			{
				var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

				AssertNullOrEmpty(nameof(error), error);
				AssertEquals(nameof(query.EffectiveDate), arrivalDate, query.EffectiveDate);
			}

			using (AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty,
				new AutoRateDateByChargeGroupConfiguration
				{
					FilterType = RatingDateFilterTypes.Codes.Departure
				}))
			{
				var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

				AssertNullOrEmpty(nameof(error), error);
				AssertEquals(nameof(query.EffectiveDate), departureDate, query.EffectiveDate);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestBuild_CriteriaHasNoFreightDate_EffectiveOnIsToday()
		{
			var criteria = ValidCriteria;
			criteria.JobDatesProvider = new Mock<IJobDatesProvider>().Object;

			var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

			AssertNullOrEmpty("Error should be null or empty when no freight date is present.", error);
			AssertEquals("Effective date should be today's date when no freight date is provided.", ZDateTime.Today.ToDateTime(), query.EffectiveDate);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetEffectiveDateFromConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Empty;
			consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Empty;
			var autoRating = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			AssertEquals(
				"Expected the effective date to match ZDateTime.Empty when ETD and ETA are empty.",
				ZDateTime.Empty,
				WiseRatesQueryBuilder.GetEffectiveDate(criteria)
			);

			var testDate = ZDateTime.Today.AddDays(1);
			consol.Transports.MostInterestingTransport.JW_ETD = testDate;

			AssertEquals(
				"Expected the effective date to match the test date when only ETD is set.",
				testDate,
				WiseRatesQueryBuilder.GetEffectiveDate(criteria)
			);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetEffectiveDateFromConsol_WhenAutoratingDateIsOverridden()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Empty;
			consol.Transports.MostInterestingTransport.JW_ETA = ZDateTime.Empty;
			var autoRating = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);
			AssertEquals(
				"Effective date should be empty when all relevant dates are empty.",
				ZDateTime.Empty,
				WiseRatesQueryBuilder.GetEffectiveDate(criteria)
			);

			consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Today;
			var testDate = ZDate.Today.AddDays(1);
			consol.AutoratingDate = testDate;
			AssertEquals(
				"AutoratingDate should override ETD.",
				testDate,
				WiseRatesQueryBuilder.GetEffectiveDate(criteria)
			);
		}

		[TestDate(2020, 01, 01)]
		public void TestGetEffectiveDateFromServiceProvider_WhenRateSelectorHasOneServiceProvider()
		{
			var arrivalDate = new DateTime(2022, 08, 15);

			var jobDatesProviderMock = new Mock<IJobDatesProvider>();
			jobDatesProviderMock
				.Setup(p => p.GetJobDateByType(It.Is<ZString>(s => s == JobDateTypes.Codes.ArrivalDate), It.IsAny<string>()))
				.Returns(arrivalDate);

			var criteria = ValidCriteria;
			(criteria as TestRatingCriteria).SetShouldUseCarrierContractDateFilter(true);
			criteria.JobDatesProvider = jobDatesProviderMock.Object;

			TransportProvider1.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			var ratingDateConfig1 = Factory.New<RatingDateConfig>();
			ratingDateConfig1.RDT_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			ratingDateConfig1.RDT_ParentID = TransportProvider1.PK;
			ratingDateConfig1.RDT_TransportMode = TransportModes.Sea;
			ratingDateConfig1.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig1.RDT_JobType = "ALL";
			ratingDateConfig1.RDT_Direction = FreightShipmentDirection.Code.All;
			ratingDateConfig1.RDT_RateType = JobRateTypes.Codes.All;
			ratingDateConfig1.RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;

			var effectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(criteria, carriersFromFilters: new OrgWithSource[] { OrgWithSource.New(TransportProvider1, new List<string>() { "Provider" }) });
			AssertEquals("The effective date should match the arrival date.", arrivalDate, effectiveDate);

			criteria.Creditors = Creditors.New(OrgWithSource.New(TransportProvider1, new List<string> { "Consol" }));

			var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

			AssertNullOrEmpty("There should be no error when building the query with one service provider.", error);
			AssertEquals("The query's effective date should match the arrival date.", arrivalDate, query.EffectiveDate);

			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "EMR";
			carrier1.OH_FullName = "Emirates Airlines";
			carrier1.OH_IsShippingProvider = true;
			var shippingLine1 = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine1.RSL_StandardCarrierAlphaCode = "EMMR";
			shippingLine1.RSL_CargoWiseOneCode = "xxxx";
			carrier1.OH_RSL_ShippingLine = shippingLine1.PK;

			criteria.Creditors = Creditors.New(OrgWithSource.New(TransportProvider1, new List<string> { "Consol" }),
				OrgWithSource.New(carrier1, new List<string> { "Consol" }));

			(query, ursQuery, legacyQuery, error) = GetQuery(criteria);
			AssertNullOrEmpty("There should be no error when building the query with multiple service providers.", error);
			AssertEquals(
				"We only use the service provider's autorating date if there is just one service provider.",
				ZDateTime.Today.ToDateTime(),
				query.EffectiveDate
			);
		}

		public void TestGetEffectiveDateFromConsol_CustomDatePriority_FoundOnJob()
		{
			using (SetupAutoRateDateConfiguration(JobDateTypes.Codes.LastContainerGateInDate))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.BrettsBirthday.AddMonths(1);
				var container = consol.Containers.AddNew();
				container.JC_FCLWharfGateIn = ZDateTime.BrettsBirthday;
				var autoRating = new AutoRatingProxy(consol.RatingAdapter);
				var criteria = new RatingCriteria(autoRating, Factory);
				var effectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(criteria);

				AssertEquals("The effective date should match Brett's birthday.", ZDateTime.BrettsBirthday, effectiveDate);
			}
		}

		public void TestGetEffectiveDateFromConsol_CustomDatePriority_NotFound_NoFallback()
		{
			using (SetupAutoRateDateConfiguration(JobDateTypes.Codes.LastContainerGateInDate, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.BrettsBirthday.AddMonths(1);
				var container = consol.Containers.AddNew();
				container.JC_FCLWharfGateIn = ZDateTime.Empty;

				var autoRating = new AutoRatingProxy(consol.RatingAdapter);
				var criteria = new RatingCriteria(autoRating, Factory);
				var actualEffectiveDate = WiseRatesQueryBuilder.GetEffectiveDate(criteria);

				AssertEquals("Expected effective date should be ZDateTime.Empty", ZDateTime.Empty, actualEffectiveDate);
			}
		}

		public void TestGetEffectiveDateFromConsol_CustomDatePriority_NotFound_FallbackToStandard()
		{
			using (SetupAutoRateDateConfiguration(JobDateTypes.Codes.LastContainerGateInDate, false))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Transports.MostInterestingTransport.JW_ETD = ZDateTime.BrettsBirthday.AddMonths(1);
				var container = consol.Containers.AddNew();
				container.JC_FCLWharfGateIn = ZDateTime.Empty;

				var autoRating = new AutoRatingProxy(consol.RatingAdapter);
				var criteria = new RatingCriteria(autoRating, Factory);
				var expectedDate = ZDateTime.BrettsBirthday.AddMonths(1);
				var actualDate = WiseRatesQueryBuilder.GetEffectiveDate(criteria);

				AssertEquals("The effective date should match the expected fallback date.", expectedDate, actualDate);
			}
		}

		public void TestGetEffectiveDateFromForwardingShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var autoRating = new AutoRatingProxy(shipment.RatingAdapter);
			var criteria = new RatingCriteria(autoRating, Factory);

			shipment.JS_E_DEP = ZDateTime.Empty;
			shipment.JS_E_ARV = ZDateTime.Empty;
			AssertEquals("Effective date should match the expected empty date", ZDateTime.Empty, WiseRatesQueryBuilder.GetEffectiveDate(criteria));

			var testDate = ZDateTime.Today.AddDays(1);
			shipment.JS_E_DEP = testDate;
			AssertEquals("Effective date should match the expected test date", testDate, WiseRatesQueryBuilder.GetEffectiveDate(criteria));
		}

		public void TestGetEffectiveDateFromBookingWithQuote()
		{
			var quotePK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
			var bookingPK = QuotedBooking.CreateNewBooking(Factory).PK;
			var quotedBooking = QuotedBooking.New(quotePK, bookingPK, Factory);
			var autoRating = new AutoRatingProxy(quotedBooking.GetFirstAdapter());
			var criteria = new RatingCriteria(autoRating, Factory);

			quotedBooking.StartDate = ZDateTime.Empty;
			quotedBooking.EndDate = ZDate.Empty;
			quotedBooking.Booking.JS_E_DEP = ZDateTime.Empty;
			quotedBooking.Booking.JS_E_ARV = ZDate.Empty;
			AssertEquals(
				"The effective date should match the empty date when all relevant fields are empty.",
				ZDateTime.Empty,
				WiseRatesQueryBuilder.GetEffectiveDate(criteria)
			);

			var testDate = ZDateTime.Today.AddDays(1);
			quotedBooking.Booking.JS_E_DEP = testDate;
			AssertEquals(
				"The effective date should match the departure date when it is set.",
				testDate,
				WiseRatesQueryBuilder.GetEffectiveDate(criteria)
			);

			testDate = ZDateTime.Today.AddDays(2);
			quotedBooking.Booking.JS_E_DEP = ZDateTime.Empty;
			quotedBooking.ETD = testDate;
			AssertEquals(
				"The effective date should match the ETD date when departure date is empty and ETD is set.",
				testDate,
				WiseRatesQueryBuilder.GetEffectiveDate(criteria)
			);
		}

		public void TestGetEffectiveDateFromOneOffQuote()
		{
			var quotePK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK;
			var oneOffQuote = QuotedBooking.New(quotePK, ZGuid.Empty, Factory);
			var autoRating = new AutoRatingProxy(oneOffQuote.GetFirstAdapter());
			var criteria = new RatingCriteria(autoRating, Factory);

			oneOffQuote.StartDate = ZDateTime.Empty;
			oneOffQuote.EndDate = ZDate.Empty;
			AssertEquals("The effective date should be ZDateTime.Empty when start and end dates are empty.", ZDateTime.Empty, WiseRatesQueryBuilder.GetEffectiveDate(criteria));

			var testDate = ZDateTime.Today.AddDays(1);
			oneOffQuote.StartDate = testDate;
			AssertEquals("The effective date should match the start date when it is provided.", testDate, WiseRatesQueryBuilder.GetEffectiveDate(criteria));
		}

		[TestDate(2023, 01, 01)]
		public void TestBuild_CriteriaHasContract_UseContractDate()
		{
			var contractOrg = Helper.NewOrgHeader();
			var someRandomOrg = Helper.NewOrgHeader();
			var ratingContract = Helper.NewRatingContract(contractOrg, "C1234", RatingContractTypes.Provider, transportMode: TransportModes.Sea);
			ratingContract.RCT_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;
			Helper.Factory.Save();

			var arrivalDate = new DateTime(2022, 08, 15);
			var lastContainerGateInDateDate = new DateTime(2020, 08, 04);

			var jobDatesProviderMock = new Mock<IJobDatesProvider>();
			jobDatesProviderMock
				.Setup(p => p.GetJobDateByType(It.Is<ZString>(s => s == JobDateTypes.Codes.ArrivalDate), It.IsAny<string>()))
				.Returns(arrivalDate);
			jobDatesProviderMock
				.Setup(p => p.GetJobDateByType(It.Is<ZString>(s => s == JobDateTypes.Codes.LastContainerGateInDate), It.IsAny<string>()))
				.Returns(lastContainerGateInDateDate);

			var criteria = ValidCriteria;
			(criteria as TestRatingCriteria).SetShouldUseCarrierContractDateFilter(true);
			criteria.JobDatesProvider = jobDatesProviderMock.Object;

			var builder = new WiseRatesQueryBuilder(new TestLogger());

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractTariffsAndRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (query, error) = builder.Build(criteria, contractNumbersFromFilters: new string[] { "C1234" });
				var today = ZDateTime.Today.ToDateTime();
				var serviceProviderSource = OrgWithSource.New(contractOrg, new List<string>() { "Provider" });
				var randomProviderSOurce = OrgWithSource.New(someRandomOrg, new List<string>() { "Random Provider" });

				AssertNullOrEmpty(nameof(error), error);
				AssertEquals("Because there is no service provider", today, query.EffectiveDate);

				(query, error) = builder.Build(criteria, contractNumbersFromFilters: new string[] { "C1234" },
					carriersFromFilters: new OrgWithSource[] { randomProviderSOurce });

				AssertNullOrEmpty(nameof(error), error);
				AssertEquals("Because contract does not belong to service provider", today, query.EffectiveDate);

				(query, error) = builder.Build(criteria, contractNumbersFromFilters: new string[] { "C1234" },
					carriersFromFilters: new OrgWithSource[] { serviceProviderSource });

				AssertNullOrEmpty(nameof(error), error);
				AssertEquals("Because contract belongs to service provider", arrivalDate, query.EffectiveDate);

				(query, error) = builder.Build(criteria, contractNumbersFromFilters: new string[] { "C1234" },
																	carriersFromFilters: new OrgWithSource[] { serviceProviderSource, randomProviderSOurce });

				AssertNullOrEmpty(nameof(error), error);
				AssertEquals("Because there are more than one service provider", today, query.EffectiveDate);

				ratingContract.RCT_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;
				var ratingDateConfig1 = Helper.Factory.New<RatingDateConfig>();
				ratingDateConfig1.RDT_ParentTableCode = RatingContractSchema.Constants.Prefix;
				ratingDateConfig1.RDT_ParentID = ratingContract.PK;
				ratingDateConfig1.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
				ratingDateConfig1.RDT_JobType = "ALL";
				ratingDateConfig1.RDT_Direction = FreightShipmentDirection.Code.All;
				ratingDateConfig1.RDT_TransportMode = TransportModes.Sea;
				ratingDateConfig1.RDT_RateType = JobRateTypes.Codes.All;
				ratingDateConfig1.RDT_AutoratingDate = JobDateTypes.Codes.LastContainerGateInDate;

				Helper.Factory.Save();

				(query, error) = builder.Build(criteria, contractNumbersFromFilters: new string[] { "C1234" },
														 carriersFromFilters: new OrgWithSource[] { serviceProviderSource });

				AssertNullOrEmpty(nameof(error), error);
				AssertEquals("Because contract belongs to service provider and has FRT custom date", lastContainerGateInDateDate, query.EffectiveDate);
			}
		}

		#endregion

		#region Contract Numbers

		public void TestBuild_CriteriaHasContractNumbers_IncludeNumbersTheQuery()
		{
			var criteria = ValidCriteria;
			criteria.CarrierContractNumbers = new ZString[] { "McLaren", "MU" };

			var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

			AssertNullOrEmpty(nameof(error), error);

			var expectedContracts = new[]
			{
				"McLaren",
				"MU"
			};

			var actualContracts = query.Contract
				.Select(c => c.ContractNumber)
				.ToArray();

			AssertContainsExactElementsInAnyOrder(expectedContracts, actualContracts);
		}

		public void TestBuild_CriteriaHasOnlyBlankContractNumber_DoNotIncludeNumbersTheQuery()
		{
			var criteria = ValidCriteria;
			criteria.CarrierContractNumbers = new ZString[] { "" };

			var builder = new WiseRatesQueryBuilder(new TestLogger());
			var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

			AssertNullOrEmpty("Error should be null or empty when criteria has only blank contract numbers", error);
			AssertEquals("Query contract should be empty when criteria has only blank contract numbers", 0, query.Contract.Count());
		}

		#endregion

		#region Registry Validation

		public void TestBuild_DisabledRegistry()
		{
			var criteria = ValidCriteria;
			criteria.ContainerMode = ContainerModes.Loose;
			criteria.FreightMode = FreightMode.AIR;

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				var builder = new WiseRatesQueryBuilder(new TestLogger());
				var (_, error) = builder.Build(criteria);
				AssertContains(
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service Subscription",
					error
				);
			}
		}

		public void TestBuild_EnabledRegistry()
		{
			var criteria = ValidCriteria;
			criteria.ContainerMode = ContainerModes.BuyersConsol;

			var enableSubscriptionCollection = new RatesServiceRegistrySettingsCollection
			{
				new RatesServiceRegistrySettings
				{
					TransportMode = TransportModes.Sea,
					ContainerMode = ContainerModes.BuyersConsol,
					IsSubscriptionEnabled = true
				}
			};

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSubscriptionCollection))
			{
				var (query, ursQuery, legacyQuery, error) = GetQuery(criteria);

				var expectedQuery = new RatesService.RatesQuery
				{
					ContainerMode = new[] { "FCL" },
					TransportMode = new[] { "SEA" }
				};

				AssertNullOrEmpty(nameof(error), error);

				var expectedContainerMode = expectedQuery.ContainerMode.Select(cm => cm);
				var actualContainerMode = query.ContainerMode.Select(cm => cm);

				var expectedTransportMode = expectedQuery.TransportMode.Select(tm => tm);
				var actualTransportMode = query.TransportMode.Select(tm => tm);

				AssertContainsExactElementsInAnyOrder("The container modes should match expected", expectedContainerMode, actualContainerMode);
				AssertContainsExactElementsInAnyOrder("The transport modes should match expected", expectedTransportMode, actualTransportMode);
			}
		}

		#endregion

		public void TestAddMeasureChargeableVolume()
		{
			var criteria = new TestRatingCriteria();
			criteria.RateableMeasures.SetQuantity(MeasureType.Chargeable, 5, Volume.CubicMetres);

			var builder = new WiseRatesQueryBuilder(new TestLogger());
			var query = new RatesService.RatesQuery();
			builder.AddMeasureChargeableVolume(query, criteria);

			var volumeMeasure = query.Measures?.FirstOrDefault(x => x.Type == RatesService.MeasureType.Volume);
			AssertNotNull(nameof(volumeMeasure), volumeMeasure);

			var expectedMeasure = $"{(int)RatesService.MeasureType.Volume}|5|{Volume.CubicMetres}";
			var actual = $"{(int)volumeMeasure.Type}|{volumeMeasure.Amount}|{volumeMeasure.Unit}";

			AssertEquals(
				"The volume measure added should match the expected measure",
				expectedMeasure,
				actual
			);
		}

		public void TestAddMeasureChargeableVolumeWhenNotExistsInCriteria()
		{
			var criteria = new TestRatingCriteria();
			var builder = new WiseRatesQueryBuilder(new TestLogger());
			var query = new RatesService.RatesQuery();
			builder.AddMeasureChargeableVolume(query, criteria);
			AssertNull(query.Measures);

			// Set a weight instead of volume
			criteria.RateableMeasures.SetQuantity(MeasureType.Chargeable, 5, Weight.Kilograms);
			builder.AddMeasureChargeableVolume(query, criteria);
			AssertNull(query.Measures);
		}

		IDisposable SetupAutoRateDateConfiguration(string dateType, bool isFallbackDisabled = false)
		{
			var customAutoRateDate = new AutoRateDate
			{
				JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code,
				Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All,
				DirectionCode = FreightShipmentDirection.Code.All,
				DateType = dateType,
				IsFallbackDisabled = isFallbackDisabled
			};
			var configuration = new AutoRateDateByChargeGroupConfiguration
			{
				FilterType = RatingDateFilterTypes.Codes.Custom,
			};
			var freightChargeGroup = configuration.AutoRateDateByChargeGroups.Cast<AutoRateDateByChargeGroup>()
					.First(item => item.ChargeGroup == ChargeCodeGroupList.Codes.Freight);
			freightChargeGroup.ChargeGroupSettings.Add(customAutoRateDate);

			return AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configuration);
		}

		(RatesQuery wiseQuery, QueryRequestDto ursQuery, QueryRequestDto legacyQuery, string error) GetQuery(RatingCriteria criteria)
		{
			var queryBuilder = new UrsRatesQueryBuilder(new TestLogger());
			var builder = new WiseRatesQueryBuilder(new TestLogger());
			var ursWiseRatesConverter = new UrsWiseRatesConverter(new TestLogger());

			var (ursQuery, ursError) = queryBuilder.Build(criteria);
			var (wiseQuery, wiseError) = builder.Build(criteria);
			var rateSearchRequest = new RatesSearchRequest
			{
				RatesQuery = wiseQuery,
				ContextOperation = RatesSearchRequest.Operation.Autorating
			};
			var legacyUrsQuery = ursWiseRatesConverter.Map(rateSearchRequest);

			AssertEquals("Wise and Urs Error should be same", wiseError, ursError);

			return (wiseQuery, ursQuery, legacyUrsQuery, wiseError);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var billTo = Factory.NewWithValidTestData<OrgHeader>();

			ValidCriteria = new TestRatingCriteria("AUSYD", "UAIEV", 2, container, billTo);
			ValidCriteria.AdapterType = AdapterType.Consolidation;
			ValidCriteria.JobDatesProvider = new TestJobDatesProvider(Factory.NewWithValidTestData<DummyBusinessObject>(), new ZDateTime(2016, 08, 15), new ZDateTime(2016, 08, 04));
			ValidCriteria.OperationalJobCode = "S000234202";

			CodeMappingsOrg = Factory.NewWithValidTestData<OrgHeader>();

			currentUserEmail = GlbStaff.CurrentUser.GS_EmailAddress;
		}

		protected override void TearDown()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = currentUserEmail;
		}

		protected RatingCriteria ValidCriteria { get; private set; }

		protected OrgHeader CodeMappingsOrg { get; private set; }
		ZString currentUserEmail;
	}
}

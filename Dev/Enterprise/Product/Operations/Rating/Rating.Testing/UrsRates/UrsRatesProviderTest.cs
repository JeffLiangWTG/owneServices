using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.WiseRates;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Urs.Api.Integration;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.DTOs.Request;
using static Enterprise.Rating.Business.UrsConstants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.RatingTests.UrsRates
{
	public class UrsRatesProviderTest : TestCaseWithFactory
	{
		public void TestGetCostRateEntries_CriteriaIsServiceOnly_ReturnsAnEmptyEnumerationWithoutSearching()
		{
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var billTo = Factory.NewWithValidTestData<OrgHeader>();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", 2, container, billTo)
			{
				AdapterType = AdapterType.Consolidation,
				JobDatesProvider = new TestJobDatesProvider(Factory.NewWithValidTestData<DummyBusinessObject>(),
					new ZDateTime(2024, 01, 15), new ZDateTime(2024, 01, 10)),
				IsServicesOnly = true
			};

			var numberOfCallsToUrsClient = 0;
			var clientMock = new Mock<IUrsClient>();
			clientMock
				.Setup(c => c.GetTradeServicesAsync(
					It.IsAny<QueryRequestDto>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Callback<QueryRequestDto, string, CancellationToken>((a, b, c) => numberOfCallsToUrsClient++);

			var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(), // transportMode
					It.IsAny<string>(), // containerMode
					It.IsAny<string>(), // requestID
					It.IsAny<ILogger>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<CancellationToken>()))
				.Returns(clientMock.Object);

			var logger = new TestLogger();
			var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

			var entries = provider.GetCostRateEntries(criteria);

			AssertEquals(0, entries.Count());
			AssertEquals(0, numberOfCallsToUrsClient);
			Assert(!logger.Errors.Any());
		}

		public void TestGetCostRateEntries_UrsClientFactoryReturnsNull_ReturnsEmptyCollection()
		{
			var criteria = DefaultCriteria;
			var logger = new TestLogger();

			var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<ILogger>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<CancellationToken>()))
				.Returns((IUrsClient)null);

			var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

			var rates = provider.GetCostRateEntries(criteria);

			AssertEquals(0, rates.Count());
			clientFactoryMock.VerifyAll();
		}

		public void TestGetCostRateEntries_QueryBuilderReturnsError_LogsWarningAndReturnsEmpty()
		{
			var criteria = DefaultCriteria;
			// Create invalid criteria by setting FreightMode to UKN
			criteria.FreightMode = FreightMode.UKN;
			var invalidCriteria = new TestRatingCriteria("AUSYD", "USLAX", 2,
				Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP"),
				Factory.NewWithValidTestData<OrgHeader>())
			{
				AdapterType = criteria.AdapterType,
				JobDatesProvider = criteria.JobDatesProvider,
				OperationalJobCode = criteria.OperationalJobCode,
				FreightMode = criteria.FreightMode,
				ContainerMode = criteria.ContainerMode
			};

			var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
			var logger = new TestLogger();
			var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

			var entries = provider.GetCostRateEntries(invalidCriteria);

			AssertEquals(0, entries.Count());
			AssertCollectionContains(
				"Warning:Searching from URS cannot proceed - Job is neither Air Freight nor Sea Freight\r\nRequest will not be sent to Rates Service because Rates Service subscription is disabled in the registry for -FCL: AutoRating -> Rates Service -> Rates Service Subscription",
				logger.Warnings);
		}

		public void TestGetCostRateEntries_ClientThrowsException_HandledGracefully()
		{
			var clientMock = new Mock<IUrsClient>();
			clientMock
				.Setup(c => c.GetTradeServicesAsync(
					It.IsAny<QueryRequestDto>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("URS API error"));

			var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<ILogger>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<CancellationToken>()))
				.Returns(clientMock.Object);

			var logger = new TestLogger();
			var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

			var entries = provider.GetCostRateEntries(DefaultCriteria);

			AssertEquals(0, entries.Count());
			AssertEquals("URS API error", ErrorReporter.LastExceptionReported.Message);
			AssertCollectionContains(
				"Warning:Error connecting URS with report(s) sent to WTG. Please raise an eRequest accordingly.",
				logger.Warnings);
			ErrorReporter.Clear();
		}

		public void TestGetCostRateEntries_Timeout_HandledProperly()
		{
			using (DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var clientMock = new Mock<IUrsClient>();
				clientMock
					.Setup(c => c.GetTradeServicesAsync(
						It.IsAny<QueryRequestDto>(),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()))
					.Returns((QueryRequestDto q, string id, CancellationToken token) =>
					{
						// to simulate timeout scenario consistently
						throw new OperationCanceledException("The operation was canceled.", token);
					});

				var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
				clientFactoryMock
					.Setup(f => f.TryCreate(
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<ILogger>(),
						It.IsAny<TimeSpan>(),
						It.IsAny<CancellationToken>()))
					.Returns(clientMock.Object);

				var logger = new TestLogger();
				var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

				var stopwatch = Stopwatch.StartNew();

				var entries = provider.GetCostRateEntries(DefaultCriteria);

				stopwatch.Stop();
				AssertEquals(0, entries.Count());
				AssertCollectionContains(
					"Warning:Timeout connecting to URS. Please try again. If problem persists, please raise an eRequest.",
					logger.Warnings);
				ErrorReporter.Clear();
			}
		}

		public void TestLastRawResponse_CapturesResponse()
		{
			var tradeService = CreateValidUldOrFclRate(Core.Constants.TransportModes.Air, "QA");

			var clientMock = new Mock<IUrsClient>();
			clientMock
				.Setup(c => c.GetTradeServicesAsync(
					It.IsAny<QueryRequestDto>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(new[] { tradeService });
			clientMock.SetupGet(c => c.LastRequest).Returns("{ \"test\": \"request\" }");

			var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<ILogger>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<CancellationToken>()))
				.Returns(clientMock.Object);

			var logger = new TestLogger();
			var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

			provider.GetCostRateEntries(DefaultCriteria);

			AssertContains("test", provider.LastRawResponse);
		}

		public void TestGetCostRateEntries_LogsQueryFilterAsDebug()
		{
			var criteria = DefaultCriteria;
			var clientMock = new Mock<IUrsClient>();
			clientMock
				.Setup(c => c.GetTradeServicesAsync(
					It.IsAny<QueryRequestDto>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync([]);

			var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<ILogger>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<CancellationToken>()))
				.Returns(clientMock.Object);

			var logger = new TestLogger();
			var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

			provider.GetCostRateEntries(criteria);

			Assert(logger.Debugs.Any(debug => debug.StartsWith("Debug:Searching for costs on URS with the following filter")));
		}

		public void TestGetCostRateEntries_LogsEntryCountAsInformation()
		{
			var tradeServices = new List<TradeServiceDto>
			{
				CreateValidUldOrFclRate(Core.Constants.TransportModes.Air, "QA"),
				CreateValidUldOrFclRate(Core.Constants.TransportModes.Air, "QA"),
				CreateValidUldOrFclRate(Core.Constants.TransportModes.Air, "QA")
			};

			var clientMock = new Mock<IUrsClient>();
			clientMock
				.Setup(c => c.GetTradeServicesAsync(
					It.IsAny<QueryRequestDto>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(tradeServices);

			var clientFactoryMock = new Mock<IUrsRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<ILogger>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<CancellationToken>()))
				.Returns(clientMock.Object);

			var logger = new TestLogger();
			var provider = new UrsRatesProvider(logger, clientFactoryMock.Object, Factory);

			provider.GetCostRateEntries(criteria: DefaultCriteria);

			AssertCollectionContains(
					"Info:3 entries from URS found.",
					logger.Infos);
		}

		RatingCriteria DefaultCriteria
		{
			get
			{
				if (defaultCriteria == null)
				{
					var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
					var billTo = Factory.NewWithValidTestData<OrgHeader>();

					defaultCriteria = new TestRatingCriteria("UAIEV", "AUSYD", 2, container, billTo)
					{
						AdapterType = AdapterType.Consolidation,
						JobDatesProvider = new TestJobDatesProvider(
							Factory.NewWithValidTestData<DummyBusinessObject>(),
							utcToday,
							utcToday.AddDays(1)),
						OperationalJobCode = "S000234202",
						FreightMode = FreightMode.FCL,
						ContainerMode = Core.Constants.ContainerModes.FCL
					};
				}

				return defaultCriteria;
			}
		}

		RatingCriteria defaultCriteria;

		TradeServiceDto CreateValidUldOrFclRate(string transportMode, string carrierCode, string containerCode = "22G0")
		{
			return new TradeServiceDto()
			{
				ModesOfTransport = new ModesOfTransportDataDto()
				{
					Items = new[]
					{
						new ModeOfTransportDto()
						{
							Code = transportMode == Core.Constants.TransportModes.Air ? ModeOfTransport.Air : ModeOfTransport.Ocean
						}
					}
				},

				Type = ComposedTradeServiceType.Direct,

				// Code is the IATA code for AIR rate.
				TransportProvider = new TransportProviderDto() { Code = carrierCode },

				ServiceClass = ServiceClassCode.Selling,
				Container = new ContainerDto()
				{
					IsoCode = containerCode,
					IsShipperOwned = false
				},
				Product = new ProductDto()
				{
					Code = "PRD",
					Name = "Product X",
					Classification = new ProductClassDto()
					{
						Code = "GEN",
						Name = "General"
					},
					ServiceLevel = new ProductServiceLevelDto()
					{
						Code = "Std"
					},
					UniversalCode = "HAZD",
				},
				Commodity = new CommodityDataDto
				{
					Groups = [new CommodityGroupDto
					{
						Code = "ABC",
						Description = "Group one"
					}]
				},
				VoyageInfo = new TradeServiceVoyageDto() { OceanRoutingTerm = "Std" },

				RouteInfo = new RouteInfoDto()
				{
					Waypoints = new[]
					{
						new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Origin, Location = new LocationDto() { Code = "UAIEV", FunctionCode = LocationFunctionCode.Airport } },
						new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Via, Location = new LocationDto() { Code = "AEDXB", FunctionCode = LocationFunctionCode.Airport } },
						new GeoScopeEntryDto() { WaypointType = RouteWaypointCode.Destination, Location = new LocationDto() { Code = "AUSYD", FunctionCode = LocationFunctionCode.Airport } },
					}
				},
				Contract = new ContractDto()
				{
					EffectiveDateInfo = new EffectiveDateInfoDto()
					{
						StartDate = utcToday.AddMonths(-6),
						EndDate = utcToday.AddMonths(6)
					}
				},
				PriceInfo = new PriceInfoDto()
				{
					BaseRates = new RateCollectionDataDto()
					{
						Items = new[]
						{
							new RateCollectionDto()
							{
								VersionDateInfo = new VersionDateInfoDto
								{
									StartDate = utcToday.AddMonths(-5),
									EndDate = utcToday.AddMonths(5),
									IssueDate = new DateTime(2024,12,12)
								},
								Currency = "USD",
								PaymentTerm = PaymentTermCode.Prepaid,
								PriceEntries = new []
								{
									new UniversalRateEntryDto
									{
										Price = 1000,
										Applicable = RateApplicableCode.UnitPrice,
										BreakType = RateBreakTypeCode.Flat,
										BreakQuantity = 1,
										QuantityUnit = UnitOfMeasurementCode.Container,
										PricingQuantity = 1,
										PricingQuantityUnit = UnitOfMeasurementCode.Container,
									}
								}
							}
						}
					},
				}
			};
		}

		protected DateTime utcToday { get; } = ZDateTime.UtcToday.ToDateTime();

		#region Setup/Teardown

		List<IDisposable> disposables;

		protected override void SetUp()
		{
			base.SetUp();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";

			disposables = new List<IDisposable>
			{
				DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(
					Guid.Empty, Guid.Empty, Guid.Empty,
					RatesServiceRegistrySettingsCollection.GetEnabled())
			};
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.ForEach(x => x.Dispose());
		}

		#endregion
	}
}

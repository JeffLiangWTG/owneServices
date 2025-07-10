using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using AuthenticationService.Client.Models;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Rating.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools.Exceptions;
using static WiseRates.Api.Model.RatesSearchRequest;
using CancellationToken = System.Threading.CancellationToken;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.RatingTests.WiseRates
{
	public class WiseRatesProviderTest : TestCaseWithFactory
	{
		public void TestSetup_EnsureFactoryReturningRealWiseRatesClientIsRegistered()
		{
			Globals.IsTest_ForTest.Value = false;

			try
			{
				var authTokenProviderMock = new Mock<IAuthTokenProvider>();
				authTokenProviderMock
					.Setup(p => p.GetToken("mclaren",
						It.IsAny<int>(),
						It.IsAny<Action<LoginInfo>>(),
						It.IsAny<CancellationToken>(),
						It.IsAny<bool>()))
					.Returns(("some token", string.Empty));

				var factory = new WiseRatesClientFactory(authTokenProviderMock.Object);

				var (client, error) = factory.TryCreate("mclaren");

				AssertNullOrEmpty(nameof(error), error);
				AssertNotNull(nameof(client), client);
				AssertType(nameof(client), typeof(RatesServiceClient), client);
			}
			finally
			{
				Globals.IsTest_ForTest.ResetValue();
			}
		}

		public void TestGetCostRateEntries_CriteriaIsServiceOnly_ReturnsAnEmptyEnumerationWithoutSearching()
		{
			var numberOfCallsToWiseRatesSearch = 0;

			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var billTo = Factory.NewWithValidTestData<OrgHeader>();

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", 2, container, billTo)
			{
				AdapterType = AdapterType.Consolidation,
				JobDatesProvider = new TestJobDatesProvider(Factory.NewWithValidTestData<DummyBusinessObject>(),
					new ZDateTime(2016, 08, 15), new ZDateTime(2016, 08, 04)),
				IsServicesOnly = true
			};

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(
					It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Callback<RatesSearchRequest, string, CancellationToken>((a, b, c) => numberOfCallsToWiseRatesSearch++);

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((clientMock.Object, string.Empty));

			DataRegistryRating.Instance.CGSPApiURLOverride.DataType = new StringRegistryDataType();

			var logger = new TestLogger();

			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);

			var entries = provider.GetCostRateEntries(criteria);
			AssertEquals(0, entries.Count());
			AssertEquals(0, numberOfCallsToWiseRatesSearch);
			Assert(!(logger.Errors.Count > 0));
		}

		public void TestGetCostRateEntries_ClientFactoryReturnsNull_ReturnNoRates()
		{
			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((null, "No email!"));

			var logger = new TestLogger();
			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var rates = provider.GetCostRateEntries(DefaultCriteria);

			AssertEquals(0, rates.Count());
			clientFactoryMock.VerifyAll();
		}

		public void TestGetCostRateEntriesThrowsException_CaughtAndReported()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Throws(new Exception("unexpected exception"));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((clientMock.Object, string.Empty));

			ErrorReporter.Clear();
			var logger = new TestLogger();
			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var entries = provider.GetCostRateEntries(DefaultCriteria);

			clientMock.VerifyAll();

			Assert(!entries.Any());
			AssertEquals("unexpected exception", ErrorReporter.LastExceptionReported.Message);
			Assert(!(logger.Errors.Count > 0));
			ErrorReporter.Clear();
		}

		public void TestGetRates_ServiceDoesntAuthorizeUser_LogWarning()
		{
			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Throws(new HttpResponseException(HttpStatusCode.Unauthorized, "Buy a license dude"));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((clientMock.Object, string.Empty));

			var logger = new TestLogger();
			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var entries = provider.GetRates(new RatesQuery(), DefaultCriteria, Operation.WiseRateSearch, out var response);

			AssertEquals(0, entries.Count());

			AssertCollectionContains(
				"Warning:Rates Service: The service doesn't authorize current CW1 system",
				logger.Warnings
			);
		}

		public void TestContextOperation_ShouldBeAutoratingForGetCostRateEntries()
		{
			using (RatingDataRegistry.Instance.RatesServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
				var billTo = Factory.NewWithValidTestData<OrgHeader>();

				var criteria = new TestRatingCriteria("AUSYD", "UAIEV", 2, container, billTo)
				{
					JobDatesProvider = new TestJobDatesProvider(
						Factory.NewWithValidTestData<DummyBusinessObject>(),
						new ZDateTime(2016, 08, 15),
						new ZDateTime(2016, 08, 04)
					)
				};

				var authTokenProviderMock = new Mock<IAuthTokenProvider>();
				authTokenProviderMock
					.Setup(s => s.GetToken(It.IsAny<string>(),
						It.IsAny<int>(),
						It.IsAny<Action<LoginInfo>>(),
						It.IsAny<CancellationToken>(),
						It.IsAny<bool>())).Returns(("McLaren", ""));

				var clientMock = new Mock<IWiseRatesClient>();
				clientMock
					.Setup(c => c.SearchAsync(
						It.Is<RatesSearchRequest>(p => p.ContextOperation == Operation.Autorating),
						It.IsAny<string>(),
						It.IsAny<CancellationToken>()))
					.Returns(Task.FromResult(new RatesSearchResponse()));

				var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
				clientFactoryMock
					.Setup(f => f.TryCreate(
						It.IsAny<string>(),
						It.IsAny<int>(),
						It.IsAny<CancellationToken>(),
						It.IsAny<Integration.ILogger>()))
					.Returns((clientMock.Object, string.Empty));

				var logger = new TestLogger();
				var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);

				ErrorReporter.Instance.Clear();
				var rates = provider.GetCostRateEntries(criteria);
				AssertEquals("We should not log an error if JobId was not provided", string.Empty, ErrorReporter.LastMessageReported);
				ErrorReporter.Instance.Clear();
				clientMock.VerifyAll();
			}
		}

		public void TestRatesServiceSearch_Timeout_SendRatesRequest()
		{
			using (DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var clientMock = new Mock<IWiseRatesClient>();
				clientMock
					.Setup(c =>
						c.SearchAsync(
							It.IsAny<RatesSearchRequest>(),
							It.IsAny<string>(),
							// Must be IsAny CancellationToken since the cancelling is done via
							// the timeout registry value.
							It.IsAny<CancellationToken>()))
					.Returns((RatesSearchRequest r, string c, CancellationToken token) =>
					{
						Task.Delay(4000, token).SpinWait(Application.DoEvents);
						token.ThrowIfCancellationRequested();
						throw new Exception("Test should not reach this point");
					});

				var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
				clientFactoryMock
					.Setup(f =>
						f.TryCreate(
							It.IsAny<string>(),
							It.IsAny<int>(),
							It.IsAny<CancellationToken>(),
							It.IsAny<Integration.ILogger>()))
					.Returns((clientMock.Object, string.Empty));

				var logger = new TestLogger();
				var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
				var ratesQuery = new RatesQuery();
				var operation = Operation.Autorating;

				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var response = provider.SendRatesRequest(ratesQuery, operation);
				stopwatch.Stop();

				AssertEquals("Result set should be empty due to timeout", null, response.RatesSearchResponse);
				AssertCollectionContains(
					"Warning message should be logged for rates service timeout",
					"Warning:Rates Service: Timeout connecting to Rates Service. Please try again. If problem persists, please raise an eRequest.",
					logger.Warnings
				);
				AssertGreaterThan(
					"Elapsed time should not exceed the allowed timeout margin",
					3000,
					stopwatch.Elapsed.TotalMilliseconds
				);
			}
		}

		public void TestRatesServiceSearch_Warnings()
		{
			var warningsList = new List<string>();
			warningsList.Add("Warning 1");
			warningsList.Add("Warning 2");
			warningsList.Add("Warning 3");

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse
				{
					Rates = Array.Empty<Rate>(),
					Warnings = warningsList.ToArray()
				}));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((clientMock.Object, string.Empty));

			var logger = new TestLogger();
			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			provider.GetCostRateEntries(DefaultCriteria);

			const string expectedWarningText = @"Warning:Rates Service: Rates Service encountered following problems: 
Warning 1
Warning 2
Warning 3
Correlation ID: ";

			AssertContains(expectedWarningText, logger.ToString());
		}

		public void TestGetCostRateEntries_ChargesCannotBeConverted_RejectRate()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = "Qantas Airlines";
			carrier.OH_IsShippingProvider = true;
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "9XX";
			airline.RM_TwoCharacterCode = "QA";
			carrier.MiscServ.OM_RM_Airline = airline.PK;

			Factory.Save();

			var criteria = DefaultCriteria;
			criteria.Creditors = Creditors.New(OrgWithSource.New(carrier, new List<string> { "Consol" }));
			criteria.FreightMode = FreightMode.LSE;
			criteria.ContainerMode = Core.Constants.ContainerModes.Loose;

			var rateStartDate = criteria.JobDatesProvider.EarliestPossibleDate.AddDays(-5).ToDateTime();
			var rateExpiryDate = criteria.JobDatesProvider.LatestPossibleDate.AddDays(5).ToDateTime();

			var clientMock = new Mock<IWiseRatesClient>();
			clientMock
				.Setup(m => m.SearchAsync(It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(new RatesSearchResponse
				{
					Rates = new[]
						{
							new Rate { Carrier = "QNT", Destination = "UAIEV", Origin = "AUSYD", TransportMode = "AIR", ContainerMode = "LCL", Client = "", Provider = WRConstants.RateProviders.CargoGuide, StartDate = rateStartDate, ExpiryDate = rateExpiryDate, Charges = new []
							{
								new Charge { ChargeCode = "XXX", Currency = "AUD", FlatRate = 100m },
								new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m }
							} },
							new Rate { Carrier = "QNT", Destination = "UAIEV", Origin = "AUSYD", TransportMode = "AIR", ContainerMode = "LCL", Client = "AAABBB", Provider = WRConstants.RateProviders.CargoGuide, StartDate = rateStartDate, ExpiryDate = rateExpiryDate, Charges = new []
							{
								new Charge { ChargeCode = "XXX", Currency = "AUD", FlatRate = 100m },
								new Charge { ChargeCode = "YYY", Currency = "AUD", FlatRate = 100m }
							} },
							new Rate { Carrier = "QNT", Destination = "UAIEV", Origin = "AUSYD", TransportMode = "AIR", ContainerMode = "LCL", Client = "CCCDDD", Provider = WRConstants.RateProviders.CargoGuide, StartDate = rateStartDate, ExpiryDate = rateExpiryDate, Charges = new []
							{
								new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 100m }
							} }
						},
					Carriers = new[]
						{
							new RefCarrier { Code = "QNT", IATACode = "QA", Name = "Qantas Airlines" },
						},
					ChargeCodes = new[]
						{
							new RefChargeCode { Code = "FRT", Group = "FRT" },
							new RefChargeCode { Code = "XXX", Group = "FRT" },
							new RefChargeCode { Code = "YYY", Group = "FRT" },
						}
				}
				));

			var authTokenProviderMock = new Mock<IAuthTokenProvider>();
			authTokenProviderMock
				.Setup(s => s.GetToken(It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<Action<LoginInfo>>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<bool>()))
				.Returns(("McLaren", ""));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((clientMock.Object, string.Empty));

			var logger = new TestLogger();
			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var entries = provider.GetCostRateEntries(criteria).ToList();

			AssertCollectionContains(
				"Expected warning messages to contain specific conversion error explanations.",
				@"Warning:2 Entries from Rates Service failed to convert. Reasons:
Category cannot be identified as the Charge(s) under the Rate cannot be converted into CW1 Charges. Please check validation errors of the Rate line.
No Charge Code is assigned with or has the same Code as Universal 'XXX'.
No Charge Code is assigned with or has the same Code as Universal 'YYY'.
",
				logger.Warnings
			);

			AssertEquals("2 failed to convert, 1 converted", 1, entries.Count);
		}

		public void TestGetCostRateEntries_SeaTransportMode_BuildsRatesQueryWithMeasureVolume()
		{
			var criteria = DefaultCriteria;
			criteria.RateableMeasures.SetQuantity(Rating.Integration.MeasureType.Chargeable, 5, Core.Constants.Volume.CubicMetres);

			RatesSearchRequest actualRequest = null;
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Callback<RatesSearchRequest, string, CancellationToken>((a, b, c) => { actualRequest = a; })
				.Returns(Task.FromResult(new RatesSearchResponse()));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			provider.GetCostRateEntries(criteria);
			AssertNotNull(nameof(actualRequest), actualRequest);
			AssertCollectionContains(
				WRConstants.TransportModes.SEA,
				actualRequest.RatesQuery.TransportMode
			);
			var volumeMeasure = actualRequest.RatesQuery.Measures?.FirstOrDefault(x => x.Type == MeasureType.Volume);
			AssertNotNull(nameof(volumeMeasure), volumeMeasure);

			var actualVolumeMeasureString = $"{volumeMeasure.Type}|{volumeMeasure.Amount}|{volumeMeasure.Unit}";
			var expectedVolumeMeasureString = $"{MeasureType.Volume}|5|{Core.Constants.Volume.CubicMetres}";
			AssertEquals("Volume measure details should match the expected values", expectedVolumeMeasureString, actualVolumeMeasureString);
		}

		public void TestGetCostRateEntries_AirTransportMode_DoesNotBuildRatesQueryWithMeasureVolume()
		{
			// CargoGuide should only be passed a measure volume when using RateSelector...

			var criteria = DefaultCriteria;
			criteria.FreightMode = FreightMode.AIR;
			criteria.ContainerMode = Core.Constants.ContainerModes.Loose;
			criteria.RateableMeasures.SetQuantity(Rating.Integration.MeasureType.Chargeable, 5, Core.Constants.Volume.CubicMetres);

			RatesSearchRequest actualRequest = null;
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Callback<RatesSearchRequest, string, CancellationToken>((a, b, c) => { actualRequest = a; })
				.Returns(Task.FromResult(new RatesSearchResponse()));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<Integration.ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var provider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			provider.GetCostRateEntries(criteria);
			AssertNotNull(actualRequest);
			AssertCollectionContains(WRConstants.TransportModes.AIR, actualRequest.RatesQuery.TransportMode);
			var volumeMeasure = actualRequest.RatesQuery.Measures?.FirstOrDefault(x => x.Type == MeasureType.Volume);
			AssertNull(volumeMeasure);
		}

		RatingCriteria DefaultCriteria
		{
			get
			{
				if (defaultCriteria == null)
				{
					var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
					var billTo = Factory.NewWithValidTestData<OrgHeader>();

					defaultCriteria = new TestRatingCriteria("AUSYD", "UAIEV", 2, container, billTo);
					defaultCriteria.AdapterType = AdapterType.Consolidation;
					defaultCriteria.JobDatesProvider = new TestJobDatesProvider(Factory.NewWithValidTestData<DummyBusinessObject>(), new ZDateTime(2016, 08, 15), new ZDateTime(2016, 08, 04));
					defaultCriteria.OperationalJobCode = "S000234202";
					defaultCriteria.FreightMode = FreightMode.FCL;
				}

				return defaultCriteria;
			}
		}

		RatingCriteria defaultCriteria;

		#region Setup/Teardown

		List<IDisposable> disposables;

		protected override void SetUp()
		{
			base.SetUp();

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";

			disposables = new List<IDisposable>
			{
				DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty,Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled())
			};
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.ForEach(x => x.Dispose());
		}

		#endregion
	}

	public class TestJobDatesProvider : JobDatesProvider<DummyBusinessObject>
	{
		public TestJobDatesProvider(DummyBusinessObject parent, ZDateTime arrivalDate, ZDateTime departureDate) : base(parent)
		{
			this.arrivalDate = arrivalDate;
			this.departureDate = departureDate;
		}

		readonly ZDateTime arrivalDate;
		readonly ZDateTime departureDate;

		protected override ZDateTime GetArrivalDateCore()
		{
			return arrivalDate;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return departureDate;
		}
	}
}

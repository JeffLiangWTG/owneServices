using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Business.WiseRates;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Tools.Exceptions;
using DTO = WiseRates.Api.Model;
using RateDTO = WiseRates.Api.Model.Rate;

namespace Enterprise.Rating.Testing.GUITests.RateSelector
{
	public class RateServiceRateViewModelsProviderTest : TestCaseWithFactory
	{
		public void TestGetRates_Cancelled_WhenCW1Timesout()
		{
			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns((RatesSearchRequest r, string c, CancellationToken token) =>
				{
					Task.Delay(10000, token).SpinWait(Application.DoEvents);
					token.ThrowIfCancellationRequested();
					throw new Exception("Test should not reach this point");
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var rate = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				stopwatch.Stop();

				AssertEquals("Expected rates to be empty during timeout", 0, rate.Count());

				var warningLogs = logger.Logs
					.Where(l => l.Level == LogType.Warning)
					.Select(l => l.Message)
					.ToArray();

				AssertCollectionContains(
					"Timeout warning message should appear in logs",
					"Timeout connecting to Rates Service. Please try again. If problem persists, please raise an eRequest.",
					warningLogs
				);

				AssertCloseEnough(
					"Elapsed time should be close to 2 seconds with some allowance",
					2,
					(long)stopwatch.Elapsed.TotalSeconds,
					(long)0.5
				);
			}
		}

		public void TestGetRates_Cancelled_WhenUserCancels()
		{
			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns((RatesSearchRequest r, string c, CancellationToken token) =>
				{
					Task.Delay(10000, token).SpinWait(Application.DoEvents);
					token.ThrowIfCancellationRequested();
					throw new Exception("Test should not reach this point");
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			var cancelSource = new CancellationTokenSource(2000);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceRateSearchRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				var rate = provider.GetRatesAsync(filter, cancelSource.Token).GetAwaiter().GetResult();
				stopwatch.Stop();

				AssertEquals(
					"The rate collection should be empty since user cancelled the operation",
					0,
					rate.Count()
				);

				var warningMessages = logger.Logs
					.Where(l => l.Level == LogType.Warning)
					.Select(l => l.Message)
					.ToList();

				AssertCollectionNotContains(
					"Timeout warning message must not appear in the logs",
					"Timeout connecting to Rates Service. Please try again. If problem persists, please raise an eRequest.",
					warningMessages
				);

				AssertCloseEnough(
					"Elapsed time should be within the timeout limit (2 seconds with some buffer)",
					(long)2.5,
					(long)stopwatch.Elapsed.TotalSeconds,
					(long)1.50
				);
			}
		}

		public void TestGetRates()
		{
			var testDate = DateTime.Today;

			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(GetValidRatesSearchResponse(testDate));

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			var expectedResult = new CargoguideRateViewModel
			{
				IssueDate = testDate.AddDays(-10),
				StartDate = testDate.AddDays(-5),
				ExpiryDate = testDate.AddDays(10),

				Origin = "AUSYD",
				Destination = "USLAX",
				Via = "CNSHA",

				CarrierCode = null,             //	Not mapped, i.e. IATA code is not assigned to any local carrier
				CarrierName = "Japan Air",
				CarrierServiceLevel = "SVL",

				Commodities = "Small Package",
				ContractNumber = "CONTRACT",
				PaymentTerms = "PPD",
				CommodityGroups = new List<string>() { "GENL" },
			};

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rate = provider.GetRatesAsync(filter).GetAwaiter().GetResult();

				var expectedDataStrings = new[]
				{
					$"{expectedResult.Origin}|{expectedResult.Destination}|{expectedResult.Via}|{expectedResult.CarrierCode}|{expectedResult.CarrierName}|{expectedResult.CarrierServiceLevel}|{expectedResult.Commodities}|{expectedResult.ContractNumber}|{expectedResult.PaymentTerms}"
				};

				var actualDataStrings = rate
					.Select(r => $"{r.Origin}|{r.Destination}|{r.Via}|{r.CarrierCode}|{r.CarrierName}|{r.CarrierServiceLevel}|{r.Commodities}|{r.ContractNumber}|{r.PaymentTerms}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder("All properties of the expected object should match the actual ones.", expectedDataStrings, actualDataStrings);

				var traceIDs = rate.Cast<CargoguideRateViewModel>().Select(r => r.context.RatesServiceTraceID);

				AssertNotNullOrEmpty("RatesServiceTraceID", traceIDs.Single());
			}
		}

		public void TestGetRates_ShouldGetValuesFromProviderCustomFields()
		{
			var testDate = DateTime.Today;

			var rateSearchResponse = GetValidRatesSearchResponse(testDate);
			var testRate = rateSearchResponse.Rates.Single();
			testRate.ProviderCustomFields = new[]
			{
				new CustomField { Code = RateDTO.CustomFields.Cargoguide.Ratio, Description = "ratio", Value = "1:3" },
				new CustomField { Code = RateDTO.CustomFields.Cargoguide.Remarks, Description = "remarks", Value = "REMARKABLE!" },
				new CustomField { Code = RateDTO.CustomFields.Cargoguide.DeckType, Description = "deck type", Value = "UpperDeck" },
			};

			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(rateSearchResponse);

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rate = provider.GetRatesAsync(filter).GetAwaiter().GetResult().Single() as CargoguideRateViewModel;
				AssertEquals("Cargoguide ratio should be correct", "1:3", rate.Ratio);
				AssertEquals("Cargoguide remarks should be correct", "REMARKABLE!", rate.Remarks);
				AssertEquals("Cargoguide deck type should be correct", "UpperDeck", rate.Deck);
			}
		}

		public void TestGetRates_ShouldGetValuesFromProviderCustomFields_WhenRateSelectorIsNotActive()
		{
			AssertGetRateFromProvider(false, true, true);
			Assert("This test uses FluentAssertions", true);
		}

		public void TestGetRates_ShouldNotGetValuesFromProviderCustomFields_WhenRateSelectorIsActiveAndRateServiceIsNotActive()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ AssertGetRateFromProvider(true, false, true); });
		}

		public void TestGetRates_ShouldNotGetValuesFromProviderCustomFields_WhenRateSelectorIsActiveAndRateServiceIsActiveAndCargoGuideIsNotActive()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), delegate
			{ AssertGetRateFromProvider(true, true, false); });
		}

		void AssertGetRateFromProvider(bool rateSelectionRegistry, bool rateServicesRegistry, bool cargoGuideRegistry)
		{
			var testDate = DateTime.Today;

			var rateSearchResponse = GetValidRatesSearchResponse(testDate);
			var testRate = rateSearchResponse.Rates.Single();
			testRate.ProviderCustomFields = new[]
			{
				new CustomField { Code = RateDTO.CustomFields.Cargoguide.Ratio, Description = "ratio", Value = "1:3" },
				new CustomField
				{
					Code = RateDTO.CustomFields.Cargoguide.Remarks, Description = "remarks", Value = "REMARKABLE!"
				},
				new CustomField
				{
					Code = RateDTO.CustomFields.Cargoguide.DeckType, Description = "deck type", Value = "UpperDeck"
				},
			};

			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(rateSearchResponse);

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(),
					It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));
			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object,
				new Mock<IDialogService>().Object, logger);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDefault(rateSelectionRegistry)))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDefault(rateServicesRegistry)))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cargoGuideRegistry))
			{
				var providerRates = provider.GetRatesAsync(filter);
				var awaiter = providerRates.GetAwaiter();
				var results = awaiter.GetResult().Single();
				var rate = results as CargoguideRateViewModel;

				AssertEquals("Expected ratio to match the specified value.", "1:3", rate.Ratio);
				AssertEquals("Expected remarks to match the specified value.", "REMARKABLE!", rate.Remarks);
				AssertEquals("Expected deck type to match the specified value.", "UpperDeck", rate.Deck);
			}
		}

		public void TestGetRates_HttpRequestException()
		{
			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new HttpRequestException("Http request exception message"));

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult();

				AssertEquals("Expected no rates to be returned.", 0, rates.Count());
				AssertCollectionContains("Expected an error log containing the exception message.", "Http request exception message", logger.Logs
					.Where(x => x.Level == LogType.Error)
					.Select(x => x.Message));
			}
		}

		public void TestGetRates_HttpResponseException()
		{
			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new HttpResponseException(HttpStatusCode.Forbidden, "Http response exception message"));

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult();

				AssertEquals("Expected no rates to be returned.", 0, rates.Count());
				AssertCollectionContains("403 - Forbidden: Http response exception message", logger.Logs.Where(x => x.Level == LogType.Error).Select(x => x.Message));
			}
		}

		public void TestCanSearchCargoguide()
		{
			var testDate = DateTime.Today;

			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(GetValidRatesSearchResponse(testDate));

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((new RatesServiceClient(rateServiceClientMock.Object), null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				logger.Clear();

				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				AssertEquals(0, rates.Count());

				const string expectedMessage = "Email address of current user in staff details is mandatory for accessing CGGD";
				AssertCollectionContains(expectedMessage, logger.Logs
					.Where(x => x.Level == LogType.Warning)
					.Select(x => x.Message));
			}

			GlbStaff.CurrentUser.GS_EmailAddress = "blah@example.com";
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				logger.Clear();

				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				AssertEquals(0, rates.Count());

				const string expectedMessage = "Request will not be sent to Rates Service because CGGD integration is disabled in the registry";
				AssertCollectionContains(expectedMessage, logger.Logs
					.Where(x => x.Level == LogType.Warning)
					.Select(x => x.Message));
			}

			GlbStaff.CurrentUser.GS_EmailAddress = "blah@example.com";
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				logger.Clear();

				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				AssertEquals(0, rates.Count());

				const string expectedMessage = "Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service Subscription";
				AssertCollectionContains(expectedMessage, logger.Logs
					.Where(x => x.Level == LogType.Warning)
					.Select(x => x.Message));
			}

			GlbStaff.CurrentUser.GS_EmailAddress = "blah@example.com";
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				logger.Clear();

				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				AssertGreaterThan(rates.Count(), 0);

				var messagesNotWanted = new[]
				{
					"Email address of current user in staff details is mandatory for accessing CGGD",
					"Request will not be sent to Rates Service because CGGD integration is disabled in the registry",
					"Request will not be sent to Rates Service because Rates Service subscription is disabled in the registry for AIR-LSE: AutoRating -> Rates Service -> Rates Service Subscription"
				};

				var actualMessages = logger.Logs
					.Where(x => x.Level == LogType.Warning)
					.Select(x => x.Message);

				foreach (var message in messagesNotWanted)
				{
					AssertCollectionNotContains(message, actualMessages);
				}
			}
		}

		public void TestHBLRFound_WasLoggedAsInformation()
		{
			var testDate = ZDateTime.BrettsBirthday.ToDateTime();
			var rateSearchResponse = GetValidRatesSearchResponse(testDate);
			var rate = rateSearchResponse.Rates[0];
			foreach (var charge in rate.Charges)
			{
				charge.IsHigherBreakLowerRate = true;
			}

			rateSearchResponse.Rates = new[] { rate, rate, rate, rate };

			var rateServiceClientMock = new Mock<IWiseRatesClient>();
			rateServiceClientMock
				.Setup(x => x.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(rateSearchResponse);

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(x => x.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((rateServiceClientMock.Object, null));

			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.LSE, 10, 1, null);
			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new RateServiceRateViewModelsProvider(wiseRatesClientFactoryMock.Object, new Mock<IDialogService>().Object, logger);

			GlbStaff.CurrentUser.GS_EmailAddress = "support@cargowise.com";
			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var results = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				var hblrLogsCount = logger.Logs.Count(l => l.Level == LogType.Information && l.Message == "One or more rates have HBLR applied");

				AssertEquals(
					"HBLR notice should be in the logs once only, regardless of the number of rates that have HBLR",
					1,
					hblrLogsCount
				);
			}
		}

		public static RatesSearchResponse GetValidRatesSearchResponse(DateTime testDate) => new RatesSearchResponse
		{
			Rates = new[]
			{
				new RateDTO
				{
					TransportMode = "AIR",
					ContainerMode = "LCL",

					IssueDate = testDate.AddDays(-10),
					StartDate = testDate.AddDays(-5),
					ExpiryDate = testDate.AddDays(10),

					Origin = "AUSYD",
					Destination = "USLAX",
					Via = "CNSHA",

					Carrier = "JPAIR",
					ServiceLevel = "SVL",

					Commodity = "GENL",
					ContractNumber = "CONTRACT",
					PaymentTerm = "PPD",

					CarrierCommodityInfo = new CarrierSpecificCommodity
					{
						Code = "SP1",
						GroupName = "Small Package"
					},

					Charges = new []
					{
						new Charge
						{
							ChargeCode = "FRT",
							PerUnitRate = 200,
							Currency = "USD",
							Unit = "KG"
						}
					}
				},
			},
			ChargeCodes = new[]
			{
				new RefChargeCode
				{
					Code = "FRT",
					Description = "Freight"
				}
			},
			Carriers = new[]
			{
				new RefCarrier
				{
					Code = "JPAIR",
					IATACode = "JP",
					Name = "Japan Air"
				}
			},
			ServiceLevels = new[]
			{
				new DTO.RefServiceLevel
				{
					Code = "SVL",
					Description  = "Some service level"
				}
			}
		};
	}
}

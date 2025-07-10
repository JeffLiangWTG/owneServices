using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.Rating.Integration;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using DTO = WiseRates.Api.Model;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class NonContainerizedRatesViewModelTest : BaseRatingIntegrationTest
	{
		#region CanApply

		public void TestCanApply_ValidRateHasBeenSelected_ShouldChangeToTrue()
		{
			using (var viewModel = CreateViewModel())
			{
				AssertEquals("Precondition", false, viewModel.CanApply);

				var propertyChanges = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);

				var rate = CreateCargoguideRate();
				viewModel.SelectedRate = rate;

				AssertCollectionContains(
					"Expected CanApply property change was not raised.",
					"CanApply",
					propertyChanges
				);
				AssertEquals(true, viewModel.CanApply);
			}
		}

		public void TestCanApply_InvalidRateHasBeenSelected_ShouldChangeToFalse()
		{
			using (var viewModel = CreateViewModel())
			{
				var validRate = CreateCargoguideRate();
				viewModel.SelectedRate = validRate;
				AssertEquals("Precondition", true, viewModel.CanApply);

				var propertyChanges = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);

				var invalidRate = CreateCargoguideRate();
				invalidRate.CarrierErrorLevel = ErrorLevel.Error;

				viewModel.SelectedRate = invalidRate;

				AssertCollectionContains(
					"Property change notification for CanApply should be raised",
					"CanApply",
					propertyChanges
				);
				AssertEquals(false, viewModel.CanApply);
			}
		}

		public void TestCanApply_ValidRateBecomesInvalid_ShouldChangeToFalse()
		{
			using (var viewModel = CreateViewModel())
			{
				var rate = CreateCargoguideRate();
				viewModel.SelectedRate = rate;
				AssertEquals("Precondition", true, viewModel.CanApply);

				var propertyChanges = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);

				rate.CarrierErrorLevel = ErrorLevel.Error;

				AssertEquals(
					"Property change for CanApply should be raised.",
					true,
					propertyChanges.Contains(nameof(viewModel.CanApply))
				);

				AssertEquals(false, viewModel.CanApply);
			}
		}

		#endregion

		public void TestError_ManyProvidersOneErroredOneException_OthersStillProvideRates()
		{
			var providerWithRates = new Mock<IRateViewModelsProvider>();
			var providerWithImmediateError = new Mock<IRateViewModelsProvider>();
			var providerWithDelayedError = new Mock<IRateViewModelsProvider>();
			var providerWithImmediateException = new Mock<IRateViewModelsProvider>();
			var providerWithDelayedException = new Mock<IRateViewModelsProvider>();

			var providerMocks = new[]
			{
				providerWithRates,
				providerWithImmediateError,
				providerWithDelayedError,
				providerWithImmediateException,
				providerWithDelayedException
			};

			foreach (var mock in providerMocks)
			{
				mock
					.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
					.Returns(true);
			}

			var logger = new MemoryLogger();
			var jobCarrier = Helper.NewOrgHeader("JobCarrier");
			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.LSE, 6, 1, jobCarrier);
			criteria.Carrier = jobCarrier;
			criteria.Creditors = Creditors.New();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));

			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);

			var rate = CreateRatesServiceRate();
			var rateSelectorContext = new RateSelectorContext
			{
				RatesServiceResponse = CreateRatesServiceResponse(new[] { rate }),
				Factory = Factory,
				Filters = filters,
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			providerWithRates.Setup(x => x.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()))
				.ReturnsRateResult((new[] { new CargoguideRateViewModel(rate, rateSelectorContext) } as IEnumerable<RateViewModel>));
			providerWithImmediateException.Setup(x => x.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()))
				.Throws(new Exception("Exception"));
			providerWithDelayedException.Setup(x => x.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Exception("Delayed exception"));
			providerWithImmediateError.Setup(x => x.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()))
				.Throws(new AutoRaterException("Error"));
			providerWithDelayedError.Setup(x => x.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()))
				.ThrowsAsync(new AutoRaterException("Delayed error"));

			using (var viewModel = CreateViewModel(filters, providerMocks.Select(m => m.Object), logger))
			{
				AssertNoExceptionThrown("All exceptions should be caught internally to the method", () =>
				{
					viewModel.SearchAsync(default).ConfigureAwait(false).GetAwaiter().GetResult();
				});

				AssertEquals("The rate count should be one as only one provider succeeds", 1, viewModel.Rates.Count());

				AssertEquals("One Issue per each Exception", 2, ErrorReporter.TotalErrorCount);
				AssertEquals("Delayed exception", ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();

				AssertCollectionContains(
					"Logger should contain error for immediate error",
					logger.Logs.Single(x => x.Level == LogType.Error && x.Message == "Loading rates from unknown provider failed due to: Error"),
					logger.Logs
				);
				AssertCollectionContains(
					"Logger should contain error for immediate exception",
					logger.Logs.Single(x => x.Level == LogType.Error && x.Message == "An unexpected error occurred while loading rates from unknown provider"),
					logger.Logs
				);
				AssertCollectionContains(
					"Logger should contain error for delayed error",
					logger.Logs.Single(x => x.Level == LogType.Error && x.Message == "Loading rates failed due to: Delayed error"),
					logger.Logs
				);
				AssertCollectionContains(
					"Logger should contain error for delayed exception",
					logger.Logs.Single(x => x.Level == LogType.Error && x.Message == "An unexpected error occurred while loading rates"),
					logger.Logs
				);
			}
		}

		#region GetSelectedRate

		public void TestGetSelectedRate_CriteriaHasNoCarrier_ShouldIncludeRatesFromOtherPartiesOtherThanSelectedCarrier()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var costing = Helper.NewCosting(creditor);
			var costingRateEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD");
			costingRateEntry.RateLines.RemoveAndDeleteAll();
			costingRateEntry.AddFlatCharge(TestFRT.AC_Code, 10);

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 10m, 10m, creditor);
			criteria.Creditors = Creditors.New();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(creditor, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(carrier, new List<string>(new[] { "Test" })));

			AssertNull("Precondition: criteria has no carrier", criteria.Carrier);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
			using (_Rating.StartCost())
			{
				var logger = new MemoryLogger();
				var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);

				using (var viewModel = CreateViewModel(filters))
				{
					var rateMock = new Mock<RateViewModel>();
					rateMock.Setup(r => r.GetAutoRateInfos()).Returns(new[]
					{
						new AutoRateInfo(Factory)
						{
							ProviderPK = carrier.PK,
							ChargeCode = TestBAF,
							IsFromRatesService = true,
							IsInclusiveCalculator = false,
							IsSubjectTo = false,
							HasExplicitZeroAmount = false,
							CalculationDescription = "No subject-to fallback happened",
						},
					});
					viewModel.SelectedRate = rateMock.Object;
					AssertNoExceptionThrown(() =>
					{
						var results = viewModel.GetSelectedCharges()
							.Select(r => $"{r.ProviderPK}|{r.ChargeCode.AC_Code}|{r.SingleLineDescription}");

						var expectedResults = new[]
						{
							$"{carrier.PK}|{TestBAF.AC_Code}|TESTBAF: No subject-to fallback happened",
							$"{creditor.PK}|{TestFRT.AC_Code}|TESTFRT: Base Rate AUD 10.00",
						};

						AssertContainsExactElementsInAnyOrder(
							"The resulting charges should match the expected collection",
							expectedResults,
							results
						);
					});
				}
			}
		}

		public void TestSelectRate_ShouldReturnSelectedCarrierRatesAndCW1RatesFromOtherParties()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var jobCarrier = Helper.NewOrgHeader("JobCarrier");
			var sendAgent = Helper.NewOrgHeader("SendingAgent");
			var receiveAgent = Helper.NewOrgHeader("RecvAgent");
			var carrierFromRateService = Helper.NewOrgHeader("RSCarrier");

			var jobCarrierCosting = Helper.NewCosting(jobCarrier);
			var jobCarrierAirRate = jobCarrierCosting.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL", "STD");
			jobCarrierAirRate.RateLines.RemoveAndDeleteAll();
			jobCarrierAirRate.AddRateLine(TestFSC, "FLT").Calculator["BAS"] = (ZDecimal)10;
			var jobCarrierOrgRate = jobCarrierCosting.AddRateEntry("ORG", "ALL", "AUSYD", "AUMEL", "STD");
			jobCarrierOrgRate.RateLines.RemoveAndDeleteAll();
			jobCarrierOrgRate.AddRateLine(TestSEA, "FLT").Calculator["BAS"] = (ZDecimal)11;
			var jobCarrierDestiantionRate = jobCarrierCosting.AddRateEntry("DST", "ALL", "AUSYD", "AUMEL", "STD");
			jobCarrierDestiantionRate.RateLines.RemoveAndDeleteAll();
			jobCarrierDestiantionRate.AddRateLine(TestPSC, "FLT").Calculator["BAS"] = (ZDecimal)12;

			var sendAgentCosting = Helper.NewCosting(sendAgent);
			var sendAgentAirRate = sendAgentCosting.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL", "STD");
			sendAgentAirRate.RateLines.RemoveAndDeleteAll();
			sendAgentAirRate.AddRateLine(TestFRT, "FLT").Calculator["BAS"] = (ZDecimal)201;
			sendAgentAirRate.AddRateLine(TestBAF, "FLT").Calculator["BAS"] = (ZDecimal)202;
			var sendAgentOriginRate = sendAgentCosting.AddRateEntry("ORG", "ALL", "AUSYD", "AUMEL", "STD");
			sendAgentOriginRate.RateLines.RemoveAndDeleteAll();
			sendAgentOriginRate.AddRateLine(TestAWB, "FLT").Calculator["BAS"] = (ZDecimal)211;
			sendAgentOriginRate.AddRateLine(TestBBK, "FLT").Calculator["BAS"] = (ZDecimal)212;
			var sendAgentDestinationRate = sendAgentCosting.AddRateEntry("DST", "ALL", "AUSYD", "AUMEL", "STD");
			sendAgentDestinationRate.RateLines.RemoveAndDeleteAll();
			sendAgentDestinationRate.AddRateLine(TestLOL, "FLT").Calculator["BAS"] = (ZDecimal)22;

			var receiveAgentCosting = Helper.NewCosting(receiveAgent);
			var receiveAgentAirRate = receiveAgentCosting.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL", "STD");
			receiveAgentAirRate.RateLines.RemoveAndDeleteAll();
			receiveAgentAirRate.AddRateLine(TestFRT, "FLT").Calculator["BAS"] = (ZDecimal)301;
			receiveAgentAirRate.AddRateLine(TestCAF, "FLT").Calculator["BAS"] = (ZDecimal)302;
			receiveAgentAirRate.AddRateLine(TestWAR, "FLT").Calculator["BAS"] = (ZDecimal)303;
			var receiveAgentOriginRate = receiveAgentCosting.AddRateEntry("ORG", "ALL", "AUSYD", "AUMEL", "STD");
			receiveAgentOriginRate.RateLines.RemoveAndDeleteAll();
			receiveAgentOriginRate.AddRateLine(TestAWB, "FLT").Calculator["BAS"] = (ZDecimal)31;
			var receiveAgentDestinationRate = receiveAgentCosting.AddRateEntry("DST", "ALL", "AUSYD", "AUMEL", "STD");
			receiveAgentDestinationRate.RateLines.RemoveAndDeleteAll();
			receiveAgentDestinationRate.AddRateLine(TestLOL, "FLT").Calculator["BAS"] = (ZDecimal)321;
			receiveAgentDestinationRate.AddRateLine(TestADF, "FLT").Calculator["BAS"] = (ZDecimal)322;

			var carrierCosting = Helper.NewCosting(carrierFromRateService);
			var carrierAirRate = carrierCosting.AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL", "STD");
			carrierAirRate.RateLines.RemoveAndDeleteAll();
			carrierAirRate.AddRateLine(TestWAR, "FLT").Calculator["BAS"] = (ZDecimal)10;
			var carrierOriginRate = carrierCosting.AddRateEntry("ORG", "ALL", "AUSYD", "AUMEL", "STD");
			carrierOriginRate.RateLines.RemoveAndDeleteAll();
			carrierOriginRate.AddRateLine(TestANY, "FLT").Calculator["BAS"] = (ZDecimal)11;
			var carrierDestinationRate = carrierCosting.AddRateEntry("DST", "ALL", "AUSYD", "AUMEL", "STD");
			carrierDestinationRate.RateLines.RemoveAndDeleteAll();
			carrierDestinationRate.AddRateLine(TestITF, "FLT").Calculator["BAS"] = (ZDecimal)12;

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.LSE, 6, 1, jobCarrier);
			criteria.Carrier = jobCarrier;
			criteria.Creditors = Creditors.New();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(2, OrgWithSource.New(sendAgent, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(3, OrgWithSource.New(receiveAgent, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(4, OrgWithSource.New(carrierFromRateService, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(2, OrgWithSource.New(sendAgent, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(3, OrgWithSource.New(receiveAgent, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(4, OrgWithSource.New(carrierFromRateService, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(2, OrgWithSource.New(sendAgent, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(3, OrgWithSource.New(receiveAgent, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(4, OrgWithSource.New(carrierFromRateService, new List<string>(new[] { "Test" })));

			var rateMock = new Mock<RateViewModel>();
			rateMock.Setup(r => r.GetAutoRateInfos()).Returns(new[]
			{
				new AutoRateInfo(Factory)
				{
					ProviderPK = carrierFromRateService.PK,
					ChargeCode = TestFRT,
					RateSource = "Selected Freight rate",
					IsFromRatesService = true
				},
				new AutoRateInfo(Factory)
				{
					ProviderPK = carrierFromRateService.PK,
					ChargeCode = TestWAR,
					RateSource = "Selected Freight Inclusive rate",
					IsFromRatesService = true,
					IsInclusiveCalculator = true
				},
				new AutoRateInfo(Factory)
				{
					ProviderPK = carrierFromRateService.PK,
					ChargeCode = TestAWB,
					RateSource = "Selected Origin rate"
				},
				new AutoRateInfo(Factory)
				{
					ProviderPK = carrierFromRateService.PK,
					ChargeCode = TestLOL,
					RateSource = "Selected Destination rate"
				}
			});

			var viewModelsProvider = new Mock<IRateViewModelsProvider>();
			viewModelsProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (_Rating.Start(new LoggerDecorator()))
			using (_Rating.StartCost())
			{
				var expectedResults = new[]
				{
					$"{carrierFromRateService.PK}|{TestFRT.AC_Code}|Selected Freight rate",
					$"{carrierFromRateService.PK}|{TestWAR.AC_Code}|Selected Freight Inclusive rate",
					$"{carrierFromRateService.PK}|{TestAWB.AC_Code}|Selected Origin rate",
					$"{carrierFromRateService.PK}|{TestLOL.AC_Code}|Selected Destination rate",
					$"{sendAgent.PK}|{TestBAF.AC_Code}|{sendAgentCosting.DisplayInfo()}",
					$"{sendAgent.PK}|{TestBBK.AC_Code}|{sendAgentCosting.DisplayInfo()}",
					$"{receiveAgent.PK}|{TestCAF.AC_Code}|{receiveAgentCosting.DisplayInfo()}",
					$"{receiveAgent.PK}|{TestADF.AC_Code}|{receiveAgentCosting.DisplayInfo()}"
				};

				var logger = new MemoryLogger();
				var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);
				using (var viewModel = CreateViewModel(filters))
				{
					viewModel.SelectedRate = rateMock.Object;

					var results = viewModel.GetSelectedCharges().Select(r => $"{r.ProviderPK}|{r.ChargeCode.AC_Code}|{r.RateSource}");

					AssertContainsExactElementsInAnyOrder("The selected carrier rates and CW1 rates should match the expected collection", expectedResults, results.ToArray());
				}
			}
		}

		public void TestSelectRate_ShouldSetDescriptionOfSelectedCW1Rates()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var org1Rate = Factory.New<Costing>();
			org1Rate.TH_OH = org1.PK;

			var org1AirRate = org1Rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD");
			org1AirRate.RateLines.RemoveAndDeleteAll();
			var rateLine1 = org1AirRate.AddRateLine(TestFRT, UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var org1OriginRate = org1Rate.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "STD");
			org1OriginRate.RateLines.RemoveAndDeleteAll();
			var rateLine2 = org1OriginRate.AddRateLine(TestAWB, UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var org1DestinationRate = org1Rate.AddRateEntry("DST", "ALL", "AUSYD", "USLAX", "STD");
			org1DestinationRate.RateLines.RemoveAndDeleteAll();
			var rateLine3 = org1DestinationRate.AddRateLine(TestPSC, UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 6, 1, org1);
			criteria.Carrier = org1;
			criteria.Creditors = Creditors.New();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var calculationResult1 = CalculationResult.CreateForTest(rateLine1, 40, 30, 60, criteria);
			var calculationResult2 = CalculationResult.CreateForTest(rateLine2, 50, 30, 60, criteria);
			var calculationResult3 = CalculationResult.CreateForTest(rateLine3, 60, 30, 70, criteria);

			var rateMock = new Mock<RateViewModel>();

			rateMock.Setup(r => r.GetAutoRateInfos()).Returns(new[]
			{
				new AutoRateInfo(calculationResult1, parameters, Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestFRT
				},
				new AutoRateInfo(calculationResult2, parameters, Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestAWB
				},
				new AutoRateInfo(calculationResult3, parameters, Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestPSC
				}
			});

			var viewModelsProvider = new Mock<IRateViewModelsProvider>();
			viewModelsProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (_Rating.Start(new LoggerDecorator()))
			using (_Rating.StartCost())
			{
				var expectedResults = new[]
				{
					"ProviderPK=" + org1.PK + "|ChargeCode=" + TestFRT.AC_Code + "|SingleLineDescription=TESTFRT: Base Rate AUD 40.00",
					"ProviderPK=" + org1.PK + "|ChargeCode=" + TestAWB.AC_Code + "|SingleLineDescription=TESTAWB: Base Rate AUD 50.00",
					"ProviderPK=" + org1.PK + "|ChargeCode=" + TestPSC.AC_Code + "|SingleLineDescription=TESTPSC: Base Rate AUD 60.00",
				};

				var logger = new MemoryLogger();
				var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);
				using (var viewModel = CreateViewModel(filters))
				{
					viewModel.SelectedRate = rateMock.Object;

					var results = viewModel.GetSelectedCharges().Select(r =>
						"ProviderPK=" + r.ProviderPK + "|ChargeCode=" + r.ChargeCode.AC_Code + "|SingleLineDescription=" + r.SingleLineDescription.ToString()).ToArray();

					AssertContainsExactElementsInAnyOrder(
						"Selected charges should match the expected results.",
						expectedResults,
						results);
				}
			}
		}

		public void TestSelectRate_SubjectToFallback_DisabledAtRegistry_NoFallbackOccurred()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var registryValue = new FallbackSubjectToChargesCollection();
			registryValue.Add(new FallbackSubjectToCharges()
			{
				RatesProviderCode = WRConstants.RateProviders.CargoGuide,
				TransportMode = TransportModes.Air,
				ContainerMode = ContainerModes.Loose,
				IsFallbackEnabled = false
			});

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Rate = Helper.NewCosting(org1);
			var org2Rate = Helper.NewCosting(org2);

			var org1AirRate = org1Rate
				.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD")
				.AddFlatCharge(TestBAF.AC_Code, 1234);

			var org2OrgRate = org2Rate
				.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "STD")
				.AddFlatCharge(TestBBK.AC_Code, 2000);

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 6, 1, org1);
			criteria.Carrier = org1;
			criteria.Creditors = Creditors.New();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(2, OrgWithSource.New(org2, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(2, OrgWithSource.New(org2, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(2, OrgWithSource.New(org2, new List<string>(new[] { "Test" })));

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var rateMock = new Mock<RateViewModel>();

			rateMock.Setup(r => r.GetAutoRateInfos()).Returns(new[]
			{
				new AutoRateInfo(Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestBAF,
					IsFromRatesService = true,
					IsInclusiveCalculator = false,
					IsSubjectTo = true,
					HasExplicitZeroAmount = true,
					CalculationDescription = "No subject-to fallback happened",
				},
				new AutoRateInfo(Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestBBK,
					IsFromRatesService = true,
					IsInclusiveCalculator = false,
					IsSubjectTo = true,
					HasExplicitZeroAmount = true,
					CalculationDescription = "No subject-to fallback happened",
				}
			});

			var expectedResults = new[]
			{
				"ProviderPK=" + org1.PK.ToString() + "|ChargeCode=" + TestBAF.AC_Code + "|Description=TESTBAF: No subject-to fallback happened",
				"ProviderPK=" + org1.PK.ToString() + "|ChargeCode=" + TestBBK.AC_Code + "|Description=TESTBBK: No subject-to fallback happened"
			};

			var viewModelsProvider = new Mock<IRateViewModelsProvider>();
			viewModelsProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
			using (_Rating.StartCost())
			using (DataRegistryRating.Instance.RateServiceFallbackSubjectToCharges.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var logger = new MemoryLogger();
				var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);
				using (var viewModel = CreateViewModel(filters))
				{
					viewModel.SelectedRate = rateMock.Object;

					var results = viewModel.GetSelectedCharges()
						.Select(r => "ProviderPK=" + r.ProviderPK.ToString() + "|ChargeCode=" + r.ChargeCode.AC_Code + "|Description=" + r.SingleLineDescription.ToString())
						.ToArray();

					AssertContainsExactElementsInAnyOrder("Expected results do not match actual results.", expectedResults, results);
				}
			}
		}

		public void TestSelectRate_SubjectToFallback_EnabledAtRegistry_OneChargeReplaced_OneChargeNonZeroNotReplaced_OneChargeNoMatchingCW1ChargeFound()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var registryValue = new FallbackSubjectToChargesCollection();
			registryValue.Add(new FallbackSubjectToCharges()
			{
				RatesProviderCode = WRConstants.RateProviders.CargoGuide,
				TransportMode = TransportModes.Air,
				ContainerMode = ContainerModes.Loose,
				IsFallbackEnabled = true
			});

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org1Rate = Helper.NewCosting(org1);
			var org2Rate = Helper.NewCosting(org2);

			var org1AirRate = org1Rate
				.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", "STD")
				.AddFlatCharge(TestBAF.AC_Code, 3000);
			org1Rate
				.AddRateEntry("AIR", "LSE", "NZAKL", "USLAX", "STD")
				.AddFlatCharge(TestBAF.AC_Code, 1234);

			var org2OrgRate = org2Rate
				.AddRateEntry("ORG", "ALL", "AUSYD", "USLAX", "STD")
				.AddFlatCharge(TestBBK.AC_Code, 2000);
			org2Rate
				.AddRateEntry("ORG", "ALL", "NZAKL", "USLAX", "STD")
				.AddFlatCharge(TestFSC.AC_Code, 4321);
			Factory.Save();

			var criteria = new TestRatingCriteria("NZAKL", "USLAX", FreightMode.LSE, 6, 1, org1);
			criteria.Carrier = org1;
			criteria.Creditors = Creditors.New();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, OrgWithSource.New(org1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(2, OrgWithSource.New(org2, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(2, OrgWithSource.New(org2, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(2, OrgWithSource.New(org2, new List<string>(new[] { "Test" })));

			var rateMock = new Mock<RateViewModel>();

			rateMock.Setup(r => r.GetAutoRateInfos()).Returns(new[]
			{
				new AutoRateInfo(Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestFRT,
					IsFromRatesService = true,
					IsInclusiveCalculator = false,
					IsSubjectTo = false,
					CalculationDescription = "This is the FRT charge that always exists from CG",
					HasExplicitZeroAmount = false,
				},
				new AutoRateInfo(Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestBAF,
					IsFromRatesService = true,
					IsInclusiveCalculator = false,
					IsSubjectTo = true,
					HasExplicitZeroAmount = true,
					CalculationDescription = "No subject-to fallback happened",
				},
				new AutoRateInfo(Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestBBK,
					IsFromRatesService = true,
					IsInclusiveCalculator = false,
					IsSubjectTo = true,
					HasExplicitZeroAmount = true,
					CalculationDescription = "No subject-to fallback happened",
				},
				new AutoRateInfo(Factory)
				{
					ProviderPK = org1.PK,
					ChargeCode = TestFSC,
					IsFromRatesService = true,
					IsInclusiveCalculator = false,
					IsSubjectTo = true,
					HasExplicitZeroAmount = false,
					CalculationDescription = "No subject-to fallback needed. Non-zero value",
				}
			});

			var expectedResults = new[]
			{
				"TESTFRT|TESTFRT: This is the FRT charge that always exists from CG",
				"TESTBAF|TESTBAF: Base Rate NZD 1234.00",
				"TESTBBK|TESTBBK: No subject-to fallback happened",
				"TESTFSC|TESTFSC: No subject-to fallback needed. Non-zero value",
			};

			var viewModelsProvider = new Mock<IRateViewModelsProvider>();
			viewModelsProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
			using (_Rating.StartCost())
			using (DataRegistryRating.Instance.RateServiceFallbackSubjectToCharges.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var logger = new MemoryLogger();
				var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);
				using (var viewModel = CreateViewModel(filters, logger: logger))
				{
					viewModel.SelectedRate = rateMock.Object;

					var results = viewModel.GetSelectedCharges().Select(r =>
						$"{r.ChargeCode.AC_Code}|{r.SingleLineDescription}");

					AssertContainsExactElementsInAnyOrder(
						"Expected charges should match the processed results",
						expectedResults,
						results
					);
				}

				var loggedMessages = logger.Logs.Select(l => l.Message).ToArray();
				AssertCollectionContains(
					"Expected specific log message for TESTBAF fallback",
					"RateLine Found  with Subject To charge fallback to RateLine TESTBAF-FLT-Costing XVBQP68SIYXQ.",
					loggedMessages
				);
				AssertCollectionContains(
					"Expected specific log message for no match",
					"RateLine Found  with Subject To charge could not fallback to Costing as no match was found.",
					loggedMessages
				);
				AssertCollectionNotContains(
					"Unexpected fallback log message for TESTFSC",
					"RateLine Found  with Subject To charge fallback to RateLine TESTFSC-FLT-Costing H5ZX52PAMCOI.",
					loggedMessages
				);
			}
		}

		#region Zero Charges

		public void TestGetSelectedRates_ApplyZeroCharges_CW1Charges()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, CW1Constants.RateMode.LSE, "NZAKL", "USLAX", TestBAF.AC_Code, 0);

			Factory.Save();

			var criteria = new TestRatingCriteria("NZAKL", "USLAX", FreightMode.LSE, 6, 1, TransportProvider1);
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(TransportProvider1, new List<string>(new[] { "Test" })));

			var gp20 = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var raterServiceFRTRateLine = CreateWiseLine("NZAKL", "USLAX", RatingConstants.RateCategory.FCL, CW1Constants.RateMode.SEA, TransportProvider1, gp20, TestFRT, 100m, "NZD");
			var rateServiceFRTCalculationResult = CalculationResult.CreateForTest(raterServiceFRTRateLine, 10, 0, 0, criteria);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var autoRateInfos = new[]
			{
				new AutoRateInfo(rateServiceFRTCalculationResult, parameters, Factory) { ProviderPK = TransportProvider2.PK, IsSubjectTo = true, HasExplicitZeroAmount = true },
			};

			AssertGetSelectedRates_ApplyZeroCharges
			(
				criteria,
				autoRateInfos,
				applyZeroCharges: true,
				expectedSelectedRates: new object[]
				{
					new { ProviderPK = TransportProvider2.PK, ChargeCode = TestFRT.AC_Code, SingleLineDescription = (ZString)"TESTFRT: Base Rate NZD 10.00" },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestBAF.AC_Code, SingleLineDescription = (ZString)"TESTBAF: Base Rate NZD 0.00" },
				}
			);

			AssertGetSelectedRates_ApplyZeroCharges
			(
				criteria,
				autoRateInfos,
				applyZeroCharges: false,
				expectedSelectedRates: new object[]
				{
					new { ProviderPK = TransportProvider2.PK, ChargeCode = TestFRT.AC_Code, SingleLineDescription = (ZString)"TESTFRT: Base Rate NZD 10.00" },
				}
			);
		}

		void AssertGetSelectedRates_ApplyZeroCharges(RatingCriteria criteria, AutoRateInfo[] autoRateInfos, bool applyZeroCharges, object[] expectedSelectedRates, string[] expectedLogs = null)
		{
			var rateViewModelMock = new Mock<RateViewModel>();
			rateViewModelMock.Setup(rateViewModel => rateViewModel.GetAutoRateInfos()).Returns(autoRateInfos);

			var registryValue = new FallbackSubjectToChargesCollection();
			registryValue.Add(new FallbackSubjectToCharges() { RatesProviderCode = WRConstants.RateProviders.CargoGuide, TransportMode = TransportModes.Air, ContainerMode = ContainerModes.Loose, IsFallbackEnabled = true });

			using (_Rating.Start(new Mock<IAutoRatingGUIInteractor>().Object))
			using (_Rating.StartCost())
			using (DataRegistryRating.Instance.RateServiceFallbackSubjectToCharges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var logger = new MemoryLogger();
				var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);
				using (var viewModel = CreateViewModel(filters, logger: logger))
				{
					viewModel.ApplyZeroCharges = applyZeroCharges;
					viewModel.SelectedRate = rateViewModelMock.Object;

					var results = viewModel.GetSelectedCharges()
						.Select(r => $"{r.ChargeCode.AC_Code}|{r.ProviderPK}|{r.SingleLineDescription}")
						.ToArray();

					var expected = expectedSelectedRates
						.Select(r => $"{r.GetType().GetProperty("ChargeCode").GetValue(r)}|{r.GetType().GetProperty("ProviderPK").GetValue(r)}|{r.GetType().GetProperty("SingleLineDescription").GetValue(r)}")
						.ToArray();

					AssertContainsExactElementsInAnyOrder(
						"The selected charges should match the expected collection",
						expected,
						results
					);
				}

				if (expectedLogs != null)
				{
					var actualLogs = logger.Logs.Select(log => log.Message).ToArray();
					foreach (var expectedLog in expectedLogs)
					{
						AssertCollectionContains("The log should contain the expected message", expectedLog, actualLogs);
					}
				}
			}
		}

		WiseLine CreateWiseLine(string origin, string destination, string category, string mode, OrgHeader carrier, MasterFiles.Business.RefContainer container, AccChargeCode chargeCode, decimal flatRate, string currency)
		{
			var charge = new Charge() { ChargeCode = chargeCode.AC_Code, Currency = currency, FlatRate = flatRate, PerUnitRate = null };
			var wiseLine = new WiseLine(Factory, charge) { TL_AC = chargeCode.PK, TL_RX_NKCurrency = currency };
			var wiseEntry = new WiseEntry(new Rate(), Factory)
			{
				TI_RateCategory = category,
				TI_Mode = mode,
				TI_OriginLRC = origin,
				TI_DestinationLRC = destination,
				TI_RC = container.PK,
				RateProvider = WRConstants.RateProviders.CargoGuide,
				TI_RateStartDate = new ZDate(2020, 06, 06),
				TI_RateEndDate = new ZDate(2030, 06, 06),
				TI_RH_NKCommodityCode = "MCLAREN",
				TI_PL_NKCarrierServiceLevel = "GOD",
				TI_ContractNumber = "ZXC02192",
				CommodityGroup = "CM1",
				ChildRateLines = new[] { wiseLine }
			};

			new WiseHeader(Factory) { TH_OH = carrier.PK, ChildRateEntries = new[] { wiseEntry } };

			return wiseLine;
		}

		public void TestGetSelectedRates_ApplyZeroCharges()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);
			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, CW1Constants.RateMode.LSE, "NZAKL", "USLAX", TestBAF.AC_Code, 10);
			var cw1FSCRateLine = rateEntry.AddFlatRateLine(TestFSC.AC_Code, 20);
			var cw1ADFRateLine = rateEntry.AddFlatRateLine(TestADF.AC_Code, 30);

			Factory.Save();

			var criteria = new TestRatingCriteria("NZAKL", "USLAX", FreightMode.LSE, 6, 1, TransportProvider1);
			criteria.Carrier = TransportProvider1;
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(TransportProvider1, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, OrgWithSource.New(TransportProvider1, new List<string>(new[] { "Test" })));

			var cw1FSCCalculationResult = CalculationResult.CreateForTest(cw1FSCRateLine, 0, 0, 0, criteria);
			var cw1ADFCalculationResult = CalculationResult.CreateForTest(cw1ADFRateLine, 40, 0, 0, criteria);

			var gp20 = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var raterServiceFRTRateLine = CreateWiseLine("NZAKL", "USLAX", RatingConstants.RateCategory.FCL, CW1Constants.RateMode.SEA, TransportProvider1, gp20, TestFRT, 100m, "NZD");
			var rateServiceFRTCalculationResult = CalculationResult.CreateForTest(raterServiceFRTRateLine, 0, 0, 0, criteria);
			var raterServiceBAFRateLine = CreateWiseLine("NZAKL", "USLAX", RatingConstants.RateCategory.FCL, CW1Constants.RateMode.SEA, TransportProvider1, gp20, TestBAF, 110m, "NZD");
			var rateServiceBAFCalculationResult = CalculationResult.CreateForTest(raterServiceBAFRateLine, 120, 0, 0, criteria);

			var raterServiceCAFRateLine = CreateWiseLine("NZAKL", "USLAX", RatingConstants.RateCategory.FCL, CW1Constants.RateMode.SEA, TransportProvider1, gp20, TestCAF, 200m, "NZD");
			var rateServiceCAFCalculationResult = CalculationResult.CreateForTest(raterServiceCAFRateLine, 0, 0, 0, criteria);
			var rateServiceWARRateLine = CreateWiseLine("NZAKL", "USLAX", RatingConstants.RateCategory.FCL, CW1Constants.RateMode.SEA, TransportProvider1, gp20, TestWAR, 210m, "NZD");
			var rateServiceWARCalculationResult = CalculationResult.CreateForTest(rateServiceWARRateLine, 220, 0, 0, criteria);

			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			var autoRateInfos = new[]
			{
				new AutoRateInfo(cw1FSCCalculationResult, parameters, Factory) { ProviderPK = TransportProvider1.PK },
				new AutoRateInfo(cw1ADFCalculationResult, parameters, Factory) { ProviderPK = TransportProvider1.PK },
				new AutoRateInfo(rateServiceFRTCalculationResult, parameters, Factory) { ProviderPK = TransportProvider1.PK, IsSubjectTo = true, HasExplicitZeroAmount = true },
				new AutoRateInfo(rateServiceBAFCalculationResult, parameters, Factory) { ProviderPK = TransportProvider1.PK, IsSubjectTo = true, HasExplicitZeroAmount = true },
				new AutoRateInfo(rateServiceCAFCalculationResult, parameters, Factory) { ProviderPK = TransportProvider1.PK },
				new AutoRateInfo(rateServiceWARCalculationResult, parameters, Factory) { ProviderPK = TransportProvider1.PK },
			};

			AssertGetSelectedRates_ApplyZeroCharges
			(
				criteria,
				autoRateInfos: autoRateInfos,
				applyZeroCharges: true,
				expectedSelectedRates: new object[]
				{
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestFSC.AC_Code, SingleLineDescription = (ZString)"TESTFSC: " },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestADF.AC_Code, SingleLineDescription = (ZString)"TESTADF: Base Rate NZD 40.00" },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestFRT.AC_Code, SingleLineDescription = (ZString)"TESTFRT: " },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestBAF.AC_Code, SingleLineDescription = (ZString)"TESTBAF: Base Rate NZD 10.00" },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestCAF.AC_Code, SingleLineDescription = (ZString)"TESTCAF: " },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestWAR.AC_Code, SingleLineDescription = (ZString)"TESTWAR: Base Rate NZD 220.00" },
				},
				expectedLogs: new[] { "RateLine Found TESTBAF--20GP-Wise Costing TRASPROV1 with Subject To charge fallback to RateLine TESTBAF-FLT-Costing TRASPROV1." }
			);

			AssertGetSelectedRates_ApplyZeroCharges
			(
				criteria,
				autoRateInfos: autoRateInfos,
				applyZeroCharges: false,
				expectedSelectedRates: new object[]
				{
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestADF.AC_Code, SingleLineDescription = (ZString)"TESTADF: Base Rate NZD 40.00" },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestBAF.AC_Code, SingleLineDescription = (ZString)"TESTBAF: Base Rate NZD 10.00" },
					new { ProviderPK = TransportProvider1.PK, ChargeCode = TestWAR.AC_Code, SingleLineDescription = (ZString)"TESTWAR: Base Rate NZD 220.00" },
				},
				expectedLogs: new[] { "RateLine Found TESTBAF-FLT-Costing TRASPROV1" }
			);
		}

		#endregion

		#endregion

		public void TestSelectRate_WhereThereIsExistingCostsWithSAAorSBABehaviour_ShouldFilterChargesHavingFreightChargeGroup()
		{
			Helper.NewAirlineOrg("EK");
			Factory.Save();

			TransportProvider1.OH_IsCreditor = true;
			TransportProvider1.OH_IsShippingProvider = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var rate = CreateTestRate("AIR", "LCL", "AUSYD", "SGSIN", "", "", "", "EK", "", "");
			var rateCharge1 = CreatePerUnitCharge(charge1.AC_Code, "KG", "AUD", 1m);
			var rateCharge2 = CreatePerUnitCharge(charge2.AC_Code, "KG", "AUD", 2m);

			rate.Charges.Add(rateCharge1);
			rate.Charges.Add(rateCharge2);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 1000m);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00100";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;
			consol.CreditorPK = TransportProvider1.PK;

			var existingConsolCost = CreateConsolCost(consol, charge3, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var expected = new []
			{
				// We expect that CC1 being filtered because there is a cost on consol with SAA rating behaviour
				new { ChargeCode = new ZString("CC2"), Amount = new ZDecimal(2000m) },
			};
			var expectedStrings = expected.Select(r => $"{r.ChargeCode}|{r.Amount}").ToArray();

			var expectedLogs = new[] { "Charge 'CC1' from selected rate, removed due to existing Costs with SAA/SBA Rating Behavior" };

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			AssertSelectedRate(criteria, rate, expectedStrings, expectedLogs);
		}

		public void TestSelectRates_WhereThereIsExistingCostsWithSAForSBFBehaviour_ShouldFilterRatesWithSameChargeCode()
		{
			Helper.NewAirlineOrg("EK");
			Factory.Save();

			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var rate = CreateTestRate("AIR", "LCL", "AUSYD", "SGSIN", "", "", "", "EK", "", "");
			var rateCharge1 = CreatePerUnitCharge(charge1.AC_Code, "KG", "AUD", 1m);
			var rateCharge2 = CreatePerUnitCharge(charge2.AC_Code, "KG", "AUD", 4m);
			var rateCharge3 = CreatePerUnitCharge(charge3.AC_Code, "KG", "AUD", 6m);

			rate.Charges.Add(rateCharge1);
			rate.Charges.Add(rateCharge2);
			rate.Charges.Add(rateCharge3);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 1000m);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00100";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var existingConsolCost = CreateConsolCost(consol, charge2, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.FreightAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var expected = new[]
			{
				new { ChargeCode = new ZString("CC1"), Amount = new ZDecimal(1000m) },
				new { ChargeCode = new ZString("CC3"), Amount = new ZDecimal(6000m) },
			};
			var expectedStrings = expected.Select(r => $"{r.ChargeCode}|{r.Amount}").ToArray();

			var expectedLogs = new[] { "Charge 'CC2' from selected rate, removed due to existing Costs with exact charge code and SAF/SBF Rating Behavior" };

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			AssertSelectedRate(criteria, rate, expectedStrings, expectedLogs);
		}

		void AssertSelectedRate(RatingCriteria criteria, Rate inputRate, string[] expectedResult, string[] expectedLogs)
		{
			var logger = new MemoryLogger();
			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);

			var rateSelectorContext = new RateSelectorContext
			{
				RatesServiceResponse = CreateRatesServiceResponse(new[] { inputRate }),
				Factory = Factory,
				Filters = filters,
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			var rateViewModels =
				rateSelectorContext.RatesServiceResponse.Rates.Select(r =>
					new CargoguideRateViewModel(r, rateSelectorContext)).ToArray();

			var provider = new Mock<IRateViewModelsProvider>();
			provider
				.Setup(p => p.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()))
				.ReturnsRateResult(rateViewModels);
			provider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (var viewModel = CreateViewModel(filters, new[] { provider.Object }, logger: logger))
			{
				viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

				// apply rate
				viewModel.SelectedRate = viewModel.Rates.First();

				var results = viewModel.GetSelectedCharges()
					.Select(r => $"{r.ChargeCode.AC_Code}|{r.Amount}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder("The selected charges should match the expected results", expectedResult, results);
			}

			AssertContainsExactElementsInAnyOrder(
				"The logged messages should include the expected logs",
				expectedLogs,
				logger.Logs.Select(l => l.Message).ToArray()
			);
		}

		public void TestSearchAsync_ShouldReportUsage_WithRatesFoundDetails()
		{
			var logger = new MemoryLogger();
			var criteria = new TestRatingCriteria();
			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);

			var cargoguideRatesProvider = new Mock<IRateViewModelsProvider>();
			cargoguideRatesProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);
			cargoguideRatesProvider
				.Setup(p => p.GetRatesAsync(filters, It.IsAny<CancellationToken>()))
				.ReturnsRateResult(new[]
				{
					CreateCargoguideRate(),
					CreateCargoguideRate(),
					CreateCargoguideRate(r => r.CarrierErrorLevel = ErrorLevel.Error),
					CreateCargoguideRate(r => r.CarrierErrorLevel = ErrorLevel.Warning),
					CreateCargoguideRate(r => r.Charges.First().Charges.First().ChargeCodeErrorLevel = ErrorLevel.Error),
					CreateCargoguideRate(r => r.Charges.First().Charges.First().ChargeCodeErrorLevel = ErrorLevel.Warning),
					CreateCargoguideRate(r =>
					{
						r.CarrierServiceLevelErrorLevel = ErrorLevel.Error;
						r.CarrierErrorLevel = ErrorLevel.Warning;
					}),
					CreateCargoguideRate(r => r.CarrierErrorLevel = ErrorLevel.Warning),
				});

			var cw1RatesProvider = new Mock<IRateViewModelsProvider>();
			cw1RatesProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);
			cw1RatesProvider
				.Setup(p => p.GetRatesAsync(filters, It.IsAny<CancellationToken>()))
				.ReturnsRateResult(new[]
				{
					CreateCW1Rate(),
					CreateCW1Rate(),
					CreateCW1Rate(r => r.Charges.First().Charges.First().ChargeCodeErrorLevel = ErrorLevel.Error),
					CreateCW1Rate(r => r.Charges.First().Charges.First().ChargeCodeErrorLevel = ErrorLevel.Warning),
					CreateCW1Rate(r => r.Charges.First().Charges.First().ChargeCodeErrorLevel = ErrorLevel.Warning),
					CreateCW1Rate(r => r.Charges.First().Charges.First().ChargeCodeErrorLevel = ErrorLevel.Warning),
				});

			using (var viewModel = CreateViewModel(filters, new[] { cargoguideRatesProvider.Object, cw1RatesProvider.Object }, calculateRates: false))
			{
				viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

				var expected = new UsageRatesSearchResult();
				expected.Cargoguide.TotalRates = 8;
				expected.Cargoguide.ErrorRates = 3;
				expected.Cargoguide.WarningRates = 4;
				expected.Cargoguide.ValidRates = 2;
				expected.CW1.TotalRates = 6;
				expected.CW1.ErrorRates = 1;
				expected.CW1.WarningRates = 3;
				expected.CW1.ValidRates = 2;

				var messages = UsageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.RateSelectorSearch);
				var message = messages[0];
				var actual = message.GetProperty<UsageRatesSearchResult>(UsageProperties.RatesSearchResult);

				var expectedString = $"{expected.Cargoguide.TotalRates}|{expected.Cargoguide.ErrorRates}|{expected.Cargoguide.WarningRates}|{expected.Cargoguide.ValidRates}|{expected.CW1.TotalRates}|{expected.CW1.ErrorRates}|{expected.CW1.WarningRates}|{expected.CW1.ValidRates}";
				var actualString = $"{actual.Cargoguide.TotalRates}|{actual.Cargoguide.ErrorRates}|{actual.Cargoguide.WarningRates}|{actual.Cargoguide.ValidRates}|{actual.CW1.TotalRates}|{actual.CW1.ErrorRates}|{actual.CW1.WarningRates}|{actual.CW1.ValidRates}";

				AssertEquals("The usage rates search result should match the expected result", expectedString, actualString);
			}
		}

		public void TestMapSelectedRates_ChargesAreNotMapped_RefreshChargesAfterUserCompletesMapping()
		{
			var ekCarrier = Helper.NewAirlineOrg("EK");
			var qfCarrier = Helper.NewAirlineOrg("QF");
			var lfCarrier = Helper.NewAirlineOrg("LH");

			Factory.Save();

			var rate1 = CreateRatesServiceRate(carrier: "EK", charges: new[]
			{
				new Charge { ChargeCode = "FRT", FlatRate = 100, Currency = "AUD" },
				new Charge { ChargeCode = "AAA", FlatRate = 200, Currency = "AUD" },
				new Charge { ChargeCode = "BBB", FlatRate = 300, Currency = "AUD", ChargeType = DTO.ChargeType.Optional },
			});

			var rate2 = CreateRatesServiceRate(carrier: "QF", charges: new[]
			{
				new Charge { ChargeCode = "CCC", FlatRate = 400, Currency = "AUD" },
			});

			var rate3 = CreateRatesServiceRate(carrier: "LH", charges: new[]
			{
				new Charge { ChargeCode = "AAA", FlatRate = 500, Currency = "AUD" },
			});

			var logger = new MemoryLogger();
			var jobCarrier = Helper.NewOrgHeader("JobCarrier");
			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.LSE, 6, 1, jobCarrier);
			criteria.Carrier = jobCarrier;
			criteria.Creditors = Creditors.New();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, OrgWithSource.New(jobCarrier, new List<string>(new[] { "Test" })));

			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var rateSelectorContext = new RateSelectorContext
			{
				RatesServiceResponse = CreateRatesServiceResponse(new[] { rate1, rate2, rate3 }),
				Factory = Factory,
				Filters = filters,
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			var rateViewModels =
				rateSelectorContext.RatesServiceResponse.Rates.Select(r =>
						new CargoguideRateViewModel(r, rateSelectorContext)).ToArray();

			var dialogService = new Mock<IDialogService>();
			dialogService
				.Setup(d => d.MapUniversalChargeCodes(
					It.Is<UniversalChargeCodeMapBizoCollection>(c => c
						.Cast<UniversalChargeCodeMapBizo>()
						.Select(b => (string)b.Code)
						.IsEquivalentTo(new[] { "AAA" }))))
				.Returns(false);

			var ratesProvider = new Mock<IRateViewModelsProvider>();
			ratesProvider
				.Setup(p => p.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(), It.IsAny<CancellationToken>()))
				.ReturnsRateResult(rateViewModels);
			ratesProvider
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (var viewModel = CreateViewModel(providers: new[] { ratesProvider.Object }, dialogService: dialogService.Object))
			{
				viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();
				viewModel.SelectedRate = rateViewModels.First(r => r.CarrierOrg == ekCarrier);

				var preConditionActual = viewModel.Rates.SelectMany(r => r.Charges.SelectMany(c => c.Charges))
					.Select(c => $"{c.ChargeCode}|{c.Amount}")
					.ToArray();
				var preConditionExpected = new[]
				{
					"FRT|100.00",
					"AAA|0",
					"BBB|0",
					"CCC|0",
					"AAA|0"
				};
				AssertContainsExactElementsInAnyOrder(
					"PRECONDITION: 0 amounts because charges are not mapped and thus can't be calculated",
					preConditionExpected,
					preConditionActual
				);

				Helper.ChargeCodes["BAF"].UniversalChargeCodeMappingsCollection.AddNew().AUP_Code = "AAA";
				Factory.Save();
				viewModel.MapSelectedRates();

				var postConditionActual = viewModel.Rates.SelectMany(r => r.Charges.SelectMany(c => c.Charges))
					.Select(c => $"{c.ChargeCode}|{c.Amount}")
					.ToArray();
				var postConditionExpected = new[]
				{
					"FRT|100.00",
					"BAF|200.00",
					"BBB|0",
					"CCC|0",
					"BAF|500.00"
				};
				AssertContainsExactElementsInAnyOrder(
					"AAA should be mapped (to BAF) and calculated. BBB is not mapped because it is optional and not selected.",
					postConditionExpected,
					postConditionActual
				);
			}
		}

		protected RateSelectorFilterStripBusinessObject CreateFilters(ILogger logger)
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 10m, 10m, creditor);
			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);

			return filters;
		}

		protected CargoguideRateViewModelSample CreateCargoguideRate(Action<CargoguideRateViewModelSample> setup = null)
		{
			var rate = new CargoguideRateViewModelSample();
			rate.CarrierErrorLevel = ErrorLevel.None;
			rate.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			rate.CommodityGroupErrorLevel = ErrorLevel.None;
			rate.Charges.SelectMany(c => c.Charges).ForEach(c => c.ChargeCodeErrorLevel = ErrorLevel.None);

			setup?.Invoke(rate);
			return rate;
		}

		protected RatesSearchResponse CreateRatesServiceResponse(IEnumerable<Rate> rates)
		{
			return new RatesSearchResponse
			{
				Rates = rates.ToArray(),
				Carriers = rates.Select(r => r.Carrier).Distinct().Select(c => new RefCarrier
				{
					Code = c,
					IATACode = c
				}).ToArray(),
				ChargeCodes = rates.SelectMany(r => r.Charges).Distinct().Select(c => new RefChargeCode()
				{
					Code = c.ChargeCode,
					Description = c.ChargeCode
				}).ToArray(),
			};
		}

		protected Rate CreateRatesServiceRate(string carrier = "EK", string commodityGroup = null, Charge[] charges = null)
		{
			return new Rate
			{
				TransportMode = "AIR",
				ContainerMode = "LCL",
				Origin = "UAIEV",
				Destination = "AUSYD",
				Carrier = carrier,
				Commodity = commodityGroup,
				Charges = charges ?? Array.Empty<Charge>()
			};
		}

		protected CW1RateViewModel CreateCW1Rate(Action<CW1RateViewModel> setup = null)
		{
			var rate = new CW1RateViewModelSample();
			rate.Charges.SelectMany(c => c.Charges).ForEach(c => c.ChargeCodeErrorLevel = ErrorLevel.None);

			setup?.Invoke(rate);
			return rate;
		}

		NonContainerizedRatesViewModel CreateViewModel(RateSelectorFilterStripBusinessObject filters = null, IEnumerable<IRateViewModelsProvider> providers = null, MemoryLogger logger = null, IDialogService dialogService = null, bool calculateRates = true)
		{
			logger = logger ?? new MemoryLogger();
			filters = filters ?? CreateFilters(logger);
			dialogService = dialogService ?? new Mock<IDialogService>().Object;

			if (providers == null)
			{
				var viewModelsProvider = new Mock<IRateViewModelsProvider>();
				viewModelsProvider
					.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
					.Returns(true);

				providers = new[] { viewModelsProvider.Object };
			}

			var context = RatingContext.CreateForManualSelect(logger, dialogService);
			return new TestNonContainerizedRatesViewModel(filters, providers, context, logger, calculateRates);
		}

		protected UsageCollectorTestHelper UsageCollectorTestHelper => usageCollectorTestHelper ?? (usageCollectorTestHelper = new UsageCollectorTestHelper(Factory));
		UsageCollectorTestHelper usageCollectorTestHelper;

		class TestNonContainerizedRatesViewModel : NonContainerizedRatesViewModel
		{
			public TestNonContainerizedRatesViewModel(
				RateSelectorFilterStripBusinessObject filters,
				IEnumerable<IRateViewModelsProvider> rateProviders,
				IRatingContext ratingContext,
				MemoryLogger logger,
				bool calculateRates = true)
				: base(filters, rateProviders, ratingContext, logger)
			{
				this.calculateRates = calculateRates;
			}

			protected override void OnRatesFound(IEnumerable<RateViewModel> newRates)
			{
				if (calculateRates)
				{
					base.OnRatesFound(newRates);
				}
				else
				{
					newRates.ForEach(r => rates.Add(r));
				}
			}

			readonly bool calculateRates;
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.RatingTests.Testing;
using Moq;
using WiseRates.Api.Model;
using WiseRatesModel = WiseRates.Api.Model;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public class RatesViewModelTest : BaseRatingIntegrationTest
	{
		#region Zero Amount Charges

		public void TestApply_UnmappedChargeCode_ZeroChargeCode_ApplyZeroCharge()
			=> TestApply_UnmappedChargeCode_ZeroChargeCode(applyZeroCharges: true, expectedMapUniversalChargeCodesDialogShown: true);

		public void TestApply_UnmappedChargeCode_ZeroChargeCode_NotApplyZeroCharge()
			=> TestApply_UnmappedChargeCode_ZeroChargeCode(applyZeroCharges: false, expectedMapUniversalChargeCodesDialogShown: false);

		void TestApply_UnmappedChargeCode_ZeroChargeCode(bool applyZeroCharges, bool expectedMapUniversalChargeCodesDialogShown)
		{
			var response = CargoguideRateViewModelTest.GetValidResponse
			(
				charges: new List<Charge>(new[]
				{
					new Charge { ChargeCode = "U_FRT", PerUnitRate = 0, Unit = "KG", Currency = "AUD" },
				}),
				chargeCodes: new[]
				{
					new RefChargeCode { Code = "U_FRT", Description = "Universal Freight" },
				}
			);

			AssertShowChargeCodeMapping
			(
				response,
				applyZeroCharges,
				expectedUnmappedCodes: expectedMapUniversalChargeCodesDialogShown ? new[] { "U_FRT" } : System.Array.Empty<string>(),
				expectedMapUniversalChargeCodesDialogShown: expectedMapUniversalChargeCodesDialogShown
			);
		}

		void AssertShowChargeCodeMapping(RatesSearchResponse response, bool applyZeroCharges, string[] expectedUnmappedCodes, bool expectedMapUniversalChargeCodesDialogShown)
		{
			var dialogService = new Mock<IDialogService>();
			dialogService.SetupSequence
			(d => d.MapUniversalChargeCodes
				(It.Is<UniversalChargeCodeMapBizoCollection>
					(c => c.Cast<UniversalChargeCodeMapBizo>()
						.Select(b => (string)b.Code)
						.IsEquivalentTo(expectedUnmappedCodes)
					)
				)
			)
			.Returns(true);

			var filters = CreateFilters(new MemoryLogger());
			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), dialogService.Object);
			var context = new RateSelectorContext
			{
				Factory = Factory,
				Filters = filters,
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory),
				RatesServiceResponse = response,
				DialogService = dialogService.Object
			};

			var rate = new CargoguideRateViewModel(response.Rates[0], context);
			rate.CarrierErrorLevel = ErrorLevel.None;
			rate.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			rate.CommodityGroupErrorLevel = ErrorLevel.None;

			rate.Calculate(
				filters.CreateCriteria(),
				filters.OriginalCriteria.Creditors?.ChargeCodeGroups);

			using (var viewModel = new RatesViewModelForTest(new[] { rate }, new[] { rate }, filters, ratingContext))
			{
				viewModel.ApplyZeroCharges = applyZeroCharges;

				Assert("Nothing to map", viewModel.MapSelectedRates());

				dialogService.Verify
				(
					d => d.MapUniversalChargeCodes(It.IsAny<UniversalChargeCodeMapBizoCollection>()),
					expectedMapUniversalChargeCodesDialogShown ? Times.Once() : Times.Never()
				);
			}
		}

		public void TestZeroAmountChargePopulatedWithNoError()
		{
			InsertChargeCode(Factory, "MSC", "Local Subject To Charge", "FLT", "FRT", "U_MSC");
			Factory.Save();

			var response = CargoguideRateViewModelTest.GetValidResponse
			(
				charges: new List<Charge>(new[]
				{
					new Charge
					{
						ChargeCode = "U_MSC",
						FlatRate = 0,
						ChargeType = ChargeType.SubjectTo,
						Currency = "USD",
						CarrierChargeCodeInfo = new CarrierSpecificChargeCode
						{
							Code = "MSC",
							Description = "Supplementary Charge"
						}
					},
				}),
				chargeCodes: new[]
				{
					new RefChargeCode { Code = "U_MSC", Description = "Universal MSC" },
				}
			);

			var filters = CreateFilters(new DummyLogger());
			var dialogService = new Mock<IDialogService>();
			var context = new RateSelectorContext
			{
				Factory = Factory,
				Filters = filters,
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory),
				RatesServiceResponse = response,
				DialogService = dialogService.Object
			};

			var rate = new CargoguideRateViewModel(response.Rates[0], context);
			rate.CarrierErrorLevel = ErrorLevel.None;
			rate.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			rate.CommodityGroupErrorLevel = ErrorLevel.None;

			rate.Calculate(
				filters.CreateCriteria(),
				filters.OriginalCriteria.Creditors?.ChargeCodeGroups);

			var testCharge = rate.Charges.OfType<ChargesViewModel>().Single().Charges.Single();
			AssertEquals("The test charge amount should be zero.", 0m, testCharge.Amount);
			AssertEquals("The error level of the test charge should be 'None'.", ErrorLevel.None, testCharge.ErrorLevel);
		}

		#region Inclusive Charge

		public void TestApply_UnmappedFRTInclusiveCharge_FRTChargeIsZero_ApplyZeroCharge()
			=> TestApply_UnmappedFRTInclusiveCharge_FRTChargeIsZero(applyZeroCharges: true, expectedMapUniversalChargeCodesDialogShown: true);

		public void TestApply_UnmappedFRTInclusiveCharge_FRTChargeIsZero_NotApplyZeroCharge()
			=> TestApply_UnmappedFRTInclusiveCharge_FRTChargeIsZero(applyZeroCharges: false, expectedMapUniversalChargeCodesDialogShown: false);

		void TestApply_UnmappedFRTInclusiveCharge_FRTChargeIsZero(bool applyZeroCharges, bool expectedMapUniversalChargeCodesDialogShown)
		{
			InsertChargeCode(Factory, "L_FRT", "Local Freight Charge Code", "FLT", "FRT", "U_FRT");
			Factory.Save();

			var response = CargoguideRateViewModelTest.GetValidResponse
			(
				charges: new List<Charge>(new[]
				{
					new Charge { ChargeCode = "U_FRT", Currency = "USD" },
					new Charge { ChargeCode = "U_FUL", Currency = "USD", ChargeType = ChargeType.Included, FreightInclusiveCarriageCharge = "U_FRT" },
				}),
				chargeCodes: new[]
				{
					new RefChargeCode { Code = "U_FRT", Description = "Universal Freight" },
					new RefChargeCode { Code = "U_FUL", Description = "Universal FUL" },
				}
			);

			AssertShowChargeCodeMapping
			(
				response,
				applyZeroCharges,
				expectedUnmappedCodes: new[] { "U_FUL" },
				expectedMapUniversalChargeCodesDialogShown: expectedMapUniversalChargeCodesDialogShown
			);
		}

		public void TestApply_UnmappedInclusiveCharge_FRTChargeIsNotZero()
		{
			InsertChargeCode(Factory, "L_FRT", "Local Freight Charge Code", "FLT", "FRT", "U_FRT");
			Factory.Save();

			var response = CargoguideRateViewModelTest.GetValidResponse
			(
				charges: new List<Charge>(new[]
				{
					new Charge { ChargeCode = "U_FRT", Currency = "USD", PerUnitRate = 10m, Unit = "KG" },
					new Charge { ChargeCode = "U_FUL", Currency = "USD", ChargeType = ChargeType.Included, FreightInclusiveCarriageCharge = "U_FRT" },
				}),
				chargeCodes: new[]
				{
					new RefChargeCode { Code = "U_FRT", Description = "Universal Freight" },
					new RefChargeCode { Code = "U_FUL", Description = "Universal FUL" },
				}
			);

			AssertShowChargeCodeMapping
			(
				response,
				applyZeroCharges: false,
				expectedUnmappedCodes: new[] { "U_FUL" },
				expectedMapUniversalChargeCodesDialogShown: true // FUL is FRT inclusive charge
			);
		}

		#endregion

		#endregion

		#region MapSelectedRates

		public void TestMapSelectedRates_EverythingIsMapped_ShouldNotShowAnyPopup()
		{
			var filters = CreateFilters(new DummyLogger());
			var dialogService = new Mock<IDialogService>();
			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), dialogService.Object);
			var mappedRate = CreateValidRate();

			using (var viewModel = new RatesViewModelForTest(new[] { mappedRate }, new[] { mappedRate }, filters, ratingContext))
			{
				AssertEquals("Nothing to map", true, viewModel.MapSelectedRates());

				dialogService.Verify(
					d => d.MapUniversalChargeCodes(It.IsAny<UniversalChargeCodeMapBizoCollection>()),
					Times.Never);

				dialogService.Verify(
					d => d.MapCarrier(It.IsAny<string>()),
					Times.Never);

				dialogService.Verify(
					d => d.MapServiceLevel(It.IsAny<OrgHeader>(), It.IsAny<WiseRatesModel.RefServiceLevel>()),
					Times.Never);

				dialogService.Verify(
					d => d.MapCommodity(It.IsAny<string>()),
					Times.Never);
			}
		}

		#region Carrier

		public void TestMapSelectedRates_CarrierIsNotMapped_UserCancelsMapping_ReturnFalse()
		{
			AssertMapSelectedRates_CarrierIsNotMapped(completeMapping: false);
		}

		public void TestMapSelectedRates_CarrierIsNotMapped_UserCompletesMapping_ReturnTrue()
		{
			AssertMapSelectedRates_CarrierIsNotMapped(completeMapping: true);
		}

		void AssertMapSelectedRates_CarrierIsNotMapped(bool completeMapping)
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();

			var unmappedRate1 = CreateValidRate();
			unmappedRate1.CarrierErrorLevel = ErrorLevel.Warning;
			unmappedRate1.RawCarrier = new RefCarrier
			{
				IATACode = "QF"
			};

			var unmappedRate2 = CreateValidRate();
			unmappedRate2.CarrierErrorLevel = ErrorLevel.Warning;
			unmappedRate2.RawCarrier = new RefCarrier
			{
				IATACode = "LH"
			};

			var unmappedRate3 = CreateValidRate();
			unmappedRate3.CarrierErrorLevel = ErrorLevel.Warning;
			unmappedRate3.RawCarrier = new RefCarrier
			{
				IATACode = "AF"
			};

			var mappedRate1 = CreateValidRate();
			mappedRate1.RawCarrier = new RefCarrier
			{
				IATACode = "EK"
			};

			var mappedRate2 = CreateValidRate();
			mappedRate2.RawCarrier = new RefCarrier
			{
				IATACode = "UA"
			};

			var filters = CreateFilters(new DummyLogger());
			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(m => m.MapCarrier("QF")).Returns(carrier1).Verifiable();
			dialogService.Setup(m => m.MapCarrier("AF")).Returns(completeMapping ? carrier2 : null).Verifiable();

			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), dialogService.Object);

			var allRates = new[]
			{
				unmappedRate1,
				unmappedRate2,
				unmappedRate3,
				mappedRate1,
				mappedRate2,
			};

			var selectedRates = new[]
			{
				unmappedRate1,
				unmappedRate3,
				mappedRate1
			};

			using (var viewModel = new RatesViewModelForTest(allRates, selectedRates, filters, ratingContext))
			{
				AssertEquals("Mapping result mismatch for selected rates.", completeMapping, viewModel.MapSelectedRates());

				dialogService.VerifyAll();
				AssertEquals("Carrier mapping refresh status mismatch for unmappedRate1.", true, unmappedRate1.CarrierMappingRefreshed);
				AssertEquals("Carrier mapping refresh status mismatch for unmappedRate2.", true, unmappedRate2.CarrierMappingRefreshed);
				AssertEquals("Carrier mapping refresh status mismatch for unmappedRate3.", true, unmappedRate3.CarrierMappingRefreshed);
				AssertEquals("Carrier mapping refresh status mismatch for mappedRate1.", true, mappedRate1.CarrierMappingRefreshed);
				AssertEquals("Carrier mapping refresh status mismatch for mappedRate2.", true, mappedRate2.CarrierMappingRefreshed);
			}
		}

		#endregion

		#region CarrierServiceLevel

		public void TestMapSelectedRates_CarrierServiceLevelIsNotMapped_UserCancelsMapping_ReturnFalse()
		{
			AssertMapSelectedRates_CarrierServiceLevelIsNotMapped(completeMapping: false);
		}

		public void TestMapSelectedRates_CarrierServiceLevelIsNotMapped_UserCompletesMapping_ReturnTrue()
		{
			AssertMapSelectedRates_CarrierServiceLevelIsNotMapped(completeMapping: true);
		}

		void AssertMapSelectedRates_CarrierServiceLevelIsNotMapped(bool completeMapping)
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();

			var unmappedRate1 = CreateValidRate();
			unmappedRate1.CarrierOrg = carrier1;
			unmappedRate1.CarrierServiceLevelErrorLevel = ErrorLevel.Warning;
			unmappedRate1.RawCarrierServiceLevel = new WiseRatesModel.RefServiceLevel
			{
				Code = "EXP",
				Description = "Express"
			};

			var unmappedRate2 = CreateValidRate();
			unmappedRate2.CarrierOrg = carrier1;
			unmappedRate2.CarrierServiceLevelErrorLevel = ErrorLevel.Warning;
			unmappedRate2.RawCarrierServiceLevel = new WiseRatesModel.RefServiceLevel
			{
				Code = "PRY",
				Description = "Priority"
			};

			var unmappedRate3 = CreateValidRate();
			unmappedRate3.CarrierOrg = carrier2;
			unmappedRate3.CarrierServiceLevelErrorLevel = ErrorLevel.Warning;
			unmappedRate3.RawCarrierServiceLevel = new WiseRatesModel.RefServiceLevel
			{
				Code = "FST",
				Description = "Fast"
			};

			var mappedRate1 = CreateValidRate();
			mappedRate1.CarrierOrg = carrier3;
			mappedRate1.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			mappedRate1.RawCarrierServiceLevel = new WiseRatesModel.RefServiceLevel
			{
				Code = "AAA",
				Description = "AAA"
			};

			var mappedRate2 = CreateValidRate();
			mappedRate2.CarrierOrg = carrier1;
			mappedRate2.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			mappedRate2.RawCarrierServiceLevel = new WiseRatesModel.RefServiceLevel
			{
				Code = "BBB",
				Description = "BBB"
			};

			var filters = CreateFilters(new DummyLogger());
			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(m => m.MapServiceLevel(carrier1, unmappedRate1.RawCarrierServiceLevel)).Returns(true).Verifiable();
			dialogService.Setup(m => m.MapServiceLevel(carrier2, unmappedRate3.RawCarrierServiceLevel)).Returns(completeMapping).Verifiable();

			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), dialogService.Object);

			var allRates = new[]
			{
				unmappedRate1,
				unmappedRate2,
				unmappedRate3,
				mappedRate1,
				mappedRate2,
			};

			var selectedRates = new[]
			{
				unmappedRate1,
				unmappedRate3,
				mappedRate1
			};

			using (var viewModel = new RatesViewModelForTest(allRates, selectedRates, filters, ratingContext))
			{
				AssertEquals("The mapping result should match the expected completion state", completeMapping, viewModel.MapSelectedRates());

				dialogService.VerifyAll();

				Assert("Unmapped rate 1 should have its carrier service level refreshed", unmappedRate1.CarrierServiceLevelRefreshed);
				Assert("Unmapped rate 2 should have its carrier service level refreshed", unmappedRate2.CarrierServiceLevelRefreshed);
				Assert("Unmapped rate 3 should have its carrier service level refreshed", unmappedRate3.CarrierServiceLevelRefreshed);
				AssertEquals(
					"It has Carrier3 which wasn't among selected and thus the user didn't have a chance to change the mapping for it, so no point in refreshing mapping for this rate",
					false,
					mappedRate1.CarrierServiceLevelRefreshed
				);
				Assert("Mapped rate 2 should have its carrier service level refreshed", mappedRate2.CarrierServiceLevelRefreshed);
			}
		}

		#endregion

		#region Commodity

		public void TestMapSelectedRates_CommodityIsNotMapped_UserCancelsMapping_ReturnFalse()
		{
			AssertMapSelectedRates_CommodityNotMapped(completeMapping: false);
		}

		public void TestMapSelectedRates_CCommodityIsNotMapped_UserCompletesMapping_ReturnTrue()
		{
			AssertMapSelectedRates_CommodityNotMapped(completeMapping: true);
		}

		void AssertMapSelectedRates_CommodityNotMapped(bool completeMapping)
		{
			var unmappedRate1 = CreateValidRate();
			unmappedRate1.CommodityGroupErrorLevel = ErrorLevel.Warning;
			unmappedRate1.RawRate = new Rate { Commodity = "PHARMA" };

			var unmappedRate2 = CreateValidRate();
			unmappedRate2.CommodityGroupErrorLevel = ErrorLevel.Warning;
			unmappedRate2.RawRate = new Rate { Commodity = "AUTO" };

			var unmappedRate3 = CreateValidRate();
			unmappedRate3.CommodityGroupErrorLevel = ErrorLevel.Warning;
			unmappedRate3.RawRate = new Rate { Commodity = "BOAT" };

			var mappedRate1 = CreateValidRate();
			mappedRate1.CommodityGroupErrorLevel = ErrorLevel.None;
			mappedRate1.RawRate = new Rate { Commodity = "AAA" };

			var mappedRate2 = CreateValidRate();
			mappedRate2.CommodityGroupErrorLevel = ErrorLevel.None;
			mappedRate2.RawRate = new Rate { Commodity = "BBB" };

			var filters = CreateFilters(new DummyLogger());
			var dialogService = new Mock<IDialogService>();
			dialogService.Setup(m => m.MapCommodity("PHARMA")).Returns(true).Verifiable();
			dialogService.Setup(m => m.MapCommodity("BOAT")).Returns(completeMapping).Verifiable();

			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), dialogService.Object);

			var allRates = new[]
			{
				unmappedRate1,
				unmappedRate2,
				unmappedRate3,
				mappedRate1,
				mappedRate2,
			};

			var selectedRates = new[]
			{
				unmappedRate1,
				unmappedRate3,
				mappedRate1
			};

			using (var viewModel = new RatesViewModelForTest(allRates, selectedRates, filters, ratingContext))
			{
				AssertEquals("The mapping result should match the completeMapping value", completeMapping, viewModel.MapSelectedRates());

				dialogService.VerifyAll();
				AssertEquals("CommodityRefreshed for unmappedRate1 should be true", true, unmappedRate1.CommodityRefreshed);
				AssertEquals("CommodityRefreshed for unmappedRate2 should be true", true, unmappedRate2.CommodityRefreshed);
				AssertEquals("CommodityRefreshed for unmappedRate3 should be true", true, unmappedRate3.CommodityRefreshed);
				AssertEquals("CommodityRefreshed for mappedRate1 should be true", true, mappedRate1.CommodityRefreshed);
				AssertEquals("CommodityRefreshed for mappedRate2 should be true", true, mappedRate2.CommodityRefreshed);
			}
		}

		#endregion

		#region ChargeCode

		public void TestMapSelectedRates_ChargesAreNotMapped_AskUserToMapUnmappedSelectedCharges()
		{
			var charges = new[]
			{
				// Rate 1
				new RatesServiceChargeViewModel
				{
					ChargeCode = "FRT",
					ChargeCodeDescription = "FRT Desc",
					ChargeCodeErrorLevel = ErrorLevel.None,
				},
				new RatesServiceChargeViewModel
				{
					ChargeCode = "BAF",
					ChargeCodeDescription = "BAF Desc",
					ChargeCodeErrorLevel = ErrorLevel.Warning,
				},
				new RatesServiceChargeViewModel
				{
					ChargeCode = "CAF",
					ChargeCodeDescription = "CAF Desc",
					ChargeCodeErrorLevel = ErrorLevel.Warning,
					IsOptional = true,
					IsSelected = false
				},
				// Rate 2
				new RatesServiceChargeViewModel
				{
					ChargeCode = "WAR",
					ChargeCodeDescription = "WAR Desc",
					ChargeCodeErrorLevel = ErrorLevel.Warning,
				},
				// Rate 3
				new RatesServiceChargeViewModel
				{
					ChargeCode = "BAF",
					ChargeCodeDescription = "BAF Desc",
					ChargeCodeErrorLevel = ErrorLevel.Warning,
				}
			};

			var rate1 = CreateValidRate();
			rate1.FreightCharges.Add(charges[0]);
			rate1.FreightCharges.Add(charges[1]);
			rate1.FreightCharges.Add(charges[2]);

			var rate2 = CreateValidRate();
			rate2.FreightCharges.Add(charges[3]);

			var rate3 = CreateValidRate();
			rate3.FreightCharges.Add(charges[4]);

			var filters = CreateFilters(new DummyLogger());
			var dialogService = new Mock<IDialogService>();
			dialogService
				.SetupSequence(d => d.MapUniversalChargeCodes(
					It.Is<UniversalChargeCodeMapBizoCollection>(c => c
						.Cast<UniversalChargeCodeMapBizo>()
						.Select(b => (string)b.Code)
						.IsEquivalentTo(new[] { "BAF", "WAR" }))))
				.Returns(false)
				.Returns(true);

			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), dialogService.Object);

			using (var viewModel = new RatesViewModelForTest(new[] { rate1, rate2, rate3 }, new[] { rate1, rate2 }, filters, ratingContext))
			{
				AssertEquals("Precondition", false, viewModel.ChargeCodesRefreshed);

				// During the first call the user cancels the mapping
				var hasBeenMapped = viewModel.MapSelectedRates();
				AssertEquals("Since the user cancelled mapping", false, hasBeenMapped);
				AssertEquals(true, viewModel.ChargeCodesRefreshed);

				// During the second call the user completes the mapping
				hasBeenMapped = viewModel.MapSelectedRates();
				AssertEquals(true, hasBeenMapped);
			}
		}

		#endregion

		#endregion

		public void TestGetSelectedCharges_ReturnAutoRateNonConsolFromSelectedCharges()
		{
			var testFRT = InsertChargeCode(Factory, "TestFRT", "Freight", "FLT", "FRT", "U_FRT", false, true);
			var testSEC = InsertChargeCode(Factory, "TestSEC", "Freight", "FLT", "SEC", "U_SEC", false, false);

			var info1 = new AutoRateInfo(Factory);
			info1.ChargeCode = testFRT;

			var info2 = new AutoRateInfo(Factory);
			info2.ChargeCode = testSEC;

			var filters = CreateFilters(new DummyLogger());
			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), new Mock<IDialogService>().Object);
			var mappedRate = CreateValidRate(new[] { info1, info2 });

			using (_Rating.Start(new LoggerDecorator(), new AutoRateOptions(excludeConsolLevelChargesOnCosting: true)))
			using (_Rating.StartCost())
			{
				using (var viewModel = new RatesViewModelForTest(new[] { mappedRate }, new[] { mappedRate }, filters, ratingContext))
				{
					var charges = viewModel.GetSelectedCharges();
					AssertContainsExactElementsInAnyOrder(new[] { info2 }, charges);
				}
			}

			using (_Rating.Start(new LoggerDecorator(), new AutoRateOptions(excludeConsolLevelChargesOnCosting: false)))
			using (_Rating.StartCost())
			{
				using (var viewModel = new RatesViewModelForTest(new[] { mappedRate }, new[] { mappedRate }, filters, ratingContext))
				{
					var charges = viewModel.GetSelectedCharges();
					AssertContainsExactElementsInAnyOrder(new[] { info1, info2 }, charges);
				}
			}
		}

		public void TestSupportedSortOptions()
		{
			var filters = CreateFilters(new DummyLogger());
			var ratingContext = RatingContext.CreateForManualSelect(new DummyLogger(), new Mock<IDialogService>().Object);

			using (var viewModel = new RatesViewModelForTest(Enumerable.Empty<RateViewModel>(), Enumerable.Empty<RateViewModel>(), filters, ratingContext))
			{
				var actualPropertyNames = viewModel.SupportedSortOptions.Select(x => x.PropertyName).ToArray();
				AssertContainsExactElementsInAnyOrder("Supported sort options should only contain 'TotalPriceAmount'.", new[] { "TotalPriceAmount" }, actualPropertyNames);
			}
		}

		protected RateSelectorFilterStripBusinessObject CreateFilters(ILogger logger)
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 10m, 10m, creditor);
			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);

			return filters;
		}

		protected CargoguideRateViewModelMock CreateValidRate(IEnumerable<AutoRateInfo> autoRateInfos = null)
		{
			var rate = new CargoguideRateViewModelMock(autoRateInfos);
			rate.Origin = "UAIEV";
			rate.Destination = "AUSYD";
			rate.CarrierCode = "EMIRATES";
			rate.CarrierErrorLevel = ErrorLevel.None;
			rate.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			rate.CommodityGroupErrorLevel = ErrorLevel.None;

			return rate;
		}

		class RatesViewModelForTest : RatesViewModel
		{
			public RatesViewModelForTest(IEnumerable<RateViewModel> rates, IEnumerable<RateViewModel> selectedRates, RateSelectorFilterStripBusinessObject filters, IRatingContext ratingContext)
				: base(filters, new List<IRateViewModelsProvider>(), ratingContext, new MemoryLogger())
			{
				this.rates = rates;
				this.selectedRates = selectedRates;
			}

			public override string TransportMode { get; }
			public override string ContainerMode { get; }
			public override IEnumerable<RateViewModel> Rates => rates;
			public bool ChargeCodesRefreshed { get; private set; }
			public override IEnumerable<RateViewModel> GetSelectedRates()
			{
				return selectedRates;
			}

			protected override void RefreshCharges()
			{
				ChargeCodesRefreshed = true;
			}

			readonly IEnumerable<RateViewModel> rates;
			readonly IEnumerable<RateViewModel> selectedRates;
		}

		protected class CargoguideRateViewModelMock : CargoguideRateViewModel
		{
			public CargoguideRateViewModelMock(IEnumerable<AutoRateInfo> autoRateInfos)
			{
				this.autoRateInfos = autoRateInfos;
			}

			public bool CarrierMappingRefreshed { get; set; }
			public bool CarrierServiceLevelRefreshed { get; set; }
			public bool CommodityRefreshed { get; set; }

			public override void PopulateCarrier(IDictionary<string, OrgHeader> carriersCache = null)
			{
				CarrierMappingRefreshed = true;
			}

			public override void PopulateCarrierServiceLevel()
			{
				CarrierServiceLevelRefreshed = true;
			}

			public override void PopulateCommodity(IDictionary<string, IEnumerable<RefCommodityCode>> commoditiesCache = null)
			{
				CommodityRefreshed = true;
			}

			public override IEnumerable<AutoRateInfo> GetAutoRateInfos()
			{
				return autoRateInfos;
			}

			readonly IEnumerable<AutoRateInfo> autoRateInfos;
		}
	}
}

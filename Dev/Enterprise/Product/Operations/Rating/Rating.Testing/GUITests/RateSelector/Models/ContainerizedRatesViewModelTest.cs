using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.RatingTests.Testing;
using Moq;
using Moq.Language.Flow;
using ChargeViewModel = Enterprise.Rating.GUI.RateSelector.Models.ChargeViewModel;
using DTO = WiseRates.Api.Model;
using MeasureInfo = Enterprise.MasterFiles.Business.MeasureInfo;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public static class MoqRateProviderExtensions
	{
		public static void ReturnsRateResult<T>(
			this ISetup<IRateViewModelsProvider, Task<RateViewModelsProviderResult>> setup,
			IEnumerable<T> rateViewModels)
			where T : RateViewModel
		{
			setup.ReturnsAsync(new RateViewModelsProviderResult
			{
				Rates = rateViewModels.Cast<RateViewModel>(),
				ProviderType = RateProviderType.CW1,
				ElapsedMilliseconds = 0
			});
		}
	}

	public class ContainerizedRatesViewModelTest : BaseRatingIntegrationTest
	{
		public void TestSearchAsync_ShouldGroupWiseRatesPerContainerCommodityPair()
		{
			Helper.NewCommodity("MLRN", "CAR");
			Helper.NewCommodity("FRRI", "CAR");
			Helper.NewCommodity("HAM", "HUMAN");
			Helper.NewCommodity("VER", "HUMAN");

			var emirates = Helper.NewAirlineOrg("EK");
			var qantas = Helper.NewAirlineOrg("QF");
			var lufthansa = Helper.NewAirlineOrg("LH");

			Factory.Save();

			var carrier = Helper.NewOrgHeader("MCLAREN");
			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.ULD, 6, 1, carrier);
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-7"].PK, "MLRN", "111", new MeasureInfo.ContainerInfo(weight: 50m, volume: 1.4m));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-7"].PK, "FRRI", "222", new MeasureInfo.ContainerInfo(weight: 70m, volume: 6.9m));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-8"].PK, "HAM", "333", new MeasureInfo.ContainerInfo(weight: 20m, volume: 2.5m));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-8"].PK, "VER", "444", new MeasureInfo.ContainerInfo(weight: 100m, volume: 4.5m));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-8"].PK, "VER", "555", new MeasureInfo.ContainerInfo(weight: 85m, volume: 1.3m));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-8"].PK, "VER", "666", new MeasureInfo.ContainerInfo(weight: 220m, volume: 1.9m));

			var filters = new RateSelectorFilterStripBusinessObject(criteria, new DummyLogger());
			var rateViewModels = new List<RateViewModel>();

			// Rate Service Rates
			var ratesServiceRates = new[]
			{
				CreateCargoguideRate("EK", "LD-7", "CAR"),
				CreateCargoguideRate("QF", "LD-7", "CAR"),

				CreateCargoguideRate("EK", "LD-8", "HUMAN"),
				CreateCargoguideRate("LH", "LD-8", "HUMAN"),

				CreateCargoguideRate("EK", "LD-6", "HUMAN"),
				CreateCargoguideRate("QF", "LD-8", "XXXX"),

				CreateCargoguideRate("EK", "LD-7", RefCommodityCode.UniversalGroups.General),
				CreateCargoguideRate("QF", "LD-8", RefCommodityCode.UniversalGroups.NotClassified),
			};

			var rateSelectorContext = new RateSelectorContext
			{
				RatesServiceResponse = CreateRatesServiceResponse(ratesServiceRates),
				Factory = Factory,
				Filters = filters,
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			rateViewModels.AddRange(ratesServiceRates.Select(r => new CargoguideRateViewModel(r, rateSelectorContext)));

			var viewModelsProviderMock = new Mock<IRateViewModelsProvider>();
			viewModelsProviderMock
				.Setup(m => m.GetRatesAsync(filters, It.IsAny<CancellationToken>()))
				.ReturnsRateResult(rateViewModels);
			viewModelsProviderMock
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			var logger = new MemoryLogger();
			var ratingContext = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);

			using (var viewModel = new ContainerizedRatesViewModel(filters, new[] { viewModelsProviderMock.Object }, ratingContext, logger))
			{
				bool searchCompletedFired = false;
				viewModel.SearchCompleted += (s, e) => searchCompletedFired = true;
				viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();
				viewModel.ContainerGroups.ForEach(g => AssertEquals($"Rate loading state for group {g.ContainerCode}", false, g.IsLoadingRates));
				Assert("Expected SearchCompleted event to be raised.", searchCompletedFired);

				var actual = viewModel.ContainerGroups.Select(g => new
				{
					g.ContainerCode,
					g.CommodityCode,
					g.ContainerCount,
					g.GoodsWeight,
					g.GoodsVolume,
					Rates = g.Rates.Select(r => new
					{
						CarrierCode = r.GetCarrierToApplyToJob()?.OH_Code,
						r.ContainerType,
						r.Commodities,
						CommodityGroup = r.CommodityGroups.FirstOrDefault()
					})
				})
				.Select(i => $"{i.ContainerCode}|{i.CommodityCode}|{i.ContainerCount}|{i.GoodsWeight}|{i.GoodsVolume}|{string.Join(";", i.Rates.Select(r => $"{r.CarrierCode},{r.ContainerType},{r.CommodityGroup}"))}")
				.ToArray();

				var expected = new[]
				{
					"LD-7|MLRN|1|50|1.4|EK_AIRLINE,LD-7,CAR;QF_AIRLINE,LD-7,CAR;EK_AIRLINE,LD-7,GENL",
					"LD-7|FRRI|1|70|6.9|EK_AIRLINE,LD-7,CAR;QF_AIRLINE,LD-7,CAR;EK_AIRLINE,LD-7,GENL",
					"LD-8|HAM|1|20|2.5|EK_AIRLINE,LD-8,HUMAN;LH_AIRLINE,LD-8,HUMAN;QF_AIRLINE,LD-8,NCLS",
					"LD-8|VER|3|405|7.7|EK_AIRLINE,LD-8,HUMAN;LH_AIRLINE,LD-8,HUMAN;QF_AIRLINE,LD-8,NCLS"
				};

				AssertContainsExactElementsInAnyOrder("Container group rates comparison", expected, actual);
			}
		}

		public void TestSearchAsync_ShouldCalculateCW1Charges_PerContainerGroup()
		{
			var carrier = Helper.NewAirlineOrg("AA");
			var costing = Helper.NewCosting(carrier);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "AUSYD", "AUMEL", "FRT", 20m, container: "LD-1");
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "AUSYD", "AUMEL", "FRT", 50m, container: "LD-3");
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "", "OCART", 1m, "KG");

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.ULD, 1500m, 1.4m, carrier)
			{
				Consignee = NewClient,
				Consignor = NewClient,
				Carrier = carrier
			};
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-1"].PK, "GEN", "111", new MeasureInfo.ContainerInfo(250m, "KG", 1.4m, "M3", 1));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-1"].PK, "GEN", "222", new MeasureInfo.ContainerInfo(250m, "KG", 1.4m, "M3", 1));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-3"].PK, "GEN", "333", new MeasureInfo.ContainerInfo(500m, "KG", 1.4m, "M3", 1));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-3"].PK, "GEN", "444", new MeasureInfo.ContainerInfo(500m, "KG", 1.4m, "M3", 1));

			var parts = new RateablePartList { HasCommodity = true, HasContainerType = true, WeightUnit = "KG" };
			parts.AddPart(new RateablePart { ContainerTypePk = Helper.Containers["LD-1"].PK.ToGuid(), CommodityCode = "GEN", Weight = 250m });
			parts.AddPart(new RateablePart { ContainerTypePk = Helper.Containers["LD-1"].PK.ToGuid(), CommodityCode = "GEN", Weight = 250m });
			parts.AddPart(new RateablePart { ContainerTypePk = Helper.Containers["LD-3"].PK.ToGuid(), CommodityCode = "GEN", Weight = 500m });
			parts.AddPart(new RateablePart { ContainerTypePk = Helper.Containers["LD-3"].PK.ToGuid(), CommodityCode = "GEN", Weight = 500m });
			criteria.RateableMeasures.AddPartList(MeasureType.Weight, parts);

			var filters = new RateSelectorFilterStripBusinessObject(criteria, new DummyLogger());
			var logger = new MemoryLogger();
			var ratingContext = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var cwRatesProvider = new CW1RateViewModelsProvider(ratingContext, logger);

			using (var viewModel = new ContainerizedRatesViewModel(filters, new[] { cwRatesProvider }, ratingContext, logger))
			{
				bool searchCompletedFired = false;
				viewModel.SearchCompleted += (s, e) => searchCompletedFired = true;

				viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

				viewModel.ContainerGroups.ForEach(g => AssertEquals($"{g.ContainerCode} should no longer be loading rates.", false, g.IsLoadingRates));
				Assert("Expected SearchCompleted event to be raised.", searchCompletedFired);

				var ld1Rate = viewModel.ContainerGroups.Single(cg => cg.ContainerCode == "LD-1").Rates.Single();
				var ld1Charges = ld1Rate.Charges.SelectMany(c => c.Charges)
					.Select(c => $"{c.ChargeCode}|{c.Amount}")
					.ToArray();
				var ld3Rate = viewModel.ContainerGroups.Single(cg => cg.ContainerCode == "LD-3").Rates.Single();
				var ld3Charges = ld3Rate.Charges.SelectMany(c => c.Charges)
					.Select(c => $"{c.ChargeCode}|{c.Amount}")
					.ToArray();

				var expectedLd1Charges = new[]
				{
					"OCART|500.00",
					"FRT|20.00"
				};

				var expectedLd3Charges = new[]
				{
					"OCART|1000.00",
					"FRT|50.00"
				};

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder("LD1 charges are not as expected.", expectedLd1Charges, ld1Charges);
					AssertContainsExactElementsInAnyOrder("LD3 charges are not as expected.", expectedLd3Charges, ld3Charges);
				});
			}
		}

		public void TestMapSelectedRates_ChargesAreNotMapped_AskUserToMapUnmappedSelectedChargesAndRecalculateCharges()
		{
			Helper.NewCommodity("MLRN", "CAR");
			Helper.NewAirlineOrg("EK");

			Factory.Save();

			var rate1 = CreateCargoguideRate(container: "LD-7");
			rate1.ContainerMode = "FCL";
			rate1.Charges = new List<DTO.Charge>(new[]
			{
				new DTO.Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 100,
					Unit = "CN",
					Currency = "AUD"
				},
				new DTO.Charge
				{
					ChargeCode = "U_CAF",
					PerUnitRate = 120,
					Unit = "CN",
					Currency = "UAH"
				},
				new DTO.Charge
				{
					ChargeCode = "U_WAR",
					PerUnitRate = 140,
					Unit = "CN",
					Currency = "USD"
				}
			});

			var rate2 = CreateCargoguideRate(container: "LD-8");
			rate2.ContainerMode = "FCL";
			rate2.Charges = new List<DTO.Charge>(new[]
			{
				new DTO.Charge
				{
					ChargeCode = "FRT",
					PerUnitRate = 200,
					Unit = "CN",
					Currency = "AUD"
				},
				new DTO.Charge
				{
					ChargeCode = "U_BAF",
					PerUnitRate = 220,
					Unit = "CN",
					Currency = "UAH"
				},
				new DTO.Charge
				{
					ChargeCode = "U_WAR",
					PerUnitRate = 240,
					Unit = "CN",
					Currency = "USD"
				}
			});

			var carrier = Helper.NewOrgHeader("MCLAREN");
			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.ULD, 6, 1, carrier);
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-7"].PK, "MLRN", "1", new MeasureInfo.ContainerInfo());
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-8"].PK, "MLRN", "1", new MeasureInfo.ContainerInfo());

			var logger = new MemoryLogger();
			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var rateSelectorContext = new RateSelectorContext
			{
				RatesServiceResponse = CreateRatesServiceResponse(new[] { rate1, rate2 }),
				Factory = Factory,
				Filters = filters,
				Logger = new MemoryLogger(),
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			var rate1ViewModel = new CargoguideRateViewModel(rate1, rateSelectorContext);
			var rate2ViewModel = new CargoguideRateViewModel(rate2, rateSelectorContext);

			var dialogService = new Mock<IDialogService>();
			dialogService
				.SetupSequence(d => d.MapUniversalChargeCodes(
					It.Is<UniversalChargeCodeMapBizoCollection>(c => c
						.Cast<UniversalChargeCodeMapBizo>()
						.Select(b => (string)b.Code)
						.SequenceEqual(new[] { "U_BAF", "U_WAR" }))))
				.Returns(false)
				.Returns(true);

			var ratingContext = RatingContext.CreateForManualSelect(logger, dialogService.Object);
			var viewModelsProviderMock = new Mock<IRateViewModelsProvider>();
			viewModelsProviderMock
				.Setup(m => m.GetRatesAsync(It.IsAny<RateSelectorFilterStripBusinessObject>(),
					It.IsAny<CancellationToken>()))
				.ReturnsRateResult(new[] { rate1ViewModel, rate2ViewModel });
			viewModelsProviderMock
				.Setup(m => m.IsApplicable(It.IsAny<RateSelectorFilterStripBusinessObject>()))
				.Returns(true);

			using (var viewModel = new ContainerizedRatesViewModel(filters, new[] { viewModelsProviderMock.Object }, ratingContext, logger))
			{
				viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

				// PRECONDITION: Only mapped charges should be calculated, i.e. FRT
				var actualPrecondition = viewModel.Rates.Select(r => new
				{
					r.ContainerType,
					Charges = r.Charges.SelectMany(c => c.Charges).Select(c => new
					{
						c.ChargeCode,
						c.Amount,
						c.Currency
					}).Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}")
				}).Select(r => $"{r.ContainerType}|{string.Join(";", r.Charges)}").ToArray();

				var expectedPrecondition = new[]
				{
					"LD-7|FRT|100|AUD;U_CAF|0|;U_WAR|0|",
					"LD-8|FRT|200|AUD;U_BAF|0|;U_WAR|0|"
				};

				AssertContainsExactElementsInAnyOrder(
					"Precondition check failed: Only mapped charges should be calculated.",
					expectedPrecondition,
					actualPrecondition
				);

				// Select a rate with some charges
				var tab2Rates = viewModel.ContainerGroups.First(c => c.ContainerCode == "LD-8");
				tab2Rates.SelectedRate = tab2Rates.Rates[0];

				// Ask the user to map charges codes which he doesn't map and cancels mapping
				AssertEquals(
					"Since the user cancelled mapping",
					false,
					viewModel.MapSelectedRates()
				);

				// Nothing should change since nothing has been mapped
				var actualAfterCancel = viewModel.Rates.Select(r => new
				{
					r.ContainerType,
					Charges = r.Charges.SelectMany(c => c.Charges).Select(c => new
					{
						c.ChargeCode,
						c.Amount,
						c.Currency
					}).Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}")
				}).Select(r => $"{r.ContainerType}|{string.Join(";", r.Charges)}").ToArray();

				AssertContainsExactElementsInAnyOrder(
					"Nothing should change since nothing has been mapped.",
					expectedPrecondition,
					actualAfterCancel
				);

				// Ask the user to map again and this time he completes mapping
				InsertChargeCode(Factory, "GBAF", "BAF", "FLT", "FRT", "U_BAF");
				InsertChargeCode(Factory, "GWAR", "WAR", "FLT", "FRT", "U_WAR");
				Factory.Save();
				AssertEquals(true, viewModel.MapSelectedRates());

				// Newly mapped charges must be calculated now, i.e. BAF and WAR but not CAF which has not been mapped
				var actualAfterMapping = viewModel.Rates.Select(r => new
				{
					r.ContainerType,
					Charges = r.Charges.SelectMany(c => c.Charges).Select(c => new
					{
						c.ChargeCode,
						c.Amount,
						c.Currency
					}).Select(c => $"{c.ChargeCode}|{c.Amount}|{c.Currency}")
				}).Select(r => $"{r.ContainerType}|{string.Join(";", r.Charges)}").ToArray();

				var expectedAfterMapping = new[]
				{
					"LD-7|FRT|100|AUD;U_CAF|0|;GWAR|140|USD",
					"LD-8|FRT|200|AUD;GBAF|220|UAH;GWAR|240|USD"
				};

				AssertContainsExactElementsInAnyOrder(
					"Newly mapped charges must now be calculated.",
					expectedAfterMapping,
					actualAfterMapping
				);
			}
		}

		public void TestCanApply_ShouldReturnTrueIfAtLeastOneRateSelected()
		{
			using (var viewModel = new ContainerizedRatesViewModel())
			{
				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-7", "McLaren", 2, new[]
				{
					CreateRateViewModel(carrier: "Emirates"),
					CreateRateViewModel(carrier: "Qantas"),
				}));
				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-8", "McLaren", 2, new[]
				{
					CreateRateViewModel(carrier: "Emirates"),
					CreateRateViewModel(carrier: "KLM"),
				}));
				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-9", "McLaren", 2, new List<RateViewModel>()));

				AssertEquals("0 out of 2 tabs with rates have selected rate", false, viewModel.CanApply);

				var propertyChanges = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);
				viewModel.ContainerGroups[0].SelectedRate = viewModel.ContainerGroups[0].Rates[1];
				AssertEquals("1 out of 2 tabs with rates have selected rate", true, viewModel.CanApply);
				AssertCollectionContains("Property change event for 'CanApply' was expected", nameof(viewModel.CanApply), propertyChanges);

				var propertyChanges1 = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges1.Add(e.PropertyName);

				// Selecting Emirates in the second group will select Emirates rate in the first group
				viewModel.ContainerGroups[1].SelectedRate = viewModel.ContainerGroups[1].Rates[0];
				AssertEquals("2 out of 2 tabs with rates have selected rate", true, viewModel.CanApply);
				AssertCollectionNotContains("Property change event for 'CanApply' was not expected", nameof(viewModel.CanApply), propertyChanges1);

				var propertyChanges2 = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges2.Add(e.PropertyName);

				viewModel.ContainerGroups[1].SelectedRate = null;
				AssertEquals("1 out of 2 tabs with rates have selected rate", true, viewModel.CanApply);
				AssertCollectionNotContains("Property change event for 'CanApply' was not expected", nameof(viewModel.CanApply), propertyChanges2);

				var propertyChanges3 = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges3.Add(e.PropertyName);

				viewModel.ContainerGroups[0].SelectedRate = null;
				AssertEquals("0 out of 2 tabs with rates have selected rate", false, viewModel.CanApply);
				AssertCollectionContains("Property change event for 'CanApply' was expected", nameof(viewModel.CanApply), propertyChanges3);
			}
		}

		public void TestCanApply_ShouldReturnTrueIfAllSelectedRatesAreValid()
		{
			using (var viewModel = new ContainerizedRatesViewModel())
			{
				var propertyChanges = new List<string>();
				viewModel.PropertyChanged += (s, e) => propertyChanges.Add(e.PropertyName);

				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-7", "McLaren", 2, new[]
				{
					CreateRateViewModel(),
					CreateRateViewModel(),
				}));
				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-8", "McLaren", 2, new[]
				{
					CreateRateViewModel(),
					CreateRateViewModel(),
				}));

				viewModel.ContainerGroups[0].SelectedRate = viewModel.ContainerGroups[0].Rates[0];
				viewModel.ContainerGroups[1].SelectedRate = viewModel.ContainerGroups[1].Rates[0];
				AssertEquals("2 out of 2 selected rates are valid", true, viewModel.CanApply);

				// Making the selected rate invalid
				propertyChanges.Clear();
				((CargoguideRateViewModel)viewModel.ContainerGroups[1].SelectedRate).CarrierErrorLevel = ErrorLevel.Error;
				AssertCollectionContains("Property change event for CanApply is expected", nameof(viewModel.CanApply), propertyChanges);
				AssertEquals("1 out of 2 selected rates is valid", false, viewModel.CanApply);

				// Selecting the valid rate
				propertyChanges.Clear();
				viewModel.ContainerGroups[1].SelectedRate = viewModel.ContainerGroups[1].Rates[1];
				AssertCollectionContains("Property change event for CanApply is expected", nameof(viewModel.CanApply), propertyChanges);
				AssertEquals("2 out of 2 selected rates are valid", true, viewModel.CanApply);
			}
		}

		public void TestRates_ShouldReturnRatesFromAllGroups()
		{
			using (var viewModel = new ContainerizedRatesViewModel())
			{
				var rates = new[]
				{
					CreateRateViewModel(),
					CreateRateViewModel(),
					CreateRateViewModel(),
					CreateRateViewModel(),
				};

				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-7", "McLaren", 2, new[]
				{
					rates[0],
					rates[1],
				}));
				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-8", "McLaren", 2, new[]
				{
					rates[2],
					rates[3],
				}));

				AssertContainsExactElementsInAnyOrder(rates, viewModel.Rates);
			}
		}

		public void TestGetSelectedRates_ShouldReturnSelectedRatesFromAllGroups()
		{
			using (var viewModel = new ContainerizedRatesViewModel())
			{
				var rates = new[]
				{
					CreateRateViewModel(),
					CreateRateViewModel(),
					CreateRateViewModel(),
					CreateRateViewModel(),
				};

				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-7", "McLaren", 2, new[]
				{
					rates[0],
					rates[1],
				}));
				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("LD-8", "McLaren", 2, new[]
				{
					rates[2],
					rates[3],
				}));

				viewModel.ContainerGroups[0].SelectedRate = rates[1];
				viewModel.ContainerGroups[1].SelectedRate = rates[3];

				var selectedRates = viewModel.GetSelectedRates();
				AssertContainsExactElementsInAnyOrder(new[] { rates[1], rates[3] }, selectedRates);
			}
		}

		public void TestReportRatesLoaded_WithValidStats_ShouldReportUsageEvent()
		{
			var carrier = Helper.NewAirlineOrg("AA");
			var costing = Helper.NewCosting(carrier);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "AUSYD", "AUMEL", "FRT", 20m, container: "LD-1");
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "AUSYD", "AUMEL", "FRT", 50m, container: "LD-3");
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "", "OCART", 1m, "KG");

			Factory.Save();

			var criteria = new TestRatingCriteria("AUSYD", "AUMEL", FreightMode.ULD, 1500m, 1.4m, carrier) { Consignee = NewClient, Consignor = NewClient, Carrier = carrier };
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-1"].PK, "GEN", "111", new MeasureInfo.ContainerInfo(250m, "KG", 1.4m, "M3", 1));
			criteria.RateableMeasures.AddContainerWithCommodityAndNumber(Helper.Containers["LD-3"].PK, "GEN", "222", new MeasureInfo.ContainerInfo(500m, "KG", 1.4m, "M3", 1));

			var parts = new RateablePartList { HasCommodity = true, HasContainerType = true, WeightUnit = "KG" };
			parts.AddPart(new RateablePart { ContainerTypePk = Helper.Containers["LD-1"].PK.ToGuid(), CommodityCode = "GEN", Weight = 250m });
			parts.AddPart(new RateablePart { ContainerTypePk = Helper.Containers["LD-3"].PK.ToGuid(), CommodityCode = "GEN", Weight = 500m });
			criteria.RateableMeasures.AddPartList(MeasureType.Weight, parts);

			var filters = new RateSelectorFilterStripBusinessObject(criteria, new DummyLogger());
			var logger = new MemoryLogger();
			var ratingContext = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var cwRatesProvider = new CW1RateViewModelsProvider(ratingContext, logger);

			using (var viewModel = new ContainerizedRatesViewModel(filters, new[] { cwRatesProvider }, ratingContext, logger))
			{
				viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

				var stats = new RatesLoadedStats()
				{
					LoadedRatesCount = 3,
					FilteredRatesCount = 3,
				};

				var helper = new UsageCollectorTestHelper(Factory);
				var messages = helper.LoadUsageMessages(UsageFeatures.Codes.RatesLoaded);
				var message = messages.First();

				var actualStats = message.GetProperty<RatesLoadedStats>(UsageProperties.RatesLoadedStats);
				var elapsedTime = message.GetProperty<long>(UsageProperties.ElapsedTime);
				var dbStats = message.GetProperty<DbStats>(UsageProperties.DBStats);
				AssertNotNull("The RatesLoadedStats should be reported in the usage event RLD.", actualStats);
				AssertNotNull("The DBStats should be reported in the usage event RLD.", dbStats);

				AssertEquals("LoadedRatesCount should match", stats.LoadedRatesCount, actualStats.LoadedRatesCount);
				AssertEquals("FilteredRatesCount should match", stats.FilteredRatesCount, actualStats.FilteredRatesCount);
				AssertGreaterThan("TotalElapsedTimeMs should match", elapsedTime, 0);
			}
		}

		#region GetSelectedCharges

		/// <summary>
		///		It is for a case if there is a flat charge without container, i.e. it applies once regardless of how many containers are on the job.
		///		But, since in rate selector we allow a user to select a completely calculated rate per each container group (i.e. tabs in rate selector),
		///		such flat charge gets duplicated to cards in each group. For example:
		///
		///		The shipment has:
		///			2 x AKE container
		///			1 x RKN container
		///
		///		Charges for Emirates:
		///			FRT, $1000 per AKE container
		///			FRT, $1500 per RKN container
		///			FUL, $500
		///
		///		We will have the following cards in rate selector:
		///		2xAKE			1xRKN
		///
		///		Emirates		Emirates
		///		FRT $2000		FRT $1500
		///		FUL $500		FUL
		///
		///		But, when these 2 cards are selected, the following charges should be applied:
		///		FRT $2000
		///		FRT $1500
		///		FUL $500
		/// </summary>
		public void TestGetSelectedCharges_MultipleRatesWithSameFlatCharge_ShouldReturnFlatChargeOnce()
		{
			var carrier = Helper.NewAirlineOrg("AA");
			var costing = Helper.NewCosting(carrier);
			var ake = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "UAIEV", "AUSYD");
			var rkn = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.ULD, "UAIEV", "AUSYD");
			var ful = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "UAIEV", "AUSYD");

			var frt1 = rkn.AddUnitRateLine("FRT", 1000, "CN");
			var frt2 = ake.AddUnitRateLine("FRT", 1500, "CN");
			var odoc = ful.AddFlatCharge("ODOC", 500);

			var logger = new MemoryLogger();
			var criteria = new TestRatingCriteria("UAIEV", "AUSYD", FreightMode.ULD, 6, 1, carrier);
			var filters = new RateSelectorFilterStripBusinessObject(criteria, logger);

			var rateSelectorContext = new RateSelectorContext
			{
				Factory = Factory,
				Logger = logger,
				CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory)
			};

			var ratingContext = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);

			using (var viewModel = new ContainerizedRatesViewModel(filters, new[] { new Mock<IRateViewModelsProvider>().Object }, ratingContext, logger))
			{
				var akeRate = CreateCW1RateViewModel(charges: new[]
				{
					new CW1ChargeViewModel(new AutoRateInfo(Factory, frt1), rateSelectorContext),
					new CW1ChargeViewModel(new AutoRateInfo(Factory, odoc), rateSelectorContext)
				});

				var rknRate = CreateCW1RateViewModel(charges: new[]
				{
					new CW1ChargeViewModel(new AutoRateInfo(Factory, frt2), rateSelectorContext),
					new CW1ChargeViewModel(new AutoRateInfo(Factory, odoc), rateSelectorContext)
				});

				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("AKE", "McLaren", 2, new[] { akeRate }));
				viewModel.ContainerGroups.Add(new ContainerGroupViewModel("RKN", "McLaren", 2, new[] { rknRate }));

				viewModel.ContainerGroups[0].SelectedRate = akeRate;
				viewModel.ContainerGroups[1].SelectedRate = rknRate;

				var selectedCharges = viewModel.GetSelectedCharges();
				AssertContainsExactElementsInAnyOrder(new[] { frt1, frt2, odoc }, selectedCharges.Select(c => c.Line));
			}
		}

		#endregion

		protected DTO.Rate CreateCargoguideRate(string carrier = "EK", string container = "LD-7", string commodityGroup = null)
		{
			return new DTO.Rate()
			{
				TransportMode = "AIR",
				ContainerMode = "FCL",
				Origin = "UAIEV",
				Destination = "AUSYD",
				Carrier = carrier,
				Container = new DTO.RefContainer { Code = container },
				Commodity = commodityGroup,
				Charges = Array.Empty<DTO.Charge>()
			};
		}

		protected DTO.RatesSearchResponse CreateRatesServiceResponse(IEnumerable<DTO.Rate> rates)
		{
			return new DTO.RatesSearchResponse
			{
				Rates = rates.ToArray(),
				Carriers = rates.Select(r => r.Carrier).Distinct().Select(c => new DTO.RefCarrier
				{
					Code = c,
					IATACode = c
				}).ToArray(),
				ChargeCodes = rates.SelectMany(r => r.Charges).Distinct().Select(c => new DTO.RefChargeCode()
				{
					Code = c.ChargeCode,
					Description = c.ChargeCode
				}).ToArray(),
			};
		}

		protected CargoguideRateViewModel CreateRateViewModel(string carrier = "EMIRATES", string container = "LD-7", string commodity = null, string commodityGroup = null)
		{
			var rate = new CargoguideRateViewModel();
			rate.Origin = "UAIEV";
			rate.Destination = "AUSYD";
			rate.CarrierCode = carrier;
			rate.ContainerType = container;
			rate.CarrierErrorLevel = ErrorLevel.None;
			rate.CarrierServiceLevelErrorLevel = ErrorLevel.None;
			rate.CommodityGroupErrorLevel = ErrorLevel.None;
			rate.Commodities = commodity;
			rate.CommodityGroups = !string.IsNullOrEmpty(commodityGroup) ? new[] { commodityGroup } : Array.Empty<string>();

			return rate;
		}

		protected CW1RateViewModel CreateCW1RateViewModel(string carrier = "EMIRATES", string container = "LD-7", string commodity = null, string commodityGroup = null, IEnumerable<ChargeViewModel> charges = null)
		{
			var rate = new CW1RateViewModel();
			rate.Origin = "UAIEV";
			rate.Destination = "AUSYD";
			rate.CarrierCode = carrier;
			rate.ContainerType = container;
			rate.Commodities = commodity;
			rate.CommodityGroups = !string.IsNullOrEmpty(commodityGroup) ? new[] { commodityGroup } : Array.Empty<string>();
			charges.ForEach(c => rate.FreightCharges.Add(c));

			return rate;
		}
	}
}

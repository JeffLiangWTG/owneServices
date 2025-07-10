using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.ZArchitecture.Business;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models.CW1RateViewModelsProviderTest
{
	public class MiscTest : BaseTest
	{
		public override bool IsContainerised => false; // Because all tests in this file are based on non-containerised

		public void TestPopulateCalculatedCW1_WhenFilterCarrierIsEmpty_ShouldCalculateCW1RateForAllCarriers_ExceptEmptyAirlines()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			creditor.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;

			var chargeCodeFRT = Helper.ChargeCodes["FRT"];
			chargeCodeFRT.AC_DepartmentFilterList = "ALL";

			var criteria = new TestRatingCriteria(DefaultOrigin, DefaultDestination, FreightMode.LSE, 1500, 1, null);
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, new[] { OrgWithSource.New(creditor, new List<string> { "Test" }) });

			var logger = new MemoryLogger();
			var cw1Provider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, null, null, true);

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, DefaultOrigin, DefaultDestination);
			costEntry.AddRateLine(chargeCodeFRT, UnitCalculator.Code, Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 5;

			var anotherCreditor = Factory.NewWithValidTestData<OrgHeader>(); // MiscServ.OM_RM_Airline is empty by default
			var anotherCost = Helper.NewCosting(anotherCreditor);
			var anotherCostEntry = anotherCost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, DefaultOrigin, DefaultDestination);
			anotherCostEntry.AddRateLine(chargeCodeFRT, UnitCalculator.Code, Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 7;

			Factory.Save();

			anotherCreditor.CompanyData.OB_IsCreditor = false;
			Factory.Save();

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			AssertEquals("Filter carriers count should be 0 when none are available", 0, filter.Carriers.Count());

			var provider = new CW1RateViewModelsProvider(context);
			var carriers = provider.GetRatesAsync(filter)
				.GetAwaiter()
				.GetResult()
				.Select(x => (x as CW1RateViewModel)?.ServiceProviderCode)
				.WhereNotNull()
				.Distinct()
				.ToList();
			AssertContainsExactElementsInAnyOrder(
				"Carriers should only include the expected creditor's codes initially",
				new[] { creditor.OH_Code },
				carriers
			);

			anotherCreditor.MiscServ.OM_RM_Airline = Factory.NewWithValidTestData<RefAirline>().PK;
			Factory.Save();

			carriers = provider.GetRatesAsync(filter)
				.GetAwaiter()
				.GetResult()
				.Select(x => (x as CW1RateViewModel)?.ServiceProviderCode)
				.WhereNotNull()
				.Distinct()
				.ToList();
			AssertContainsExactElementsInAnyOrder(
				"Carriers should include both the creditor and the updated creditor with airline codes",
				new[] { creditor.OH_Code, anotherCreditor.OH_Code },
				carriers
			);
		}

		public void TestPopulateCalculatedCW1_WhenSelectedCarrierOnFilterStripHasAnEmptyChargeCodeGroup_ShouldStillCalculateCW1RateForSelectedCarrier()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var chargeCodeFrt = Helper.ChargeCodes["FRT"];
			var chargeCodeBAF = Helper.ChargeCodes["BAF"];

			var criteria = new TestRatingCriteria(DefaultOrigin, DefaultDestination, FreightMode.LSE, 1000, 1, null);
			criteria.Creditors = Creditors.New(OrgWithSource.New(creditor, new List<string> { "Test" }));

			var logger = new MemoryLogger();
			var cw1Provider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, null, null, true);

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, DefaultOrigin, DefaultDestination);
			costEntry.RateLines.RemoveAndDeleteAll();
			costEntry.AddRateLine(chargeCodeFrt, UnitCalculator.Code, Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 5;

			var anotherCreditor = Factory.NewWithValidTestData<OrgHeader>();
			var anotherCost = Helper.NewCosting(anotherCreditor);
			var anotherCostEntry = anotherCost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, DefaultOrigin, DefaultDestination);
			anotherCostEntry.RateLines.RemoveAndDeleteAll();
			anotherCostEntry.AddRateLine(chargeCodeBAF, UnitCalculator.Code, Weight.Kilograms).GetCalculator<UnitCalculator>().PerUnit = 6;

			Factory.Save();

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);

			var carrierStrip1 = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.CarrierTransportProvider);
			((ModuleGuidFilter)carrierStrip1.CurrentModuleFilter).Property = creditor.PK;

			CombineAssertions("Filter carrier should only have one carrier", () =>
			{
				AssertEquals("Filter carrier count mismatch.", 1, filter.Carriers.Count());
				AssertEquals("Filter carrier PK mismatch.", creditor.PK, filter.Carriers.First().Org.PK);
			});

			var provider = new CW1RateViewModelsProvider(context);

			var rateViewModels = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
			var actualRates = rateViewModels
				.Select(m => m.Lines.Single().ParentRateEntry)
				.Select(r => $"{r.PK}|{r.ParentRatingHeader.Header.OH_Code}")
				.ToArray();

			var expectedRates = new[] { costEntry }
				.Select(r => $"{r.PK}|{r.Parent.Header.OH_Code}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder("The calculated rates should match the expected rates.", expectedRates, actualRates);
		}

		public void TestGetRates_RatesFromDifferentCarriers_PopulateProviderPKOnAutoRateInfos()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var carrier1 = Helper.NewCosting(TransportProvider1);
			var carrier1Rate = carrier1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
			carrier1Rate.RateLines.RemoveAndDeleteAll();
			carrier1Rate.AddRateLine(TestFRT, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5;

			var carrier2 = Helper.NewCosting(TransportProvider2);
			var carrier2Rate = carrier2.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
			carrier2Rate.RateLines.RemoveAndDeleteAll();
			carrier2Rate.AddRateLine(TestFRT, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 10;

			Factory.Save();

			var logger = new MemoryLogger();

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 1500, 1, null);

			criteria.Creditors = new Creditors();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);

			var carrierStrip1 = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.CarrierTransportProvider);
			((ModuleGuidFilter)carrierStrip1.CurrentModuleFilter).Property = TransportProvider1.PK;

			var carrierStrip2 = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.CarrierTransportProvider);
			((ModuleGuidFilter)carrierStrip2.CurrentModuleFilter).Property = TransportProvider2.PK;

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var context = RatingContext.CreateForManualSelect(new LoggerDecorator(logger), new Mock<IDialogService>().Object);
				var provider = new CW1RateViewModelsProvider(context);

				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult().Cast<CW1RateViewModel>();

				var rate1 = rates.First(r => r.ServiceProviderCode == TransportProvider1.OH_Code);
				var autoRateInfos1 = rate1.GetAutoRateInfos().Select(a => a.ProviderPK).ToArray();
				var expectedProviderPKs = Enumerable.Repeat(TransportProvider1.PK, autoRateInfos1.Length).ToArray();

				AssertContainsExactElementsInAnyOrder(
					"All auto rate infos should have the provider PK set to the transport provider.",
					expectedProviderPKs,
					autoRateInfos1);

				var rate2 = rates.First(r => r.ServiceProviderCode == TransportProvider2.OH_Code);
				var autoRateInfos2 = rate2.GetAutoRateInfos().Select(a => a.ProviderPK).ToArray();
				var expectedProviderPKs2 = Enumerable.Repeat(TransportProvider1.PK, autoRateInfos2.Length).ToArray();

				AssertContainsExactElementsInAnyOrder(
					"All auto rate infos should have the provider PK set to the transport provider.",
					expectedProviderPKs2,
					autoRateInfos2
				);
			}
		}

		public void TestGetRates_RatesFromDifferentCarriers_PopulateProviderPKOnAutoRateInfos_WhenThereIsNoFRTCharge()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var carrier1 = Helper.NewCosting(TransportProvider1);
			var carrier1Rate = carrier1.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV");
			carrier1Rate.RateLines.RemoveAndDeleteAll();
			carrier1Rate.AddRateLine(TestAWB, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5;

			Factory.Save();

			var logger = new MemoryLogger();

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 1500, 1, null);

			criteria.Creditors = new Creditors();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);

			var carrierStrip1 = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.CarrierTransportProvider);
			((ModuleGuidFilter)carrierStrip1.CurrentModuleFilter).Property = TransportProvider1.PK;

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var context = RatingContext.CreateForManualSelect(new LoggerDecorator(logger), new Mock<IDialogService>().Object);
				var provider = new CW1RateViewModelsProvider(context);

				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult().Cast<CW1RateViewModel>();

				var rate1 = rates.First(r => r.ServiceProviderCode == TransportProvider1.OH_Code);
				var actualProviderPKs = rate1.GetAutoRateInfos().Select(a => a.ProviderPK).ToArray();
				var expectedProviderPKs = Enumerable.Repeat(TransportProvider1.PK, actualProviderPKs.Length).ToArray();

				AssertContainsExactElementsInAnyOrder(
					"All auto rate infos should have the provider PK set to the transport provider.",
					expectedProviderPKs,
					actualProviderPKs
				);
			}
		}

		public void TestGetRates_IgnoreFilterStripCommodityGroupFilter()
		{
			var commodityCode2 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode2.RH_Code = "CC2";
			commodityCode2.RH_IsHazardous = true;
			commodityCode2.RH_UniversalCommodityGroup = "UCG2";

			var costing = Helper.NewCosting(TransportProvider1);

			var entry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV", "FRT", 100m);
			entry1.TI_RH_NKCommodityCode = "";

			var entry2 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV", "FRT", 200m);
			entry2.TI_RH_NKCommodityCode = "CC1";

			var entry3 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "UAIEV", "FRT", 300m);
			entry3.TI_RH_NKCommodityCode = "CC2";

			Factory.Save();

			var logger = new MemoryLogger();

			var criteria = new TestRatingCriteria("AUSYD", "UAIEV", FreightMode.LSE, 1500, 1, null);
			criteria.Creditors = new Creditors();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });

			criteria.SetCommodityCode("CC1");

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var commodityFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.UniversalCommodityGroup);
			((ModuleTextFilter)commodityFilter.CurrentModuleFilter).Property = "UCG2";

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var context = RatingContext.CreateForManualSelect(new LoggerDecorator(logger), new Mock<IDialogService>().Object);
				var provider = new CW1RateViewModelsProvider(context);

				var rateViewModels = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				var actualRates = rateViewModels
					.SelectMany(m => m.Lines.Select(l => l.ParentRateEntry).Distinct())
					.Select(r => $"{r.PK}|{r.TI_RH_NKCommodityCode}")
					.ToArray();

				var expectedRates = new[] { entry1, entry2 }
					.Select(r => $"{r.PK}|{r.TI_RH_NKCommodityCode}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder("The actual and expected rates should match.", expectedRates, actualRates);
			}
		}

		public void TestGetRates_ByLocation_FilterStripHasOriginDifferentFromJob_LoadRatesForFilterStripOrigin()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rates = new[]
			{
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "UAIEV", "FRT", 100m),
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUMEL", "UAIEV", "FRT", 200m),
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "UAIEV", "FRT", 300m),
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "", "UAIEV", "FRT", 400m),
			};

			Factory.Save();

			var logger = new MemoryLogger();

			var criteria = new TestRatingCriteria("AUMEL", "UAIEV", FreightMode.LSE, 1500, 1, null);
			criteria.Creditors = new Creditors();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var originDestinationFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.OriginDestination);
			((ModuleLocationFilter)originDestinationFilter.CurrentModuleFilter).Property1 = "AUSYD";

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var context = RatingContext.CreateForManualSelect(new LoggerDecorator(logger), new Mock<IDialogService>().Object);
				var provider = new CW1RateViewModelsProvider(context);

				var rateViewModels = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				var actualRates = rateViewModels
					.Select(m => m.Lines.Single().ParentRateEntry)
					.Select(r => $"{r.PK}|{r.TI_OriginLRC}|{r.TI_DestinationLRC}")
					.ToArray();

				var expectedRates = new[] { rates[0] }
					.Select(r => $"{r.PK}|{r.TI_OriginLRC}|{r.TI_DestinationLRC}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder(
					"The FilterStip value is AUSYD and it is more specific than AU and empty origin which also match the FilterStrip value",
					expectedRates,
					actualRates
				);
			}
		}

		public void TestGetRates_ByLocation_FilterStripHasDestinationDifferentFromJob_LoadRatesForFilterStripDestination()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rates = new[]
			{
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "UAIEV", "FRT", 100m),
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "USLAX", "FRT", 200m),
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "UA", "FRT", 300m),
				costing.AddRateEntryWithFlatRateLine("AIR", "LSE", "AUSYD", "", "FRT", 400m),
			};

			Factory.Save();

			var logger = new MemoryLogger();

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 1500, 1, null);
			criteria.Creditors = new Creditors();
			criteria.Creditors[ChargeCodeGroupList.Codes.Freight].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Origin].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });
			criteria.Creditors[ChargeCodeGroupList.Codes.Destination].Add(1, new[] { OrgWithSource.New(TransportProvider1, new List<string> { "Test" }) });

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var originDestinationFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.OriginDestination);
			((ModuleLocationFilter)originDestinationFilter.CurrentModuleFilter).Property2 = "UAIEV";

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var context = RatingContext.CreateForManualSelect(new LoggerDecorator(logger), new Mock<IDialogService>().Object);
				var provider = new CW1RateViewModelsProvider(context);

				var rateViewModels = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				var actualRates = rateViewModels
					.Select(m => m.Lines.Single().ParentRateEntry)
					.Select(r => $"{r.PK}|{r.TI_OriginLRC}|{r.TI_DestinationLRC}")
					.ToArray();

				var expectedRates = new[] { rates[0] }
					.Select(r => $"{r.PK}|{r.TI_OriginLRC}|{r.TI_DestinationLRC}")
					.ToArray();

				AssertContainsExactElementsInAnyOrder(
					"The FilterStip value is UAIEV and it is more specific than UA and empty destination which also match the FilterStrip value",
					expectedRates,
					actualRates
				);
			}
		}

		public void TestGetRates_WhenRateEntryStartDateOrEndDateInvalid_ShouldShowDatesAsInvalid()
		{
			var costing = Helper.NewCosting(TransportProvider1);

			var rateEntry1 = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 100m);
			rateEntry1.TI_RateStartDate = ZDate.Invalid;
			rateEntry1.TI_RateEndDate = ZDate.Invalid;

			var criteria = new TestRatingCriteria("AUSYD", "USLAX", FreightMode.LSE, 1500, 1, null);
			var context = new RateSelectorContext();
			context.Factory = Factory;
			context.CurrencyConverter = new RefCurrenciesCurrencyConverter(Factory);

			var viewModel = new CW1RateViewModel(context, criteria, ZGuid.Empty, ZString.Empty, rateEntry1.RateLines.Cast<RateLine>(), DisposableAction.NoAction);
			viewModel.Calculate(criteria, criteria.Creditors?.ChargeCodeGroups);
			AssertEquals(DateTime.MinValue, viewModel.StartDate);
			AssertEquals(null, viewModel.ExpiryDate);
		}

		public void TestPopulateCalculatedCW1_WhenWeHaveCarrierCostWithCSTCalculatorAndStandardCost_ShouldBringCarrierCharge()
		{
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			var chargeCodeFrt = Helper.ChargeCodes["FRT"];

			var standardCosting = Helper.NewCosting(null);
			standardCosting.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, DefaultOrigin, DefaultDestination, "FRT", 10, "KG");

			var criteria = new TestRatingCriteria(DefaultOrigin, DefaultDestination, FreightMode.LSE, 1000, 1, null);
			criteria.Creditors = Creditors.New(OrgWithSource.New(creditor, new List<string> { "Test" }));

			var logger = new MemoryLogger();
			var cw1Provider = new CW1RatesProvider(Factory, logger);
			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, null, null, true);

			var cost = Helper.NewCosting(creditor);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, DefaultOrigin, DefaultDestination);
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCodeFrt, CompanyTariffOrCostBasedCalculator.CostBasedCode, QuantityUnit.KG);
			costLine.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnitPercent = 10;

			Factory.Save();

			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);
			var provider = new CW1RateViewModelsProvider(context);

			var rateViewModels = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
			var actualRates = rateViewModels
				.Select(m => m.Lines.Single().ParentRateEntry)
				.Select(r => $"{r.PK}|{r.ParentRatingHeader.Header.OH_Code}")
				.ToArray();

			var expectedRates = new[] { costEntry }
				.Select(r => $"{r.PK}|{r.Parent.Header.OH_Code}")
				.ToArray();

			AssertContainsExactElementsInAnyOrder(
				"Expected the calculated rates to match the provided rates based on the inputs.",
				expectedRates,
				actualRates
			);
		}

		#region Spot Rating Behaviour

		public void TestGetRates_ExistingCostsOnJobWithAnyRatingBehaviour_ShouldNotAffectLogic()
		{
			TransportProvider1.OH_IsCreditor = true;

			var charge1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Charge 1", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			var charge2 = Helper.ChargeCodes.NewConsolChargeCode("CC2", "Charge 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			var charge3 = Helper.ChargeCodes.NewConsolChargeCode("CC3", "Charge 3", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight);

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "SGSIN");
			costEntry.RateLines.RemoveAndDeleteAll();

			costEntry.AddFlatRateLine(charge1.AC_Code, 100m);

			var originEntry = costing.AddRateEntry("ORG", "LSE", "AUSYD", "SGSIN");
			originEntry.RateLines.RemoveAndDeleteAll();
			originEntry.AddFlatRateLine(charge2.AC_Code, 400m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "SGSIN", 1000m);

			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C00100";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.SetDefaultShippingLineAddress(TransportProvider1);
			consol.JK_OA_CreditorAddress = TransportProvider1.MainAddress.PK;

			var existingConsolCost = CreateConsolCost(consol, charge3, TransportProvider1);
			existingConsolCost.E6_RatingBehaviour = RatingBehaviours.AllInAdhocOverridingSpotRate;
			existingConsolCost.E6_OSCostAmount = 1234m;

			Factory.Save();

			var ratingAdapter = consol.RatingAdapter;
			var autoRatingProxy = new AutoRatingProxy(ratingAdapter);
			var criteria = new RatingCriteria(autoRatingProxy, Factory);

			var logger = new MemoryLogger();
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var context = RatingContext.CreateForManualSelect(new LoggerDecorator(logger), new Mock<IDialogService>().Object);
				var provider = new CW1RateViewModelsProvider(context);

				var rateViewModels = provider.GetRatesAsync(filter).GetAwaiter().GetResult();
				var actualRates = rateViewModels.SelectMany(m => m.Lines.Select(l => l.ParentRateEntry.PK)).ToArray();
				var expectedRates = new[] { costEntry.PK, originEntry.PK };

				AssertContainsExactElementsInAnyOrder("The actual rates should match the expected rates.", expectedRates, actualRates);
			}
		}

		#endregion
	}
}

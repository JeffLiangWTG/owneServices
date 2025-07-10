using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection;
using Moq;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.Business.Test
{
	public class RateChooserChargesViewModelTest : RatingTestCase
	{
		public void TestIsAnyOptionalActive()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var bolCharge1 = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge2 = Helper.ChargeCodes.NewConsolChargeCode("BL2", "Doc Fee 2 - Optional", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge3 = Helper.ChargeCodes.NewConsolChargeCode("BL3", "Doc Fee 3 - Optional", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge3.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge2 = Helper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge3 = Helper.ChargeCodes.NewConsolChargeCode("FR3", "FRT 3 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge3.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save(); // carrier mapping is a DBOnly query

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL, 3);
			ChooserHelper.AddContainer(consol, "20GP", "ATPT", Core.Constants.ContainerModes.FCL, 5);

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);
			var frtCharge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 10m)
				.OfType(ChargeType.Optional);
			var frtCharge3 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR3", 7m)
				.OfType(ChargeType.Optional);
			var docCharge1 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BOL", 30m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.BOL);
			var docCharge2 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BL2", 3m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.BOL)
				.OfType(ChargeType.Optional);
			var docCharge3 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BL3", 2m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.BOL)
				.OfType(ChargeType.Optional);

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			model.AddWiseRatesForTest(response);
			var modelTab1 = model.ContainerGroups.First();
			modelTab1.SelectedRate = modelTab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			var modelTab2 = model.ContainerGroups.Skip(1).First();
			modelTab2.SelectedRate = modelTab2.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			model.Validate();
			AssertEquals("PRE:", true, model.IsValid);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var tab1 = viewModel.ContainerTabs.First();
			var tab2 = viewModel.ContainerTabs.Skip(1).First();

			var additionalCharges = tab1.SelectedRow.OceanCharges.Charges.First(c => c.Group == ChargesViewModel.ChargesGroup.Additional);
			additionalCharges.IsAnyOptionalActive = true;

			AssertEquals("All optional charges selected", 2, additionalCharges.Charges.Count(x => x.IsActive));
			AssertEquals(true, additionalCharges.IsAnyOptionalActive);
			AssertEquals("All optional charges selected on other tab", 2, additionalCharges.Charges.Count(x => x.IsActive));

			additionalCharges.IsAnyOptionalActive = false;
			AssertEquals("All optional charges deselected", 0, additionalCharges.Charges.Count(x => x.IsActive));
			AssertEquals(false, additionalCharges.IsAnyOptionalActive);
			AssertEquals("All optional charges deselected on other tab", 0, additionalCharges.Charges.Count(x => x.IsActive));

			additionalCharges.Charges.First().IsActive = true;
			AssertEquals(true, additionalCharges.IsAnyOptionalActive);
			AssertEquals(true, additionalCharges.IsAnyOptionalActive);

			additionalCharges.Charges.First().IsActive = false;
			AssertEquals(false, additionalCharges.IsAnyOptionalActive);
			AssertEquals(false, additionalCharges.IsAnyOptionalActive);
		}

		public void TestLabels()
		{
			var services = new DummyRateChooserServices(Factory);
			var chargesViewModel = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, Enumerable.Empty<ChargeViewModel>());

			CombineAssertions("Labels", () =>
			{
				AssertEquals("UnmappedChargesLabel", "TBA:", chargesViewModel.UnmappedChargesLabel);
				AssertEquals("DSTChargesLabel", "DST:", chargesViewModel.DSTChargesLabel);
				AssertEquals("ORGChargesLabel", "ORG:", chargesViewModel.ORGChargesLabel);
				AssertEquals("FRTChargesLabel", "FRT:", chargesViewModel.FRTChargesLabel);
			});
		}

		public void TestTotalPriceCurrency()
		{
			var services = new DummyRateChooserServices(Factory);
			var chargeViewModel = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, Enumerable.Empty<ChargeViewModel>());
			AssertEquals("AUD", chargeViewModel.TotalPriceCurrency);

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD";
			chargeViewModel = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, Enumerable.Empty<ChargeViewModel>());
			AssertEquals("USD", chargeViewModel.TotalPriceCurrency);
		}

		public void TestIsAnyOptionalActive_CW1Costing()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var bolCharge1 = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save(); // carrier mapping is a DBOnly query

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL, 3);

			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			var modelTab1 = model.ContainerGroups.First();
			modelTab1.SelectedRate = modelTab1.Rates.Single(x => x.RateEntry == rateEntry1);
			model.Validate();
			AssertEquals("PRE:", true, model.IsValid);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var tab1 = viewModel.ContainerTabs.First();
			var baseCharges = tab1.SelectedRow.CW1FreightCharges;
			AssertEquals("base charges are shown", Visibility.Visible, baseCharges.OverallVisibility);
			AssertEquals("checkbox not shown", false, baseCharges.IsActiveVisibility);
			AssertEquals("CW1 costing doesn't have optional charges", false, baseCharges.IsAnyOptionalActive);
			AssertEquals("no optional charges", false, baseCharges.IsAnyOptionalActiveEnabled);
		}

		public void TestPriceGetter_BaseChargesInDifferentCurrencies_ShouldSumChargePricesInDefaultCurrency()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m, mapCodeWithCW1ChargeCode: true);
			var frtCharge2 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FR2", 10m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD");

			var frtCharge3 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FR3", 7m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD")
				.OfType(ChargeType.Optional);
			var frtCharge4 = ChooserHelper.AddPerContainerCharge(apiCosting1, "INC", 999m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD")
				.OfType(ChargeType.Included)
				.IncludedIn(frtCharge2.ChargeCode);
			var frtCharge5 = ChooserHelper.AddPerContainerCharge(apiCosting1, "BAF", 150m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("HKD");
			var frtCharge6 = ChooserHelper.AddPerContainerCharge(apiCosting1, "BAF", 75m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("NZD");

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD"; //default currency
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var results = ChooserHelper.GetCalculatedResultFromEntries(converted1, carrier, logger: logger);
			var services = new DummyRateChooserServices(Factory);
			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, results);

			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[]
			{
				new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge2, convertedLines.First(l => l.WiseCharge == frtCharge2), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge3, convertedLines.First(l => l.WiseCharge == frtCharge3), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge4, convertedLines.First(l => l.WiseCharge == frtCharge4), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge5, convertedLines.First(l => l.WiseCharge == frtCharge5), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge6, convertedLines.First(l => l.WiseCharge == frtCharge6), chooserRateEntry1, true),
			});

			AssertEquals("Total price does not match expected value", 152.50m, charges.TotalPrice);
			AssertEquals("Total price string does not match expected value", "$152.50", charges.TotalPriceString);
			AssertEquals("Total price currency does not match expected value", "USD", charges.TotalPriceCurrency);
		}

		public void TestPriceGetter_BaseChargesInMissingCurrencies_TotalPriceStringShouldBeEmpty()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("USD");
			var frtCharge2 = ChooserHelper.AddPerContainerCharge(apiCosting1, "BAF", 150m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("HKD");

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD"; //default currency
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var results = ChooserHelper.GetCalculatedResultFromEntries(converted1, carrier, logger: logger);
			var services = new DummyRateChooserServices(Factory, "HKD");
			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, results);

			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[]
			{
				new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge2, convertedLines.First(l => l.WiseCharge == frtCharge2), chooserRateEntry1, true),
			});

			AssertEquals("The total price should be 100.00.", 100.00m, charges.TotalPrice);
			AssertNullOrEmpty("The total price string should be empty.", charges.TotalPriceString);
			AssertEquals("The total price currency should be USD.", "USD", charges.TotalPriceCurrency);
		}

		public void TestCalculationIcon_WhenFrtChargeIsInCalculation_ShouldCollapsed()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedRates = converter.Convert(response, null);
			var convertedRate = convertedRates.Single();
			var convertedLine = convertedRate.ChildRateLines.Single();

			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var ratingCriteria = new RatingCriteria(null, Factory);

			var info1 = new AutoRateInfo(Factory)
			{
				ChargeCode = Helper.ChargeCodes["FRT"],
				Currency = "AUD",
				ChargeUnit = "KG"
			};
			info1.SetLine_ForTest(convertedLine);

			info1.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
			var collection = new AutoRateInfoCollection(Factory) { info1 };

			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, convertedRates, services, collection);

			var convertedLines = convertedRates.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[]
			{
				new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry1, true),
			});

			AssertEquals("Calculation icon visibility should be collapsed when FRT charge is in calculation.", false, charges.CalculationIconVisibility);
		}

		public void TestCalculationIcon_WhenFrtChargeIsInNotInCalculation_ShouldBeWarning()
		{
			var testHelper = new TestHelper(Factory);
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			var freightCharge2 = testHelper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge2.AC_GC = GlbCompany.CurrentCompany.PK;

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 10m);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var ratingCriteria = new RatingCriteria(null, Factory);

			var info1 = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FR2"], Currency = "AUD", ChargeUnit = "KG" };
			info1.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
			var collection = new AutoRateInfoCollection(Factory) { info1 };

			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, collection);

			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Base, services, new[]
			{
				new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry1, true),
			});

			AssertSame("Calculation icon should be the warning icon", RateChooserImageRepository.WarningIcon, charges.CalculationIcon);
			AssertEquals("Calculation icon visibility should be true", true, charges.CalculationIconVisibility);
		}

		public void TestCalculationIcon_WhenBOLChargeIsInNotInCalculation_ShouldBeWarning()
		{
			var testHelper = new TestHelper(Factory);
			var consol = ChooserHelper.CreateConsol();

			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var bolCharge1 = testHelper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;

			var costing = testHelper.NewCosting(carrier);
			var rateEntryFRT = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			var rateLineFRT = rateEntryFRT.AddRateLine(testHelper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLineFRT.GetCalculator<UnitCalculator>().PerUnit = 100m;
			var rateLineBOL = rateEntryFRT.AddRateLine(bolCharge1, FlatCalculator.Code, "", "USD");
			rateLineBOL.GetCalculator<FlatCalculator>().BaseRate = 30m;

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var docCharge1 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BOL", 30m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var ratingCriteria = new RatingCriteria(null, Factory);

			var info1 = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FRT"], Currency = "AUD", ChargeUnit = "KG" };
			info1.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
			var collection = new AutoRateInfoCollection(Factory) { info1 };

			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, collection);

			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.BOL, services, new[]
			{
				new ChargeViewModel(docCharge1, convertedLines.First(l => l.WiseCharge == docCharge1), chooserRateEntry1, true),
			});

			AssertSame(
				"Calculation icon should be a warning icon for BOL charges not in the calculation.",
				RateChooserImageRepository.WarningIcon,
				charges.CalculationIcon
			);

			AssertEquals(
				"Calculation icon visibility should be true for BOL charges.",
				true,
				charges.CalculationIconVisibility
			);
		}

		public void TestCalculationIcon_WhenAdditionalChargeIsInNotInCalculation_ShouldBeWarning()
		{
			var testHelper = new TestHelper(Factory);
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			var freightCharge2 = testHelper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge2.AC_GC = GlbCompany.CurrentCompany.PK;

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.OfType(ChargeType.Optional);

			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 10m);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var ratingCriteria = new RatingCriteria(null, Factory);

			var info1 = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FR2"], Currency = "AUD", ChargeUnit = "KG" };
			info1.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
			var collection = new AutoRateInfoCollection(Factory) { info1 };

			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, collection);

			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Additional, services, new[]
			{
				new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry1, true),
			});

			AssertEquals("Calculation icon is expected to be a warning icon.", RateChooserImageRepository.WarningIcon, charges.CalculationIcon);
			AssertEquals("Calculation icon visibility should be true.", true, charges.CalculationIconVisibility);
		}

		public void TestPriceGetter_BOLChargesInDifferentCurrencies_ShouldSumChargePricesInDefaultCurrency()
		{
			var testHelper = new TestHelper(Factory);

			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var bolCharge1 = testHelper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge2 = testHelper.ChargeCodes.NewConsolChargeCode("BL2", "Doc Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;

			var costing = testHelper.NewCosting(carrier);
			var rateEntryFRT = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			var rateLineFRT = rateEntryFRT.AddRateLine(testHelper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "AUD");
			rateLineFRT.GetCalculator<UnitCalculator>().PerUnit = 100m;
			var rateLineBOL = rateEntryFRT.AddRateLine(bolCharge1, FlatCalculator.Code, "", "USD");
			rateLineBOL.GetCalculator<FlatCalculator>().BaseRate = 30m;
			var rateLineBL2 = rateEntryFRT.AddRateLine(bolCharge2, FlatCalculator.Code, "", "AUD");
			rateLineBL2.GetCalculator<FlatCalculator>().BaseRate = 2m;

			Factory.Save();

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD"; //default currency
			var results = ChooserHelper.GetCalculatedResultFromEntries(new[] { rateEntryFRT }, carrier);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");
			var chooserRateEntry1 = new ChooserRateEntry(Factory, rateEntryFRT, services, results);

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.BOL, services, new[]
			{
				new ChargeViewModel(rateLineBOL, chooserRateEntry1),
				new ChargeViewModel(rateLineBL2, chooserRateEntry1),
			});

			AssertEquals("Charges total price string mismatch", "$31.50", charges.TotalPriceString);
			AssertEquals("Charges total price currency mismatch", "USD", charges.TotalPriceCurrency);
		}

		public void TestPriceGetter_AdditionalChargesInDifferentCurrencies_ShouldSumChargePricesInDefaultCurrency()
		{
			var testHelper = new TestHelper(Factory);
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");

			var bolCharge1 = testHelper.ChargeCodes.NewConsolChargeCode("BOL", "Bill of Lading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge2 = testHelper.ChargeCodes.NewConsolChargeCode("BL2", "Doc Fee 2", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge2 = testHelper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge3 = testHelper.ChargeCodes.NewConsolChargeCode("FR3", "FRT 3 - Optional ", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge3.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge4 = testHelper.ChargeCodes.NewConsolChargeCode("INC", "FRT Included 1", FreightInclusiveCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge4.AC_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save(); // carrier mapping is a DBOnly query

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.OfType(ChargeType.Optional);
			var frtCharge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 10m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.WithCurrency("USD")
				.OfType(ChargeType.Optional);
			var frtCharge3 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR3", 7m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.WithCurrency("USD")
				.OfType(ChargeType.Optional);
			var frtCharge4 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "INC", 999m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.WithCurrency("USD")
				.OfType(ChargeType.Optional);
			var frtCharge5 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BAF", 150m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.WithCurrency("HKD")
				.OfType(ChargeType.Optional);
			var frtCharge6 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BAF", 75m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.WithCurrency("NZD")
				.OfType(ChargeType.Optional);
			var docCharge1 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BOL", 30m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.OfType(ChargeType.Optional);
			var docCharge2 = RateChooserTestHelper.AddFlatCharge(apiCosting1, "BL2", 3m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.OfType(ChargeType.Optional);

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD"; //default currency
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting1);
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			AssertEquals("PRE 1xOrigin 1xFreight:", 2, converted1.Count);

			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var results = ChooserHelper.GetCalculatedResultFromEntries(converted1, carrier, logger: logger);
			var services = new DummyRateChooserServices(Factory);
			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, results);

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Additional, services, new[]
			{
				new ChargeViewModel(frtCharge1, convertedLines.First(l => l.WiseCharge == frtCharge1), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge2, convertedLines.First(l => l.WiseCharge == frtCharge2), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge3, convertedLines.First(l => l.WiseCharge == frtCharge3), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge4, convertedLines.First(l => l.WiseCharge == frtCharge4), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge5, convertedLines.First(l => l.WiseCharge == frtCharge5), chooserRateEntry1, true),
				new ChargeViewModel(frtCharge6, convertedLines.First(l => l.WiseCharge == frtCharge6), chooserRateEntry1, true),
				new ChargeViewModel(docCharge1, convertedLines.First(l => l.WiseCharge == docCharge1), chooserRateEntry1, true),
				new ChargeViewModel(docCharge2, convertedLines.First(l => l.WiseCharge == docCharge2), chooserRateEntry1, true),
			});

			AssertEquals("Initial total price should be 0.", 0m, charges.TotalPrice);
			charges.Charges.ElementAt(2).IsActive = true;
			charges.Charges.ElementAt(7).IsActive = true;
			charges.Recalculate();
			// docCharge2 = AUD 3 * AUDRate = USD 2.25
			AssertEquals(9.25m, charges.TotalPrice);
			AssertEquals("$9.25", charges.TotalPriceString);
			AssertEquals("USD", charges.TotalPriceCurrency);
			AssertEquals("", charges.TotalPriceErrorString);

			charges.Charges.ElementAt(4).IsActive = true;
			charges.Charges.ElementAt(5).IsActive = true;
			charges.Recalculate();
			AssertEquals(76.75m, charges.TotalPrice);
			AssertEquals("$76.75", charges.TotalPriceString);
			AssertEquals("USD", charges.TotalPriceCurrency);
		}

		public void TestPriceGetter_AdditionalChargesHavePercentageCalculator_ShouldSumChargePricesInDefaultCurrency()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge = ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m, mapCodeWithCW1ChargeCode: true)
				.WithCurrency("NZD");
			var vatCharge = ChooserHelper.AddPercentageCharge(apiCosting1, "VAT", 10m, "FRT", mapCodeWithCW1ChargeCode: true)
				.WithCurrency("NZD")
				.OfType(ChargeType.Optional);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var results = ChooserHelper.GetCalculatedResultFromEntries(converted1, carrier, logger: logger);
			var services = new DummyRateChooserServices(Factory);
			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, results);

			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();
			var frtChargeViewModel = new ChargeViewModel(frtCharge, convertedLines.First(l => l.WiseCharge == frtCharge), chooserRateEntry1, true);
			var vatChargeViewModel = new ChargeViewModel(vatCharge, convertedLines.First(l => l.WiseCharge == vatCharge), chooserRateEntry1, true);
			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Additional, services, new[]
			{
				frtChargeViewModel,
				vatChargeViewModel
			});

			AssertEquals("Total price should equal 50m initially.", 50m, charges.TotalPrice);
			AssertEquals("Total price string should be '$50.00' initially.", "$50.00", charges.TotalPriceString);
			AssertEquals("Total price currency should be 'AUD' initially.", "AUD", charges.TotalPriceCurrency);

			vatChargeViewModel.IsActive = true;
			charges.Recalculate();
			AssertEquals("Total price should equal 55m after VAT activation.", 55m, charges.TotalPrice);
			AssertEquals("Total price string should be '$55.00' after VAT activation.", "$55.00", charges.TotalPriceString);
			AssertEquals("Total price currency should remain 'AUD' after VAT activation.", "AUD", charges.TotalPriceCurrency);
		}

		public void TestPriceGetter_PopulateDifferentGroups_RatesService_FCL()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();

			var bolCharge1 = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Doc Fee 1 - Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge2 = Helper.ChargeCodes.NewConsolChargeCode("BL2", "Doc Fee 2 - Loading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge3 = Helper.ChargeCodes.NewConsolChargeCode("BL3", "Doc Fee 3 - Origin Brokerage", FlatCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage);
			bolCharge3.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge4 = Helper.ChargeCodes.NewConsolChargeCode("BL4", "Doc Fee 4 - Origin Brokerage Only", FlatCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerageOnly);
			bolCharge4.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge5 = Helper.ChargeCodes.NewConsolChargeCode("BL5", "Doc Fee 5 - Customs Duty", FlatCalculator.Code, ChargeCodeGroupList.Codes.CustomsDuty);
			bolCharge5.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge6 = Helper.ChargeCodes.NewConsolChargeCode("BL6", "Doc Fee 6 - Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			bolCharge6.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge7 = Helper.ChargeCodes.NewConsolChargeCode("BL7", "Doc Fee 7 - Unloading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Unloading);
			bolCharge7.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge8 = Helper.ChargeCodes.NewConsolChargeCode("BL8", "Doc Fee 8 - Brokerage", FlatCalculator.Code, ChargeCodeGroupList.Codes.Brokerage);
			bolCharge8.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge9 = Helper.ChargeCodes.NewConsolChargeCode("BL9", "Doc Fee 9 - Brokerage Only", FlatCalculator.Code, ChargeCodeGroupList.Codes.BrokerageOnly);
			bolCharge9.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge2 = Helper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2 - Freight", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge3 = Helper.ChargeCodes.NewConsolChargeCode("FR3", "FRT 3 - Insurance", UnitCalculator.Code, ChargeCodeGroupList.Codes.Insurance);
			freightCharge3.AC_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);
			var frtCharge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 200m);
			var orgCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BOL", 300m);
			var orgCharge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL2", 400m);
			var orgCharge3 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL3", 500m);
			var orgCharge4 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL4", 600m);
			var orgCharge5 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL5", 700m);
			var dstCharge6 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL6", 800m);
			var dstCharge7 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL7", 900m);
			var dstCharge8 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL8", 1000m);
			var dstCharge9 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL9", 1100m);
			var allCharges = apiCosting1.Charges.ToList();

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD"; //default currency
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var convertedEntries = converter.Convert(response, null);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var convertedLines = convertedEntries.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToList();

			var infoFRT = ChooserHelper.BuildAutoRateInfo("FRT", convertedLines);
			infoFRT.AddFlatPaymentBasis(100m, consol.RatingAdapter.OperationalJobCode);
			var infoFRT2 = ChooserHelper.BuildAutoRateInfo("FR2", convertedLines);
			infoFRT2.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
			var infoORG = ChooserHelper.BuildAutoRateInfo("BOL", convertedLines);
			infoORG.AddFlatPaymentBasis(300m, consol.RatingAdapter.OperationalJobCode);
			var infoORG2 = ChooserHelper.BuildAutoRateInfo("BL2", convertedLines);
			infoORG2.AddFlatPaymentBasis(400m, consol.RatingAdapter.OperationalJobCode);
			var infoORG3 = ChooserHelper.BuildAutoRateInfo("BL3", convertedLines);
			infoORG3.AddFlatPaymentBasis(500m, consol.RatingAdapter.OperationalJobCode);
			var infoORG4 = ChooserHelper.BuildAutoRateInfo("BL4", convertedLines);
			infoORG4.AddFlatPaymentBasis(600m, consol.RatingAdapter.OperationalJobCode);
			var infoORG5 = ChooserHelper.BuildAutoRateInfo("BL5", convertedLines);
			infoORG5.AddFlatPaymentBasis(700m, consol.RatingAdapter.OperationalJobCode);
			var infoDST = ChooserHelper.BuildAutoRateInfo("BL6", convertedLines);
			infoDST.AddFlatPaymentBasis(800m, consol.RatingAdapter.OperationalJobCode);
			var infoDST2 = ChooserHelper.BuildAutoRateInfo("BL7", convertedLines);
			infoDST2.AddFlatPaymentBasis(900m, consol.RatingAdapter.OperationalJobCode);
			var infoDST3 = ChooserHelper.BuildAutoRateInfo("BL8", convertedLines);
			infoDST3.AddFlatPaymentBasis(1000m, consol.RatingAdapter.OperationalJobCode);
			var infoDST4 = ChooserHelper.BuildAutoRateInfo("BL9", convertedLines);
			infoDST4.AddFlatPaymentBasis(1100m, consol.RatingAdapter.OperationalJobCode);

			var collection = new AutoRateInfoCollection(Factory) { infoFRT, infoFRT2, infoORG, infoORG2, infoORG3, infoORG4, infoORG5, infoDST, infoDST2, infoDST3, infoDST4 };

			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, convertedEntries, services, collection);

			var charges = new ChargesViewModel(
				ChargesViewModel.ChargesGroup.Base,
				services,
				allCharges
					.Select(c => new ChargeViewModel(c, FindRateLineWithCharge(c, convertedLines), chooserRateEntry1, true)),
				isLCL: false);

			Assert("It is FCL", charges.Charges.First().CalculatedFormulaVisibility);

			AssertContains("BL2, BL3, BL4, BL5, BOL", charges.ORGChargeCodesString);
			AssertEquals("(300 + 400 + 500 + 600 + 700) * 0.75 - AUD to USD", 1875m, charges.TotalORGChargesPrice);
			AssertEquals(true, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			AssertContains("BL6, BL7, BL8, BL9", charges.DSTChargeCodesString);
			AssertEquals("(800 + 900 + 1000 + 1100) * 0.75 - AUD to USD", 2850m, charges.TotalDSTChargesPrice);
			AssertEquals(true, charges.DSTChargesVisibility);
			AssertContains("BL6, BL7, BL8, BL9", string.Join(", ", charges.DestinationChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			AssertEquals("FR2, FRT", charges.FRTChargeCodesString);
			AssertEquals("(100 + 200) * 0.75 - AUD to USD", 225m, charges.TotalFRTChargesPrice);
			AssertEquals(true, charges.FRTChargesVisibility);
			AssertContains("FR2, FRT", string.Join(", ", charges.FRTChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			//deactivate some origin charges
			charges.Charges.ElementAt(3).IsActive = false;
			charges.Recalculate();

			AssertContains("BL3, BL4, BL5, BOL", charges.ORGChargeCodesString);
			AssertEquals("(300 + 500 + 600 + 700) * 0.75 - AUD to USD", 1575m, charges.TotalORGChargesPrice);
			AssertEquals(true, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			charges.Charges.ElementAt(2).IsActive = false;
			charges.Recalculate();

			AssertContains("BL3, BL4, BL5", charges.ORGChargeCodesString);
			AssertEquals("(500 + 600 + 700) * 0.75 - AUD to USD", 1350m, charges.TotalORGChargesPrice);
			AssertEquals(true, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			//deactivate all Origin charges
			charges.Charges.ElementAt(4).IsActive = false;
			charges.Charges.ElementAt(5).IsActive = false;
			charges.Charges.ElementAt(6).IsActive = false;
			charges.Recalculate();

			AssertEquals(0m, charges.TotalORGChargesPrice);
			AssertNullOrEmpty(charges.ORGChargeCodesString);
			AssertEquals(false, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			//deactivate some destination charges
			charges.Charges.ElementAt(8).IsActive = false;
			charges.Recalculate();

			AssertContains("BL6, BL8, BL9", charges.DSTChargeCodesString);
			AssertEquals("(800 + 1000 + 1100) * 0.75 - AUD to USD", 2175m, charges.TotalDSTChargesPrice);
			AssertEquals(true, charges.DSTChargesVisibility);
			AssertContains("BL6, BL7, BL8, BL9", string.Join(", ", charges.DestinationChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			//deactivate all destination charges
			charges.Charges.ElementAt(7).IsActive = false;
			charges.Charges.ElementAt(9).IsActive = false;
			charges.Charges.ElementAt(10).IsActive = false;
			charges.Recalculate();

			AssertEquals(0m, charges.TotalDSTChargesPrice);
			AssertNullOrEmpty(charges.DSTChargeCodesString);
			AssertEquals(false, charges.DSTChargesVisibility);
			AssertContains("BL6, BL7, BL8, BL9", string.Join(", ", charges.DestinationChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			AssertEquals("FR2, FRT", charges.FRTChargeCodesString);
			AssertEquals("(100 + 200) * 0.75 - AUD to USD", 225m, charges.TotalFRTChargesPrice);
			AssertEquals(true, charges.FRTChargesVisibility);
			AssertContains("FR2, FRT", string.Join(", ", charges.FRTChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));
		}

		public void TestPriceGetter_PopulateDifferentGroups_RatesService_LCL()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();
			consol.JK_ConsolMode = "LCL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bolCharge1 = Helper.ChargeCodes.NewConsolChargeCode("BOL", "Doc Fee 1 - Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			bolCharge1.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge2 = Helper.ChargeCodes.NewConsolChargeCode("BL2", "Doc Fee 2 - Loading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Loading);
			bolCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge3 = Helper.ChargeCodes.NewConsolChargeCode("BL3", "Doc Fee 3 - Origin Brokerage", FlatCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerage);
			bolCharge3.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge4 = Helper.ChargeCodes.NewConsolChargeCode("BL4", "Doc Fee 4 - Origin Brokerage Only", FlatCalculator.Code, ChargeCodeGroupList.Codes.OriginBrokerageOnly);
			bolCharge4.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge5 = Helper.ChargeCodes.NewConsolChargeCode("BL5", "Doc Fee 5 - Customs Duty", FlatCalculator.Code, ChargeCodeGroupList.Codes.CustomsDuty);
			bolCharge5.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge6 = Helper.ChargeCodes.NewConsolChargeCode("BL6", "Doc Fee 6 - Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			bolCharge6.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge7 = Helper.ChargeCodes.NewConsolChargeCode("BL7", "Doc Fee 7 - Unloading", FlatCalculator.Code, ChargeCodeGroupList.Codes.Unloading);
			bolCharge7.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge8 = Helper.ChargeCodes.NewConsolChargeCode("BL8", "Doc Fee 8 - Brokerage", FlatCalculator.Code, ChargeCodeGroupList.Codes.Brokerage);
			bolCharge8.AC_GC = GlbCompany.CurrentCompany.PK;
			var bolCharge9 = Helper.ChargeCodes.NewConsolChargeCode("BL9", "Doc Fee 9 - Brokerage Only", FlatCalculator.Code, ChargeCodeGroupList.Codes.BrokerageOnly);
			bolCharge9.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge2 = Helper.ChargeCodes.NewConsolChargeCode("FR2", "FRT 2 - Freight", UnitCalculator.Code, ChargeCodeGroupList.Codes.Freight);
			freightCharge2.AC_GC = GlbCompany.CurrentCompany.PK;
			var freightCharge3 = Helper.ChargeCodes.NewConsolChargeCode("FR3", "FRT 3 - Insurance", UnitCalculator.Code, ChargeCodeGroupList.Codes.Insurance);
			freightCharge3.AC_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", "LCL");

			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 100m);
			var frtCharge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 200m);
			var orgCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BOL", 300m);
			var orgCharge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL2", 400m);
			var orgCharge3 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL3", 500m);
			var orgCharge4 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL4", 600m);
			var orgCharge5 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL5", 700m);
			var dstCharge6 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL6", 800m);
			var dstCharge7 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL7", 900m);
			var dstCharge8 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL8", 1000m);
			var dstCharge9 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "BL9", 1100m);
			var allCharges = apiCosting1.Charges.ToList();

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD"; // default currency
			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToList();
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var infoFRT = ChooserHelper.BuildAutoRateInfo("FRT", convertedLines);
			infoFRT.AddFlatPaymentBasis(100m, consol.RatingAdapter.OperationalJobCode);
			var infoFRT2 = ChooserHelper.BuildAutoRateInfo("FR2", convertedLines);
			infoFRT2.AddFlatPaymentBasis(200m, consol.RatingAdapter.OperationalJobCode);
			var infoORG = ChooserHelper.BuildAutoRateInfo("BOL", convertedLines);
			infoORG.AddFlatPaymentBasis(300m, consol.RatingAdapter.OperationalJobCode);
			var infoORG2 = ChooserHelper.BuildAutoRateInfo("BL2", convertedLines);
			infoORG2.AddFlatPaymentBasis(400m, consol.RatingAdapter.OperationalJobCode);
			var infoORG3 = ChooserHelper.BuildAutoRateInfo("BL3", convertedLines);
			infoORG3.AddFlatPaymentBasis(500m, consol.RatingAdapter.OperationalJobCode);
			var infoORG4 = ChooserHelper.BuildAutoRateInfo("BL4", convertedLines);
			infoORG4.AddFlatPaymentBasis(600m, consol.RatingAdapter.OperationalJobCode);
			var infoORG5 = ChooserHelper.BuildAutoRateInfo("BL5", convertedLines);
			infoORG5.AddFlatPaymentBasis(700m, consol.RatingAdapter.OperationalJobCode);
			var infoDST = ChooserHelper.BuildAutoRateInfo("BL6", convertedLines);
			infoDST.AddFlatPaymentBasis(800m, consol.RatingAdapter.OperationalJobCode);
			var infoDST2 = ChooserHelper.BuildAutoRateInfo("BL7", convertedLines);
			infoDST2.AddFlatPaymentBasis(900m, consol.RatingAdapter.OperationalJobCode);
			var infoDST3 = ChooserHelper.BuildAutoRateInfo("BL8", convertedLines);
			infoDST3.AddFlatPaymentBasis(1000m, consol.RatingAdapter.OperationalJobCode);
			var infoDST4 = ChooserHelper.BuildAutoRateInfo("BL9", convertedLines);
			infoDST4.AddFlatPaymentBasis(1100m, consol.RatingAdapter.OperationalJobCode);

			var collection = new AutoRateInfoCollection(Factory) { infoFRT, infoFRT2, infoORG, infoORG2, infoORG3, infoORG4, infoORG5, infoDST, infoDST2, infoDST3, infoDST4 };

			var chooserRateEntry1 = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting1, converted1, services, collection);

			var charges = new ChargesViewModel(
				ChargesViewModel.ChargesGroup.Base,
				services,
				allCharges.Select(c => new ChargeViewModel(c, FindRateLineWithCharge(c, convertedLines), chooserRateEntry1, true)),
				isLCL: true);

			AssertContains("BL2, BL3, BL4, BL5, BOL", charges.ORGChargeCodesString);
			AssertEquals("Expected ORG charges price to match", 1875m, charges.TotalORGChargesPrice);
			AssertEquals("Expected ORG charges visibility to be true", true, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			AssertContains("BL6, BL7, BL8, BL9", charges.DSTChargeCodesString);
			AssertEquals("Expected DST charges price to match", 2850m, charges.TotalDSTChargesPrice);
			AssertEquals("Expected DST charges visibility to be true", true, charges.DSTChargesVisibility);
			AssertContains("BL6, BL7, BL8, BL9", string.Join(", ", charges.DestinationChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			AssertEquals("Expected FRT charges string to match", "FR2, FRT", charges.FRTChargeCodesString);
			AssertEquals("Expected total FRT charges price to match", 225m, charges.TotalFRTChargesPrice);
			AssertEquals("Expected FRT charges visibility to be true", true, charges.FRTChargesVisibility);
			AssertContains("FR2, FRT", string.Join(", ", charges.FRTChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			// deactivate some origin charges
			charges.Charges.ElementAt(3).IsActive = false;
			charges.Recalculate();

			AssertContains("BL3, BL4, BL5, BOL", charges.ORGChargeCodesString);
			AssertEquals("Expected ORG charges price to update", 1575m, charges.TotalORGChargesPrice);
			AssertEquals("Expected ORG charges visibility to remain true", true, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			charges.Charges.ElementAt(2).IsActive = false;
			charges.Recalculate();

			AssertContains("BL3, BL4, BL5", charges.ORGChargeCodesString);
			AssertEquals("Expected ORG charges price to update further", 1350m, charges.TotalORGChargesPrice);
			AssertEquals("Expected ORG charges visibility to remain true", true, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			// deactivate all Origin charges
			charges.Charges.ElementAt(4).IsActive = false;
			charges.Charges.ElementAt(5).IsActive = false;
			charges.Charges.ElementAt(6).IsActive = false;
			charges.Recalculate();

			AssertEquals("Expected ORG charges price to be zero", 0m, charges.TotalORGChargesPrice);
			AssertNullOrEmpty(charges.ORGChargeCodesString);
			AssertEquals("Expected ORG charges visibility to be false", false, charges.ORGChargesVisibility);
			AssertContains("BL2, BL3, BL4, BL5, BOL", string.Join(", ", charges.OriginChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			// deactivate some destination charges
			charges.Charges.ElementAt(8).IsActive = false;
			charges.Recalculate();

			AssertContains("BL6, BL8, BL9", charges.DSTChargeCodesString);
			AssertEquals("Expected DST charges price to update", 2175m, charges.TotalDSTChargesPrice);
			AssertEquals("Expected DST charges visibility to remain true", true, charges.DSTChargesVisibility);
			AssertContains("BL6, BL7, BL8, BL9", string.Join(", ", charges.DestinationChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			// deactivate all destination charges
			charges.Charges.ElementAt(7).IsActive = false;
			charges.Charges.ElementAt(9).IsActive = false;
			charges.Charges.ElementAt(10).IsActive = false;
			charges.Recalculate();

			AssertEquals("Expected DST charges price to be zero", 0m, charges.TotalDSTChargesPrice);
			AssertNullOrEmpty(charges.DSTChargeCodesString);
			AssertEquals("Expected DST charges visibility to be false", false, charges.DSTChargesVisibility);
			AssertContains("BL6, BL7, BL8, BL9", string.Join(", ", charges.DestinationChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));

			AssertEquals("Expected FRT charges string to remain unchanged", "FR2, FRT", charges.FRTChargeCodesString);
			AssertEquals("Expected total FRT charges price to remain unchanged", 225m, charges.TotalFRTChargesPrice);
			AssertEquals("Expected FRT charges visibility to remain true", true, charges.FRTChargesVisibility);
			AssertContains("FR2, FRT", string.Join(", ", charges.FRTChargesForCalculation.Select(c => c.Charge.ChargeCode).OrderBy(c => c)));
		}

		public void TestChargesViewModelChargeGroupsViewShouldContainUnmappedWiseRatesCharges()
		{
			var testHelper = new TestHelper(Factory);
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var consol = ChooserHelper.CreateConsol();

			var apiCosting = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge = RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FRT", 100m)
				.WithCustomCategory(WRConstants.ChargeCustomCategory.Ocean)
				.OfType(ChargeType.Optional);

			var fr2Charge = RateChooserTestHelper.AddPerContainerCharge(apiCosting, "FR2", 10m);

			var logger = new ElementaryLogger();
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting });
			var converter = new WiseRatesConverter(Factory, logger);
			var converted1 = converter.Convert(response, null);
			var services = new DummyRateChooserServices(Factory, "HKD", "NZD");

			var ratingCriteria = new RatingCriteria(null, Factory);

			var info = new AutoRateInfo(Factory) { ChargeCode = Helper.ChargeCodes["FR2"], Currency = "AUD", ChargeUnit = "KG" };
			info.AddFlatPaymentBasis(90m, consol.RatingAdapter.OperationalJobCode);
			var collection = new AutoRateInfoCollection(Factory) { info };

			var chooserRateEntry = new ChooserRateEntry(Factory, serviceProvider: carrier, apiCosting, converted1, services, collection);
			var convertedLines = converted1.SelectMany(c => c.ChildRateLines).Cast<WiseLine>().ToArray();

			var frtChargeViewModel = new ChargeViewModel(frtCharge, convertedLines.First(l => l.WiseCharge == frtCharge), chooserRateEntry, true);
			var fr2ChargeViewModel = new ChargeViewModel(fr2Charge, convertedLines.First(l => l.WiseCharge == fr2Charge), chooserRateEntry, true);

			var charges = new ChargesViewModel(ChargesViewModel.ChargesGroup.Additional, services, new[]
			{
				frtChargeViewModel,
				fr2ChargeViewModel
			});

			AssertEquals("Charge group should match expected value", "FRT", frtChargeViewModel.ChargeGroup);
			AssertEquals("Charge group for missing mapping should match expected value", "Missing Mapping", fr2ChargeViewModel.ChargeGroup);

			var actualChargeViewModels = charges.ChargeGroupsView.SelectMany(g => g.Charges).ToList();
			var expectedChargeViewModels = new[] { frtChargeViewModel, fr2ChargeViewModel };

			AssertContainsExactElementsInAnyOrder("Charge groups view should contain expected charges", expectedChargeViewModels, actualChargeViewModels);
		}

		WiseLine FindRateLineWithCharge(Charge wantedCharge, IList<WiseLine> rateLines)
		{
			return rateLines.Single(rl => rl.ChargeCode.AC_Code == wantedCharge.ChargeCode);
		}

		protected RateChooserTestHelper ChooserHelper
		{
			get { return chooserHelper ?? (chooserHelper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper chooserHelper;
	}

	public class RateChooserOneChargeViewModelTest : RatingTestCase
	{
		public void TestChargeCodeErrorToolTipText()
		{
			var carrier = ChooserHelper.CreateCarrierOrg("SCAC");
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "NZD");
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 100m;
			var chargeCodeFR3 = Helper.ChargeCodes["FR3"];

			Factory.Save(); // carrier mapping is a DBOnly query

			var consol = ChooserHelper.CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			ChooserHelper.AddContainer(consol, "20GP", "GEN", Core.Constants.ContainerModes.FCL, 3);
			ChooserHelper.AddContainer(consol, "20GP", "ATPT", Core.Constants.ContainerModes.FCL, 5);

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			var frtCharge1 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FR2", 100m)
				.WithCurrency("HKD");
			var frtCharge2 = RateChooserTestHelper.AddPerContainerCharge(apiCosting1, chargeCodeFR3.AC_Code, 100m)
				.WithCurrency("HKD");

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD"; //default currency
			var logger = new TestLogger();
			var context = RatingContext.CreateForManualSelect(logger, new Mock<IDialogService>().Object);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(new[] { apiCosting1 });
			model.AddWiseRatesForTest(response);
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(ChooserHelper.NewCW1RateCombinations(criteria, "20GP", "ATPT", rateEntry1));
			var modelTab1 = model.ContainerGroups.First();
			modelTab1.SelectedRate = modelTab1.Rates.Single(x => x.WiseRateEntry == apiCosting1);
			var modelTab2 = model.ContainerGroups.Skip(1).First();
			modelTab2.SelectedRate = modelTab2.Rates.Single(x => x.RateEntry == rateEntry1);
			model.Validate();

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var tab1 = viewModel.ContainerTabs.First();
			var tab2 = viewModel.ContainerTabs.Skip(1).First();

			var tab1Charges = tab1.SelectedRow.OceanCharges.Charges.First();
			var tab2Charges = tab2.SelectedRow.CW1FreightCharges;

			var tab1Charge1 = tab1Charges.Charges.First(x => x.Code == "FR2");
			var tab1Charge2 = tab1Charges.Charges.First(x => x.Code == "FR3");
			var tab2Charge = tab2Charges.Charges.First();

			string tab1ChargeMissingExchangeRateError = "Missing exchange rate(s) for HKD to USD";
			string tab2ChargeMissingExchangeRateError = "Missing exchange rate(s) for NZD to USD";

			CombineAssertions(() =>
			{
				AssertContains("tab1Charge1.ChargeCodeErrorToolTipText", @"This universal charge code is not mapped.
The Apply button will prompt to map any selected unmapped codes.
No Charge Code is assigned with or has the same Code as Universal 'FR2'.", tab1Charge1.ChargeCodeErrorToolTipText);
				AssertNotContains("tab1Charge1.ChargeCodeErrorToolTipText: Individual charge should not display missing exchange error", tab1ChargeMissingExchangeRateError, tab1Charge1.ChargeCodeErrorToolTipText);
				AssertEquals("tab1Charge1.ChargeCodeErrorVisibility: should be visible as it contains mapping error", true, tab1Charge1.ChargeCodeErrorVisibility);

				AssertNotContains("tab1Charge2.ChargeCodeErrorToolTipText: Individual charge should not display missing exchange error", tab1ChargeMissingExchangeRateError, tab1Charge2.ChargeCodeErrorToolTipText);
				AssertEquals("tab1Charge2.ChargeCodeErrorVisibility: should be Collapsed as it has no error", false, tab1Charge2.ChargeCodeErrorVisibility);
				AssertEquals("tab1Charges.TotalPriceErrorString: Total of all charges should display missing exchange error", tab1ChargeMissingExchangeRateError, tab1Charges.TotalPriceErrorString);
				Assert("tab1Charges.TotalPriceErrorVisibility", tab1Charges.TotalPriceErrorVisibility);

				AssertNotContains("tab2Charge.ChargeCodeErrorToolTipText: Individual charge should not display missing exchange error", tab2ChargeMissingExchangeRateError, tab2Charge.ChargeCodeErrorToolTipText);
				AssertEquals("tab2Charge.ChargeCodeErrorVisibility: should be Collapsed as it has no error", false, tab2Charge.ChargeCodeErrorVisibility);
				AssertEquals("tab2Charges.TotalPriceErrorString: Total of all charges should display missing exchange error", tab2ChargeMissingExchangeRateError, tab2Charges.TotalPriceErrorString);
				Assert("tab2Charges.TotalPriceErrorVisibility", tab2Charges.TotalPriceErrorVisibility);
			});
		}

		protected RateChooserTestHelper ChooserHelper
		{
			get { return chooserHelper ?? (chooserHelper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper chooserHelper;
	}
}

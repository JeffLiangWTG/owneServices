using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.RatesLoad;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Constants;
using Api = WiseRates.Api;
using Constants = Enterprise.Core.Constants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business.Test
{
	public class RateChooserViewModelTest : RatingTestCase
	{
		public void TestShowMoreRates()
		{
			var creditor = CreateCarrierOrg("ABCD");
			var carrier = CreateCarrierOrg("SCAC");

			var refContainer20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));

			var consol = CreateConsol();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", "FCL", 3);

			Factory.Save();

			var apiCosting = RateChooserTestHelper.CreateApiRate(refContainer20, carrier, "");
			apiCosting.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "CN", PerUnitRate = 10m });
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(apiCosting);
			var logger = new TestLogger();
			var testWiseRatesProvider1 = ChooserHelper.CreateMockWiseRateProviderWithResponse(response, (r) => new[] { 0, 1 }.Contains(r.PageID));
			var context = new RatingContext(new LoggerDecorator(logger), Factory, null, testWiseRatesProvider1, null, true);
			var autoRatingInfo = new AutoRatingProxy(consol.RatingAdapter);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);
			viewModel.SendRatesRequest(filter, BuildRatesQuery(criteria));
			AssertEquals(1, model.ContainerGroups.Count());

			viewModel.ShowMoreRates();
			AssertEquals(1, model.ContainerGroups.Count());
		}

		public void TestConstructor()
		{
			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			var viewModel = new RateChooserViewModel(model);
			AssertEquals(model, viewModel.Model);
		}

		public void TestProperties()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			AssertEquals(false, viewModel.IsApplyEnabled);
		}

		public void TestIsApplyEnabled()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "ATPT", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "ATPT", rateEntry1));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			viewModel.Validate();
			AssertEquals(true, viewModel.IsValid);

			var tab1 = viewModel.ContainerTabs.First();
			var tab2 = viewModel.ContainerTabs.Skip(1).First();
			AssertEquals(false, viewModel.IsApplyEnabled);
			tab1.Rates[0].IsSelected = true;
			AssertEquals("IsApplyEnabled when only tab1 has a selection", true, viewModel.IsApplyEnabled);

			tab1.Rates[0].IsSelected = false;
			AssertEquals("IsApplyEnabled when no tab has a selection", false, viewModel.IsApplyEnabled);

			tab2.Rates[0].IsSelected = true;
			AssertEquals("IsApplyEnabled when only tab2 has a selection", true, viewModel.IsApplyEnabled);

			tab1.Rates[0].IsSelected = true;
			AssertEquals("IsApplyEnabled when both tabs have a selection", true, viewModel.IsApplyEnabled);
		}

		public void TestUnselectRateFromSummary()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			containerViewModel1.Rates[0].IsSelected = true;
			containerViewModel2.Rates[0].IsSelected = true;
			var selectedRows = viewModel.SelectedRows.ToList();
			AssertEquals(2, selectedRows.Count);
			AssertEquals(rateEntry1, selectedRows[0].Rate.RateEntry);
			AssertEquals(rateEntry2, selectedRows[1].Rate.RateEntry);

			viewModel.UnselectRateFromSummary(selectedRows[1]);

			AssertEquals(rateEntry1, viewModel.SelectedRows.First().Rate.RateEntry);
			AssertNull(viewModel.SelectedRows.ToList()[1].Rate);
		}

		public void TestClearAll()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.NamedAccounts = new string[] { "SSA" };
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.NamedAccounts = new string[] { "SSB" };

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			containerViewModel1.Rates[0].IsSelected = true;
			containerViewModel2.Rates[0].IsSelected = true;
			var selectedRows = viewModel.SelectedRows.ToList();
			AssertEquals(2, selectedRows.Count);
			AssertEquals(rateEntry1, selectedRows[0].Rate.RateEntry);
			AssertEquals(rateEntry2, selectedRows[1].Rate.RateEntry);
			AssertContains("The rates have different Named Accounts.", viewModel.Notifications.First().Message);

			viewModel.ClearAll();

			CombineAssertions("Should unselect all selected rates", () =>
			 {
				 selectedRows = viewModel.SelectedRows.ToList();
				 AssertNull(selectedRows[0].Rate);
				 AssertNull(selectedRows[1].Rate);
				 AssertEquals("Should clear all notifications", false, viewModel.Notifications.Any());

				 AssertNull(containerViewModel1.SelectedRow);
				 AssertNull(containerViewModel2.SelectedRow);
			 });
		}

		public void TestRateVisibilityForFCL()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();

			CombineAssertions(() =>
			{
				Assert("Rates list should be visible", containerViewModel1.RateVisibility);
				Assert("No rates warning should be invisible", !containerViewModel1.NoRateVisibility);
				AssertNullOrEmpty("No rates message should be empty", containerViewModel1.NoRatesFound);

				Assert("Rates list should be invisible", !containerViewModel2.RateVisibility);
				Assert("No rates warning should be visible", containerViewModel2.NoRateVisibility);
				Assert("No rates warning should be visible", containerViewModel2.NoRateVisibility);
				AssertEquals("No rates warning should be for container", containerViewModel2.NoRateFoundForContainer, containerViewModel2.NoRatesFound);
			});
		}

		public void TestRateVisibilityForLCL()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", Constants.ContainerModes.LCL);
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var wiseRates = new[] { apiCosting1 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var model = new RateChooserModel(criteria, context);
			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();

			CombineAssertions(() =>
			{
				Assert("Rates list should be visible", containerViewModel1.RateVisibility);
				Assert("No rates warning should be invisible", !containerViewModel1.NoRateVisibility);
				AssertNullOrEmpty("No rates message should be empty", containerViewModel1.NoRatesFound);
			});

			model = new RateChooserModel(criteria, context);
			viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			containerViewModel1 = viewModel.ContainerTabs.First();

			CombineAssertions(() =>
			{
				Assert("Rates list should be invisible", !containerViewModel1.RateVisibility);
				Assert("No rates warning should be visible", containerViewModel1.NoRateVisibility);
				Assert("No rates warning should be visible", containerViewModel1.NoRateVisibility);
				AssertEquals("No rates warning should be for LCL", containerViewModel1.NoRateFoundForLCL, containerViewModel1.NoRatesFound);
			});
		}

		public void TestCheckRateIsLCL()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", Constants.ContainerModes.LCL);
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var wiseRates = new[] { apiCosting1 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var model = new RateChooserModel(criteria, context);
			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();

			CombineAssertions(() =>
			{
				var chooserRateEntry = containerViewModel1.Rates[0].Rate;
				var wiseRate = chooserRateEntry.RateEntry as WiseEntry;

				AssertEquals("Category is LCL", "LCL", wiseRate.TI_RateCategory);
				AssertEquals("Rate is LCL", false, chooserRateEntry.IsFCL());

				//in case we cannot set it while we convert WiseRates
				wiseRate.TI_RateCategory = "";
				AssertEquals("Mode is LCL", "LCL", wiseRate.TI_Mode);
				AssertEquals("Rate is LCL based on TI_Mode", false, chooserRateEntry.IsFCL());
			});
		}

		public void Test_ShouldLoadOrgDstFrtCharges_WhenConsolIsLCL()
		{
			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;

			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var rateEntryFRT = costing.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "USLAX", "HKHKG");
			rateEntryFRT.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntryFRT.RateLines.RemoveAndDeleteAll();
			var rateLineFrt = rateEntryFRT.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, "KG", "AUD");
			rateLineFrt.GetCalculator<UnitCalculator>().PerUnit = 100;

			var rateEntryOrigin = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.ALL, "USLAX", "");
			var rateLineOrigin = rateEntryOrigin.AddRateLine(Helper.ChargeCodes["ODOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineOrigin.GetCalculator<UnitCalculator>().PerUnit = 50;

			var rateEntryDST = costing.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.ALL, "", "HKHKG");
			var rateLineDST = rateEntryDST.AddRateLine(Helper.ChargeCodes["DDOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineDST.GetCalculator<UnitCalculator>().PerUnit = 40;

			Factory.Save();

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "", "", rateEntryFRT, rateEntryOrigin, rateEntryDST));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();

			CombineAssertions(() =>
			{
				var chooserRateEntry = containerViewModel1.Rates.Single().Rate;

				AssertEquals("Calculated result", 3, chooserRateEntry.CalculatedResult.Count);
				var odocResult = chooserRateEntry.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["ODOC"]);
				AssertEquals("Calculated amount ODOC", 2500000m, odocResult.Amount); // 50000 * 50
				AssertEquals("Calculated description ODOC", "ODOC: 50000 Kilogram(s) @ AUD 50.00/KG", odocResult.SingleLineDescription);

				var ddocResult = chooserRateEntry.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["DDOC"]);
				AssertEquals("Calculated amount DDOC", 2000000m, ddocResult.Amount); // 40000 * 50
				AssertEquals("Calculated description DDOC", "DDOC: 50000 Kilogram(s) @ AUD 40.00/KG", ddocResult.SingleLineDescription);

				var frtResult = chooserRateEntry.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["FRT"]);
				AssertEquals("Calculated amount FRT", 5000000m, frtResult.Amount); // 50000 * 100
				AssertEquals("Calculated description FRT", "FRT: 50000 Kilogram(s) @ AUD 100.00/KG", frtResult.SingleLineDescription);
			});
		}

		public void Test_ShouldLoadOrgDstCharges_WhenConsolIsLCLAndNoFrtCharges()
		{
			Helper.ChargeCodes["ODOC"].AC_IsGroupageCharge = true;
			Helper.ChargeCodes["DDOC"].AC_IsGroupageCharge = true;

			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var rateEntryOrigin = costing.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.ALL, "USLAX", "");
			var rateLineOrigin = rateEntryOrigin.AddRateLine(Helper.ChargeCodes["ODOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineOrigin.GetCalculator<UnitCalculator>().PerUnit = 50;

			var rateEntryDST = costing.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.ALL, "", "HKHKG");
			var rateLineDST = rateEntryDST.AddRateLine(Helper.ChargeCodes["DDOC"], UnitCalculator.Code, "KG", "AUD");
			rateLineDST.GetCalculator<UnitCalculator>().PerUnit = 40;

			Factory.Save();

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "", "", rateEntryOrigin, rateEntryDST));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();

			CombineAssertions(() =>
			{
				var chooserRateEntry = containerViewModel1.Rates.Single().Rate;

				AssertEquals("Calculated result", 2, chooserRateEntry.CalculatedResult.Count);
				var odocResult = chooserRateEntry.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["ODOC"]);
				AssertEquals("Calculated amount ODOC", 2500000m, odocResult.Amount); // 50000 * 50
				AssertEquals("Calculated description ODOC", "ODOC: 50000 Kilogram(s) @ AUD 50.00/KG", odocResult.SingleLineDescription);

				var ddocResult = chooserRateEntry.CalculatedResult.Single(x => x.ChargeCode == Helper.ChargeCodes["DDOC"]);
				AssertEquals("Calculated amount DDOC", 2000000m, ddocResult.Amount); // 40000 * 50
				AssertEquals("Calculated description DDOC", "DDOC: 50000 Kilogram(s) @ AUD 40.00/KG", ddocResult.SingleLineDescription);
			});
		}

		public void TestCheckRateIsFCL()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = "AFL";
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting1.ReservedForJobIDs = new[] { consol.JK_UniqueConsignRef.ToString() };
			apiCosting1.ServiceGroupId = "1";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "AFL");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting2, "FRT", 2000);
			apiCosting1.ServiceGroupId = "2";

			var wiseRates = new[] { apiCosting1, apiCosting2 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();

			var cw1Rate = containerViewModel2.Rates.First(r => r.Rate.WiseRateEntry == null);
			var cargoSphere1 = containerViewModel1.Rates.First(r => r.Rate.WiseRateEntry != null).Rate;

			CombineAssertions(() =>
			{
				var chooserRateEntry = containerViewModel1.Rates[0].Rate;
				var wiseRate = cargoSphere1.RateEntry as WiseEntry;

				AssertEquals("Category is FCL", "FCL", wiseRate.TI_RateCategory);
				AssertEquals("Rate is FCL", true, chooserRateEntry.IsFCL());

				//in case we cannot set it while we convert WiseRates
				wiseRate.TI_RateCategory = "";
				AssertEquals("Mode is SEA", "SEA", wiseRate.TI_Mode);
				AssertEquals("Rate is FCL based on TI_Mode", true, chooserRateEntry.IsFCL());

				AssertEquals("For CW1 rate RateEntry.IsFCL should be equal to Rate.IsFCL", cw1Rate.Rate.RateEntry.IsFCL(), chooserRateEntry.IsFCL());
			});
		}

		#region Select Related Rates

		public void TestSelectAllRelatedRatesButtonVisibility_ForFCL()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = "AFL";
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting1.ReservedForJobIDs = new[] { consol.JK_UniqueConsignRef.ToString() };
			apiCosting1.ServiceGroupId = "1";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "AFL");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting2, "FRT", 2000);
			apiCosting1.ServiceGroupId = "2";

			var wiseRates = new[] { apiCosting1, apiCosting2 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();

			containerViewModel1.SelectedRow.IsSelected = false;
			containerViewModel2.SelectedRow.IsSelected = false;

			var cw1Rate = containerViewModel2.Rates.First(r => r.Rate.WiseRateEntry == null);
			var cargoSphere1 = containerViewModel1.Rates.First(r => r.Rate.WiseRateEntry is { ServiceGroupId: "2" });
			var cargoSphere1RelatedRate = containerViewModel2.Rates.First(r => r.Rate.WiseRateEntry is { ServiceGroupId: "2" });

			CombineAssertions(() =>
			{
				AssertEquals("First container tab should have two rates", 2, containerViewModel1.Rates.Count);
				AssertEquals("Rate is FCL", true, containerViewModel1.Rates[0].Rate.IsFCL());
				AssertEquals("Second container tab should have three rates", 3, containerViewModel2.Rates.Count);
				AssertEquals("Rate is FCL", true, containerViewModel2.Rates[0].Rate.IsFCL());

				Assert("There is no related rate for this rate", !cw1Rate.SelectRelatedRatesApplyButtonVisibility);
				AssertEquals("Both rate should have the same service group id", cargoSphere1.Rate.WiseRateEntry.ServiceGroupId, cargoSphere1RelatedRate.Rate.WiseRateEntry.ServiceGroupId);
				Assert("Apply button should be visible because there is related rate in other tab", cargoSphere1.SelectRelatedRatesApplyButtonVisibility);
				Assert("Apply button should be visible because there is related rate in other tab", cargoSphere1RelatedRate.SelectRelatedRatesApplyButtonVisibility);
			});
		}

		public void TestSelectAllRelatedRatesButtonVisibilityForLCL_ShouldBeDisabled()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 50000m;
			shipment1.JS_UnitOfWeight = "KG";

			var apiCosting1 = ChooserHelper.CreateApiRate("", carrier, "", Constants.ContainerModes.LCL);
			apiCosting1.Charges.Add(new Api.Model.Charge { ChargeCode = "FRT", Currency = "AUD", Unit = "KG", PerUnitRate = 10m });

			var wiseRates = new[] { apiCosting1 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var model = new RateChooserModel(criteria, context);
			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var cargoSphere1 = containerViewModel1.Rates[0];

			CombineAssertions("Related rates should be collapsed for LCL", () =>
			{
				AssertEquals("Rate is LCL", false, containerViewModel1.Rates[0].Rate.IsFCL());
				AssertEquals("First container tab should have two rates", 1, containerViewModel1.Rates.Count);
				Assert(!cargoSphere1.SelectRelatedRatesApplyButtonVisibility);
			});
		}

		public void TestRelatedRatesTextMessage()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = "AFL";
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting1.ReservedForJobIDs = new[] { consol.JK_UniqueConsignRef.ToString() };
			apiCosting1.ServiceGroupId = "1";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "AFL");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting2, "FRT", 2000);
			apiCosting1.ServiceGroupId = "2";

			var wiseRates = new[] { apiCosting1, apiCosting2 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();

			containerViewModel1.SelectedRow.IsSelected = false;
			containerViewModel2.SelectedRow.IsSelected = false;

			var cw1Rate = containerViewModel2.Rates.First(r => r.Rate.WiseRateEntry == null);
			var cargoSphere1 = containerViewModel1.Rates.First(r => r.Rate.WiseRateEntry is { ServiceGroupId: "2" });
			var cargoSphere1RelatedRate = containerViewModel2.Rates.First(r => r.Rate.WiseRateEntry is { ServiceGroupId: "2" });

			CombineAssertions(() =>
			{
				AssertEquals("First container tab should have two rates", 2, containerViewModel1.Rates.Count);
				AssertEquals("Second container tab should have three rates", 3, containerViewModel2.Rates.Count);
				AssertEquals("There is no related rate for this rate", null, cw1Rate.SelectRelatedRatesText);
				AssertEquals("Both rate should have the same service group id", cargoSphere1.Rate.WiseRateEntry.ServiceGroupId, cargoSphere1RelatedRate.Rate.WiseRateEntry.ServiceGroupId);
				AssertEquals("There is no related rate for this rate", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL).", cargoSphere1.SelectRelatedRatesText);
				AssertEquals("There is no related rate for this rate", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN).", cargoSphere1RelatedRate.SelectRelatedRatesText);
			});
		}

		public void TestRelatedRatesForCW1RatesWithDefaultValues()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL) & 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be empty", string.Empty, cw1Rate.Rate.ContractNumber);
				AssertEquals("Carrier Service Level should be empty", string.Empty, cw1Rate.Rate.CarrierServiceLevel);
				AssertEquals("Named Accounts should be empty", 0, cw1Rate.Rate.RateEntry.NamedAccounts.Count());

				AssertEquals("Should be CW1 rate", null, cw1RelatedRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN) & 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be empty", string.Empty, cw1RelatedRate.Rate.ContractNumber);
				AssertEquals("Carrier Service Level should be empty", string.Empty, cw1RelatedRate.Rate.CarrierServiceLevel);
				AssertEquals("Named Accounts should be empty", 0, cw1RelatedRate.Rate.RateEntry.NamedAccounts.Count());

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 40GP () (GEN) & 20GP () (GEN) & 20GP () (AFL).", cw40GPRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be empty", string.Empty, cw40GPRate.Rate.ContractNumber);
				AssertEquals("Carrier Service Level should be empty", string.Empty, cw40GPRate.Rate.CarrierServiceLevel);
				AssertEquals("Named Accounts should be empty", 0, cw40GPRate.Rate.RateEntry.NamedAccounts.Count());
			});
		}

		public void TestRelatedRatesForCW1RatesWithNoneDefaultValues()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry1.TI_ContractNumber = "Contract 1";
			rateEntry1.TI_PL_NKCarrierServiceLevel = "CAR";
			rateEntry1.NamedAccounts = new List<string> { "Name1", "Name2" };

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.TI_ContractNumber = "Contract 1";
			rateEntry2.TI_PL_NKCarrierServiceLevel = "CAR";
			rateEntry2.NamedAccounts = new List<string> { "Name2", "Name1" }; // order is not important

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL) & 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw1Rate.Rate.ContractNumber);
				AssertEquals("Carrier Service Level should be CAR", "CAR", cw1Rate.Rate.CarrierServiceLevel);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw1Rate.Rate.RateEntry.NamedAccounts);

				AssertEquals("Should be CW1 rate", null, cw1RelatedRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN) & 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw1RelatedRate.Rate.ContractNumber);
				AssertEquals("Carrier Service Level should be CAR", "CAR", cw1RelatedRate.Rate.CarrierServiceLevel);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw1RelatedRate.Rate.RateEntry.NamedAccounts);

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 40GP () (GEN) & 20GP () (GEN) & 20GP () (AFL).", cw40GPRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw40GPRate.Rate.ContractNumber);
				AssertEquals("Carrier Service Level should be CAR", "CAR", cw40GPRate.Rate.CarrierServiceLevel);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw40GPRate.Rate.RateEntry.NamedAccounts);
			});
		}

		public void TestRelatedRatesForCW1RatesWhenContractNumbersAreTheSame()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry1.TI_ContractNumber = "Contract 1";

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.TI_ContractNumber = "Contract 1";

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL) & 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw1Rate.Rate.ContractNumber);

				AssertEquals("Should be CW1 rate", null, cw1RelatedRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN) & 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw1RelatedRate.Rate.ContractNumber);

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 40GP () (GEN) & 20GP () (GEN) & 20GP () (AFL).", cw40GPRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw40GPRate.Rate.ContractNumber);
			});
		}

		public void TestRelatedRatesForCW1RatesWhenContractNumbersAreDifferent()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry1.TI_ContractNumber = "Contract 1";

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.TI_ContractNumber = "Contract 2";

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("20GP CW1 rates are related and text should contain only 20GP", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL).", cw1Rate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw1Rate.Rate.ContractNumber);

				AssertEquals("Should be CW1 rate", null, cw1RelatedRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain only 20GP", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 1", "Contract 1", cw1RelatedRate.Rate.ContractNumber);

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("This rate is different because of Contract Number and text should be null", null, cw40GPRate.SelectRelatedRatesText);
				AssertEquals("Contract Number should be Contract 2", "Contract 2", cw40GPRate.Rate.ContractNumber);
			});
		}

		public void TestRelatedRatesForCW1RatesWhenCarrierServiceLevelsAreTheSame()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry1.TI_PL_NKCarrierServiceLevel = "CAR";

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.TI_PL_NKCarrierServiceLevel = "CAR";

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL) & 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				AssertEquals("Carrier Service Level should be CAR", "CAR", cw1Rate.Rate.CarrierServiceLevel);

				AssertEquals("Should be CW1 rate", null, cw1RelatedRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN) & 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				AssertEquals("Carrier Service Level should be CAR", "CAR", cw1RelatedRate.Rate.CarrierServiceLevel);

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 40GP () (GEN) & 20GP () (GEN) & 20GP () (AFL).", cw40GPRate.SelectRelatedRatesText);
				AssertEquals("Carrier Service Level should be CAR", "CAR", cw40GPRate.Rate.CarrierServiceLevel);
			});
		}

		public void TestRelatedRatesForCW1RatesWhenCarrierServiceLevelsAreDifferent()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry1.TI_PL_NKCarrierServiceLevel = "CAR";

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.TI_PL_NKCarrierServiceLevel = "SAR";

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1Related = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("20GP CW1 rates are related and text should contain only 20GP", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL).", cw1Rate.SelectRelatedRatesText);
				AssertEquals("Carrier Service Level should be should be CAR", "CAR", cw1Rate.Rate.CarrierServiceLevel);

				AssertEquals("Should be CW1 rate", null, cw1Related.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain only 20GP", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN).", cw1Related.SelectRelatedRatesText);
				AssertEquals("Carrier Service Level should be should be CAR", "CAR", cw1Related.Rate.CarrierServiceLevel);

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("This rate is different because of Carrier Service Level and text should be null", null, cw40GPRate.SelectRelatedRatesText);
				AssertEquals("Carrier Service Level should be SAR", "SAR", cw40GPRate.Rate.CarrierServiceLevel);
			});
		}

		public void TestRelatedRatesForCW1RatesWhenNamedAccountsAreTheSame()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry1.NamedAccounts = new List<string> { "Name1", "Name2" };

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.NamedAccounts = new List<string> { "Name2", "Name1" }; // order is not important

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL) & 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw1Rate.Rate.RateEntry.NamedAccounts);

				AssertEquals("Should be CW1 rate", null, cw1RelatedRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN) & 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw1RelatedRate.Rate.RateEntry.NamedAccounts);

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("CW1 rates are related and text should contain all of them", "Apply this rate to these containers: 40GP () (GEN) & 20GP () (GEN) & 20GP () (AFL).", cw40GPRate.SelectRelatedRatesText);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw40GPRate.Rate.RateEntry.NamedAccounts);
			});
		}

		public void TestRelatedRatesForCW1RatesWhenNamedAccountsAreDifferent()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry1.NamedAccounts = new List<string> { "Name1", "Name2" };

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			rateEntry2.NamedAccounts = new List<string> { "Name1", "Name2", "Name3" };

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			CombineAssertions(() =>
			{
				AssertEquals("Should be CW1 rate", null, cw1Rate.Rate.WiseRateEntry);
				AssertEquals("20GP CW1 rates are related and text should contain only 20GP", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (AFL).", cw1Rate.SelectRelatedRatesText);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw1Rate.Rate.RateEntry.NamedAccounts);

				AssertEquals("Should be CW1 rate", null, cw1RelatedRate.Rate.WiseRateEntry);
				AssertEquals("20GP CW1 rates are related and text should contain only 20GP", "Apply this rate to these containers: 20GP () (AFL) & 20GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2", new List<string> { "Name1", "Name2" }, cw1RelatedRate.Rate.RateEntry.NamedAccounts);

				AssertEquals("Should be CW1 rate", null, cw40GPRate.Rate.WiseRateEntry);
				AssertEquals("This rate is different because of Named Accounts and text should be null", null, cw40GPRate.SelectRelatedRatesText);
				AssertContainsExactElementsInAnyOrder("Named Accounts should contain Name1 and Name2 and Name3", new List<string> { "Name1", "Name2", "Name3" }, cw40GPRate.Rate.RateEntry.NamedAccounts);
			});
		}

		public void TestSelectCargoSphereRelatedRate()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "TST", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "TST", rateEntry1));

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting1.ServiceGroupId = "1";
			RateChooserTestHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "TST");
			RateChooserTestHelper.AddPerContainerCharge(apiCosting2, "FRT", 2000);
			apiCosting1.ServiceGroupId = "2";

			var wiseRates = new[] { apiCosting1, apiCosting2 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();

			var cargoSphereRate = containerViewModel1.Rates.Skip(1).First();
			var cargoSphereRelatedRate = containerViewModel2.Rates.Skip(1).First();

			cargoSphereRate.IsSelected = true;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show applied for 20GP(GEN)", "Applied to 20GP () (GEN). Also apply this rate to: 20GP () (TST).", cargoSphereRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(GEN)", cargoSphereRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show rate can apply for 20GP(TST)", "Applied to 20GP () (GEN). Also apply this rate to: 20GP () (TST).", cargoSphereRelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(TST)", cargoSphereRelatedRate.SelectRelatedRatesApplyButtonVisibility);
			});

			cargoSphereRelatedRate.IsSelected = true;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show all rates selected for 20GP(GEN)", "This rate has been applied to these containers: 20GP () (GEN) & 20GP () (TST).", cargoSphereRate.SelectRelatedRatesText);
				Assert("Apply button should be collapsed for 20GP(GEN)", !cargoSphereRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show all rates selected for 20GP(TST)", "This rate has been applied to these containers: 20GP () (TST) & 20GP () (GEN).", cargoSphereRelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be collapsed for 20GP(TST)", !cargoSphereRelatedRate.SelectRelatedRatesApplyButtonVisibility);
			});

			var otherCargoSphereRate = containerViewModel1.Rates.Skip(2).First();
			var otherCargoSphereRelatedRate = containerViewModel2.Rates.Skip(2).First();

			otherCargoSphereRate.IsSelected = true;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show all rates selected for 20GP(GEN)", "Applied to 20GP () (TST). Also apply this rate to: 20GP () (GEN).", cargoSphereRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(GEN)", cargoSphereRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show all rates selected for 20GP(TST)", "Applied to 20GP () (TST). Also apply this rate to: 20GP () (GEN).", cargoSphereRelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(TST)", cargoSphereRelatedRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show all rates selected for 2nd 20GP(GEN)", "Applied to 20GP () (GEN). Also apply this rate to: 20GP () (TST).", otherCargoSphereRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 2nd 20GP(GEN)", otherCargoSphereRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show all rates selected for 2nd 20GP(TST)", "Applied to 20GP () (GEN). Also apply this rate to: 20GP () (TST).", otherCargoSphereRelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 2nd 20GP(TST)", otherCargoSphereRelatedRate.SelectRelatedRatesApplyButtonVisibility);
			});

			containerViewModel1.SelectedRow.IsSelected = false;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show all rates selected for 20GP(GEN)", "Applied to 20GP () (TST). Also apply this rate to: 20GP () (GEN).", cargoSphereRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(GEN)", cargoSphereRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show all rates selected for 20GP(TST)", "Applied to 20GP () (TST). Also apply this rate to: 20GP () (GEN).", cargoSphereRelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(TST)", cargoSphereRelatedRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show all rates selected for 2nd 20GP(GEN)", "Apply this rate to these containers: 20GP () (GEN) & 20GP () (TST).", otherCargoSphereRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 2nd 20GP(GEN)", otherCargoSphereRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show all rates selected for 2nd 20GP(TST)", "Apply this rate to these containers: 20GP () (TST) & 20GP () (GEN).", otherCargoSphereRelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 2nd 20GP(TST)", otherCargoSphereRelatedRate.SelectRelatedRatesApplyButtonVisibility);
			});
		}

		public void TestSelectCW1RelatedRate()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;

			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");

			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "20GP", "AFL", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);

			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "AFL", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var containerViewModel3 = viewModel.ContainerTabs.Skip(2).First();

			var cw1Rate = containerViewModel1.Rates[0];
			var cw1RelatedRate = containerViewModel2.Rates[0];
			var cw40GPRate = containerViewModel3.Rates[0];

			cw1Rate.IsSelected = true;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show applied for 20GP(GEN)", "Applied to 20GP () (GEN). Also apply this rate to: 20GP () (AFL) & 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(GEN)", cw1Rate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show rate can apply for 20GP(AFL)", "Applied to 20GP () (GEN). Also apply this rate to: 20GP () (AFL) & 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(AFL)", cw1RelatedRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show rate can apply for 40GP(GEN)", "Applied to 20GP () (GEN). Also apply this rate to: 40GP () (GEN) & 20GP () (AFL).", cw40GPRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 40GP(GEN)", cw40GPRate.SelectRelatedRatesApplyButtonVisibility);
			});

			cw1RelatedRate.IsSelected = true;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show applied for 20GP(GEN)", "Applied to 20GP () (GEN) & 20GP () (AFL). Also apply this rate to: 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(GEN)", cw1Rate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show rate can apply for 20GP(AFL)", "Applied to 20GP () (AFL) & 20GP () (GEN). Also apply this rate to: 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(AFL)", cw1RelatedRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show rate can apply for 40GP(GEN)", "Applied to 20GP () (GEN) & 20GP () (AFL). Also apply this rate to: 40GP () (GEN).", cw40GPRate.SelectRelatedRatesText);
				Assert("Apply button should be visible for 40GP(GEN)", cw40GPRate.SelectRelatedRatesApplyButtonVisibility);
			});

			cw40GPRate.IsSelected = true;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show applied for 20GP(GEN)", "This rate has been applied to these containers: 20GP () (GEN) & 20GP () (AFL) & 40GP () (GEN).", cw1Rate.SelectRelatedRatesText);
				Assert("Apply button should be collapsed for 20GP(GEN)", !cw1Rate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show applied for 20GP(AFL)", "This rate has been applied to these containers: 20GP () (AFL) & 20GP () (GEN) & 40GP () (GEN).", cw1RelatedRate.SelectRelatedRatesText);
				Assert("Apply button should be collapsed for 20GP(AFL)", !cw1RelatedRate.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show applied for 40GP(GEN)", "This rate has been applied to these containers: 40GP () (GEN) & 20GP () (GEN) & 20GP () (AFL).", cw40GPRate.SelectRelatedRatesText);
				Assert("Apply button should be collapsed for 40GP(GEN)", !cw40GPRate.SelectRelatedRatesApplyButtonVisibility);
			});
		}

		public void TestUnselectRateFromSummaryUpdateRelatedRateText()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();
			var costing = Helper.NewCosting(carrier);
			costing.TH_GC = Env.CurrentCompanyPK;
			var rateEntry1 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry1.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry1.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "40GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.KG, "AUD");
			Factory.Save();

			var consol = CreateConsol();
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			AddContainer(consol, "40GP", "GEN", Constants.ContainerModes.FCL);

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "20GP", "GEN", rateEntry1));
			model.AddCalculateCW1Rates(chooserHelper.NewCW1RateCombinations(criteria, "40GP", "GEN", rateEntry2));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			var containerViewModel1 = viewModel.ContainerTabs.First();
			var containerViewModel2 = viewModel.ContainerTabs.Skip(1).First();
			var rate20GP = containerViewModel1.Rates[0];
			var rate40GP = containerViewModel2.Rates[0];
			rate20GP.IsSelected = true;
			rate40GP.IsSelected = true;

			CombineAssertions(() =>
			{
				AssertEquals("Text should show applied for 20GP(GEN)", "This rate has been applied to these containers: 20GP () (GEN) & 40GP () (GEN).", rate20GP.SelectRelatedRatesText);
				Assert("Apply button should be collapsed for 20GP(GEN)", !rate20GP.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("TText should show applied for 40GP(GEN)", "This rate has been applied to these containers: 40GP () (GEN) & 20GP () (GEN).", rate40GP.SelectRelatedRatesText);
				Assert("Apply button should be collapsed for 40GP(GEN)", !rate40GP.SelectRelatedRatesApplyButtonVisibility);
			});

			viewModel.UnselectRateFromSummary(viewModel.SelectedRows.ToList()[1]);

			CombineAssertions(() =>
			{
				AssertEquals("Text should show applied for 20GP(GEN)", "Applied to 20GP () (GEN). Also apply this rate to: 40GP () (GEN).", rate20GP.SelectRelatedRatesText);
				Assert("Apply button should be visible for 20GP(GEN)", rate20GP.SelectRelatedRatesApplyButtonVisibility);

				AssertEquals("Text should show rate can apply for 40GP(GEN)", "Applied to 20GP () (GEN). Also apply this rate to: 40GP () (GEN).", rate40GP.SelectRelatedRatesText);
				Assert("Apply button should be visible for 40GP(GEN)", rate40GP.SelectRelatedRatesApplyButtonVisibility);
			});
		}

		#endregion

		public void TestSortRatesAndAutoSelectReservedRate()
		{
			var carrier = CreateCarrierOrg("SCAC");
			var consol = CreateConsol();
			consol.JK_UniqueConsignRef = "C0000123";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;
			AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL, 3);

			var costing = Helper.NewCosting(carrier);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "USLAX", "HKHKG", "", "20GP");
			rateEntry.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry.TI_ContractNumber = "CN1";
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "USD");
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var rateEntry2 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "US", "", "", "20GP");
			rateEntry2.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry2.TI_ContractNumber = "CN2";
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine2 = rateEntry2.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "USD");
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 160m;
			rateEntry2.AddRateLine(Helper.ChargeCodes["BAF"], FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 350m;

			var rateEntry3 = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "", "HK", "", "20GP");
			rateEntry3.TI_RH_NKCommodityCode = ZString.Empty;
			rateEntry3.TI_ContractNumber = "CN3";
			rateEntry3.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry3.AddRateLine(Helper.ChargeCodes["FRT"], UnitCalculator.Code, QuantityUnit.CN, "USD");
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 160m;
			rateEntry3.AddRateLine(Helper.ChargeCodes["CAF"], FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 140;

			Factory.Save();

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting1.ReservedForJobIDs = new[] { consol.JK_UniqueConsignRef.ToString() };
			ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000).WithCurrency("USD");

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			ChooserHelper.AddPerContainerCharge(apiCosting2, "FRT", 3000).WithCurrency("USD");

			var apiCosting3 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			ChooserHelper.AddPerContainerCharge(apiCosting3, "FRT", 2000).WithCurrency("USD");

			((GlbCompany)Env.CurrentCompany).GC_RX_NKLocalCurrency = "USD";
			var wiseRates = new[] { apiCosting1, apiCosting2, apiCosting3 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);
			var logger = new TestLogger();
			var testWiseRatesProvider = ChooserHelper.CreateMockWiseRateProviderWithResponse(response);
			var cw1Provider = new CW1RatesProvider(Factory, logger);

			var context = new RatingContext(new LoggerDecorator(logger), Factory, cw1Provider, testWiseRatesProvider, null, true);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var filter = new RateChooserFilterStripBusinessObject(criteria);
			var model = new RateChooserModel(criteria, context);
			model.SendRatesRequest(filter, BuildRatesQuery(criteria));
			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();
			AssertEquals(1, viewModel.ContainerTabs.Count());

			var containerCommodity = viewModel.ContainerTabs.First();
			var expectedSelectedRow = containerCommodity.Rates.Single(x => x.Rate?.WiseRateEntry?.ReservedForJobIDs != null && x.Rate.WiseRateEntry.ReservedForJobIDs.Contains("C0000123"));
			AssertEquals(expectedSelectedRow, containerCommodity.SelectedRow);

			CombineAssertions("Should Sort by reserved, CW1 by @CN then by BOL, then by CS ", () =>
			{
				AssertEquals("the first rate should be the reserved rate", expectedSelectedRow.Rate, containerCommodity.Rates[0].Rate);

				AssertEquals("the second rate should be CW1 rate", string.Empty, containerCommodity.Rates[1].Rate.RateProvider);
				AssertEquals("the second rate should be CW1 rate with lowest @CN 150x3 = 450", "$450.00", containerCommodity.Rates[1].CW1FreightCharges.TotalPriceString);

				AssertEquals("the third rate should be CW1 rate", string.Empty, containerCommodity.Rates[2].Rate.RateProvider);
				AssertEquals("the third rate should be CW1 rate with lowest @CN 160x3 = 480", "$480.00", containerCommodity.Rates[2].CW1FreightCharges.TotalPriceString);
				AssertEquals("the third rate should be CW1 rate with lowest BOL", "$140.00", containerCommodity.Rates[2].CW1BillOfLadingCharges.TotalPriceString);

				AssertEquals("the forth rate should be CW1 rate", string.Empty, containerCommodity.Rates[3].Rate.RateProvider);
				AssertEquals("the forth rate should be CW1 rate with lowest @CN 160x3 = 480", "$480.00", containerCommodity.Rates[3].CW1FreightCharges.TotalPriceString);
				AssertEquals("the forth rate should be CW1 rate with highest BOL", "$350.00", containerCommodity.Rates[3].CW1BillOfLadingCharges.TotalPriceString);

				AssertEquals("the fifth rate should be CargoSphere Rate", WRConstants.RateProviders.CargoSphere, containerCommodity.Rates[4].Rate.RateProvider);
				AssertEquals("the fifth rate should be CargoSphere Rate as it comes from CS (Not sorted) 3000x3 = 9000",
					"$9,000.00",
					containerCommodity.Rates[4].OceanCharges.Charges.First(c => c.Group == ChargesViewModel.ChargesGroup.Base).TotalPriceString);

				AssertEquals("the sixth rate should be CargoSphere Rate", WRConstants.RateProviders.CargoSphere, containerCommodity.Rates[5].Rate.RateProvider);
				AssertEquals("the sixth rate should be CargoSphere Rate as it comes from CS (Not sorted) 2000x3 = 6000",
					"$6,000.00",
					containerCommodity.Rates[5].OceanCharges.Charges.First(c => c.Group == ChargesViewModel.ChargesGroup.Base).TotalPriceString);
			});
		}

		public void TestGrouppingCargoSphereRatesBasedOnContainerQuality()
		{
			var helper = new RateChooserTestHelper(Factory);
			var carrier = helper.CreateCarrierOrg("SCAC");
			Factory.Save();

			var consol = CreateConsol();
			var container1 = AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			var container2 = AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			container2.JC_ContainerQuality = "GOH";
			var container3 = AddContainer(consol, "20GP", "GEN", Constants.ContainerModes.FCL);
			container3.JC_ContainerQuality = "XYZ";

			var logger = new ElementaryLogger();
			var context = new RatingContext(logger);
			var autoRating = consol.RatingAdapter;
			var autoRatingInfo = new AutoRatingProxy(autoRating);
			var criteria = new RatingCriteria(autoRatingInfo, Factory);
			var model = new RateChooserModel(criteria, context);

			var apiCosting1 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting1.ServiceGroupId = "1";
			ChooserHelper.SetContainerQuality(apiCosting1, null); // Should be appear only on the tab with blank Quality
			ChooserHelper.AddPerContainerCharge(apiCosting1, "FRT", 1000);

			var apiCosting2 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting2.ServiceGroupId = "2";
			ChooserHelper.SetContainerQuality(apiCosting2, "GOH"); // Should be appear only on GOH tab
			ChooserHelper.AddPerContainerCharge(apiCosting2, "FRT", 2000);

			var apiCosting3 = ChooserHelper.CreateApiRate("20GP", carrier, "");
			apiCosting3.ServiceGroupId = "3";
			ChooserHelper.SetContainerQuality(apiCosting3, "ABC"); // Should NOT be appear on any tab
			ChooserHelper.AddPerContainerCharge(apiCosting3, "FRT", 3000);

			var wiseRates = new[] { apiCosting1, apiCosting2, apiCosting3 };
			var response = ChooserHelper.CreateWiseRateResponseForCargoSphere(wiseRates);

			model.AddWiseRatesForTest(response);

			var viewModel = new RateChooserViewModel(model);
			viewModel.RefreshRates();

			CombineAssertions("Container with blank Quality", () =>
			{
				var containerViewModel = viewModel.ContainerTabs.First();
				AssertEquals("1 x 20GP () (GEN) - 1 Rate(s)", containerViewModel.TabText);

				var actualRateGroup = containerViewModel.Rates.Select(x => x.Rate.WiseRateEntry).ToList();
				AssertEquals(1, actualRateGroup.Count);
				AssertEquals("", string.Join(",", actualRateGroup.Select(x => x.ContainerQuality())));
			});

			CombineAssertions("Container with Quality specified, GOH", () =>
			{
				var containerViewModel = viewModel.ContainerTabs.Skip(1).First();
				AssertEquals("1 x 20GP (GOH) (GEN) - 1 Rate(s)", containerViewModel.TabText);

				var actualRateGroup = containerViewModel.Rates.Select(x => x.Rate.WiseRateEntry).ToList();
				AssertEquals(1, actualRateGroup.Count);
				AssertEquals("GOH", string.Join(",", actualRateGroup.Select(x => x.ContainerQuality())));
			});

			CombineAssertions("Container with Quality specified, XYZ", () =>
			{
				var containerViewModel = viewModel.ContainerTabs.Skip(2).First();
				AssertEquals("1 x 20GP (XYZ) (GEN) - 0 Rate(s)", containerViewModel.TabText);

				var actualRateGroup = containerViewModel.Rates.Select(x => x.Rate.WiseRateEntry).ToList();
				AssertEquals(0, actualRateGroup.Count);
			});
		}

		#region Helper

		OrgHeader CreateCarrierOrg(string scac = "SCAC") => ChooserHelper.CreateCarrierOrg(scac);
		public ForwardingConsol CreateConsol(string origin = "USLAX", string destination = "HKHKG") => ChooserHelper.CreateConsol(origin, destination);

		public ForwardingContainer AddContainer(
			ForwardingConsol consol,
			string code = "20GP",
			string commodityCode = "GEN",
			string containerMode = Constants.ContainerModes.FCL,
			short containerCount = 1)
			=> ChooserHelper.AddContainer(consol, code, commodityCode, containerMode, containerCount);

		public RateEntry CreateRate(
			RatingHeader header,
			string category = "FCL",
			string mode = "SEA",
			string origin = "USLAX",
			string destination = "HKHKG",
			string container = "20GP",
			string commodity = "GEN",
			string carrierServiceLevel = "STD")
		{
			var entry = header.AddRateEntry(category, mode, origin, destination, carrierServiceLevel, container, commodity);
			entry.RateLines.RemoveAndDeleteAll();

			// Force set the commodity instead it being ignored if it is null or empty by AddRateEntry
			entry.TI_RH_NKCommodityCode = commodity;

			return entry;
		}

		Api.Model.RatesQuery BuildRatesQuery(RatingCriteria criteria)
		{
			if (wiseRatesQueryBuilder == null)
			{
				wiseRatesQueryBuilder = new WiseRatesQueryBuilder(new DummyLogger());
			}

			var (ratesQuery, _) = wiseRatesQueryBuilder.Build(criteria);
			return ratesQuery;
		}
		WiseRatesQueryBuilder wiseRatesQueryBuilder;

		protected RateChooserTestHelper ChooserHelper
		{
			get { return chooserHelper ?? (chooserHelper = new RateChooserTestHelper(Factory)); }
		}
		RateChooserTestHelper chooserHelper;

		#endregion
	}
}

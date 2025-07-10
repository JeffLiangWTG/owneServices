using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	internal class RateLineItemsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHousebillReleaseTypesHasAllItem()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateLine = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			var item = rateLine.RateLineItems.AddNew();

			Assert("All item is present", item.Lookups.HousebillReleaseTypes.ContainsCode(RateLineItemsLookups.StandardReleaseType));
		}

		public void TestPercentOfs()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT");
			Factory.Save();
			var localChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);

			var localClientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var percentOfs = localClientRate.AddRateEntry(RatingConstants.RateCategory.AIR).RateLines[0].RateLineItems.AddNew().Lookups.ChargeCodes;
			percentOfs.Load();

			var chargeCodePKs = percentOfs.Select(x => x.PK).ToArray();
			AssertCollectionContains("Should only include the Local FRT charge code", localChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalChargeCode.PK, chargeCodePKs);
		}

		public void TestPercentOfs_GlobalRates()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT");
			Factory.Save();
			var localChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);

			var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
			var percentOfs = globalClientRate.AddRateEntry(RatingConstants.RateCategory.AIR).RateLines[0].RateLineItems.AddNew().Lookups.ChargeCodes;
			percentOfs.Load();

			var chargeCodePKs = percentOfs.Select(x => x.PK).ToArray();
			AssertCollectionContains("Should only include the Global FRT charge code", globalChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(localChargeCode.PK, chargeCodePKs);
		}

		public void TestPercentOfs_PublishedEntry()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT");
			Factory.Save();

			var globalCosting = Helper.NewGlobalCosting(null);
			var localCosting = Helper.NewCosting(null);
			var localCostEntry = localCosting.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN");
			localCostEntry.IsPublished = true;

			var percentOfs = localCostEntry.RateLines[0].RateLineItems.AddNew().Lookups.ChargeCodes;
			percentOfs.Load();

			var chargeCodePKs = percentOfs.Select(x => x.PK).ToArray();
			AssertCollectionContains("Should only include the Global FRT charge code", globalChargeCode.PK, chargeCodePKs);
			AssertCollectionNotContains(globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK).PK, chargeCodePKs);
		}

		public void TestWeightBreaks()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var line = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			line.Calculator.UseInclusiveBreaks = true;
			AssertEquals("Less Than Or Equal To", line.RateLineItems[0].Lookups.WeightBreaks.GetDescriptionFromCode("-"));
			AssertEquals("Greater Than", line.RateLineItems[0].Lookups.WeightBreaks.GetDescriptionFromCode("+"));
			line.Calculator.UseInclusiveBreaks = false;
			AssertEquals("Less Than", line.RateLineItems[0].Lookups.WeightBreaks.GetDescriptionFromCode("-"));
			AssertEquals("Greater Than Or Equal To", line.RateLineItems[0].Lookups.WeightBreaks.GetDescriptionFromCode("+"));
		}

		public void TestTimeUnits()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var airRateLine = clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
			airRateLine.TL_RateCalculator = "TME";

			var airRateLineTimeLineItem = airRateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 0, 20);

			var allTimeUnits = UnitHelper.GetUnitList(Factory, RateType.Forwarding, airRateLine.Country().RN_Code)
				.OfType<ICodeDescription>()
				.Where(u => (new[] { "HR", "DY", "WK" }).Contains(u.Code));

			var warehouseTimeUnits = UnitHelper.GetUnitList(Factory, RateType.Forwarding, airRateLine.Country().RN_Code)
				.OfType<ICodeDescription>()
				.Where(u => (new[] { "DY", "WK" }).Contains(u.Code));

			Assert("Air rate should have 3 time units", airRateLineTimeLineItem.Lookups.TimeUnits.Count == 3);
			AssertContainsExactElementsInAnyOrder(allTimeUnits, airRateLineTimeLineItem.Lookups.TimeUnits);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "WH";
			chargeCode.AC_ChargeGroup = "WHS";
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;

			var warehouseRateEntry = clientRate.AddRateEntry("WHS", "ALL", string.Empty, string.Empty);
			var warehouseRateLine = warehouseRateEntry.AddRateLine("WH", "TME", "KG");

			var warehouseRateLineTimeLineItem = warehouseRateLine.Calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 0, 20);

			Assert("Warehouse rate should have 2 time units", warehouseRateLineTimeLineItem.Lookups.TimeUnits.Count == 2);
			AssertContainsExactElementsInAnyOrder(warehouseTimeUnits, warehouseRateLineTimeLineItem.Lookups.TimeUnits);

			Assert("Air rate should have 3 time units", airRateLineTimeLineItem.Lookups.TimeUnits.Count == 3);
			AssertContainsExactElementsInAnyOrder(allTimeUnits, airRateLineTimeLineItem.Lookups.TimeUnits);
		}

		public void TestWeightVolumes_WarehousePackageLine()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.WHS, "ALL", "AU", "US");
			var rateLine = rateEntry.AddRateLine("WHSCHG", CombinedCalculator.Code, QuantityUnit.KG);
			rateLine.TL_UnitFactor = UnitFactorList.Codes.PackageLine;

			var rateLineItem = rateLine.RateLineItems.AddNew();
			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;

			var units = rateLineItem.Lookups.WeightVolumes.Cast<CodeDescriptionPair>().Select(weightVolume => weightVolume.Code);
			AssertContainsExactElementsInAnyOrder
			(
				"KG with Minus",
				new[] { "MI", "KM" },
				units
			);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertContainsExactElementsInAnyOrder("First plus should act like minus if no minus exists", new[] { "MI", "KM" }, units);

			rateLine.TL_WeightVolume = Core.Constants.PkgUnit.Package;
			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			units = rateLineItem.Lookups.WeightVolumes.Cast<CodeDescriptionPair>().Select(weightVolume => weightVolume.Code);
			AssertContainsExactElementsInAnyOrder
			(
				"Package with Minus: should not contain TI and PK because it is not supported",
				new[] { "DT", "G", "HG", "KG", "KT", "LB", "LT", "MC", "MG", "OT", "OZ", "T", "TL", "TN", "CC", "CF", "CI", "CY", "D3", "GA", "GI", "L", "M3", "ML", "TE" },
				units
			);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;
			AssertContainsExactElementsInAnyOrder
			(
				"Package with plus: first plus should act like minus if no minus exists and should not contain TI and PK because it is not supported",
				new[] { "DT", "G", "HG", "KG", "KT", "LB", "LT", "MC", "MG", "OT", "OZ", "T", "TL", "TN", "CC", "CF", "CI", "CY", "D3", "GA", "GI", "L", "M3", "ML", "TE" },
				units
			);
		}

		public void TestDistanceUnits()
		{
			var rateEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("ORG", "LCL", "AUSYD", "USLAX");
			var rateLine = rateEntry.AddRateLine("ODOC", CombinedCalculator.Code, Core.Constants.Weight.Kilograms);
			var rateLineItem = rateLine.RateLineItems.AddNew();
			rateLineItem.TM_Type = Calculator.Items.Operator.Minus;
			var units = rateLineItem.Lookups.WeightVolumes.Cast<CodeDescriptionPair>().Select(x => x.Code);

			AssertContainsExactElementsInAnyOrder(new[] { "MI", "KM" }, units);

			rateLineItem.TM_Type = Calculator.Items.Operator.Plus;

			AssertContainsExactElementsInAnyOrder("First plus should act like minus if no minus exists", new[] { "MI", "KM" }, units);
		}

		public void TestWeightVolumesRestrictedWithTeu()
		{
			var rateLineItem = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("FCL", "SEA", "AUSYD", "USLAX").RateLines[0].RateLineItems[0];
			AssertEquals("Package", rateLineItem.Lookups.WeightVolumes.GetDescriptionFromCode(QuantityUnit.PK));
			AssertEquals("Twenty foot equivalent unit", rateLineItem.Lookups.WeightVolumes.GetDescriptionFromCode(QuantityUnit.TU));
		}

		public void TestWeightVolumesRestrictedWithoutTeu()
		{
			var rateLineItem = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("FCL", "AIR", "AUSYD", "USLAX").RateLines[0].RateLineItems[0];
			var weightVolumes = rateLineItem.Lookups.WeightVolumes;
			AssertEquals("Package", weightVolumes.GetDescriptionFromCode(QuantityUnit.PK));
			AssertNull("Don't have to return Twenty foot equivalent unit", weightVolumes.GetDescriptionFromCode(QuantityUnit.TU));
		}

		public void TestPercentageApplyToListWithLoadingAndCustomsBrokerageCharges_ClientRate()
			=> AssertPercentageApplyToListWithLoadingAndCustomsBrokerageCharges(Helper.NewClientRate(Helper.NewOrgHeader()), true);

		public void TestPercentageApplyToListWithLoadingAndCustomsBrokerageCharges_CompanyTariff()
			=> AssertPercentageApplyToListWithLoadingAndCustomsBrokerageCharges(Factory.NewWithValidTestData<CompanyTariff>(), true);

		public void TestPercentageApplyToListWithLoadingAndCustomsBrokerageCharges_Costing()
			=> AssertPercentageApplyToListWithLoadingAndCustomsBrokerageCharges(Helper.NewCosting(Helper.NewOrgHeader()), true);

		public void TestPercentageApplyToListWithLoadingAndCustomsBrokerageCharges_Quote()
			=> AssertPercentageApplyToListWithLoadingAndCustomsBrokerageCharges(Helper.NewQuote(Helper.NewOrgHeader()), true);

		public void TestPercentageApplyToListWithLoadingAndCustomsBrokerageCharges_IntercompanyTariff()
			=> AssertPercentageApplyToListWithLoadingAndCustomsBrokerageCharges(Helper.NewIntercompanyTariff(), false);

		void AssertPercentageApplyToListWithLoadingAndCustomsBrokerageCharges(RatingHeader header, bool expectedValue)
		{
			//Forwarding Tab
			AssertListItems(RatingConstants.RateCategory.AIR, expectedValue);
			AssertListItems(RatingConstants.RateCategory.FCL, expectedValue);
			AssertListItems(RatingConstants.RateCategory.LCL, expectedValue);
			AssertListItems(RatingConstants.RateCategory.ORG, expectedValue);
			AssertListItems(RatingConstants.RateCategory.DST, expectedValue);

			//LinerAndAgency Tab
			AssertListItems(RatingConstants.RateCategory.SOR, expectedValue);
			AssertListItems(RatingConstants.RateCategory.SDE, expectedValue);
			AssertListItems(RatingConstants.RateCategory.SCO, expectedValue);
			AssertListItems(RatingConstants.RateCategory.SNC, expectedValue);
			AssertListItems(RatingConstants.RateCategory.SED, expectedValue);
			AssertListItems(RatingConstants.RateCategory.SID, expectedValue);

			//Other Tabs
			AssertListItems(RatingConstants.RateCategory.CST, false);
			AssertListItems(RatingConstants.RateCategory.PAC, false);
			AssertListItems(RatingConstants.RateCategory.UNP, false);
			AssertListItems(RatingConstants.RateCategory.TRN, false);
			AssertListItems(RatingConstants.RateCategory.TBC, false);
			AssertListItems(RatingConstants.RateCategory.TRW, false);
			AssertListItems(RatingConstants.RateCategory.WHS, false);
			AssertListItems(RatingConstants.RateCategory.CYD, false);

			void AssertListItems(string category, bool expected)
			{
				var entry = header.AddRateEntry(category);
				entry.RateLines.RemoveAndDeleteAll();

				var rateLine = entry.AddRateLine(ChargeCodeGroupList.Codes.Freight, PercentageCalculator.Code);
				var lineItem = rateLine.RateLineItems[0];
				var list = lineItem.Lookups().PercentageApplyToList;

				if (expected)
				{
					Factory.TryGetValueFromCacheOnly<ValueApplyToListCodeDescriptionPairList>("PercentageApplyToListWithLoadingAndCustomsBrokerageCharges", out var cachedList);
					AssertNotNull("cachedList should not be null", cachedList);
					AssertEquals("list should be returning from cached object", cachedList, list);
				}

				CombineAssertions($"PercentageApplyToListWithLoadingAndCustomsBrokerageCharges tests failed for category: {category}", () =>
				{
					AssertEquals("LoadingCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.LoadingCharges));
					AssertEquals("OriginCustomsBrokerageCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.OriginCustomsBrokerageCharges));
					AssertEquals("CustomsBrokerageCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.CustomsBrokerageCharges));
					AssertEquals("UnloadingCharges: should meet expected value.", expected, list.ContainsCode(CalculatorConstants.Text.UnloadingCharges));
				});
			}
		}

		#region Implementation

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}

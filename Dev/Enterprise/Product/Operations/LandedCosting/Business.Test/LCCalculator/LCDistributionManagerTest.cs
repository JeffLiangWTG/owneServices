using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.LandedCosting.Business.LCDistributionManager;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LCDistributionManagerTest : TestCaseWithFactory
	{
		[TestDate(2005, 5, 10)]
		public void TestDateOfProcessingIsSet()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();

			LCDistributionManager manager = new LCDistributionManager(lCHeader);
			manager.RunLandedCosting();
			AssertEquals("Date of processing is set", "10-May-05", lCHeader.LT_DateOfProcessing.ToShortDateString());
		}

		public void TestBackRoundingEndToEnd()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithBackRoundingTest();

			LCDistributionManager manager = new LCDistributionManager(lCHeader);
			manager.RunLandedCosting();

			AssertEquals("There should be three histories", 3, lCHeader.Histories.Count);
			ZDecimal apportioned = lCHeader.Histories[0].LH_LandedCostGroup1 + lCHeader.Histories[1].LH_LandedCostGroup1 + lCHeader.Histories[2].LH_LandedCostGroup1;
			AssertEquals("TotalGroup1 from Lines should be equal to the initial Cost", helper.LCInput1.LI_CostAmount, apportioned, 0.01m);
		}

		[ExpectNoExceptions]
		public void TestLCHeaderWithNoHistoryRunLandedCostingWithoutError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var helper = new TestHelper(Factory);
				var landedCostHeader = helper.GetLCHeaderWithNothingButOneCostInput();
				var manager = new LCDistributionManager(landedCostHeader);
				manager.RunLandedCosting();
			}
		}

		public void TestBackRoundingForCustomsFeeDutyTaxes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				TestHelper helper = new TestHelper(Factory);
				LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();

				DutyTaxEntryFee total = new DutyTaxEntryFee();
				total["TDT"] = 10.25m;
				total["ENT"] = 10.25m;
				total["OTH"] = 10.25m;
				total["ST1"] = 10.25m;
				total["ST2"] = 10.25m;
				total["ST3"] = 10.25m;
				total["EXC"] = 10.25m;
				total["QUA"] = 10.25m;
				helper.DummyHeader.TotalDutyTaxEntryFeeItemsExposed = total;

				helper.Ultimate1.CostInLocalCurrencyExposed = 100m;
				helper.Ultimate2.CostInLocalCurrencyExposed = 50m;

				DutyTaxEntryFee dutyTaxEntryFeeLine1 = new DutyTaxEntryFee();
				DutyTaxEntryFee dutyTaxEntryFeeLine2 = new DutyTaxEntryFee();
				dutyTaxEntryFeeLine1["TDT"] = 10m;
				dutyTaxEntryFeeLine2["TDT"] = 0.24m;
				dutyTaxEntryFeeLine1["ENT"] = 0.24m;
				dutyTaxEntryFeeLine2["ENT"] = 10m;
				dutyTaxEntryFeeLine1["OTH"] = 0.24m;
				dutyTaxEntryFeeLine2["OTH"] = 10m;
				dutyTaxEntryFeeLine1["ST1"] = 10m;
				dutyTaxEntryFeeLine2["ST1"] = 0.24m;
				dutyTaxEntryFeeLine1["ST2"] = 0.24m;
				dutyTaxEntryFeeLine2["ST2"] = 10m;
				dutyTaxEntryFeeLine1["ST3"] = 10m;
				dutyTaxEntryFeeLine2["ST3"] = 0.24m;
				dutyTaxEntryFeeLine1["QUA"] = 10m;
				dutyTaxEntryFeeLine2["QUA"] = 0.24m;
				helper.Ultimate1.LineDutyTaxEntryFeeItemsExposed = dutyTaxEntryFeeLine1;
				helper.Ultimate2.LineDutyTaxEntryFeeItemsExposed = dutyTaxEntryFeeLine2;

				LCDistributionManager manager = new LCDistributionManager(lCHeader);
				manager.RunLandedCosting();

				LandedCostHistory lCHistory1 = lCHeader.Histories.GetOrCreate(helper.Ultimate1);
				LandedCostHistory lCHistory2 = lCHeader.Histories.GetOrCreate(helper.Ultimate2);

				AssertEquals("BackRounded for Duty", 10.01m, lCHistory1.GetLineValue("TDT"));
				AssertEquals("BackRounded for Duty", 0.24m, lCHistory2.GetLineValue("TDT"));
				AssertEquals("BackRounded for EntryFee", 0.24m, lCHistory1.GetLineValue("ENT"));
				AssertEquals("BackRounded for EntryFee", 10.01m, lCHistory2.GetLineValue("ENT"));
				AssertEquals("BackRounded for OtherOrFlatDuty", 0.24m, lCHistory1.GetLineValue("OTH"));
				AssertEquals("BackRounded for OtherOrFlatDuty", 10.01m, lCHistory2.GetLineValue("OTH"));
				AssertEquals("BackRounded for SpecialTax1", 10.01m, lCHistory1.GetLineValue("ST1"));
				AssertEquals("BackRounded for SpecialTax1", 0.24m, lCHistory2.GetLineValue("ST1"));
				AssertEquals("BackRounded for SpecialTax2", 0.24m, lCHistory1.GetLineValue("ST2"));
				AssertEquals("BackRounded for SpecialTax2", 10.01m, lCHistory2.GetLineValue("ST2"));
				AssertEquals("BackRounded for SpecialTax3", 10.01m, lCHistory1.GetLineValue("ST3"));
				AssertEquals("BackRounded for SpecialTax3", 0.24m, lCHistory2.GetLineValue("ST3"));
				AssertEquals("BackRounded for QuarantineFee", 10.01m, lCHistory1.GetLineValue("QUA"));
				AssertEquals("BackRounded for QuarantineFee", 0.24m, lCHistory2.GetLineValue("QUA"));
			}
		}

		public void TestAddRoundingDifferenceBackToLine()
		{
			GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory1 = lCHeader.Histories.AddNew();
			lCHistory1.LH_LandedCostGroup1 = 50m;
			lCHistory1.LH_LandedCostGroup2 = 55m;

			LandedCostHistory lCHistory2 = lCHeader.Histories.AddNew();
			lCHistory2.LH_LandedCostGroup1 = 60m;
			lCHistory2.LH_LandedCostGroup2 = 65m;

			LandedCostHistory lCHistory3 = lCHeader.Histories.AddNew();
			lCHistory3.LH_LandedCostGroup1 = 70m;
			lCHistory3.LH_LandedCostGroup2 = 75m;

			var list = new List<LandedCostHistory>(lCHeader.Histories);
			Manager.AddRoundingDifferenceBackToLine(list, 100.00m, 99.98m, LandedLineCostType.Codes.LandedCostGroup1);
			AssertEquals("The difference should go to the first two records", 50.01m, lCHistory1.LH_LandedCostGroup1);
			AssertEquals("The difference should go to the first two records", 60.01m, lCHistory2.LH_LandedCostGroup1);
			AssertEquals("The difference should go to the first two records", 70m, lCHistory3.LH_LandedCostGroup1);

			Manager.AddRoundingDifferenceBackToLine(list, 100.00m, 100.01m, LandedLineCostType.Codes.LandedCostGroup2);
			AssertEquals("The difference should go to the first record", 54.99m, lCHistory1.LH_LandedCostGroup2);
			AssertEquals("The difference should go to the first record", 65m, lCHistory2.LH_LandedCostGroup2);
			AssertEquals("The difference should go to the first record", 75m, lCHistory3.LH_LandedCostGroup2);
		}

		[ExpectNoExceptions]
		public void TestAddRoundingDifferenceBackToLineDoesNotCauseException()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandedCostHistory lCHistory1 = lCHeader.Histories.AddNew();
			AssertEquals("One LC history", 1, lCHeader.Histories.Count);
			var list = new List<LandedCostHistory>(lCHeader.Histories);
			Manager.AddRoundingDifferenceBackToLine(list, 100.00m, 100.03m, LandedLineCostType.Codes.LandedCostGroup1);

			Manager.MarkHighestUltimateDistributeeFor("ST2", null, lCHistory1, 10);
			Manager.AddRoundingDifferenceBackToLineForCustomsCharges(100.00m, 100.03m, "ST2");
		}

		public void TestRunLandedCostingForCustomsCharges()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.DutyPercentExposed = 10m;

			DutyTaxEntryFee line = new DutyTaxEntryFee();
			line["ENT"] = 100m;
			line["EXC"] = 200m;
			line["OTH"] = 300m;
			line["ST1"] = 400m;
			line["ST2"] = 500m;
			line["ST3"] = 600m;
			line["TDT"] = 700m;

			distributee.LineDutyTaxEntryFeeItemsExposed = line;

			DummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { distributee };
			Manager.RunLandedCostingForCustomsCharges();
			AssertEquals("1 History created", 1, LCHeader.Histories.Count);
			LandedCostHistory lCHistory = LCHeader.Histories[0];
			AssertEquals("Duty Percent", distributee.DutyPercent, lCHistory.LH_DutyPercent);
			AssertEquals("Duty Percent", line["ENT"], lCHistory.GetLineValue("ENT"));
			AssertEquals("Duty Percent", line["EXC"], lCHistory.GetLineValue("EXC"));
			AssertEquals("Duty Percent", line["OTH"], lCHistory.GetLineValue("OTH"));
			AssertEquals("Duty Percent", line["ST1"], lCHistory.GetLineValue("ST1"));
			AssertEquals("Duty Percent", line["ST2"], lCHistory.GetLineValue("ST2"));
			AssertEquals("Duty Percent", line["ST3"], lCHistory.GetLineValue("ST3"));
			AssertEquals("Duty Percent", line["TDT"], lCHistory.GetLineValue("TDT"));
		}

		public void TestRunLandedCostingTwiceForCustomsCharges_ConsistentlyReturnsSameResults()
		{
			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.TableCodeExposed = "";
			DutyTaxEntryFee line = new DutyTaxEntryFee();
			distributee.LineDutyTaxEntryFeeItemsExposed = line;
			DummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { distributee };

			Manager.RunLandedCosting();
			AssertEquals("1 History created", 1, Manager.LCHeader.Histories.Count);

			Manager.RunLandedCosting();
			AssertEquals("History still exists", 1, Manager.LCHeader.Histories.Count);
		}

		public void TestGetDistinctParentIDAndTypes()
		{
			DummyLandedCostDistributeTo distribute1 = Factory.New<DummyLandedCostDistributeTo>();
			DummyLandedCostDistributeTo distribute2 = Factory.New<DummyLandedCostDistributeTo>();
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute1, distribute2 };

			LandCostInput lCInput1 = LCHeader.CostInputs.AddNew();
			lCInput1.LI_ParentID = distribute1.PK;
			lCInput1.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LandCostInput lCInput2 = LCHeader.CostInputs.AddNew();
			lCInput2.LI_ParentID = distribute1.PK;
			lCInput2.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LandCostInput lCInput3 = LCHeader.CostInputs.AddNew();
			lCInput3.LI_ParentID = distribute2.PK;
			lCInput3.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			AssertEquals("Parent of LCInput1", distribute1, lCInput1.Parent);
			AssertEquals("Parent of LCInput2", distribute1, lCInput2.Parent);
			AssertEquals("Parent of LCInput3", distribute2, lCInput3.Parent);

			Dictionary<ZGuid, ZString> result = Manager.GetDistinctParentIDAndTypes();
			AssertEquals("Two keys", 2, result.Keys.Count);
			AssertEquals("Result has Distribute1 PK", true, result.ContainsKey(distribute1.PK));
			AssertEquals("Result has Distribute2 PK", true, result.ContainsKey(distribute2.PK));
		}

		public void TestGetRatioAccordingToDistributeBy()
		{
			DummyLandedCostDistributeTo distribute = Factory.New<DummyLandedCostDistributeTo>();
			DummyIUltimateDistributee ultimate1 = Factory.New<DummyIUltimateDistributee>();
			DummyIUltimateDistributee ultimate2 = Factory.New<DummyIUltimateDistributee>();
			ultimate1.ActualVolumeInM3Exposed = 2;
			ultimate2.ActualVolumeInM3Exposed = 3;

			distribute.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1, ultimate2 };
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute };
			LCFactors totals = Manager.GetTotals(distribute);
			DistributionFactors result = Manager.GetRatioAccordingToDistributeBy(ultimate1, totals, CostDistributionMechanismList.Codes.ActualVolume);
			AssertEquals("Numerator", 2m, result.Numerator);
			AssertEquals("Denominator", 5m, result.Denominator);

			result = Manager.GetRatioAccordingToDistributeBy(ultimate2, totals, CostDistributionMechanismList.Codes.ActualVolume);
			AssertEquals("Numerator", 3m, result.Numerator);
			AssertEquals("Denominator", 5m, result.Denominator);
		}

		public void TestDistributeWithDifferentLevel()
		{
			DummyLandedCostDistributeTo distribute1 = Factory.New<DummyLandedCostDistributeTo>();
			DummyLandedCostDistributeTo distribute2 = Factory.New<DummyLandedCostDistributeTo>();
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute1, distribute2 };

			DummyIUltimateDistributee ultimate1 = Factory.New<DummyIUltimateDistributee>();
			ultimate1.PKExposed = ZGuid.NewZGuid();
			ultimate1.TableCodeExposed = "XX";

			DummyIUltimateDistributee ultimate2 = Factory.New<DummyIUltimateDistributee>();
			ultimate2.PKExposed = ZGuid.NewZGuid();
			ultimate2.TableCodeExposed = "XX";

			DummyIUltimateDistributee ultimate3 = Factory.New<DummyIUltimateDistributee>();
			ultimate3.PKExposed = ZGuid.NewZGuid();
			ultimate3.TableCodeExposed = "XX";

			ultimate1.CostInLocalCurrencyExposed = 100;
			ultimate2.CostInLocalCurrencyExposed = 200;
			ultimate3.CostInLocalCurrencyExposed = 400;

			distribute1.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1, ultimate2, ultimate3 };
			distribute2.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate2, ultimate3 };
			DummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1, ultimate2, ultimate3 };

			LandCostInput lCInput1 = LCHeader.CostInputs.AddNew();
			lCInput1.LI_ParentID = distribute1.PK;
			lCInput1.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			lCInput1.LI_CostAmount = 10m;
			lCInput1.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			lCInput1.LI_LandedCostGroup = 5;
			lCInput1.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;

			LandCostInput lCInput2 = LCHeader.CostInputs.AddNew();
			lCInput2.LI_ParentID = distribute2.PK;
			lCInput2.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			lCInput2.LI_CostAmount = 25m;
			lCInput2.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			lCInput2.LI_LandedCostGroup = 5;
			lCInput2.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;

			Manager = new LCDistributionManager(LCHeader);
			Manager.RunLandedCosting();

			ZDecimal total = ZDecimal.Zero;

			foreach (var history in LCHeader.Histories)
			{
				total += history.RoundedGroup5;
			}

			AssertEquals(35m, total);
		}

		public void TestGetTotals()
		{
			DummyLandedCostDistributeTo distribute = Factory.New<DummyLandedCostDistributeTo>();
			DummyIUltimateDistributee ultimate1 = Factory.New<DummyIUltimateDistributee>();
			DummyIUltimateDistributee ultimate2 = Factory.New<DummyIUltimateDistributee>();
			ultimate1.ActualExposed = 1.1m;
			ultimate1.ActualVolumeInM3Exposed = 3.3m;
			ultimate1.ActualWeightInKGExposed = 4.4m;
			ultimate1.CostInLocalCurrencyExposed = 5.5m;
			ultimate1.ItemCountExposed = 6.6m;

			ultimate2.ActualExposed = 1.2m;
			ultimate2.ActualVolumeInM3Exposed = 3.4m;
			ultimate2.ActualWeightInKGExposed = 4.5m;
			ultimate2.CostInLocalCurrencyExposed = 5.6m;
			ultimate2.ItemCountExposed = 6.7m;

			distribute.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1, ultimate2 };
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute };

			LCFactors result = Manager.GetTotals(distribute);
			AssertEquals("Actual", 2.3m, result.Actual);
			AssertEquals("VolumeInM3", 6.7m, result.VolumeInM3);
			AssertEquals("WeightInKG", 8.9m, result.WeightInKG);
			AssertEquals("CostInLocalCurrency", 11.1m, result.CostInLocalCurrency);
			AssertEquals("ItemCount", 13.3m, result.ItemCount);
		}

		public void TestDistributeCostAndWriteToHistory()
		{
			DummyLandedCostDistributeTo distribute = Factory.New<DummyLandedCostDistributeTo>();
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute };

			LandCostInput lCInput = LCHeader.CostInputs.AddNew();
			lCInput.LI_ParentID = distribute.PK;
			lCInput.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			lCInput.LI_CostAmount = 10000m;
			lCInput.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandedCostHistory lCHistory = LCHeader.Histories.AddNew();

			DistributionFactors ratio = new DistributionFactors();
			ratio.Denominator = 10;
			ratio.Numerator = 1;

			lCInput.LI_LandedCostGroup = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
			AssertEquals("Cost in LC History", 1000m, lCHistory.LH_LandedCostGroup1);

			lCInput.LI_LandedCostGroup = 2;
			ratio.Denominator = 2;
			ratio.Numerator = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
			AssertEquals("Cost in LC History", 5000m, lCHistory.LH_LandedCostGroup2);

			lCInput.LI_LandedCostGroup = 3;
			ratio.Denominator = 3;
			ratio.Numerator = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
			AssertEquals("Cost in LC History", 3333.33m, lCHistory.LH_LandedCostGroup3, 0.01m);

			lCInput.LI_LandedCostGroup = 4;
			ratio.Denominator = 4;
			ratio.Numerator = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
			AssertEquals("Cost in LC History", 2500m, lCHistory.LH_LandedCostGroup4);

			lCInput.LI_LandedCostGroup = 5;
			ratio.Denominator = 6;
			ratio.Numerator = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
			AssertEquals("Cost in LC History", 1666.67m, lCHistory.LH_LandedCostGroup5, 0.01m);

			lCInput.LI_LandedCostGroup = 6;
			ratio.Denominator = 8;
			ratio.Numerator = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
			AssertEquals("Cost in LC History", 1250m, lCHistory.LH_LandedCostGroup6);

			lCInput.LI_LandedCostGroup = 7;
			ratio.Denominator = 10;
			ratio.Numerator = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
			AssertEquals("Cost in LC History", 1000m, lCHistory.LH_LandedCostGroupMisc);
		}

		[ExpectNoExceptions()]
		public void TestDistributeCostAndWriteToHistoryWhenDenominatorIsZero()
		{
			DummyLandedCostDistributeTo distribute = Factory.New<DummyLandedCostDistributeTo>();
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute };

			LandCostInput lCInput = LCHeader.CostInputs.AddNew();
			lCInput.LI_ParentID = distribute.PK;
			lCInput.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			LandedCostHistory lCHistory = LCHeader.Histories.AddNew();
			DistributionFactors ratio = new DistributionFactors();
			ratio.Denominator = 0;
			ratio.Numerator = 1;
			lCInput.LI_LandedCostGroup = 1;
			Manager.DistributeCostAndWriteToHistory(lCHistory, lCInput, ratio);
		}

		public void TestRunLandedCostForChargeGroup()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			AssertEquals("No histories", 0, lCHeader.Histories.Count);

			Manager = new LCDistributionManager(lCHeader);
			Manager.RunLandedCostingForChargeGroup(testHelper.Distribute2);

			AssertEquals("Two histories", 2, lCHeader.Histories.Count);
			LandedCostHistory lCHistory1 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate1);
			LandedCostHistory lCHistory2 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate2);
			AssertNotNull("History for Ultimte1", lCHistory1);
			AssertNotNull("History for Ultimte2", lCHistory2);
			AssertEquals("LC Group1 for history1", 0m, lCHistory1.LH_LandedCostGroup1);
			AssertEquals("LC Group1 for history2", 0m, lCHistory1.LH_LandedCostGroup1);
			AssertEquals("LC Group2 for History1", 2500m, lCHistory1.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History2", 2500m, lCHistory2.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History1", 1000m, lCHistory1.LH_LandedCostGroup3);
			AssertEquals("LC Group2 for History2", 9000m, lCHistory2.LH_LandedCostGroup3);
		}

		public void TestRunLandedCostForChargeGroup__IsNoCostApportionmentItem()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			AssertEquals("No histories", 0, lCHeader.Histories.Count);

			Manager = new LCDistributionManager(lCHeader);
			foreach (var history in lCHeader.Histories)
			{
				history.LH_LandedCostGroup1 = 0m;
				history.LH_LandedCostGroup2 = 0m;
				history.LH_LandedCostGroup3 = 0m;
				history.LH_LandedCostGroup4 = 0m;
				history.LH_LandedCostGroup5 = 0m;
				history.LH_LandedCostGroup6 = 0m;
				history.LH_LandedCostGroupMisc = 0m;
			}
			Manager.RunLandedCostingForChargeGroup(testHelper.Distribute2);

			AssertEquals("Two histories", 2, lCHeader.Histories.Count);
			LandedCostHistory lCHistory1 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate1);
			LandedCostHistory lCHistory2 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate2);
			AssertNotNull("History for Ultimte1", lCHistory1);
			AssertNotNull("History for Ultimte2", lCHistory2);
			AssertEquals("LC Group1 for history1", 0m, lCHistory1.LH_LandedCostGroup1);
			AssertEquals("LC Group1 for history2", 0m, lCHistory2.LH_LandedCostGroup1);
			AssertEquals("LC Group2 for History1", 2500m, lCHistory1.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History2", 2500m, lCHistory2.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History1", 1000m, lCHistory1.LH_LandedCostGroup3);
			AssertEquals("LC Group2 for History2", 9000m, lCHistory2.LH_LandedCostGroup3);

			lCHistory1.LH_LandedCostHistoryLineType = LandedCostType.NoCostApportionmentItem;

			foreach (var history in lCHeader.Histories)
			{
				history.LH_LandedCostGroup1 = 0m;
				history.LH_LandedCostGroup2 = 0m;
				history.LH_LandedCostGroup3 = 0m;
				history.LH_LandedCostGroup4 = 0m;
				history.LH_LandedCostGroup5 = 0m;
				history.LH_LandedCostGroup6 = 0m;
				history.LH_LandedCostGroupMisc = 0m;
			}
			Manager.RunLandedCostingForChargeGroup(testHelper.Distribute2);
			CombineAssertions(() =>
			{
				AssertEquals("LC Group1 for history1", 0m, lCHistory1.LH_LandedCostGroup1);
				AssertEquals("LC Group1 for history2", 0m, lCHistory2.LH_LandedCostGroup1);
				AssertEquals("LC Group2 for History1", 0m, lCHistory1.LH_LandedCostGroup2);
				AssertEquals("LC Group2 for History2", 5000m, lCHistory2.LH_LandedCostGroup2);
				AssertEquals("LC Group2 for History1", 0m, lCHistory1.LH_LandedCostGroup3);
				AssertEquals("LC Group2 for History2", 10000m, lCHistory2.LH_LandedCostGroup3);
			});

			lCHistory2.LH_LandedCostHistoryLineType = LandedCostType.NoCostApportionmentItem;

			foreach (var history in lCHeader.Histories)
			{
				history.LH_LandedCostGroup1 = 0m;
				history.LH_LandedCostGroup2 = 0m;
				history.LH_LandedCostGroup3 = 0m;
				history.LH_LandedCostGroup4 = 0m;
				history.LH_LandedCostGroup5 = 0m;
				history.LH_LandedCostGroup6 = 0m;
				history.LH_LandedCostGroupMisc = 0m;
			}
			Manager.RunLandedCostingForChargeGroup(testHelper.Distribute2);
			CombineAssertions(() =>
			{
				AssertEquals("LC Group1 for history1", 0m, lCHistory1.LH_LandedCostGroup1);
				AssertEquals("LC Group1 for history2", 0m, lCHistory2.LH_LandedCostGroup1);
				AssertEquals("LC Group2 for History1", 0m, lCHistory1.LH_LandedCostGroup2);
				AssertEquals("LC Group2 for History2", 0m, lCHistory2.LH_LandedCostGroup2);
				AssertEquals("LC Group2 for History1", 0m, lCHistory1.LH_LandedCostGroup3);
				AssertEquals("LC Group2 for History2", 0m, lCHistory2.LH_LandedCostGroup3);
			});
		}

		public void TestEndToEndTest()
		{
			TestHelper testHelper = new TestHelper(Factory);
			LandedCostHeader lCHeader = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			AssertEquals("No histories", 0, lCHeader.Histories.Count);

			Manager = new LCDistributionManager(lCHeader);
			Manager.RunLandedCosting();

			AssertEquals("Two histories", 2, lCHeader.Histories.Count);
			LandedCostHistory lCHistory1 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate1);
			LandedCostHistory lCHistory2 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate2);
			AssertNotNull("History for Ultimte1", lCHistory1);
			AssertNotNull("History for Ultimte2", lCHistory2);
			AssertEquals("LC Group2 for History1", 2500m, lCHistory1.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History2", 2500m, lCHistory2.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History1", 1000m, lCHistory1.LH_LandedCostGroup3);
			AssertEquals("LC Group2 for History2", 9000m, lCHistory2.LH_LandedCostGroup3);

			Manager.RunLandedCosting(); //running again
			AssertEquals("Two histories", 2, lCHeader.Histories.Count);
			lCHistory1 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate1);
			lCHistory2 = lCHeader.Histories.GetOrCreate(testHelper.Ultimate2);
			AssertNotNull("History for Ultimte1", lCHistory1);
			AssertNotNull("History for Ultimte2", lCHistory2);
			AssertEquals("LC Group2 for History1", 2500m, lCHistory1.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History2", 2500m, lCHistory2.LH_LandedCostGroup2);
			AssertEquals("LC Group2 for History1", 1000m, lCHistory1.LH_LandedCostGroup3);
			AssertEquals("LC Group2 for History2", 9000m, lCHistory2.LH_LandedCostGroup3);
		}

		public void TestDistributeCostAndRunBackRoundingForUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var ultimate1 = Factory.New<DummyIUltimateDistributee>();
				ultimate1.PKExposed = ZGuid.NewZGuid();
				ultimate1.CostInLocalCurrencyExposed = 9651.39m;
				ultimate1.TableCodeExposed = DummyBizoSchema.Constants.Prefix;

				var ultimate2 = Factory.New<DummyIUltimateDistributee>();
				ultimate2.PKExposed = ZGuid.NewZGuid();
				ultimate2.CostInLocalCurrencyExposed = 15034.38m;
				ultimate2.TableCodeExposed = DummyBizoSchema.Constants.Prefix;

				var ultimate3 = Factory.New<DummyIUltimateDistributee>();
				ultimate3.PKExposed = ZGuid.NewZGuid();
				ultimate3.CostInLocalCurrencyExposed = 100m;
				ultimate3.TableCodeExposed = DummyBizoSchema.Constants.Prefix;

				var distribute = Factory.New<DummyLandedCostDistributeTo>();
				distribute.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1, ultimate2, ultimate3 };
				distribute.UniqueCodeExposed = "DB";

				var dummyHeader = Factory.New<DummyLandedCostHeader>();
				dummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute };
				dummyHeader.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
				dummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1, ultimate2, ultimate3 };
				dummyHeader.ChargeHoldersExposed = Array.Empty<ILandedCostChargeHolder>();

				var costHeader = Factory.New<LandedCostHeader>();
				costHeader.LT_ParentID = dummyHeader.PK;
				costHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

				var costInput = costHeader.CostInputs.AddNew();
				costInput = costHeader.CostInputs.AddNew();
				costInput.LinkedObjectUniqueCode = distribute.UniqueCode;
				costInput.LI_ParentID = distribute.PK;
				costInput.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
				costInput.LI_CostAmount = 142.48m;
				costInput.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				costInput.LI_LandedCostGroup = 3;
				costInput.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;

				var manager = new LCDistributionManager(costHeader);
				manager.RunLandedCosting();
				AssertEquals(3, costHeader.Histories.Count);
				AssertEquals(142.48m, costHeader.Histories.Sum(x => x.RoundedGroup3));
				AssertEquals(142.48m, costHeader.Histories.Sum(x => x.LH_LandedCostGroup3));
			}
		}

		DummyLandedCostHeader DummyHeader;
		LandedCostHeader LCHeader;
		LCDistributionManager Manager;

		protected override void SetUp()
		{
			base.SetUp();
			DummyHeader = Factory.New<DummyLandedCostHeader>();
			LCHeader = Factory.New<LandedCostHeader>();
			LCHeader.LT_ParentID = DummyHeader.PK;
			LCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			Manager = new LCDistributionManager(LCHeader);
		}
	}
}

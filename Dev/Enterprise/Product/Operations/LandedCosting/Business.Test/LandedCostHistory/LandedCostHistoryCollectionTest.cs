using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostHistoryCollection))]
	sealed class LandedCostHistoryCollectionTest : ActiveBusinessObjectCollectionTestCase<LandedCostHistoryCollection>
	{
		public void TestCompareInvalidElements()
		{
			DummyIUltimateDistributee dummyDistributee1 = Factory.New<DummyIUltimateDistributee>();
			DummyIUltimateDistributee dummyDistributee2 = Factory.New<DummyIUltimateDistributee>();

			LandedCostHistoryCollection collection = new LandedCostHistoryCollection(Header);
			LandedCostHistory lCLine1 = collection.AddNew();
			lCLine1.UltimateDistributee = dummyDistributee1;
			lCLine1.LH_LandedCostHistoryLineType = "B";
			lCLine1.LH_LandedCostGroup1 = 10m;

			LandedCostHistory lCLine2 = collection.AddNew();
			lCLine2.UltimateDistributee = dummyDistributee2;
			lCLine2.LH_LandedCostHistoryLineType = "B";
			lCLine2.LH_LandedCostGroup1 = 10m;

			dummyDistributee1.HumanReadableCodeInfosExposed = new ZPropertyInfo[] { lCLine1.LH_LandedCostHistoryLineTypeInfo, lCLine1.LH_LandedCostGroup1InfoForTesting };
			dummyDistributee2.HumanReadableCodeInfosExposed = new ZPropertyInfo[] { lCLine2.LH_LandedCostGroup1InfoForTesting, lCLine2.LH_LandedCostHistoryLineTypeInfo };

			collection.ApplySort(LandedCostHistory.Schema.HumanReadableUltimateDistributeeCode, ListSortDirection.Ascending);

			AssertExceptionThrown("expect invalid operation exception", typeof(InvalidOperationException), () => collection[0].Delete());
			AssertEquals("should have reported a developer error", 1, ErrorReporter.TotalErrorCount);
			AssertContains("reported developer error should contain first compared element type", lCLine1.LH_LandedCostHistoryLineType.GetType().FullName, ErrorReporter.LastMessageReported);
			AssertContains("reported developer error should contain second compared element type", lCLine2.LH_LandedCostGroup1.GetType().FullName, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSortLCLines()
		{
			DummyIUltimateDistributee dummyDistributee1 = Factory.New<DummyIUltimateDistributee>();
			DummyIUltimateDistributee dummyDistributee2 = Factory.New<DummyIUltimateDistributee>();
			DummyIUltimateDistributee dummyDistributee3 = Factory.New<DummyIUltimateDistributee>();

			LandedCostHistoryCollection collection = new LandedCostHistoryCollection(Header);
			LandedCostHistory lCLine1 = collection.AddNew();
			lCLine1.UltimateDistributee = dummyDistributee1;
			lCLine1.LH_LandedCostHistoryLineType = "B";
			lCLine1.LH_LandedCostGroup1 = 10m;
			lCLine1.LH_LandedCostGroup2 = 9m;

			LandedCostHistory lCLine2 = collection.AddNew();
			lCLine2.UltimateDistributee = dummyDistributee2;
			lCLine2.LH_LandedCostHistoryLineType = "B";
			lCLine2.LH_LandedCostGroup1 = 15m;
			lCLine2.LH_LandedCostGroup2 = 8m;

			LandedCostHistory lCLine3 = collection.AddNew();
			lCLine3.UltimateDistributee = dummyDistributee3;
			lCLine3.LH_LandedCostHistoryLineType = "A";
			lCLine3.LH_LandedCostGroup1 = 10m;
			lCLine3.LH_LandedCostGroup2 = 7m;

			dummyDistributee1.HumanReadableCodeInfosExposed = new ZPropertyInfo[] { lCLine1.LH_LandedCostHistoryLineTypeInfo, lCLine1.LH_LandedCostGroup1InfoForTesting };
			dummyDistributee2.HumanReadableCodeInfosExposed = new ZPropertyInfo[] { lCLine2.LH_LandedCostHistoryLineTypeInfo, lCLine2.LH_LandedCostGroup1InfoForTesting };
			dummyDistributee3.HumanReadableCodeInfosExposed = new ZPropertyInfo[] { lCLine3.LH_LandedCostHistoryLineTypeInfo, lCLine3.LH_LandedCostGroup1InfoForTesting };

			//Sorted on HumanReadableCode which here is hooked up LH_LandedCostHistoryLineTypeInfo and then LH_LandedCostGroup1Info in an Ascending order
			collection.ApplySort(LandedCostHistory.Schema.HumanReadableUltimateDistributeeCode, ListSortDirection.Ascending);
			AssertEquals("First After Ascending Sort on HumanReadableCode", lCLine3, collection[0]);
			AssertEquals("Second After Ascending Sort on HumanReadableCode", lCLine1, collection[1]);
			AssertEquals("Third After Ascending Sort on HumanReadableCode", lCLine2, collection[2]);

			//Sorted on HumanReadableCode which here is hooked up LH_LandedCostHistoryLineTypeInfo and then LH_LandedCostGroup1Info in a descending order
			collection.ApplySort(LandedCostHistory.Schema.HumanReadableUltimateDistributeeCode, ListSortDirection.Descending);
			AssertEquals("First After Descending Sort on HumanReadableCode", lCLine2, collection[0]);
			AssertEquals("Second After Descending Sort on HumanReadableCode", lCLine1, collection[1]);
			AssertEquals("Third After Descending Sort on HumanReadableCode", lCLine3, collection[2]);

			collection.ApplySort("LH_LandedCostGroup2", ListSortDirection.Ascending);
			AssertEquals("First After Ascending Sort on other property", lCLine3, collection[0]);
			AssertEquals("Second After Ascending Sort on other property", lCLine2, collection[1]);
			AssertEquals("Third After Ascending Sort on other property", lCLine1, collection[2]);

			collection.ApplySort("LH_LandedCostGroup2", ListSortDirection.Descending);
			AssertEquals("First After Descending Sort on other property", lCLine1, collection[0]);
			AssertEquals("Second After Descending Sort on other property", lCLine2, collection[1]);
			AssertEquals("Third After Descending Sort on other property", lCLine3, collection[2]);
		}

		public void TestDefaultValuesForNewChild()
		{
			Header.LT_LandedCostType = "ACT";
			LandedCostHistoryCollection collection = new LandedCostHistoryCollection(Header);
			LandedCostHistory lCHistory = collection.AddNew();
			AssertEquals("LC Type set", Header.LT_LandedCostType, lCHistory.LH_LandedCostHistoryLineType);
		}

		public void TestLoadUsingForeignKey()
		{
			LandedCostHistoryCollection collection = new LandedCostHistoryCollection(Header);
			AssertEquals("No items yet", 0, collection.Count);

			LandedCostHistory history1 = Factory.New<LandedCostHistory>();
			history1.LH_LT = Header.PK;
			AssertEquals("one item", 1, collection.Count);

			LandedCostHistory history2 = Factory.New<LandedCostHistory>();
			history2.LH_LT = Header.PK;
			AssertEquals("Two items", 2, collection.Count);
		}

		public void TestGetOrCreate()
		{
			LandedCostHistoryCollection collection = new LandedCostHistoryCollection(Header);

			DummyIUltimateDistributee distributee = Factory.New<DummyIUltimateDistributee>();
			distributee.PKExposed = ZGuid.NewZGuid();
			distributee.TableCodeExposed = "Z0";
			distributee.FKToProductExposed = ZGuid.NewZGuid();

			LandedCostHistory lCHistory = collection.GetOrCreate(distributee);
			AssertNotNull("History for Distributee", lCHistory);
			AssertEquals("Foreign Key is set", distributee.PK, lCHistory.LH_ParentID);
			AssertEquals("TableCode is set", "Z0", lCHistory.LH_ParentTableCode);
			AssertEquals("OP is set", distributee.FKToProduct, lCHistory.LH_OP);

			LandedCostHistory lCHistory2 = collection.GetOrCreate(distributee);
			AssertEquals("Existing Item", lCHistory, lCHistory2);
		}

		public void TestTotals()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			helper.Ultimate1.CostInLocalCurrencyExposed = 100m;
			helper.Ultimate2.CostInLocalCurrencyExposed = 200m;

			LandedCostHistory lCHistory1 = lCHeader.Histories.AddNew();
			lCHistory1.UltimateDistributee = helper.Ultimate1;

			LandedCostHistory lCHistory2 = lCHeader.Histories.AddNew();
			lCHistory2.UltimateDistributee = helper.Ultimate2;

			AssertEquals("Total Invoice cost", 300m, lCHeader.Histories.TotalInvoiceCost);
			AssertEquals("Total Cost", 300m, lCHeader.Histories.TotalCost);
		}

		public void TestTotalCostWithMarkupApplied()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			helper.Ultimate1.CostInLocalCurrencyExposed = 4000m;
			helper.Ultimate2.CostInLocalCurrencyExposed = 3000m;

			LandedCostHistory lCHistory1 = lCHeader.Histories.AddNew();
			lCHistory1.UltimateDistributee = helper.Ultimate1;
			lCHistory1.LH_LandedCostMarginPercent1 = 5.00;

			LandedCostHistory lCHistory2 = lCHeader.Histories.AddNew();
			lCHistory2.UltimateDistributee = helper.Ultimate2;
			lCHistory2.LH_LandedCostMarginPercent1 = 10.00;

			AssertEquals("TotalCost", 7000m, lCHeader.Histories.TotalCost);
			AssertEquals("TotalCostWithMarkup1Applied", 7500m, lCHeader.Histories.TotalCostWithMarkup1Applied);
		}

		protected override LandedCostHistoryCollection GetCollectionToTest() => new LandedCostHistoryCollection(Header);

		LandedCostHeader header;
		LandedCostHeader Header => header ?? (header = Factory.New<LandedCostHeader>());
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsAdjustmentBuilderTest : WhsDocketCollectionBuilderTestCase<WhsAdjustment, LineData>
	{
		#region Finalise All Dockets

		public override void TestFinaliseAllDockets()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("2", "B", 2, 1);
			var org1 = Helper.CreateClient("1");
			var prod1 = Helper.CreateProduct(org1, "P1");
			Factory.Save();

			var line1 = Builder.AddLine(whs1, org1, "A", "", prod1, 10m, "A-1", "C1", "DAM");
			var line2 = Builder.AddLine(whs2, org1, "B", "", prod1, 20m, "B-1", "C3", "DAM");
			line1.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			line2.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;

			foreach (var docket in Builder.Dockets)
			{
				ProcessDocketBeforeTestFinaliseAllDockets(docket);
			}

			Factory.Save(); // need to save as finalise uses a 2nd factory
			Builder.FinaliseAllDockets();

			var dockets = Builder.Dockets.ToArray();
			AssertEquals(2, dockets.Length);
			AssertEquals(true, dockets[0].IsFinalised);
			AssertEquals(true, dockets[1].IsFinalised);
		}

		#endregion

		#region TestAddingNewLine

		[TestDate(2012, 03, 26)]
		public void TestAddingNewLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("2", "A");
			var expiryDate = ZDate.Today.AddDays(2);
			var packingDate = ZDate.Today.AddDays(-2);

			var lineWithoutProductAttributes = Builder.AddLine(data.Whs1, data.Org1, "A", "", data.Part1, 10m, "A", "C1", "C2");
			var lineWithProductAttributes = Builder.AddLine(whs2, data.Org1, "D", "", data.Part1, -10m, "B", "C4", "C5", "E1", expiryDate, packingDate, "PA1", "PA2", "PA3", "");

			AssertEquals(2, Builder.Dockets.Count);

			var adjustment = Builder.Dockets.FindDocket(data.Whs1, data.Org1);
			AssertEquals(false, adjustment.IsFinalised);
			AssertEquals("A", adjustment.WD_ExternalReference);
			AssertEquals(1, adjustment.Lines.Count);
			AssertDocketLine(adjustment.Lines[0], data.Part1, 10m, "A", "C1", "C2", "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			AssertEquals(InventoryStatus.Codes.Available, adjustment.Lines[0].WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Available, adjustment.Lines[0].WE_CurrentInventoryStatus);

			adjustment = Builder.Dockets.FindDocket(whs2, data.Org1);
			AssertEquals(false, adjustment.IsFinalised);
			AssertEquals("D", adjustment.WD_ExternalReference);
			AssertEquals(1, adjustment.Lines.Count);
			AssertDocketLine(adjustment.Lines[0], data.Part1, -10m, "B", "C4", "C5", "E1", expiryDate, packingDate, "PA1", "PA2", "PA3", "");
			AssertEquals(InventoryStatus.Codes.Available, adjustment.Lines[0].WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Available, adjustment.Lines[0].WE_CurrentInventoryStatus);
		}

		#endregion

		#region Implementation

		protected override string SetTestAddLineDocketSubType => AdjustmentType.Codes.Adjustment;

		protected override WhsDocketCollectionBuilder<WhsAdjustment, LineData> GetNewDocketBuilder(BusinessObjectFactory factory)
		{
			return new WhsAdjustmentCollectionBuilder(factory);
		}

		protected override WhsAdjustment GetNewDocket()
		{
			return Factory.New<WhsAdjustment>();
		}

		#endregion
	}
}

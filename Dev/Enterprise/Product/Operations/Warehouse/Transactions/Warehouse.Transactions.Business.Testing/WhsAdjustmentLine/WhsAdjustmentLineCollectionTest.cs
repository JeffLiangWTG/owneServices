using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentLineCollection))]
	class WhsAdjustmentLineCollectionTest : WhsDocketLineCollectionTestCase<WhsAdjustmentLineCollection>
	{
		#region TestSetDefaultsForNewChild

		public override void TestSetDefaultsForNewChild()
		{
			base.TestSetDefaultsForNewChild();

			var line = Factory.New<WhsAdjustmentLine>();
			AssertEquals("No docket so location should be empty", ZGuid.Empty, line.WE_WL);

			var adjustment = Factory.New<WhsAdjustment>();
			line = adjustment.Lines.AddNew();
			AssertEquals("No warehouse set so location should be empty", ZGuid.Empty, line.WE_WL);
			AssertEquals("No reason should be added by default since Adjustment type is not ownership type.", "", line.WE_ReasonCode);

			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 1, 1);
			adjustment.WD_WW_Whs = whs.PK;

			line = adjustment.Lines.AddNew();
			AssertEquals("Not a bonded warehouse so location should be empty", ZGuid.Empty, line.WE_WL);

			whs.WW_IsVirtualWarehouse = true;
			line = adjustment.Lines.AddNew();
			AssertEquals("Location should be warehouse default location", whs.DefaultLocation.PK, line.WE_WL);

			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			line = adjustment.Lines.AddNew();
			AssertEquals("Reason code should be OCH since docket sub type is New Ownership Adjustment.", "OCH", line.WE_ReasonCode);
		}

		#endregion

		#region TestSetDfaultsForNewChild_VirtualWarehouse

		public void TestSetDefaultsForNewChild_VirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			warehouse.WW_IsVirtualWarehouse = true;

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, warehouse);
			var adjustmentLine = adjustment.Lines.AddNew();
			AssertEquals("Location should NOT be empty.", false, adjustmentLine.WE_WL.IsEmpty);
			AssertEquals("Location should be warehouse default location", warehouse.DefaultLocation.PK, adjustmentLine.WE_WL);
		}

		#endregion

		#region TestAllowNew_Adjustment

		public void TestAllowNew_Adjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var childAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var parentAdjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			childAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			parentAdjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			childAdjustment.WD_WD_ParentDocket = parentAdjustment.PK;
			AssertEquals(true, ((IBindingList)adjustment.Lines).AllowNew);
			AssertEquals(false, ((IBindingList)childAdjustment.Lines).AllowNew);
			AssertEquals(true, ((IBindingList)parentAdjustment.Lines).AllowNew);

			adjustment.WD_DocketStatus = DocketStatus.Codes.Finalised;
			childAdjustment.WD_DocketStatus = DocketStatus.Codes.Finalised;
			parentAdjustment.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals(false, ((IBindingList)adjustment.Lines).AllowNew);
			AssertEquals(false, ((IBindingList)childAdjustment.Lines).AllowNew);
			AssertEquals(false, ((IBindingList)parentAdjustment.Lines).AllowNew);
		}

		#endregion

		#region TestLocationStringSortedProperly

		protected override void TestLocationSortedProperlyCore(string locationPropertyToCompare)
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-10");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 30m, location3, "");
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -3m, location1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 4m, location2);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 5m, location3);

			// Check sort ascending
			adjustment.Lines.ApplySort(locationPropertyToCompare, ListSortDirection.Ascending);
			AssertEquals("A-1", adjustment.Lines[0].LocationString);
			AssertEquals("A-2", adjustment.Lines[1].LocationString);
			AssertEquals("A-10", adjustment.Lines[2].LocationString);

			// Check sort descending
			adjustment.Lines.ApplySort(locationPropertyToCompare, ListSortDirection.Descending);
			AssertEquals("A-10", adjustment.Lines[0].LocationString);
			AssertEquals("A-2", adjustment.Lines[1].LocationString);
			AssertEquals("A-1", adjustment.Lines[2].LocationString);
		}

		protected override void TestTransferFromLocationStringSortedProperlyCore(string locationPropertyToCompare)
		{
			// adjustment lines do not have transfer from location
			Assert(true);
		}

		#endregion

		#region GetCollectionToTest

		protected override WhsAdjustmentLineCollection GetCollectionToTest()
		{
			return new WhsAdjustmentLineCollection(Factory.New<WhsAdjustment>());
		}

		#endregion
	}
}

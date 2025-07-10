using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickAvailableInventorySplitByUOM))]
	class WhsPickAvailableInventorySplitByUOMTest : WhsPickAvailableInventorySplitBaseTest<WhsPickAvailableInventorySplitByUOM>
	{
		#region TestSetDataAndCheckProperties

		public override void TestSetDataAndCheckProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "KEG";
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(1, pick.OrderedInventories[0].AvailableInventories.Count);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals(2, availableInventory.PickLines.Count()); // because of 2 inventories

			var inventorySplit = CreateAvailableInventory();
			AssertExceptionThrown<ArgumentNullException>(() => inventorySplit.SetData("O1", 3m, "CAS", UOMPackTypesList.Codes.Pallet, availableInventory.PickLines.ToPickLinePairs(), null));
			AssertExceptionThrown<ArgumentNullException>(() => inventorySplit.SetData("O1", 3m, "CAS", UOMPackTypesList.Codes.Pallet, null, availableInventory));

			inventorySplit.SetData("O1", 3m, "CAS", UOMPackTypesList.Codes.Pallet, availableInventory.PickLines.ToPickLinePairs(), availableInventory);
			AssertEquals("O1", inventorySplit.OrderReference);
			AssertEquals(3m, inventorySplit.PackQuantity);
			AssertEquals("CAS", inventorySplit.PackQuantityUQ);
			AssertEquals(12m, inventorySplit.StockUnitQuantity);
			AssertEquals("KEG", inventorySplit.StockKeepingUnit);
			AssertEquals("PLT", inventorySplit.UOMType);
			AssertExceptionThrown<InvalidOperationException>(() => inventorySplit.SetData("O1", 3m, "CAS", UOMPackTypesList.Codes.Pallet, availableInventory.PickLines.ToPickLinePairs(), availableInventory));
		}

		#endregion

		#region TestPickedDate

		public override void TestPickedDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 85m);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			AssertNotNull(pick.OrderedInventories[0]);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull(availableInventory);

			availableInventory.PickLineQuantity = 85;
			AssertEquals(85m, availableInventory.PickLineQuantity);

			AssertEquals(5, availableInventory.PickLines.Count());

			AssertEquals(2, availableInventory.AvailableInventoriesSplitByUOM.Count); // 8 CAS and 5 UNT
			var uomSplits = availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>().ToArray();
			var uomInventory1 = uomSplits.Single(i => i.PackQuantityUQ == "CAS");
			AssertEquals("CAS", uomInventory1.PackQuantityUQ);
			AssertEquals(8m, uomInventory1.PackQuantity);
			AssertEquals(2, availableInventory.PickLines.Count(pl => pl.WZ_F3_NKAllocatedPackType == "CAS"));

			var uomInventory2 = uomSplits.Single(i => i.PackQuantityUQ == "UNT");
			AssertEquals("UNT", uomInventory2.PackQuantityUQ);
			AssertEquals(5m, uomInventory2.PackQuantity);
			AssertEquals(3, availableInventory.PickLines.Count(pl => pl.WZ_F3_NKAllocatedPackType == "UNT"));

			foreach (var pickLine in availableInventory.PickLines)
			{
				AssertEquals(ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
			}

			var testDate = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now);
			uomInventory1.PickedDate = testDate;
			foreach (var pickLine in availableInventory.PickLines)
			{
				if (pickLine.WZ_F3_NKAllocatedPackType == uomInventory1.PackQuantityUQ)
				{
					AssertEquals("PickLines related to this uomInventory should have WZ_PickedDateTime changed", testDate, pickLine.WZ_PickedDateTime);
				}
				else
				{
					AssertEquals(ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
				}
			}

			uomInventory1.PickedDate = ZDateTimeOffset.Empty;
			foreach (var pickLine in availableInventory.PickLines)
			{
				AssertEquals(ZDateTimeOffset.Empty, pickLine.WZ_PickedDateTime);
			}
		}

		#endregion

		#region TestPickedDate_SuspendsPickPercentageRecalculation

		protected override void AddSetupToTestPickedDate_SuspendsPickPercentageRecalculation(TestDataSimpleEnvironment data)
		{
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Whs1.WW_IsPickByUOMEnabled = true;
		}

		#endregion

		#region TestAssignedToPK

		public override void TestAssignedToPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 85m);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			AssertNotNull(pick.OrderedInventories[0]);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull(availableInventory);

			availableInventory.PickLineQuantity = 85;
			AssertEquals(85m, availableInventory.PickLineQuantity);

			AssertEquals(5, availableInventory.PickLines.Count());

			AssertEquals(2, availableInventory.AvailableInventoriesSplitByUOM.Count); // 8 CAS and 5 UNT
			var uomSplits = availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>().ToArray();
			var uomInventory1 = uomSplits.Single(i => i.PackQuantityUQ == "CAS");
			AssertEquals(8m, uomInventory1.PackQuantity);
			AssertEquals(2, availableInventory.PickLines.Count(pl => pl.WZ_F3_NKAllocatedPackType == "CAS"));

			var uomInventory2 = uomSplits.Single(i => i.PackQuantityUQ == "UNT");
			AssertEquals(5m, uomInventory2.PackQuantity);
			AssertEquals(3, availableInventory.PickLines.Count(pl => pl.WZ_F3_NKAllocatedPackType == "UNT"));

			foreach (var pickLine in availableInventory.PickLines)
			{
				AssertEquals(ZString.Empty, pickLine.WZ_GS_NKAssignedTo);
			}

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";

			uomInventory1.AssignedToPK = staff.PK;
			foreach (var pickLine in availableInventory.PickLines)
			{
				if (pickLine.WZ_F3_NKAllocatedPackType == uomInventory1.PackQuantityUQ)
				{
					AssertEquals("PickLines related to this uomInventory should have WZ_PickedDateTime changed", "TST", pickLine.WZ_GS_NKAssignedTo);
				}
				else
				{
					AssertEquals(ZString.Empty, pickLine.WZ_GS_NKAssignedTo);
				}
			}

			uomInventory1.AssignedToPK = ZGuid.Empty;
			foreach (var pickLine in availableInventory.PickLines)
			{
				AssertEquals(ZString.Empty, pickLine.WZ_GS_NKAssignedTo);
			}
		}

		#endregion

		#region TestValidation

		protected override Type ExpectedValidationType => typeof(WhsPickAvailableInventorySplitByUOMValidation);

		#endregion

		#region Implementation

		protected override WhsPickAvailableInventorySplitByUOM CreateAvailableInventory()
		{
			return new WhsPickAvailableInventorySplitByUOM(Factory);
		}

		protected override WhsPickAvailableInventorySplitByUOM CreateAvailableInventoryWithPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, "CAS", 10);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertNotNull("Precondition", pick.OrderedInventories[0]);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			AssertNotNull("Precondition", availableInventory);
			AssertEquals("Precondition", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Precondition", 1, availableInventory.PickLines.Count());
			AssertEquals("Precondition", 1, availableInventory.AvailableInventoriesSplitByUOM.Count);
			return availableInventory.AvailableInventoriesSplitByUOM.Cast<WhsPickAvailableInventorySplitByUOM>().Single();
		}

		protected override WhsPickAvailableInventorySplitBaseCollection<WhsPickAvailableInventorySplitByUOM> GetCollectionFromAvailableInventory(WhsPickAvailableInventory availableInventory)
		{
			return availableInventory.AvailableInventoriesSplitByUOM;
		}

		protected override bool EnableUOM => true;

		#endregion
	}
}

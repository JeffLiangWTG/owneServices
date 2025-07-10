using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsRMAOrderLine))]
	public class WhsRMAOrderLineTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region TestGetWhsRMAOrderLine_WhsOrderLineIsNull

		public void TestGetWhsRMAOrderLine_WhsOrderLineIsNull()
		{
			AssertExceptionThrown("No OrderLine passed in, should throw exception", typeof(ArgumentNullException), () => WhsRMAOrderLine.GetWhsRMAOrderLine(null, null, 10m, 10m));
		}

		#endregion

		#region TestProperties

		[TestDate(2019, 1, 1)]
		public void TestProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var year = ZDateTime.Now.Year;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, new ZDate(year, 1, 2), new ZDate(year, 1, 2), "ATTR1", "ATTR2", "ATTR3", "");
			inventory.WI_SerialNumber = "SERNUM";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m, new ZDate(year, 1, 2), new ZDate(year, 1, 2), "ATTR1", "ATTR2", "ATTR3", "", "");
			orderLine.WE_SerialNumber = "SERNUM";
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			AssertEquals("Precondition: only 1m units released.", 1m, orderLine.SumOfUnitsMet);

			var rmaOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 2m, 1m);

			AssertNotNull(rmaOrderLine.Product);
			AssertEquals("P1", rmaOrderLine.Product.OP_PartNum);
			AssertEquals(2m, rmaOrderLine.Quantity);
			AssertEquals(1m, rmaOrderLine.AvailableQtyToReturn);
			AssertEquals(0m, rmaOrderLine.QuantityToReturn);
			AssertEquals(new ZDate(year, 1, 2), rmaOrderLine.ExpiryDate);
			AssertEquals(new ZDate(year, 1, 2), rmaOrderLine.PackingDate);
			AssertEquals("ATTR1", rmaOrderLine.PartAttrib1);
			AssertEquals("ATTR2", rmaOrderLine.PartAttrib2);
			AssertEquals("ATTR3", rmaOrderLine.PartAttrib3);
			AssertEquals("SERNUM", rmaOrderLine.SerialNumber);
			AssertEquals("Original Warehouse is set", data.Whs1.PK, rmaOrderLine.OriginalWarehousePK);
			AssertEquals("Warehouse Override is not set", ZGuid.Empty, rmaOrderLine.WhsOverride);
		}

		#endregion

		#region TestWhsOverride_Lookups

		public void TestWhsOverride_Lookups()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsRMAOrderLine), nameof(WhsRMAOrderLine.WhsOverride), false, a => a.ListDataSourceMember == "Lookups.Warehouses");
		}

		#endregion

		#region TestLoadParentInventory

		public void TestLoadParentInventory()
		{
			var adjustmentLine = Factory.New<WhsAdjustmentLine>();
			var transferLine = Factory.New<WhsTransferLine>();
			var receiveLine = Factory.New<WhsReceiveLine>();
			var order = Factory.New<WhsOrder>();

			var whsRMAOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(adjustmentLine, order, 10m, 10m);
			AssertNotNull("WhsAdjustmentLine can be loaded from PK.", whsRMAOrderLine.ParentInventory);
			AssertEquals(adjustmentLine.PK, whsRMAOrderLine.ParentInventory.PK);

			whsRMAOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(transferLine, order, 10m, 10m);
			AssertNotNull("WhsTransferLine can be loaded from PK.", whsRMAOrderLine.ParentInventory);
			AssertEquals(transferLine.PK, whsRMAOrderLine.ParentInventory.PK);

			whsRMAOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receiveLine, order, 10m, 10m);
			AssertNotNull("WhsReceiveLine can be loaded from PK.", whsRMAOrderLine.ParentInventory);
			AssertEquals(receiveLine.PK, whsRMAOrderLine.ParentInventory.PK);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			return WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10m, 10m);
		}

		#endregion
	}
}

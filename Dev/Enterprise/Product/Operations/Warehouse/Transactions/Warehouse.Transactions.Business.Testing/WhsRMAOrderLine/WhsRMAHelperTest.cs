using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	//The logic of method GetWhsRMAOrderLines has been further tested under WhsRMAOrderTest and GenerateRMAByOrdersActionMethodApplicatorTest
	public class WhsRMAHelperTest : WhsTestCaseWithFactory
	{
		public void TestGetWhsRMAOrderLines_NullParameters()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WhsRMAHelper.GetWhsRMAOrderLines(null, Factory.NewWithValidTestData<WhsOrder>()));
			AssertExceptionThrown<ArgumentNullException>(() => WhsRMAHelper.GetWhsRMAOrderLines(Enumerable.Empty<WhsOrderLine>(), null));
		}

		public void TestGetWhsRMAOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var inventory = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(inventory, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(inventory, data.Part2, 20m);
			inventory.AllocateLocationsWithMock();
			inventory.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var rmaOrderLines = WhsRMAHelper.GetWhsRMAOrderLines(new List<WhsOrderLine>() { orderLine1, orderLine2 }, order);
			AssertEquals(2, rmaOrderLines.Count);

			AssertRMAOrderLine(rmaOrderLines.Single(line => line.ProductPK == orderLine1.WE_OP),
				productPK: orderLine1.WE_OP,
				externalReference: "O1",
				qty: 5m,
				availableQty: 5m,
				packingDate: ZDate.Empty,
				expiryDate: ZDate.Empty,
				partAttrib1: string.Empty,
				partAttrib2: string.Empty,
				partAttrib3: string.Empty,
				serialNumber: string.Empty,
				key: WhsRMAHelper.GetKey(receiveLine1),
				shouldCopyBOMLinks: false);

			AssertRMAOrderLine(rmaOrderLines.Single(line => line.ProductPK == orderLine2.WE_OP),
				productPK: orderLine2.WE_OP,
				externalReference: "O1",
				qty: 15m,
				availableQty: 15m,
				packingDate: ZDate.Empty,
				ZDate.Empty,
				partAttrib1: string.Empty,
				partAttrib2: string.Empty,
				partAttrib3: string.Empty,
				serialNumber: string.Empty,
				key: WhsRMAHelper.GetKey(receiveLine2),
				shouldCopyBOMLinks: false);
		}

		public void TestGetWhsRMAOrderLines_WithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Factory.Save();

			var inventory = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today;
			var receiveLine = Helper.CreateWhsReceiveLine(inventory, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "PLT-1", expiryDate, packingDate, "A", "B", "C", "S1", "");
			inventory.AllocateLocationsWithMock();
			inventory.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var rmaOrderLines = WhsRMAHelper.GetWhsRMAOrderLines(new List<WhsOrderLine>() { orderLine }, order);
			AssertEquals(1, rmaOrderLines.Count);

			AssertRMAOrderLine(rmaOrderLines.Single(),
				productPK: orderLine.WE_OP,
				externalReference: "Order123",
				qty: 1m,
				availableQty: 1m,
				packingDate: packingDate,
				expiryDate: expiryDate,
				partAttrib1: "A",
				partAttrib2: "B",
				partAttrib3: "C",
				serialNumber: "S1",
				key: WhsRMAHelper.GetKey(receiveLine),
				shouldCopyBOMLinks: false);
		}

		public void TestGetWhsRMAOrderLines_WithAttributes_MultipleOriginalInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Factory.Save();

			var inventory1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(inventory1, data.Part1.PK, 8m, data.Whs1.DefaultLocation.PK, "PLT-1", ZDate.Empty, ZDate.Empty, "", "Blue", "Long", "", "");
			inventory1.AllocateLocationsWithMock();
			inventory1.FinaliseDocketWithoutUserConfirmation();

			var inventory2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(inventory2, data.Part1.PK, 12m, data.Whs1.DefaultLocation.PK, "PLT-2", ZDate.Empty, ZDate.Empty, "", "Red", "Short", "", "");
			inventory2.AllocateLocationsWithMock();
			inventory2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var rmaOrderLines = WhsRMAHelper.GetWhsRMAOrderLines(new List<WhsOrderLine>() { orderLine }, order);
			AssertEquals(2, rmaOrderLines.Count);

			AssertRMAOrderLine(rmaOrderLines.Single(line => line.Quantity == 8m),
				productPK: orderLine.WE_OP,
				externalReference: "Order123",
				qty: 8m,
				availableQty: 8m,
				packingDate: ZDate.Empty,
				expiryDate: ZDate.Empty,
				partAttrib1: "",
				partAttrib2: "Blue",
				partAttrib3: "Long",
				serialNumber: "",
				key: WhsRMAHelper.GetKey(receiveLine1),
				shouldCopyBOMLinks: false);

			AssertRMAOrderLine(rmaOrderLines.Single(line => line.Quantity == 12m),
				productPK: orderLine.WE_OP,
				externalReference: "Order123",
				qty: 12m,
				availableQty: 12m,
				packingDate: ZDate.Empty,
				expiryDate: ZDate.Empty,
				partAttrib1: "",
				partAttrib2: "Red",
				partAttrib3: "Short",
				serialNumber: "",
				key: WhsRMAHelper.GetKey(receiveLine2),
				shouldCopyBOMLinks: false);
		}

		public void TestGetWhsRMAOrderLines_WithExistingReturnReceive()
			=> TestGetWhsRMAOrderLines_WithExistingReturnReceiveCore(excludeExistingReceive: false);

		public void TestGetWhsRMAOrderLines_WithExistingReturnReceive_ExcludeExistingReceive()
			=> TestGetWhsRMAOrderLines_WithExistingReturnReceiveCore(excludeExistingReceive: true);

		void TestGetWhsRMAOrderLines_WithExistingReturnReceiveCore(bool excludeExistingReceive)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var inventory = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(inventory, data.Part1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(inventory, data.Part2, 20m);
			inventory.AllocateLocationsWithMock();
			inventory.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "O1");
			returnReceive.WD_WD_ParentDocket = order.PK;
			Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 5m);
			Helper.CreateWhsReceiveLine(returnReceive, data.Part2, 10m);
			Factory.Save();

			var rmaOrderLines = WhsRMAHelper.GetWhsRMAOrderLines(new List<WhsOrderLine>() { orderLine1, orderLine2 }, order, excludeExistingReceive ? returnReceive.PK : ZGuid.Empty);
			AssertEquals(2, rmaOrderLines.Count);

			AssertRMAOrderLine(rmaOrderLines.Single(line => line.ProductPK == orderLine1.WE_OP),
				productPK: orderLine1.WE_OP,
				externalReference: "O1",
				qty: 5m,
				availableQty: excludeExistingReceive ? 5m : 0m,
				packingDate: ZDate.Empty,
				expiryDate: ZDate.Empty,
				partAttrib1: string.Empty,
				partAttrib2: string.Empty,
				partAttrib3: string.Empty,
				serialNumber: string.Empty,
				key: WhsRMAHelper.GetKey(receiveLine1),
				shouldCopyBOMLinks: false);

			AssertRMAOrderLine(rmaOrderLines.Single(line => line.ProductPK == orderLine2.WE_OP),
				productPK: orderLine2.WE_OP,
				externalReference: "O1",
				qty: 15m,
				availableQty: excludeExistingReceive ? 15m : 5m,
				packingDate: ZDate.Empty,
				expiryDate: ZDate.Empty,
				partAttrib1: string.Empty,
				partAttrib2: string.Empty,
				partAttrib3: string.Empty,
				serialNumber: string.Empty,
				key: WhsRMAHelper.GetKey(receiveLine2),
				shouldCopyBOMLinks: false);
		}

		void AssertRMAOrderLine(WhsRMAOrderLine line, ZGuid productPK, ZString externalReference,
			ZDecimal qty, ZDecimal availableQty,
			ZDate packingDate, ZDate expiryDate,
			ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber,
			string key, bool shouldCopyBOMLinks)
		{
			CombineAssertions(() =>
			{
				AssertEquals(productPK, line.ProductPK);
				AssertEquals(qty, line.Quantity);
				AssertEquals(0m, line.QuantityToReturn);
				AssertEquals(availableQty, line.AvailableQtyToReturn);
				AssertEquals(packingDate, line.PackingDate);
				AssertEquals(expiryDate, line.ExpiryDate);
				AssertEquals(partAttrib1, line.PartAttrib1);
				AssertEquals(partAttrib2, line.PartAttrib2);
				AssertEquals(partAttrib3, line.PartAttrib3);
				AssertEquals(serialNumber, line.SerialNumber);
				AssertEquals(externalReference, line.ExternalReference);
				AssertEquals(shouldCopyBOMLinks, line.ShouldCopyBOMLinks);
				AssertEquals(key, line.Key);
			});
		}

		public void TestGetWhsRMAOrderLines_WithComponents()
		{
			AssertExceptionThrown<ArgumentNullException>(() => WhsRMAHelper.GetWhsRMAOrderLines(null, null));

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			part3.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");

			var receiveForPart3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receiveForPart3, part3, 1m);
			receiveForPart3.AllocateLocationsWithMock();
			receiveForPart3.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 1");
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder1, part3, 1m);
			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 2");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder2, part3, 1m);

			Helper.CreatePickNew(workOrder1);
			workOrder1.FinaliseDocketAlwaysFinalisingPick();

			Helper.CreatePickNew(workOrder2);
			workOrder2.FinaliseDocketAlwaysFinalisingPick();

			workOrder1.Receive.FinaliseDocketWithoutUserConfirmation();
			workOrder2.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, part3, 4m);
			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			Factory.Save();

			var rmaOrderLines = WhsRMAHelper.GetWhsRMAOrderLines(new List<WhsOrderLine>() { orderLine }, order);
			AssertEquals(3, rmaOrderLines.Count);
			AssertEquals(1, rmaOrderLines.Count(l => l.Quantity == 2 && !l.ShouldCopyBOMLinks));
			AssertEquals(2, rmaOrderLines.Count(l => l.Quantity == 1 && l.ShouldCopyBOMLinks));
		}

		#region TestCopyBOMComponentLinks

		public void TestCopyBOMComponentLinks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");

			var receiveForCompentPart = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForCompentPart, data.Part1, 30m);
			receiveForCompentPart.AllocateLocationsWithMock();
			receiveForCompentPart.FinaliseDocketWithoutUserConfirmation();

			Helper.CreateProductBOM(data.Part2, data.Part1, 1m, "UNT");
			data.Part2.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			data.Part2.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 4m);
			Helper.SetDocketLineAttributes(workOrderLine1, ZDate.Empty, ZDate.Empty, "color 1", "size 1", "", "");
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var originalInventory = workOrder.Receive.Lines[0];

			var newInventory1 = Factory.New<WhsReceiveLine>();
			newInventory1.WE_TransactionQuantity = 2m;
			WhsRMAHelper.CopyBOMComponentLinks(originalInventory, newInventory1);

			var newInventory2 = Factory.New<WhsReceiveLine>();
			newInventory2.WE_TransactionQuantity = 0m;
			newInventory2.WE_ClientOrderedUnits = 3m;
			WhsRMAHelper.CopyBOMComponentLinks(originalInventory, newInventory2, false);

			AssertResult(newInventory1, originalInventory, newInventory1.WE_TransactionQuantity / originalInventory.WE_TransactionQuantity);
			AssertResult(newInventory2, originalInventory, newInventory2.WE_ClientOrderedUnits / originalInventory.WE_TransactionQuantity);
		}

		void AssertResult(WhsDocketLine newInventory, WhsDocketLine originalInventory, decimal ratio = 0)
		{
			AssertEquals("BOMComponentLinks should contain 1 element", 1, newInventory.BOMComponentLinks.Count());
			AssertEquals("WIP_WE_ComponentLine should be the same as original inventory", originalInventory.BOMComponentLinks.First().WIP_WE_ComponentLine, newInventory.BOMComponentLinks.First().WIP_WE_ComponentLine);
			AssertEquals("WIP_WE_InventoryLine should be the PK of new inventory", newInventory.PK, newInventory.BOMComponentLinks.First().WIP_WE_InventoryLine);
			AssertEquals("WIP_ComponentQuantity should be correct", originalInventory.BOMComponentLinks.First().WIP_ComponentQuantity * ratio, newInventory.BOMComponentLinks.First().WIP_ComponentQuantity);
		}

		#endregion
	}
}

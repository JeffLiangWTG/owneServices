using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsRMAOrder))]
	public class WhsRMAOrderTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region TestConstructors

		public void TestConstructor_ParametersAreNull()
		{
			AssertExceptionThrown("No ParentOrder passed in, should throw exception", typeof(NullReferenceException), () => WhsRMAOrder.GetNew(null, null));

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertExceptionThrown("No Pick on Order, should throw exception", typeof(ArgumentNullException), () => WhsRMAOrder.GetNew(order, null));

			var pick = Helper.CreatePickNew(order);
			AssertExceptionThrown("No OrderLines passed in, should throw exception", typeof(ArgumentNullException), () => WhsRMAOrder.GetNew(order, null));
		}

		public void TestConstructor_PickNotFinalized()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertExceptionThrown(typeof(ArgumentException), "Must finalize Pick for Parent Order first.", () => WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>()));
		}

		public void TestConstructor_NoOrderLinesPassedIn()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "ORD");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			AssertExceptionThrown(typeof(ArgumentException), "Must pass in Order Lines.", () => WhsRMAOrder.GetNew(order, Array.Empty<WhsOrderLine>()));
		}

		#endregion

		#region TestProperties

		public void TestProperties()
		{
			var rmaOrder = (WhsRMAOrder)GetNewBusinessObject();
			AssertNotNull(rmaOrder.Order);
			AssertNotNull(rmaOrder.Lines);
			AssertEquals(1, rmaOrder.Lines.Count);
			AssertEquals("Collection should be of type 'WhsRMAOrderLineCollection'", typeof(WhsRMAOrderLineCollection), rmaOrder.Lines.GetType());
			AssertEquals("Collection should be registered child editable", true, rmaOrder.IsRegisteredEditableChildObject(rmaOrder.Lines));
		}

		#endregion

		#region TestGenerateRMAReceivesMessage

		public void TestCopyBOMComponentLinks_HaveMultipleGroupReleaseLinesFromWorkOrderInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 30m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			var part4 = Helper.CreateProduct(data.Org1, "P4");
			Helper.CreateProductBOM(part4, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part4, data.Part2, 1m, "UNT");
			part4.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part4.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.SetDocketLineAttributes(workOrderLine1, ZDate.Empty, ZDate.Empty, "color 1", "size 1", "", "");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 3m);
			Helper.SetDocketLineAttributes(workOrderLine2, ZDate.Empty, ZDate.Empty, "color 2", "size 2", "", "");
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, part4, 4m);
			Helper.SetDocketLineAttributes(workOrderLine3, ZDate.Empty, ZDate.Empty, "color 3", "size 3", "", "");
			var workOrderLine4 = Helper.CreateWhsWorkOrderLine(workOrder, part4, 5m);
			Helper.SetDocketLineAttributes(workOrderLine4, ZDate.Empty, ZDate.Empty, "color 4", "size 4", "", "");

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 5m);
			Helper.CreateWhsOrderLine(order, part4, 9m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			rmaOrder.Lines.Cast<WhsRMAOrderLine>().ForEach(l => l.QuantityToReturn = l.Quantity);

			rmaOrder.GenerateRMAReceivesMessage();
			var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);

			AssertEquals("Should contain 4 receive lines", 4, newReceive.Lines.Count);

			var expectedComponetsPartPKs = receiveForComponents.Lines.Select(l => l.PK).ToList();
			AssertReturnReceiveLines(newReceive, 2, 1, part3.PK, expectedComponetsPartPKs);
			AssertReturnReceiveLines(newReceive, 3, 1, part3.PK, expectedComponetsPartPKs);
			AssertReturnReceiveLines(newReceive, 4, 1, part4.PK, expectedComponetsPartPKs);
			AssertReturnReceiveLines(newReceive, 5, 1, part4.PK, expectedComponetsPartPKs);
		}

		public void TestCopyBOMComponentLinks_OneReleaseLineFromInventoryOfMultipleTypes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			data.Part2.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
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
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, part3, 4m);
			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			Factory.Save();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			rmaOrder.Lines.Cast<WhsRMAOrderLine>().ForEach(l => l.QuantityToReturn = l.Quantity);
			rmaOrder.GenerateRMAReceivesMessage();
			var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);

			AssertEquals("Should contain 3 receive lines", 3, newReceive.Lines.Count);

			var receiveLinesFromPBBAndNormalReceiveLine = newReceive.Lines.Where(l => l.WE_TransactionQuantity == 2 && l.WE_OP == part3.PK && !l.BOMComponentLinks.Any()).ToArray();
			AssertEquals(1, receiveLinesFromPBBAndNormalReceiveLine.Length);

			var expectedComponentsInventoryPKs = receiveForComponents.Lines.Select(l => l.PK).ToList();
			AssertReturnReceiveLines(newReceive, 1, 2, part3.PK, expectedComponentsInventoryPKs);
		}

		public void TestCopyBOMComponentLinks_MultipleOrderLinesFromSameInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 1m);
			Helper.CreateWhsOrderLine(order, part3, 1m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			rmaOrder.Lines.Cast<WhsRMAOrderLine>().ForEach(l => l.QuantityToReturn = l.Quantity);

			rmaOrder.GenerateRMAReceivesMessage();
			var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);

			AssertEquals("Should contain 1 receive lines", 1, newReceive.Lines.Count);
			AssertReturnReceiveLines(newReceive, 2, 1, part3.PK, receive.Lines.Select(l => l.PK).ToList());
		}

		public void TestCopyBOMComponentLinks_DisassembleReturnedKit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WWW", "D");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var kit = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(kit, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(kit, data.Part2, 1m, "UNT");

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, kit, 2m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, kit, 2m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			rmaOrder.Lines.Cast<WhsRMAOrderLine>().ForEach(l =>
			{
				l.QuantityToReturn = l.Quantity;
				l.WhsOverride = warehouse2.PK;
			});

			rmaOrder.GenerateRMAReceivesMessage();
			var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);

			AssertEquals("Should contain 1 receive lines", 1, newReceive.Lines.Count);
			AssertEquals(data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals(warehouse2.PK, newReceive.WD_WW_Whs);
			AssertEquals(CodeLists.ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertReturnReceiveLines(newReceive, 2, 1, kit.PK, receive.Lines.Select(l => l.PK).ToList());

			newReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
			newReceive.AllocateLocationsWithMock();
			newReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(newReceive);

			Factory.Save();

			var disassemblyWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, warehouse2, "W1", "DIS");
			Helper.CreateWhsWorkOrderLine(disassemblyWorkOrder, kit, 2m);
			Helper.CreatePickNew(disassemblyWorkOrder);
			disassemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Disassembly should succeed.", true, disassemblyWorkOrder.IsFinalised);

			disassemblyWorkOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Disassembly should succeed.", true, disassemblyWorkOrder.Receive.IsFinalised);
			AssertEquals("Receive should have two components.", 2, disassemblyWorkOrder.Receive.Lines.Count);

			var component1 = disassemblyWorkOrder.Receive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			var component2 = disassemblyWorkOrder.Receive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Disassembled Qty is correct.", 2m, component1.WE_TransactionQuantity);
			AssertEquals("Disassembled Qty is correct.", 2m, component2.WE_TransactionQuantity);
			AssertEquals("Original Inventory is correct.", inventoryLine1.PK, component1.WE_WE_OriginalDocketLineForRating);
			AssertEquals("Original Inventory is correct.", inventoryLine2.PK, component2.WE_WE_OriginalDocketLineForRating);
		}

		public void TestCopyBOMComponentLinks_DisassembleReturnedKit_WithInTransitLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse2 = Helper.CreateWarehouse("WWW", "D");
			var staging = Helper.CreateRowAndGenerateLocations(warehouse2, "STAGING").Locations.Single();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var kit = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(kit, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(kit, data.Part2, 1m, "UNT");

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, kit, 2m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, kit, 2m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			Factory.Save();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			rmaOrder.Lines.Cast<WhsRMAOrderLine>().ForEach(l =>
			{
				l.QuantityToReturn = l.Quantity;
				l.WhsOverride = warehouse2.PK;
			});

			rmaOrder.GenerateRMAReceivesMessage();
			var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);

			AssertEquals("Should contain 1 receive lines", 1, newReceive.Lines.Count);
			AssertEquals(data.Org1.PK, newReceive.WD_OH_Client);
			AssertEquals(warehouse2.PK, newReceive.WD_WW_Whs);
			AssertEquals(CodeLists.ReceiveType.Codes.Returns, newReceive.WD_DocketSubType);
			AssertReturnReceiveLines(newReceive, 2, 1, kit.PK, receive.Lines.Select(l => l.PK).ToList());

			newReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
			newReceive.AllocateLocationsWithMock();
			newReceive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(newReceive);

			Factory.Save();

			var disassemblyWorkOrder = Helper.CreateWhsWorkOrder(data.Org1, warehouse2, "W1", "DIS");
			Helper.CreateWhsWorkOrderLine(disassemblyWorkOrder, kit, 2m);
			var disassemblyPick = Helper.CreatePickNew(disassemblyWorkOrder);
			disassemblyPick.GetAllPickLines().ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Now);
			Factory.Save();

			// Assign Dest Location to one of the Transfer lines
			var transferLine = (WhsTransferLine)disassemblyPick.Transfers.Single().Lines.Single();
			transferLine.WE_WL = staging.PK;
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			disassemblyWorkOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("Disassembly should succeed.", true, disassemblyWorkOrder.IsFinalised);

			disassemblyWorkOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Disassembly should succeed.", true, disassemblyWorkOrder.Receive.IsFinalised);
			AssertEquals("Receive should have two components.", 2, disassemblyWorkOrder.Receive.Lines.Count);

			var component1 = disassemblyWorkOrder.Receive.Lines.Single(l => l.WE_OP == data.Part1.PK);
			var component2 = disassemblyWorkOrder.Receive.Lines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("Disassembled Qty is correct.", 2m, component1.WE_TransactionQuantity);
			AssertEquals("Disassembled Qty is correct.", 2m, component2.WE_TransactionQuantity);
			AssertEquals("Original Inventory is correct.", inventoryLine1.PK, component1.WE_WE_OriginalDocketLineForRating);
			AssertEquals("Original Inventory is correct.", inventoryLine2.PK, component2.WE_WE_OriginalDocketLineForRating);
			AssertEquals("Defaulted Location is correct.", staging.PK, component1.WE_WL);
			AssertEquals("Defaulted Location is correct.", staging.PK, component2.WE_WL);
		}

		public void TestDbHitsForWhsBOMInventoryPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 30m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			var part4 = Helper.CreateProduct(data.Org1, "P4");
			Helper.CreateProductBOM(part4, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part4, data.Part2, 1m, "UNT");
			part4.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part4.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.SetDocketLineAttributes(workOrderLine1, ZDate.Empty, ZDate.Empty, "color 1", "size 1", "", "");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 3m);
			Helper.SetDocketLineAttributes(workOrderLine2, ZDate.Empty, ZDate.Empty, "color 2", "size 2", "", "");
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, part4, 4m);
			Helper.SetDocketLineAttributes(workOrderLine3, ZDate.Empty, ZDate.Empty, "color 3", "size 3", "", "");
			var workOrderLine4 = Helper.CreateWhsWorkOrderLine(workOrder, part4, 5m);
			Helper.SetDocketLineAttributes(workOrderLine4, ZDate.Empty, ZDate.Empty, "color 4", "size 4", "", "");

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 5m);
			Helper.CreateWhsOrderLine(order, part4, 9m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				var rmaOrder = WhsRMAOrder.GetNew(orderInNewFactory, orderInNewFactory.Lines.Cast<WhsOrderLine>());
			}
		}

		public void TestDbHitsForWhsBOMInventoryPivot_PickByBOM()
		{
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

			var part4 = Helper.CreateProduct(data.Org1, "P4");
			part4.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(part4, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part4, data.Part2, 1m, "UNT");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			Helper.CreateWhsOrderLine(order, part3, 5m);
			Helper.CreateWhsOrderLine(order, part4, 5m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			using (RowFactory.SetCachedTables())
			{
				var rmaOrder = WhsRMAOrder.GetNew(orderInNewFactory, orderInNewFactory.Lines.Cast<WhsOrderLine>());
			}
		}

		[TestDate(2021, 8, 20)]
		public void TestCopyBOMComponentLinks_AllAttributesBeenRetained()
		{
			var data = new TestDataSimpleEnvironment(Factory);
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

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var expiryDate = ZDate.Today;
			var packingDate = ZDate.Today;
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "PLT-1", expiryDate, packingDate, "A", "B", "C", "S1", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].Allocate = true;

			Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			Factory.Save();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			rmaOrder.Lines.Cast<WhsRMAOrderLine>().ForEach(l => l.QuantityToReturn = l.Quantity);

			rmaOrder.GenerateRMAReceivesMessage();
			var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);

			Assert("All attributes are retained for new inventory", newReceive.Lines.All(l => l.WE_PartAttrib1 == "A" && l.WE_PartAttrib2 == "B" && l.WE_PartAttrib3 == "C" && l.WE_SerialNumber == "S1" && l.WE_ExpiryDate == expiryDate && l.WE_PackingDate == packingDate));
		}

		void AssertReturnReceiveLines(WhsDocket returnReceive, int expectedQuantity, int expectedLineCount, ZGuid originalPartPK, List<ZGuid> componentLinePKs)
		{
			var returnReceiveLine = returnReceive.Lines.Where(line => line.WE_TransactionQuantity == expectedQuantity && line.WE_OP == originalPartPK && line.BOMComponentLinks.Any()).ToList();
			AssertEquals($"Should be {expectedLineCount} receive line", expectedLineCount, returnReceiveLine.Count);
			for (int i = 0; i < expectedLineCount; i++)
			{
				var receiveLine = returnReceiveLine[i];
				AssertEquals(2, receiveLine.ComponentInventoryLines.Count);
				Assert(receiveLine.BOMComponentLinks.All(link => link.WIP_ComponentQuantity == expectedQuantity));
				Assert(receiveLine.ComponentInventoryLines.All(l => componentLinePKs.Contains(l.PK)));
			}
		}

		public void TestWhsOverrideEmpty_GenerateAllLines()
		{
			TestWhsOverrideCore(2, isGenerateAllLines: true, isWhsOverrideEmpty: true);
		}

		public void TestWhsOverrideEmpty_GenerateLinesByEnteredAmountMoreThanZero()
		{
			TestWhsOverrideCore(1, isGenerateAllLines: false, isWhsOverrideEmpty: true);
		}
		public void TestWhsOverrideNonEmpty_GenerateAllLines()
		{
			TestWhsOverrideCore(2, isGenerateAllLines: true, isWhsOverrideEmpty: false);
		}

		public void TestWhsOverrideNonEmpty_GenerateLinesByEnteredAmountMoreThanZero()
		{
			TestWhsOverrideCore(1, isGenerateAllLines: false, isWhsOverrideEmpty: false);
		}

		void TestWhsOverrideCore(int expectedGenLinesCount, bool isGenerateAllLines, bool isWhsOverrideEmpty)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newWarehouse = Helper.CreateWarehouse("W2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var order = GetValidOrder(data, expectedGenLinesCount, isGenerateAllLines);
			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			var expectedLogText = "Following Receives have been generated and can be accessed on Order [W00000002] - Related Jobs tab:\r\n" +
				"W00000003";
			AssertEquals("Precondition:", isGenerateAllLines ? expectedGenLinesCount : expectedGenLinesCount + 1, rmaOrder.Lines.Count);

			for (int i = 0; i < expectedGenLinesCount; i++)
			{
				rmaOrder.Lines[i].QuantityToReturn = 10m;
				if (!isWhsOverrideEmpty)
				{
					rmaOrder.Lines[i].WhsOverride = newWarehouse.PK;
				}
			}

			var actualLogText = rmaOrder.GenerateRMAReceivesMessage();
			AssertEquals(expectedLogText, actualLogText);
			AssertEquals("Order should Link to New Receive", 1, order.RelatedJobs.Count);

			var newReceive = Factory.Load<WhsReceive>(order.RelatedJobs[0].BusinessObjectPK);
			AssertReceive(data, newReceive, order.PK, expectedGenLinesCount, isWhsOverrideEmpty ? data.Whs1 : newWarehouse, null);
		}

		public void TestMultipleWhsOverride()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newWarehouse1 = Helper.CreateWarehouse("W1");
			var newWarehouse2 = Helper.CreateWarehouse("W2");
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var order = GetValidOrder(data, 5, false);
			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());

			var expectedLogText = "Following Receives have been generated and can be accessed on Order [W00000002] - Related Jobs tab:\r\n" +
				"W00000003\r\n" +
				"W00000004\r\n" +
				"W00000005";

			AssertEquals("Precondition:", 6, rmaOrder.Lines.Count);

			rmaOrder.Lines[0].QuantityToReturn = 10m;
			rmaOrder.Lines[0].WhsOverride = newWarehouse1.PK;
			//keeping original warehouse value				
			rmaOrder.Lines[1].QuantityToReturn = 10m;
			rmaOrder.Lines[2].QuantityToReturn = 10m;
			rmaOrder.Lines[2].WhsOverride = newWarehouse2.PK;
			rmaOrder.Lines[3].QuantityToReturn = 10m;
			rmaOrder.Lines[4].QuantityToReturn = 10m;
			rmaOrder.Lines[4].WhsOverride = newWarehouse1.PK;

			var actualLogText = rmaOrder.GenerateRMAReceivesMessage();
			AssertEquals(expectedLogText, actualLogText);
			AssertEquals("Order should Link to New Receives", 3, order.RelatedJobs.Count);

			var newReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK));
			AssertReceive(data, newReceives[0], order.PK, 2, newWarehouse1, new ZString[] { "PA10", "PA14" });
			AssertReceive(data, newReceives[1], order.PK, 2, data.Whs1, new ZString[] { "PA11", "PA13" });
			AssertReceive(data, newReceives[2], order.PK, 1, newWarehouse2, new ZString[] { "PA12" });
		}

		WhsOrder GetValidOrder(TestDataSimpleEnvironment data, int expectedGenLinesCount, bool isGenerateAllLines = true)
		{
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var date = ZDate.Today.AddDays(1);

			expectedGenLinesCount = isGenerateAllLines ? expectedGenLinesCount : expectedGenLinesCount + 1;

			for (int i = 0; i < expectedGenLinesCount; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, date, date, "PA1" + i, "PA2", "PA3", "");
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			for (int i = 0; i < expectedGenLinesCount; i++)
			{
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			}
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			return order;
		}

		void AssertReceive(TestDataSimpleEnvironment data, WhsReceive receive, ZGuid orderPK, int expectedGenLinesCount, WhsWarehouse expectedWarehouse, ZString[] expectedPartAttrib1)
		{
			AssertNotNull(receive);
			AssertEquals(data.Org1.PK, receive.WD_OH_Client);
			AssertEquals(expectedWarehouse.PK, receive.WD_WW_Whs);
			AssertEquals(CodeLists.ReceiveType.Codes.Returns, receive.WD_DocketSubType);
			AssertEquals("Reference", "O1 " + receive.WD_DocketID, receive.WD_ExternalReference);
			AssertEquals("Should link to Order", 1, receive.RelatedJobs.Count);
			AssertEquals("Should link to Order", orderPK, receive.RelatedJobs[0].BusinessObjectPK);

			AssertEquals("Should have receive lines", expectedGenLinesCount, receive.Lines.Count);

			for (int i = 0; i < expectedGenLinesCount; i++)
			{
				AssertEquals(data.Part1.PK, receive.Lines[i].WE_OP);
				AssertEquals(10m, receive.Lines[i].WE_TransactionQuantity);
				AssertEquals(ZDate.Today.AddDays(1), receive.Lines[i].WE_ExpiryDate);
				AssertEquals(ZDate.Today.AddDays(1), receive.Lines[i].WE_PackingDate);
				if (expectedPartAttrib1.IsNullOrEmpty())
				{
					AssertEquals("PA1" + i, receive.Lines[i].WE_PartAttrib1);
				}
				AssertEquals("PA2", receive.Lines[i].WE_PartAttrib2);
				AssertEquals("PA3", receive.Lines[i].WE_PartAttrib3);
			}

			if (!expectedPartAttrib1.IsNullOrEmpty())
			{
				Assert(receive.Lines.All(l => expectedPartAttrib1.Contains(l.WE_PartAttrib1)));
			}
		}

		#endregion

		#region TestSplitRMALinesByAttributeOnInventory

		public void TestSplitRMALinesByAttributeOnInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			data.Part2.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 1m);
			Helper.SetDocketLineAttributes(workOrderLine1, ZDate.Empty, ZDate.Empty, "color 1", "size 1", "", "");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.SetDocketLineAttributes(workOrderLine2, ZDate.Empty, ZDate.Empty, "color 2", "size 2", "", "");

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			Factory.Save();

			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 3m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			AssertEquals(2, rmaOrder.Lines.Count);
			AssertEquals(1, rmaOrder.Lines.Cast<WhsRMAOrderLine>().Count(l => l.PartAttrib1 == "color 1" && l.PartAttrib2 == "size 1" && l.Quantity == 1));
			AssertEquals(1, rmaOrder.Lines.Cast<WhsRMAOrderLine>().Count(l => l.PartAttrib1 == "color 2" && l.PartAttrib2 == "size 2" && l.Quantity == 2));
		}

		#endregion

		#region TestMergeInToOneRMALineIfMultipleOrderLinesFromInventoryWithSameAttib

		public void TestMergeInToOneRMALineIfMultipleOrderLinesFromInventoryWithSameAttib()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			data.Part1.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			data.Part2.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.SetDocketLineAttributes(workOrderLine, ZDate.Empty, ZDate.Empty, "color 1", "size 1", "", "");

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();

			Factory.Save();

			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 1m);
			Helper.CreateWhsOrderLine(order, part3, 1m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			AssertEquals(1, rmaOrder.Lines.Count);
			AssertEquals(1, rmaOrder.Lines.Cast<WhsRMAOrderLine>().Count(l => l.PartAttrib1 == "color 1" && l.PartAttrib2 == "size 1" && l.Quantity == 2));
		}

		#endregion

		#region TestSplitRMALinesByInventoryTypes

		public void TestSplitRMALinesByInventoryTypes()
		{
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

			var rmaOrder = WhsRMAOrder.GetNew(order, order.Lines.Cast<WhsOrderLine>());
			AssertEquals(3, rmaOrder.Lines.Count);
			AssertEquals(1, rmaOrder.Lines.Cast<WhsRMAOrderLine>().Count(l => !l.ShouldCopyBOMLinks && l.Quantity == 2));
			AssertEquals(2, rmaOrder.Lines.Cast<WhsRMAOrderLine>().Count(l => l.ShouldCopyBOMLinks && l.Quantity == 1));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			return WhsRMAOrder.GetNew(order, new[] { orderLine });
		}

		#endregion
	}
}

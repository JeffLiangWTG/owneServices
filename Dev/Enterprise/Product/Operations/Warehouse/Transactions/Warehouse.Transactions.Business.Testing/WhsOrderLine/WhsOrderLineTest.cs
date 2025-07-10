using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderLine))]
	public class WhsOrderLineTest : WhsPickableDocketLineTest<WhsOrderLine, WhsOrder>
	{
		#region Business Object Overrides

		public void TestCopyPersistentValuesFrom()
		{
			WhsOrderLine orderLine1 = GetNewBusinessObject();

			orderLine1.WE_TransactionQuantity = 100m;
			AssertEquals("Precondition", 0m, DocketLine.WE_TransactionQuantity);
			DocketLine.CopyPersistentValuesFrom(orderLine1);
			AssertEquals(100m, DocketLine.WE_TransactionQuantity);
		}

		public override void TestOnDeleteByInventory()
		{
			Assert("Suppress Exception - Order lines don't use Inventory", true);
		}

		#endregion

		#region Related Business Objects

		public void TestInventoryFilter()
		{
			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.WD_WW_Whs = Factory.New<WhsWarehouse>().PK;

			var orderLine = GetNewBusinessObject(order);
			orderLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			orderLine.WE_ExpiryDate = ZDate.Today.AddDays(1);
			orderLine.WE_PackingDate = ZDate.Today.AddDays(-1);
			orderLine.WE_PartAttrib1 = "PA1";
			orderLine.WE_PartAttrib2 = "PA2";
			orderLine.WE_PartAttrib3 = "PA3";
			orderLine.WE_SerialNumber = "SN1";

			var inventory = orderLine.InventoryFilter;
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Client" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Product" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Expiry Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Packing Date" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property2"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 1" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 2" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Part Attribute 3" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertEquals(true, inventory.FilterBusinessObjectDefaults.ContainsDefaultFor("Serial Number" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
		}

		#region TestPickLines

		protected override WhsPickableDocket GetPickableDocket_ForPickLinesTest(TestDataForBOM data)
		{
			var docket = GetNewWhsDocket();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			Helper.CreateWhsPickableDocketLine(docket, data.BOM.BikeWheel, 2m);
			Helper.CreateWhsPickableDocketLine(docket, data.BOM.BikeEngine, 1m);

			return docket;
		}

		#endregion

		#endregion

		#region Validation

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsOrderLineValidation);
		}

		public void TestValidationUS()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			order.WD_DocketSubType = ReceiveType.Codes.Customs;
			AssertEquals(typeof(US.WhsOrderLineValidationUS), order.Lines[0].Validation.GetType());
		}

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsOrderLineLookups);
		}

		#endregion

		#region Properties

		#region TestCanGenerateChildWorkOrder

		protected override void TestCanGenerateChildWorkOrderCore()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			AssertEquals(false, orderLine.CanGenerateChildWorkOrder);

			var part = Factory.New<OrgSupplierPart>();
			orderLine.WE_OP = part.PK;
			AssertEquals(false, orderLine.CanGenerateChildWorkOrder);

			part.BillOfMaterials.AddNew();
			AssertEquals(true, orderLine.CanGenerateChildWorkOrder);
		}

		#endregion

		#region TestSumOfUnitsMet

		protected override void TestSumOfUnitsMetCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var docket = GetNewWhsDocket();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneePK = data.Org1.PK;
			Factory.Save();

			var docketLine = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 10m);
			AssertEquals("Docket is Unpicked, should have no SumOfUnitsMet.", 0m, docketLine.SumOfUnitsMet);

			Helper.CreatePickNew(docket);
			AssertEquals("Docket has 10 Picked, SumOfUnits Met should be equal to PickLineQuantity.", 10m, docketLine.SumOfUnitsMet);

			var releaseLine1 = docketLine.ReleaseLines[0];
			releaseLine1.Quantity = 15m;
			AssertEquals("Docket has 10 Picked but the Non-Persistent Wrapper has taken Precedence.", 15m, docketLine.SumOfUnitsMet);

			var releaseLine2 = docketLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 35m;
			AssertEquals("Docket has 10 Picked but the Non-Persistent Wrapper has taken Precedence.", 50m, docketLine.SumOfUnitsMet);
		}

		public void TestSumOfUnitsMetOfComponentLine()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();

			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);

			AssertEquals(10m, wheelComponentLine.SumOfUnitsMet);
			AssertEquals(5m, frameComponentLine.SumOfUnitsMet);
		}

		#endregion

		#region TestPickLinesForBindingForGUI

		public void TestPickLinesForBindingForGUI()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("1 directly picked, 1 assembled.", 2, orderLine.PickLines.Count);
			AssertEquals("Plus 1 on component line.", 3, orderLine.PickLinesForRelease.Count);
		}

		#endregion

		#region TestPickLinesForRelease

		public void TestPickLinesForRelease()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = order.Lines.AddNew();

			var pickline1 = Factory.New<WhsPickLine>();
			pickline1.WZ_WE_TransactionLine = orderLine.PK;
			pickline1.WZ_Units = 1;

			AssertContainsExactElementsInAnyOrder(new[] { pickline1 }, orderLine.PickLinesForRelease);

			var orderline2 = Factory.New<WhsOrderLine>();
			orderline2.WE_WE_ParentDocketLine = orderLine.PK;
			var pickline2 = Factory.New<WhsPickLine>();
			pickline2.WZ_Units = 2;

			pickline2.WZ_WE_TransactionLine = Guid.NewGuid();
			AssertContainsExactElementsInAnyOrder(new[] { pickline1 }, orderLine.PickLinesForRelease);

			pickline2.WZ_WE_TransactionLine = orderline2.PK;
			AssertContainsExactElementsInAnyOrder(new[] { pickline1, pickline2 }, orderLine.PickLinesForRelease);

			var pickline3 = Factory.New<WhsPickLine>();
			pickline3.WZ_WE_TransactionLine = orderLine.PK;
			pickline3.WZ_Units = 3;

			AssertContainsExactElementsInAnyOrder(new[] { pickline1, pickline2, pickline3 }, orderLine.PickLinesForRelease);
			pickline2.WZ_Units = 0;
			AssertContainsExactElementsInAnyOrder(new[] { pickline1, pickline3 }, orderLine.PickLinesForRelease);
		}

		#endregion

		#region TestPickLinesForRelease_DbHits

		public void TestPickLinesForRelease_DbHits()
		{
			const int NumberOfInventoriesToCreate = 10;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 50m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 200m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			for (int i = 0; i < NumberOfInventoriesToCreate / 2; i++)
			{
				Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			}

			Helper.CreatePickNew(order);

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsDocketSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsDocketLineSchema.Constants.TableName, 3);
			expectedDbHits.Add(WhsPickLineSchema.Constants.TableName, 1);

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			orderInOtherFactory.LinesToPickForBinding.Cast<WhsPickableDocketLine>().SelectMany(l => l.PickLinesForRelease).Count();
			AssertDbHits(expectedDbHits, otherFactory);
		}

		#endregion

		#region TestChildComponentLines

		public void TestChildComponentLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, mainProduct, 5m, inventoryLocation);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive1.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, mainProduct, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, mainProduct, 10m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, mainProduct, 10m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(orderLine1.PickableDocket);
			AssertChildComponentLinesLength(mainProduct, pick1, 0);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory21 = Helper.CreateWhsReceiveInventoryLine(receive2, mainProduct, 5m, inventoryLocation);
			var inventory22 = Helper.CreateWhsReceiveInventoryLine(receive2, bomComponentProduct, 2m, inventoryLocation);
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive2.IsFinalised);
			Factory.Save();

			var pick2 = Helper.CreatePickNew(orderLine2.PickableDocket);
			AssertChildComponentLinesLength(mainProduct, pick2, 0);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventory31 = Helper.CreateWhsReceiveInventoryLine(receive3, mainProduct, 5m, inventoryLocation);
			var inventory32 = Helper.CreateWhsReceiveInventoryLine(receive3, bomComponentProduct, 3m, inventoryLocation);
			receive3.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive3.IsFinalised);
			Factory.Save();

			var pick3 = Helper.CreatePickNew(orderLine3.PickableDocket);
			AssertChildComponentLinesLength(mainProduct, pick3, 1);
		}

		void AssertChildComponentLinesLength(OrgSupplierPart mainProduct, WhsPick pick, ZInt length)
		{
			var pickableDocketLine = pick.Orders.Cast<WhsPickableDocket>().SelectMany(o => o.Lines).Cast<WhsPickableDocketLine>().Single(l => l.SupplierPart.PK == mainProduct.PK);
			var lengthOfChildComponentLine = pickableDocketLine.ChildComponentLines != null ? pickableDocketLine.ChildComponentLines.Count : 0;
			AssertEquals(length.ToString() + " childcomponent should be created.", length, lengthOfChildComponentLine);
		}

		#endregion

		#region TestParentDocketLine

		public void TestParentDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var pickableDocketLine = pick.Orders.Cast<WhsPickableDocket>().SelectMany(o => o.Lines).Cast<WhsPickableDocketLine>().Single(l => l.SupplierPart.PK == mainProduct.PK);
			AssertEquals("1 childcomponent will be created.", 1, pickableDocketLine.ChildComponentLines.Count);
			AssertEquals("Childcomponent's ParentDocketLine should be orderLine", orderLine.PK, pickableDocketLine.ChildComponentLines.ElementAt(0).ParentLine.PK);
		}

		#endregion

		#region TestQuantityAcheivedFromComponents

		public void TestQuantityAcheivedFromComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			AssertEquals("QuantityAcheivedFromComponents should be 6", 6m, orderLine.TotalPickLineQuantityFromComponents);
		}

		#endregion

		#region TestPickGroupDefaultsFromProductWhenProductSet

		public void TestPickGroupDefaultsFromProductWhenProductSet()
		{
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				AssertEquals("Precondition:", new ZShort(0), orderLine.WE_PickGroup);

				var productParams = orderLine.Product.ParamsByWhsAndClient.AddNew();
				productParams.W3_WW = Factory.New<WhsWarehouse>().PK;
				productParams.W3_PickGroup = 1;
				orderLine.WE_OP = ZGuid.Empty;
				orderLine.WE_OP = data.Part1.PK;
				AssertEquals("No Product Param with same warehouse.", new ZShort(0), orderLine.WE_PickGroup);

				productParams.W3_WW = data.Whs1.PK;
				orderLine.WE_OP = ZGuid.Empty;
				orderLine.WE_OP = data.Part1.PK;
				AssertEquals("Should have defaulted from Product.", new ZShort(1), orderLine.WE_PickGroup);

				orderLine.WE_PickGroup = 2;
				orderLine.WE_OP = ZGuid.Empty;
				orderLine.WE_OP = data.Part1.PK;
				AssertEquals("Should not default from Product if has value.", new ZShort(2), orderLine.WE_PickGroup);

				productParams.W3_PickGroup = 0;
				orderLine.WE_PickGroup = 0;
				orderLine.WE_OP = ZGuid.Empty;
				orderLine.WE_OP = data.Part1.PK;
				AssertEquals("Should not default if no pick group set on product params.", new ZShort(0), orderLine.WE_PickGroup);
			}
		}

		#endregion

		#region TestPickGroupDescription

		public void TestPickGroupDescription()
		{
			var collection = new PickGroupCollection();
			var pickGroup = collection.AddNew();
			pickGroup.Description = (NoResString)"Desc";

			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var orderLine = Factory.New<WhsOrderLine>();
				AssertEquals("", orderLine.PickGroupDescription);

				orderLine.WE_PickGroup = 1;
				AssertEquals("Desc", orderLine.PickGroupDescription);

				orderLine.WE_PickGroup = 2;
				AssertEquals("", orderLine.PickGroupDescription);
			}
		}

		#endregion

		#region TestTempAllocated

		public void TestTempAllocated()
		{
			DocketLine.TempAllocated = 150m;
			AssertEquals(150m, DocketLine.TempAllocated);
		}

		#endregion

		#region TestRelatedInLineForExternalProcessing

		public void TestRelatedInLineForExternalProcessing()
		{
			var iLine = new WhsWarehouseTransactionLine();
			DocketLine.RelatedInLineForExternalProcessing = iLine;
			AssertEquals(iLine, DocketLine.RelatedInLineForExternalProcessing);
		}

		#endregion

		#region TestCanUpdateFromInventory

		public override void TestCanUpdateFromInventory()
		{
			base.TestCanUpdateFromInventory();

			var docketLine = GetNewBusinessObject();
			var docket = GetNewWhsDocket(docketLine);
			docket.WD_WP = Factory.New<WhsPick>().PK;
			AssertEquals(false, docketLine.CanUpdateFromInventory);
		}

		#endregion

		#region TestWE_CrossDockQuantity

		protected override ZDecimal ExpectedCrossDockQuantity
		{
			get { return 8m; }
		}

		#endregion

		#region TestWE_OP_UpdatesReleaseLinesIfBuilt

		public void TestWE_OP_UpdatesReleaseLinesIfBuilt()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			AssertEquals("Precondition: Release Line is built.", 1, orderLine1.ReleaseLines.Count);

			var releaseLine = orderLine1.ReleaseLines[0];
			AssertEquals("Precondition: 10 units released.", 10m, releaseLine.Quantity);

			pick.OrderedInventories.GetOrderedInventoryForLine(orderLine1).AvailableInventories[0].PickLineQuantity = 0m;
			orderLine1.WE_OP = data.Part2.PK;
			AssertEquals("Changing product should rebuild the collection.", true, orderLine1.IsReleaseLineCollectionBuilt);
			AssertEquals("No units should be released as nothing has been allocated to this orderLine.", 0, orderLine1.ReleaseLines.Count);

			var orderedInventory = pick.OrderedInventories.GetOrderedInventoryForLine(orderLine2);
			orderedInventory.Owners.Add(orderLine1);
			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.PickLineQuantity = 15m;
			AssertEquals("New Release Line should get added to this orderLine.", 1, orderLine1.ReleaseLines.Count);

			var newReleaseLine = orderLine1.ReleaseLines[0];
			AssertEquals("10 Units should be released.", 10m, newReleaseLine.Quantity);
		}

		#endregion

		#region TestWE_TransactionQuantity_ReadOnly

		public void TestWE_TransactionQuantity_ReadOnly()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("WE_TransactionQuantity should be editable for a new Order.", false, orderLine.WE_TransactionQuantityInfo.ReadOnly);

			Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("WE_TransactionQuantity should be editable for a Picked but non-Finalised Order.", false, orderLine.WE_TransactionQuantityInfo.ReadOnly);

			order.FinaliseDocket();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("WE_TransactionQuantity should be ReadOnly for a Finalised Order.", true, orderLine.WE_TransactionQuantityInfo.ReadOnly);
		}

		#endregion

		#region TestClientDetail

		public void TestClientDetail()
		{
			var client = Helper.CreateClient("CLIENT", "NAME");
			Docket.WD_OH_Client = client.PK;
			AssertEquals("ClientDetail should come from order.", "CLIENT NAME", DocketLine.ClientDetail);
		}

		#endregion

		#region TestConsigneeDetail

		public void TestConsigneeDetail()
		{
			var consignee = Helper.CreateClient("CONSIGNEE", "NAME");
			Docket.ConsigneePK = consignee.PK;
			AssertEquals("ConsigneeDetail should come from order.", "CONSIGNEE NAME", DocketLine.ConsigneeDetail);
		}

		#endregion

		#region ReadOnly

		#region TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly

		protected override void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			if (data == null)
			{
				data = new TestDataSimpleEnvironment(Factory);
			}

			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_PreventOrderLinesUpdateWhenOrderIsPickingRegistryDisabled(info, data);
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_PreventOrderLinesUpdateWhenOrderIsPickingRegistryEnabled(info, data);
		}

		void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_PreventOrderLinesUpdateWhenOrderIsPickingRegistryDisabled(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			var orderLine = (WhsDocketLine)info.BizObj;
			var order1 = (WhsOrder)orderLine.Docket;
			TestReadOnly(info, false, false, true, true);
			order1.WD_DocketStatus = DocketStatus.Codes.Entered; // clean up

			Helper.SetUpWhsOrder(order1, data.Org1, data.Whs1);
			orderLine.WE_OP = data.Part1.PK;
			orderLine.WE_TransactionQuantity = 10m;
			Factory.Save();

			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_PickedLines(data, order1, info);

			var customsOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "CustomsOrder");
			customsOrder.WD_DocketSubType = OrderType.Codes.Customs;

			var customsOrderLine = Helper.CreateWhsOrderLine(customsOrder, data.Part1, 10m);
			var infoCustoms = customsOrderLine.FindPropertyInfo(info.Name);
			Factory.Save();
			customsOrderLine.CustomsData.WB_EntryKey = "ABC";
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_USBonded(data, customsOrder, infoCustoms);
			customsOrderLine.CustomsData.WB_EntryKey = "ABC";
			TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_MultiOrderPick(data, customsOrder, infoCustoms);
		}

		void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_PickedLines(TestDataSimpleEnvironment data, WhsOrder order, ZPropertyInfo info)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 10m, totalPickLineQuantity);

			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("None of the allocated stock was picked - can modify order.", false, info.ReadOnly);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("When any allocated stock was picked - cannot modify order.", true, info.ReadOnly);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition: None of the allocated stock was picked - can modify order.", false, info.ReadOnly);

			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;
			AssertEquals("When any allocated stock was picked from putaway location - cannot modify order.", true, info.ReadOnly);

			// clean up
			pickLine.WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			pick.CancelPick();
			AssertEquals(true, pick.IsCancelled);
		}

		void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_USBonded(TestDataSimpleEnvironment data, WhsOrder order, ZPropertyInfo info)
		{
			// ensure ReadOnly for not picked US Bonded orders.
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);

			AssertEquals("Precondition", true, order.IsCustomsTransaction);
			AssertEquals(string.Format("{0} should not be readonly when the Order is not picked and is Customs Transaction.", info.Name), false, info.ReadOnly);

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals(string.Format("{0} should not be readonly when the Order is not picked and is US Customs Transaction.", info.Name), false, info.ReadOnly);

			// pick a order and ensure ReadOnly is false for a picked order
			var pick = Helper.CreatePickNew(order);
			AssertEquals(string.Format("{0} should not be readonly when the Order picked by not a Multi-Order Pick.", info.Name), false, info.ReadOnly);

			// ensure ReadOnly for picked US Bonded orders.
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			AssertEquals(string.Format("{0} should be readonly when the Order is Customs Transaction.", info.Name), true, info.ReadOnly);

			// clean up
			pick.CancelPick();
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			AssertEquals("Precondition", true, pick.IsCancelled);
			AssertEquals("Precondition", false, info.ReadOnly);
		}

		void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_MultiOrderPick(TestDataSimpleEnvironment data, WhsOrder order, ZPropertyInfo info)
		{
			// pick second order and ensure ReadOnly is true for Multi-Order picks.
			var order2 = GetNewWhsDocket();
			Helper.SetUpWhsOrder(order2, data.Org1, data.Whs1);
			Helper.CreateWhsPickableDocketLine(order2, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order, order2);
			AssertEquals("Precondition - Pick has multiple Orders.", true, pick.IsMultiOrderPick);
			AssertEquals(string.Format("{0} should be readonly when the Order is on a Multi-Order Pick.", info.Name), true, info.ReadOnly);

			// clean up
			pick.CancelPick();
			AssertEquals("Precondition", true, pick.IsCancelled);
			AssertEquals("Precondition", false, info.ReadOnly);
		}

		void TestPackQuantityAndTypeAndRelatedQuantityInfoReadOnly_PreventOrderLinesUpdateWhenOrderIsPickingRegistryEnabled(ZPropertyInfo info, TestDataSimpleEnvironment data)
		{
			using (WarehouseDataRegistry.Instance.PreventOrderLinesUpdateWhenOrderIsInPicking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 10m);
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

				var propertyInfo = orderLine.FindPropertyInfo(info.Name);
				AssertEquals("Precondition: property is not readonly.", false, propertyInfo.ReadOnly);

				var pick = Helper.CreatePickNew(order);
				AssertEquals("", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);
				AssertEquals("Property is readonly.", true, propertyInfo.ReadOnly);
			}
		}

		#endregion

		#region TestAfterPickProductAndAttribsReadOnly

		protected override void TestAfterPickProductAndAttribsReadOnly(ZPropertyInfo info)
		{
			var orderLine = (WhsDocketLine)info.BizObj;
			var order1 = (WhsOrder)orderLine.Docket;
			TestReadOnly(info, false, false, true, true);

			order1.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Precondition - The DocketLine must not be saved.", false, orderLine.IsInDatabase);
			AssertEquals(string.Format("{0} should be editable for a Picked Order when the DocketLine is new (not in the DB).", info.Name), false, info.ReadOnly);

			// setup Orders 1 & 2 so that they can be picked
			Helper.SetUpWhsOrder(order1, order1.Client, order1.Warehouse);
			orderLine.WE_TransactionQuantity = 10m;

			var order2 = GetNewWhsDocket();
			Helper.SetUpWhsOrder(order2, order1.Client, order1.Warehouse, "Order2");
			Helper.CreateWhsPickableDocketLine(order2, orderLine.SupplierPart, 10m);

			// pick the orders and ensure ReadOnly is true for a multi-order pick
			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition - Pick did not have multiple Orders.", true, pick.IsMultiOrderPick);
			AssertEquals(string.Format("{0} should be readonly when the Order is on a Multi-Order Pick.", info.Name), true, info.ReadOnly);

			pick.CancelPick(); // cleanup
			AssertEquals("Precondition - Cancel Pick failed.", false, order1.IsAttachedToPickButNotFinalised);

			pick = Helper.CreatePickNew(order1);
			order1.WD_DocketStatus = DocketStatus.Codes.AttachedToPick; // SOP (shh)

			Factory.Save();
			AssertEquals("Precondition - The DocketLine must be saved.", true, orderLine.IsInDatabase);
			AssertEquals(string.Format("{0} should be readonly for a Picked Order when the DocketLine is saved to the DB.", info.Name), true, info.ReadOnly);
		}

		#endregion

		#endregion

		#region TestIsUpdateForOrderPickingStatusAllowed

		protected override void TestIsUpdateForOrderPickingStatusAllowedCore()
		{
			var orderLine = GetNewBusinessObject();
			AssertEquals("Not null OrderLine true", true, orderLine.IsUpdateForOrderPickingStatusAllowed());
		}

		#endregion

		#region TestWE_WHC_NKOrderedHeldCode_ReadOnly

		public new void TestWE_WHC_NKOrderedHeldCode_ReadOnly()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			AssertEquals("Precondition - Order should not be attached to the pick.", false, order.IsAttachedToPick);
			AssertEquals("WE_WHC_NKOrderedHeldCode should be a read/write property when order is not attached to a pick", false, orderLine.WE_WHC_NKOrderedHeldCodeInfo.ReadOnly);

			Helper.CreatePickNew(order);
			AssertEquals("Precondition - Order should be attached to the pick.", true, order.IsAttachedToPick);
			AssertEquals("WE_WHC_NKOrderedHeldCode should be read only when order is attached to a pick", true, orderLine.WE_WHC_NKOrderedHeldCodeInfo.ReadOnly);
		}

		public void TestWE_WHC_NKOrderedHeldCode_ReadOnly_CancelledDocket()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			order.CancelReactivateDocket();
			AssertEquals("Precondition - Order should be cancelled.", true, order.IsFinalisedOrCancelled);
			AssertEquals("WE_WHC_NKOrderedHeldCode should be read only when order is cancelled", true, orderLine.WE_WHC_NKOrderedHeldCodeInfo.ReadOnly);
		}

		#endregion

		#region TestIsFinalisedProperty

		public void TestIsFinalisedProperty_Departed()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			AssertEquals("Precondition - IsFinalised should be false.", false, orderLine.IsFinalised);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Order line status should be departed.", DocketLineStatus.Codes.Departed, orderLine.WE_DocketLineStatus);
			Assert("Order line should have finalised date", orderLine.WE_FinalisedDate.IsValid);
			AssertEquals("IsFinalised should be true after pick finalisation.", true, orderLine.IsFinalised);
		}

		public void TestIsFinalisedProperty()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			AssertEquals("Precondition - IsFinalised should be false.", false, orderLine.IsFinalised);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			AssertEquals("Order line status should be finalised.", DocketLineStatus.Codes.Finalised, orderLine.WE_DocketLineStatus);
			Assert("Order line should have finalised date", orderLine.WE_FinalisedDate.IsValid);
			AssertEquals("IsFinalised should be true after pick finalisation.", true, orderLine.IsFinalised);
		}

		#endregion

		#endregion

		#region UnitPricePropertyInfos

		public override void TestWE_UnitDiscountAmountInfo()
		{
			Docket = GetNewWhsDocket();
			DocketLine = GetNewBusinessObject(Docket);
			AssertIsJobFinalisedOrCancelledReadOnly(DocketLine.WE_UnitDiscountAmountInfo);
		}

		public override void TestWE_UnitDiscountPercentInfo()
		{
			Docket = GetNewWhsDocket();
			DocketLine = GetNewBusinessObject(Docket);
			AssertIsJobFinalisedOrCancelledReadOnly(DocketLine.WE_UnitDiscountPercentInfo);
		}

		public override void TestWE_UnitPriceAfterDiscountInfo()
		{
			Docket = GetNewWhsDocket();
			DocketLine = GetNewBusinessObject(Docket);
			AssertIsJobFinalisedOrCancelledReadOnly(DocketLine.WE_UnitPriceAfterDiscountInfo);
		}

		public override void TestWE_RecommendedUnitPriceInfo()
		{
			Docket = GetNewWhsDocket();
			DocketLine = GetNewBusinessObject(Docket);
			AssertIsJobFinalisedOrCancelledReadOnly(DocketLine.WE_RecommendedUnitPriceInfo);
		}

		void AssertIsJobFinalisedOrCancelledReadOnly(ZPropertyInfo info)
		{
			TestReadOnly(info, false, false, true, true);
		}

		public void TestWE_UnitDiscountAmountInfo_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertIsJobFinalisedOrCancelledReadOnly_TaskPlanningStatus(pick, orderLine.WE_UnitDiscountAmountInfo);
		}

		public void TestWE_UnitDiscountPercentInfo_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertIsJobFinalisedOrCancelledReadOnly_TaskPlanningStatus(pick, orderLine.WE_UnitDiscountPercentInfo);
		}

		public void TestWE_UnitPriceAfterDiscountInfo_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertIsJobFinalisedOrCancelledReadOnly_TaskPlanningStatus(pick, orderLine.WE_UnitPriceAfterDiscountInfo);
		}

		public void TestWE_RecommendedUnitPriceInfo_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			AssertIsJobFinalisedOrCancelledReadOnly_TaskPlanningStatus(pick, orderLine.WE_RecommendedUnitPriceInfo);
		}

		void AssertIsJobFinalisedOrCancelledReadOnly_TaskPlanningStatus(WhsPick pick, ZPropertyInfo info)
		{
			pick.WP_TaskPlanningStatus = string.Empty;
			AssertEquals(false, info.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(true, info.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals(false, info.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals(true, info.ReadOnly);
		}

		#endregion

		#region Flags

		#region TestCanCrossDockInventory

		protected override bool ExpectedCanCrossDockInventory => true;

		#endregion

		#region TestIsComponentLineOnSalesOrder

		protected override bool IsChildOrderLineAComponentLineOnSalesOrder => true;

		#endregion

		#endregion

		#region TestWE_CalculateExtendedLinePriceCore

		protected override void TestWE_CalculateExtendedLinePriceCore(WhsOrderLine orderLine)
		{
			Helper.CreatePickNew(orderLine.Order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Should calculate extended line price", 15m * 20m, orderLine.WE_ExtendedLinePrice);

			releaseLine.Quantity = 5m;
			AssertEquals("Should calculate extended line price", 5m * 20m, orderLine.WE_ExtendedLinePrice);

			SetLineOrderUnitAndPrice(orderLine, 25m, 30m); // is not change the price
			AssertEquals("Should calculate extended line price", 5m * 30m, orderLine.WE_ExtendedLinePrice);
		}

		#endregion

		#region TestMatchesInventoryForCrossDocking

		public void TestMatchesInventoryForCrossDocking()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, ZDate.Empty, ZDate.Empty, "A1", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			orderLine.WE_PartAttrib1 = "Different";
			AssertEquals("Order Line has different attribute to Inventory, should not match.", false, orderLine.MatchesInventoryForCrossDocking(inventory1));
			orderLine.WE_PartAttrib1 = ""; // clean-up

			inventory1.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory1.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertEquals("Inventory is damaged, should not match.", false, orderLine.MatchesInventoryForCrossDocking(inventory1));
			inventory1.WI_InventoryStatus = InventoryStatus.Codes.Available; // clean-up
			inventory1.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "";

			var orderLineWithDifferentProduct = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			AssertEquals("Order Line has different Product to Inventory, should not match.", false, orderLineWithDifferentProduct.MatchesInventoryForCrossDocking(inventory1));

			var orderWithDifferentClient = Helper.CreateWhsOrder(Helper.CreateClient(), data.Whs1, "O2");
			var orderLineWithDifferentClient = Helper.CreateWhsOrderLine(orderWithDifferentClient, data.Part1, 10m);
			AssertEquals("Order Line has different Client to Inventory, should not match.", false, orderLineWithDifferentClient.MatchesInventoryForCrossDocking(inventory1));

			var orderWithDifferentWarehouse = Helper.CreateWhsOrder(data.Org1, Helper.CreateWarehouse("WHS"), "O3");
			var orderLineWithDifferentWarehouse = Helper.CreateWhsOrderLine(orderWithDifferentWarehouse, data.Part1, 10m);
			AssertEquals("Order Line has different Warehouse to Inventory, should not match.", false, orderLineWithDifferentWarehouse.MatchesInventoryForCrossDocking(inventory1));
			AssertEquals("Order Line has matching whs, client, product & attribs, should not match.", true, orderLine.MatchesInventoryForCrossDocking(inventory1));
		}

		#endregion

		#region TestReserveStockIfAbleTo

		public void TestReserveStockIfAbleTo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, ZDate.Empty, ZDate.Empty, "A1", "", "", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation, ZDate.Empty, ZDate.Empty, "A2", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertNull("Passing in null should not reserve anything.", orderLine.ReserveStockIfAbleTo(null));

			orderLine.WE_PartAttrib1 = "Different";
			AssertNull("Should not be able to reserve Inventory with different attributes.", orderLine.ReserveStockIfAbleTo(inventory1));
			orderLine.WE_PartAttrib1 = ""; // clean-up

			inventory1.WI_InventoryStatus = InventoryStatus.Codes.Held;
			inventory1.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			AssertNull("Should not be able to reserve Damaged Inventory", orderLine.ReserveStockIfAbleTo(inventory1));
			inventory1.WI_InventoryStatus = InventoryStatus.Codes.Available; // clean-up
			inventory1.InDocketLine.WE_WHC_NKCurrentInventoryHeldCode = "";

			order.CancelReactivateDocket();
			AssertNull("Should not be able to reserve Inventory for Cancelled Orders.", orderLine.ReserveStockIfAbleTo(inventory1));
			order.CancelReactivateDocket(); // clean-up

			var orderLineWithDifferentProduct = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			AssertNull("Should not be able to reserve Inventory for a different product.", orderLineWithDifferentProduct.ReserveStockIfAbleTo(inventory1));

			var orderWithDifferentClient = Helper.CreateWhsOrder(Helper.CreateClient(), data.Whs1, "O2");
			var orderLineWithDifferentClient = Helper.CreateWhsOrderLine(orderWithDifferentClient, data.Part1, 10m);
			AssertNull("Should not be able to reserve Inventory for a different client.", orderLineWithDifferentClient.ReserveStockIfAbleTo(inventory1));

			var orderWithDifferentWarehouse = Helper.CreateWhsOrder(data.Org1, Helper.CreateWarehouse("WHS"), "O3");
			var orderLineWithDifferentWarehouse = Helper.CreateWhsOrderLine(orderWithDifferentWarehouse, data.Part1, 10m);
			AssertNull("Should not be able to reserve Inventory for a different warehouse.", orderLineWithDifferentWarehouse.ReserveStockIfAbleTo(inventory1));

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, inventoryLocation, data.Whs1.FindLocation("A-2"));
			transferLine.WE_PartAttrib1 = "PA1";
			transferLine.RunPreSaveValidation(); // Commits stock
			Factory.Save();
			AssertNull("Should not be able to reserve Inventory that is fully committed.", orderLine.ReserveStockIfAbleTo(inventory1));

			transferLine.WE_TransactionQuantity = 6m;
			transferLine.RunPreSaveValidation(); // Commits stock
			Factory.Save();
			AssertExceptionThrown(typeof(ArgumentException), "Should not attempt to Reserve a Negative amount.\r\nParameter name: quantityToReserve",
				() => orderLine.ReserveStockIfAbleTo(inventory1, -1m));
			AssertInventoryHasBeenReserved("As Inventory has 6 stock committed, should be able to reserve the other 4.", orderLine, inventory1, 4m);

			orderLine.WE_TransactionQuantity = 4m;
			AssertNull("Should not be able to reserve Inventory as the order line has been fully reserved.", orderLine.ReserveStockIfAbleTo(inventory2));
			orderLine.WE_TransactionQuantity = 10m; // clean-up
			AssertInventoryHasBeenReserved("Orderline has 6 stock available to reserve, but we have specified to only reserve 1.", orderLine, inventory3, 1m, quantityToReserve: 1m);
			AssertInventoryHasBeenReserved("As Orderline already has 5 stock reserved, should only reserve 5 even if more is specified.", orderLine, inventory2, 5m, quantityToReserve: 10m);

			inventory2.ReservedPickLines.DeleteAll(); // clean-up
			AssertEquals("Precondition", 5m, orderLine.WE_CrossDockQuantity);
			AssertInventoryHasBeenReserved("As Orderline already has 5 stock reserved, should only reserve 5.", orderLine, inventory2, 5m);

			var orderLineWithSpecifiedAttrib = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLineWithSpecifiedAttrib.WE_PartAttrib1 = "A1";
			Helper.CreatePickNew(order);
			AssertNull("Should not be able to reserve Inventory for picked orders.", orderLineWithSpecifiedAttrib.ReserveStockIfAbleTo(inventory2));
		}

		WhsPickLine AssertInventoryHasBeenReserved(string assertionMessage, WhsOrderLine orderLine, WhsInventoryView inventory, decimal expectedQty, decimal? quantityToReserve = null, bool inventoryHasDocketLine = true)
		{
			var reservedPickLine = quantityToReserve.HasValue
				? orderLine.ReserveStockIfAbleTo(inventory, quantityToReserve.Value)
				: orderLine.ReserveStockIfAbleTo(inventory);

			AssertNotNull("Should be able to reserve Inventory.", reservedPickLine);
			AssertEquals("Related Collections should all contain the new Reserved pick line.", 1, orderLine.ReservedPickLines.Count(p => p == reservedPickLine)); // make sure adhoc collection doesn't add same pickline twice
			AssertCollectionContains("Related Collections should all contain the new Reserved pick line.", reservedPickLine, orderLine.ReservedPickLines);
			AssertCollectionContains("Related Collections should all contain the new Reserved pick line.", reservedPickLine, inventory.ReservedPickLines);
			AssertEquals("reservedPickLine.WZ_WE_TransactionLine", orderLine.PK, reservedPickLine.WZ_WE_TransactionLine);
			AssertEquals("reservedPickLine.WZ_WE_InventoryLine", inventory.WI_WE_InDocketLine, reservedPickLine.WZ_WE_InventoryLine);

			if (inventoryHasDocketLine)
			{
				AssertEquals("reservedPickLine.WZ_WE_InventoryLine", inventory.InDocketLine.PK, reservedPickLine.WZ_WE_InventoryLine);
			}
			else
			{
				AssertEquals("Reserved Pick Line should not have inventory line set, if the Inventory Docket Line is not yet created.", ZGuid.Empty, reservedPickLine.WZ_WE_InventoryLine);
			}

			AssertEquals(assertionMessage, expectedQty, reservedPickLine.WZ_Units);
			AssertEquals(assertionMessage, expectedQty, reservedPickLine.WZ_OriginalReservedQty);

			return reservedPickLine;
		}

		public void TestReserveStockIfAbleTo_PalletIDOnOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var locationA = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive1", data.Part1, 100m, locationA, "PLT1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive2", data.Part1, 10m, locationA, "PLT2");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Receive3", data.Part1, 50m, locationA, "PLT3");

			var inventory1 = receive1.Inventory[0];
			var inventory2 = receive2.Inventory[0];
			var inventory3 = receive3.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			orderLine1.WE_PalletID = "Different";
			AssertNull("Should not be able to reserve Inventory with different pallet ID on order line", orderLine1.ReserveStockIfAbleTo(inventory1));

			orderLine1.WE_PalletID = "PLT1";
			AssertNotNull("Should be able to reserve Inventory with same pallet ID on order line", orderLine1.ReserveStockIfAbleTo(inventory1));

			orderLine2.WE_PalletID = "plt2";
			AssertNotNull("Should be able to reserve Inventory with same pallet ID on order line when pallet id provided is case-insensitive", orderLine2.ReserveStockIfAbleTo(inventory2));

			orderLine3.WE_PalletID = "";
			AssertNotNull("Should be able to reserve Inventory with pallet ID even when pallet id is not provided on order line", orderLine3.ReserveStockIfAbleTo(inventory3));
		}

		#endregion

		#region TestReserveStockIfAbleTo_DockDoorStock

		public void TestReserveStockIfAbleTo_DockDoorStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation1.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var now = ZDateTimeOffset.Now;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", now);
			var line1InRec1ForPart1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 2m, dockDoorLocation1, "A");
			var line2InRec1ForPart1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 3m, dockDoorLocation1, "A");
			var line3InRec1ForPart2 = Helper.CreateWhsReceiveLine(receive1, data.Part2, 1m, dockDoorLocation1, "B");

			var receive2 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", now);
			var line1InRec2ForPart2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 3m, dockDoorLocation1, "B");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation1, nonDockDoorLocation, "A", 5m);
			transfer.RunPreSaveValidation();
			transferLineForPart1.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals(InventoryStatus.Codes.Received, line1InRec1ForPart1.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, line2InRec1ForPart1.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.PuttingAway, transferLineForPart1.WE_OriginalInventoryStatus);

			AssertEquals(InventoryStatus.Codes.Received, line3InRec1ForPart2.WE_OriginalInventoryStatus);
			AssertEquals(InventoryStatus.Codes.Received, line1InRec2ForPart2.WE_OriginalInventoryStatus);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLineForPart1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLineForPart2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			AssertInventoryCannotBeReserved("Puttingaway inventory must not be able to reserve as the inventory is putting-away.", orderLineForPart1, line1InRec1ForPart1.Inventory[0]);
			AssertInventoryCannotBeReserved("Puttingaway inventory must not be able to reserve as the inventory is putting-away.", orderLineForPart1, line2InRec1ForPart1.Inventory[0]);
			AssertInventoryHasBeenReserved("Received inventory must be able to reserve received Part2.", orderLineForPart2, line3InRec1ForPart2.Inventory[0], 1m);
			AssertInventoryHasBeenReserved("Received inventory must be able to reserve received Part2.", orderLineForPart2, line1InRec2ForPart2.Inventory[0], 1m, 1m);

			transfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(transfer);
			AssertEquals($"Precondition: The InventoryStatus of Transfer Line should be {InventoryStatus.Codes.Putaway}", InventoryStatus.Codes.Putaway, transferLineForPart1.WE_CurrentInventoryStatus);
			AssertEquals($"Precondition: The InventoryStatus of Inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, line1InRec1ForPart1.Inventory[0].WI_InventoryStatus);

			var transferLineForPart1Inv = Factory.Load<WhsInventoryView>(transferLineForPart1.PK);
			AssertInventoryHasBeenReserved("Putaway inventory is on putaway transfer inventory and must be able to reserve.", orderLineForPart1, transferLineForPart1Inv, 2m);
		}

		void AssertInventoryCannotBeReserved(string errorMessage, WhsOrderLine orderLine, WhsInventoryView inventory)
		{
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertNull(errorMessage, reservedPickLine);
		}

		#endregion

		#region TestReserveStockIfAbleTo_WhenDivotAlreadyExists

		public void TestReserveStockIfAbleTo_WhenDivotAlreadyExists()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var existingReservedLine = Factory.New<WhsPickLine>();
			existingReservedLine.IsReserveLine = true;
			existingReservedLine.WZ_WE_InventoryLine = inventory.WI_WE_InDocketLine;
			existingReservedLine.WZ_WE_TransactionLine = orderLine.PK;
			existingReservedLine.ReservedQuantity = 5m;

			var pickLineFromReserve1 = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Should re-use existing Pick Line.", existingReservedLine, pickLineFromReserve1);
			AssertEquals("Should reserve available amount.", 10m, pickLineFromReserve1.WZ_Units);
			pickLineFromReserve1.ReservedQuantity = 5m; // clean-up

			var pickLineFromReserve2 = AssertInventoryHasBeenReserved("If pick line already exists should still be able to reserve.", orderLine, inventory, 6m, quantityToReserve: 6m);
			AssertEquals("Should re-use existing Pick Line.", existingReservedLine, pickLineFromReserve2);
		}

		#endregion

		#region TestReserveStockIfAbleTo_WhenCancellingPick

		public void TestReserveStockIfAbleTo_WhenCancellingPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals(10m, inventory.WI_CrossDockQuantity);
			AssertEquals(10m, reservedPickLine.ReservedQuantity);

			var pick = Helper.CreatePickNew(order);
			AssertEquals(0m, inventory.WI_CrossDockQuantity);
			AssertEquals(10m, reservedPickLine.ReservedQuantity);

			pick.CancelPick();
			AssertEquals(10m, inventory.WI_CrossDockQuantity);
			AssertEquals(10m, reservedPickLine.ReservedQuantity);
		}

		#endregion

		#region TestReserveStockIfAbleTo_WhenCancellingPickAndInventoryIsNotAvailable

		public void TestReserveStockIfAbleTo_WhenCancellingPickAndInventoryIsNotAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var reservedPickLine1 = orderLine1.ReserveStockIfAbleTo(inventory1);
			AssertEquals(10m, inventory1.WI_CrossDockQuantity);
			AssertEquals(10m, reservedPickLine1.ReservedQuantity);
			AssertEquals(0m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);

			var pick1 = Helper.CreatePickNew(order1);
			AssertEquals(0m, inventory1.WI_CrossDockQuantity);
			AssertEquals(10m, reservedPickLine1.ReservedQuantity);
			AssertEquals(10m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);

			pick1.OrderedInventories[0].ClearAllocations();
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var reservedPickLine2 = orderLine2.ReserveStockIfAbleTo(inventory1);
			AssertEquals(10m, inventory1.WI_CrossDockQuantity);
			AssertEquals(10m, reservedPickLine2.ReservedQuantity);
			AssertEquals(0m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);

			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals(0m, inventory1.WI_CrossDockQuantity);
			AssertEquals(10m, reservedPickLine2.ReservedQuantity);
			AssertEquals(10m, inventory1.CommittedQuantityIncludingUnfinalisedReceipt);

			pick1.CancelPick();
			AssertEquals(0m, reservedPickLine1.ReservedQuantity);
			AssertNoErrors(reservedPickLine1);

			pick1.RunPreSaveValidation();
			reservedPickLine1.Validation.ValidateAll();
			AssertNoErrors(reservedPickLine1);

			Factory.Save(); // to make sure no dodgy data is left in the factory
		}

		#endregion

		#region TestReserveStockIfAbleTo_WhenUsingInTransitInventory

		public void TestReserveStockIfAbleTo_WhenUsingInTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, sourceLocation);
			receive.FinaliseDocket();
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, sourceLocation, destinationLocation);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition, created In-Transit Inventory.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var reservedStock = orderLine.ReserveStockIfAbleTo(transferLine.Inventory[0]);
			AssertEquals("Should not be able to reserve In-Transit stock", null, reservedStock);
		}

		#endregion

		#region TestReserveStockIfAbleTo_ClientOrderedUnitsAsReservedQuantity

		public void TestReserveStockIfAbleTo_ClientOrderedUnitsAsReservedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 15m);
			receiveLine.WE_TransactionQuantity = 10m;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Factory.Save();

			var inventory = receiveLine.Inventory[0];
			AssertEquals("Precondition: WI_TotalUnits", 10m, inventory.WI_TotalUnits);
			AssertEquals("Precondition: WI_ExpectedReceiptQuantity", 15m, inventory.WI_ExpectedReceiptQuantity);

			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Reserved pick line's quantity is correct.", 15m, reservedPickLine.WZ_Units);
			AssertEquals("Reserved pick line's reserved quantity is correct.", 15m, reservedPickLine.ReservedQuantity);
			AssertEquals("Reserved quantity on receive line is correct.", 15m, receiveLine.ReservedQuantity);
			AssertEquals("Crossdock quantity on order line is correct.", 15m, orderLine.WE_CrossDockQuantity);
		}

		#endregion

		#region Shortfalls

		#region TestShortfallCalculationGetsFiredWhenUnitsHadBeenZeroOnLoad

		public void TestShortfallCalculationGetsFiredWhenUnitsHadBeenZeroOnLoad()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "BOWLHAT");
			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse, product, 0m);
			Factory.Save();

			var orderLineInNewFactory = new BusinessObjectFactory().Load<WhsOrderLine>(order.Lines[0].PK);
			AssertEquals("Precondition:", 0m, orderLineInNewFactory.WE_TransactionQuantity);
			AssertEquals("Simulate GUI getter", 0m, orderLineInNewFactory.WE_ShortfallQuantityCached);

			orderLineInNewFactory.WE_TransactionQuantity = 1000m;
			AssertNoWarnings(orderLineInNewFactory.WE_ShortfallQuantityCachedInfo);

			orderLineInNewFactory.Validation.ValidateWE_ShortfallQuantityCached();
			AssertHasWarning(orderLineInNewFactory.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 0 unit(s) currently available");
			AssertEquals(1000m, orderLineInNewFactory.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestGetShortfallExistsStatus_AfterPick

		protected override void TestGetShortfallExistsStatus_AfterPickCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			DocketLine.PickableDocket.WD_OH_Client = data.Org1.PK;
			DocketLine.PickableDocket.WD_WW_Whs = data.Whs1.PK;
			DocketLine.PickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			DocketLine.PickableDocket.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			DocketLine.WE_OP = data.Part1.PK;
			DocketLine.WE_TransactionQuantity = 107;
			Factory.Save();

			Helper.CreatePickNew(DocketLine.PickableDocket);
			AssertEquals(true, DocketLine.GetShortfallExistsStatus());

			DocketLine.ReleaseLines[0].Quantity += 7; // allocate another 7
			AssertEquals(false, DocketLine.GetShortfallExistsStatus());

			DocketLine.ReleaseLines[0].Quantity -= 1;  // reduce allocation by 1
			AssertEquals(true, DocketLine.GetShortfallExistsStatus());
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick

		protected override void TestWE_ShortfallQuantityCached_BeforePickCore() => TestWE_ShortfallQuantityCached_BeforePickCore(isAfterSave: true);

		public void TestWE_ShortfallQuantityCached_BeforePick_BeforeSave() => TestWE_ShortfallQuantityCached_BeforePickCore(isAfterSave: false);

		void TestWE_ShortfallQuantityCached_BeforePickCore(bool isAfterSave)
		{
			var inventoryTestData = new TestDataForInventory(Factory);
			inventoryTestData.CreateSimpleInventory(); // only 100 units in inventory

			Factory.Save();

			Docket.WD_OH_Client = inventoryTestData.Org1.PK;
			Docket.WD_WW_Whs = inventoryTestData.Whs1.PK;

			DocketLine.WE_OP = inventoryTestData.Part1.PK;
			DocketLine.WE_TransactionQuantity = 80m;

			WhsOrderLine orderLine2 = Docket.Lines.AddNew();
			orderLine2.WE_OP = inventoryTestData.Part1.PK;
			orderLine2.WE_TransactionQuantity = 50m;

			if (isAfterSave)
			{
				Factory.Save();
				Assert("Precondition", orderLine2.CanCalculateShortfallForAllLinesFromDB);
				Assert("Precondition", orderLine2.IsInDatabase); // Testing CalculateShortfallForUnpickedOrder_AllLinesInOneDbHit()
			}
			else
			{
				AssertEquals("Precondition", false, orderLine2.IsInDatabase); // Testing CalculateShortfallForUnpickedOrder()
			}

			Helper.CreateReservePickLine(DocketLine, inventoryTestData.Line111, 80m); // reserve 80 units in inventory for first line
			AssertEquals("Should be no shortfalls, because units were reserved for this line", 0m, DocketLine.WE_ShortfallQuantityCached);
			AssertEquals("Should be shortfall as only 20 units are available for pick, and 80 are reserved by another order line", 30m, orderLine2.WE_ShortfallQuantityCached);

			if (ShortfallQuantityIsCached)
			{
				DocketLine.WE_TransactionQuantity = 150;
				AssertEquals("For performance reasons, the Shortfall Qty should not be recalculated.", 0m, DocketLine.WE_ShortfallQuantityCached);

				DocketLine.ClearWE_ShortfallQuantityCached();
				AssertEquals("Cache was cleared, Qty should be recalculated.", 50m, DocketLine.WE_ShortfallQuantityCached);
			}
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_HeldInventoryOrder

		public void TestWE_ShortfallQuantityCached_BeforePick_HeldInventoryOrder_AfterSave() => TestWE_ShortfallQuantityCached_BeforePick_HeldInventoryOrder_Core(isAfterSave: true);

		public void TestWE_ShortfallQuantityCached_BeforePick_HeldInventoryOrder_BeforeSave() => TestWE_ShortfallQuantityCached_BeforePick_HeldInventoryOrder_Core(isAfterSave: true);

		void TestWE_ShortfallQuantityCached_BeforePick_HeldInventoryOrder_Core(bool isAfterSave)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held);
				var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultLocation);
				receive.FinaliseDocketWithoutUserConfirmation();
				Assert(receive.IsFinalised);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;

				if (isAfterSave)
				{
					Factory.Save();
					Assert("Precondition", orderLine.CanCalculateShortfallForAllLinesFromDB);
					Assert("Precondition", orderLine.IsInDatabase); // Testing CalculateShortfallForUnpickedOrder_AllLinesInOneDbHit()
				}
				else
				{
					AssertEquals("Precondition", false, orderLine.IsInDatabase); // Testing CalculateShortfallForUnpickedOrder()
				}

				AssertEquals("Should be shortfall as only 90, only the Damaged inventory is pickable", 90m, orderLine.WE_ShortfallQuantityCached);

				if (ShortfallQuantityIsCached)
				{
					orderLine.WE_TransactionQuantity += 2m;
					AssertEquals("For performance reasons, the Shortfall Qty should not be recalculated.", 90m, orderLine.WE_ShortfallQuantityCached);

					orderLine.ClearWE_ShortfallQuantityCached();
					AssertEquals("Cache was cleared, Qty should be recalculated.", 92m, orderLine.WE_ShortfallQuantityCached);
				}
			}
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventory

		public void TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventory_OrderLineInDB_InventoryNotAvailable()
		{
			TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventoryCore(orderLineInDb: true, inventoryAvailable: false);
		}

		public void TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventory_OrderLineInDB_InventoryAvailable()
		{
			TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventoryCore(orderLineInDb: true, inventoryAvailable: true);
		}

		public void TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventory_OrderLineNotInDB_InventoryNotAvailable()
		{
			TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventoryCore(orderLineInDb: false, inventoryAvailable: false);
		}

		public void TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventory_OrderLineNotInDB_InventoryAvailable()
		{
			TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventoryCore(orderLineInDb: false, inventoryAvailable: true);
		}

		void TestWE_ShortfallQuantityCached_BeforePick_WithReservedInventoryCore(bool orderLineInDb, bool inventoryAvailable)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: inventoryAvailable);
			var inventory = receive.Inventory[0];
			Factory.Save();

			if (inventoryAvailable)
			{
				AssertEquals("Precondition: Inventory status is available.", InventoryStatus.Codes.Available, inventory.WI_InventoryStatus);
			}
			else
			{
				AssertNotEquals("Precondition: Inventory status is not available.", InventoryStatus.Codes.Available, inventory.WI_InventoryStatus);
			}

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, inventory, 10m);
			if (orderLineInDb)
			{
				Factory.Save();
			}

			var expectedShortfall = inventoryAvailable ? 0m : 10m;
			AssertEquals("Docket line has reserved quantity.", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Calculated shortfall is correct.", expectedShortfall, orderLine.WE_ShortfallQuantityCached);
		}

		public void TestWE_ShortfallQuantityCached_BeforePick_WithReservedQuantityOnAvailableAndNotAvailableInventories_OrderLineInDB()
		{
			TestWE_ShortfallQuantityCached_BeforePick_WithReservedQuantityOnAvailableAndNotAvailableInventoriesCore(orderLineInDb: true);
		}

		public void TestWE_ShortfallQuantityCached_BeforePick_WithReservedQuantityOnAvailableAndNotAvailableInventories_OrderLineNotInDB()
		{
			TestWE_ShortfallQuantityCached_BeforePick_WithReservedQuantityOnAvailableAndNotAvailableInventoriesCore(orderLineInDb: false);
		}

		void TestWE_ShortfallQuantityCached_BeforePick_WithReservedQuantityOnAvailableAndNotAvailableInventoriesCore(bool orderLineInDb)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, finalise: false);
			var inventory1 = receive1.Inventory[0];
			var inventory2 = receive2.Inventory[0];
			Factory.Save();

			AssertEquals("First inventory is available.", InventoryStatus.Codes.Available, inventory1.WI_InventoryStatus);
			AssertNotEquals("Second inventory is not available.", InventoryStatus.Codes.Available, inventory2.WI_InventoryStatus);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 30m);

			Helper.CreateReservePickLine(orderLine, inventory1, 10m);
			Helper.CreateReservePickLine(orderLine, inventory2, 20m);
			if (orderLineInDb)
			{
				Factory.Save();
			}

			AssertEquals("Reserved quantity is correct.", 30m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Shortfall quantity is correct.", 20m, orderLine.WE_ShortfallQuantityCached);
		}

		public void TestWE_ShortfallQuantityCached_BeforePick_AvailableInventoryPresentButReservedOnOtherOrder_OrderLineInDb()
		{
			TestWE_ShortfallQuantityCached_BeforePick_AvailableInventoryPresentButReservedOnOtherOrderCore(true);
		}

		public void TestWE_ShortfallQuantityCached_BeforePick_AvailableInventoryPresentButReservedOnOtherOrder_OrderLineNotInDb()
		{
			TestWE_ShortfallQuantityCached_BeforePick_AvailableInventoryPresentButReservedOnOtherOrderCore(false);
		}

		void TestWE_ShortfallQuantityCached_BeforePick_AvailableInventoryPresentButReservedOnOtherOrderCore(bool orderLineInDb)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 20m, finalise: false);
			var inventory1 = receive1.Inventory[0];
			var inventory2 = receive2.Inventory[0];
			Factory.Save();

			AssertEquals("First inventory is available.", InventoryStatus.Codes.Available, inventory1.WI_InventoryStatus);
			AssertNotEquals("Second inventory is not available.", InventoryStatus.Codes.Available, inventory2.WI_InventoryStatus);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			Helper.CreateReservePickLine(order1Line, inventory1, 20m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 20m);
			Helper.CreateReservePickLine(order2Line, inventory2, 20m);
			if (orderLineInDb)
			{
				Factory.Save();
			}

			AssertEquals("Reserved quantity is correct.", 20m, order1Line.WE_CrossDockQuantity);
			AssertEquals("There should be no shortfall since it's reserved inventory is available.", 0m, order1Line.WE_ShortfallQuantityCached);

			AssertEquals("Reserved quantity is correct.", 20m, order2Line.WE_CrossDockQuantity);
			AssertEquals("There should be a shortfall quantity since its reserved inventory is not available and the available inventory for the product is reserved to another order.", 20m, order2Line.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_AfterPick

		protected override void TestWE_ShortfallQuantityCached_AfterPickCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			DocketLine.PickableDocket.WD_OH_Client = data.Org1.PK;
			DocketLine.PickableDocket.WD_WW_Whs = data.Whs1.PK;
			DocketLine.PickableDocket.WD_RequiredDate = ZDateTimeOffset.Now;
			DocketLine.PickableDocket.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			DocketLine.WE_OP = data.Part1.PK;
			DocketLine.WE_TransactionQuantity = 107;
			Factory.Save();

			Helper.CreatePickNew(DocketLine.PickableDocket);
			AssertEquals(7m, DocketLine.WE_ShortfallQuantityCached);

			DocketLine.ReleaseLines[0].Quantity += 6; // allocate another 6
			AssertEquals(1m, DocketLine.WE_ShortfallQuantityCached);

			DocketLine.ReleaseLines[0].Quantity += 1; // allocate another 1
			AssertEquals(0m, DocketLine.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestWE_ShortfallQuantityCached_TakesIntoAccountBondedEntryKey

		public void TestWE_ShortfallQuantityCached_TakesIntoAccountBondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "B123-3");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			var orderInDB = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			orderInDB.WD_DocketStatus = OrderType.Codes.Customs;
			var orderLineInDB1 = Helper.CreateWhsOrderLine(orderInDB, data.Part1, 5m, "B123-4", "DummyOutward-4", "");
			var orderLineInDB2 = Helper.CreateWhsOrderLine(orderInDB, data.Part1, 5m);
			var orderLineInDB3 = Helper.CreateWhsOrderLine(orderInDB, data.Part1, 5m, "B123-3", "DummyOutward-3", "");
			Factory.Save();

			var orderNotInDB = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			orderNotInDB.WD_DocketStatus = OrderType.Codes.Customs;
			var orderLineNotInDB1 = Helper.CreateWhsOrderLine(orderNotInDB, data.Part1, 5m, "B123-4", "DummyOutward-4", "");
			var orderLineNotInDB2 = Helper.CreateWhsOrderLine(orderNotInDB, data.Part1, 5m);
			var orderLineNotInDB3 = Helper.CreateWhsOrderLine(orderNotInDB, data.Part1, 5m, "B123-3", "DummyOutward-3", "");

			CombineAssertions(delegate
			{
				AssertEquals("In DB: Different Entry Key, should *not* match stock.", 5m, orderLineInDB1.WE_ShortfallQuantityCached);
				AssertEquals("In DB: No Entry Key, should match stock.", 0m, orderLineInDB2.WE_ShortfallQuantityCached);
				AssertEquals("In DB: Same Entry Key, should match stock.", 0m, orderLineInDB3.WE_ShortfallQuantityCached);
				AssertEquals("Not In DB: Different Entry Key, should *not* match stock.", 5m, orderLineNotInDB1.WE_ShortfallQuantityCached);
				AssertEquals("Not In DB: No Entry Key, should match stock.", 0m, orderLineNotInDB2.WE_ShortfallQuantityCached);
				AssertEquals("Not In DB: Same Entry Key, should match stock.", 0m, orderLineNotInDB3.WE_ShortfallQuantityCached);
			});
		}

		#endregion

		#region TestSuspendShortfallCalculationCore()

		protected override void TestSuspendShortfallCalculationCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 150);  //shortfall of 50 units

			Factory.Save();

			AssertEquals("Pre-condition", false, line.Shortfall.IsShortfallCalculationSuspended);

			line.Shortfall.SuspendShortfallCalculation();
			AssertEquals("Shortfall calculation is suspended.", 0m, line.WE_ShortfallQuantityCached);
			AssertEquals("Cached value should not be updated if calculation is suspended", false, line.GetShortfallCacheValueForTest().HasValue);
			AssertEquals(true, line.Shortfall.IsShortfallCalculationSuspended);

			line.Shortfall.ResumeShortfallCalculation();
			AssertEquals("Shortfall calculation is resumed.", 50m, line.WE_ShortfallQuantityCached);
			AssertEquals(false, line.Shortfall.IsShortfallCalculationSuspended);

			line.Shortfall.SuspendShortfallCalculation();
			AssertEquals("Shortfall calculation is suspended but value has been cached.", 50m, line.WE_ShortfallQuantityCached);
			AssertEquals(true, line.Shortfall.IsShortfallCalculationSuspended);

			line.GetShortfallExistsStatus();
			AssertEquals("Shortfall calculation suspended so cached value should not be cleared.", 50m, line.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestShortfallQuantity_HeldInventory

		public void TestShortfallQuantity_HeldInventory_HEL() => TestShortfallQuantity_HeldInventory_Core(InventoryHoldCodes.Codes.Held, InventoryHoldCodes.Codes.Damaged);
		public void TestShortfallQuantity_HeldInventory_DAM() => TestShortfallQuantity_HeldInventory_Core(InventoryHoldCodes.Codes.Damaged, InventoryHoldCodes.Codes.Held);

		void TestShortfallQuantity_HeldInventory_Core(string heldCode, string otherHeldCode)
		{
			DocketLine.PickableDocket.Delete();
			DocketLine.Delete();

			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Factory, 6, 1);
				var location = data.Whs1.FindLocation("A-1");

				var hldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "HLDREC1", Notify);
				Helper.CreateWhsReceiveLine(hldReceive, data.Part1, 30m, location, "", InventoryStatus.Codes.Held, heldCode);
				Helper.CreateWhsReceiveLine(hldReceive, data.Part1, 1000m, location, "", InventoryStatus.Codes.Held, otherHeldCode);
				hldReceive.WD_ArrivalDate = ZDateTimeOffset.Now;
				hldReceive.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 37m);
				orderLine.WE_WHC_NKOrderedHeldCode = heldCode;

				Helper.CreatePickNew(order);
				
				AssertEquals(true, orderLine.GetShortfallExistsStatus());

				orderLine.ReleaseLines[0].Quantity += 7;
				AssertEquals(false, orderLine.GetShortfallExistsStatus());

				orderLine.ReleaseLines[0].Quantity -= 1;
				AssertEquals(true, orderLine.GetShortfallExistsStatus());
			}
		}

		#endregion

		#region TestChildLinesAreReadOnly

		public void TestChildLinesAreReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			Helper.CreatePickNew(orderLine.PickableDocket);
			AssertEquals("1 Child Lines should be created", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Child Lines should be readonly", true, orderLine.ChildComponentLines.ElementAt(0).ReadOnly);
		}

		#endregion

		#region TestIsComponentPickedOnBOMOrder

		public void TestIsComponentPickedOnBOMOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			Helper.CreatePickNew(orderLine.PickableDocket);
			AssertEquals("1 Child Lines should be created", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("IsComponentPickedOnBOMOrder should be true", true, orderLine.IsBOMProductPickedOnSalesOrder);
			AssertEquals("IsComponentPickedOnBOMOrder should be false for child line", false, orderLine.ChildComponentLines.ElementAt(0).IsBOMProductPickedOnSalesOrder);
		}

		#endregion

		#region TestShortFallWithoutConsideringComponents

		public void TestShortFallWithoutConsideringComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			AssertEquals("ShortFallWithoutConsideringComponents should be 10", 10m, orderLine.GetShortfallWithoutConsideringComponents());
			AssertEquals("WE_ShortfallQuantityCached should be 4", 4m, orderLine.WE_ShortfallQuantityCached);
		}

		public void TestShortFallWithoutConsideringComponents_HeldInventory()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
				mainProduct.OP_IsComponentPickedOnSalesOrder = true;
				var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
				var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 100m, Constants.PkgUnit.Unit);
				var inventoryLocation = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveLine(receive, mainProduct, 5m, inventoryLocation, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				Helper.CreateWhsReceiveLine(receive, mainProduct, 20m, inventoryLocation, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
				Helper.CreateWhsReceiveLine(receive, mainProduct, 20m, inventoryLocation, "", InventoryStatus.Codes.Available);
				Helper.CreateWhsReceiveLine(receive, bomComponentProduct, 100m, inventoryLocation, "", InventoryStatus.Codes.Available);
				Helper.CreateWhsReceiveLine(receive, bomComponentProduct, 200m, inventoryLocation, InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
				var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				Factory.Save();

				AssertEquals(10m, orderLine.GetShortfallWithoutConsideringComponents());
				AssertEquals("Baseline: Held Order currently includes Available components", 9m, orderLine.WE_ShortfallQuantityCached); // Will be fixed in WI00795397
			}
		}

		public void TestShortFallWithoutConsideringComponents_WithPartAttrib1()
		{
			TestShortFallWithoutConsideringComponents_WithPartAttribCore(AttributeNumber.One, (line) => line.WE_PartAttrib1 = "Attrib1");
		}
		public void TestShortFallWithoutConsideringComponents_WithPartAttrib1_NoPartAttributeOnOrder()
		{
			TestShortFallWithoutConsideringComponents_WithPartAttribCore(AttributeNumber.One, (line) => line.WE_PartAttrib1 = "Attrib1", setAttributeOnOrderLine: false);
		}

		public void TestShortFallWithoutConsideringComponents_WithPartAttrib2()
		{
			TestShortFallWithoutConsideringComponents_WithPartAttribCore(AttributeNumber.Two, (line) => line.WE_PartAttrib2 = "Attrib2");
		}

		public void TestShortFallWithoutConsideringComponents_WithPartAttrib2_NoPartAttributeOnOrder()
		{
			TestShortFallWithoutConsideringComponents_WithPartAttribCore(AttributeNumber.Two, (line) => line.WE_PartAttrib2 = "Attrib2", setAttributeOnOrderLine: false);
		}

		public void TestShortFallWithoutConsideringComponents_WithPartAttrib3()
		{
			TestShortFallWithoutConsideringComponents_WithPartAttribCore(AttributeNumber.Three, (line) => line.WE_PartAttrib3 = "Attrib3");
		}

		public void TestShortFallWithoutConsideringComponents_WithPartAttrib3_NoPartAttributeOnOrder()
		{
			TestShortFallWithoutConsideringComponents_WithPartAttribCore(AttributeNumber.Three, (line) => line.WE_PartAttrib3 = "Attrib3", setAttributeOnOrderLine: false);
		}

		void TestShortFallWithoutConsideringComponents_WithPartAttribCore(AttributeNumber attributeNumber, Action<WhsDocketLine> attributeSetter, bool setAttributeOnOrderLine = true)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, true);

			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			Helper.SetProductAttributeUse(data.Org1, mainProduct, attributeNumber, true);

			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLineForMainProduct = Helper.CreateWhsReceiveLine(receive, mainProduct, 5m, inventoryLocation);
			attributeSetter(receiveLineForMainProduct);
			Helper.CreateWhsReceiveLine(receive, bomComponentProduct, 20m, inventoryLocation);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			if (setAttributeOnOrderLine)
			{
				attributeSetter(receiveLineForMainProduct);
			}
			Factory.Save();

			AssertEquals("ShortFallWithoutConsideringComponents should be correct", 10m, orderLine.GetShortfallWithoutConsideringComponents());
			AssertEquals("WE_ShortfallQuantityCached should be 4", 4m, orderLine.WE_ShortfallQuantityCached);
		}

		public void TestShortFallWithoutConsideringComponents_WithBondedEntryKey()
		{
			TestShortFallWithoutConsideringComponents_WithBondedEntryKeyCore(assignBondedEntryKeyOnOrder: true);
		}

		public void TestShortFallWithoutConsideringComponents_WithBondedEntryKey_NoBondedEntryKeyOnOrder()
		{
			TestShortFallWithoutConsideringComponents_WithBondedEntryKeyCore(assignBondedEntryKeyOnOrder: false);
		}

		void TestShortFallWithoutConsideringComponents_WithBondedEntryKeyCore(bool assignBondedEntryKeyOnOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;

			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLineForMainProduct = Helper.CreateWhsReceiveLine(receive, mainProduct, 5m);
			receiveLineForMainProduct.WE_BondedEntryKey = "KEY-1";
			var customsDataMainProduct = receiveLineForMainProduct.CustomsData;
			customsDataMainProduct.WB_EntryKey = "KEY";
			customsDataMainProduct.WB_EntryLineNo = (ZShort)1;

			var receiveLineForComponentProduct = Helper.CreateWhsReceiveLine(receive, bomComponentProduct, 20m);
			receiveLineForComponentProduct.WE_BondedEntryKey = "KEY-1";
			var customsDataComponentProduct = receiveLineForComponentProduct.CustomsData;
			customsDataComponentProduct.WB_EntryKey = "KEY";
			customsDataComponentProduct.WB_EntryLineNo = (ZShort)1;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			if (assignBondedEntryKeyOnOrder)
			{
				orderLine.WE_BondedEntryKey = "KEY-1";
			}
			Factory.Save();

			AssertEquals("ShortFallWithoutConsideringComponents should be correct", 10m, orderLine.GetShortfallWithoutConsideringComponents());
			AssertEquals("WE_ShortfallQuantityCached should be 4", 4m, orderLine.WE_ShortfallQuantityCached);
		}

		public void TestShortFallWithoutConsideringComponents_WithPalletID()
		{
			TestShortFallWithoutConsideringComponents_PalletIDCore(assignPalletIDOnOrder: true);
		}

		public void TestShortFallWithoutConsideringComponents_NoPalletIDOnOrderLine()
		{
			TestShortFallWithoutConsideringComponents_PalletIDCore(assignPalletIDOnOrder: false);
		}

		void TestShortFallWithoutConsideringComponents_PalletIDCore(bool assignPalletIDOnOrder)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "ABC");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "DEF");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part2, 2m);
			var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			if (assignPalletIDOnOrder)
			{
				orderLine1.WE_PalletID = "ABC";
				orderLine3.WE_PalletID = "XXX";
				orderLine4.WE_PalletID = "DEF";
				orderLine5.WE_PalletID = "DEF";
			}

			AssertEquals("Full ordered quantity is available, should have no shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("No PalletID on order line, should have no shortfall.", 0m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("PalletID on order line does not match inventory, should have shortfall.", assignPalletIDOnOrder ? 1m : 0m, orderLine3.WE_ShortfallQuantityCached);
			AssertEquals("Full ordered quantity is available, should have no shortfall.", 0m, orderLine4.WE_ShortfallQuantityCached);
			AssertEquals("Ordered more than available amount, should have shortfall.", 2m, orderLine5.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestShortfallCalculationWithBOMBeforePick

		public void TestShortfallCalculationWithBOMBeforePick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 5m);
			Factory.Save();

			orderLine.WE_TransactionQuantity = 5m;
			orderLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(0m, orderLine.WE_ShortfallQuantityCached);
			AssertNoWarnings(orderLine.WE_ShortfallQuantityCachedInfo);

			orderLine.WE_TransactionQuantity = 11m;
			orderLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(0m, orderLine.WE_ShortfallQuantityCached);
			AssertNoWarnings(orderLine.WE_ShortfallQuantityCachedInfo);

			orderLine.WE_TransactionQuantity = 15m;
			orderLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals(4m, orderLine.WE_ShortfallQuantityCached);
			AssertHasWarning(orderLine.WE_ShortfallQuantityCachedInfo, "Shortfall: Only " + string.Format("{0:0.##}", 11) + " unit(s) currently available");
		}

		#endregion

		#region TestGetShortfallWithoutConsideringComponents_DBHits

		public void TestGetShortfallWithoutConsideringComponents_DBHits()
		{
			const short numberOfInventoriesToCreate = 202; // 101 main docket lines
			var data = new TestDataSimpleEnvironment(Factory, numberOfInventoriesToCreate, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");

			for (var i = 0; i < numberOfInventoriesToCreate / 2; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R{i.ToString()}");
				var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m);
				var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			}

			Factory.Save();

			var orderFactory = new BusinessObjectFactory();
			var helper_orderFactory = new WhsTestHelperFunctions(orderFactory);
			var order = helper_orderFactory.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper_orderFactory.CreateWhsOrderLine(order, mainProduct, 15m);
			orderFactory.ResetDatabaseLoadCount();

			AssertEquals("ShortFallWithoutConsideringComponents should be 0", 0m, orderLine.GetShortfallWithoutConsideringComponents());

			var expectedDBHits = new Dictionary<string, int>();
			expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, 3); // fetches in lots of 100, but shortfall calculation short shortcircuits and doesn't load the second batch of 100 (in this case just 1)
			expectedDBHits.Add(WhsLocationViewSchema.Constants.TableName, 2);
			expectedDBHits.Add(WhsDocketLineSchema.Constants.TableName, 2);

			AssertDbHits(expectedDBHits, orderFactory);
		}

		#endregion

		#region TestGetShortfallWithoutConsideringComponents_DBHits_WithShortfall

		public void TestGetShortfallWithoutConsideringComponents_DBHits_WithShortfall()
		{
			const short numberOfInventoriesToCreate = 202; // 101 main docket lines
			var data = new TestDataSimpleEnvironment(Factory, numberOfInventoriesToCreate, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);
			var inventoryLocation = data.Whs1.FindLocation("A-1");

			for (var i = 0; i < numberOfInventoriesToCreate / 2; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R{i.ToString()}");
				var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 1m);
				var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m);
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			}

			Factory.Save();

			var orderFactory = new BusinessObjectFactory();
			var helper_orderFactory = new WhsTestHelperFunctions(orderFactory);
			var order = helper_orderFactory.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper_orderFactory.CreateWhsOrderLine(order, mainProduct, 150m);
			orderFactory.ResetDatabaseLoadCount();

			AssertEquals("ShortFallWithoutConsideringComponents should be 150 - 101x1 = 49", 49m, orderLine.GetShortfallWithoutConsideringComponents());

			var expectedDBHits = new Dictionary<string, int>();
			expectedDBHits.Add(WhsInventoryViewSchema.Constants.TableName, 1);
			expectedDBHits.Add(WhsPickLineSchema.Constants.TableName, 3); // fetches in lots of 100, and shortfall calculation didn't get to short shortcircuit, so did fetch second batch of 100
			expectedDBHits.Add(WhsLocationViewSchema.Constants.TableName, 2);
			expectedDBHits.Add(WhsDocketLineSchema.Constants.TableName, 2);

			AssertDbHits(expectedDBHits, orderFactory);
		}

		#endregion

		#region TestCalculateShortfallWithAttribute

		public void TestCalculateShortfallWithAttribute_PartAttrib1()
		{
			TestCalculateShortfallWithAttribute_Core(AttributeNumber.One, (docketline) => docketline.WE_PartAttrib1 = "AT1");
		}

		public void TestCalculateShortfallWithAttribute_PartAttrib2()
		{
			TestCalculateShortfallWithAttribute_Core(AttributeNumber.Two, (docketline) => docketline.WE_PartAttrib2 = "AT2");
		}

		public void TestCalculateShortfallWithAttribute_PartAttrib3()
		{
			TestCalculateShortfallWithAttribute_Core(AttributeNumber.One, (docketline) => docketline.WE_PartAttrib3 = "AT3");
		}

		public void TestCalculateShortfallWithAttribute_ExpiryDate()
		{
			TestCalculateShortfallWithAttribute_Core(AttributeNumber.ExpiryDate, (docketline) => docketline.WE_ExpiryDate = ZDate.Today);
		}

		public void TestCalculateShortfallWithAttribute_PackingDate()
		{
			TestCalculateShortfallWithAttribute_Core(AttributeNumber.PackingDate, (docketline) => docketline.WE_PackingDate = ZDate.Today);
		}

		public void TestCalculateShortfallWithAttribute_Core(AttributeNumber attributeNumber, Action<WhsDocketLine> setAttribute)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attributeNumber, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeNumber, false);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO1", data.Part1, 1m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO2", data.Part1, 5m);
			setAttribute(receive.Lines[0]);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5);
			setAttribute(orderLine2);

			AssertEquals("For now just we group lines by attributes.(this test can change when we have better calculation for shortfall.)", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("For now just we group lines by attributes.(this test can change when we have better calculation for shortfall.)", 0m, orderLine2.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestCalculateShortfallWithOrderedSerialNumber

		public void TestCalculateShortfallWithOrderedSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN2", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN2";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine3.WE_SerialNumber = "XXX";
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part2, 8m);
			AssertEquals("Ordered full available amount of serial SN2, should have no shortfall.", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("Did not specify serial, should have no shortfall.", 0m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("Ordered non existant serial, should have shortfall.", 1m, orderLine3.WE_ShortfallQuantityCached);
			AssertEquals("Ordered less than available amount, should have no shortfall.", 0m, orderLine4.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestCalculateShortfall_GroupProducts_BondedEntryKey

		public void TestCalculateShortfall_GroupProducts_BondedEntryKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO1", data.Part1, 1m, "B123-3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO2", data.Part1, 1m, "B123-3").Lines[0].WE_PartAttrib1 = "AT1";
			Factory.Save();

			var orderInDB = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			orderInDB.WD_DocketStatus = OrderType.Codes.Customs;
			Helper.CreateWhsOrderLine(orderInDB, data.Part1, 1m, "B123-3", "DummyOutward-3", "");
			Helper.CreateWhsOrderLine(orderInDB, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "AT1", "", "", "B123-3", "DummyOutward-3");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1, "B123-3", "DummyOutward-3", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2, "B123-3", "DummyOutward-3", "");
			orderLine2.WE_PartAttrib1 = "AT1";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 3, "B123-4", "DummyOutward-4", "");
			orderLine3.WE_PartAttrib1 = "AT1";

			AssertEquals("For now just we group lines by attributes.(this test can change when we have better calculation for shortfall.)", 0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals("For now just we group lines by attributes.(this test can change when we have better calculation for shortfall.)", 1m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals("For now just we group lines by attributes.(this test can change when we have better calculation for shortfall.)", 3m, orderLine3.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestCalculateShortfall_GroupProducts_ByAttribute

		public void TestCalculateShortfall_GroupProducts_ByAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, false);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO1", data.Part1, 1m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO2", data.Part1, 5m).Lines[0].WE_PartAttrib1 = "AAA";
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO3", data.Part1, 5m).Lines[0].WE_PartAttrib1 = "BBB";
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 7);
			orderLine2.WE_PartAttrib1 = "AAA";
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 8);
			orderLine3.WE_PartAttrib1 = "BBB";

			AssertEquals(0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals(2m, orderLine2.WE_ShortfallQuantityCached);
			AssertEquals(3m, orderLine3.WE_ShortfallQuantityCached);
		}

		#endregion

		#region TestCalculateShortfall_GroupProducts_NoAttribute

		public void TestCalculateShortfall_GroupProducts_NoAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RO1", data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1);

			AssertEquals(0m, orderLine1.WE_ShortfallQuantityCached);
			AssertEquals(1m, orderLine2.WE_ShortfallQuantityCached);
		}

		#endregion

		#endregion

		#region TestWhsOrderLineToBeClearedLater

		public void TestWhsOrderLineToBeClearedLater()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Precondition", ZBool.False, orderLine1.WhsOrderLineToBeClearedLater);
			AssertEquals("Precondition", ZBool.False, orderLine2.WhsOrderLineToBeClearedLater);
			AssertEquals("Should not have created any GenAddOnColumn rows.", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)).Length);

			orderLine1.WhsOrderLineToBeClearedLater = ZBool.False;
			AssertEquals("Should be set.", ZBool.False, orderLine1.WhsOrderLineToBeClearedLater);
			AssertEquals("Should not be set.", ZBool.False, orderLine2.WhsOrderLineToBeClearedLater);

			orderLine2.WhsOrderLineToBeClearedLater = ZBool.True;
			AssertEquals("Should *not* change.", ZBool.False, orderLine1.WhsOrderLineToBeClearedLater);
			AssertEquals("Should be set.", ZBool.True, orderLine2.WhsOrderLineToBeClearedLater);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var whsOrderLine1_InOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine1.PK);
			var whsOrderLine2_InOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Should persist.", ZBool.False, whsOrderLine1_InOtherFactory.WhsOrderLineToBeClearedLater);
			AssertEquals("Should persist.", ZBool.True, whsOrderLine2_InOtherFactory.WhsOrderLineToBeClearedLater);
		}

		#endregion

		#region TestCustomsClearingInProgress

		public void TestCustomsClearingInProgress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Precondition", ZBool.False, orderLine1.CustomsClearingInProgress);
			AssertEquals("Precondition", ZBool.False, orderLine2.CustomsClearingInProgress);
			AssertEquals("Should not have created any GenAddOnColumn rows.", 0, Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_ParentTableCode, WhsDocketLineSchema.Constants.Prefix)).Length);

			orderLine1.CustomsClearingInProgress = ZBool.False;
			AssertEquals("Should be set.", ZBool.False, orderLine1.CustomsClearingInProgress);
			AssertEquals("Should not be set.", ZBool.False, orderLine2.CustomsClearingInProgress);

			orderLine2.CustomsClearingInProgress = ZBool.True;
			AssertEquals("Should *not* change.", ZBool.False, orderLine1.CustomsClearingInProgress);
			AssertEquals("Should be set.", ZBool.True, orderLine2.CustomsClearingInProgress);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var whsOrderLine1_InOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine1.PK);
			var whsOrderLine2_InOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertEquals("Should persist.", ZBool.False, whsOrderLine1_InOtherFactory.CustomsClearingInProgress);
			AssertEquals("Should persist.", ZBool.True, whsOrderLine2_InOtherFactory.CustomsClearingInProgress);
		}

		#endregion

		#region Delete

		#region TestCanDelete

		public void TestCanDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Line is not in DB, should be able to delete.", true, line.CanDelete);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("The Order is picked, user should be able to delete lines.", true, line.CanDelete);

			Factory.Save();
			AssertEquals("Line is in the DB, should not be able to delete (instead units will be reduced to 0).", false, line.CanDelete);

			var unsavedLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Line is not in DB, should be able to delete.", true, unsavedLine.CanDelete);
		}

		#endregion

		#region TestOnCannotDelete_IsMultiOrderPick_ChangeUnits

		public void TestOnCannotDelete_IsMultiOrderPick_ChangeUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);
			((ICanDelete)orderLine1).OnCannotDelete();
			AssertEquals("Pick is multi order, Should not set to zero", 10m, orderLine1.WE_TransactionQuantity);

			pick.Orders.Remove(order2);
			AssertEquals("Precondition", false, pick.IsMultiOrderPick);
			((ICanDelete)orderLine1).OnCannotDelete();
			AssertEquals("Pick is not multi order, Should set units to zero", 0m, orderLine1.WE_TransactionQuantity);
		}

		#endregion

		#region TestReasonForNotAbleToDelete_IsOnMultiOrderPick

		public void TestReasonForNotAbleToDelete_IsOnMultiOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();
			AssertEquals("Precondition", true, orderLine.ReasonForNotAbleToDelete.IsEmpty);

			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocket();
			AssertEquals("Precondition, pick is not multi pick", false, orderLine.Order.Pick.IsMultiOrderPick);
			AssertEquals("pick is not multi pick still should be able to delete the line.", true, orderLine.ReasonForNotAbleToDelete.IsEmpty);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			pick.Orders.Add(order2);
			AssertEquals("Precondition - pick is multi order.", true, orderLine.Order.Pick.IsMultiOrderPick);
			AssertEquals("pick is not multi pick should not be able to delete the line.", "This order is attached to multi-order pick, no changes to ordered content is allowed.", orderLine.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestCanDelete_PartiallyPickedLines

		public void TestCanDelete_PickedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Precondition", true, orderLine.CanDelete);

			var pick = Factory.New<WhsPick>(); // Avoid helper as it calls methods which will save to the db
			order.WD_WP = pick.PK;
			AssertEquals("Precondition.", true, orderLine.CanDelete);

			var pickLine = Helper.CreateWhsPickLine(orderLine, receive.Inventory[0], 10m);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Line is picked, should not be able to delete.", false, orderLine.CanDelete);

			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", true, orderLine.CanDelete);

			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;
			AssertEquals("Line is picked from PutawayLocation, should not be able to delete.", false, orderLine.CanDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete_PickedLines

		public void TestReasonForNotAbleToDelete_PickedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Precondition", true, string.IsNullOrEmpty(orderLine.ReasonForNotAbleToDelete));

			var pick = Factory.New<WhsPick>(); // Avoid helper as it calls methods which will save to the db
			order.WD_WP = pick.PK;
			AssertEquals("Precondition.", true, orderLine.CanDelete);

			var pickLine = Helper.CreateWhsPickLine(orderLine, receive.Inventory[0], 10m);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("This line has been partially or fully picked, it cannot be deleted.", orderLine.ReasonForNotAbleToDelete);

			order.WD_WP = ZGuid.Empty;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition", true, string.IsNullOrEmpty(orderLine.ReasonForNotAbleToDelete));

			pickLine.WZ_WE_OriginalPickedInventoryLine = receive.Lines[0].PK;
			AssertEquals("This line has been partially or fully picked, it cannot be deleted.", orderLine.ReasonForNotAbleToDelete);
		}

		#endregion

		#region TestReasonForNotAbleToDelete_PickedFTZCustomsLines

		public void TestReasonForNotAbleToDelete_PickedFTZCustomsLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, whs, "O1");
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();
			AssertEquals("Precondition", true, string.IsNullOrEmpty(orderLine.ReasonForNotAbleToDelete));

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				var pick = Helper.CreatePickNew(order);
				AssertEquals("Reason to Delete should be correct.",
					"This order is allocated to a Pick for an FTZ Warehouse in a Country/Region that uses Permits, it cannot be deleted.", orderLine.ReasonForNotAbleToDelete);
			}
		}

		#endregion

		#endregion

		#region Find Attributes

		protected override ErrorType CannotUseChosenInventoryError => OrderErrorTypes.CannotPerformThisOperationBecauseOrderIsFinalisedOrCancelledOrPicked;

		#endregion

		#region TestPackTypeDefaultedFromProduct

		public void TestPackTypeDefaultedFromProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Docket.WD_OH_Client = data.Org1.PK;
			Docket.WD_WW_Whs = data.Whs1.PK;
			object createLine = DocketLine;

			WhsProduct product = WhsProduct.GetWhsProduct(data.Part1);
			WhsProductParamsByWhsAndClient productParams = product.ParamsByWhsAndClient.AddNew();
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_F3_NKReleasedPackType = "BAG";
			productParams.W3_F3_NKReceivedPackType = "BOX";

			DocketLine.WE_OP = data.Part2.PK;
			AssertEquals("UNT", DocketLine.WE_F3_NKPackType);

			DocketLine.WE_OP = data.Part1.PK;
			AssertEquals("Pack Type should be defaulted from W3_F3_NKReleasedPackType.", "BAG", DocketLine.WE_F3_NKPackType);
		}

		#endregion

		#region TestBreakingProductAfterOrderFinalisationWillNotMakeValidationErrors

		[ExpectNoExceptions]
		public void TestBreakingProductAfterOrderFinalisationWillNotMakeValidationErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
			Factory.Save();

			// now let's change product's owner
			var org2 = Helper.CreateClient("C2");
			data.Part1.RelatedOrganisations[0].OU_OH = org2.PK;

			orderLine.Validation.ValidateAll();
			AssertNoErrors("Should be no validation errors, even with a broken product.", orderLine);

			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Cannot change or delete the Owner of a Product with existing transactions.", true), "Should not be able to save due to the prohibited product ownership relation change");
		}

		#endregion

		#region WhsOrderLineWrapperStrategy

		public void TestBuildWrapperStrategyIsWeb()
		{
			// Arrange
			var originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;

				// Action
				var wrapper = GetNewBusinessObject().WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && !(wrapper is WhsOrderLine));
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		public void TestBuildWrapperStrategyDefault()
		{
			// Arrange
			var originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = false;

				// Action
				var wrapper = GetNewBusinessObject().WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && wrapper is WhsOrderLine);
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		#endregion

		#region ApplyPermitResponse

		#region TestApplyPermitResponseIsNotAllowedNull

		public void TestApplyPermitResponseIsNotAllowedNull()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: permit", () => { orderLine.ApplyPermitResponse(null); });
		}

		#endregion

		#region TestApplyPermitResponse_Success

		public void TestApplyPermitResponse_Success()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			AssertEquals("Precondition", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("Precondition", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("Precondition", orderLine);

			var permit = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.Success,
				OutwardEntryNumber = "001",
				OutwardEntryLineNumber = 10
			};
			orderLine.ApplyPermitResponse(permit);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("OrderLine Has No Warning.", orderLine);
		}

		#endregion

		#region TestApplyPermitResponse_Failure

		[TestDate(2017, 9, 4)]
		public void TestApplyPermitResponse_Failure_DetailedTrackingEnabled()
		{
			TestApplyPermitResponse_Failure_Core(true, "Tariff:9999.99.9999, Country of Origin: XX, Zone Status: X, Manufacturer: org code");
		}

		[TestDate(2017, 9, 4)]
		public void TestApplyPermitResponse_Failure_DetailedTrackingDisabled()
		{
			TestApplyPermitResponse_Failure_Core(false, "Tariff:9999.99.9999");
		}

		void TestApplyPermitResponse_Failure_Core(bool isDetailedTrackingEnabled, string failureReason)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS(isDetailedTrackingEnabled: isDetailedTrackingEnabled);
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			AssertEquals("Precondition", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("Precondition", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("Precondition", orderLine);

			var permit = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.Failure,
				FailureReason = failureReason
			};
			orderLine.ApplyPermitResponse(permit);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertHasRowWarning("OrderLine Has Warning.", orderLine, $"No weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, {failureReason}).");
		}

		#endregion

		#region TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits

		[TestDate(2017, 9, 4)]
		public void TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits_DetailedTrackingEnabled()
		{
			TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits_Core(true, "Tariff:9999.99.9999, Country of Origin: XX, Zone Status: X, Manufacturer: org code");
		}

		[TestDate(2017, 9, 4)]
		public void TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits_DetailedTrackingDisabled()
		{
			TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits_Core(false, "Tariff:9999.99.9999");
		}

		void TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits_Core(bool isDetailedTrackingEnabled, string failureReason)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS(isDetailedTrackingEnabled: isDetailedTrackingEnabled);
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			AssertEquals("Precondition", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("Precondition", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("Precondition", orderLine);

			var permit = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits,
				FailureReason = failureReason
			};
			orderLine.ApplyPermitResponse(permit);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertHasRowWarning("OrderLine Has Warning.", orderLine, $"No weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, {failureReason}). However the line can be fulfilled across weekly estimates.");
		}

		#endregion

		#region TestApplyPermitResponse_Failure_ThenSetSuccess

		[TestDate(2017, 9, 4)]
		public void TestApplyPermitResponse_Failure_ThenSetSuccess()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			AssertEquals("Precondition", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("Precondition", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("Precondition", orderLine);

			var permitFail = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.Failure,
				FailureReason = "Tariff:9999.99.9999, Country of Origin: XX, Zone Status: X, Manufacturer: org code"
			};
			orderLine.ApplyPermitResponse(permitFail);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertHasRowWarning("OrderLine Has Warning.", orderLine, "No weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, Tariff:9999.99.9999, Country of Origin: XX, Zone Status: X, Manufacturer: org code).");

			var permit = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.Success,
				OutwardEntryNumber = "001",
				OutwardEntryLineNumber = 10
			};
			orderLine.ApplyPermitResponse(permit);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("OrderLine Has No Warning.", orderLine);
		}

		#endregion

		#region TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits_ThenSetSuccess

		[TestDate(2017, 9, 4)]
		public void TestApplyPermitResponse_FailureButCanBeFulfilledByMultiplePermits_ThenSetSuccess()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			AssertEquals("Precondition", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("Precondition", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("Precondition", orderLine);

			var permitFail = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits,
				FailureReason = "Tariff:9999.99.9999, Country of Origin: XX, Zone Status: X, Manufacturer: org code"
			};
			orderLine.ApplyPermitResponse(permitFail);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertHasRowWarning("OrderLine Has Warning.", orderLine, "No weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, Tariff:9999.99.9999, Country of Origin: XX, Zone Status: X, Manufacturer: org code). However the line can be fulfilled across weekly estimates.");

			var permit = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.Success,
				OutwardEntryNumber = "001",
				OutwardEntryLineNumber = 10
			};
			orderLine.ApplyPermitResponse(permit);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("OrderLine Has No Warning.", orderLine);
		}

		[TestDate(2017, 9, 4)]
		public void TestApplyPermitResponse_OutwardEntryNumberCanBeNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			AssertEquals("Precondition", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("Precondition", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertNoRowWarnings("Precondition", orderLine);

			var permitFail = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.FailureButCanBeFulfilledByMultiplePermits
			};
			orderLine.ApplyPermitResponse(permitFail);

			AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			AssertHasRowWarning("OrderLine Has Warning.", orderLine, "No weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, ). However the line can be fulfilled across weekly estimates.");

			var permit = new PermitWithdrawRequestResponseForTest
			{
				Request = new WhsPermitWithdrawRequest(orderLine),
				SuccessOrFailure = SuccessOrFailure.Success,
				OutwardEntryNumber = null,
				OutwardEntryLineNumber = null
			};
			AssertNoExceptionThrown(() => orderLine.ApplyPermitResponse(permit));
		}

		#endregion

		#region PermitWithdrawRequestResponseForTest

		class PermitWithdrawRequestResponseForTest : IPermitWithdrawRequestResponse
		{
			public IPermitWithdrawRequest Request { get; set; }

			public SuccessOrFailure SuccessOrFailure { get; set; }

			public ZString FailureReason { get; set; }

			public ZDecimal? AvailableQty { get; set; }

			public ZString? OutwardEntryNumber { get; set; }

			public ZShort? OutwardEntryLineNumber { get; set; }
		}

		#endregion

		#endregion

		#region TesDeferTriggerOnUpdateConditionStrategy WhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdateProduct()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_OP = data.Part2.PK, expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdateProduct_NotCustoms()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Order, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_OP = data.Part2.PK, expectToSaveAfterChange: false);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdateProduct_VirtualWerhouse()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: true, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_OP = data.Part2.PK, expectToSaveAfterChange: false);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdateProduct_NotImportingData()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: false, isPicked: true,
				(data, orderline) => orderline.WE_OP = data.Part2.PK, expectToSaveAfterChange: false);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdateProduct_NotPicked()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: false, isPicked: false,
				(data, orderline) => orderline.WE_OP = data.Part2.PK, expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_NotCriticalField()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: true, isUniversalObjectFactory: false, isPicked: true,
				(data, orderline) => orderline.WE_LineNo = 2, expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdateSerial()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_SerialNumber = "SN2", expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdateExpiryDate()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_ExpiryDate = ZDate.Today.AddMonths(1), expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_UpdatePackingDate()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_PackingDate = ZDate.Today.AddMonths(-1), expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_PartAttrib1()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_PartAttrib1 = "NW1", expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_PartAttrib2()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_PartAttrib2 = "NW2", expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_PartAttrib3()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) => orderline.WE_PartAttrib3 = "NW3", expectToSaveAfterChange: true);
		}

		[ExpectNoExceptions]
		public void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_MoreThanOneChange()
		{
			TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(OrderType.Codes.Customs, isVirtualWarehouse: false, isUniversalObjectFactory: true, isPicked: true,
				(data, orderline) =>
				{
					orderline.WE_OP = data.Part2.PK;
					orderline.WE_PartAttrib3 = "NW3";
				}, expectToSaveAfterChange: true);
		}

		void TestWhsCustomCanChangeCriticalFieldsOnPickedOrderWhenImportingOrderLine_Core(string docketSubType, bool isVirtualWarehouse, bool isUniversalObjectFactory, bool isPicked,
			Action<EnvTestDataSimpleEnvironment, WhsOrderLine> changeOrderLine, bool expectToSaveAfterChange)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, false);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location, ZDate.Today.AddDays(10), ZDate.Today.AddDays(-1), "AT1", "AT2", "AT3", "ENTRY-123");
			inventory.WI_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = docketSubType;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1, "ENTRY-123", "DummyOutwards");
			Factory.Save();

			AssertEquals("Precondition", false, data.Whs1.WW_IsVirtualWarehouse);
			data.Whs1.WW_IsVirtualWarehouse = isVirtualWarehouse;
			if (isPicked)
			{
				Helper.CreatePickByAttachingOrders(order);
				AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals("Precondition", 1m, orderLine.PickLines.Single().WZ_Units);
			}
			Factory.Save();

			if (expectToSaveAfterChange)
			{
				ApplyChangesAndSave();
			}
			else
			{
				NUnit.Framework.Assert.That(ApplyChangesAndSave, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change critical fields on picked order.", true), "Should not be able to update line if order is picked.");
			}

			void ApplyChangesAndSave()
			{
				if (isUniversalObjectFactory)
				{
					var factory = new UniversalObjectFactory();
					var orderLineFromDB = factory.Load<WhsOrderLine>(orderLine.PK);
					changeOrderLine(data, orderLineFromDB);
					factory.SaveForTesting();
				}
				else
				{
					var factory = new BusinessObjectFactory();
					var orderLineFromDB = factory.Load<WhsOrderLine>(orderLine.PK);
					changeOrderLine(data, orderLineFromDB);
					factory.Save();
				}
			}
		}

		#endregion

		#region TestNotAllocateableInventoryCannotBeAttachedToPickLine_PickByBOM

		public void TestNotAllocateableInventoryCannotBeAttachedToPickLine_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts

			var kitReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var kitReceiveLine1 = kitReceive.Lines[0];
			AssertEquals("Precondition: newly created Kit Receive Line should be PND.", InventoryStatus.Codes.Pending, kitReceiveLine1.WE_CurrentInventoryStatus);
			var kitPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, kitReceiveLine1.PK));
			AssertEquals("Precondition: Should be committed to relevant Order Line through PickLine.", 1, kitPickLines.Length);
			var kitPickeLine = kitPickLines[0];
			AssertEquals(orderLine.PK, kitPickeLine.WZ_WE_TransactionLine);

			var exceptionThrown = false;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException e)
			{
				AssertEquals(WhsDocketLine.PreventAttemptToMakeAllocatedInventoryUnallocateable, e.InnerException.InnerException.Message);
				exceptionThrown = true;
			}

			AssertEquals("Trigger should NOT have prevented save.", false, exceptionThrown);
		}

		#endregion

		// interfaces

		#region ICustomsDataParent Members

		#region TestIsOutwardTypeRequired

		public void TestIsOutwardTypeRequired()
		{
			ICustomsDataParent customsDataParent = Factory.New<WhsOrderLine>();
			AssertEquals("customsDataParent.IsOutwardTypeUsed", true, customsDataParent.IsOutwardTypeRequired);
		}

		#endregion

		#region TestSetDefaultValueWhsBondedWarehouseAttribute

		public void TestSetDefaultValueWhsBondedWarehouseAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = order.Lines.AddNew();
			AssertEquals("Outward should be set to default", WhsBondedWarehouseAttributeOutwardType.Codes.CNN, orderLine.CustomsData.WB_OutwardType);
		}

		#endregion

		#region TestIsCustomsDataReadOnly

		public void TestIsCustomsDataReadOnly()
		{
			ICustomsDataParent customsDataParent = Factory.New<WhsOrderLine>();
			AssertEquals("customsDataParent.IsCustomsDataReadOnly", true, customsDataParent.IsCustomsDataReadOnly);
		}

		#endregion

		#region TestIsCustomsOutwardDataReadOnly

		#region TestIsCustomsOutwardDataReadOnly_NoOrders

		public void TestIsCustomsOutwardDataReadOnly_NoOrders()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			ICustomsDataParent customsDataParent = orderLine;
			AssertEquals("If order is null, customsDataParent.IsCustomsOutwardDataReadOnly must be true.", true, customsDataParent.IsCustomsOutwardDataReadOnly);
		}

		#endregion

		#region TestIsCustomsOutwardDataReadOnly_Picking

		public void TestIsCustomsOutwardDataReadOnly_Picking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10);
			ICustomsDataParent customsDataParent = orderLine;

			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("If order is not picking, customsDataParent.IsCustomsOutwardDataReadOnly must be false.", false, customsDataParent.IsCustomsOutwardDataReadOnly);

			Helper.CreatePickNew(order);
			order.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertEquals("If order is picking, customsDataParent.IsCustomsOutwardDataReadOnly must be true.", true, customsDataParent.IsCustomsOutwardDataReadOnly);
		}

		#endregion

		#region TestIsCustomsOutwardDataReadOnly_CustomsOrders

		public void TestIsCustomsOutwardDataReadOnly_CUS_FTZ_US()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.Customs, WarehouseTypes.Codes.FreeTradeZone, "USLAX", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CUS_FTZ_PR()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.Customs, WarehouseTypes.Codes.FreeTradeZone, "PRSJU", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CPS_FTZ_US()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.CustomsReleaseWithPermit, WarehouseTypes.Codes.FreeTradeZone, "USLAX", expectedValue: true);
		}

		public void TestIsCustomsOutwardDataReadOnly_CPS_FTZ_PR()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.CustomsReleaseWithPermit, WarehouseTypes.Codes.FreeTradeZone, "PRSJU", expectedValue: true);
		}

		public void TestIsCustomsOutwardDataReadOnly_NonCustoms_FTZ_US()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.Order, WarehouseTypes.Codes.FreeTradeZone, "USLAX", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_NonCustoms_FTZ_PR()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.Order, WarehouseTypes.Codes.FreeTradeZone, "PRSJU", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CUS_FTZ_NonUS()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.Customs, WarehouseTypes.Codes.FreeTradeZone, "AUSYD", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CPS_FTZ_NonUS()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.CustomsReleaseWithPermit, WarehouseTypes.Codes.FreeTradeZone, "AUSYD", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CUS_NonFTZ_US()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.Customs, WarehouseTypes.Codes.Product, "USLAX", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CUS_NonFTZ_PR()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.Customs, WarehouseTypes.Codes.Product, "PRSJU", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CPS_NonFTZ_US()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.CustomsReleaseWithPermit, WarehouseTypes.Codes.Product, "USLAX", expectedValue: false);
		}

		public void TestIsCustomsOutwardDataReadOnly_CPS_NonFTZ_PR()
		{
			TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(OrderType.Codes.CustomsReleaseWithPermit, WarehouseTypes.Codes.Product, "PRSJU", expectedValue: false);
		}

		void TestIsCustomsOutwardDataReadOnly_FTZCustomsOrders(string orderType, string warehouseType, string portCode, bool expectedValue = true)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs1 = data.Whs1;
			SetupWarehouse(whs1, warehouseType, portCode);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs1, "O1", data.Part1, 1);
			order.WD_DocketSubType = orderType;
			var orderLine = (WhsOrderLine)order.Lines.Single();

			ICustomsDataParent customsDataParent = orderLine;
			AssertEquals($"For {warehouseType} Orders with Order Type {orderType} in {portCode} customsDataParent.IsCustomsOutwardDataReadOnly must be {(expectedValue ? "true" : "false")}.", expectedValue, customsDataParent.IsCustomsOutwardDataReadOnly);
		}

		void SetupWarehouse(WhsWarehouse warehouse, string warehouseType, string portCode)
		{
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = portCode;
			Helper.EnableWarehouseForBond(warehouse, true);
			warehouse.WW_WarehouseType = warehouseType;
		}

		#endregion

		#region TestIsCustomsOutwardDataReadOnly_VirtualWarehousing

		public void TestIsCustomsOutwardDataReadOnly_CustomsOrderAttachedToPickVirtualWarehousing()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			data.Whs1.WW_IsVirtualWarehouse = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			_ = Helper.CreatePickByAttachingOrders(order);

			var customsDataParent = (ICustomsDataParent)orderLine;

			AssertEquals(false, customsDataParent.IsCustomsOutwardDataReadOnly);
		}

		public void TestIsCustomsOutwardDataReadOnly_NoVirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			_ = Helper.CreatePickByAttachingOrders(order);

			var customsDataParent = (ICustomsDataParent)orderLine;

			AssertEquals(true, customsDataParent.IsCustomsOutwardDataReadOnly);
		}

		public void TestIsCustomsOutwardDataReadOnly_NonCustomsOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			data.Whs1.WW_IsVirtualWarehouse = true;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			_ = Helper.CreatePickByAttachingOrders(order);

			var customsDataParent = (ICustomsDataParent)orderLine;

			AssertEquals(true, customsDataParent.IsCustomsOutwardDataReadOnly);
		}

		#endregion

		#endregion

		#region TestIsMainCustomsDataPropertiesReadOnly

		public void TestIsMainCustomsDataPropertiesReadOnly()
		{
			ICustomsDataParent customsDataParent = Factory.New<WhsOrderLine>();
			AssertEquals("customsDataParent.IsMainCustomsDataPropertiesReadOnly", false, customsDataParent.IsMainCustomsDataPropertiesReadOnly);
		}

		#endregion

		#endregion

		#region Implementation

		protected override FinalisableDocketHelper<WhsOrder> GetNewDocketHelper(BusinessObjectFactory factory)
		{
			return new FinalisableOrderHelper(factory);
		}

		#endregion
	}
}

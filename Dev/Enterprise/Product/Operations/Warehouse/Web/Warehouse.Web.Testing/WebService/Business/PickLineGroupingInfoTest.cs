using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	class PickLineGroupingInfoTest : WhsTestCaseWithFactory
	{
		#region TestProduct

		public void TestProduct()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			var newProduct = Helper.CreateProduct(orderLine.Docket.Client, "P2");
			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_OP = newProduct.PK);
		}

		#endregion

		#region TestBOMProduct

		public void TestBOMProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, bike, 10m);
			pick.AddOrders(new[] { order1 });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var wheelPickLine1 = orderLine1.ChildComponentLines.Single().PickLines[0];
			var wheelPickLine2 = orderLine2.ChildComponentLines.Single().PickLines[0];

			var group1 = new PickLineGroupingInfo(wheelPickLine1);
			var group2 = new PickLineGroupingInfo(wheelPickLine2);
			AssertEquals(bike, group1.BOMProduct);
			AssertEquals(bike, group2.BOMProduct);

			AssertEqualityChangedWhenFieldValueDiffers(wheelPickLine1, wheelPickLine2, line => line.DocketLine.WE_WE_ParentDocketLine = ZGuid.Empty);
		}

		#endregion

		#region TestClient

		public void TestClient()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			var newClient = Helper.CreateClient("Org2");
			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.Docket.WD_OH_Client = newClient.PK);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.LocationString = "A-2");
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_PalletID = "SomeValue");
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);
			var newProduct = Helper.CreateProduct(orderLine.Docket.Client, "P2");
			newProduct.OP_StockKeepingUnit = "BOX";

			AssertEquals("UNT", orderLine.PickLines[0].WZ_UnitsUQ);
			AssertEquals("UNT", orderLine.PickLines[1].WZ_UnitsUQ);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.Inventory.WI_OP = newProduct.PK);

			AssertEquals("BOX", orderLine.PickLines[0].WZ_UnitsUQ);
			AssertEquals("BOX", orderLine.PickLines[1].WZ_UnitsUQ);
		}

		#endregion

		#region TestPartAttrib1

		public void TestPartAttrib1()
		{
			var orderLine = GetSimpleOrderLine();

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_PartAttrib1 = "SomeValue");
		}

		#endregion

		#region TestPartAttrib2

		public void TestPartAttrib2()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_PartAttrib2 = "SomeValue");
		}

		#endregion

		#region TestPartAttrib3

		public void TestPartAttrib3()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_PartAttrib3 = "SomeValue");
		}

		#endregion

		#region TestSerialNumber

		public void TestSerialNumber()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_SerialNumber = "SomeValue");
		}

		#endregion

		#region TestExpiryDate

		public void TestExpiryDate()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_ExpiryDate = ZDate.Today);
		}

		#endregion

		#region TestPackingDate

		public void TestPackingDate()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => line.InventoryLine.WE_PackingDate = ZDate.Today);
		}

		#endregion

		#region Test Ordered Attributes

		#region TestOrderedAttribute1

		public void TestOrderedAttribute1_PalletIDNeutral()
		{
			TestOrderedAttribute1Core(GetDataForPalletIDNeutralSetup());
		}

		public void TestOrderedAttribute1_AttributeNeutral()
		{
			TestOrderedAttribute1Core(GetDataForAttributeNeutralSetup());
		}

		void TestOrderedAttribute1Core(WhsPick pick)
		{
			var picklines = pick.GetAllPickLines().ToArray();
			AssertEqualityChangedWhenFieldValueDiffers(picklines[0], picklines[1], line => line.DocketLine.WE_PartAttrib1 = "SomeValue");
		}

		#endregion

		#region TestOrderedAttribute2

		public void TestOrderedAttribute2_PalletIDNeutral()
		{
			TestOrderedAttribute2Core(GetDataForPalletIDNeutralSetup());
		}

		public void TestOrderedAttribute2_AttributeNeutral()
		{
			TestOrderedAttribute2Core(GetDataForAttributeNeutralSetup());
		}

		void TestOrderedAttribute2Core(WhsPick pick)
		{
			var picklines = pick.GetAllPickLines().ToArray();
			AssertEqualityChangedWhenFieldValueDiffers(picklines[0], picklines[1], line => line.DocketLine.WE_PartAttrib2 = "SomeValue");
		}

		#endregion

		#region TestOrderedAttribute3

		public void TestOrderedAttribute3_PalletIDNeutral()
		{
			TestOrderedAttribute3Core(GetDataForPalletIDNeutralSetup());
		}

		public void TestOrderedAttribute3_AttributeNeutral()
		{
			TestOrderedAttribute3Core(GetDataForAttributeNeutralSetup());
		}

		void TestOrderedAttribute3Core(WhsPick pick)
		{
			var picklines = pick.GetAllPickLines().ToArray();
			AssertEqualityChangedWhenFieldValueDiffers(picklines[0], picklines[1], line => line.DocketLine.WE_PartAttrib3 = "SomeValue");
		}

		#endregion

		#region TestOrderedSerialNumber

		public void TestOrderedSerialNumber_PalletIDNeutral()
		{
			TestOrderedSerialNumberCore(GetDataForPalletIDNeutralSetup());
		}

		public void TestOrderedSerialNumber_AttributeNeutral()
		{
			TestOrderedSerialNumberCore(GetDataForAttributeNeutralSetup());
		}

		void TestOrderedSerialNumberCore(WhsPick pick)
		{
			var picklines = pick.GetAllPickLines().ToArray();
			AssertEqualityChangedWhenFieldValueDiffers(picklines[0], picklines[1], line => line.DocketLine.WE_SerialNumber = "SomeValue");
		}

		#endregion

		#region TestOrderedExpiryDate

		public void TestOrderedExpiryDate_PalletIDNeutral()
		{
			TestOrderedExpiryDateCore(GetDataForPalletIDNeutralSetup());
		}

		public void TestOrderedExpiryDate_AttributeNeutral()
		{
			TestOrderedExpiryDateCore(GetDataForAttributeNeutralSetup());
		}

		void TestOrderedExpiryDateCore(WhsPick pick)
		{
			var picklines = pick.GetAllPickLines().ToArray();
			AssertEqualityChangedWhenFieldValueDiffers(picklines[0], picklines[1], line => line.DocketLine.WE_ExpiryDate = ZDate.Today);
		}

		#endregion

		#region TestOrderedPackingDate

		public void TestOrderedPackingDate_PalletIDNeutral()
		{
			TestOrderedPackingDateCore(GetDataForPalletIDNeutralSetup());
		}

		public void TestOrderedPackingDate_AttributeNeutral()
		{
			TestOrderedPackingDateCore(GetDataForAttributeNeutralSetup());
		}

		void TestOrderedPackingDateCore(WhsPick pick)
		{
			var picklines = pick.GetAllPickLines().ToArray();
			AssertEqualityChangedWhenFieldValueDiffers(picklines[0], picklines[1], line => line.DocketLine.WE_PackingDate = ZDate.Today);
		}

		#endregion

		#region TestOrderedPalletID

		public void TestOrderedPalletID()
		{
			var pick = GetDataForPalletIDNeutralSetup();
			var picklines = pick.GetAllPickLines().ToArray();
			AssertEqualityChangedWhenFieldValueDiffers(picklines[0], picklines[1], line => line.DocketLine.WE_PalletID = "PID1");
		}

		#endregion

		WhsPick GetDataForAttributeNeutralSetup()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			data.Org1.PartAttributeManager.SetProductToUseAttribute(data.Part1, 1, true);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			data.Part1.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			AssertEquals(2, pick.GetAllPickLines().Count());

			return pick;
		}

		WhsPick GetDataForPalletIDNeutralSetup()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.DefaultLocation;
			location.LocationType.WLT_IsPalletIDNeutral = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, location, "");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			AssertEquals(2, pick.GetAllPickLines().Count());

			return pick;
		}

		#endregion

		#region TestIsLocationEmptyAfterFinalisingPick

		public void TestIsLocationEmptyAfterFinalisingPick()
		{
			var orderLine = GetSimpleOrderLine();
			AssertEquals(2, orderLine.PickLines.Count);

			var isLocationEmptyAfterFinalisingPick = ((IEmptyLocationAfterPickFinalisation)orderLine.PickLines[0]).IsLocationEmptyAfterFinalisingPick;
			AssertEqualityChangedWhenFieldValueDiffers(orderLine.PickLines[0], orderLine.PickLines[1], line => ((IEmptyLocationAfterPickFinalisation)line).IsLocationEmptyAfterFinalisingPick = !isLocationEmptyAfterFinalisingPick);
		}

		#endregion

		#region TestPackageID

		public void TestPackageID()
		{
			var orderLine = GetSimpleOrderLine();

			Assert("Precondition: two lines must be identical at the beginning",
				new PickLineGroupingInfo(orderLine.PickLines[0], "PKG1", 1).Equals(
				new PickLineGroupingInfo(orderLine.PickLines[1], "PKG1", 1)));

			Assert("After changing grouped infos for first line, must not match", !(
				new PickLineGroupingInfo(orderLine.PickLines[0], "PKG2", 1).Equals(
				new PickLineGroupingInfo(orderLine.PickLines[1], "PKG1", 1))));

			Assert("After applying same change to secong grouped info, must match again",
				new PickLineGroupingInfo(orderLine.PickLines[0], "PKG2", 1).Equals(
				new PickLineGroupingInfo(orderLine.PickLines[1], "PKG2", 1)));
		}

		#endregion

		#region TestSlotNumber

		public void TestSlotNumber()
		{
			var orderLine = GetSimpleOrderLine();

			Assert("Precondition: two lines must be identical at the beginning",
				new PickLineGroupingInfo(orderLine.PickLines[0], "PKG1", 1).Equals(
				new PickLineGroupingInfo(orderLine.PickLines[1], "PKG1", 1)));

			Assert("After changing grouped infos for first line, must not match", !(
				new PickLineGroupingInfo(orderLine.PickLines[0], "PKG1", 2).Equals(
				new PickLineGroupingInfo(orderLine.PickLines[1], "PKG1", 1))));

			Assert("After applying same change to secong grouped info, must match again",
				new PickLineGroupingInfo(orderLine.PickLines[0], "PKG1", 2).Equals(
				new PickLineGroupingInfo(orderLine.PickLines[1], "PKG1", 2)));
		}

		#endregion

		#region Test Helpers

		WhsOrderLine GetSimpleOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 7m, data.Whs1.DefaultLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 12m);
			Helper.CreatePickNew(order);
			var orderLine = order.Lines[0];
			AssertEquals(2, orderLine.PickLines.Count);
			return orderLine;
		}

		void AssertEqualityChangedWhenFieldValueDiffers(WhsPickLine pickLine1, WhsPickLine pickLine2, Action<WhsPickLine> permutationAction)
		{
			Assert("Precondition: two lines must be identical at the beginning", new PickLineGroupingInfo(pickLine1).Equals(new PickLineGroupingInfo(pickLine2)));
			AssertEquals("Precondition: two lines must be identical at the beginning", new PickLineGroupingInfo(pickLine1).GetHashCode(), new PickLineGroupingInfo(pickLine2).GetHashCode());
			permutationAction(pickLine2);
			Assert("After pickLine2 change - grouped infos must not match", !(new PickLineGroupingInfo(pickLine1).Equals(new PickLineGroupingInfo(pickLine2))));
			AssertNotEquals("After pickLine2 change - grouped infos must not match", new PickLineGroupingInfo(pickLine1).GetHashCode(), new PickLineGroupingInfo(pickLine2).GetHashCode());
			permutationAction(pickLine1);
			Assert("After applying same change to pickLine1 grouped infos must match again", new PickLineGroupingInfo(pickLine1).Equals(new PickLineGroupingInfo(pickLine2)));
			AssertEquals("After applying same change to pickLine1 grouped infos must match again", new PickLineGroupingInfo(pickLine1).GetHashCode(), new PickLineGroupingInfo(pickLine2).GetHashCode());
		}

		#endregion
	}
}

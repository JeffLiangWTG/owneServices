using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public abstract class WhsInventoryLineBaseInfoTestCase<T> : DataObjectInfoTestCase<T>
		where T : WhsInventoryLineBaseInfo<T>, new()
	{
		#region Constructors

		public void TestAdditionalConstructors()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);
			var client = data.Org1;
			var part = data.Part1;
			var location = data.Whs1.DefaultLocation;
			var heldCode = helper.CreateInventoryHeldCode("ABC", "Held 1");

			factory.Save();

			helper.SetClientAttributeType(client, AttributeNumber.One, true, "Attr1 Name");
			helper.SetClientAttributeType(client, AttributeNumber.Two, true, "Attr2 Name");
			helper.SetClientAttributeType(client, AttributeNumber.Three, true, "Attr3 Name");

			helper.CreateProductUnit(part, "M3", "KG", 10m);
			part.OP_StockKeepingUnit = "KG";
			part.OP_PartNum = "PART NO";
			part.RelatedOrganisations[0].OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line = helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			line.WI_WL = location.PK;
			line.WI_ExpiryDate = new ZDate(2008, 3, 12);
			line.WI_PackingDate = new ZDate(2008, 3, 13);
			line.WI_PartAttrib1 = "PartAttr1";
			line.WI_PartAttrib2 = "PartAttr2";
			line.WI_PartAttrib3 = "PartAttr3";
			line.WI_SerialNumber = "SerialNum";
			line.WI_PalletID = "Pallet ID";
			line.WI_InDocketLineUnits = 10m;
			line.WI_F3_NKPackType = "M3";

			var inventoryLine = line.InDocketLine;
			inventoryLine.Logs.AddNew(Events.WarehouseReceiptConfirmedPutaway);
			inventoryLine.WE_PackQuantity = 20m;
			inventoryLine.WE_WHC_NKOriginalInventoryHeldCode = "AB1";
			inventoryLine.WE_WHC_NKCurrentInventoryHeldCode = "ABC";
			inventoryLine.WE_CurrentHoldReason = "Hold for QA check";

			var lineCollection = GetCollection();
			var lineInfo = GetInventoryLineInfo(lineCollection, line);
			AssertEquals("PK", line.PK.ToGuid(), lineInfo.PK);
			AssertEquals("DocketPK", line.Docket.PK.ToGuid(), lineInfo.DocketPK);
			AssertEquals("ExpiryDate", new DateTime(2008, 3, 12), lineInfo.ExpiryDate);
			AssertEquals("ExpiryDate.Kind", DateTimeKind.Unspecified, lineInfo.ExpiryDate.Kind);
			AssertEquals("Attribute1", "PartAttr1", lineInfo.Attribute1);
			AssertEquals("Attribute2", "PartAttr2", lineInfo.Attribute2);
			AssertEquals("Attribute3", "PartAttr3", lineInfo.Attribute3);
			AssertEquals("SerialNumber", "SerialNum", lineInfo.SerialNumber);
			AssertEquals("PackingDate", new DateTime(2008, 3, 13), lineInfo.PackingDate);
			AssertEquals("PackingDate.Kind", DateTimeKind.Unspecified, lineInfo.PackingDate.Kind);
			AssertEquals("PalletID", "Pallet ID", lineInfo.PalletID);
			AssertEquals("Packs", 20m, lineInfo.Packs);
			AssertEquals("Qty", 2m, lineInfo.Qty);
			AssertEquals("QtyUQ", part.OP_StockKeepingUnit, lineInfo.QtyUQ);
			AssertEquals("Location", location.ToLocationString(), lineInfo.Location);
			AssertEquals("StatusDesc", InventoryStatus.Descriptions.Putaway, lineInfo.StatusDesc);
			AssertEquals("RfAttributeConfirm", 1, lineInfo.RfAttributeConfirm);
			AssertEquals("HeldCode", "ABC", lineInfo.HeldCode);
			AssertEquals("HeldCode", "Held 1", lineInfo.HeldCodeDesc);
			AssertEquals("InventoryStatus", "PUT", lineInfo.InventoryStatus);
			AssertEquals("HoldReason", "Hold for QA check", lineInfo.HoldReason);
			AssertEquals("Location PK", location.PK, lineInfo.LocationPK);

			lineCollection.InventoryLineInfos.Add(lineInfo);
			AssertEquals(1, lineCollection.InventoryLineInfos.Count);
			AssertEquals(1, lineCollection.ProductInfos.Count);
			AssertEquals(1, lineCollection.ProductPartAttributesInfos.Count);

			heldCode.Delete();
			var lineInfo_AfterHoldCodeDeleted = GetInventoryLineInfo(lineCollection, line);
			AssertEquals("Should fall back to hold code NK if the records has been deleted.", "ABC", lineInfo_AfterHoldCodeDeleted.HeldCodeDesc);
		}

		public void TestAdditionalConstructors_FixedWidthLocation()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);
			var part = data.Part1;
			var warehouse = helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			factory.Save();

			var location = warehouse.FindLocation("Z030201");
			var receive = helper.CreateWhsReceive(data.Org1, warehouse);
			var line = helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			line.WI_WL = location.PK;
			line.WI_PalletID = "Pallet ID";
			line.WI_InDocketLineUnits = 10m;

			var lineCollection = GetCollection();
			var lineInfo = GetInventoryLineInfo(lineCollection, line);
			AssertEquals("Location", "Z030201", lineInfo.Location);
			AssertEquals("Location_UserFriendly", "Z-03-02-01", lineInfo.Location_UserFriendly);
		}

		public void TestConstructors_LocationString()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, dockDoorLocation, "PLT1");
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			factory.Save();

			var lineCollection = GetCollection();
			var lineInfo = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("LocationString should get LocationString", "DOCKDOOR", lineInfo.Location);
			AssertEquals("DestinationLocation should get putaway transfer's LocationString", "", lineInfo.DestinationLocation);
			AssertEquals("DestinationPalletId should get putaway transfer's pallet id.", "", lineInfo.DestinationPalletId);
			AssertEquals("", ZGuid.Empty, lineInfo.DestinationLocationPK);
			AssertEquals("Expect No putaway transfer", false, lineInfo.HasPutawayTransfer);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT1", 15m);
			transfer.RunPreSaveValidation();

			AssertEquals($"Precondition: InventoryStatus of Transfer Line should be {InventoryStatus.Codes.Received}.", InventoryStatus.Codes.Received, transferLine.WE_CurrentInventoryStatus);
			AssertEquals($"Precondition: InventoryStatus of Inventory should be {InventoryStatus.Codes.Received}.", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var lineInfo2 = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("LocationString is still dock door as inventory is still received with transfer line unpicked", "DOCKDOOR", lineInfo2.Location);
			AssertEquals("DestinationLocation should get putaway transfer's locationstring", "A-2", lineInfo2.DestinationLocation);
			AssertEquals("DestinationPalletId should get putaway transfer's pallet id.", "PLT1", lineInfo2.DestinationPalletId);
			AssertEquals("Location PK", nonDockDoorLocation.PK, lineInfo2.DestinationLocationPK);
			AssertEquals("Expect putaway transfer", true, lineInfo2.HasPutawayTransfer);

			transferLine.Docket.FinaliseDocketWithoutUserConfirmation();
			factory.Save();

			var lineInfo3 = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("LocationString should get WE_WL of receive line - LocationString", "DOCKDOOR", lineInfo3.Location);
			AssertEquals("DestinationLocation should get putaway transfer's locationstring", "A-2", lineInfo3.DestinationLocation);
			AssertEquals("DestinationPalletId should get putaway transfer's pallet id.", "PLT1", lineInfo3.DestinationPalletId);
			AssertEquals("Location PK", nonDockDoorLocation.PK, lineInfo3.DestinationLocationPK);
			AssertEquals("Expect putaway transfer", true, lineInfo3.HasPutawayTransfer);

			var lineInfo4 = GetInventoryLineInfo(lineCollection, inventory2);
			AssertEquals("LocationString should be empty as Location & TransferFromLocation are all null", "", lineInfo4.Location);
			AssertEquals("DestinationLocation should be empty as this inventory has no putaway transfer", "", lineInfo4.DestinationLocation);
			AssertEquals("DestinationPalletId should be empty as this inventory has no putaway transfer.", "", lineInfo4.DestinationPalletId);
			AssertEquals("Location PK", ZGuid.Empty, lineInfo4.DestinationLocationPK);
			AssertEquals("Receiveline has no location so no putaway transfer", false, lineInfo4.HasPutawayTransfer);
		}

		public void TestConstructors_LocationString_FixedWidthLocation()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			var warehouse = helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			factory.Save();

			var dockDoorLocation = warehouse.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = warehouse.FindLocation("Z040302");

			var receive = helper.CreateWhsReceive(data.Org1, warehouse, "R1");
			var inventory = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, dockDoorLocation, "PLT1");
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			factory.Save();

			var lineCollection = GetCollection();
			var lineInfo = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("LocationString should get LocationString", "DOCKDOOR", lineInfo.Location);
			AssertEquals("Location_UserFriendly should get LocationString", "DOCKDOOR", lineInfo.Location_UserFriendly);
			AssertEquals("LocationPK should get dock door Location PK", dockDoorLocation.PK, lineInfo.LocationPK);
			AssertEquals("DestinationLocation should get putaway transfer's LocationString", "", lineInfo.DestinationLocation);
			AssertEquals("DestinationLocation_UserFriendly should get putaway transfer's LocationString", "", lineInfo.DestinationLocation_UserFriendly);
			AssertEquals("DestinationLocationPK should get putaway transfer's Location PK", Guid.Empty, lineInfo.DestinationLocationPK);
			AssertEquals("DestinationPalletId should get putaway transfer's pallet id.", "", lineInfo.DestinationPalletId);
			AssertEquals("Expect No putaway transfer", false, lineInfo.HasPutawayTransfer);

			var transfer = helper.CreateWhsTransfer(data.Org1, warehouse, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT1", 15m);
			transfer.RunPreSaveValidation();

			AssertEquals($"Precondition: InventoryStatus of Transfer Line should be {InventoryStatus.Codes.Received}.", InventoryStatus.Codes.Received, transferLine.WE_CurrentInventoryStatus);
			AssertEquals($"Precondition: InventoryStatus of Inventory should be {InventoryStatus.Codes.Received}.", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var lineInfo2 = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("LocationString is still dock door as inventory is still received with transfer line unpicked", "DOCKDOOR", lineInfo2.Location);
			AssertEquals("Location_UserFriendly is still dock door as inventory is still received with transfer line unpicked", "DOCKDOOR", lineInfo2.Location_UserFriendly);
			AssertEquals("LocationPK should still get dock door Location PK", dockDoorLocation.PK, lineInfo.LocationPK);
			AssertEquals("DestinationLocation should get putaway transfer's locationstring", "Z040302", lineInfo2.DestinationLocation);
			AssertEquals("DestinationLocation should get putaway transfer's locationstring", "Z-04-03-02", lineInfo2.DestinationLocation_UserFriendly);
			AssertEquals("DestinationLocationPK should get putaway transfer's Location PK", nonDockDoorLocation.PK, lineInfo2.DestinationLocationPK);
			AssertEquals("DestinationPalletId should get putaway transfer's pallet id.", "PLT1", lineInfo2.DestinationPalletId);
			AssertEquals("Expect putaway transfer", true, lineInfo2.HasPutawayTransfer);

			transferLine.Docket.FinaliseDocketWithoutUserConfirmation();
			factory.Save();

			var lineInfo3 = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("LocationString should get WE_WL of receive line - LocationString", "DOCKDOOR", lineInfo3.Location);
			AssertEquals("LocationString should get WE_WL of receive line - Location_UserFriendly", "DOCKDOOR", lineInfo3.Location_UserFriendly);
			AssertEquals("DestinationLocation should get putaway transfer's locationstring", "Z040302", lineInfo3.DestinationLocation);
			AssertEquals("DestinationLocation should get putaway transfer's locationstring", "Z-04-03-02", lineInfo3.DestinationLocation_UserFriendly);
			AssertEquals("DestinationPalletId should get putaway transfer's pallet id.", "PLT1", lineInfo3.DestinationPalletId);
			AssertEquals("Expect putaway transfer", true, lineInfo3.HasPutawayTransfer);

			var lineInfo4 = GetInventoryLineInfo(lineCollection, inventory2);
			AssertEquals("LocationString should be empty as Location & TransferFromLocation are all null", "", lineInfo4.Location);
			AssertEquals("LocationString should be empty as Location & TransferFromLocation are all null", "", lineInfo4.Location_UserFriendly);
			AssertEquals("DestinationLocation should be empty as this inventory has no putaway transfer", "", lineInfo4.DestinationLocation);
			AssertEquals("DestinationLocation should be empty as this inventory has no putaway transfer", "", lineInfo4.DestinationLocation_UserFriendly);
			AssertEquals("DestinationPalletId should be empty as this inventory has no putaway transfer.", "", lineInfo4.DestinationPalletId);
			AssertEquals("Receiveline has no location so no putaway transfer", false, lineInfo4.HasPutawayTransfer);
		}

		public void TestConstructors_FormattedCheckDigit()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory, 2, 1);

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			AssertEquals("", dockDoorLocation.FormattedCheckDigit);

			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			nonDockDoorLocation.FormattedCheckDigit = "11";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, dockDoorLocation, "PLT1");
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			factory.Save();

			var lineCollection = GetCollection();
			var lineInfo = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("", lineInfo.LocationFormattedCheckDigit);
			AssertEquals("", lineInfo.DestinationLocationFormattedCheckDigit);

			var transfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT1", 15m);
			transfer.RunPreSaveValidation();

			var lineInfo2 = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("", lineInfo2.LocationFormattedCheckDigit);
			AssertEquals("11", lineInfo2.DestinationLocationFormattedCheckDigit);

			transferLine.Docket.FinaliseDocketWithoutUserConfirmation();
			factory.Save();

			var lineInfo3 = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("", lineInfo3.LocationFormattedCheckDigit);
			AssertEquals("11", lineInfo3.DestinationLocationFormattedCheckDigit);

			var lineInfo4 = GetInventoryLineInfo(lineCollection, inventory2);
			AssertEquals("", lineInfo4.LocationFormattedCheckDigit);
			AssertEquals("", lineInfo4.DestinationLocationFormattedCheckDigit);
		}

		public void TestAdditionalConstructors_AvailableQty()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);

			var whs = helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			var client = helper.CreateClient("TEST123", "Test Client");
			var product = helper.CreateProduct(client, "PROD1");
			var docketLine = (WhsDocketLine)helper.CreateStock(whs.PK, client.PK, product.PK, 10m);
			helper.Factory.Save();

			var inventory = (WhsInventoryView)docketLine.Inventory.FirstOrDefault();
			AssertNotNull(inventory);

			var lineCollection = GetCollection();
			var lineInfo = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("AvailableQty", 10m, lineInfo.AvailableQty);
		}

		public void TestAdditionalConstructors_AvailableToTransferQty()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);

			var whs = helper.CreateWarehouse("Warehouse1", "A", 2, 2);
			var client = helper.CreateClient("TEST123", "Test Client");
			var product = helper.CreateProduct(client, "PROD1");
			var docketLine = (WhsDocketLine)helper.CreateStock(whs.PK, client.PK, product.PK, 10m);
			helper.Factory.Save();

			var inventory = (WhsInventoryView)docketLine.Inventory.FirstOrDefault();
			AssertNotNull(inventory);

			var lineCollection = GetCollection();
			var lineInfo = GetInventoryLineInfo(lineCollection, inventory);
			AssertEquals("AvailableToTransferQty", 10m, lineInfo.AvailableToTransferQty);
		}

		#endregion

		#region Product

		public void TestProduct()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			var line3 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m);

			var lineCollection = GetCollection();
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line1));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line2));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line3));

			AssertEquals(2, lineCollection.ProductInfos.Count);
			AssertContainsExactElementsInAnyOrder(new[] { data.Part1.PK, data.Part2.PK }, lineCollection.ProductInfos.Select(p => p.PK));
		}

		#endregion

		#region ProductPartAttributes

		public void TestProductPartAttributes()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			var line3 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var line4 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			var line5 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);

			var lineCollection = GetCollection();
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line1));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line2));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line3));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line4));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line5));

			AssertEquals(2, lineCollection.ProductPartAttributesInfos.Count);
			AssertEquals(1, lineCollection.ProductPartAttributesInfos.Count(pp => pp.ClientPK == data.Org1.PK && pp.ProductPK == data.Part1.PK));
			AssertEquals(1, lineCollection.ProductPartAttributesInfos.Count(pp => pp.ClientPK == data.Org1.PK && pp.ProductPK == data.Part2.PK));
		}

		#endregion

		#region Properties

		#region TestLocation

		public void TestLocation()
		{
			AssertEquals("", Parent.Location);

			Parent.Location = "1234";
			AssertEquals("1234", Parent.Location);

			Parent.Location = "4321";
			AssertEquals("4321", Parent.Location);
		}

		public void TestLocation_UserFriendly()
		{
			AssertEquals("", Parent.Location_UserFriendly);

			Parent.Location_UserFriendly = "1234";
			AssertEquals("1234", Parent.Location_UserFriendly);

			Parent.Location_UserFriendly = "4321";
			AssertEquals("4321", Parent.Location_UserFriendly);
		}

		public void TestLocationFormattedCheckDigit()
		{
			AssertEquals("", Parent.LocationFormattedCheckDigit);

			Parent.LocationFormattedCheckDigit = "12";
			AssertEquals("12", Parent.LocationFormattedCheckDigit);

			Parent.LocationFormattedCheckDigit = "21";
			AssertEquals("21", Parent.LocationFormattedCheckDigit);
		}

		public void TestDestinationLocationFormattedCheckDigit()
		{
			AssertEquals("", Parent.DestinationLocationFormattedCheckDigit);

			Parent.DestinationLocationFormattedCheckDigit = "12";
			AssertEquals("12", Parent.DestinationLocationFormattedCheckDigit);

			Parent.DestinationLocationFormattedCheckDigit = "21";
			AssertEquals("21", Parent.DestinationLocationFormattedCheckDigit);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			AssertEquals("", Parent.ClientCode);

			Parent.ClientCode = "1234";
			AssertEquals("1234", Parent.ClientCode);

			Parent.ClientCode = "4321";
			AssertEquals("4321", Parent.ClientCode);
		}

		#endregion

		#region TestPK

		public void TestPK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			var newPK = Guid.NewGuid();
			Parent.PK = newPK;
			AssertEquals(newPK, Parent.PK);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			AssertEquals("", Parent.PalletID);

			Parent.PalletID = "1234";
			AssertEquals("1234", Parent.PalletID);

			Parent.PalletID = "4321";
			AssertEquals("4321", Parent.PalletID);
		}

		#endregion

		#region TestPacks

		public void TestPacks()
		{
			AssertEquals(0m, Parent.Packs);

			Parent.Packs = 10m;
			AssertEquals(10m, Parent.Packs);

			Parent.Packs = 20.20m;
			AssertEquals(20.20m, Parent.Packs);
		}

		#endregion

		#region TestQty

		public void TestQty()
		{
			AssertEquals(0m, Parent.Qty);

			Parent.Qty = 10m;
			AssertEquals(10m, Parent.Qty);

			Parent.Qty = 20.20m;
			AssertEquals(20.20m, Parent.Qty);
		}

		#endregion

		#region TestQtyUQ

		public void TestQtyUQ()
		{
			AssertEquals("", Parent.QtyUQ);

			Parent.QtyUQ = "1234";
			AssertEquals("1234", Parent.QtyUQ);

			Parent.QtyUQ = "4321";
			AssertEquals("4321", Parent.QtyUQ);
		}

		#endregion

		#region TestAttribute1

		public void TestAttribute1()
		{
			AssertEquals("", Parent.Attribute1);

			Parent.Attribute1 = "1234";
			AssertEquals("1234", Parent.Attribute1);

			Parent.Attribute1 = "4321";
			AssertEquals("4321", Parent.Attribute1);
		}

		#endregion

		#region TestAttribute2

		public void TestAttribute2()
		{
			AssertEquals("", Parent.Attribute2);

			Parent.Attribute2 = "1234";
			AssertEquals("1234", Parent.Attribute2);

			Parent.Attribute2 = "4321";
			AssertEquals("4321", Parent.Attribute2);
		}

		#endregion

		#region TestAttribute3

		public void TestAttribute3()
		{
			AssertEquals("", Parent.Attribute3);

			Parent.Attribute3 = "1234";
			AssertEquals("1234", Parent.Attribute3);

			Parent.Attribute3 = "4321";
			AssertEquals("4321", Parent.Attribute3);
		}

		#endregion

		#region TestSerialNumber

		public void TestSerialNumber()
		{
			AssertEquals("", Parent.SerialNumber);

			Parent.SerialNumber = "1234";
			AssertEquals("1234", Parent.SerialNumber);

			Parent.SerialNumber = "4321";
			AssertEquals("4321", Parent.SerialNumber);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			AssertEquals("", Parent.InventoryStatus);

			Parent.InventoryStatus = "1234";
			AssertEquals("1234", Parent.InventoryStatus);

			Parent.InventoryStatus = "4321";
			AssertEquals("4321", Parent.InventoryStatus);
		}

		#endregion

		#region TestHeldCode

		public void TestHeldCode()
		{
			AssertEquals("", Parent.HeldCode);

			Parent.HeldCode = "1234";
			AssertEquals("1234", Parent.HeldCode);

			Parent.HeldCode = "4321";
			AssertEquals("4321", Parent.HeldCode);
		}

		#endregion

		#region TestHeldCodeDesc

		public void TestHeldCodeDesc()
		{
			AssertEquals("", Parent.HeldCodeDesc);

			Parent.HeldCodeDesc = "1234";
			AssertEquals("1234", Parent.HeldCodeDesc);

			Parent.HeldCodeDesc = "4321";
			AssertEquals("4321", Parent.HeldCodeDesc);
		}

		#endregion

		#region TestHoldReason

		public void TestHoldReason()
		{
			AssertEquals("", Parent.HoldReason);

			Parent.HoldReason = "Hold for QA check";
			AssertEquals("Hold for QA check", Parent.HoldReason);

			Parent.HoldReason = "Damaged";
			AssertEquals("Damaged", Parent.HoldReason);
		}

		#endregion

		#region TestExpiryDate

		public void TestExpiryDate()
		{
			AssertEquals(new DateTime(), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2008, 03, 12);
			AssertEquals(new DateTime(2008, 03, 12), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2008, 03, 13);
			AssertEquals(new DateTime(2008, 03, 13), Parent.ExpiryDate);
		}

		#endregion

		#region TestPackingDate

		public void TestPackingDate()
		{
			AssertEquals(new DateTime(), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2008, 03, 12);
			AssertEquals(new DateTime(2008, 03, 12), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2008, 03, 13);
			AssertEquals(new DateTime(2008, 03, 13), Parent.PackingDate);
		}

		#endregion

		#region TestAvailableQty

		public void TestAvailableQty()
		{
			AssertEquals(0m, Parent.AvailableQty);

			Parent.AvailableQty = 10m;
			AssertEquals(10m, Parent.AvailableQty);

			Parent.AvailableQty = 20.20m;
			AssertEquals(20.20m, Parent.AvailableQty);
		}

		#endregion

		#region TestAvailableToTransferQty

		public void TestAvailableToTransferQty()
		{
			AssertEquals(0m, Parent.AvailableToTransferQty);

			Parent.AvailableToTransferQty = 10m;
			AssertEquals(10m, Parent.AvailableToTransferQty);

			Parent.AvailableToTransferQty = 20.20m;
			AssertEquals(20.20m, Parent.AvailableToTransferQty);
		}

		#endregion

		#endregion

		#region Implementation

		protected abstract WhsInventoryLineBaseInfoCollection<T> GetCollection();

		protected abstract T GetInventoryLineInfo(WhsInventoryLineBaseInfoCollection<T> collection, WhsInventoryView inventory);

		protected new WhsInventoryLineBaseInfo<T> Parent
		{
			get { return (WhsInventoryLineBaseInfo<T>)base.Parent; }
		}

		#endregion
	}

	[TestedType(typeof(WhsInventoryLineInfo))]
	public class WhsInventoryLineInfoTestCase : WhsInventoryLineBaseInfoTestCase<WhsInventoryLineInfo>
	{
		protected override WhsInventoryLineBaseInfoCollection<WhsInventoryLineInfo> GetCollection()
		{
			return new WhsInventoryLineInfoCollection();
		}

		protected override WhsInventoryLineInfo GetInventoryLineInfo(WhsInventoryLineBaseInfoCollection<WhsInventoryLineInfo> collection, WhsInventoryView inventory)
		{
			return new WhsInventoryLineInfo(collection, inventory);
		}

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsInventoryLineInfo();
		}

		#endregion
	}
}

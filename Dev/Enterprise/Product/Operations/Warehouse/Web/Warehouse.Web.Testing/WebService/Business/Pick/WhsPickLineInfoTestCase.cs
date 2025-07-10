using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsPickLineInfo))]
	public class WhsPickLineInfoTestCase : DataObjectInfoTestCase<WhsPickLineInfo>
	{
		#region Constructors

		#region TestConstructorWithPickLineGroupingInfo

		public void TestConstructorWithPickLineGroupingInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.DefaultLocation.FormattedCheckDigit = "11";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pickLine1 = Helper.CreateWhsPickLine(order.Lines[0], inventory1, 3m);
			var pickLine2 = Helper.CreateWhsPickLine(order.Lines[0], inventory2, 7m);

			var pickLines = new[] { pickLine1, pickLine2 };

			foreach (var pickLine in pickLines)
			{
				AssertNotNull(pickLine.InventoryLine.Location);
				AssertNotEquals(ZString.Empty, pickLine.InventoryLine.LocationString);
				AssertNotEquals(ZString.Empty, pickLine.InventoryLine.Location.OldBarcode);

				pickLine.InventoryLine.WE_ExpiryDate = new ZDate(2008, 3, 12);
				pickLine.InventoryLine.WE_PackingDate = new ZDate(2008, 3, 13);
				pickLine.InventoryLine.WE_PartAttrib1 = "PartAttr1";
				pickLine.InventoryLine.WE_PartAttrib2 = "PartAttr2";
				pickLine.InventoryLine.WE_PartAttrib3 = "PartAttr3";
				pickLine.InventoryLine.WE_PalletID = "Pallet ID";
				AssertNotNull(pickLine.SupplierPart);

				pickLine.SupplierPart.OP_StockKeepingUnit = "KG";
				AssertEquals(pickLine.WZ_UnitsUQ, pickLine.SupplierPart.OP_StockKeepingUnit);

				pickLine.SupplierPart.OP_PartNum = "PART NO";
				AssertNotNull(pickLine.InventoryLine.Docket.Client);
			}

			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";

			var lineInfo1 = new WhsPickLineInfo(new PickLineGroupingInfo(pickLine1), new[] { pickLine1.PK, pickLine2.PK }, 10m, new WhsPickInfo());

			AssertEquals(new DateTime(2008, 3, 12), lineInfo1.ExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.ExpiryDate.Kind);
			AssertEquals("PartAttr1", lineInfo1.Attribute1);
			AssertEquals("Attr1 Name", lineInfo1.PartAttributes.Attribute1Caption);
			AssertEquals("PartAttr2", lineInfo1.Attribute2);
			AssertEquals("Attr2 Name", lineInfo1.PartAttributes.Attribute2Caption);
			AssertEquals("PartAttr3", lineInfo1.Attribute3);
			AssertEquals("Attr3 Name", lineInfo1.PartAttributes.Attribute3Caption);

			var locationString = data.Whs1.DefaultLocation.ToLocationString();
			AssertEquals(locationString, lineInfo1.Location);
			AssertEquals(data.Whs1.DefaultLocation.RowName, lineInfo1.Row);
			AssertEquals("11", lineInfo1.LocationFormattedCheckDigit);

			var spaceDelimitedLocation = locationString.Replace(data.Whs1.WW_LocationComponentDelimiter[0], ' ');
			AssertEquals(spaceDelimitedLocation, lineInfo1.LocationWithoutDelimiter);
			AssertEquals(spaceDelimitedLocation.SubstringSafe(lineInfo1.Row.Length + 1), lineInfo1.LocationWithoutRowAndDelimiter);
			AssertEquals(data.Whs1.DefaultLocation.OldBarcode, lineInfo1.LocationBarcode);
			AssertEquals(true, lineInfo1.IsSingleProductInLocation);
			AssertEquals(new DateTime(2008, 3, 13), lineInfo1.PackingDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.PackingDate.Kind);
			AssertEquals("Pallet ID", lineInfo1.PalletID);
			AssertEquals(false, lineInfo1.PalletIDNeutral);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1.PK, pickLine2.PK }, lineInfo1.PKs);
			AssertEquals(data.Part1.PK, lineInfo1.ProductPK);
			AssertEquals(10m, lineInfo1.Units);
			AssertEquals("KG", lineInfo1.UnitsUQ);
			AssertEquals(data.Org1.PK, lineInfo1.ClientPK);
			AssertEquals(data.Org1.OH_Code, lineInfo1.ClientCode);
		}

		public void TestConstructorWithPickLineGroupingInfo_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);
			var pickLine = Helper.CreateWhsPickLine(order.Lines[0], inventory, 1m);

			AssertNotNull(pickLine.InventoryLine.Location);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.LocationString);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.Location.OldBarcode);

			pickLine.InventoryLine.WE_ExpiryDate = new ZDate(2008, 3, 12);
			pickLine.InventoryLine.WE_PackingDate = new ZDate(2008, 3, 13);
			pickLine.InventoryLine.WE_PartAttrib1 = "PartAttr1";
			pickLine.InventoryLine.WE_PartAttrib2 = "PartAttr2";
			pickLine.InventoryLine.WE_PartAttrib3 = "PartAttr3";
			pickLine.InventoryLine.WE_SerialNumber = "SN1";
			pickLine.InventoryLine.WE_PalletID = "Pallet ID";
			AssertNotNull(pickLine.SupplierPart);

			pickLine.SupplierPart.OP_StockKeepingUnit = "KG";
			AssertEquals(pickLine.WZ_UnitsUQ, pickLine.SupplierPart.OP_StockKeepingUnit);

			pickLine.SupplierPart.OP_PartNum = "PART NO";
			AssertNotNull(pickLine.InventoryLine.Docket.Client);

			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";

			var lineInfo1 = new WhsPickLineInfo(new PickLineGroupingInfo(pickLine), new[] { pickLine.PK }, 1m, new WhsPickInfo());

			AssertEquals(new DateTime(2008, 3, 12), lineInfo1.ExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.ExpiryDate.Kind);
			AssertEquals("PartAttr1", lineInfo1.Attribute1);
			AssertEquals("Attr1 Name", lineInfo1.PartAttributes.Attribute1Caption);
			AssertEquals("PartAttr2", lineInfo1.Attribute2);
			AssertEquals("Attr2 Name", lineInfo1.PartAttributes.Attribute2Caption);
			AssertEquals("PartAttr3", lineInfo1.Attribute3);
			AssertEquals("Attr3 Name", lineInfo1.PartAttributes.Attribute3Caption);
			AssertEquals("SN1", lineInfo1.SerialNumber);

			var locationString = data.Whs1.DefaultLocation.ToLocationString();
			AssertEquals(locationString, lineInfo1.Location);
			AssertEquals(data.Whs1.DefaultLocation.RowName, lineInfo1.Row);

			var spaceDelimitedLocation = locationString.Replace(data.Whs1.WW_LocationComponentDelimiter[0], ' ');
			AssertEquals(spaceDelimitedLocation, lineInfo1.LocationWithoutDelimiter);
			AssertEquals(spaceDelimitedLocation.SubstringSafe(lineInfo1.Row.Length + 1), lineInfo1.LocationWithoutRowAndDelimiter);
			AssertEquals(data.Whs1.DefaultLocation.OldBarcode, lineInfo1.LocationBarcode);
			AssertEquals(true, lineInfo1.IsSingleProductInLocation);
			AssertEquals(new DateTime(2008, 3, 13), lineInfo1.PackingDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.PackingDate.Kind);
			AssertEquals("Pallet ID", lineInfo1.PalletID);
			AssertEquals(false, lineInfo1.PalletIDNeutral);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine.PK }, lineInfo1.PKs);
			AssertEquals(data.Part1.PK, lineInfo1.ProductPK);
			AssertEquals(1m, lineInfo1.Units);
			AssertEquals("KG", lineInfo1.UnitsUQ);
			AssertEquals(data.Org1.PK, lineInfo1.ClientPK);
			AssertEquals(data.Org1.OH_Code, lineInfo1.ClientCode);
		}

		public void TestConstructorWithPickLineGroupingInfo_UsesWarehouseCountryFormatString()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);
			var pickLine1 = Helper.CreateWhsPickLine(order1.Lines[0], inventory1, 1m);

			var lineInfo1 = new WhsPickLineInfo(new PickLineGroupingInfo(pickLine1), new[] { pickLine1.PK }, 1m, new WhsPickInfo());
			AssertEquals("ddMMyy", lineInfo1.PartAttributes.ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", lineInfo1.PartAttributes.PackingDateFormatString); // Date format for testing

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 1);
			var pickLine2 = Helper.CreateWhsPickLine(order2.Lines[0], inventory2, 1m);

			var lineInfo2 = new WhsPickLineInfo(new PickLineGroupingInfo(pickLine2), new[] { pickLine2.PK }, 1m, new WhsPickInfo());
			AssertEquals("yyMMdd", lineInfo2.PartAttributes.ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", lineInfo2.PartAttributes.PackingDateFormatString); // Date format for testing
		}

		public void TestConstructorWithPickLineGroupingInfo_FixedWidthLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z040302");
			var receive = Helper.CreateWhsReceive(data.Org1, warehouse, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, data.Part1, 10);
			var pickLine1 = Helper.CreateWhsPickLine(order.Lines[0], inventory1, 3m);
			var pickLine2 = Helper.CreateWhsPickLine(order.Lines[0], inventory2, 7m);

			var pickLines = new[] { pickLine1, pickLine2 };

			foreach (var pickLine in pickLines)
			{
				AssertNotNull(pickLine.InventoryLine.Location);
				AssertNotEquals(ZString.Empty, pickLine.InventoryLine.LocationString);
				AssertNotEquals(ZString.Empty, pickLine.InventoryLine.Location.OldBarcode);

				pickLine.InventoryLine.WE_ExpiryDate = new ZDate(2008, 3, 12);
				pickLine.InventoryLine.WE_PackingDate = new ZDate(2008, 3, 13);
				pickLine.InventoryLine.WE_PartAttrib1 = "PartAttr1";
				pickLine.InventoryLine.WE_PartAttrib2 = "PartAttr2";
				pickLine.InventoryLine.WE_PartAttrib3 = "PartAttr3";
				pickLine.InventoryLine.WE_PalletID = "Pallet ID";
				AssertNotNull(pickLine.SupplierPart);

				pickLine.SupplierPart.OP_StockKeepingUnit = "KG";
				AssertEquals(pickLine.WZ_UnitsUQ, pickLine.SupplierPart.OP_StockKeepingUnit);

				pickLine.SupplierPart.OP_PartNum = "PART NO";
				AssertNotNull(pickLine.InventoryLine.Docket.Client);
			}

			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";

			var lineInfo1 = new WhsPickLineInfo(new PickLineGroupingInfo(pickLine1), new[] { pickLine1.PK, pickLine2.PK }, 10m, new WhsPickInfo());

			AssertEquals(new DateTime(2008, 3, 12), lineInfo1.ExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.ExpiryDate.Kind);
			AssertEquals("PartAttr1", lineInfo1.Attribute1);
			AssertEquals("Attr1 Name", lineInfo1.PartAttributes.Attribute1Caption);
			AssertEquals("PartAttr2", lineInfo1.Attribute2);
			AssertEquals("Attr2 Name", lineInfo1.PartAttributes.Attribute2Caption);
			AssertEquals("PartAttr3", lineInfo1.Attribute3);
			AssertEquals("Attr3 Name", lineInfo1.PartAttributes.Attribute3Caption);

			AssertEquals("Z040302", lineInfo1.Location);
			AssertEquals("Z-04-03-02", lineInfo1.Location_UserFriendly);
			AssertEquals(warehouse.DefaultLocation.RowName, lineInfo1.Row);

			AssertEquals("Z 04 03 02", lineInfo1.LocationWithoutDelimiter);
			AssertEquals("04 03 02", lineInfo1.LocationWithoutRowAndDelimiter);
			AssertEquals(location.OldBarcode, lineInfo1.LocationBarcode);
			AssertEquals(true, lineInfo1.IsSingleProductInLocation);
			AssertEquals(new DateTime(2008, 3, 13), lineInfo1.PackingDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.PackingDate.Kind);
			AssertEquals("Pallet ID", lineInfo1.PalletID);
			AssertEquals(false, lineInfo1.PalletIDNeutral);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1.PK, pickLine2.PK }, lineInfo1.PKs);
			AssertEquals(data.Part1.PK, lineInfo1.ProductPK);
			AssertEquals(10m, lineInfo1.Units);
			AssertEquals("KG", lineInfo1.UnitsUQ);
			AssertEquals(data.Org1.PK, lineInfo1.ClientPK);
			AssertEquals(data.Org1.OH_Code, lineInfo1.ClientCode);
		}

		#endregion

		#region TestAdditionalConstructors

		public void TestAdditionalConstructors()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pickLine = Helper.CreateWhsPickLine(order.Lines[0], receive.Inventory[0], 10);

			AssertNotNull(pickLine.InventoryLine.Location);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.LocationString);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.Location.OldBarcode);

			pickLine.InventoryLine.WE_ExpiryDate = new ZDate(2008, 3, 12);
			pickLine.InventoryLine.WE_PackingDate = new ZDate(2008, 3, 13);
			pickLine.InventoryLine.WE_PartAttrib1 = "PartAttr1";
			pickLine.InventoryLine.WE_PartAttrib2 = "PartAttr2";
			pickLine.InventoryLine.WE_PartAttrib3 = "PartAttr3";
			pickLine.InventoryLine.WE_PalletID = "Pallet ID";
			pickLine.WZ_Units = 10m;

			AssertNotNull(pickLine.SupplierPart);
			pickLine.SupplierPart.OP_StockKeepingUnit = "KG";
			AssertEquals(pickLine.WZ_UnitsUQ, pickLine.SupplierPart.OP_StockKeepingUnit);
			pickLine.SupplierPart.OP_PartNum = "PART NO";

			AssertNotNull(pickLine.InventoryLine.Docket.Client);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";

			var lineInfo1 = new WhsPickLineInfo(pickLine, new WhsPickInfo(), "PKG1", 54);
			AssertEquals(new DateTime(2008, 3, 12), lineInfo1.ExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.ExpiryDate.Kind);
			AssertEquals("PartAttr1", lineInfo1.Attribute1);
			AssertEquals("Attr1 Name", lineInfo1.PartAttributes.Attribute1Caption);
			AssertEquals("PartAttr2", lineInfo1.Attribute2);
			AssertEquals("Attr2 Name", lineInfo1.PartAttributes.Attribute2Caption);
			AssertEquals("PartAttr3", lineInfo1.Attribute3);
			AssertEquals("Attr3 Name", lineInfo1.PartAttributes.Attribute3Caption);

			var locationString = pickLine.Inventory.LocationString;
			AssertEquals(locationString, lineInfo1.Location);
			AssertEquals(pickLine.Inventory.Location.RowName, lineInfo1.Row);

			var spaceDelimitedLocation = locationString.Replace(data.Whs1.WW_LocationComponentDelimiter[0], ' ');
			AssertEquals(spaceDelimitedLocation, lineInfo1.LocationWithoutDelimiter);
			AssertEquals(spaceDelimitedLocation.SubstringSafe(lineInfo1.Row.Length + 1), lineInfo1.LocationWithoutRowAndDelimiter);
			AssertEquals(pickLine.Inventory.Location.OldBarcode, lineInfo1.LocationBarcode);
			AssertEquals(true, lineInfo1.IsSingleProductInLocation);
			AssertEquals(new DateTime(2008, 3, 13), lineInfo1.PackingDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.PackingDate.Kind);
			AssertEquals("Pallet ID", lineInfo1.PalletID);
			AssertEquals(false, lineInfo1.PalletIDNeutral);
			AssertEquals(pickLine.PK.ToGuid(), lineInfo1.PKs[0]);
			AssertEquals(pickLine.SupplierPart.PK, lineInfo1.ProductPK);
			AssertEquals(10m, lineInfo1.Units);
			AssertEquals("KG", lineInfo1.UnitsUQ);
			AssertEquals(data.Org1.PK, lineInfo1.ClientPK);
			AssertEquals(data.Org1.OH_Code, lineInfo1.ClientCode);
			AssertEquals("PKG1", lineInfo1.PackageID);
			AssertEquals((short)54, lineInfo1.SlotNumber);
		}

		public void TestAdditionalConstructors_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var pickLine = Helper.CreateWhsPickLine(order.Lines[0], receive.Inventory[0], 1m);

			AssertNotNull(pickLine.InventoryLine.Location);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.LocationString);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.Location.OldBarcode);

			pickLine.InventoryLine.WE_ExpiryDate = new ZDate(2008, 3, 12);
			pickLine.InventoryLine.WE_PackingDate = new ZDate(2008, 3, 13);
			pickLine.InventoryLine.WE_PartAttrib1 = "PartAttr1";
			pickLine.InventoryLine.WE_PartAttrib2 = "PartAttr2";
			pickLine.InventoryLine.WE_PartAttrib3 = "PartAttr3";
			pickLine.InventoryLine.WE_SerialNumber = "SN1";
			pickLine.InventoryLine.WE_PalletID = "Pallet ID";
			pickLine.WZ_Units = 1m;

			AssertNotNull(pickLine.SupplierPart);
			pickLine.SupplierPart.OP_StockKeepingUnit = "KG";
			AssertEquals(pickLine.WZ_UnitsUQ, pickLine.SupplierPart.OP_StockKeepingUnit);
			pickLine.SupplierPart.OP_PartNum = "PART NO";

			AssertNotNull(pickLine.InventoryLine.Docket.Client);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";

			var lineInfo1 = new WhsPickLineInfo(pickLine, new WhsPickInfo(), "PKG1", 54);
			AssertEquals(new DateTime(2008, 3, 12), lineInfo1.ExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.ExpiryDate.Kind);
			AssertEquals("PartAttr1", lineInfo1.Attribute1);
			AssertEquals("Attr1 Name", lineInfo1.PartAttributes.Attribute1Caption);
			AssertEquals("PartAttr2", lineInfo1.Attribute2);
			AssertEquals("Attr2 Name", lineInfo1.PartAttributes.Attribute2Caption);
			AssertEquals("PartAttr3", lineInfo1.Attribute3);
			AssertEquals("Attr3 Name", lineInfo1.PartAttributes.Attribute3Caption);
			AssertEquals("SN1", lineInfo1.SerialNumber);

			var locationString = pickLine.Inventory.LocationString;
			AssertEquals(locationString, lineInfo1.Location);
			AssertEquals(pickLine.Inventory.Location.RowName, lineInfo1.Row);

			var spaceDelimitedLocation = locationString.Replace(data.Whs1.WW_LocationComponentDelimiter[0], ' ');
			AssertEquals(spaceDelimitedLocation, lineInfo1.LocationWithoutDelimiter);
			AssertEquals(spaceDelimitedLocation.SubstringSafe(lineInfo1.Row.Length + 1), lineInfo1.LocationWithoutRowAndDelimiter);
			AssertEquals(pickLine.Inventory.Location.OldBarcode, lineInfo1.LocationBarcode);
			AssertEquals(true, lineInfo1.IsSingleProductInLocation);
			AssertEquals(new DateTime(2008, 3, 13), lineInfo1.PackingDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.PackingDate.Kind);
			AssertEquals("Pallet ID", lineInfo1.PalletID);
			AssertEquals(false, lineInfo1.PalletIDNeutral);
			AssertEquals(pickLine.PK.ToGuid(), lineInfo1.PKs[0]);
			AssertEquals(pickLine.SupplierPart.PK, lineInfo1.ProductPK);
			AssertEquals(1m, lineInfo1.Units);
			AssertEquals("KG", lineInfo1.UnitsUQ);
			AssertEquals(data.Org1.PK, lineInfo1.ClientPK);
			AssertEquals(data.Org1.OH_Code, lineInfo1.ClientCode);
			AssertEquals("PKG1", lineInfo1.PackageID);
			AssertEquals((short)54, lineInfo1.SlotNumber);
		}

		public void TestAdditionalConstructors_FixedWidthLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var location = warehouse.FindLocation("Z040302");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 20, location, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, data.Part1, 10);
			var pickLine = Helper.CreateWhsPickLine(order.Lines[0], receive.Inventory[0], 10);

			AssertNotNull(pickLine.InventoryLine.Location);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.LocationString);
			AssertNotEquals(ZString.Empty, pickLine.InventoryLine.Location.OldBarcode);

			pickLine.InventoryLine.WE_ExpiryDate = new ZDate(2008, 3, 12);
			pickLine.InventoryLine.WE_PackingDate = new ZDate(2008, 3, 13);
			pickLine.InventoryLine.WE_PartAttrib1 = "PartAttr1";
			pickLine.InventoryLine.WE_PartAttrib2 = "PartAttr2";
			pickLine.InventoryLine.WE_PartAttrib3 = "PartAttr3";
			pickLine.InventoryLine.WE_PalletID = "Pallet ID";
			pickLine.WZ_Units = 10m;

			AssertNotNull(pickLine.SupplierPart);
			pickLine.SupplierPart.OP_StockKeepingUnit = "KG";
			AssertEquals(pickLine.WZ_UnitsUQ, pickLine.SupplierPart.OP_StockKeepingUnit);
			pickLine.SupplierPart.OP_PartNum = "PART NO";

			AssertNotNull(pickLine.InventoryLine.Docket.Client);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "Attr1 Name";
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "Attr2 Name";
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "Attr3 Name";

			var lineInfo1 = new WhsPickLineInfo(pickLine, new WhsPickInfo(), "PKG1", 54);
			AssertEquals(new DateTime(2008, 3, 12), lineInfo1.ExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.ExpiryDate.Kind);
			AssertEquals("PartAttr1", lineInfo1.Attribute1);
			AssertEquals("Attr1 Name", lineInfo1.PartAttributes.Attribute1Caption);
			AssertEquals("PartAttr2", lineInfo1.Attribute2);
			AssertEquals("Attr2 Name", lineInfo1.PartAttributes.Attribute2Caption);
			AssertEquals("PartAttr3", lineInfo1.Attribute3);
			AssertEquals("Attr3 Name", lineInfo1.PartAttributes.Attribute3Caption);

			AssertEquals("Z040302", lineInfo1.Location);
			AssertEquals("Z-04-03-02", lineInfo1.Location_UserFriendly);
			AssertEquals("Z", lineInfo1.Row);

			AssertEquals("Z 04 03 02", lineInfo1.LocationWithoutDelimiter);
			AssertEquals("04 03 02", lineInfo1.LocationWithoutRowAndDelimiter);
			AssertEquals(location.OldBarcode, lineInfo1.LocationBarcode);
			AssertEquals(true, lineInfo1.IsSingleProductInLocation);
			AssertEquals(new DateTime(2008, 3, 13), lineInfo1.PackingDate);
			AssertEquals(DateTimeKind.Unspecified, lineInfo1.PackingDate.Kind);
			AssertEquals("Pallet ID", lineInfo1.PalletID);
			AssertEquals(false, lineInfo1.PalletIDNeutral);
			AssertEquals(pickLine.PK.ToGuid(), lineInfo1.PKs[0]);
			AssertEquals(pickLine.SupplierPart.PK, lineInfo1.ProductPK);
			AssertEquals(10m, lineInfo1.Units);
			AssertEquals("KG", lineInfo1.UnitsUQ);
			AssertEquals(data.Org1.PK, lineInfo1.ClientPK);
			AssertEquals(data.Org1.OH_Code, lineInfo1.ClientCode);
			AssertEquals("PKG1", lineInfo1.PackageID);
			AssertEquals((short)54, lineInfo1.SlotNumber);
		}

		#endregion

		#region TestConstructor_SetsIsSingleProductInLocation

		public void TestConstructor_SetsIsSingleProductInLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var pickLineInfo = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals(false, pickLineInfo.IsSingleProductInLocation);
		}

		#endregion

		#region TestConstructor_SetOrderedPartAttributes

		public void TestConstructor_SetOrderedPartAttributes_AttributeNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Factory.Save();
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_PartAttrib1 = "PartAttr1Ordered";
			orderLine.WE_PartAttrib2 = "PartAttr2Ordered";
			orderLine.WE_PartAttrib3 = "PartAttr3Ordered";
			orderLine.WE_SerialNumber = "SerialNumberOrdered";
			orderLine.WE_ExpiryDate = new ZDate(2010, 8, 26);
			orderLine.WE_PackingDate = new ZDate(2010, 8, 16);

			var pickLine = Helper.CreateWhsPickLine(orderLine, receive.Inventory[0], 1m);

			var pickLineInfo1 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("", pickLineInfo1.OrderedPartAttribute1);
			AssertEquals("", pickLineInfo1.OrderedPartAttribute2);
			AssertEquals("", pickLineInfo1.OrderedPartAttribute3);
			AssertEquals("", pickLineInfo1.OrderedSerialNumber);
			AssertEquals(DateTime.MinValue, pickLineInfo1.OrderedExpiryDate);
			AssertEquals(DateTime.MinValue, pickLineInfo1.OrderedPackingDate);

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var pickLineInfo2 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("PartAttr1Ordered", pickLineInfo2.OrderedPartAttribute1);
			AssertEquals("PartAttr2Ordered", pickLineInfo2.OrderedPartAttribute2);
			AssertEquals("PartAttr3Ordered", pickLineInfo2.OrderedPartAttribute3);
			AssertEquals("SerialNumberOrdered", pickLineInfo2.OrderedSerialNumber);
			AssertEquals(new ZDateTime(2010, 8, 26), pickLineInfo2.OrderedExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, pickLineInfo2.OrderedExpiryDate.Kind);
			AssertEquals(new ZDateTime(2010, 8, 16), pickLineInfo2.OrderedPackingDate);
			AssertEquals(DateTimeKind.Unspecified, pickLineInfo2.OrderedPackingDate.Kind);
		}

		public void TestConstructor_SetOrderedPartAttributes_PalletIDNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, location, "PLT");
			Factory.Save();
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine.WE_PartAttrib1 = "PartAttr1Ordered";
			orderLine.WE_PartAttrib2 = "PartAttr2Ordered";
			orderLine.WE_PartAttrib3 = "PartAttr3Ordered";
			orderLine.WE_SerialNumber = "SerialNumberOrdered";
			orderLine.WE_ExpiryDate = new ZDate(2010, 8, 26);
			orderLine.WE_PackingDate = new ZDate(2010, 8, 16);

			var pickLine = Helper.CreateWhsPickLine(orderLine, receive.Inventory[0], 1m);

			var pickLineInfo1 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals(false, location.LocationType.WLT_IsPalletIDNeutral);
			AssertEquals("", pickLineInfo1.OrderedPartAttribute1);
			AssertEquals("", pickLineInfo1.OrderedPartAttribute2);
			AssertEquals("", pickLineInfo1.OrderedPartAttribute3);
			AssertEquals("", pickLineInfo1.OrderedSerialNumber);
			AssertEquals(DateTime.MinValue, pickLineInfo1.OrderedExpiryDate);
			AssertEquals(DateTime.MinValue, pickLineInfo1.OrderedPackingDate);

			location.LocationType.WLT_IsPalletIDNeutral = true;
			var pickLineInfo2 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("PartAttr1Ordered", pickLineInfo2.OrderedPartAttribute1);
			AssertEquals("PartAttr2Ordered", pickLineInfo2.OrderedPartAttribute2);
			AssertEquals("PartAttr3Ordered", pickLineInfo2.OrderedPartAttribute3);
			AssertEquals("SerialNumberOrdered", pickLineInfo2.OrderedSerialNumber);
			AssertEquals(new ZDateTime(2010, 8, 26), pickLineInfo2.OrderedExpiryDate);
			AssertEquals(DateTimeKind.Unspecified, pickLineInfo2.OrderedExpiryDate.Kind);
			AssertEquals(new ZDateTime(2010, 8, 16), pickLineInfo2.OrderedPackingDate);
			AssertEquals(DateTimeKind.Unspecified, pickLineInfo2.OrderedPackingDate.Kind);
		}

		#endregion

		#region TestConstructor_SetPalletIDNeutral

		public void TestConstructor_SetPalletIDNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var palletIdNeutralLocationType = Helper.CreateLocationType("TS1", "Test 1 palletIdNeutral is True", true, 0, LocationClasses.Codes.NOR);
			var palletIdNotNeutralLocationType = Helper.CreateLocationType("TS2", "Test 2 palletIdNeutral is False", false, 0, LocationClasses.Codes.DDL);

			data.Whs1.DefaultLocation.WLV_WLT_LocationType = palletIdNeutralLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.DefaultLocation, "PLT-1");

			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pickLine = Helper.CreateWhsPickLine(order.Lines[0], receive.Inventory[0], 10m);

			var pickLineInfo = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("If location is PalletID Neutral - pickline must have that property set to true", true, pickLineInfo.PalletIDNeutral);

			data.Whs1.DefaultLocation.WLV_WLT_LocationType = palletIdNotNeutralLocationType.PK; // invalid location type
			var pickLineInfo2 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("If location type is invalid - it is NOT PalletID Neutral", false, pickLineInfo2.PalletIDNeutral);
		}

		public void TestConstructor_SetPalletIDNeutral_IsOrderedPalletID()
		{
			TestConstructor_SetPalletIDNeutral(true);
		}

		public void TestConstructor_SetPalletIDNeutral_IsNotOrderedPalletID()
		{
			TestConstructor_SetPalletIDNeutral(false);
		}

		void TestConstructor_SetPalletIDNeutral(bool isOrderedPalletID)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var palletIdNeutralLocationType = Helper.CreateLocationType("TS1", "Test 1 palletIdNeutral is True", true, 0, LocationClasses.Codes.NOR);
			var palletIdNotNeutralLocationType = Helper.CreateLocationType("TS2", "Test 2 palletIdNeutral is False", false, 0, LocationClasses.Codes.DDL);

			data.Whs1.DefaultLocation.WLV_WLT_LocationType = palletIdNeutralLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.DefaultLocation, "PLT-1");

			Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m); //initially no pallet id assigned
			var pickLine = Helper.CreateWhsPickLine(order.Lines[0], receive.Inventory[0], 10m);

			var pickLineInfo1 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("Pallet ID Neutral is set only when location is PalletID Neutral and is not ordered Pallet Id", true, pickLineInfo1.PalletIDNeutral);

			order.Lines[0].WE_PalletID = isOrderedPalletID ? "PLT-1" : ""; //Pallet id assigned when ordered pallet id

			var pickLineInfo2 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("Pallet ID Neutral is set only when location is PalletID Neutral and is not ordered Pallet Id", !isOrderedPalletID, pickLineInfo2.PalletIDNeutral);

			data.Whs1.DefaultLocation.WLV_WLT_LocationType = palletIdNotNeutralLocationType.PK; // not Pallet id neutral location
			var pickLineInfo3 = new WhsPickLineInfo(pickLine, new WhsPickInfo());
			AssertEquals("If location type is invalid - it is NOT PalletID Neutral", false, pickLineInfo3.PalletIDNeutral);
		}

		#endregion

		#region TestConstructor_SetBOMProductPK

		public void TestConstructor_SetBOMProductPK()
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

			pick.AddOrders(new[] { order1 });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var wheelPickLine = orderLine1.ChildComponentLines.Single().PickLines[0];
			var pickLineInfo = new WhsPickLineInfo(wheelPickLine, new WhsPickInfo());
			AssertEquals(bike.PK, pickLineInfo.BOMProductPK);
		}

		#endregion

		#endregion

		#region Properties

		#region TestPK

		public void TestPK()
		{
			AssertEquals(0, Parent.PKs.Length);

			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			Parent.PKs = new[] { guid1, guid2 };
			AssertContainsExactElementsInAnyOrder(new[] { guid1, guid2 }, Parent.PKs);
			AssertEquals(2, Parent.PKs.Length);

			var guid3 = Guid.NewGuid();
			var guid4 = Guid.NewGuid();
			Parent.PKs = new[] { guid3, guid4 };
			AssertContainsExactElementsInAnyOrder(new[] { guid3, guid4 }, Parent.PKs);
			AssertEquals(2, Parent.PKs.Length);
		}

		#endregion

		#region TestClientPK

		public void TestClientPK()
		{
			AssertEquals(Guid.Empty, Parent.ClientPK);

			Guid newGuid = new Guid();
			Parent.ClientPK = newGuid;
			AssertEquals(newGuid, Parent.ClientPK);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			AssertEquals("", Parent.ClientCode);

			Parent.ClientCode = "CLIENT123";
			AssertEquals("CLIENT123", Parent.ClientCode);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			AssertEquals("", Parent.Location);

			Parent.Location = "1234";
			AssertEquals("1234", Parent.Location);

			Parent.Location = "4321";
			AssertEquals("4321", Parent.Location);
		}

		#endregion

		#region TestLocationFormattedCheckDigit

		public void TestLocationFormattedCheckDigit()
		{
			AssertEquals("", Parent.LocationFormattedCheckDigit);

			Parent.LocationFormattedCheckDigit = "12";
			AssertEquals("12", Parent.LocationFormattedCheckDigit);

			Parent.LocationFormattedCheckDigit = "21";
			AssertEquals("21", Parent.LocationFormattedCheckDigit);
		}

		#endregion

		#region TestLocationBarcode

		public void TestLocationBarcode()
		{
			AssertEquals("", Parent.LocationBarcode);

			Parent.LocationBarcode = "1234";
			AssertEquals("1234", Parent.LocationBarcode);

			Parent.LocationBarcode = "4321";
			AssertEquals("4321", Parent.LocationBarcode);
		}

		#endregion

		#region TestLocationWithoutDelimiter

		public void TestLocationWithoutDelimiter()
		{
			AssertEquals("", Parent.LocationWithoutDelimiter);

			Parent.LocationWithoutDelimiter = "A 1 1 1";
			AssertEquals("A 1 1 1", Parent.LocationWithoutDelimiter);

			Parent.LocationWithoutDelimiter = "A 2 2 2";
			AssertEquals("A 2 2 2", Parent.LocationWithoutDelimiter);
		}

		#endregion

		#region TestLocationWithoutRowAndDelimiter

		public void TestLocationWithoutRowAndDelimiter()
		{
			AssertEquals("", Parent.LocationWithoutRowAndDelimiter);

			Parent.LocationWithoutRowAndDelimiter = "1 1 1";
			AssertEquals("1 1 1", Parent.LocationWithoutRowAndDelimiter);

			Parent.LocationWithoutRowAndDelimiter = "2 2 2";
			AssertEquals("2 2 2", Parent.LocationWithoutRowAndDelimiter);
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

		#region TestPalletIDNeutral

		public void TestPalletIDNeutral()
		{
			AssertEquals(false, Parent.PalletIDNeutral);

			Parent.PalletIDNeutral = true;
			AssertEquals(true, Parent.PalletIDNeutral);

			Parent.PalletIDNeutral = false;
			AssertEquals(false, Parent.PalletIDNeutral);
		}

		#endregion

		#region TestProductPK

		public void TestProductPK()
		{
			AssertEquals(Guid.Empty, Parent.ProductPK);

			var newGuid = new Guid();
			Parent.ProductPK = newGuid;
			AssertEquals(newGuid, Parent.ProductPK);
		}

		#endregion

		#region TestPickByBOMProductPK

		public void TestBOMProductPK()
		{
			AssertEquals(Guid.Empty, Parent.BOMProductPK);

			var newGuid = new Guid();
			Parent.BOMProductPK = newGuid;
			AssertEquals(newGuid, Parent.BOMProductPK);
		}

		#endregion

		#region TestRow

		public void TestRow()
		{
			AssertEquals("", Parent.Row);

			Parent.Row = "A";
			AssertEquals("A", Parent.Row);

			Parent.Row = "C";
			AssertEquals("C", Parent.Row);
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

		#region TestPartAttributes

		public void TestPartAttributes()
		{
			AssertNull(Parent.PartAttributes);

			var productPK = Guid.NewGuid();
			var clientPK = Guid.NewGuid();
			var newPartAttributes = new WhsProductPartAttributesInfo { ProductPK = productPK, ClientPK = clientPK };
			var whsPickInfo = new WhsPickInfo();
			whsPickInfo.ProductPartAttributesInfos.Add(newPartAttributes);
			Parent.LinkToPickInfo(whsPickInfo);

			AssertNotEquals(newPartAttributes, Parent.PartAttributes);
			Parent.ProductPK = productPK;
			Parent.ClientPK = clientPK;
			AssertEquals(newPartAttributes, Parent.PartAttributes);

			Parent.ProductPK = Guid.NewGuid();
			AssertNotEquals(newPartAttributes, Parent.PartAttributes);

			Parent.ProductPK = productPK;
			Parent.ClientPK = Guid.NewGuid();
			AssertNotEquals(newPartAttributes, Parent.PartAttributes);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			AssertEquals(0m, Parent.Units);

			Parent.Units = 10m;
			AssertEquals(10m, Parent.Units);

			Parent.Units = 20m;
			AssertEquals(20m, Parent.Units);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			AssertEquals("", Parent.UnitsUQ);

			Parent.UnitsUQ = "1234";
			AssertEquals("1234", Parent.UnitsUQ);

			Parent.UnitsUQ = "4321";
			AssertEquals("4321", Parent.UnitsUQ);
		}

		#endregion

		#region TestOrderedPartAttributes

		#region TestOrderedPartAttribute1

		public void TestOrderedPartAttribute1()
		{
			AssertEquals("", Parent.OrderedPartAttribute1);

			Parent.OrderedPartAttribute1 = "1234";
			AssertEquals("1234", Parent.OrderedPartAttribute1);
		}

		#endregion

		#region TestOrderedPartAttribute2

		public void TestOrderedPartAttribute2()
		{
			AssertEquals("", Parent.OrderedPartAttribute2);

			Parent.OrderedPartAttribute2 = "1234";
			AssertEquals("1234", Parent.OrderedPartAttribute2);
		}

		#endregion

		#region TestOrderedPartAttribute3

		public void TestOrderedPartAttribute3()
		{
			AssertEquals("", Parent.OrderedPartAttribute3);

			Parent.OrderedPartAttribute3 = "1234";
			AssertEquals("1234", Parent.OrderedPartAttribute3);
		}

		#endregion

		#region TestOrderedSerialNumber

		public void TestOrderedSerialNumber()
		{
			AssertEquals("", Parent.OrderedSerialNumber);

			Parent.OrderedSerialNumber = "1234";
			AssertEquals("1234", Parent.OrderedSerialNumber);
		}

		#endregion

		#region TestOrderedExpiryDate

		public void TestOrderedExpiryDate()
		{
			AssertEquals(DateTime.MinValue, Parent.OrderedExpiryDate);

			var today = ZDateTime.Today.ToDateTime();

			Parent.OrderedExpiryDate = today;
			AssertEquals(today, Parent.OrderedExpiryDate);
		}

		#endregion

		#region TestOrderedPackingDate

		public void TestOrderedPackingDate()
		{
			AssertEquals(DateTime.MinValue, Parent.OrderedPackingDate);

			var today = ZDateTime.Today.ToDateTime();

			Parent.OrderedPackingDate = today;
			AssertEquals(today, Parent.OrderedPackingDate);
		}

		#endregion

		#endregion

		#region TestAttachedToPick

		public void TestAttachedToPick()
		{
			AssertEquals("", Parent.AttachedToPick);

			Parent.AttachedToPick = "1234";
			AssertEquals("1234", Parent.AttachedToPick);
		}

		#endregion

		#region TestAttachedToOrder

		public void TestAttachedToOrder()
		{
			AssertEquals("", Parent.AttachedToOrder);

			Parent.AttachedToOrder = "1234";
			AssertEquals("1234", Parent.AttachedToOrder);
		}

		#endregion

		#region TestPackageID

		public void TestPackageID()
		{
			AssertEquals("", Parent.PackageID);

			Parent.PackageID = "1234";
			AssertEquals("1234", Parent.PackageID);
		}

		#endregion

		#region TestSlotNumber

		public void TestSlotNumber()
		{
			AssertEquals((short)0, Parent.SlotNumber);

			Parent.SlotNumber = 10;
			AssertEquals((short)10, Parent.SlotNumber);

			Parent.SlotNumber = 20;
			AssertEquals((short)20, Parent.SlotNumber);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsLocationEmptyAfterPicking

		public void TestIsLocationEmptyAfterPicking()
		{
			AssertEquals(false, Parent.IsLocationEmptyAfterPicking);

			Parent.IsLocationEmptyAfterPicking = true;
			AssertEquals(true, Parent.IsLocationEmptyAfterPicking);

			Parent.IsLocationEmptyAfterPicking = false;
			AssertEquals(false, Parent.IsLocationEmptyAfterPicking);
		}

		#endregion

		#region TestIsSingleProductInLocation

		public void TestIsSingleProductInLocation()
		{
			AssertEquals(false, Parent.IsSingleProductInLocation);

			Parent.IsSingleProductInLocation = true;
			AssertEquals(true, Parent.IsSingleProductInLocation);

			Parent.IsSingleProductInLocation = false;
			AssertEquals(false, Parent.IsSingleProductInLocation);
		}

		#endregion

		#region TestIsVerifiedNonEmpty

		public void TestIsVerifiedNonEmpty()
		{
			AssertEquals(false, Parent.IsVerifiedNonEmpty);

			Parent.IsVerifiedNonEmpty = true;
			AssertEquals(true, Parent.IsVerifiedNonEmpty);

			Parent.IsVerifiedNonEmpty = false;
			AssertEquals(false, Parent.IsVerifiedNonEmpty);
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsPickLineInfo Parent
		{
			get { return (WhsPickLineInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsPickLineInfo();
		}

		#endregion
	}
}

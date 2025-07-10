using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsGroupedDocketLineInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsGroupedUnloadLineInfo>
	{
		#region TestConstructor

		#region TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping

		public void TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_Inventory()
		{
			TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGroupingCore(false);
		}

		public void TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_ASN()
		{
			TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGroupingCore(true);
		}

		void TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGroupingCore(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var lineDifferent = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			line1.WI_PartAttrib1 = "AAA";
			line1.WI_SerialNumber = "SN01";
			line2.WI_PartAttrib1 = "AAA";
			line2.WI_SerialNumber = "SN02";
			lineDifferent.WI_PartAttrib1 = "BBB";
			lineDifferent.WI_SerialNumber = "SN03";

			WhsGroupedUnloadLineInfoCollection collection = null;
			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();

				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 2 grouped lines.", 2, collection.Count);
			var lineWithGroupedQty = collection.Single(l => l.Qty == 2);
			var lineWithSingleQty = collection.Single(l => l.Qty == 1);

			AssertGroupedLines(lineWithGroupedQty, "P1", 2m, "UNT", ZDate.Empty, ZDate.Empty, "", "AAA", "", "", "<Many>");
			AssertGroupedLines(lineWithSingleQty, "P1", 1m, "UNT", ZDate.Empty, ZDate.Empty, "", "BBB", "", "", "SN03");
		}

		#endregion

		#region TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_ReleaseCaptured

		public void TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_ReleaseCaptured_Inventory()
		{
			TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_ReleaseCapturedCore(false);
		}

		public void TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_ReleaseCaptured_ASN()
		{
			TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_ReleaseCapturedCore(true);
		}

		void TestConstructor_GroupByAttribute_SerialNumberShouldIgnoreForGrouping_ReleaseCapturedCore(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var lineDifferent = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			line1.WI_PartAttrib1 = "AAA";
			line2.WI_PartAttrib1 = "AAA";
			lineDifferent.WI_PartAttrib1 = "BBB";

			WhsGroupedUnloadLineInfoCollection collection = null;
			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();

				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 2 grouped lines.", 2, collection.Count);
			var lineWithGroupedQty = collection.Single(l => l.Qty == 2);
			var lineWithSingleQty = collection.Single(l => l.Qty == 1);

			AssertGroupedLines(lineWithGroupedQty, "P1", 2m, "UNT", ZDate.Empty, ZDate.Empty, "", "AAA", "", "", "");
			AssertGroupedLines(lineWithSingleQty, "P1", 1m, "UNT", ZDate.Empty, ZDate.Empty, "", "BBB", "", "", "");
		}

		#endregion

		#region TestConstructor_GroupByAttribute_SerialNumberNotUsed

		public void TestConstructor_GroupByAttribute_SerialNumberNotUsed_Inventory()
		{
			TestConstructor_GroupByAttribute_SerialNumberNotUsed(false);
		}

		public void TestConstructor_GroupByAttribute_SerialNumberNotUsed_ASN()
		{
			TestConstructor_GroupByAttribute_SerialNumberNotUsed(true);
		}

		void TestConstructor_GroupByAttribute_SerialNumberNotUsed(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "Red", "AAA", "M01", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "Red", "AAA", "M01", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "Green", "AAA", "M01", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "Red", "BBB", "M01", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "Red", "AAA", "M02", "");

			WhsGroupedUnloadLineInfoCollection collection;
			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();

				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 4 grouped lines.", 4, collection.Count);
			AssertGroupedLines(collection[0], "P1", 2m, "UNT", ZDate.Empty, ZDate.Empty, "", "Red", "AAA", "M01");
			AssertGroupedLines(collection[1], "P1", 1m, "UNT", ZDate.Empty, ZDate.Empty, "", "Green", "AAA", "M01");
			AssertGroupedLines(collection[2], "P1", 1m, "UNT", ZDate.Empty, ZDate.Empty, "", "Red", "BBB", "M01");
			AssertGroupedLines(collection[3], "P1", 1m, "UNT", ZDate.Empty, ZDate.Empty, "", "Red", "AAA", "M02");
		}

		#endregion

		#region TestConstructor_GroupByExpireDate

		public void TestConstructor_GroupByExpireDate_Inventory()
		{
			TestConstructor_GroupByExpireDate(false);
		}

		public void TestConstructor_GroupByExpireDate_ASN()
		{
			TestConstructor_GroupByExpireDate(true);
		}

		void TestConstructor_GroupByExpireDate(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, new ZDate(Year, 10, 21), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, new ZDate(Year, 10, 21), ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, new ZDate(Year, 10, 22), ZDate.Empty, "", "", "", "");

			WhsGroupedUnloadLineInfoCollection collection = null;

			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();

				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 2 grouped lines.", 2, collection.Count);
			var lineWithGroupedQty = collection.Single(l => l.Qty == 2);
			var lineWithSingleQty = collection.Single(l => l.Qty == 1);
			AssertGroupedLines(lineWithGroupedQty, "P1", 2m, "UNT", new DateTime(Year, 10, 21), ZDateTime.Empty);
			AssertGroupedLines(lineWithSingleQty, "P1", 1m, "UNT", new DateTime(Year, 10, 22), ZDateTime.Empty);
		}

		#endregion

		#region TestConstructor_GroupByPackingDate

		public void TestConstructor_GroupByPackingDate_Inventory()
		{
			TestConstructor_GroupByPackingDate(false);
		}

		public void TestConstructor_GroupByPackingDate_ASN()
		{
			TestConstructor_GroupByPackingDate(true);
		}

		void TestConstructor_GroupByPackingDate(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, new ZDate(Year, 10, 21), "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, new ZDate(Year, 10, 21), "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, new ZDate(Year, 10, 22), "", "", "", "");

			WhsGroupedUnloadLineInfoCollection collection = null;

			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();

				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 2 grouped lines.", 2, collection.Count);
			var lineWithGroupedQty = collection.Single(l => l.Qty == 2);
			var lineWithSingleQty = collection.Single(l => l.Qty == 1);
			AssertGroupedLines(lineWithGroupedQty, "P1", 2m, "UNT", ZDateTime.Empty, new DateTime(Year, 10, 21));
			AssertGroupedLines(lineWithSingleQty, "P1", 1m, "UNT", ZDateTime.Empty, new DateTime(Year, 10, 22));
		}

		#endregion

		#region TestConstructor_GroupByProduct

		public void TestConstructor_GroupByProduct_Inventory()
		{
			TestConstructor_GroupByProduct(false);
		}

		public void TestConstructor_GroupByProduct_ASN()
		{
			TestConstructor_GroupByProduct(true);
		}

		void TestConstructor_GroupByProduct(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);

			WhsGroupedUnloadLineInfoCollection collection = null;

			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();

				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 2 grouped lines.", 2, collection.Count);
			var lineWithGroupedQty = collection.Single(l => l.Qty == 2);
			var lineWithSingleQty = collection.Single(l => l.Qty == 1);
			AssertGroupedLines(lineWithGroupedQty, "P1", 2m, "UNT", DateTime.MinValue, DateTime.MinValue);
			AssertGroupedLines(lineWithSingleQty, "P2", 1m, "UNT", DateTime.MinValue, DateTime.MinValue);
		}

		#endregion

		#region TestConstructor_GroupByPallet

		public void TestConstructor_GroupByPallet_Inventory()
		{
			TestConstructor_GroupByPallet(false);
		}

		public void TestConstructor_GroupByPallet_ASN()
		{
			TestConstructor_GroupByPallet(true);
		}

		void TestConstructor_GroupByPallet(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT2");

			WhsGroupedUnloadLineInfoCollection collection = null;

			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();

				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 2 grouped lines.", 2, collection.Count);
			var lineWithGroupedQty = collection.Single(l => l.Qty == 2);
			var lineWithSingleQty = collection.Single(l => l.Qty == 1);
			AssertGroupedLines(lineWithGroupedQty, "P1", 2m, "UNT", ZDate.Empty, ZDate.Empty, "PLT1");
			AssertGroupedLines(lineWithSingleQty, "P1", 1m, "UNT", ZDate.Empty, ZDate.Empty, "PLT2");
		}

		#endregion

		#region TestConstructor_SerialNumber

		public void TestConstructor_SerialNumber_Inventory()
		{
			TestConstructor_SerialNumberCore(false);
		}

		public void TestConstructor_SerialNumber_AsnLine()
		{
			TestConstructor_SerialNumberCore(true);
		}

		void TestConstructor_SerialNumberCore(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			line1.WI_SerialNumber = "SN01";
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			line2.WI_SerialNumber = "SN02";
			var lineDifferentProduct = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);
			lineDifferentProduct.WI_SerialNumber = "SN03";

			WhsGroupedUnloadLineInfoCollection collection = null;
			if (isASN)
			{
				Factory.Save();
				receive.PopulateASNLines();
				collection = new WhsGroupedUnloadLineInfoCollection(receive.AsnLines.Cast<WhsAsnLine>(), receive.Client, data.Whs1);
			}
			else
			{
				collection = new WhsGroupedUnloadLineInfoCollection(receive.Inventory.Cast<WhsInventoryView>(), receive.Client, data.Whs1);
			}

			AssertEquals("Should create 2 grouped lines.", 2, collection.Count);
			var lineWithGroupedQty = collection.Single(l => l.Qty == 2);
			var lineWithSingleQty = collection.Single(l => l.Qty == 1);

			AssertEquals("<Many>", lineWithGroupedQty.SerialNumber);
			AssertEquals("SN03", lineWithSingleQty.SerialNumber);
		}

		#endregion

		#region TestConstructor_UsesWarehouseCountryFormatString

		public void TestConstructor_UsesWarehouseCountryFormatString_Inventory()
		{
			TestConstructor_UsesWarehouseCountryFormatString(false);
		}

		public void TestConstructor_UsesWarehouseCountryFormatString_ASN()
		{
			TestConstructor_UsesWarehouseCountryFormatString(true);
		}

		void TestConstructor_UsesWarehouseCountryFormatString(bool isASN)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT1");

			WhsGroupedUnloadLineInfoCollection collection1 = null;

			if (isASN)
			{
				Factory.Save();
				receive1.PopulateASNLines();

				collection1 = new WhsGroupedUnloadLineInfoCollection(receive1.AsnLines.Cast<WhsAsnLine>(), receive1.Client, data.Whs1);
			}
			else
			{
				collection1 = new WhsGroupedUnloadLineInfoCollection(receive1.Inventory.Cast<WhsInventoryView>(), receive1.Client, data.Whs1);
			}

			AssertEquals("ddMMyy", collection1.First().PartAttributes.ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", collection1.First().PartAttributes.PackingDateFormatString); // Date format for testing

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, data.Whs1.DefaultLocation, "PLT2");

			WhsGroupedUnloadLineInfoCollection collection2 = null;

			if (isASN)
			{
				Factory.Save();
				receive2.PopulateASNLines();

				collection2 = new WhsGroupedUnloadLineInfoCollection(receive2.AsnLines.Cast<WhsAsnLine>(), receive2.Client, data.Whs1);
			}
			else
			{
				collection2 = new WhsGroupedUnloadLineInfoCollection(receive2.Inventory.Cast<WhsInventoryView>(), receive2.Client, data.Whs1);
			}

			AssertEquals("yyMMdd", collection2.First().PartAttributes.ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", collection2.First().PartAttributes.PackingDateFormatString); // Date format for testing
		}

		#endregion

		#endregion

		#region Implementation

		void AssertGroupedLines(WhsGroupedUnloadLineInfo line, string expectedProduct, ZDecimal expectedQty, string expectedQtyUQ,
			ZDateTime expectedExpiryDate, ZDateTime expectedPackingDate, string expectedPalletID = "",
			string expectedAttribute1 = "", string expectedAttribute2 = "", string expectedAttribute3 = "", string expectedSerialNumber = "")
		{
			AssertNotNull(line.Product);
			AssertEquals(expectedProduct, line.Product.Code);
			AssertEquals(expectedQty, line.Qty);
			AssertEquals(expectedQtyUQ, line.QtyUQ);
			AssertEquals(expectedAttribute1, line.Attribute1);
			AssertEquals(expectedAttribute2, line.Attribute2);
			AssertEquals(expectedAttribute3, line.Attribute3);
			AssertEquals(expectedSerialNumber, line.SerialNumber);
			AssertEquals(expectedExpiryDate, line.ExpiryDate);
			AssertEquals(expectedPackingDate, line.PackingDate);
			AssertEquals(expectedPalletID, line.PalletID);
		}

		int Year
		{
			get { return ZDateTime.Now.Year - 1; }
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsGroupedUnloadLineInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsGroupedUnloadLineInfo);
		}

		protected override WhsGroupedUnloadLineInfo GetNewObjectInfo()
		{
			return new WhsGroupedUnloadLineInfo();
		}

		protected override DataObjectInfoCollection<WhsGroupedUnloadLineInfo> GetNewObjectInfoCollection()
		{
			return new WhsGroupedUnloadLineInfoCollection();
		}

		#endregion
	}
}

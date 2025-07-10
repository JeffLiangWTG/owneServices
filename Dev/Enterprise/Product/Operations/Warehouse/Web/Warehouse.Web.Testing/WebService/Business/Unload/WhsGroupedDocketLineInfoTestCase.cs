using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsGroupedUnloadLineInfo))]
	public class WhsGroupedDocketLineInfoTestCase : DataObjectInfoTestCase<WhsGroupedUnloadLineInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var groupdDocketLine = new WhsGroupedUnloadLineInfo();

			AssertNotNull(groupdDocketLine);
			AssertNotNull(groupdDocketLine.Product);
			AssertNotNull(groupdDocketLine.PartAttributes);
			AssertEquals("", groupdDocketLine.Attribute1);
			AssertEquals("", groupdDocketLine.Attribute2);
			AssertEquals("", groupdDocketLine.Attribute3);
			AssertEquals("", groupdDocketLine.SerialNumber);
			AssertEquals(new DateTime(), groupdDocketLine.ExpiryDate);
			AssertEquals(new DateTime(), groupdDocketLine.PackingDate);
			AssertEquals(0m, groupdDocketLine.Qty);
			AssertEquals("", groupdDocketLine.QtyUQ);
			AssertEquals("", groupdDocketLine.PalletID);
		}

		public void TestConstructor_WithClientAndProduct()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var part = helper.CreateProduct(client, "PART");
			var whs = helper.CreateWarehouse("WHS");

			helper.SetClientAttributeType(client, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory, "Attr1 Name");
			helper.SetClientAttributeType(client, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2 Name");
			helper.SetClientAttributeType(client, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3 Name");
			helper.SetProductAttributeUse(client, part, AttributeNumber.One, true);
			helper.SetProductAttributeUse(client, part, AttributeNumber.Two, true);
			helper.SetProductAttributeUse(client, part, AttributeNumber.Three, false);

			var groupedLineInfo = new WhsGroupedUnloadLineInfo(client, part, whs);

			AssertNotNull(groupedLineInfo);
			AssertNotNull(groupedLineInfo.Product);
			AssertNotNull(groupedLineInfo.PartAttributes);
			AssertEquals("", groupedLineInfo.Attribute1);
			AssertEquals("", groupedLineInfo.Attribute2);
			AssertEquals("", groupedLineInfo.Attribute3);
			AssertEquals("", groupedLineInfo.SerialNumber);
			AssertEquals(new DateTime(), groupedLineInfo.ExpiryDate);
			AssertEquals(new DateTime(), groupedLineInfo.PackingDate);
			AssertEquals(0m, groupedLineInfo.Qty);
			AssertEquals("UNT", groupedLineInfo.QtyUQ);

			AssertEquals("PART", groupedLineInfo.Product.Code);
			AssertEquals("Attr1 Name", groupedLineInfo.PartAttributes.Attribute1Caption);
			AssertEquals(true, groupedLineInfo.PartAttributes.Attribute1IsMandatory);
			AssertEquals(true, groupedLineInfo.PartAttributes.Attribute1IsUsed);
			AssertEquals("Attr2 Name", groupedLineInfo.PartAttributes.Attribute2Caption);
			AssertEquals(false, groupedLineInfo.PartAttributes.Attribute2IsMandatory);
			AssertEquals(true, groupedLineInfo.PartAttributes.Attribute2IsUsed);
			AssertEquals("Attr3 Name", groupedLineInfo.PartAttributes.Attribute3Caption);
			AssertEquals(false, groupedLineInfo.PartAttributes.Attribute3IsMandatory);
			AssertEquals(false, groupedLineInfo.PartAttributes.Attribute3IsUsed);
		}

		public void TestConstructor_MissClient()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var part = helper.CreateProduct(client, "PART");
			var whs = helper.CreateWarehouse("WHS");
			var expectedMessage = @"Client is required
Parameter name: client";

			AssertExceptionThrown(typeof(ArgumentNullException), expectedMessage, () => { new WhsGroupedUnloadLineInfo(null, part, whs); });
		}

		public void TestConstructor_MissProduct()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var whs = helper.CreateWarehouse("WHS");
			var expectedMessage = @"Product is required
Parameter name: part";

			AssertExceptionThrown(typeof(ArgumentNullException), expectedMessage, () => { new WhsGroupedUnloadLineInfo(client, null, whs); });
		}

		public void TestConstructor_UsesWarehouseCountryFormatString()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var part1 = helper.CreateProduct(client, "PART1");
			var part2 = helper.CreateProduct(client, "PART2");
			var whs = helper.CreateWarehouse("WHS");

			Helper.SetClientAllAttributeType(client, true);
			Helper.SetProductAllAttributeUse(client, part1, true);
			Helper.SetProductAllAttributeUse(client, part2, true);

			var groupedLineInfo1 = new WhsGroupedUnloadLineInfo(client, part1, whs);

			AssertEquals("ddMMyy", groupedLineInfo1.PartAttributes.ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", groupedLineInfo1.PartAttributes.PackingDateFormatString); // Date format for testing

			whs.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var groupedLineInfo2 = new WhsGroupedUnloadLineInfo(client, part2, whs);

			AssertEquals("yyMMdd", groupedLineInfo2.PartAttributes.ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", groupedLineInfo2.PartAttributes.PackingDateFormatString); // Date format for testing
		}

		#endregion

		#region Related Property Infos

		public void TestProduct()
		{
			AssertNotNull(Parent.Product);

			WhsProductInfo product = new WhsProductInfo();
			AssertNotEquals(product, Parent.Product);
			Parent.Product = product;
			AssertEquals(product, Parent.Product);
		}

		public void TestPartAttributes()
		{
			AssertNotNull(Parent.PartAttributes);

			WhsProductPartAttributesInfo partAttributes = new WhsProductPartAttributesInfo();
			AssertNotEquals(partAttributes, Parent.PartAttributes);
			Parent.PartAttributes = partAttributes;
			AssertEquals(partAttributes, Parent.PartAttributes);
		}

		#endregion

		#region Properties

		public void TestPalletID()
		{
			AssertEquals("", Parent.PalletID);

			Parent.PalletID = "1234";
			AssertEquals("1234", Parent.PalletID);

			Parent.PalletID = "4321";
			AssertEquals("4321", Parent.PalletID);
		}

		public void TestQty()
		{
			AssertEquals(0m, Parent.Qty);

			Parent.Qty = 10m;
			AssertEquals(10m, Parent.Qty);

			Parent.Qty = 20.20m;
			AssertEquals(20.20m, Parent.Qty);
		}

		public void TestQtyUQ()
		{
			AssertEquals("", Parent.QtyUQ);

			Parent.QtyUQ = "1234";
			AssertEquals("1234", Parent.QtyUQ);

			Parent.QtyUQ = "4321";
			AssertEquals("4321", Parent.QtyUQ);
		}

		public void TestAttribute1()
		{
			AssertEquals("", Parent.Attribute1);

			Parent.Attribute1 = "1234";
			AssertEquals("1234", Parent.Attribute1);

			Parent.Attribute1 = "4321";
			AssertEquals("4321", Parent.Attribute1);
		}

		public void TestAttribute2()
		{
			AssertEquals("", Parent.Attribute2);

			Parent.Attribute2 = "1234";
			AssertEquals("1234", Parent.Attribute2);

			Parent.Attribute2 = "4321";
			AssertEquals("4321", Parent.Attribute2);
		}

		public void TestAttribute3()
		{
			AssertEquals("", Parent.Attribute3);

			Parent.Attribute3 = "1234";
			AssertEquals("1234", Parent.Attribute3);

			Parent.Attribute3 = "4321";
			AssertEquals("4321", Parent.Attribute3);
		}

		public void TestSerialNumber()
		{
			AssertEquals("", Parent.SerialNumber);

			Parent.SerialNumber = "1234";
			AssertEquals("1234", Parent.SerialNumber);

			Parent.SerialNumber = "4321";
			AssertEquals("4321", Parent.SerialNumber);
		}

		public void TestExpiryDate()
		{
			AssertEquals(new DateTime(), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2016, 10, 18);
			AssertEquals(new DateTime(2016, 10, 18), Parent.ExpiryDate);

			Parent.ExpiryDate = new DateTime(2016, 10, 19);
			AssertEquals(new DateTime(2016, 10, 19), Parent.ExpiryDate);
		}

		public void TestPackingDate()
		{
			AssertEquals(new DateTime(), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2016, 10, 18);
			AssertEquals(new DateTime(2016, 10, 18), Parent.PackingDate);

			Parent.PackingDate = new DateTime(2016, 10, 19);
			AssertEquals(new DateTime(2016, 10, 19), Parent.PackingDate);
		}

		#endregion

		#region Implementation

		protected new WhsGroupedUnloadLineInfo Parent
		{
			get { return (WhsGroupedUnloadLineInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsGroupedUnloadLineInfo();
		}

		#endregion
	}
}

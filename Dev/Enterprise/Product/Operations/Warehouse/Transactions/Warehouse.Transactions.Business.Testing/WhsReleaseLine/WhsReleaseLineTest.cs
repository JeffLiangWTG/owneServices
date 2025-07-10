using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReleaseLine))]
	class WhsReleaseLineTest : NonPersistentBusinessObjectTestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			AssertExceptionThrown<ArgumentNullException>(() => new WhsReleaseLine(null));

			orderLine.Delete();
			AssertExceptionThrown(typeof(ArgumentException), "Should not wrap Deleted Objects.", () => new WhsReleaseLine(orderLine));
		}

		#endregion

		#region Related Entities

		#region TestClient

		public void TestClient()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("Client on Release Line should match Docket Client.", data.Org1, releaseLine.Client);
		}

		#endregion

		#region TestCustomFieldsForOrderLineAccessor

		public void TestCustomFieldsForOrderLineAccessor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);

			var today = ZDateTime.Today;
			orderLine.WE_CustomAttrib1 = "CA1";
			orderLine.WE_CustomAttrib2 = "CA2";
			orderLine.WE_CustomAttrib3 = "CA3";
			orderLine.WE_CustomAttrib4 = "CA4";
			orderLine.WE_CustomAttrib5 = "CA5";
			orderLine.WE_CustomAttrib6 = "CA6";

			orderLine.WE_CustomDecimal1 = 1.1m;
			orderLine.WE_CustomDecimal2 = 2.2m;
			orderLine.WE_CustomDecimal3 = 3.3m;
			orderLine.WE_CustomDecimal4 = 4.4m;
			orderLine.WE_CustomDecimal5 = 5.5m;

			orderLine.WE_CustomDate1 = today.AddDays(1);
			orderLine.WE_CustomDate2 = today.AddDays(2);
			orderLine.WE_CustomDate3 = today.AddDays(3);
			orderLine.WE_CustomDate4 = today.AddDays(4);
			orderLine.WE_CustomDate5 = today.AddDays(5);

			orderLine.WE_CustomFlag1 = true;
			orderLine.WE_CustomFlag2 = true;
			orderLine.WE_CustomFlag3 = true;
			orderLine.WE_CustomFlag4 = true;
			orderLine.WE_CustomFlag5 = true;

			orderLine.WE_CustomTextBlob1 = "TEXTBLOB1";
			AssertEquals("CustomFields Accessor should be cached by OrderLine.", releaseLine.CustomFieldsForOrderLineAccessor, releaseLine.CustomFieldsForOrderLineAccessor);
			AssertEquals("CustomFields Accessor should be cached by OrderLine.", releaseLine.CustomFieldsForOrderLineAccessor, new WhsReleaseLine(orderLine).CustomFieldsForOrderLineAccessor);
			AssertExceptionThrown(typeof(InvalidOperationException), "Should only use the CustomFieldsForOrderLineAccessor to access Custom Fields.",
				() => { var poke = releaseLine.CustomFieldsForOrderLineAccessor["WE_TransactionQuantity"]; });

			foreach (CustomLabelInfo customField in new WhsDocketLine.CustomLabelsProvider(order).GetCustomFields(data.Org1, Factory))
			{
				var value = (IZType)orderLine[customField.PropertyName];
				AssertEquals("Precondition: Custom field is set.", false, value.IsEmpty);
				AssertEquals("Value on OrderLine and CustomFields Accessor should be the same.", value, releaseLine.CustomFieldsForOrderLineAccessor[customField.PropertyName]);
			}
		}

		#endregion

		#region TestParentCollection

		public void TestParentCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = orderLine.ReleaseLines.AddNew();
			AssertEquals("Parent Collection should return Order Line's ReleaseLines Collection.", orderLine.ReleaseLines, releaseLine.ParentCollection);
		}

		#endregion

		#region TestPickableDocket

		public void TestPickableDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("PickableDocket on Release Line should match Docket on Order Line.", order, releaseLine.PickableDocket);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("Prodyct on Release Line should match Product on Order Line.", data.Part1, releaseLine.Product.Parent);
		}

		#endregion

		#region TestSupplierPart

		public void TestSupplierPart()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("SupplierPart on Release Line should match Part on Order Line.", data.Part1, releaseLine.SupplierPart);
		}

		#endregion

		#endregion

		#region Properties

		#region TestIsForComponentLines

		public void TestIsForComponentLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_CountDecimalPlaces = 3;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine1 = new WhsReleaseLine(orderLine, true);

			AssertEquals("Precondition", true, releaseLine1.IsForComponentLines);

			var releaseLine2 = new WhsReleaseLine(orderLine);
			AssertEquals("Precondition", false, releaseLine2.IsForComponentLines);
		}

		#endregion

		#region TestDecimalPlaces

		public void TestDecimalPlaces()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_CountDecimalPlaces = 3;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("Decimal Places should match Decimal Places on Product.", 3, releaseLine.DecimalPlaces);

			orderLine.WE_OP = ZGuid.Empty;
			AssertEquals("Decimal Places should be zero when there is no Product.", 0, releaseLine.DecimalPlaces);
		}

		#endregion

		#region Attributes

		#region Infos

		#region TestExpiryDateInfo

		public void TestExpiryDateInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Expiry should be read only by default.", true, releaseLine.ExpiryDateInfo.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			AssertEquals("Expiry should be readonly even if used by Product.", true, releaseLine.ExpiryDateInfo.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("Expiry should be read only when Pick is Finalised.", true, releaseLine.ExpiryDateInfo.ReadOnly);
		}

		#endregion

		#region TestPackingDateInfo

		public void TestPackingDateInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Packing should be read only by default.", true, releaseLine.PackingDateInfo.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			AssertEquals("Packing should be readonly even if used by Product.", true, releaseLine.PackingDateInfo.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("Packing should be read only when Pick is Finalised.", true, releaseLine.PackingDateInfo.ReadOnly);
		}

		#endregion

		#region TestPartAttribute1Info

		public void TestPartAttribute1Info()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("PartAttrib1 should be read only by default.", true, releaseLine.PartAttribute1Info.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			AssertEquals("PartAttrib1 should be read only if used by product but not release captured.", true, releaseLine.PartAttribute1Info.ReadOnly);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Factory.Save(); // Clear Attribute settings cache
			AssertEquals("PartAttrib1 should be editable if used by product AND release captured.", false, releaseLine.PartAttribute1Info.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("PartAttrib1 should be read only when Pick is Finalised.", true, releaseLine.PartAttribute1Info.ReadOnly);

			AssertEquals("PartAttrib1 should have a max length.", WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1.MaxLength, releaseLine.PartAttribute1Info.MaxLength);
		}

		public void TestPartAttribute1Info_WhenPacked()
		{
			AssertReadOnlyWhenPacked(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute2Info

		public void TestPartAttribute2Info()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("PartAttrib2 should be read only by default.", true, releaseLine.PartAttribute2Info.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			AssertEquals("PartAttrib2 should be read only if used by product but not release captured.", true, releaseLine.PartAttribute2Info.ReadOnly);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Factory.Save(); // Clear Attribute settings cache
			AssertEquals("PartAttrib1 should be editable if used by product AND release captured.", false, releaseLine.PartAttribute2Info.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("PartAttrib2 should be read only when Pick is Finalised.", true, releaseLine.PartAttribute2Info.ReadOnly);

			AssertEquals("PartAttrib2 should have a max length.", WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib2.MaxLength, releaseLine.PartAttribute2Info.MaxLength);
		}

		public void TestPartAttribute2Info_WhenPacked()
		{
			AssertReadOnlyWhenPacked(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestPartAttribute3Info

		public void TestPartAttribute3Info()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("PartAttrib3 should be read only by default.", true, releaseLine.PartAttribute3Info.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			AssertEquals("PartAttrib3 should be read only if used by product but not release captured.", true, releaseLine.PartAttribute3Info.ReadOnly);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Factory.Save(); // Clear Attribute settings cache
			AssertEquals("PartAttrib3 should be editable if used by product AND release captured.", false, releaseLine.PartAttribute3Info.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("PartAttrib3 should be read only when Pick is Finalised.", true, releaseLine.PartAttribute3Info.ReadOnly);

			AssertEquals("PartAttrib3 should have a max length.", WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib3.MaxLength, releaseLine.PartAttribute3Info.MaxLength);
		}

		public void TestPartAttribute3Info_WhenPacked()
		{
			AssertReadOnlyWhenPacked(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestSerialNumberInfo

		public void TestSerialNumberInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("SerialNumber should be read only by default.", true, releaseLine.SerialNumberInfo.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			AssertEquals("SerialNumber should be read only if used by product but not release captured.", true, releaseLine.SerialNumberInfo.ReadOnly);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Factory.Save(); // Clear Attribute settings cache
			AssertEquals("SerialNumber should be editable if used by product AND release captured.", false, releaseLine.SerialNumberInfo.ReadOnly);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("SerialNumber should be read only when Pick is Finalised.", true, releaseLine.SerialNumberInfo.ReadOnly);

			AssertEquals("Serial Number should have a max length.", WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber.MaxLength, releaseLine.SerialNumberInfo.MaxLength);
		}

		public void TestSerialNumberInfo_WhenPacked()
		{
			AssertReadOnlyWhenPacked(AttributeNumber.Serial, WhsReleaseLine.Schema.SerialNumber);
		}

		#endregion

		void AssertReadOnlyWhenPacked(AttributeNumber attribNo, string propertyName)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, attribNo, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attribNo, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var orderLine = order.Lines[0];
			AssertEquals("Precondition - 5 items should be picked.", 10m, orderLine.PickLineQuantity);

			// pack 5 items
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX");

			var releaseLine = orderLine.ReleaseLines[0];
			var info = releaseLine.ZPropertyInfoHash[propertyName];
			AssertEquals("Release Captured Attribute by default should be editable.", false, info.ReadOnly);

			package.Pack(releaseLine, 5);
			AssertEquals("Now that Release Line is Packed, the Release Captured Attribs should be read only.", true, info.ReadOnly);
		}

		#endregion

		#region TestOrderedPartAttribute1

		public void TestOrderedPartAttribute1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_PartAttrib1 = "PA1";
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("OrderedPartAttribute1 should match Part Attrib 1 on Order Line.", "PA1", releaseLine.OrderedPartAttribute1);
		}

		#endregion

		#region TestOrderedPartAttribute2

		public void TestOrderedPartAttribute2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_PartAttrib2 = "PA2";
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("OrderedPartAttribute2 should match Part Attrib 2 on Order Line.", "PA2", releaseLine.OrderedPartAttribute2);
		}

		#endregion

		#region TestOrderedPartAttribute3

		public void TestOrderedPartAttribute3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_PartAttrib3 = "PA3";
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("OrderedPartAttribute3 should match Part Attrib 3 on Order Line.", "PA3", releaseLine.OrderedPartAttribute3);
		}

		#endregion

		#region TestOrderedSerialnumber

		public void TestOrderedSerialnumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_SerialNumber = "SN1";
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("OrderedSerialNumber should match Serial Number on Order Line.", "SN1", releaseLine.OrderedSerialNumber);
		}

		#endregion

		#region TestOrderedExpiryDate

		public void TestOrderedExpiryDate()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_ExpiryDate = today;
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("OrderedExpiryDate should match Expiry Date on Order Line.", today, releaseLine.OrderedExpiryDate);
		}

		#endregion

		#region TestOrderedPackingDate

		public void TestOrderedPackingDate()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_PackingDate = today;
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("OrderedPackingDate should match Packing Date on Order Line.", today, releaseLine.OrderedPackingDate);
		}

		#endregion

		#region TestExpiryDate_FiresAttributeChanged

		public void TestExpiryDate_FiresAttributeChanged()
		{
			AssertAttributesChangedFired(PartAttributeNumber.None, r => r.SetExpiryDateForTesting(ZDate.Today));
		}

		#endregion

		#region TestPackingDate_FiresAttributeChanged

		public void TestPackingDate_FiresAttributeChanged()
		{
			AssertAttributesChangedFired(PartAttributeNumber.None, r => r.SetPackingDateForTesting(ZDate.Today));
		}

		#endregion

		#region TestPartAttribute1_FiresAttributeChanged

		public void TestPartAttribute1_FiresAttributeChanged()
		{
			AssertAttributesChangedFired(PartAttributeNumber.One, r => r.PartAttribute1 = "PA1");
		}

		#endregion

		#region TestPartAttribute2_FiresAttributeChanged

		public void TestPartAttribute2_FiresAttributeChanged()
		{
			AssertAttributesChangedFired(PartAttributeNumber.Two, r => r.PartAttribute2 = "PA2");
		}

		#endregion

		#region TestPartAttribute3_FiresAttributeChanged

		public void TestPartAttribute3_FiresAttributeChanged()
		{
			AssertAttributesChangedFired(PartAttributeNumber.Three, r => r.PartAttribute3 = "PA3");
		}

		#endregion

		#region TestSerialNumber_FiresAttributeChanged

		public void TestSerialNumber_FiresAttributeChanged()
		{
			AssertAttributesChangedFired(PartAttributeNumber.SerialNumber, r => r.SerialNumber = "SN1");
		}

		#endregion

		#region TestPartAttribute1_NonPersistentPropertyIsSetProperly

		public void TestPartAttribute1_NonPersistentPropertyIsSetProperly()
		{
			AssertNonPersistentPropertyIsSetProperly(WhsReleaseLine.Schema.PartAttribute1, (ZString)"PA1");
		}

		#endregion

		#region TestPartAttribute2_NonPersistentPropertyIsSetProperly

		public void TestPartAttribute2_NonPersistentPropertyIsSetProperly()
		{
			AssertNonPersistentPropertyIsSetProperly(WhsReleaseLine.Schema.PartAttribute2, (ZString)"PA2");
		}

		#endregion

		#region TestPartAttribute3_NonPersistentPropertyIsSetProperly

		public void TestPartAttribute3_NonPersistentPropertyIsSetProperly()
		{
			AssertNonPersistentPropertyIsSetProperly(WhsReleaseLine.Schema.PartAttribute3, (ZString)"PA3");
		}

		#endregion

		#region TestSerialNumber_NonPersistentPropertyIsSetProperly

		public void TestSerialNumber_NonPersistentPropertyIsSetProperly()
		{
			AssertNonPersistentPropertyIsSetProperly(WhsReleaseLine.Schema.SerialNumber, (ZString)"SN1");
		}

		#endregion

		#region TestQuantity_NonPersistentPropertyIsSetProperly

		public void TestQuantity_NonPersistentPropertyIsSetProperly()
		{
			AssertNonPersistentPropertyIsSetProperly(WhsReleaseLine.Schema.Quantity, (ZDecimal)5m);
		}

		#endregion

		#region TestSerialNumber_ReducesUnitsToOneIfSerialAttributeIsEntered

		public void TestSerialNumber_ReducesUnitsToOneIfSerialAttributeIsEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 5m;
			releaseLine.SerialNumber = "SN1";
			AssertEquals(1m, releaseLine.Quantity);
		}

		#endregion

		#region TestSerialNumber_DoesNotSetQuantityToOneIfQuantityIsZero

		public void TestSerialNumber_DoesNotSetQuantityToOneIfQuantityIsZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 0m;
			releaseLine.SerialNumber = "SN1";
			AssertEquals("Quantity should remain as zero as it should only reduce Quantity to 1.", 0m, releaseLine.Quantity);
		}

		#endregion

		#region TestSerialNumber_DoesNotSetQuantityToOneIfSerialIsEmpty

		public void TestSerialNumber_DoesNotSetQuantityToOneIfSerialIsEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.SerialNumber = "SN1";
			releaseLine.Quantity = 5m;
			releaseLine.SerialNumber = "";
			AssertEquals("Quantity should remain as 5 as it the Serial is Empty.", 5m, releaseLine.Quantity);
		}

		#endregion

		#region TestPartAttribute_UpdatesUnreleasedQty

		#region TestExpiryDate_UpdatesUnreleasedQty_MultiOrder

		public void TestExpiryDate_UpdatesUnreleasedQty_MultiOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_MultiOrder(AttributeNumber.ExpiryDate, WhsReleaseLine.Schema.ExpiryDate, (r, value) => r.SetExpiryDateForTesting(((ZDateTime)value).Date));
		}

		#endregion

		#region TestPackingDate_UpdatesUnreleasedQty_MultiOrder

		public void TestPackingDate_UpdatesUnreleasedQty_MultiOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_MultiOrder(AttributeNumber.PackingDate, WhsReleaseLine.Schema.PackingDate, (r, value) => r.SetPackingDateForTesting(((ZDateTime)value).Date));
		}

		#endregion

		#region TestPartAttribute1_UpdatesUnreleasedQty_MultiOrder

		public void TestPartAttribute1_UpdatesUnreleasedQty_MultiOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_MultiOrder(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute1_IsCaseInsensitive

		public void TestPartAttribute1_IsCaseInsensitive()
		{
			TestPartAttribute_IsCaseInsensitive(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute2_UpdatesUnreleasedQty_MultiOrder

		public void TestPartAttribute2_UpdatesUnreleasedQty_MultiOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_MultiOrder(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestPartAttribute2_IsCaseInsensitive

		public void TestPartAttribute2_IsCaseInsensitive()
		{
			TestPartAttribute_IsCaseInsensitive(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestPartAttribute3_UpdatesUnreleasedQty_MultiOrder

		public void TestPartAttribute3_UpdatesUnreleasedQty_MultiOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_MultiOrder(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestPartAttribute3_IsCaseInsensitive

		public void TestPartAttribute3_IsCaseInsensitive()
		{
			TestPartAttribute_IsCaseInsensitive(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestSerialNumber_UpdatesUnreleasedQty_MultiOrder

		public void TestSerialNumber_UpdatesUnreleasedQty_MultiOrder()
		{
			var a = "A";
			var b = "B";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			inventory1.InDocketLine.WE_SerialNumber = a;
			inventory1.WI_SerialNumber = a;
			inventory2.InDocketLine.WE_SerialNumber = b;
			inventory2.WI_SerialNumber = b;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var order1Line = order1.Lines[0];
			var order2Line = order2.Lines[0];
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Precondition", 1, order2Line.ReleaseLines.Count);

			var order1ReleaseLine1 = order1Line.ReleaseLines[0];
			order1ReleaseLine1.Quantity = 1m;

			var order2ReleaseLine1 = order2Line.ReleaseLines[0];
			order2ReleaseLine1.Quantity = 1m;

			AssertEquals(0m, order1ReleaseLine1.UnreleasedQty);
			AssertEquals(0m, order2ReleaseLine1.UnreleasedQty);
		}

		#endregion

		#region TestSerialNumber_IsCaseInsensitive

		public void TestSerialNumber_IsCaseInsensitive()
		{
			var a = "A";
			var b = "B";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			inventory1.InDocketLine.WE_SerialNumber = a;
			inventory1.WI_SerialNumber = a;
			inventory2.InDocketLine.WE_SerialNumber = b;
			inventory2.WI_SerialNumber = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var order1Line = order1.Lines[0];
			var order2Line = order2.Lines[0];
			order2Line.WE_SerialNumber = a.ToLower(); // Test case insensitive
			order1Line.WE_SerialNumber = b;
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.IsAlterPick = true;
			AssertEquals("Precondition", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Precondition", 1, order2Line.ReleaseLines.Count);

			AssertEquals(b, order1Line.ReleaseLines[0].SerialNumber);
			AssertEquals(a, order2Line.ReleaseLines[0].SerialNumber);

			// Test changing case of part attrib and GetReleaseLineWithNoRCAttribs
			pick.Orders.Remove(order1);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInOtherFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInOtherFactory.IsAlterPick = true;
			var order2Line1InOtherFactory = newFactory.Load<WhsOrderLine>(order2Line.PK);

			var orderedInvForA = pickInOtherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SerialNumber == a.ToLower());
			orderedInvForA.AvailableInventories[0].PickLineQuantity = 1m;
			AssertEquals("Precondition", 1m, pickInOtherFactory.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			AssertEquals("Precondition", 1m, order2Line1InOtherFactory.ReleaseLines[0].Quantity);

			AssertEquals("Precondition", a, order2Line1InOtherFactory.ReleaseLines[0].SerialNumber);
			order2Line1InOtherFactory.ReleaseLines[0].SerialNumber = a.ToLower();
			AssertEquals("Quantity should be unchanged", 1m, order2Line1InOtherFactory.ReleaseLines[0].Quantity);
			AssertEquals("Quantity should be unchanged", 0m, order2Line1InOtherFactory.ReleaseLines[0].UnreleasedQty);

			// Test GetReleaseLineWithNoRCAttribs in RemoveUnreleasedQtyOnReleaseLines
			orderedInvForA.AvailableInventories[0].PickLineQuantity = 0m;
			AssertEquals("Should have deleted the release lines", 0, order2Line1InOtherFactory.ReleaseLines.Count);

			// Test GetReleaseLineWithNoRCAttribs in AddOrUpdateUnreleasedQtyOnReleaseLines
			orderedInvForA.AvailableInventories[0].PickLineQuantity = 1m;
			AssertEquals("Should have increased created another line.", 1, order2Line1InOtherFactory.ReleaseLines.Count);
			AssertEquals("Should have increased release line directly", 1m, order2Line1InOtherFactory.ReleaseLines[0].Quantity);
			AssertEquals("Should have increased release line directly", 0m, order2Line1InOtherFactory.ReleaseLines[0].UnreleasedQty);
			AssertEquals("SerialNumber", a, order2Line1InOtherFactory.ReleaseLines[0].SerialNumber);
		}

		#endregion

		#region TestExpiryDate_UpdatesUnreleasedQty_SingleOrder

		public void TestExpiryDate_UpdatesUnreleasedQty_SingleOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_SingleOrder(AttributeNumber.ExpiryDate, WhsReleaseLine.Schema.ExpiryDate, (r, value) => r.SetExpiryDateForTesting(((ZDateTime)value).Date));
		}

		#endregion

		#region TestPackingDate_UpdatesUnreleasedQty_SingleOrder

		public void TestPackingDate_UpdatesUnreleasedQty_SingleOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_SingleOrder(AttributeNumber.PackingDate, WhsReleaseLine.Schema.PackingDate, (r, value) => r.SetPackingDateForTesting(((ZDateTime)value).Date));
		}

		#endregion

		#region TestPartAttribute1_UpdatesUnreleasedQty_SingleOrder

		public void TestPartAttribute1_UpdatesUnreleasedQty_SingleOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_SingleOrder(AttributeNumber.One, WhsReleaseLine.Schema.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute2_UpdatesUnreleasedQty_SingleOrder

		public void TestPartAttribute2_UpdatesUnreleasedQty_SingleOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_SingleOrder(AttributeNumber.Two, WhsReleaseLine.Schema.PartAttribute2);
		}

		#endregion

		#region TestPartAttribute3_UpdatesUnreleasedQty_SingleOrder

		public void TestPartAttribute3_UpdatesUnreleasedQty_SingleOrder()
		{
			TestPartAttribute_UpdatesUnreleasedQty_SingleOrder(AttributeNumber.Three, WhsReleaseLine.Schema.PartAttribute3);
		}

		#endregion

		#region TestSerialNumber_UpdatesUnreleasedQty_SingleOrder

		public void TestSerialNumber_UpdatesUnreleasedQty_SingleOrder()
		{
			var a = "A";
			var b = "B";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);

			inventory1.InDocketLine.WE_SerialNumber = a;
			inventory2.InDocketLine.WE_SerialNumber = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, orderLine.ReleaseLines.Count);

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 1m;

			var releaseLine2 = orderLine.ReleaseLines[1];
			releaseLine2.Quantity = 1m;

			AssertEquals(0m, releaseLine1.UnreleasedQty);
			AssertEquals(0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestPartAttribute_UpdatesUnreleasedQty

		void TestPartAttribute_UpdatesUnreleasedQty_MultiOrder(AttributeNumber attributeType, string attributeColumn, Action<WhsReleaseLine, IZType> setField = null)
		{
			if (setField == null)
			{
				setField = (r, value) => r[attributeColumn] = value;
			}

			var today = ZDateTime.Today;
			var isDate = attributeType == AttributeNumber.ExpiryDate || attributeType == AttributeNumber.PackingDate;
			var a = isDate ? today.AddDays(5) : (IZType)new ZString("A");
			var b = isDate ? today.AddDays(10) : (IZType)new ZString("B");

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeType, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var columnNameWithUteRemoved = attributeColumn.Replace("ute", "");
			inventory1.InDocketLine["WE_" + columnNameWithUteRemoved] = a;
			inventory2.InDocketLine["WE_" + columnNameWithUteRemoved] = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order1Line = order1.Lines[0];
			var order2Line = order2.Lines[0];
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Precondition", 1, order2Line.ReleaseLines.Count);

			// Release 1 unit of A on each Order (2x unreleased total)
			var order1ReleaseLine1 = order1Line.ReleaseLines[0];
			setField(order1ReleaseLine1, a);
			order1ReleaseLine1.Quantity = 1m;

			var order2ReleaseLine1 = order2Line.ReleaseLines[1];
			setField(order2ReleaseLine1, a);
			order2ReleaseLine1.Quantity = 1m;

			// Release 2 units of B on each order (4x unreleased total)
			var order2ReleaseLine2 = order2Line.ReleaseLines[0];
			setField(order2ReleaseLine2, b);
			order2ReleaseLine2.Quantity = 2m;

			var order1ReleaseLine2 = order1Line.ReleaseLines[1];
			setField(order1ReleaseLine2, b);
			order1ReleaseLine2.Quantity = 2m;

			AssertEquals("Precondition", 8m, order1ReleaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 6m, order1ReleaseLine2.UnreleasedQty);
			AssertEquals("Precondition", 8m, order2ReleaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 6m, order2ReleaseLine2.UnreleasedQty);

			order2ReleaseLine2.Quantity += 1m;
			AssertEquals("A should have 8 unreleased still", 8m, order1ReleaseLine1.UnreleasedQty);
			AssertEquals("B should now have 1 less unreleased unit", 5m, order1ReleaseLine2.UnreleasedQty);
			AssertEquals("A should have 8 unreleased still", 8m, order2ReleaseLine1.UnreleasedQty);
			AssertEquals("B should now have 1 less unreleased unit", 5m, order2ReleaseLine2.UnreleasedQty);

			// Change released units so A is fully released on Order 1 and B is fully released on Order 2
			order1ReleaseLine2.Quantity = 0m;
			order2ReleaseLine2.Quantity = 0m;
			setField(order1ReleaseLine1, a);
			setField(order2ReleaseLine1, b);
			order1ReleaseLine1.Quantity = 10m;
			order2ReleaseLine1.Quantity = 10m;
			AssertEquals("Should be one release line on each order line", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Should be one release line on each order line", 1, order2Line.ReleaseLines.Count);
			AssertEquals("Should be fully released", 0m, order1ReleaseLine1.UnreleasedQty);
			AssertEquals("Should be fully released", 0m, order2ReleaseLine1.UnreleasedQty);

			// Over release B by changing the release line on order 1 for A to be for B
			setField(order1ReleaseLine1, b);
			AssertEquals("Should have added zero quantity line for a on the other order line.", 2, order2Line.ReleaseLines.Count);
			AssertEquals("Should have added zero quantity line for a on the other order line.", a, order2Line.ReleaseLines[1][attributeColumn]);

			AssertEquals("Precondition", 10m, order1ReleaseLine1.Quantity);
			AssertEquals("Precondition", 10m, order2ReleaseLine1.Quantity);
			AssertEquals("Should be 10 units over released", -10m, order1ReleaseLine1.UnreleasedQty);
			AssertEquals("Should be 10 units over released", -10m, order2ReleaseLine1.UnreleasedQty);

			AssertEquals("Precondition", 0m, order2Line.ReleaseLines[1].Quantity);
			AssertEquals("A should be 10 units under released.", 10m, order2Line.ReleaseLines[1].UnreleasedQty);
		}

		void TestPartAttribute_UpdatesUnreleasedQty_SingleOrder(AttributeNumber attributeType, string attributeColumn, Action<WhsReleaseLine, IZType> setField = null)
		{
			if (setField == null)
			{
				setField = (r, value) => r[attributeColumn] = value;
			}

			var today = ZDateTime.Today;
			var isDate = attributeType == AttributeNumber.ExpiryDate || attributeType == AttributeNumber.PackingDate;
			var a = isDate ? today.AddDays(5) : (IZType)new ZString("A");
			var b = isDate ? today.AddDays(10) : (IZType)new ZString("B");

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeType, true);
			// To test this scenario we need 1x RCA for another attribute so it is valid to add new release lines and have duplicates for the attribute we are testing
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeType != AttributeNumber.One ? AttributeNumber.One : AttributeNumber.Two, true, setReleaseCaptured: true); // Some other attribute besides the one we want to use

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var columnNameWithUteRemoved = attributeColumn.Replace("ute", "");
			inventory1.InDocketLine["WE_" + columnNameWithUteRemoved] = a;
			inventory2.InDocketLine["WE_" + columnNameWithUteRemoved] = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, orderLine.ReleaseLines.Count);

			Action<WhsReleaseLine, string> setSomeOtherAttribute = (rl, value) =>
			{
				// Set some other attribute. Use AttributeOne if not one, two if we are setting one
				if (attributeType != AttributeNumber.One)
				{
					rl.PartAttribute1 = value;
				}
				else
				{
					rl.PartAttribute2 = value;
				}
			};

			// Create 2x Release Lines with units 1 each for A
			var releaseLine1 = orderLine.ReleaseLines[0];
			setField(releaseLine1, a);
			setSomeOtherAttribute(releaseLine1, "1");
			releaseLine1.Quantity = 1m;

			var releaseLine2 = orderLine.ReleaseLines[1];
			setField(releaseLine2, a);
			setSomeOtherAttribute(releaseLine2, "2");
			releaseLine2.Quantity = 1m;

			// Create 2x Release Lines with units 1 each for B
			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			setField(releaseLine3, b);
			setSomeOtherAttribute(releaseLine3, "3");
			releaseLine3.Quantity = 1m;

			var releaseLine4 = orderLine.ReleaseLines.AddNew();
			setField(releaseLine4, b);
			setSomeOtherAttribute(releaseLine4, "4");
			releaseLine4.Quantity = 1m;

			AssertEquals("Precondition", 8m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 8m, releaseLine2.UnreleasedQty);
			AssertEquals("Precondition", 8m, releaseLine3.UnreleasedQty);
			AssertEquals("Precondition", 8m, releaseLine4.UnreleasedQty);

			// Change the attribute on one of the release lines for B to A
			setField(releaseLine3, a);
			AssertEquals("A should now have 1 less UnreleasedQty", 7m, releaseLine1.UnreleasedQty);
			AssertEquals("A should now have 1 less UnreleasedQty", 7m, releaseLine2.UnreleasedQty);
			AssertEquals("A should now have 1 less UnreleasedQty", 7m, releaseLine3.UnreleasedQty);
			AssertEquals("The single remaining line for B should now have 1 extra Unreleased Unit.", 9m, releaseLine4.UnreleasedQty);
		}

		#endregion

		#region TestPartAttribute_IsCaseInsensitive

		void TestPartAttribute_IsCaseInsensitive(AttributeNumber attributeType, string attributeColumn)
		{
			var a = "A";
			var b = "B";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, attributeType, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);

			var columnFixedForPartAttribVsPartAttribute = attributeColumn.Replace("ute", "");
			inventory1.InDocketLine["WE_" + columnFixedForPartAttribVsPartAttribute] = a;
			inventory2.InDocketLine["WE_" + columnFixedForPartAttribVsPartAttribute] = b;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order1Line = order1.Lines[0];
			var order2Line1 = order2.Lines[0];
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			order2Line1["WE_" + columnFixedForPartAttribVsPartAttribute] = a.ToLower(); // Test case insensitive
			order2Line2["WE_" + columnFixedForPartAttribVsPartAttribute] = b;
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.IsAlterPick = true;
			order2Line1.ReleaseLines[0].Quantity = 0m;
			order1Line.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Precondition", 0, order2Line1.ReleaseLines.Count);
			AssertEquals("Precondition", 1, order2Line2.ReleaseLines.Count);

			order1Line.ReleaseLines[0].Quantity = 5m;
			AssertEquals("Should have added no new lines.", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Should have added a new zero quantity line.", 1, order2Line1.ReleaseLines.Count);
			AssertEquals("Should *not* have added a zero quantity line.", 1, order2Line2.ReleaseLines.Count);

			AssertEquals("Should have added a new zero quantity line.", a, order2Line1.ReleaseLines[0][attributeColumn]);
			AssertEquals("Should *not* have added a zero quantity line.", b, order2Line2.ReleaseLines[0][attributeColumn]);

			// Test changing case of part attrib and GetReleaseLineWithNoRCAttribs
			pick.Orders.Remove(order1);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInOtherFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInOtherFactory.IsAlterPick = true;
			var order2Line1InOtherFactory = newFactory.Load<WhsOrderLine>(order2Line1.PK);
			var order2Line2InOtherFactory = newFactory.Load<WhsOrderLine>(order2Line2.PK);

			var orderedInvForA = pickInOtherFactory.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ((ZString)ordInv[columnFixedForPartAttribVsPartAttribute]) == a.ToLower());
			orderedInvForA.AvailableInventories[0].PickLineQuantity = 10m;
			AssertEquals("Precondition", 10m, pickInOtherFactory.OrderedInventories[0].AvailableInventories[0].PickLineQuantity);
			AssertEquals("Precondition", 10m, order2Line1InOtherFactory.ReleaseLines[0].Quantity);

			AssertEquals("Precondition", a, order2Line1InOtherFactory.ReleaseLines[0][attributeColumn]);
			order2Line1InOtherFactory.ReleaseLines[0][attributeColumn] = a.ToLower();
			AssertEquals("Quantity should be unchanged", 10m, order2Line1InOtherFactory.ReleaseLines[0].Quantity);
			AssertEquals("Quantity should be unchanged", 0m, order2Line1InOtherFactory.ReleaseLines[0].UnreleasedQty);

			// Test GetReleaseLineWithNoRCAttribs in RemoveUnreleasedQtyOnReleaseLines
			orderedInvForA.AvailableInventories[0].PickLineQuantity = 5m;
			AssertEquals("Should have reduced release line directly", 5m, order2Line1InOtherFactory.ReleaseLines[0].Quantity); // Would have just increased UnreleasedQty if it didnt match release line correctly
			AssertEquals("Should have reduced release line directly", 0m, order2Line1InOtherFactory.ReleaseLines[0].UnreleasedQty);

			// Test GetReleaseLineWithNoRCAttribs in AddOrUpdateUnreleasedQtyOnReleaseLines
			orderedInvForA.AvailableInventories[0].PickLineQuantity = 10m;
			AssertEquals("Should have increased release line directly and not create another line.", 1, order2Line1InOtherFactory.ReleaseLines.Count);
			AssertEquals("Should have increased release line directly", 10m, order2Line1InOtherFactory.ReleaseLines[0].Quantity);
			AssertEquals("Should have increased release line directly", 0m, order2Line1InOtherFactory.ReleaseLines[0].UnreleasedQty);
		}

		#endregion

		#endregion

		#region TestPartAttribute_DeduplicatingMaintainsCache

		public void TestPartAttribute_DeduplicatingMaintainsCache()
		{
			TestPartAttribute_DeduplicatingMaintainsCache(overRelease: false);
		}

		public void TestPartAttribute_DeduplicatingMaintainsCache_OverRelease()
		{
			TestPartAttribute_DeduplicatingMaintainsCache(overRelease: true);
		}

		void TestPartAttribute_DeduplicatingMaintainsCache(bool overRelease)
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory1.InDocketLine.WE_PartAttrib1 = "A";
			inventory2.InDocketLine.WE_PartAttrib1 = "B";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 2, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 2, orderLine.ReleaseLines.Count);

			var aReleaseLine = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1 == "A");
			var bReleaseLine = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(rl => rl.PartAttribute1 == "B");

			aReleaseLine.Quantity = 0m;

			if (overRelease)
			{
				bReleaseLine.Quantity = 11m;
			}

			bReleaseLine.PartAttribute1 = "A";
			AssertEquals("Precondition", true, aReleaseLine.IsDeleted);
			AssertEquals("Precondition", overRelease ? 11m : 10m, bReleaseLine.Quantity);
			AssertEquals("Precondition", overRelease ? -1m : 0m, bReleaseLine.UnreleasedQty);

			bReleaseLine.Quantity = 5m;
			AssertEquals("Precondition", 5m, bReleaseLine.Quantity);
			AssertEquals("Should have updated UnreleasedQuantity", 5m, bReleaseLine.UnreleasedQty);
		}

		public void TestPartAttribute_DeduplicatingMaintainsCache_SingleLine()
		{
			// This case requires extra cache maintenance as PickLinesUnreleasedByAttributes will get deleted
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition", 1, orderLine.ReleaseLines.Count);

			orderLine.ReleaseLines[0].Quantity = 0m;
			orderLine.ReleaseLines.AddNew().Quantity = 10m;
			AssertEquals("Should only be 1 release line.", 1, orderLine.ReleaseLines.Count);
			AssertEquals("Should have updated UnreleasedQuantity", 0m, orderLine.ReleaseLines[0].UnreleasedQty);

			AssertNoExceptionThrown(() => orderLine.ReleaseLines[0].PartAttribute1 = "RED");
			AssertEquals("Caches should be maintained", 0m, orderLine.ReleaseLines[0].UnreleasedQty);
			AssertEquals("Caches should be maintained", 10m, orderLine.ReleaseLines[0].Quantity);

			orderLine.ReleaseLines[0].Quantity = 8m;
			AssertEquals("Caches should be maintained", 2m, orderLine.ReleaseLines[0].UnreleasedQty);
		}

		#endregion

		#region TestReleaseCapturedAttributeForComponent_IsReadOnly

		public void TestReleaseCapturedAttributeForComponent_IsReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, attributeName: "Colour");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, attributeName: "Model");
			Helper.SetProductAttributeUse(data.Org1, bike, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(data.Org1, bike, AttributeNumber.Two, true, false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLocation = data.Whs1.FindLocation("A-1-1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, bike, 5m, inventoryLocation, ZDate.Empty, ZDate.Empty, "", "HARLEY", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, inventoryLocation);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 8m);
			var pick = Helper.CreatePickNew(order);

			var releaseLineFromStock = orderLine.ReleaseLines.Cast<WhsReleaseLine>().FirstOrDefault(r => r.Quantity == 5m);
			AssertEquals("PartAttribute1 should not be read only for release lines from stock.", false, releaseLineFromStock.PartAttribute1Info.ReadOnly);

			var releaseLineBuiltFromComponents = orderLine.ReleaseLines.Cast<WhsReleaseLine>().FirstOrDefault(r => r.Quantity == 3m);
			AssertEquals("PartAttribute1 should be read only for release lines 'built on the fly'.", true, releaseLineBuiltFromComponents.PartAttribute1Info.ReadOnly);
		}

		#endregion

		void AssertAttributesChangedFired(PartAttributeNumber attributeNumber, Action<WhsReleaseLine> setField)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];

			bool attributesChangedFired = false;
			releaseLine.AttributesChanged += (sender, e) =>
			{
				AssertEquals(attributeNumber, e.PartAttribChanged);
				AssertEquals("", e.OldAttributes.PartAttribute1);
				AssertEquals("", e.OldAttributes.PartAttribute2);
				AssertEquals("", e.OldAttributes.PartAttribute3);
				AssertEquals("", e.OldAttributes.SerialNumber);
				AssertEquals(ZDate.Empty, e.OldAttributes.ExpiryDate);
				AssertEquals(ZDate.Empty, e.OldAttributes.PackingDate);

				attributesChangedFired = true;
			};

			setField(releaseLine);
			AssertEquals("Attribute Changed Event should have fired.", true, attributesChangedFired);
		}

		void AssertNonPersistentPropertyIsSetProperly(string propertyName, IZType value)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: HasChanges is false.", false, releaseLine.HasChanges);

			var oldValue = releaseLine[propertyName];
			AssertNotEquals("Should be setting a different value.", oldValue, value);

			bool refreshBindingCalled = false;
			var info = releaseLine.ZPropertyInfoHash[propertyName];
			EventHandler valueChanged = (sender, e) =>
			{
				refreshBindingCalled = true;

				var valueChangedArgs = (ValueChangedEventArgs)e;
				AssertEquals("Refresh Binding should be called with Correct Values.", info, valueChangedArgs.Info);
				AssertEquals("Refresh Binding should be called with Correct Values.", value, valueChangedArgs.NewValue);
				AssertEquals("Refresh Binding should be called with Correct Values.", oldValue, valueChangedArgs.OldValue);
				AssertEquals("Release Line should have Correct Value.", value, releaseLine[propertyName]);
			};
			info.ValueChanged += valueChanged;

			releaseLine[propertyName] = value;
			AssertEquals("HasChanges should be true when setting property.", true, releaseLine.HasChanges);
			AssertEquals("Refresh Binding should be called properly.", true, refreshBindingCalled);
			info.ValueChanged -= valueChanged;

			refreshBindingCalled = false;
			info.ValueChanged += (sender, e) =>
			{
				refreshBindingCalled = !(e is ValueChangedEventArgs);
			};
			releaseLine[propertyName] = value;
			AssertEquals("Setting the same value should call refresh binding.", true, refreshBindingCalled);
		}

		#endregion

		#region TestOrderedQuantity

		public void TestOrderedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 1m;
			AssertEquals("OrderedQuantity should match Quantity on Order Line.", 10m, releaseLine.OrderedQuantity);
		}

		#endregion

		#region TestOrderPK

		public void TestOrderPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("OrderPK on Release Line should match Order PK on Order Line.", order.PK, releaseLine.OrderPK);
		}

		#endregion

		#region TestQuantity

		public void TestQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];

			AssertEquals("Release Lines should have 10m released.", 10m, orderLine.ReleaseLines.SumOfUnitsMet);
			AssertEquals("Release Line should have 10m released.", 10m, releaseLine.Quantity);

			releaseLine.Quantity = 5m;
			AssertEquals("Sum of Units Met should have been updated when setting Quantity.", 5m, orderLine.ReleaseLines.SumOfUnitsMet);
		}

		#endregion

		#region TestQuantity_UpdatesOrder

		public void TestQuantity_UpdatesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			Helper.SetProductWeightAndVolume(data.Part1, 4m, "KG", 1.1m, "M3");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_TotalCubicUnit = "M3";
			order.WD_TotalWeightUnit = "KG";

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", 1m, order.WD_UnitsSent);
			AssertEquals("Precondition", 1.1m, order.WD_CubicSent);
			AssertEquals("Precondition", 4m, order.WD_WeightSent);
			AssertEquals("Precondition", 4m, order.WD_WeightSentUserEntered);

			releaseLine.Quantity = 3m;
			AssertEquals("Fields on the order should be updated.", 3m, order.WD_UnitsSent);
			AssertEquals("Fields on the order should be updated.", 3.3m, order.WD_CubicSent);
			AssertEquals("Fields on the order should be updated.", 12m, order.WD_WeightSent);
			AssertEquals("Fields on the order should be updated.", 12m, order.WD_WeightSentUserEntered);

			releaseLine.Quantity = 1m;
			AssertEquals("Fields on the order should be updated.", 1m, order.WD_UnitsSent);
			AssertEquals("Fields on the order should be updated.", 1.1m, order.WD_CubicSent);
			AssertEquals("Fields on the order should be updated.", 4m, order.WD_WeightSent);
			AssertEquals("Fields on the order should be updated.", 4m, order.WD_WeightSentUserEntered);

			releaseLine.Quantity = 1m;
			AssertEquals("Fields on the order should be updated.", 1m, order.WD_UnitsSent);
			AssertEquals("Fields on the order should be updated.", 1.1m, order.WD_CubicSent);
			AssertEquals("Fields on the order should be updated.", 4m, order.WD_WeightSent);
			AssertEquals("Fields on the order should be updated.", 4m, order.WD_WeightSentUserEntered);

			order.WD_UnitsSent = 0m;
			order.WD_CubicSent = 0m;
			order.WD_WeightSent = 0m;
			releaseLine.Quantity = 0;
			AssertEquals("Fields on the order should be updated.", 0m, order.WD_UnitsSent);
			AssertEquals("Fields on the order should be updated.", 0m, order.WD_CubicSent);
			AssertEquals("Fields on the order should be updated.", 0m, order.WD_WeightSent);
			AssertEquals("Fields on the order should be updated.", 0m, order.WD_WeightSentUserEntered);
		}

		#endregion

		#region TestQuantity_CalculatesExtendedLinePrice

		public void TestQuantity_CalculatesExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.Lines[0].WE_UnitPriceAfterDiscount = 5m;
			Helper.CreatePickNew(order);

			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: LinePrice used transaction quantity prior to picking.", 50m, order.Lines[0].WE_ExtendedLinePrice);

			releaseLine.Quantity = 5m;
			AssertEquals("LinePrice should be updated.", 25m, order.Lines[0].WE_ExtendedLinePrice);

			releaseLine.Quantity = 10m;
			AssertEquals("LinePrice should be updated.", 50m, order.Lines[0].WE_ExtendedLinePrice);

			releaseLine.Quantity = 2m;
			AssertEquals("LinePrice should be updated.", 10m, order.Lines[0].WE_ExtendedLinePrice);
		}

		#endregion

		#region TestQuantity_DoesNotOverrideUserEnteredExtendedLinePrice

		public void TestQuantity_DoesNotOverrideUserEnteredExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.Lines[0].WE_ExtendedLinePrice = 35m;
			Helper.CreatePickNew(order);

			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals("Precondition: LinePrice.", 35m, order.Lines[0].WE_ExtendedLinePrice);

			releaseLine.Quantity = 5m;
			AssertEquals("LinePrice should *not* be updated.", 35m, order.Lines[0].WE_ExtendedLinePrice);

			releaseLine.Quantity = 10m;
			AssertEquals("LinePrice should *not* be updated.", 35m, order.Lines[0].WE_ExtendedLinePrice);
		}

		#endregion

		#region TestQuantity_SetsUnreleasedQuantity

		public void TestQuantity_SetsUnreleasedQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			AssertEquals("Precondition", 20m, releaseLine.Quantity);
			AssertEquals("Precondition", 0m, releaseLine.UnreleasedQty);

			releaseLine.Quantity = 10m;
			AssertEquals("Precondition", 10m, releaseLine.Quantity);
			AssertEquals("UnreleasedQty should be set.", 10m, releaseLine.UnreleasedQty);
		}

		#endregion

		#region TestQuantity_SetsUnreleasedQuantity_MultiOrder

		public void TestQuantity_SetsUnreleasedQuantity_MultiOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order1Line = order1.Lines[0];
			var order2Line = order2.Lines[0];
			var pick = Helper.CreatePickNew(order1, order2);

			var releaseLine1 = (WhsReleaseLine)order1Line.ReleaseLines.Single();
			AssertEquals("Precondition", 0, order2Line.ReleaseLines.Count);

			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);

			releaseLine1.Quantity = 5m;
			AssertEquals("Should have added empty quantity line to the other order.", 1, order2Line.ReleaseLines.Count);
			var releaseLine2 = (WhsReleaseLine)order2Line.ReleaseLines.Single();

			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 5m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 0m, releaseLine2.Quantity);
			AssertEquals("Precondition", 5m, releaseLine2.UnreleasedQty);

			releaseLine2.Quantity = 5m;
			AssertEquals("Precondition.", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition.", 5m, releaseLine2.Quantity);
			AssertEquals("Unreleased Quantity should remain in sync on both orders.", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Unreleased Quantity should remain in sync on both orders.", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestQuantity_SetsUnreleasedQuantity_MultipleClients

		public void TestQuantity_SetsUnreleasedQuantity_MultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(org2, data.Whs1, "O2", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2);

			var releaseLine1 = (WhsReleaseLine)order1.Lines[0].ReleaseLines.Single();
			var releaseLine2 = (WhsReleaseLine)order2.Lines[0].ReleaseLines.Single();
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 5m, releaseLine1.UnreleasedQty);
			AssertEquals("Should not affect other client.", 10m, releaseLine2.Quantity);
			AssertEquals("Should not affect other client.", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestQuantity_SetsUnreleasedQuantity_MultipleParts

		public void TestQuantity_SetsUnreleasedQuantity_MultipleParts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreatePickNew(order);

			var releaseLine1 = (WhsReleaseLine)orderLine1.ReleaseLines.Single();
			var releaseLine2 = (WhsReleaseLine)orderLine2.ReleaseLines.Single();
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 5m, releaseLine1.UnreleasedQty);
			AssertEquals("Should not affect other part.", 10m, releaseLine2.Quantity);
			AssertEquals("Should not affect other part.", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestQuantity_SetsUnreleasedQuantity_MultiplePicks

		public void TestQuantity_SetsUnreleasedQuantity_MultiplePicks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var releaseLine1 = (WhsReleaseLine)order1.Lines[0].ReleaseLines.Single();
			var releaseLine2 = (WhsReleaseLine)order2.Lines[0].ReleaseLines.Single();
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 10m, releaseLine2.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 0m, releaseLine2.UnreleasedQty);

			releaseLine1.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 5m, releaseLine1.UnreleasedQty);
			AssertEquals("Should not affect other pick.", 10m, releaseLine2.Quantity);
			AssertEquals("Should not affect other pick.", 0m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestQuantity_SetsUnreleasedQuantity_ReleaseCaptured

		public void TestQuantity_SetsUnreleasedQuantity_ReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);

			releaseLine1.Quantity = 2m;
			releaseLine1.PartAttribute1 = "PA1-1";
			releaseLine1.PartAttribute2 = "PA2-1";
			releaseLine1.PartAttribute3 = "PA3-1";
			AssertEquals("Precondition", 2m, releaseLine1.Quantity);
			AssertEquals("Should be 8 Unreleased units", 8m, releaseLine1.UnreleasedQty);

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 2m;
			releaseLine2.PartAttribute1 = "PA1-2";
			releaseLine2.PartAttribute2 = "PA2-2";
			releaseLine2.PartAttribute3 = "PA3-2";
			AssertEquals("Precondition", 2m, releaseLine1.Quantity);
			AssertEquals("Precondition", 2m, releaseLine2.Quantity);
			AssertEquals("Unreleased Quantity should be shared and in sync", 6m, releaseLine1.UnreleasedQty);
			AssertEquals("Unreleased Quantity should be shared and in sync", 6m, releaseLine2.UnreleasedQty);

			var releaseLine3 = orderLine.ReleaseLines.AddNew();
			releaseLine3.Quantity = 6m;
			releaseLine3.PartAttribute1 = "PA1-3";
			releaseLine3.PartAttribute2 = "PA2-3";
			releaseLine3.PartAttribute3 = "PA3-3";
			AssertEquals("Precondition", 2m, releaseLine1.Quantity);
			AssertEquals("Precondition", 2m, releaseLine2.Quantity);
			AssertEquals("Precondition", 6m, releaseLine3.Quantity);
			AssertEquals("Unreleased Quantity should be shared and in sync", 0m, releaseLine1.UnreleasedQty);
			AssertEquals("Unreleased Quantity should be shared and in sync", 0m, releaseLine2.UnreleasedQty);
			AssertEquals("Unreleased Quantity should be shared and in sync", 0m, releaseLine3.UnreleasedQty);

			releaseLine3.Quantity -= 4m;
			AssertEquals("Precondition", 2m, releaseLine1.Quantity);
			AssertEquals("Precondition", 2m, releaseLine2.Quantity);
			AssertEquals("Precondition", 2m, releaseLine3.Quantity);
			AssertEquals("Unreleased Quantity should be shared and in sync", 4m, releaseLine1.UnreleasedQty);
			AssertEquals("Unreleased Quantity should be shared and in sync", 4m, releaseLine2.UnreleasedQty);
			AssertEquals("Unreleased Quantity should be shared and in sync", 4m, releaseLine3.UnreleasedQty);
		}

		#endregion

		#region TestQuantity_ClearsUnreleasedQuantityErrors

		public void TestQuantity_ClearsUnreleasedQuantityErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order2Line1 = order2.Lines[0];
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order1, order2);
			var releaseLine1 = (WhsReleaseLine)order1Line1.ReleaseLines.Single();
			var releaseLine2 = (WhsReleaseLine)order1Line2.ReleaseLines.Single();
			var releaseLine3 = (WhsReleaseLine)order2Line1.ReleaseLines.Single();
			var releaseLine4 = (WhsReleaseLine)order2Line2.ReleaseLines.Single();
			releaseLine1.Quantity = 0m;
			releaseLine2.Quantity = 0m;
			releaseLine3.Quantity = 0m;
			releaseLine4.Quantity = 0m;
			AssertEquals("Precondition", 40m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 40m, releaseLine2.UnreleasedQty);
			AssertEquals("Precondition", 40m, releaseLine3.UnreleasedQty);
			AssertEquals("Precondition", 40m, releaseLine4.UnreleasedQty);

			releaseLine1.RunPreSaveValidation();
			releaseLine2.RunPreSaveValidation();
			releaseLine3.RunPreSaveValidation();
			releaseLine4.RunPreSaveValidation();
			AssertHasError(releaseLine1.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
			AssertHasError(releaseLine2.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
			AssertHasError(releaseLine3.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
			AssertHasError(releaseLine4.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");

			releaseLine1.Quantity = 1m;
			AssertNoError(releaseLine1.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
			AssertNoError(releaseLine2.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
			AssertNoError(releaseLine3.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
			AssertNoError(releaseLine4.UnreleasedQtyInfo, "Allocated Quantity and Released Quantity do not match, either correct the Release Lines or Modify Quantity Allocated.");
		}

		#endregion

		#region TestQuantity_RemovesUnneccessaryReleaseLinesOnThePick

		public void TestQuantity_RemovesUnneccessaryReleaseLinesOnThePick()
		{
			TestQuantity_RemovesUnneccessaryReleaseLinesOnThePick_Core();
		}

		public void TestQuantity_RemovesUnneccessaryReleaseLinesOnThePick_OverAllocate()
		{
			TestQuantity_RemovesUnneccessaryReleaseLinesOnThePick_Core(overAllocate: true);
		}

		void TestQuantity_RemovesUnneccessaryReleaseLinesOnThePick_Core(bool overAllocate = false)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15m);
			var order1Line1 = order1.Lines[0];
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var order2Line1 = order2.Lines[0];

			var pick = Helper.CreatePickNew(order1, order2);
			var releaseLine1 = (WhsReleaseLine)order1Line1.ReleaseLines.Single();
			releaseLine1.Quantity = 0m;

			var releaseLine2 = (WhsReleaseLine)order1Line2.ReleaseLines.Single();
			var releaseLine3 = (WhsReleaseLine)order2Line1.ReleaseLines.Single();
			releaseLine2.Quantity = 0m;
			releaseLine3.Quantity = 0m;

			AssertEquals("Precondition", 10m, releaseLine1.UnreleasedQty);
			AssertEquals("Precondition", 10m, releaseLine2.UnreleasedQty);
			AssertEquals("Precondition", 10m, releaseLine3.UnreleasedQty);

			releaseLine1.Quantity = overAllocate ? 15m : 10m;
			AssertEquals("Precondition", true, releaseLine1.UnreleasedQty <= 0m);
			AssertEquals("Precondition", overAllocate, releaseLine1.UnreleasedQty < 0m);

			AssertEquals("Should *not* have deleted the first line.", false, releaseLine1.IsDeleted);
			AssertEquals("Should have deleted the un-needed release lines.", true, releaseLine2.IsDeleted);
			AssertEquals("Should have deleted the un-needed release lines.", true, releaseLine2.IsDeleted);
		}

		#endregion

		#region TestQuantity_ReleaseLineNotInCache

		public void TestQuantity_ReleaseLineNotInCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			AssertEquals("Precondition", 10m, releaseLine1.Quantity);
			AssertEquals("Precondition", 0m, releaseLine1.UnreleasedQty);

			releaseLine1.Quantity = 5m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Should now have unreleased units.", 5m, releaseLine1.UnreleasedQty);

			var releaseLine2 = orderLine.ReleaseLines.AddNew(); // Not added to cache as it does not yet have unique attributes.
			releaseLine2.Quantity = 2m;
			AssertEquals("Precondition", 5m, releaseLine1.Quantity);
			AssertEquals("Precondition", 2m, releaseLine2.Quantity);
			AssertEquals("Unreleased Quantity should be updated and in sync.", 3m, releaseLine1.UnreleasedQty);
			AssertEquals("Unreleased Quantity should be updated and in sync.", 3m, releaseLine2.UnreleasedQty);
		}

		#endregion

		#region TestQuantity_RevertsValueIfUnderPacking

		public void TestQuantity_RevertsValueIfUnderPacking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 6m);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine.IsPacked);

			releaseLine.Quantity = 5m;
			AssertEquals("Invalid Quantity should be reverted.", 10m, releaseLine.Quantity);
			AssertEquals("Invalid Quantity that was reverted needs to be stored.", 5m, releaseLine.InvalidQuantityThatWasReversed);
			AssertHasError(releaseLine.QuantityInfo, "This item is packed.\r\n\r\nQuantity released (5) cannot be less than quantity packed (6). Reduce the quantity packed first.");

			releaseLine.Quantity = 10m;
			AssertEquals("Invalid Quantity should be cleared out.", false, releaseLine.InvalidQuantityThatWasReversed.HasValue);
			AssertNoErrors(releaseLine.QuantityInfo);

			releaseLine.Quantity = 5m;
			AssertEquals("Invalid Quantity should be reverted.", 10m, releaseLine.Quantity);
			AssertEquals("Invalid Quantity that was reverted needs to be stored.", 5m, releaseLine.InvalidQuantityThatWasReversed);
			AssertHasError(releaseLine.QuantityInfo, "This item is packed.\r\n\r\nQuantity released (5) cannot be less than quantity packed (6). Reduce the quantity packed first.");

			releaseLine.RunPreSaveValidation();
			AssertEquals("Invalid Quantity should be cleared out.", false, releaseLine.InvalidQuantityThatWasReversed.HasValue);
			AssertNoErrors(releaseLine.QuantityInfo);
		}

		#endregion

		#region TestQuantity_OrderLineDeletedAndRecreated

		public void TestQuantity_OrderLineDeletedAndRecreatedFromAnotherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition", true, orderLine2.ReleaseLines.Count > 0);
			AssertEquals("Precondition", false, orderLine1.ReleaseLines.Count > 0);
			orderLine2.ReleaseLines[0].Quantity = 8m;

			AssertEquals("Precondition: release lines built for orderline1.", true, orderLine1.ReleaseLines.Count > 0);

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var orderLine1InNewFactory = orderInNewFactory.Lines.Single(line => line.WE_TransactionQuantity == 6m);
			orderLine1InNewFactory.WE_TransactionQuantity = 0m;
			var orderLine3InNewFactory = orderInNewFactory.Lines.AddNew();
			orderLine3InNewFactory.WE_TransactionQuantity = 4m;
			orderLine3InNewFactory.WE_OP = data.Part1.PK;
			orderInNewFactory.IsSavedFromOrderForm = true;
			orderInNewFactory.RunPreSaveValidation();
			newFactory.Save();

			var orderLine3InOrigFactory = Factory.Load<WhsOrderLine>(orderLine3InNewFactory.PK);
			AssertEquals("New order line has release lines in the original factory.", true, orderLine3InOrigFactory.ReleaseLines.Count > 0);
			AssertNoExceptionThrown(() => orderLine3InOrigFactory.ReleaseLines[0].Quantity = 2m);
			AssertNoExceptionThrown(() => orderLine3InOrigFactory.ReleaseLines[0].Quantity = 0m); // creates zero quantity release line
		}

		#endregion

		#region TestQuantity_OrderHasBeenDetachedFromPickInAnotherFactory

		public void TestQuantity_OrderHasBeenDetachedFromPickInAnotherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLineInNewFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
			var releaseLine = (WhsReleaseLine)orderLineInNewFactory.ReleaseLines.Single();
			AssertEquals("Precondition", 5m, releaseLine.Quantity);
			releaseLine.PartAttribute1 = "AAA";

			pick.Orders.Remove(order);
			Factory.Save();

			AssertEquals("ReleaseLine should be removed by DataRefreshBus", 0, orderLineInNewFactory.ReleaseLines.Count);
		}

		#endregion

		#region TestQuantityInfo

		public void TestQuantityInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Quantity should be editable by default.", false, releaseLine.QuantityInfo.ReadOnly);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);
			AssertEquals("Quantity should be read only when Pick is Finalised.", true, releaseLine.QuantityInfo.ReadOnly);
		}

		#endregion

		#region TestQuantity_ValidatesShortfallQuantityOnOrderLine

		public void TestQuantity_ValidatesShortfallQuantityOnOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			AssertEquals("Precondition: No shortfall", 0m, orderLine.WE_ShortfallQuantityCached);
			AssertNoWarnings(orderLine.WE_ShortfallQuantityCachedInfo);

			releaseLine.Quantity = 8m;
			AssertHasWarning(orderLine.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 8 unit(s) have been selected for release");
		}

		#endregion

		#region TestTotalQuantityOrderedFromComponents

		public void TestTotalQuantityOrderedFromComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			mainProduct.OP_StockKeepingUnit = Constants.PkgUnit.Piece;
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

			Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("TotalQuantityOrderedFromComponents should be 5", 5m, releaseLine.TotalQuantityOrderedFromComponents);
		}

		#endregion

		#region TestTotalPickLineQuantityFromComponents

		public void TestTotalPickLineQuantityFromComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			mainProduct.OP_StockKeepingUnit = Constants.PkgUnit.Piece;
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

			Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("TotalPickLineQuantityFromComponents should be 5.", 5m, releaseLine.TotalPickLineQuantityFromComponents);

			var componentPickLine = orderLine.ChildComponentLines.Single().PickLines.Single();
			componentPickLine.WZ_Units = 12m; // reduce kits picked.
			AssertEquals("TotalPickLineQuantityFromComponents should be 4.", 4m, releaseLine.TotalPickLineQuantityFromComponents);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = Constants.PkgUnit.Bottle;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("UnitsUQ should match SKU on Product.", Constants.PkgUnit.Bottle, releaseLine.UnitsUQ);

			orderLine.WE_OP = ZGuid.Empty;
			AssertEquals("UnitsUQ should be 'UNT' when there is no Product.", Constants.PkgUnit.Unit, releaseLine.UnitsUQ);
		}

		#endregion

		#region TestUnitPriceAfterDiscount

		public void TestUnitPriceAfterDiscount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_UnitPriceAfterDiscount = 8146.18m;
			var releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals("UnitPriceAfterDiscount should match WE_UnitPriceAfterDiscount on orderline.", 8146.18m, releaseLine.UnitPriceAfterDiscount);

			orderLine.WE_UnitPriceAfterDiscount = 0m;
			AssertEquals("If WE_UnitPriceAfterDiscount changes so should UnitPriceAfterDiscount.", 0m, releaseLine.UnitPriceAfterDiscount);
		}

		#endregion

		#endregion

		#region Flags

		#region TestAllAttributesEmpty

		public void TestAllAttributesEmpty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals(true, releaseLine.AllAttributesEmpty);

			releaseLine.PartAttribute1 = "PA1";
			AssertEquals(false, releaseLine.AllAttributesEmpty);
			releaseLine.PartAttribute1 = ""; // clean up

			releaseLine.PartAttribute2 = "PA2";
			AssertEquals(false, releaseLine.AllAttributesEmpty);
			releaseLine.PartAttribute2 = ""; // clean up

			releaseLine.PartAttribute3 = "PA3";
			AssertEquals(false, releaseLine.AllAttributesEmpty);
			releaseLine.PartAttribute3 = ""; // clean up

			releaseLine.SerialNumber = "SN1";
			AssertEquals(false, releaseLine.AllAttributesEmpty);
			releaseLine.SerialNumber = ""; // clean up

			releaseLine.SetExpiryDateForTesting(ZDate.Today);
			AssertEquals(false, releaseLine.AllAttributesEmpty);
			releaseLine.SetExpiryDateForTesting(ZDate.Empty); // clean up

			releaseLine.SetPackingDateForTesting(ZDate.Today);
			AssertEquals(false, releaseLine.AllAttributesEmpty);
		}

		#endregion

		#region TestHasChangesWorksAsExpected

		public void TestHasChangesWorksAsExpected()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var releaseLine = new WhsReleaseLine(orderLine);

			bool wasValueChanged = false;
			releaseLine.PartAttribute1Info.ValueChanged += (sender, e) => wasValueChanged = true;
			releaseLine.PartAttribute2Info.ValueChanged += (sender, e) => wasValueChanged = true;
			releaseLine.PartAttribute3Info.ValueChanged += (sender, e) => wasValueChanged = true;
			releaseLine.SerialNumberInfo.ValueChanged += (sender, e) => wasValueChanged = true;
			releaseLine.PackingDateInfo.ValueChanged += (sender, e) => wasValueChanged = true;
			releaseLine.ExpiryDateInfo.ValueChanged += (sender, e) => wasValueChanged = true;
			releaseLine.QuantityInfo.ValueChanged += (sender, e) => wasValueChanged = true;

			releaseLine.HasChanges = true;
			AssertEquals("HasChanges should not have any odd behaviour.", false, wasValueChanged);
		}

		#endregion

		#region TestIsBOMProductPickedOnSalesOrder

		public void TestIsBOMProductPickedOnSalesOrder()
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

			Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("IsComponentPickedOnBOMOrder should be true.", true, releaseLine.IsBOMProductPickedOnSalesOrder);
		}

		#endregion

		#region TestIsPacked

		public void TestIsPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var orderLine = order.Lines[0];
			AssertEquals("Precondition - 5 items should be picked.", 10m, orderLine.PickLineQuantity);

			// pack 5 items
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX");

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: Release Line is not packed.", false, releaseLine.IsPacked);

			package.Pack(releaseLine, 5);
			AssertEquals("Release Line is Packed.", true, releaseLine.IsPacked);

			var releaseLineCopy = orderLine.ReleaseLines.AddNew();
			AssertEquals("Duplicate Release Lines should not be considered Packed.", false, releaseLineCopy.IsPacked);

			var nonCommittedReleaseLine = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertEquals("Non-Committed Release Lines should not be considered Packed.", false, nonCommittedReleaseLine.IsPacked);
		}

		#endregion

		#region TestIsSerialisedProduct

		public void TestIsSerialisedProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals(false, releaseLine.IsSerialisedProduct());

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			AssertEquals(false, releaseLine.IsSerialisedProduct());

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			AssertEquals(true, releaseLine.IsSerialisedProduct());
		}

		#endregion

		#endregion

		#region Delete

		public void TestDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", false, releaseLine.IsDeleted);

			releaseLine.Delete();
			AssertEquals("Release Line should be deleted.", true, releaseLine.IsDeleted);
			AssertEquals("Release Line should be removed.", 0, orderLine.ReleaseLines.Count);
		}

		public void TestDelete_UpdatesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();
			Helper.SetProductWeightAndVolume(data.Part1, 4m, "KG", 1.1m, "M3");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_TotalCubicUnit = "M3";
			order.WD_TotalWeightUnit = "KG";

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition", 1m, order.WD_UnitsSent);
			AssertEquals("Precondition", 1.1m, order.WD_CubicSent);
			AssertEquals("Precondition", 4m, order.WD_WeightSent);
			AssertEquals("Precondition", 4m, order.WD_WeightSentUserEntered);

			releaseLine.Quantity = 3m;
			AssertEquals(3m, order.WD_UnitsSent);
			AssertEquals(3.3m, order.WD_CubicSent);
			AssertEquals(12m, order.WD_WeightSent);
			AssertEquals(12m, order.WD_WeightSentUserEntered);

			bool validationWasSuspendedDuringDelete = false;
			releaseLine.QuantityInfo.ValueChanged += delegate
			{
				validationWasSuspendedDuringDelete = releaseLine.IsValidationSuspended;
			};

			releaseLine.Delete();
			AssertEquals(0m, order.WD_UnitsSent);
			AssertEquals(0m, order.WD_CubicSent);
			AssertEquals(0m, order.WD_WeightSent);
			AssertEquals(0m, order.WD_WeightSentUserEntered);
			AssertEquals("Validation should be suspended during Delete.", true, validationWasSuspendedDuringDelete);
		}

		#endregion

		#region TestGetPackedQty

		public void TestGetPackedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var orderLine = order.Lines[0];
			AssertEquals("Precondition - 5 items should be picked.", 10m, orderLine.PickLineQuantity);

			// pack 5 items
			var package = order.PackageJob.Packages.AddNew("BOX");
			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Release Line should have nothing packed.", 0m, releaseLine.GetPackedQty());

			var packedItems = package.Pack(releaseLine, 5);
			AssertEquals("Release Line should have 5 packed.", 5m, releaseLine.GetPackedQty());

			package.Unpack(packedItems.Single(), 5m);
			AssertEquals("Packed Qty should be zero when nothing is packed.", 0m, releaseLine.GetPackedQty());
		}

		#endregion

		#region TestGetPartAttribute

		public void TestGetPartAttribute()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "PA1";
			releaseLine.PartAttribute2 = "PA2";
			releaseLine.PartAttribute3 = "PA3";
			releaseLine.SerialNumber = "SN1";

			AssertEquals("releaseLine.GetPartAttribute(PartAttributeNumber.One)", "PA1", releaseLine.GetPartAttribute(PartAttributeNumber.One));
			AssertEquals("releaseLine.GetPartAttribute(PartAttributeNumber.Two)", "PA2", releaseLine.GetPartAttribute(PartAttributeNumber.Two));
			AssertEquals("releaseLine.GetPartAttribute(PartAttributeNumber.Three)", "PA3", releaseLine.GetPartAttribute(PartAttributeNumber.Three));
			AssertEquals("releaseLine.GetPartAttribute(PartAttributeNumber.Serial)", "SN1", releaseLine.GetPartAttribute(PartAttributeNumber.SerialNumber));
			AssertEquals("releaseLine.GetPartAttribute(PartAttributeNumber.None)", "", releaseLine.GetPartAttribute(PartAttributeNumber.None));
		}

		#endregion

		#region TestIsForOrderLine

		public void TestIsForOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var line1ReleaseLine1 = orderLine1.ReleaseLines[0];
			var line1ReleaseLine2 = orderLine1.ReleaseLines.AddNew();
			var line2ReleaseLine1 = orderLine2.ReleaseLines[0];

			AssertEquals(true, line1ReleaseLine1.IsForOrderLine(orderLine1));
			AssertEquals(true, line1ReleaseLine2.IsForOrderLine(orderLine1));
			AssertEquals(false, line1ReleaseLine1.IsForOrderLine(orderLine2));
			AssertEquals(false, line1ReleaseLine2.IsForOrderLine(orderLine2));

			AssertEquals(true, line2ReleaseLine1.IsForOrderLine(orderLine2));
			AssertEquals(false, line2ReleaseLine1.IsForOrderLine(orderLine1));
		}

		#endregion

		#region TestRunningValidationBeforePokingReleaseLinesDoesntDoubleUpOnReleaseLineQuantity

		public void TestRunningValidationBeforePokingReleaseLinesDoesntDoubleUpOnReleaseLineQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInOtherFactory = factory2.Load<WhsOrder>(order.PK);

			orderInOtherFactory.Pick.IsAlterPick = true;
			orderInOtherFactory.Pick.PickPriority = 1;

			orderInOtherFactory.Pick.RunPreSaveValidation();
			AssertEquals(10m, orderInOtherFactory.Lines[0].ReleaseLines[0].Quantity);
		}

		#endregion

		#region Validation

		public void TestLightValidatonDisabled()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			AssertEquals(false, new WhsReleaseLine(orderLine).LightValidationEnabled);
		}

		public void TestValidation()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			AssertEquals(typeof(WhsReleaseLineValidation), new WhsReleaseLine(orderLine).Validation.GetType());
		}

		#endregion

		// interfaces

		#region ICanDelete Members

		public void TestICanDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - 5 items should be picked.", 10m, order.Lines[0].PickLineQuantity);

			var releaseLine = order.Lines[0].ReleaseLines[0];
			AssertEquals(false, releaseLine.CanDelete);
			AssertEquals("This item is Allocated to this Order Line and cannot be deleted, reduce the Quantity instead to allow allocations to another Order Line.", releaseLine.ReasonForNotAbleToDelete);

			// pack 5 items
			var package = order.PackageJob.Packages.AddNew("BOX");
			var packedItems = package.Pack(releaseLine, 5);
			AssertEquals(false, releaseLine.CanDelete);
			AssertEquals("This item is packed and cannot be deleted.", releaseLine.ReasonForNotAbleToDelete);

			// unpack the 5 items
			package.Unpack(packedItems.Single(), 5m);
			AssertEquals(false, releaseLine.CanDelete);
			AssertEquals("This item is Allocated to this Order Line and cannot be deleted, reduce the Quantity instead to allow allocations to another Order Line.", releaseLine.ReasonForNotAbleToDelete);
		}

		public void TestIAllowUserToDelete_WithReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			AssertEquals("Precondition: 10 Units released.", 10m, releaseLine1.Quantity);

			releaseLine1.PartAttribute1 = "RED";
			AssertEquals(false, releaseLine1.CanDelete);
			AssertEquals("This item is Allocated to this Order Line and cannot be deleted, reduce the Quantity instead to allow allocations to another Order Line.", releaseLine1.ReasonForNotAbleToDelete);

			releaseLine1.Quantity = 5m;
			AssertEquals(false, releaseLine1.CanDelete);
			AssertEquals("This item is Allocated to this Order Line and cannot be deleted, reduce the Quantity instead to allow allocations to another Order Line.", releaseLine1.ReasonForNotAbleToDelete);

			orderLine2.ReleaseLines[0].Quantity = 10m;
			AssertEquals("Precondition", 0m, releaseLine1.Quantity + releaseLine1.UnreleasedQty);
			AssertEquals("Should be able to delete release line 1 as it is now redundant", true, releaseLine1.CanDelete);

			releaseLine1.Quantity = 0m;
			AssertEquals("Deleted by setting Quantity to 0m.", true, releaseLine1.IsDeleted);

			orderLine2.ReleaseLines[0].Quantity = 5m;
			releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 3m;
			var newReleaseLine = orderLine1.ReleaseLines.AddNew();
			newReleaseLine.Quantity = 2m;
			newReleaseLine.PartAttribute1 = "RED";
			AssertEquals(true, releaseLine1.CanDelete);

			orderLine1.PickLines.Single(l => l.WZ_Units == 3m).WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals(false, releaseLine1.CanDelete);
			AssertEquals("This Release Captured Attribute is picked and cannot be deleted.", releaseLine1.ReasonForNotAbleToDelete);

			orderLine1.PickLines.Single(l => l.WZ_Units == 3m).WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals(true, releaseLine1.CanDelete);

			// pack 3 items
			PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = order.PackageJob.Packages.AddNew("BOX");
			var packedItem = package.Pack(releaseLine1, 3).Single();

			AssertEquals(false, releaseLine1.CanDelete);
			AssertEquals("This item is packed and cannot be deleted.", releaseLine1.ReasonForNotAbleToDelete);

			// unpack the 3 items
			package.Unpack(packedItem, 3m);
			AssertEquals(true, releaseLine1.CanDelete);
		}

		#endregion

		#region IPackableItemParent Members

		#region TestIPackableItemParent

		public void TestIPackableItemParent()
		{
			// setup the client and product attribs
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false, "Colour");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.CreateProductUnit(data.Part1, "BOX", 5);
			data.Part1.OP_Weight = 100;
			data.Part1.OP_WeightUQ = "KG";

			// receive 100 units of red and 100 units of blue stock
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, ZDate.Empty, ZDate.Empty, "Red", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, ZDate.Empty, ZDate.Empty, "Blue", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// order 100 units of stock on a single order line
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			line.WE_PackQuantity = 21; // equivalent of 105 items
			line.WE_F3_NKPackType = "BOX";

			// manually pick 50 red units, and 50 blue units
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - 105 items should have been ordered.", 105m, line.WE_TransactionQuantity);

			var availableInventories = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>();
			var availableInventoryRed = availableInventories.Single(availInv => availInv.PartAttrib1 == "Red");
			var availableInventoryBlue = availableInventories.Single(availInv => availInv.PartAttrib1 == "Blue");

			availableInventoryRed.PickLineQuantity = 50;
			availableInventoryBlue.PickLineQuantity = 50;
			AssertEquals("Precondition - 100 items should have been picked.", 100m, line.SumOfUnitsMet);
			AssertEquals("Precondition - DocketLine should have two AttributesMet lines (one for Red and one for Blue stock).", 2, line.ReleaseLines.Count);

			// test the packable items
			bool redPackableItemFound = false;
			bool bluePackableItemFound = false;

			foreach (WhsReleaseLine releaseLine in line.ReleaseLines)
			{
				IPackableItemParent packableItemParent = releaseLine;
				AssertEquals("P1", packableItemParent.Code);
				AssertEquals(5m, packableItemParent.AutoPackQtyPerPackage);
				AssertEquals("BOX", packableItemParent.AutoPackPackageType);
				AssertEquals("P1", packableItemParent.Description);
				AssertEquals(50m, packableItemParent.TotalQty);
				AssertEquals("UNT", packableItemParent.TotalQtyUQ);
				AssertEquals(100m, packableItemParent.WeightPerUnit);
				AssertEquals("KG", packableItemParent.WeightUQ);

				var attrib1 = packableItemParent.AdditionalProperties.CustomProperties.Single(property => property.Identifier == "Colour");
				if (attrib1.GetValue(releaseLine).ToString() == "Red")
				{
					redPackableItemFound = true;
				}
				else if (attrib1.GetValue(releaseLine).ToString() == "Blue")
				{
					bluePackableItemFound = true;
				}
			}

			AssertEquals("Did not find a packable item with attribute of Colour:Red.", true, redPackableItemFound);
			AssertEquals("Did not find a packable item with attribute of Colour:Blue.", true, bluePackableItemFound);
		}

		#endregion

		#region TestIPackableItemParent_AdditionalPropertiesAndDescriptionSupplement

		public void TestIPackableItemParent_AdditionalProperties_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			// test the packable item's additional properties
			var releaseLine = line.ReleaseLines[0];
			IPackableItemParent packableItemParent = releaseLine;
			var customProperties = packableItemParent.AdditionalProperties.CustomProperties.Cast<CustomPropertyImplementation<WhsReleaseLine>>();
			var serialNumber = customProperties.SingleOrDefault(property => property.Identifier == "Serial Number" && property.Info.GetCaption() == "Serial Number");
			AssertEquals("", serialNumber.GetValue(releaseLine));
		}

		public void TestIPackableItemParent_AdditionalPropertiesAndDescriptionSupplement_English()
		{
			TestIPackableItemParent_AdditionalPropertiesAndDescriptionSupplementCore(true);
		}

		public void TestIPackableItemParent_AdditionalPropertiesAndDescriptionSupplement_NonEnglish()
		{
			TestIPackableItemParent_AdditionalPropertiesAndDescriptionSupplementCore(false);
		}

		void TestIPackableItemParent_AdditionalPropertiesAndDescriptionSupplementCore(bool isEnglish)
		{
			// setup the client and product attribs
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			using (Res.TemporarilySwitchLanguage(isEnglish ? Enterprise.Core.SharedConstants.Languages.English : Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				if (!isEnglish)
				{
					mockRes.Put("8ef52f13-c752-4f32-a65f-b30b32737aa7", new ResourceStringData("8ef52f13-c752-4f32-a65f-b30b32737aa7", "包装日期"));
					mockRes.Put("73b921ff-fb9f-4fa4-821f-6bb1ef445ea5", new ResourceStringData("73b921ff-fb9f-4fa4-821f-6bb1ef445ea5", "有效期"));
					mockRes.Put("7b6b5903-e25f-412d-9292-3f5f19079a52", new ResourceStringData("7b6b5903-e25f-412d-9292-3f5f19079a52", "产品编号"));
				}

				Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false, "Colour");
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false, "Size");
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, false, "Sex");
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, false);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, false);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);

				// receive some stock
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "black", "XL", "M", "A0001", "");
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				// order 10 units with partial specific attributes
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				line.WE_PartAttrib1 = "black";
				line.WE_PartAttrib2 = "XL";
				line.WE_PartAttrib3 = "M";
				line.WE_SerialNumber = "A0001";

				// pick the stock
				var pick = Helper.CreatePickNew(order);
				AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

				// test the packable item's additional properties
				var releaseLine = line.ReleaseLines[0];
				IPackableItemParent packableItemParent = releaseLine;
				var customProperties = packableItemParent.AdditionalProperties.CustomProperties.Cast<CustomPropertyImplementation<WhsReleaseLine>>();
				var attrib1 = customProperties.Single(property => property.Identifier == "Colour" && property.Info.GetCaption() == "Colour");
				var attrib2 = customProperties.Single(property => property.Identifier == "Size" && property.Info.GetCaption() == "Size");
				var attrib3 = customProperties.Single(property => property.Identifier == "Sex" && property.Info.GetCaption() == "Sex");
				var expiry = customProperties.Single(property => property.Identifier == "Expiry" &&
					property.Info.GetCaption() == (isEnglish ? "Expiry" : "有效期"));
				var packing = customProperties.Single(property => property.Identifier == "Packing" &&
					property.Info.GetCaption() == (isEnglish ? "Packing" : "包装日期"));

				AssertEquals("black", attrib1.GetValue(releaseLine));
				AssertEquals("XL", attrib2.GetValue(releaseLine));
				AssertEquals("M", attrib3.GetValue(releaseLine));
				AssertEquals(ZDateTime.Empty, expiry.GetValue(releaseLine));
				AssertEquals(ZDateTime.Empty, packing.GetValue(releaseLine));

				// test description supplement
				AssertEquals("P1", packableItemParent.Code);
				AssertEquals("--", packableItemParent.DescriptionSupplementSeparator);
				AssertEquals("Colour: black, Size: XL, Sex: M, Serial Number: A0001", packableItemParent.DescriptionSupplement);
			}
		}

		#endregion

		#region TestIPackableItemParent_IsMatch_WithDBHits

		public void TestIPackableItemParent_IsMatch_WithDBHits()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);
			data.Part1.OP_StockKeepingUnit = "CTN";

			// setup barcodes
			var part1Barcode1 = data.Part1.PartBarcodes.AddNew();
			var part1Barcode2 = data.Part1.PartBarcodes.AddNew();
			var part1Barcode3 = data.Part1.PartBarcodes.AddNew();
			var part1Barcode4 = data.Part1.PartBarcodes.AddNew();
			part1Barcode1.PH_Barcode = "P1-Barcode";
			part1Barcode1.PH_F3_NKPackType = "UNT";
			part1Barcode2.PH_Barcode = "P1-Barcode-TUN";
			part1Barcode2.PH_F3_NKPackType = "BOX"; // TUN
			part1Barcode3.PH_Barcode = "P1-Barcode-UNT";
			part1Barcode3.PH_F3_NKPackType = "UNT"; // UNT should *not* be considered a TUN
			part1Barcode4.PH_Barcode = "P1-Barcode-SKU";
			part1Barcode4.PH_F3_NKPackType = "CTN"; // CTN is the SKU, thus should *not* be considered a TUN

			// add a unit conversion for the 5x units per BOX
			Helper.CreateProductUnit(data.Part1, "BOX", 5m);

			// order and pick 10 units of each product
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Order should be Picked.", true, order.IsAttachedToPickButNotFinalised); // should save
			AssertEquals("Precondition - Barcodes should be in the DB.", true, part1Barcode1.IsInDatabase);

			// load the order and lines into a new factory
			var otherFactory = new BusinessObjectFactory();
			int initialDbHitsToBarcodeTable = otherFactory.GetTableHitCount(OrgSupplierPartBarcodeSchema.Constants.TableName);
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			var line1InOtherFactory = otherFactory.Load<WhsOrderLine>(line1.PK);
			var line2InOtherFactory = otherFactory.Load<WhsOrderLine>(line2.PK);

			// test the packable item barcodes
			IPackableItemParent packableItemParent1 = line1InOtherFactory.ReleaseLines[0];
			AssertBarcodeMatch(packableItemParent1, "P1", expectMatch: true, matchMsg: "Barcode check should match on product code.");
			AssertBarcodeMatch(packableItemParent1, "P1-Barcode", expectMatch: true);
			AssertBarcodeMatch(packableItemParent1, "P1-Barcode-TUN", expectMatch: true, expectIsTUN: true, expectedPackTypeToPackInto: "BOX", expectedQtyToPack: 5m);
			AssertBarcodeMatch(packableItemParent1, "P1-Barcode-UNT", expectMatch: true); // should not treat UNT as a TUN
			AssertBarcodeMatch(packableItemParent1, "P1-Barcode-SKU", expectMatch: true); // should not treat the SKU (CTN) as a TUN
			AssertBarcodeMatch(packableItemParent1, "Unused-Barcode", expectMatch: false);

			IPackableItemParent packableItemParent2 = line2InOtherFactory.ReleaseLines[0];
			AssertBarcodeMatch(packableItemParent2, "P2", expectMatch: true, matchMsg: "Barcode check should match on product code.");
			AssertBarcodeMatch(packableItemParent2, "P1-Barcode", expectMatch: false);
			AssertBarcodeMatch(packableItemParent2, "P1-Barcode-TUN", expectMatch: false);
			AssertBarcodeMatch(packableItemParent2, "Unused-Barcode", expectMatch: false);

			// test db hits
			int finalDbHitsToBarcodeTable = otherFactory.GetTableHitCount(OrgSupplierPartBarcodeSchema.Constants.TableName);
			AssertEquals("Accessing IsMatch for multiple products should only perform 1 DBHit on the Barcode table.", 1, finalDbHitsToBarcodeTable - initialDbHitsToBarcodeTable);
		}

		void AssertBarcodeMatch(IPackableItemParent packableItemParent, ZString barcode, bool expectMatch, bool expectIsTUN = false, string expectedPackTypeToPackInto = "", decimal expectedQtyToPack = 1, string matchMsg = "")
		{
			if (expectMatch)
			{
				var barcodeMatch = packableItemParent.IsMatch(barcode);
				AssertEquals(matchMsg, expectMatch, barcodeMatch.IsMatch);
				AssertEquals(expectIsTUN, barcodeMatch.IsTUN);
				AssertEquals(expectedPackTypeToPackInto, barcodeMatch.PackTypeToPackInto);
				AssertEquals(expectedQtyToPack, barcodeMatch.QtyToPack);
			}
			else
			{
				AssertEquals(BarcodeMatch.No, packableItemParent.IsMatch(barcode));
			}
		}

		#endregion

		#region TestIPackableItemParent_PackableItems

		public void TestIPackableItemParent_PackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 3m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 2m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 3m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Helper.CreatePickNew(order);

			IPackableItemParent releaseLine1 = orderLine1.ReleaseLines[0];
			IPackableItemParent releaseLine2 = orderLine2.ReleaseLines[0];
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines, releaseLine1.PackableItems);
			AssertContainsExactElementsInAnyOrder(orderLine2.PickLines, releaseLine2.PackableItems);

			orderLine1.ReleaseLines[0].PartAttribute1 = "RED";
			AssertContainsExactElementsInAnyOrder(orderLine1.PickLines, releaseLine1.PackableItems);
		}

		#endregion

		#region TestIPackableItemParent_PickByBOM

		public void TestIPackableItemParent_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Should be 1 Release Line representing Built Kits + Already made Kits.", 1, orderLine.ReleaseLines.Count);
			AssertEquals("Precondition: Should be 2 Pick Lines, 1 directly picked and 1 assembled.", 2, orderLine.PickLines.Count);

			var releaseLine = orderLine.ReleaseLines[0];
			IPackableItemParent packableItemParent = releaseLine;
			AssertEquals("Should be 1 Release Line representing Built Kits + Already made Kits.", 10m, releaseLine.Quantity);
			AssertEquals("Total Quantity should be Quantity of All Kits.", 10m, packableItemParent.TotalQty);
		}

		#endregion

		#region TestIPackableItemParent_RefreshPackableItems

		public void TestIPackableItemParent_RefreshPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 5);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-1-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-1-2"));

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var releaseLines = orderLine.ReleaseLines;

			var packingHelper = new PackingTestHelper(Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");

			var packedItem1 = package.Pack_ForTesting(releaseLines[0], 3m);
			var wrapPackedItemsWithBarcodeYes = new PkgPackageItemDivotsWrapperAndBarcode[] { new PkgPackageItemDivotsWrapperAndBarcode(packedItem1, BarcodeMatch.Yes) };
			var bizO = new UnpackItemsBusinessObject(packageJob, wrapPackedItemsWithBarcodeYes, Enumerable.Empty<PkgPackage>());
			bizO.PackableItemParentsForBinding.FindByPackedItem(packedItem1).ProposedRemoveQty = 2m;
			bizO.RunValidationAndApplyChanges();

			bizO.RunValidationAndApplyChanges();
			AssertEquals("Package should not be deleted.", false, package.IsDeleted);
			AssertEquals("There should be 3 picklines.", 3, packedItem1.PackableItemParent.PackableItems.Count());
		}

		#endregion

		public void TestIPackableItemParent_UnitPrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsIsRecalculateOrderPricing = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine.WE_UnitPriceAfterDiscount = 10m;
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine.CalculateExtendedLinePrice();
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Extended line price is calculated.", 200m, orderLine.WE_ExtendedLinePrice);
			AssertEquals("Sum of Units Met", 20m, orderLine.SumOfUnitsMet);

			var releaseLine = orderLine.ReleaseLines.Single();
			AssertEquals("USD", ((IPackableItemParent)releaseLine).UnitPrice.Currency.Code);
			AssertEquals(10m, ((IPackableItemParent)releaseLine).UnitPrice.Amount);
		}

		public void TestIPackableItemParent_UnitPrice_NoUnitPriceAfterDiscount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine.WE_UnitPriceAfterDiscount = 0m;
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine.WE_ExtendedLinePrice = 100m;
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Extended line price", 100m, orderLine.WE_ExtendedLinePrice);
			AssertEquals("Sum of Units Met", 20m, orderLine.SumOfUnitsMet);

			var releaseLine = orderLine.ReleaseLines.Single();
			var unitPrice = ((IPackableItemParent)releaseLine).UnitPrice;
			AssertEquals("USD", unitPrice.Currency.Code);
			AssertEquals(5m, unitPrice.Amount);
		}

		public void TestIPackableItemParent_UnitPrice_NoCurrency()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCurrency("JPY");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine.WE_ExtendedLinePrice = 100m;
			orderLine.WE_RX_NKUnitPriceCurrency = "";
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition: unit price currency", string.Empty, orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: Extended line price", 100m, orderLine.WE_ExtendedLinePrice);
			AssertEquals("Precondition: Sum of Units Met", 20m, orderLine.SumOfUnitsMet);

			var releaseLine = orderLine.ReleaseLines.Single();
			var unitPrice = ((IPackableItemParent)releaseLine).UnitPrice;
			AssertEquals("JPY", unitPrice.Currency.Code);
			AssertEquals(5m, unitPrice.Amount);
		}

		public void TestIPackableItemParent_UnitPrice_InvalidCurrency()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCurrency("JPY");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine.WE_ExtendedLinePrice = 100m;
			orderLine.WE_RX_NKUnitPriceCurrency = "XXX";
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition: unit price currency", "XXX", orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: Extended line price", 100m, orderLine.WE_ExtendedLinePrice);
			AssertEquals("Precondition: Sum of Units Met", 20m, orderLine.SumOfUnitsMet);

			var releaseLine = orderLine.ReleaseLines.Single();
			var unitPrice = ((IPackableItemParent)releaseLine).UnitPrice;
			AssertEquals("JPY", unitPrice.Currency.Code);
			AssertEquals(5m, unitPrice.Amount);
		}

		public void TestIPackableItemParent_UnitPrice_NoExtendedLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine.WE_RX_NKUnitPriceCurrency = "USD";
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition: unit price currency", "USD", orderLine.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: Extended line price", 0m, orderLine.WE_ExtendedLinePrice);
			AssertEquals("Precondition: Sum of Units Met", 20m, orderLine.SumOfUnitsMet);

			var releaseLine = orderLine.ReleaseLines.Single();
			var unitPrice = ((IPackableItemParent)releaseLine).UnitPrice;
			AssertEquals("USD", unitPrice.Currency.Code);
			AssertEquals(0m, unitPrice.Amount);
		}

		public void TestIPackableItemParent_UnitPrice_CurrencyFromSiblingDocketLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			GlbCompany.CurrentCompany.SetCurrency("JPY");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			orderLine1.WE_ExtendedLinePrice = 100m;
			orderLine1.WE_RX_NKUnitPriceCurrency = "";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 30m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition: unit price currency is empty", "", orderLine1.WE_RX_NKUnitPriceCurrency);
			AssertEquals("Precondition: Extended line price", 100m, orderLine1.WE_ExtendedLinePrice);
			AssertEquals("Precondition: Sum of Units Met", 20m, orderLine1.SumOfUnitsMet);

			var releaseLine = orderLine1.ReleaseLines.Single();
			var unitPrice = ((IPackableItemParent)releaseLine).UnitPrice;
			AssertEquals("USD", unitPrice.Currency.Code);
			AssertEquals(5m, unitPrice.Amount);
		}

		#endregion

		#region IPartAttributes Members

		public void TestIPartAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var today = ZDate.Today;
			IPartAttributes releaseLine = orderLine.ReleaseLines.AddNew("1", "2", "3", "A", today, today.AddDays(1));
			AssertEquals(nameof(releaseLine.PartAttrib1), "1", releaseLine.PartAttrib1);
			AssertEquals(nameof(releaseLine.PartAttrib2), "2", releaseLine.PartAttrib2);
			AssertEquals(nameof(releaseLine.PartAttrib3), "3", releaseLine.PartAttrib3);
			AssertEquals(nameof(releaseLine.ExpiryDate), today, releaseLine.ExpiryDate);
			AssertEquals(nameof(releaseLine.PackingDate), today.AddDays(1), releaseLine.PackingDate);
		}

		#endregion

		#region IPartAttributeValidationConsumer Members

		#region TestIPartAttributeValidationConsumer_IsRegisteredForUniqueSerialNumberChecking

		public void TestIPartAttributeValidationConsumer_IsRegisteredForUniqueSerialNumberChecking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals(false, releaseLine.IsRegisteredForUniqueSerialNumberChecking);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(true, releaseLine.IsRegisteredForUniqueSerialNumberChecking);
		}

		#endregion

		#region TestIPartAttributeValidationConsumer_IsValidForUniqueSerialNumberChecking

		public void TestIPartAttributeValidationConsumer_IsValidForUniqueSerialNumberChecking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals(false, releaseLine.IsValidForUniqueSerialNumberChecking(""));
			AssertEquals(false, releaseLine.IsValidForUniqueSerialNumberChecking("SN1"));

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals(false, releaseLine.IsValidForUniqueSerialNumberChecking(""));
			AssertEquals(true, releaseLine.IsValidForUniqueSerialNumberChecking("SN1"));
		}

		#endregion

		#region TestIPartAttributeValidationConsumer_IsSerialNumberUsedOnThis

		public void TestIPartAttributeValidationConsumer_IsSerialNumberUsedOnThis()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals(false, releaseLine.IsSerialNumberUsedOnThis("Test"));

			releaseLine.SerialNumber = "Test";
			AssertEquals(true, releaseLine.IsSerialNumberUsedOnThis("Test"));
			AssertEquals(false, releaseLine.IsSerialNumberUsedOnThis("Random"));
		}

		#endregion

		#region TestIPartAttributeValidationConsumer_IsSerialNumberUsedOnSiblings

		public void TestIPartAttributeValidationConsumer_IsSerialNumberUsedOnSiblings()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine.ReleaseLines[0];
			IPartAttributeValidationConsumer iPartAttributeValidationConsumer = releaseLine1;
			AssertEquals(false, iPartAttributeValidationConsumer.IsSerialNumberUsedOnSiblings("Test"));

			releaseLine1.SerialNumber = "Test";
			AssertEquals(false, iPartAttributeValidationConsumer.IsSerialNumberUsedOnSiblings("Test"));

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.SerialNumber = "Test";
			AssertEquals("releaseLine2 is a duplicate so it doesn't get considered.", false, iPartAttributeValidationConsumer.IsSerialNumberUsedOnSiblings("Test"));
			AssertEquals("releaseLine1 is not a duplicate so it should get considered.", true, ((IPartAttributeValidationConsumer)releaseLine2).IsSerialNumberUsedOnSiblings("Test"));
			AssertEquals(false, iPartAttributeValidationConsumer.IsSerialNumberUsedOnSiblings("Random"));
		}

		#endregion

		#region TestIPartAttributeValidationConsumer_IsInventoryAdjustedOutOnSiblings

		public void TestIPartAttributeValidationConsumer_IsInventoryAdjustedOutOnSiblings()
		{
			var orderLine = Factory.New<WhsOrderLine>();
			IPartAttributeValidationConsumer releaseLine = new WhsReleaseLine(orderLine);
			AssertEquals(false, releaseLine.IsInventoryAdjustedOutOnSiblings(null));
		}

		#endregion

		#endregion

		#region ISerialSplittableLine Members

		#region TestISerialSplittableLine_IsFinalised

		public void TestISerialSplittableLine_IsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			ISerialSplittableLine releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("While Pick is not finalised, release line is not finalised.", false, releaseLine.IsFinalised);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals("When Pick is finalised, release line is finalised.", true, releaseLine.IsFinalised);
		}

		#endregion

		#region TestISerialSplittableLine_Units

		public void TestISerialSplittableLine_Units()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			using (orderLine.ReleaseLines.SuspendSettingDefaults())
			{
				var releaseLine = orderLine.ReleaseLines[0];
				releaseLine.Quantity = 15.1m;
				ISerialSplittableLine iSerialSplittableLine = releaseLine;
				AssertEquals(15.1m, iSerialSplittableLine.Units);
			}
		}

		#endregion

		#region TestISerialSplittableLine_SuspendValidation

		public void TestISerialSplittableLine_SuspendValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 10m;

			bool validationWasCalled = false;
			releaseLine.QuantityInfo.AdditionalValidation += () =>
			{
				validationWasCalled = true;
			};

			order.WD_UnitsSentInfo.AdditionalValidation += () =>
			{
				validationWasCalled = true;
			};

			ISerialSplittableLine iSerialSplittableLine = releaseLine;

			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(false, validationWasCalled);
		}

		#endregion

		#region TestISerialSplittableLine_DoesntSplitNonReleaseCapturedAttributes

		public void TestISerialSplittableLine_DoesntSplitNonReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 1m;

			ISerialSplittableLine iSerialSplittableLine = releaseLine;
			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(1, orderLine.ReleaseLines.Count);
			AssertEquals(1m, releaseLine.Quantity);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(1, orderLine.ReleaseLines.Count);
			AssertEquals(1m, releaseLine.Quantity);
			AssertEquals("Should not be splittable as the serial number is not an RCA.", false, iSerialSplittableLine.IsSplittableProduct);

			releaseLine.Quantity = 5m;
			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals("Should not have split as the serial number is not an RCA.", 1, orderLine.ReleaseLines.Count);
			AssertEquals("Should not have split as the serial number is not an RCA.", 5m, releaseLine.Quantity);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			AssertEquals("Should be splittable as the serial number is an RCA.", true, iSerialSplittableLine.IsSplittableProduct);

			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(5, orderLine.ReleaseLines.Count);
			AssertEquals(5, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(o => o.Quantity == 1m));
		}

		#endregion

		#region TestISerialSplittableLine_SplitWhenSerialNumberExists_WithPackedReleaseLines

		public void TestISerialSplittableLine_SplitWhenSerialNumberExists_WithPackedReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.SerialNumber = "SN001";
			releaseLine.Quantity = 5m;

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 1m);

			ISerialSplittableLine iSerialSplittableLine = releaseLine;
			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals("Should not Split Packed Release Lines.", 1, orderLine.ReleaseLines.Count);
		}

		#endregion

		#region TestISerialSplittableLine_SplitWhenSerialNumberExists_WithReleaseCapturedAttribs

		public void TestISerialSplittableLine_SplitWhenSerialNumberExists_WithReleaseCapturedAttribs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 1m;

			ISerialSplittableLine iSerialSplittableLine = releaseLine;
			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(1, orderLine.ReleaseLines.Count);
			AssertEquals(1m, releaseLine.Quantity);

			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(1, orderLine.ReleaseLines.Count);
			AssertEquals(1m, releaseLine.Quantity);

			releaseLine.SerialNumber = "SN001";
			releaseLine.Quantity = 5m;
			AssertEquals("Precondition: Stock is Release Captured.", 0m, orderLine.PickLines.Sum(l => l.UnreleaseCapturedQty));

			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(5, orderLine.ReleaseLines.Count);
			AssertEquals(5, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(o => o.Quantity == 1m));
			AssertEquals("Release Captured stock should be reduced.", 4m, orderLine.PickLines.Sum(l => l.UnreleaseCapturedQty));
		}

		#endregion

		#region TestISerialSplittableLine_SplitWhenSerialNumberExists_WithNonReleaseCapturedAttribs

		[TestDate(2019, 1, 1)]
		public void TestISerialSplittableLine_SplitWhenSerialNumberExists()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var year = ZDate.Today.Year;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(year, 1, 2), new ZDate(year, 1, 3), "ABC", "RED", "MEDIUM", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.SerialNumber = "SN001";
			releaseLine.Quantity = 5m;

			ISerialSplittableLine iSerialSplittableLine = releaseLine;
			iSerialSplittableLine.SplitWhenSerialNumberExists();
			AssertEquals(5, orderLine.ReleaseLines.Count);
			AssertEquals(1, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(o => o.SerialNumber == "SN001"));
			AssertEquals(5, orderLine.ReleaseLines.Cast<WhsReleaseLine>().Count(o =>
					o.Quantity == 1m
					&& o.UnreleasedQty == 0m
					&& o.PartAttribute1 == "ABC"
					&& o.PartAttribute2 == "RED"
					&& o.PartAttribute3 == "MEDIUM"
					&& o.ExpiryDate == new ZDateTime(year, 1, 2)
					&& o.PackingDate == new ZDateTime(year, 1, 3)));
		}

		#endregion

		#region TestISerialSplittableLine_NonCommittedElement

		public void TestISerialSplittableLine_NonCommittedElement()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 1m;

			var nonCommittedReleaseLine = (WhsReleaseLine)((IBindingList)orderLine.ReleaseLines).AddNew();
			AssertEquals("Precondition.", 2, orderLine.ReleaseLines.Count);

			ISerialSplittableLine iSerialSplittableLine = nonCommittedReleaseLine;
			AssertNoExceptionThrown(iSerialSplittableLine.SplitWhenSerialNumberExists);
			AssertEquals("Should not have added a new line.", 2, orderLine.ReleaseLines.Count);
		}

		#endregion

		#endregion

		//

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			return order.Lines[0].ReleaseLines[0];
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}

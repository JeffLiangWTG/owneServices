using System;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class AttributePartsTest : WhsTestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var today = ZDate.Today;
			var orderLine = GetPickedOrderLine();
			var releaseLine = orderLine.ReleaseLines.AddNew("PA1", "PA2", "PA3", "SER", today.AddDays(-1), today.AddDays(1));

			orderLine.WE_PartAttrib1 = "1";
			orderLine.WE_PartAttrib2 = "2";
			orderLine.WE_PartAttrib3 = "3";
			orderLine.WE_SerialNumber = "SN";
			orderLine.WE_ExpiryDate = today.AddDays(-2);
			orderLine.WE_PackingDate = today.AddDays(2);

			var attributeParts1 = AttributeParts.New(releaseLine);
			AssertEquals("attributeParts1.PartAttribute1", "PA1", attributeParts1.PartAttribute1);
			AssertEquals("attributeParts1.PartAttribute2", "PA2", attributeParts1.PartAttribute2);
			AssertEquals("attributeParts1.PartAttribute3", "PA3", attributeParts1.PartAttribute3);
			AssertEquals("attributeParts1.SerialNumber", "SER", attributeParts1.SerialNumber);
			AssertEquals("attributeParts1.ExpiryDate", today.AddDays(-1), attributeParts1.ExpiryDate);
			AssertEquals("attributeParts1.PackingDate", today.AddDays(1), attributeParts1.PackingDate);

			var attributeParts2 = AttributeParts.New(orderLine);
			AssertEquals("attributeParts2.PartAttribute1", "1", attributeParts2.PartAttribute1);
			AssertEquals("attributeParts2.PartAttribute2", "2", attributeParts2.PartAttribute2);
			AssertEquals("attributeParts2.PartAttribute3", "3", attributeParts2.PartAttribute3);
			AssertEquals("attributeParts2.SerialNumber", "SN", attributeParts2.SerialNumber);
			AssertEquals("attributeParts2.ExpiryDate", today.AddDays(-2), attributeParts2.ExpiryDate);
			AssertEquals("attributeParts2.PackingDate", today.AddDays(2), attributeParts2.PackingDate);

			AssertExceptionThrown<ArgumentNullException>(() => AttributeParts.New((WhsReleaseLine)null));
			AssertExceptionThrown<ArgumentNullException>(() => AttributeParts.New((WhsDocketLine)null));
		}

		#endregion

		#region TestEmpty

		public void TestEmpty()
		{
			var attributeParts = AttributeParts.Empty;
			AssertEquals("attributeParts.PartAttribute1", "", attributeParts.PartAttribute1);
			AssertEquals("attributeParts.PartAttribute2", "", attributeParts.PartAttribute2);
			AssertEquals("attributeParts.PartAttribute3", "", attributeParts.PartAttribute3);
			AssertEquals("attributeParts.SerialNumber", "", attributeParts.SerialNumber);
			AssertEquals("attributeParts.ExpiryDate", ZDate.Empty, attributeParts.ExpiryDate);
			AssertEquals("attributeParts.PackingDate", ZDate.Empty, attributeParts.PackingDate);
		}

		#endregion

		#region TestGetPartAttribute

		public void TestGetPartAttribute()
		{
			var today = ZDate.Today;
			var orderLine = GetPickedOrderLine();
			var releaseLine = orderLine.ReleaseLines.AddNew("PA1", "PA2", "PA3", "SER", today.AddDays(-1), today.AddDays(1));

			var attributeParts = AttributeParts.New(releaseLine);
			AssertEquals("PartAttrib.None should return empty string.", "", attributeParts.GetPartAttribute(PartAttributeNumber.None));
			AssertEquals("PartAttrib.One should return value of Part Attribute 1.", "PA1", attributeParts.GetPartAttribute(PartAttributeNumber.One));
			AssertEquals("PartAttrib.Two should return value of Part Attribute 2.", "PA2", attributeParts.GetPartAttribute(PartAttributeNumber.Two));
			AssertEquals("PartAttrib.Three should return value of Part Attribute 3.", "PA3", attributeParts.GetPartAttribute(PartAttributeNumber.Three));
			AssertEquals("PartAttrib.Three should return value of Serial Number.", "SER", attributeParts.GetPartAttribute(PartAttributeNumber.SerialNumber));
		}

		#endregion

		#region TestKey

		public void TestKey()
		{
			var today = ZDate.Today;
			var orderLine = GetPickedOrderLine();
			var releaseLine = orderLine.ReleaseLines.AddNew("PA1", "PA2", "PA3", "SER", today.AddDays(-1), today.AddDays(1));

			var attributeParts = AttributeParts.New(releaseLine);
			AssertEquals("Key should be made up of all attribs.",
				"PA1|PA2|PA3|SER|" + today.AddDays(-1).ToShortDateString() + "|" + today.AddDays(1).ToShortDateString(), attributeParts.Key);
		}

		#endregion

		// interfaces

		#region IPartAttributes Members

		public void TestIPartAttributes()
		{
			var today = ZDate.Today;
			var orderLine = GetPickedOrderLine();
			orderLine.WE_PartAttrib1 = "1";
			orderLine.WE_PartAttrib2 = "2";
			orderLine.WE_PartAttrib3 = "3";
			orderLine.WE_SerialNumber = "DN";
			orderLine.WE_ExpiryDate = today.AddDays(-2);
			orderLine.WE_PackingDate = today.AddDays(2);

			IPartAttributes parts = AttributeParts.New(orderLine);
			AssertEquals(nameof(parts.PartAttrib1), "1", parts.PartAttrib1);
			AssertEquals(nameof(parts.PartAttrib2), "2", parts.PartAttrib2);
			AssertEquals(nameof(parts.PartAttrib3), "3", parts.PartAttrib3);
			AssertEquals(nameof(parts.PartAttrib3), "DN", parts.SerialNumber);
			AssertEquals(nameof(parts.ExpiryDate), today.AddDays(-2), parts.ExpiryDate);
			AssertEquals(nameof(parts.PackingDate), today.AddDays(2), parts.PackingDate);
		}

		#endregion

		//

		#region Implementation

		WhsOrderLine GetPickedOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			return order.Lines[0];
		}

		#endregion
	}
}

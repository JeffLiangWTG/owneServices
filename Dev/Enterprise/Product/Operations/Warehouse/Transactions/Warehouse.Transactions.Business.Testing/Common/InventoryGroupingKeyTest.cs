using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class InventoryGroupingKeyTest : WhsTestCaseWithFactory
	{
		#region TestEmpty

		public void TestEmpty()
		{
			AssertEquals("Empty should be equal to Empty.", InventoryGroupingKey.Empty, InventoryGroupingKey.Empty);
			AssertEquals("Empty should be equal to Empty.", true, InventoryGroupingKey.Empty == InventoryGroupingKey.Empty);
			AssertEquals("Empty should be equal to Empty.", true, InventoryGroupingKey.Empty.Equals(InventoryGroupingKey.Empty));
		}

		#endregion

		#region TestNew

		[TestDate(2019, 8, 15)]
		public void TestNew()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, setReleaseCaptured: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, setReleaseCaptured: true);

			var dateOne = ZDate.Today.AddDays(1);
			var dateTwo = ZDate.Today.AddDays(2);
			var dateThree = ZDate.Today.AddDays(3);
			var dateFour = ZDate.Today.AddDays(4);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, dateOne, dateTwo, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, dateOne, dateTwo, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, dateThree, dateFour, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, dateOne, dateTwo, "", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();
			AssertEquals("PickLine without Inventory should return Empty Key.", InventoryGroupingKey.Empty, InventoryGroupingKey.New(Factory.New<WhsPickLine>()));

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderLine1PickLine1 = orderLine1.PickLines.First(p => p.InventoryLine.WE_ExpiryDate == dateOne);
			var orderLine1PickLine2 = orderLine1.PickLines.Last(p => p.InventoryLine.WE_ExpiryDate == dateOne);
			var orderLine1PickLine3 = orderLine1.PickLines.Single(p => p.InventoryLine.WE_ExpiryDate == dateThree);
			var orderLine2PickLine = orderLine2.PickLines.Single();

			var key1 = InventoryGroupingKey.New(orderLine1PickLine1);
			var key2 = InventoryGroupingKey.New(orderLine1PickLine2);
			var key3 = InventoryGroupingKey.New(orderLine1PickLine3);
			var key4 = InventoryGroupingKey.New(orderLine2PickLine);

			// key 1 and 2 are equal
			AssertEquals("Key 1 and 2 have the same Inventory, should be equal.", key2, key1);
			AssertEquals("Key 1 and 2 have the same Inventory, should be equal.", true, key1.Equals(key2));
			AssertEquals("Key 1 and 2 have the same Inventory, should be equal.", true, key1 == key2);
			AssertEquals("Key 1 and 2 have the same Inventory, should be equal.", key2.GetHashCode(), key1.GetHashCode());
			AssertEquals("CompareItemKey 1 and 2 have the same.", true, key1.IsSimilarItem_DoNotUse(key2));

			// key 3 and key 1 are not equal
			AssertNotEquals("Key 1 and 3 have different Expiry and Packing Dates, should not be equal.", key3, key1);
			AssertEquals("Key 1 and 3 have different Expiry and Packing Dates, should not be equal.", false, key1.Equals(key3));
			AssertEquals("Key 1 and 3 have different Expiry and Packing Dates, should not be equal.", false, key1 == key3);
			AssertEquals("CompareItemKey 1 and 3 have different Expiry and Packing Dates, should not be equal.", false, key3.IsSimilarItem_DoNotUse(key1));

			// key 4 and key 1 are not equal
			AssertNotEquals("Key 1 and 4 have different Products, should not be equal.", key4, key1);
			AssertEquals("Key 1 and 4 have different Products, should not be equal.", false, key1.Equals(key4));
			AssertEquals("Key 1 and 4 have different Products, should not be equal.", false, key1 == key4);
			AssertEquals("CompareItemKey 1 and 4 have different Products, should not be equal.", false, key4.IsSimilarItem_DoNotUse(key1));

			var releaseLine1 = order.Lines[0].ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.ExpiryDate == dateOne);
			AssertEquals("Precondition: Released qty is correct.", 10m, releaseLine1.Quantity);

			releaseLine1.PartAttribute1 = "AAA";
			releaseLine1.PartAttribute2 = "BBB";
			releaseLine1.PartAttribute3 = "CCC";

			var newKey1 = InventoryGroupingKey.New(orderLine1PickLine1);
			AssertContains("New key should contain Release Captured Attribs values.", "AAA|BBB|CCC", newKey1.ToString());

			var newKey2 = InventoryGroupingKey.New(orderLine1PickLine2);

			// pickLine keys are equal
			AssertEquals("Both RCAs have the same Release Captured Attribs for the same Inventory, should be equal.", newKey2, newKey1);
			AssertEquals("Both RCAs have the same Release Captured Attribs for the same Inventory.", true, newKey1.Equals(newKey2));
			AssertEquals("Both RCAs have the same Release Captured Attribs for the same Inventory.", true, newKey1 == newKey2);
			AssertEquals("Both RCAs have the same Release Captured Attribs for the same Inventory.", newKey2.GetHashCode(), newKey1.GetHashCode());
		}

		public void TestNew_WithSerialNumber_ReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine1.WE_SerialNumber = "SER1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine2.WE_SerialNumber = "SER2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines[1];

			var key1 = InventoryGroupingKey.New(releaseLine1, orderLine);
			var key2 = InventoryGroupingKey.New(releaseLine2, orderLine);

			AssertNotEquals(key1, key2);
		}

		public void TestNew_WithSerialNumber_PickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine1.WE_SerialNumber = "SER1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine2.WE_SerialNumber = "SER2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Helper.CreatePickNew(order);
			var pickline1 = orderLine.PickLines[0];
			var pickline2 = orderLine.PickLines[1];

			var key1 = InventoryGroupingKey.New(pickline1);
			var key2 = InventoryGroupingKey.New(pickline2);

			AssertNotEquals(key1, key2);
		}

		public void TestNew_WithSerialNumber_RCA()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Helper.CreatePickNew(order);
			var pickline1 = orderLine.PickLines[0];
			pickline1.WZ_ReleaseCapturedSerialNumber = "SER1";

			var pickline2 = orderLine.PickLines[1];
			pickline2.WZ_ReleaseCapturedSerialNumber = "SER2";

			var key1 = InventoryGroupingKey.New(pickline1);
			var key2 = InventoryGroupingKey.New(pickline2);

			AssertNotEquals(key1, key2);
		}

		#endregion

		#region TestTemporaryProperGroupingKeyForWhs_DoNotUse

		public void TestTemporaryProperGroupingKeyForWhs_DoNotUse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 1m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			Helper.CreatePickNew(order);

			var pickline1 = orderLine1.PickLines[0];
			var pickline2 = orderLine2.PickLines[0];
			var pickline3 = orderLine3.PickLines[0];

			var key1 = InventoryGroupingKey.New(pickline1);
			var key2 = InventoryGroupingKey.New(pickline2);
			var key3 = InventoryGroupingKey.New(pickline3);

			AssertEquals("Same product with same attr, should be equal.", true, key1.IsSimilarItem_DoNotUse(key2));
			AssertNotEquals("Same product with same attr but diffrent order lines should have different hash code.", key1.GetHashCode(), key2.GetHashCode());
			AssertEquals("Product are diffrent , should not be equal.", false, key1.IsSimilarItem_DoNotUse(key3));
			AssertNotEquals("Product are diffrent and diffrent order lines should have different hash code.", key1.GetHashCode(), key3.GetHashCode());
		}

		#endregion

		#region TestTemporaryProperGroupingKeyForWhs_DoNotUse_FixHashCodeCollision 

		public void TestTemporaryProperGroupingKeyForWhs_DoNotUse_FixHashCodeCollision()
		{
			// with old implimentation return same code
			//1945988324--> fda3ee61-3ec9-474d-88d8-5f63449d1429|UMM|JVY|LXD|SER1|13-May-17|22-Oct-16
			//1945988324--> b4426ca7-c7b9-428a-8228-04769ca5f9be|MH5|CB8|AEI|SER2|29-Sep-17|20-Oct-15
			var dummy1 = InventoryGroupingKey.NewForTesting(new ZGuid("fda3ee61-3ec9-474d-88d8-5f63449d1429"), new ZGuid("fda3ee61-3ec9-474d-88d8-5f63449d1429"), "UMM", "JVY", "LXD", "SER1", new ZDate("13-May-17"), new ZDate("22-Oct-16"));
			var dummy2 = InventoryGroupingKey.NewForTesting(new ZGuid("b4426ca7-c7b9-428a-8228-04769ca5f9be"), new ZGuid("b4426ca7-c7b9-428a-8228-04769ca5f9be"), "MH5", "CB8", "AEI", "SER2", new ZDate("29-Sep-17"), new ZDate("20-Oct-15"));
			AssertEquals(false, dummy1.IsSimilarItem_DoNotUse(dummy2));

			//1024411306--> 4e360da9-30e1-4c1f-bc85-a6b21ca21989|SM9|WOB|ENQ|SER3|12-Nov-15|08-Dec-15
			//1024411306--> c6f984c1-8466-4f72-9db0-7c6ba48a8e04|GIL|ERT|22F|SER4|17-Jan-16|16-Nov-17
			var dummy3 = InventoryGroupingKey.NewForTesting(new ZGuid("4e360da9-30e1-4c1f-bc85-a6b21ca21989"), new ZGuid("4e360da9-30e1-4c1f-bc85-a6b21ca21989"), "SM9", "WOB", "ENQ", "SER3", new ZDate("12-Nov-15"), new ZDate("08-Dec-15"));
			var dummy4 = InventoryGroupingKey.NewForTesting(new ZGuid("c6f984c1-8466-4f72-9db0-7c6ba48a8e04"), new ZGuid("c6f984c1-8466-4f72-9db0-7c6ba48a8e04"), "GIL", "ERT", "22F", "SER4", new ZDate("17-Jan-16"), new ZDate("16-Nov-17"));
			AssertEquals(false, dummy3.IsSimilarItem_DoNotUse(dummy4));
		}

		#endregion

		#region TestKeyForAutoPacking

		public void TestKeyForAutoPacking_PickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine1.WE_SerialNumber = "SER1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine2.WE_SerialNumber = "SER2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Helper.CreatePickNew(order);
			var pickline1 = orderLine.PickLines[0];
			var pickline2 = orderLine.PickLines[1];

			var key1 = InventoryGroupingKey.New(pickline1);
			var key2 = InventoryGroupingKey.New(pickline2);

			AssertNotEquals(key1, key1.KeyForAutoPack);
			AssertNotEquals(key1, key2);
			AssertEquals(key1.KeyForAutoPack, key2.KeyForAutoPack);
		}

		public void TestKeyForAutoPacking_RCA_SerialNumberEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Helper.CreatePickNew(order);
			var pickline1 = orderLine.PickLines[0];
			pickline1.WZ_ReleaseCapturedSerialNumber = "SER1";

			var pickline2 = orderLine.PickLines[1];
			pickline2.WZ_ReleaseCapturedSerialNumber = "SER2";

			var key1 = InventoryGroupingKey.New(pickline1);
			var key2 = InventoryGroupingKey.New(pickline2);

			AssertNotEquals(key1, key1.KeyForAutoPack);
			AssertNotEquals(key1, key2);
			AssertEquals(key1.KeyForAutoPack, key2.KeyForAutoPack);
		}

		public void TestKeyForAutoPacking_ReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine1.WE_SerialNumber = "SER1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine2.WE_SerialNumber = "SER2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Helper.CreatePickNew(order);
			var releaseLine1 = orderLine.ReleaseLines[0];
			var releaseLine2 = orderLine.ReleaseLines[1];

			var key1 = InventoryGroupingKey.New(releaseLine1, orderLine);
			var key2 = InventoryGroupingKey.New(releaseLine2, orderLine);

			AssertNotEquals(key1, key1.KeyForAutoPack);
			AssertNotEquals(key1, key2);
			AssertEquals(key1.KeyForAutoPack, key2.KeyForAutoPack);
		}

		public void TestKeyForAutoPacking_NoSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "AT1", "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "", ZDate.Empty, ZDate.Empty, "AT2", "", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Helper.CreatePickNew(order);
			var pickline1 = orderLine.PickLines[0];
			var pickline2 = orderLine.PickLines[1];

			var key1 = InventoryGroupingKey.New(pickline1);
			var key2 = InventoryGroupingKey.New(pickline2);

			AssertEquals(key1, key1.KeyForAutoPack);
			AssertNotEquals(key1, key2);
			AssertNotEquals(key1.KeyForAutoPack, key2.KeyForAutoPack);
		}

		#endregion

		#region TestToString

		[TestDate(2021, 11, 1)]
		public void TestToString()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var dateOne = ZDate.Today.AddDays(1);
			var dateTwo = ZDate.Today.AddDays(2);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", dateOne, dateTwo, "ABC", "DEF", "GHI", "SN1", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();

			var key = InventoryGroupingKey.New(pickLine);
			var expectedKeyString = orderLine.PK.ToString() + "|ABC|DEF|GHI|SN1|02-Nov-21|03-Nov-21";
			AssertEquals(expectedKeyString, key.ToString());
		}

		#endregion
	}
}

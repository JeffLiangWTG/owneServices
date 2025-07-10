using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetAvailableInventoryAttributesTest : WhsSecureServiceTestCase
	{
		#region TestGetAvailableInventoryAttributes_WithSerialNumberAttribute

		public void TestGetAvailableInventoryAttributes_WithSerialNumberAttribute()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var client2 = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var dateTime1 = ZDate.Today.AddDays(1);
			var dateTime5 = ZDate.Today.AddDays(5);

			var receive1 = Helper.CreateWhsReceive(client2, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", ""); // different client - no attributes on it.
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[0], "PLT-2", dateTime1, ZDate.Empty, "", "", "", "");
			line1.WI_SerialNumber = "SN:0002";
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[0], "PLT-2", dateTime1, ZDate.Empty, "", "", "", "");
			line2.WI_SerialNumber = "SN:0003";
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[0], "PLT-2", dateTime5, ZDate.Empty, "", "", "", ""); // different expiry date
			line3.WI_SerialNumber = "SN:0004";
			var line4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[0], "PLT-2", dateTime1, ZDate.Empty, "", "", "PA3", ""); // different PartAttrib3
			line4.WI_SerialNumber = "SN:0005";
			var line5 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[0], "PLT-2", dateTime1, ZDate.Empty, "", "", "", ""); // committed
			line5.WI_SerialNumber = "SN:0006";
			var inventoryReserved = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[0], "PLT-2", dateTime1, ZDate.Empty, "", "", "", ""); // reserved
			inventoryReserved.WI_SerialNumber = "SN:0007";
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2.PK, 1m, locations[0].PK, "PLT-2", ""); // different product
			var line8 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[0], "PLT-3", dateTime1, ZDate.Empty, "", "", "", ""); // different pallet
			line8.WI_SerialNumber = "SN:0008";

			// check that it's work if there is no Pallet ID
			var line9 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[1], "", dateTime1, ZDate.Empty, "", "", "", "");
			line9.WI_SerialNumber = "SN:0009";
			var line10 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[1], "", dateTime1, ZDate.Empty, "", "", "", "");
			line10.WI_SerialNumber = "SN:0010";
			var line11 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, locations[2], "", dateTime1, ZDate.Empty, "", "", "", ""); // different location
			line11.WI_SerialNumber = "SN:0011";
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN:0006"; // to make sure that correct inventory get picked.

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR2", Notify);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			Helper.CreateReservePickLine(orderLine2, inventoryReserved, 1m); // to reserve inventory.

			Helper.CreatePickNew(order1);
			Helper.Factory.Save();

			var pickLineInfo = new WhsPickLineInfo(orderLine1.PickLines[0], new WhsPickInfo()) { OrderedSerialNumber = "" };
			// clear ordered attributes

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetAvailableInventoryAttributes(pickLineInfo, true);
			var availableInventoryAttributes1 = response1.AvailableInventorySerialNumbers;
			AssertEquals("Only 4 inventories should be loaded.", 4, availableInventoryAttributes1.Count);
			AssertEquals("Inventory with Serial Number 'SN:0002' should be available for picking", true, availableInventoryAttributes1.Contains("SN:0002"));
			AssertEquals("Inventory with Serial Number 'SN:0003' should be available for picking", true, availableInventoryAttributes1.Contains("SN:0003"));
			AssertEquals("Inventory with Serial Number 'SN:0004' should be available for picking", true, availableInventoryAttributes1.Contains("SN:0004"));
			AssertEquals("Inventory with Serial Number 'SN:0005' should be available for picking", true, availableInventoryAttributes1.Contains("SN:0005"));

			// test load only Serial Numbers for Ordered Part Attribute.
			var webService2 = GetNewWebService(data.Whs1);
			pickLineInfo.OrderedPartAttribute3 = "PA3";
			var response2 = webService2.GetAvailableInventoryAttributes(pickLineInfo, true);
			var availableInventoryAttributes3 = response2.AvailableInventorySerialNumbers;
			AssertEquals("Only 1 inventory should be loaded.", 1, availableInventoryAttributes3.Count);
			AssertEquals("Inventory with Serial Number 'SN:0005' should be available for picking", true, availableInventoryAttributes3.Contains("SN:0005"));
			pickLineInfo.OrderedPartAttribute3 = ""; // clean up

			// test load works ever if no Pallet ID is specified.
			var webService3 = GetNewWebService(data.Whs1);
			pickLineInfo.PalletID = "";
			pickLineInfo.Location = locations[1].ToLocationString();
			var response3 = webService3.GetAvailableInventoryAttributes(pickLineInfo, true);
			var availableInventoryAttributes2 = response3.AvailableInventorySerialNumbers;
			AssertEquals("Only 2 inventories should be loaded.", 2, availableInventoryAttributes2.Count);
			AssertEquals("Inventory with Serial Number 'SN:0009' should be available for picking", true, availableInventoryAttributes2.Contains("SN:0009"));
			AssertEquals("Inventory with Serial Number 'SN:0010' should be available for picking", true, availableInventoryAttributes2.Contains("SN:0010"));

			// test load of specified Serial Number only.
			var webService4 = GetNewWebService(data.Whs1);
			pickLineInfo.OrderedSerialNumber = "SN:0010";
			var response4 = webService4.GetAvailableInventoryAttributes(pickLineInfo, true);
			var availableInventoryAttributes4 = response4.AvailableInventorySerialNumbers;
			AssertEquals("Only 1 inventory should be loaded.", 1, availableInventoryAttributes4.Count);
			AssertEquals("Inventory with Serial Number 'SN:0010' should be available for picking", true, availableInventoryAttributes4.Contains("SN:0010"));
		}

		public void TestGetAvailableInventoryAttributes_WithSerialNumberAttribute_IncludeSwappableSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory2.WI_SerialNumber = "SN:002";
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory3.WI_SerialNumber = "SN:003";

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: Available Qty of inventory1.", 1m, inventory1.WI_AvailableToTransferQuantity);
			AssertEquals("Pre-condition: Available Qty of inventory2.", 1m, inventory2.WI_AvailableToTransferQuantity);
			AssertEquals("Pre-condition: Available Qty of inventory3.", 1m, inventory3.WI_AvailableToTransferQuantity);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			Helper.Factory.Save();

			var orderLine1 = order1.Lines[0];
			var pickLine1 = pickForOrder1.GetAllPickLines().Single();
			var pickLine2 = pickForOrder2.GetAllPickLines().Single();

			AssertEquals("Precondition", "SN:001", pickLine1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLine2.Inventory.WI_SerialNumber);

			AssertEquals("Available Qty of inventory1.", 0m, inventory1.WI_AvailableToTransferQuantity);
			AssertEquals("Available Qty of inventory2.", 0m, inventory2.WI_AvailableToTransferQuantity);
			AssertEquals("Available Qty of inventory3.", 1m, inventory3.WI_AvailableToTransferQuantity);

			var pickLineInfo = new WhsPickLineInfo(orderLine1.PickLines[0], new WhsPickInfo());
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetAvailableInventoryAttributes(pickLineInfo, true);
			var availableInventorySerialNumbers = response.AvailableInventorySerialNumbers;
			AssertContainsExactElementsInAnyOrder(new[] { "SN:002", "SN:003" }, availableInventorySerialNumbers);
		}

		public void TestGetAvailableInventoryAttributes_WithSerialNumberAttribute_ButSetIsAttributeNeutralAsFalse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			data.Part1.RelatedOrganisations[0].OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var date = ZDate.Today;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-2", date, ZDate.Empty, "", "", "PA3", "");
			inventory1.WI_SerialNumber = "SN:0002";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-2", date, ZDate.Empty, "", "", "", "");
			inventory2.WI_SerialNumber = "SN:0003";

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var pickLineInfo = new WhsPickLineInfo { OrderedPartAttribute3 = "PA3" };

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetAvailableInventoryAttributes(pickLineInfo, false);
			var availableInventory1 = response1.AvailableInventorySerialNumbers;
			AssertEquals("It should load 2 inventories.", 2, availableInventory1.Count);
			AssertEquals("Inventory with Serial Number 'SN:0002' should be available for picking", true, availableInventory1.Contains("SN:0002"));
			AssertEquals("Inventory with Serial Number 'SN:0003' should be available for picking", true, availableInventory1.Contains("SN:0003"));

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetAvailableInventoryAttributes(pickLineInfo, true);
			var availableInventory2 = response2.AvailableInventorySerialNumbers;
			AssertEquals("It should load 1 inventories if IsAttributeNeutral is set as true.", 1, availableInventory2.Count);
			AssertEquals("Inventory with Serial Number 'SN:0002' should be available for picking", true, availableInventory2.Contains("SN:0002"));
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ReattachAndConfirmPickLinesTest : WhsSecureServiceTestCase
	{
		#region TestReattachAndConfirmPickLines

		#region TestReattachAndConfirmPickLines

		public void TestReattachAndConfirmPickLines()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-2", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN:003";
			receive.FinaliseDocket();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - ensure that PickLine has Serial Number SN:001.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
			AssertEquals("Precondition - ensure that PickLine was not picked yet.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);

			var pickInfo = new WhsPickInfo(pick, line1.PickLines, new List<string>());
			var pickLineInfo = new WhsPickLineInfo(line1.PickLines[0], pickInfo);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("PickLine still should have the correct Serial Number.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("PickLine should be picked now.", true, line1.PickLines[0].IsPickedFromPutawayLocation);
				AssertEquals("All Pick Lines should be sucessfuly picked", false, response.Pick.Lines.Any());
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_UsesWarehouseCountryFormatString

		public void TestReattachAndConfirmPickLines_UsesWarehouseCountryFormatString()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var now = ZDate.Today;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.PackingDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", now.AddDays(7), now, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, locations[0], "PLT-1", now.AddDays(7), now, "", "", "", "");
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocket();
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure Receive is finalised.", true, receive.IsFinalised);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var pick1 = Helper.CreatePickNew(order1);
			Helper.Factory.Save();

			var pickInfo1 = new WhsPickInfo(pick1, line1.PickLines, new List<string>());
			var pickLineInfo1 = new WhsPickLineInfo(line1.PickLines[0], pickInfo1);

			var webService = GetNewWebService(data.Whs1, staff);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo1 }, null);

			AssertEquals("ddMMyy", pickInfo1.ProductPartAttributesInfos[0].ExpiryDateFormatString);
			AssertEquals("ddMMyy", pickInfo1.ProductPartAttributesInfos[0].PackingDateFormatString);

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR2", Notify);
			var line2 = Helper.CreateWhsOrderLine(order2, data.Part2, 1m);
			var pick2 = Helper.CreatePickNew(order2);
			Helper.Factory.Save();

			var pickInfo2 = new WhsPickInfo(pick2, line2.PickLines, new List<string>());
			var pickLineInfo2 = new WhsPickLineInfo(line2.PickLines[0], pickInfo2);

			response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo2 }, null);

			AssertEquals("yyMMdd", pickInfo2.ProductPartAttributesInfos[0].ExpiryDateFormatString);
			AssertEquals("yyMMdd", pickInfo2.ProductPartAttributesInfos[0].PackingDateFormatString);
		}

		#endregion

		#region TestReattachAndConfirmPickLines_WithDifferentSerialThanAllocated

		public void TestReattachAndConfirmPickLines_WithDifferentSerialThanAllocated()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-2", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN:003";
			receive.FinaliseDocket();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - ensure that PickLine has Serial Number SN:001.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
			AssertEquals("Precondition - ensure that PickLine was not picked yet.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);

			var pickInfo = new WhsPickInfo(pick, line1.PickLines, new List<string>());

			// Try to Pick a Inventory with different Serial Number and case are insensitive.
			var pickLineInfo = new WhsPickLineInfo(line1.PickLines[0], pickInfo) { SerialNumber = "sN:002" };

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("PickLine should have new Serial Number.", "SN:002", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("PickLine should be picked now.", true, line1.PickLines[0].IsPickedFromPutawayLocation);
				AssertEquals("All Pick Lines should be sucessfuly picked", false, response.Pick.Lines.Any());
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_Errors

		#region TestReattachAndConfirmPickLines_DifferentPalletID

		public void TestReattachAndConfirmPickLines_DifferentPalletID()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-2", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN:003";
			receive.FinaliseDocket();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - ensure that PickLine has Serial Number SN:001.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
			AssertEquals("Precondition - ensure that PickLine was not picked yet.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);

			var pickInfo = new WhsPickInfo(pick, line1.PickLines, new List<string>());

			var pickLineInfo1 = new WhsPickLineInfo(line1.PickLines[0], pickInfo) { SerialNumber = "Sn:003" };

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo1 }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("PickLine Serial Number should not have changed.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("PickLine should not be picked.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);
				AssertEquals("The specified Serial Number is not available, the pick line should not be picked.", 1, response.Pick.Lines.Count);
				AssertEquals("The specified Serial Number is not available, the pick line should not be picked.", "Sn:003", response.Pick.Lines[0].SerialNumber);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_NonexistentSerialNumber

		public void TestReattachAndConfirmPickLines_NonexistentSerialNumber()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-2", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN:003";
			receive.FinaliseDocket();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - ensure that PickLine has Serial Number SN:001.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
			AssertEquals("Precondition - ensure that PickLine was not picked yet.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);

			var pickInfo = new WhsPickInfo(pick, line1.PickLines, new List<string>());

			var pickLineInfo = new WhsPickLineInfo(line1.PickLines[0], pickInfo) { SerialNumber = "sn:004" };

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("PickLine Serial Number should not have changed.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("PickLine should not be picked.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);
				AssertEquals("The specified Serial Number is not available, the pick line should not be picked.", 1, response.Pick.Lines.Count);
				AssertEquals("The specified Serial Number is not available, the pick line should not be picked.", "sn:004", response.Pick.Lines[0].SerialNumber);
			});
		}

		#endregion

		#endregion

		#region TestReattachAndConfirmPickLines_IsNotUpdatingPickLinesAssignedToAnotherUsers

		public void TestReattachAndConfirmPickLines_IsNotUpdatingPickLinesAssignedToAnotherUsers()
		{
			var staff1 = Helper.CreateGlbStaff("AAA", "AAA");
			var staff2 = Helper.CreateGlbStaff("BBB", "BBB");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);

			line1.PickLines[0].WZ_GS_NKAssignedTo = staff2.GS_Code;

			Helper.Factory.Save();

			AssertEquals("Precondition - ensure that PickLine has Serial Number SN:001.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
			AssertEquals("Precondition - ensure that PickLine was Assigned to correct User.", "BBB", line1.PickLines[0].WZ_GS_NKAssignedTo);
			AssertEquals("Precondition - ensure that PickLine was not picked yet.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);

			var pickLineInfo = new WhsPickLineInfo(line1.PickLines[0], new WhsPickInfo());

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("Original PickLine was Assigned to other User, so Original PickLine should not be updated.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("Original PickLine was Assigned to other User, so Original PickLine should not be updated.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);
				AssertEquals("Original PickLine was Assigned to other User, so its picking should be rejected.", 1, response1.Pick.Lines.Count);
				AssertEquals("Original PickLine was Assigned to other User, so its picking should be rejected.", "SN:001", response1.Pick.Lines[0].SerialNumber);
			});

			pickLineInfo.SerialNumber = "SN:002";

			var webService2 = GetNewWebService(data.Whs1, staff1);
			var response2 = webService2.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("Original PickLine was Assigned to other User, so Original PickLine should not be updated.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("Original PickLine was Assigned to other User, so Original PickLine should not be updated.", true, line1.PickLines[0].WZ_PickedDateTime.IsEmpty);
				AssertEquals("Original PickLine was Assigned to other User, so its replacement should be rejected.", 1, response2.Pick.Lines.Count);
				AssertEquals("Original PickLine was Assigned to other User, so its replacement should be rejected.", "SN:002", response2.Pick.Lines[0].SerialNumber);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_IsNotUpdatingPickLinesThatWereAlreadyPicked

		public void TestReattachAndConfirmPickLines_IsNotUpdatingPickLinesThatWereAlreadyPicked()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0], "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure order is picking.", true, order.IsAttachedToPickButNotFinalised);

			var yesterday = ZDateTimeOffset.Today.AddDays(-1);
			line1.PickLines[0].WZ_GS_NKAssignedTo = staff.GS_Code;
			var inTransitLine = Helper.PickAndMakeInTransitTransfer(line1.PickLines[0], yesterday);
			AssertEquals("Precondition - ensure that PickLine has Serial Number SN:001.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);

			Helper.Factory.Save();

			var pickedPickLine = inTransitLine.PickLines.Single();
			AssertEquals("Precondition - ensure that PickLine was Assigned to correct User.", "AAA", pickedPickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Precondition - ensure that PickLine was not picked yet.", yesterday, pickedPickLine.WZ_PickedDateTime);

			var pickLineInfo = new WhsPickLineInfo(line1.PickLines[0], new WhsPickInfo());

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("Original PickLine was already Picked, so Original PickLine should not be updated.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("Original PickLine was already Picked, so Original PickLine should not be updated.", true, line1.PickLines[0].IsPickedFromPutawayLocation);
				AssertEquals("Original PickLine was already Picked, so Original PickLine should not be updated.", ZDateTimeOffset.Empty, line1.PickLines[0].WZ_PickedDateTime);
				AssertEquals("Original PickLine was already Picked, so its picking should be rejected.", 1, response1.Pick.Lines.Count);
				AssertEquals("Original PickLine was already Picked, so its picking should be rejected.", "SN:001", response1.Pick.Lines[0].SerialNumber);
			});

			pickLineInfo.SerialNumber = "SN:002";

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("Original PickLine was already Picked, so Original PickLine should not be updated.", "SN:001", line1.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("Original PickLine was already Picked, so Original PickLine should not be updated.", true, line1.PickLines[0].IsPickedFromPutawayLocation);
				AssertEquals("Original PickLine was already Picked, so Original PickLine should not be updated.", ZDateTimeOffset.Empty, line1.PickLines[0].WZ_PickedDateTime);
				AssertEquals("Original PickLine was already Picked, so its replacement should be rejected.", 1, response2.Pick.Lines.Count);
				AssertEquals("Original PickLine was already Picked, so its replacement should be rejected.", "SN:002", response2.Pick.Lines[0].SerialNumber);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_PacksIntoPackage

		public void TestReattachAndConfirmPickLines_PacksIntoPackage()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			// set attribute neutral on product
			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, "OWN").OU_PickMode = "ANE";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN003";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("BOX", "123");
			var package2 = order.PackageJob.Packages.AddNew("PLT", "456");
			Helper.Factory.Save();

			AssertEquals("Precondition: ensure that PickLine has Serial Number SN001.", true, orderLine.PickLines.Any(p => p.Inventory.WI_SerialNumber == "SN001"));
			AssertEquals("Precondition: ensure that PickLine has Serial Number SN002.", true, orderLine.PickLines.Any(p => p.Inventory.WI_SerialNumber == "SN002"));

			var pickInfo = new WhsPickInfo(pick, orderLine.PickLines, new List<string>());
			var pickLineInfo1 = new WhsPickLineInfo(orderLine.PickLines[0], pickInfo)
			{
				SerialNumber = "sn001",
				Attribute2 = "BLUE"
			};

			orderLine.ClearReleaseLines(); // clear cached values to regenerate with new RCA's
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ReattachAndConfirmPickLines(new[] { pickLineInfo1 }, new PackageInfo { PK = package1.PK.ToGuid(), PackageID = "123" });
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull(response1.Pick);
			AssertEquals("All Pick Lines should be successfully picked.", false, response1.Pick.Lines.Any());

			var pickLine1 = orderLine.PickLines.SingleOrDefault(p => p.InventoryLine.WE_SerialNumber == "SN001");
			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().SingleOrDefault(a => a.SerialNumber == "SN001" && a.PartAttribute2 == "BLUE");
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN001.", pickLine1);
				AssertNotNull("There should be an ReleaseLine SN001 BLUE", releaseLine1);
				AssertEquals("Should have packed the Package.", 1, package1.PackedItems.Count);
				AssertEquals("Should have packed the Correct RCA Line.", pickLine1, package1.PackedItems[0].PackedItems.Single());
				AssertEquals("Should have packed the Correct Release Line.", releaseLine1, package1.PackedItems[0].PackableItemParent);
				AssertEquals("Should have packed the Correct Amount.", 1m, package1.PackedItems[0].PackedQty);
			});

			var pickLineInfo2 = new WhsPickLineInfo(orderLine.PickLines[1], pickInfo)
			{
				SerialNumber = "SN003",
				Attribute2 = "RED"
			};

			orderLine.ClearReleaseLines(); // clear cached values to regenerate with new RCA's
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ReattachAndConfirmPickLines(new[] { pickLineInfo2 }, new PackageInfo { PK = package2.PK.ToGuid(), PackageID = "456" });
			AssertSuccessfulResponse(response2, webService2);

			var newFactory = new BusinessObjectFactory();
			var loadedPackage2 = newFactory.Load<PkgPackage>(package2.PK);
			var updatedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			var pickLine2 = updatedOrderLine.PickLines.SingleOrDefault(p => p.InventoryLine.WE_SerialNumber == "SN003");
			var releaseLine2 = updatedOrderLine.ReleaseLines.Cast<WhsReleaseLine>().SingleOrDefault(a => a.SerialNumber == "SN003" && a.PartAttribute2 == "RED");

			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN003.", pickLine2);
				AssertNotNull("There should be an ReleaseLine SN003 RED", releaseLine2);
				AssertEquals("Should have packed the Package.", 1, loadedPackage2.PackedItems.Count);
				AssertEquals("Should have packed the Correct RCA Line.", pickLine2.PK, loadedPackage2.PackedItems[0].PackedItems.Single().PK);
				AssertEquals("Should have packed the Correct Release Line.", releaseLine2, loadedPackage2.PackedItems[0].PackableItemParent);
				AssertEquals("Should have packed the Correct Amount.", 1m, loadedPackage2.PackedItems[0].PackedQty);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_UpdatesReleaseLinesWithReleaseCapturedInfo

		public void TestReattachAndConfirmPickLines_UpdatesReleaseLinesWithReleaseCapturedInfo()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			// set attribute neutral on product
			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, "OWN").OU_PickMode = "ANE";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN:003";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:001");
			var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:002");

			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(l => l.SerialNumber == "SN:001");
			releaseLine1.PartAttribute2 = "GREEN";
			releaseLine1.Quantity = 1m;

			Helper.Factory.Save();

			var pickInfo = new WhsPickInfo(pick, orderLine.PickLines, new List<string>());
			var pickLineInfo1 = new WhsPickLineInfo(pickLine1, pickInfo)
			{
				SerialNumber = "SN:001",
				Attribute2 = "BLUE"
			};

			var pickLineInfo2 = new WhsPickLineInfo(pickLine2, pickInfo)
			{
				SerialNumber = "SN:003",
				Attribute2 = "RED"
			};

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo1, pickLineInfo2 }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("All Pick Lines should be sucessfully picked.", 0, response.Pick.Lines.Count);
				AssertEquals("Attribs2 on pickLine1 should be blue now.", "BLUE", pickLine1.WZ_ReleaseCapturedPartAttrib2);
			});

			var pickLineSN001 = orderLine.PickLines.SingleOrDefault(p => p.Inventory.WI_SerialNumber == "SN:001");
			var pickLineSN003 = orderLine.PickLines.SingleOrDefault(p => p.Inventory.WI_SerialNumber == "SN:003");
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN:001.", pickLineSN001);
				AssertNotNull("There should be a pickline with Serial Number SN:003.", pickLineSN003);
				AssertEquals("There should be a Release Captured Attribute for BLUE on PickLine for SN001.", "BLUE", pickLineSN001.WZ_ReleaseCapturedPartAttrib2);
				AssertEquals("Serial Number should not be stored on Release Captured Attribute.", "", pickLineSN001.WZ_ReleaseCapturedSerialNumber);
				AssertEquals(1m, pickLineSN001.WZ_Units);
				AssertEquals("There should be a Release Captured Attribute for RED on PickLine for SN003.", "RED", pickLineSN003.WZ_ReleaseCapturedPartAttrib2);
				AssertEquals("Serial Number should not be stored on Release Captured Attribute.", "", pickLineSN003.WZ_ReleaseCapturedSerialNumber);
				AssertEquals(1m, pickLineSN003.WZ_Units);
			});

			AssertNoExceptionThrown(() => webService.Factory.Save());
		}

		#endregion

		#region TestReattachAndConfirmPickLines_PrePackedFromMultiplePicks

		public void TestReattachAndConfirmPickLines_PrePackedFromMultiplePicks()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			// set attribute neutral on product
			data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, "OWN").OU_PickMode = "ANE";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN:003";
			var inventoryLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine4.WI_SerialNumber = "SN:004";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			// create pick for SN1
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine1 = order1.Lines[0];
			var pick1 = Helper.CreatePickNew(order1);
			var pickLine1 = pick1.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:001");
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			Helper.Factory.Save();

			// create pick for SN2
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var orderLine2 = order2.Lines[0];
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine2 = pick2.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:002");
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, pickLine2);
			Helper.Factory.Save();

			// create trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			var trolleySlot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 2);
			var trolleySlot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 4);
			Helper.Factory.Save();

			// Try to change serial numbers + add RCA for multiple pick lines from multiple Picks that are already packed into packages.
			var trolleyJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			var pickLine1Info = new WhsPickLineInfo(pickLine1, trolleyJobInfo)
			{
				SerialNumber = "SN:003",
				Attribute2 = "BLUE"
			};
			var pickLine2Info = new WhsPickLineInfo(pickLine2, trolleyJobInfo)
			{
				SerialNumber = "SN:004",
				Attribute2 = "RED"
			};

			orderLine1.ClearReleaseLines(); // clear cached values to regenerate with new RCA's
			orderLine2.ClearReleaseLines(); // clear cached values to regenerate with new RCA's

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLine1Info, pickLine2Info }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

			var pickLine3 = orderLine1.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:003");
			var releaseLine3 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:003" && a.PartAttribute2 == "BLUE");
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN:003.", pickLine3);
				AssertNotNull("There should be an ReleaseLine SN:003 BLUE", releaseLine3);
				AssertEquals("Should have only 1 item packed in package.", 1, pkg1.PackedItems.Count);
				AssertEquals("Should have packed the Correct RCA Line.", pickLine3, pkg1.PackedItems[0].PackedItems.Single());
				AssertEquals("Should have packed the Correct Release Line.", releaseLine3, pkg1.PackedItems[0].PackableItemParent);
				AssertEquals("Should have packed the Correct Amount.", 1m, pkg1.PackedItems[0].PackedQty);
			});

			var pickLine4 = orderLine2.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:004");
			var releaseLine4 = orderLine2.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:004" && a.PartAttribute2 == "RED");
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN:004.", pickLine4);
				AssertNotNull("There should be an ReleaseLine SN:004 RED", releaseLine4);
				AssertEquals("Should have only 1 item packed in package.", 1, pkg2.PackedItems.Count);
				AssertEquals("Should have packed the Correct RCA Line.", pickLine4, pkg2.PackedItems[0].PackedItems.Single());
				AssertEquals("Should have packed the Correct Release Line.", releaseLine4, pkg2.PackedItems[0].PackableItemParent);
				AssertEquals("Should have packed the Correct Amount.", 1m, pkg2.PackedItems[0].PackedQty);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SerialNeutralPacked

		public void TestReattachAndConfirmPickLines_SerialNeutralPacked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			var inventoryLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine3.WI_SerialNumber = "SN:003";
			var inventoryLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine4.WI_SerialNumber = "SN:004";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			// create pick for SN1
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine1 = order1.Lines[0];
			var pick1 = Helper.CreatePickNew(order1);
			var pickLine1 = pick1.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:001");
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			Helper.Factory.Save();

			// create pick for SN2
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var orderLine2 = order2.Lines[0];
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine2 = pick2.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:002");
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, pickLine2);
			Helper.Factory.Save();

			// We do not specify parent on pickLineInfos as they will be deserialised during webcall with default constructor 
			var pickLine1Info = CreatePickLineInfo(AttributeNumber.Serial, "SN:003", "A", pickLine1.PK, data.Org1.PK, data.Part1.PK);
			var pickLine2Info = CreatePickLineInfo(AttributeNumber.Serial, "SN:004", "A", pickLine2.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLine1Info, pickLine2Info }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

			orderLine1.ClearReleaseLines(); // need to rebuild release lines, since Serial has been swapped
			var pickLine3 = orderLine1.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:003");
			var releaseLine3 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:003");
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN:003.", pickLine3);
				AssertNotNull("There should be an ReleaseLine SN:003", releaseLine3);
				AssertEquals("Should have only 1 item packed in package.", 1, pkg1.PackedItems.Count);
				AssertEquals("Should have packed the Pick Line.", pickLine3, pkg1.PackedItems[0].PackedItems.Single());
				AssertEquals("Should have packed the Correct Release Line.", releaseLine3, pkg1.PackedItems[0].PackableItemParent);
				AssertEquals("Should have packed the Correct Amount.", 1m, pkg1.PackedItems[0].PackedQty);
			});

			orderLine2.ClearReleaseLines(); // need to rebuild release lines, since Serial has been swapped
			var pickLine4 = orderLine2.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:004");
			var releaseLine4 = orderLine2.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:004");
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN:004.", pickLine4);
				AssertNotNull("There should be an ReleaseLine SN:004", releaseLine4);
				AssertEquals("Should have only 1 item packed in package.", 1, pkg2.PackedItems.Count);
				AssertEquals("Should have packed the Pick Line.", pickLine4, pkg2.PackedItems[0].PackedItems.Single());
				AssertEquals("Should have packed the Correct Release Line.", releaseLine4, pkg2.PackedItems[0].PackableItemParent);
				AssertEquals("Should have packed the Correct Amount.", 1m, pkg2.PackedItems[0].PackedQty);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SerialNeutral

		public void TestReattachAndConfirmPickLines_SerialNeutral()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:001");
			Helper.Factory.Save();

			var pickLine1Info = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLine.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLine1Info }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

			var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:002");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:002");
			var pickedInventory = pickLine2.InventoryLineForAvailableInventory;
			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN:002.", pickLine2);
				AssertNotNull("There should be an ReleaseLine SN:002", releaseLine2);
				AssertEquals("Inventory with Serial Number SN:001 should remain 1 unit", 1m, inventory1.WI_TotalUnits);
				AssertEquals("Inventory with Serial Number SN:002 should be picked", inventory2.InDocketLine, pickedInventory);
				AssertEquals("Inventory with Serial Number SN:002 should reduce to 0 unit", 0m, inventory2.WI_TotalUnits);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SerialNeutral_WithRandomPKsAndPackage

		public void TestReattachAndConfirmPickLines_SerialNeutral_WithRandomPKsAndPackage()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:001");
			var pickLine2 = pick.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:002");

			var package = order.PackageJob.Packages.AddNew("BOX", "123");
			Helper.Factory.Save();

			// Mock Mr.Dodginess and pass pickLine2.PK with "SN:001", this will make pickLine1 to be picked first, so in the next iteration pickLine2Info.PKs[0] is a picked line's PK.
			var pickLine1Info = CreatePickLineInfo(AttributeNumber.Serial, "SN:001", "A", pickLine2.PK, data.Org1.PK, data.Part1.PK);
			var pickLine2Info = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLine1.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLine1Info, pickLine2Info }, new PackageInfo { PK = package.PK.ToGuid(), PackageID = "123" });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

			var pickLine3 = orderLine.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:001");
			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:001");
			var pickedInventory1 = pickLine3.InventoryLineForAvailableInventory;

			var pickLine4 = orderLine.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:002");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:002");
			var pickedInventory2 = pickLine4.InventoryLineForAvailableInventory;

			CombineAssertions(() =>
			{
				AssertNotNull("There should be a pickline with Serial Number SN:001.", pickLine3);
				AssertNotNull("There should be an ReleaseLine SN:001", releaseLine1);

				AssertNotNull("There should be a pickline with Serial Number SN:002.", pickLine4);
				AssertNotNull("There should be an ReleaseLine SN:002", releaseLine2);

				AssertEquals("Inventory with Serial Number SN:001 should be picked", inventory1.InDocketLine, pickedInventory1);
				AssertEquals("Inventory with Serial Number SN:001 should reduce to 0 unit", 0m, inventory1.WI_TotalUnits);
				AssertEquals("Inventory with Serial Number SN:002 should be picked", inventory2.InDocketLine, pickedInventory2);
				AssertEquals("Inventory with Serial Number SN:002 should reduce to 0 unit", 0m, inventory2.WI_TotalUnits);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder

		public void TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var orderLine1 = order1.Lines[0];
			var orderLine2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition", "SN:001", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);
			Helper.Factory.Save();

			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());
				AssertEquals("SN:002", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:001", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_SwappingSerialNumberAlreadyOrdered

		public void TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_SwappingSerialNumberAlreadyOrdered()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var orderLine1 = order1.Lines[0];
			var orderLine2 = order2.Lines[0];
			orderLine2.WE_SerialNumber = "SN:002";

			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition", "SN:001", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);

			Helper.Factory.Save();

			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("All Pick Lines should be rejected.", 1, response.Pick.Lines.Count);
				AssertEquals("SN:001", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:002", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_ReleaseCapture

		public void TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_ReleaseCapture()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			partRelation.OU_IsPartAttrib1ReleaseCaptured = true;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition", "SN:001", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);

			var releaseLineForSerial1 = lineInOrder1.ReleaseLines.FirstOrDefault();
			releaseLineForSerial1[WhsReleaseLine.Schema.PartAttribute1] = "Red";
			var releaseLineForSerial2 = lineInOrder2.ReleaseLines.FirstOrDefault();
			releaseLineForSerial2[WhsReleaseLine.Schema.PartAttribute1] = "Green";

			Helper.Factory.Save();

			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);
			pickLineInfo.Attribute1 = "Yellow";

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());
				AssertEquals("SN:002", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:001", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("RCA should have correct Attribute1 as 'Yellow', same as attribute in pick line info.", 1, lineInOrder1.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1 == "Yellow"));
				AssertEquals("RCAs attached to pick line for scanned serial should be removed.", false, lineInOrder2.PickLines.Single().HasReleaseCapturedAttribs);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_ReleaseCaptureAndPackage

		#region TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_1stPLHasRCAAndPackage_2ndPLHasRCANoPackage

		public void TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_1stPLHasRCAAndPackage_2ndPLHasRCANoPackage()
		{
			// setup two inventory lines for different serial numbers
			// Create two orders and allocate above inventory and create RCAs
			// create two picks
			// 1st pick line has Package linked to RCA
			// 2nd pick line has RCA but no package
			// Call webService.ReattachAndConfirmPickLines to swap serial numbers
			// assert 
			//  1st pick line has updated RCAs and above package links to RCA
			//	2nd pick line has no RCAs
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			partRelation.OU_IsPartAttrib1ReleaseCaptured = true;

			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);

			var releaseLineForSerial1 = (WhsReleaseLine)lineInOrder1.ReleaseLines.FirstOrDefault();
			releaseLineForSerial1.PartAttribute1 = "Red";
			var releaseLineForSerial2 = (WhsReleaseLine)lineInOrder2.ReleaseLines.FirstOrDefault();
			releaseLineForSerial2.PartAttribute1 = "Green";

			Helper.Factory.Save();

			var pkgJobInOrder1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageForPickLine1 = packingHelper.CreatePackage(pkgJobInOrder1, "PKG1", 1, Constants.PkgUnit.Box);
			packageForPickLine1.Pack(releaseLineForSerial1, 1m);

			AssertEquals("Precondition: KI_ParentID links to pick line with RCA.", pickLineForOrder1.PK, packageForPickLine1.PackedItemDivots.Single().KI_ParentID);
			AssertEquals("Precondition: Attrib1 on pickLineForOrder1 is Red.", "Red", pickLineForOrder1.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Precondition: Attrib1 on pickLineForOrder1 is Green.", "Green", pickLineForOrder2.WZ_ReleaseCapturedPartAttrib1);

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);
			pickLineInfo.Attribute1 = "Yellow";

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

				AssertEquals("SN:002", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:001", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);

				// RCAs
				AssertEquals("RCA should have correct Attribute1 as 'Yellow', same as attribute in pick line info.", 1,
					lineInOrder1.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1.ToString() == "Yellow"));
				AssertEquals("RCAs attached to pick line for scanned serial should be removed.", false, lineInOrder2.PickLines.Single().HasReleaseCapturedAttribs);

				// package
				AssertEquals("Package's ParentID should be the same as before (RCA).", pickLineForOrder1.PK, packageForPickLine1.PackedItemDivots.Single().KI_ParentID);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_1stPLHasRCANoPackage_2ndPLHasRCAAndPackage

		public void TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_1stPLHasRCANoPackage_2ndPLHasRCAAndPackage()
		{
			// setup two inventory lines for different serial numbers
			// Create two orders and allocate above inventory and create RCAs
			// create two picks
			// 1st pick line has RCA but no package
			// 2nd pick line has Package linked to RCA	
			// Call webService.ReattachAndConfirmPickLines to swap serial numbers
			// assert 
			//  1st pick line has updated RCAs
			//	2nd pick line has no RCAs, above package now links to 2nd pick line.
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			partRelation.OU_IsPartAttrib1ReleaseCaptured = true;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);

			var releaseLineForSerial1 = lineInOrder1.ReleaseLines.FirstOrDefault();
			releaseLineForSerial1[WhsReleaseLine.Schema.PartAttribute1] = "Red";
			var releaseLineForSerial2 = (WhsReleaseLine)lineInOrder2.ReleaseLines.FirstOrDefault();
			releaseLineForSerial2[WhsReleaseLine.Schema.PartAttribute1] = "Green";

			Helper.Factory.Save();

			var pkgJobInOrder2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageForPickLine2 = packingHelper.CreatePackage(pkgJobInOrder2, "PKG1", 1, Constants.PkgUnit.Box);
			packageForPickLine2.Pack(releaseLineForSerial2, 1m);

			AssertEquals("Precondition: KI_ParentID links to pick line with RCA.", pickLineForOrder2.PK, packageForPickLine2.PackedItemDivots.Single().KI_ParentID);
			AssertEquals("Precondition: Attrib1 on pickLineForOrder1 is Red.", "Red", pickLineForOrder1.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Precondition: Attrib1 on pickLineForOrder1 is Green.", "Green", pickLineForOrder2.WZ_ReleaseCapturedPartAttrib1);

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);
			pickLineInfo.Attribute1 = "Yellow";

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

				AssertEquals("SN:002", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:001", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);

				// RCAs
				AssertEquals("RCA should have correct Attribute1 as 'Yellow', same as attribute in pick line info.", 1,
					lineInOrder1.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1.ToString() == "Yellow"));
				AssertEquals("RCAs attached to pick line for scanned serial should be removed.", false, lineInOrder2.PickLines.Single().HasReleaseCapturedAttribs);

				// package
				AssertEquals("Package's ParentID should be changed to pick line 2.", pickLineForOrder2.PK, packageForPickLine2.PackedItemDivots.Single().KI_ParentID);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_BothPLsHaveRCAAndPackage

		public void TestReattachAndConfirmPickLines_AttributeNeutral_AllocatedToOrder_BothPLsHaveRCAAndPackage()
		{
			// setup two inventory lines for different serial numbers
			// Create two orders and allocate above inventory and create RCAs
			// create two picks
			// both pick lines have Package linked to RCA	
			// Call webService.ReattachAndConfirmPickLines to swap serial numbers
			// assert 
			//  1st pick line has updated RCAs and package links to RCA
			//	2nd pick line has no RCAs, above package now links to 2nd pick line.
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			partRelation.OU_IsPartAttrib1ReleaseCaptured = true;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);

			var releaseLineForSerial1 = (WhsReleaseLine)lineInOrder1.ReleaseLines.FirstOrDefault();
			releaseLineForSerial1[WhsReleaseLine.Schema.PartAttribute1] = "Red";
			var releaseLineForSerial2 = (WhsReleaseLine)lineInOrder2.ReleaseLines.FirstOrDefault();
			releaseLineForSerial2[WhsReleaseLine.Schema.PartAttribute1] = "Green";

			Helper.Factory.Save();

			var pkgJobInOrder1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJobInOrder1, "PKG1", 1, Constants.PkgUnit.Box);
			package1.Pack(releaseLineForSerial1, 1m);

			AssertEquals("Precondition", pickLineForOrder1.PK, package1.PackedItemDivots.Single().KI_ParentID);
			AssertEquals("Precondition: Attrib1 on pickLineForOrder1 is Red.", "Red", pickLineForOrder1.WZ_ReleaseCapturedPartAttrib1);

			var pkgJobInOrder2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = packingHelper.CreatePackage(pkgJobInOrder2, "PKG2", 1, Constants.PkgUnit.Box);
			package2.Pack(releaseLineForSerial2, 1m);

			AssertEquals("Precondition", pickLineForOrder2.PK, package2.PackedItemDivots.Single().KI_ParentID);
			AssertEquals("Precondition: Attrib1 on pickLineForOrder1 is Green.", "Green", pickLineForOrder2.WZ_ReleaseCapturedPartAttrib1);

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);
			pickLineInfo.Attribute1 = "Yellow";

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

				AssertEquals("SN:002", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:001", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);

				// RCAs
				AssertEquals("RCA should have correct Attribute1 as 'Yellow', same as attribute in pick line info.", 1,
					lineInOrder1.PickLines.Count(l => l.WZ_ReleaseCapturedPartAttrib1.ToString() == "Yellow"));
				AssertEquals("RCAs attached to pick line for scanned serial should be removed.", false, lineInOrder2.PickLines.Single().HasReleaseCapturedAttribs);

				// package
				AssertEquals("Package1's ParentID should still be pickLineForOrder1.", pickLineForOrder1.PK, package1.PackedItemDivots.Single().KI_ParentID);
				AssertEquals("Package2's ParentID should still be pickLineForOrder2.", pickLineForOrder2.PK, package2.PackedItemDivots.Single().KI_ParentID);
			});
		}

		#endregion

		#endregion

		#region TestReattachAndConfirmPickLines_SwappingPickedSerialNumber

		#region TestReattachAndConfirmPickLines_SwappingOverToPickedSerialNumber

		public void TestReattachAndConfirmPickLines_SwappingOverToPickedSerialNumber()
		{
			AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber(WhsInventoryViewSchema.WI_SerialNumber, true);
		}

		public void TestReattachAndConfirmPickLines_SwappingOverToPickedSerialNumber_WithInTransitTransfer()
		{
			AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber_WithInTransitTransfer(WhsInventoryViewSchema.WI_SerialNumber, true);
		}

		#endregion

		#region TestReattachAndConfirmPickLines_TryingToSwapPickedSerialNumberForUnPickedSerialNumber

		public void TestReattachAndConfirmPickLines_TryingToSwapPickedSerialNumberForUnPickedSerialNumber()
		{
			AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber(WhsInventoryViewSchema.WI_SerialNumber, false);
		}

		public void TestReattachAndConfirmPickLines_TryingToSwapFromUnPickedSerialNumberForPickedSerialNumber()
		{
			AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber(WhsInventoryViewSchema.WI_SerialNumber, true);
		}

		public void TestReattachAndConfirmPickLines_TryingToSwapPickedSerialNumberForUnPickedSerialNumber_WithInTransitTransfer()
		{
			AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber_WithInTransitTransfer(WhsInventoryViewSchema.WI_SerialNumber, false);
		}

		void AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber(SchemaStringColumn serialColumnName, bool swappingFromUnPickedLine)
		{
			AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber(serialColumnName, swappingFromUnPickedLine,
				pl =>
				{
					var orderLine = pl.DocketLine;

					pl.WZ_PickedDateTime = ZDateTimeOffset.Now;

					using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
					{
						Helper.Factory.Save();
						AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
					}
				});
		}

		void AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber_WithInTransitTransfer(SchemaStringColumn serialColumnName, bool swappingFromUnPickedLine)
		{
			AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber(serialColumnName, swappingFromUnPickedLine,
				pl =>
				{
					var orderLine = pl.DocketLine;
					var location = pl.InventoryLine.Location;

					var inTransitTransferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
					inTransitTransferLine.WE_WL = location.PK; // Ensure both serial numbers are in the same location. Dock Door will not suffice.

					inTransitTransferLine.FinaliseDocketLine();
					AssertIsFinalisedPrecondition(inTransitTransferLine);
					Helper.Factory.Save();

					// Clear caches to reflect newly finalised inventory
					foreach (var pick in Helper.Factory.Load<WhsPick>(new ZQuery()))
					{
						pick.ClearAllInventoriesCache();
						pick.OrderedInventories[0].ClearAvailableInventoriesCache();
					}
				});
		}

		void AssertReattachAndConfirmPickLines_SwappingPickedSerialNumber(SchemaStringColumn serialColumnName, bool swappingFromUnPickedLine, Action<WhsPickLine> pickPickLine)
		{
			// setup two inventory lines for different serial numbers, A and B
			// Create two orders and allocate above inventory
			// create two picks one for each serial numbers
			// pick A only
			// Call webService.ReattachAndConfirmPickLines to swap A for B
			// check response for businessvalidation error "Cannot swap serial numbers since A is already picked".

			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1[serialColumnName] = "SN:001";
			inventory2[serialColumnName] = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);

			// SN:002 is picked
			pickPickLine(pickForOrder2.GetAllPickLines().Single());

			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory[serialColumnName]);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory[serialColumnName]);

			Helper.Factory.Save();

			// swappingFromUnPickedLine = true:
			//		try to swap 001 -> 002, but 002 is already picked, should be rejected
			// swappingFromUnPickedLine = false:
			//		try to swap 002 -> 001, 002 is already picked, should be rejected
			var pickLineInfo = swappingFromUnPickedLine
				? CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK)
				: CreatePickLineInfo(AttributeNumber.Serial, "SN:001", "A", pickLineForOrder2.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("SN:001", pickForOrder1.GetAllPickLines().Single().Inventory[serialColumnName]);
				AssertEquals("SN:002", pickForOrder2.GetAllPickLines().Single().Inventory[serialColumnName]);
				AssertEquals("Unsuccessful", 1, response.Pick.Lines.Count);
			});

			var responsePickLineInfo = response.Pick.Lines.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Should show correct OrderNo.", swappingFromUnPickedLine ? order2.WD_ExternalReference : order1.WD_ExternalReference, responsePickLineInfo.AttachedToOrder);
				AssertEquals("Should show correct PickNo.", swappingFromUnPickedLine ? pickForOrder2.WP_PickNo : pickForOrder1.WP_PickNo, responsePickLineInfo.AttachedToPick);
			});
		}

		#endregion

		#endregion

		#region TestReattachAndConfirmPickLines_ShowsCorrectOrderAndPickErrorMessageWithExistingTransfers

		public void TestReattachAndConfirmPickLines_ShowsCorrectOrderAndPickErrorMessageWithExistingTransfers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			// Create transfer to move around SN:002
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, location1, "", location2, ZDate.Empty, ZDate.Empty, "", "", "");
			transferLine.WE_SerialNumber = "SN:002";
			transfer.RunPreSaveValidation();
			transfer.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer);

			Helper.Factory.Save();

			// Create order + picks for both serial 
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.InventoryLine.WE_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.InventoryLine.WE_SerialNumber);

			// SN:002 is picked
			pickLineForOrder2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, lineInOrder2.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
			AssertEquals("Precondition", false, pickLineForOrder1.IsPicked);
			AssertEquals("Precondition", true, pickLineForOrder2.IsPicked);

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("SN:001", pickForOrder1.GetAllPickLines().Single().InventoryLine.WE_SerialNumber);
				AssertEquals("SN:002", pickForOrder2.GetAllPickLines().Single().InventoryLine.WE_SerialNumber);
				AssertEquals("Unsuccessful", 1, response.Pick.Lines.Count);
			});

			var responsePickLineInfo = response.Pick.Lines.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Should show correct OrderNo.", order2.WD_ExternalReference, responsePickLineInfo.AttachedToOrder);
				AssertEquals("Should show correct PickNo.", pickForOrder2.WP_PickNo, responsePickLineInfo.AttachedToPick);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SwappingOverToSerialNumberInAnotherLocation

		public void TestReattachAndConfirmPickLines_SwappingOverToSerialNumberInAnotherLocation()
		{
			// setup two inventory lines for different serial numbers in different locations, A and B
			// Create two orders for each serial numbers and allocate above inventory
			// create two picks
			// Call webService.ReattachAndConfirmPickLines to swap A for B
			// check response for businessvalidation error "Cannot swap serial numbers since A and B are in different locations".
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location1, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location2, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", false, pickLineForOrder1.IsPicked);
			AssertEquals("Precondition", false, pickLineForOrder2.IsPicked);

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A-2", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("SN:001", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:002", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("Unsuccessful", 1, response.Pick.Lines.Count);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SwappingOverToSerialNumberAllocatedToTransfer

		public void TestReattachAndConfirmPickLines_SwappingOverToSerialNumberAllocatedToTransfer()
		{
			// setup two inventory lines for different serial numbers in same locations, A and B
			// Create two orders for each serial numbers and allocate above inventory
			// create two picks
			// create a transfer to transfer out B
			// Call webService.ReattachAndConfirmPickLines to swap A for B
			// check response for businessvalidation error "Cannot swap serial numbers since B is already allocated to a transfer".
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var sourceLocation = data.Whs1.FindLocation("A-1");
			var destinationLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, sourceLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, sourceLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var lineInOrder = order.Lines[0];
			var pickForOrder = Helper.CreatePickNew(order);
			var pickLineForOrder = pickForOrder.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", false, pickLineForOrder.IsPicked);

			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, sourceLocation, destinationLocation);
			transferLine.WE_SerialNumber = "SN:002";
			transfer.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 1m, transferLine.QtyCommittedIncludingMatchingLines);

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A-1", pickLineForOrder.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("SN:001", pickForOrder.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("Unsuccessful", 1, response.Pick.Lines.Count);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SwappingOverToSerialNumberAllocatedToAdjustment

		public void TestReattachAndConfirmPickLines_SwappingOverToSerialNumberAllocatedToAdjustment()
		{
			// setup two inventory lines for different serial numbers in same locations, A and B
			// Create two orders for each serial numbers and allocate above inventory
			// create two picks
			// create a transfer to adjust out B
			// Call webService.ReattachAndConfirmPickLines to swap A for B
			// check response for businessvalidation error "Cannot swap serial numbers since B is already allocated to a adjustment".
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var lineInOrder = order.Lines[0];
			var pickForOrder = Helper.CreatePickNew(order);
			var pickLineForOrder = pickForOrder.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", false, pickLineForOrder.IsPicked);

			Helper.Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -1m, location);
			adjustmentLine.WE_SerialNumber = "SN:002";
			adjustment.RunPreSaveValidation();
			AssertEquals("Precondition: Stock is committed.", 1m, adjustmentLine.CommittedQuantity);

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A-1", pickLineForOrder.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("SN:001", pickForOrder.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("Unsuccessful", 1, response.Pick.Lines.Count);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SwappingSerialNumbersAssignedToPackage

		public void TestReattachAndConfirmPickLines_SwappingOverToSerialNumberAssignedToPackage()
		{
			AssertReattachAndConfirmPickLines_SwappingOverToSerialNumberAssignedToPackage(true);
		}

		public void TestReattachAndConfirmPickLines_SwappingFromSerialNumberAssignedToPackage()
		{
			AssertReattachAndConfirmPickLines_SwappingOverToSerialNumberAssignedToPackage(false);
		}

		void AssertReattachAndConfirmPickLines_SwappingOverToSerialNumberAssignedToPackage(bool isSwappingOver)
		{
			// setup two inventory lines for different serial numbers in same locations, A and B
			// Create two orders for each serial numbers and allocate above inventory
			// create two picks
			// assign serial number B to a package
			// Call webService.ReattachAndConfirmPickLines to swap A for B
			// assert A is now packed to the package and B is not assigned to a package
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", false, pickLineForOrder1.IsPicked);
			AssertEquals("Precondition", false, pickLineForOrder2.IsPicked);

			Helper.Factory.Save();

			var pkgJobInOrder2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package = packingHelper.CreatePackage(pkgJobInOrder2, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, pickLineForOrder2);
			AssertEquals("Precondition", pickLineForOrder2.PK, package.PackedItemDivots.Single().KI_ParentID);

			var pickLineInfo = isSwappingOver
				? CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK)
				: CreatePickLineInfo(AttributeNumber.Serial, "SN:001", "A", pickLineForOrder2.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("SN:002", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:001", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("Package's ParentID should be the same as before.", pickLineForOrder2.PK, package.PackedItemDivots.Single().KI_ParentID);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_MaintainsTaskDetails

		public void TestReattachAndConfirmPickLines_WithUnallocatedLine_MaintainsTaskDetails_SwappedLines()
		{
			AssertReattachAndConfirmPickLines_WithUnallocatedLine_MaintainsTaskDetails(true);
		}

		public void TestReattachAndConfirmPickLines_WithUnallocatedLine_MaintainsTaskDetails()
		{
			AssertReattachAndConfirmPickLines_WithUnallocatedLine_MaintainsTaskDetails(false);
		}

		void AssertReattachAndConfirmPickLines_WithUnallocatedLine_MaintainsTaskDetails(bool isSwappingOver)
		{
			// setup two inventory lines for different serial numbers in same locations, A and B
			// Create one order for first serial numbers and allocate above inventory
			// Create pick
			// Call webService.ReattachAndConfirmPickLines to swap A for B
			// Assert Tasks have modified appropriately
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			var staff2 = Helper.CreateGlbStaff("ST2", "ST2");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine = order.Lines[0];
			var pickForOrder = Helper.CreatePickNew(order);
			var pickLineForOrder = pickForOrder.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", false, pickLineForOrder.IsPicked);

			Helper.Factory.Save();

			var pickLineInfo = isSwappingOver
				? CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder.PK, data.Org1.PK, data.Part1.PK)
				: CreatePickLineInfo(AttributeNumber.Serial, "SN:001", "A", pickLineForOrder.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pickForOrder, staff1);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			var taskPickLines = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, task.PK)).Single();

			CombineAssertions(() =>
			{
				var orderLineForTask = taskPickLines.WZ_WE_OriginalOrderLine.IsValid
					? taskPickLines.WZ_WE_OriginalOrderLine
					: taskPickLines.WZ_WE_TransactionLine;
				AssertEquals(isSwappingOver ? "SN:002" : "SN:001", taskPickLines.Inventory.WI_SerialNumber);
				AssertEquals("Line should be for OrderLine", orderLine.PK, orderLineForTask);
			});
		}

		public void TestReattachAndConfirmPickLines_SwappingWithOtherTask_MaintainsTaskDetails_SwappedLines()
		{
			AssertReattachAndConfirmPickLines_SwappingWithOtherTask_MaintainsTaskDetails(true);
		}

		public void TestReattachAndConfirmPickLines_SwappingWithOtherTask_MaintainsTaskDetails()
		{
			AssertReattachAndConfirmPickLines_SwappingWithOtherTask_MaintainsTaskDetails(false);
		}

		void AssertReattachAndConfirmPickLines_SwappingWithOtherTask_MaintainsTaskDetails(bool isSwappingOver)
		{
			// setup two inventory lines for different serial numbers in same locations, A and B
			// Create two orders for each serial numbers and allocate above inventory
			// Create two picks
			// Call webService.ReattachAndConfirmPickLines to swap A for B
			// Assert Tasks have modified appropriately
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			var staff2 = Helper.CreateGlbStaff("ST2", "ST2");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);
			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", false, pickLineForOrder1.IsPicked);
			AssertEquals("Precondition", false, pickLineForOrder2.IsPicked);

			Helper.Factory.Save();

			var pickLineInfo = isSwappingOver
				? CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK)
				: CreatePickLineInfo(AttributeNumber.Serial, "SN:001", "A", pickLineForOrder2.PK, data.Org1.PK, data.Part1.PK);

			Helper.Factory.Save();

			var task1 = Helper.CreateProcessTaskForPickJob(pickForOrder1, staff1);
			var task2 = Helper.CreateProcessTaskForPickJob(pickForOrder2, staff2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			var taskPickLinesForTask1 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, task1.PK)).Single();
			var taskPickLinesForTask2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, task2.PK)).Single();

			CombineAssertions(() =>
			{
				var orderLineForPickLine1 = taskPickLinesForTask1.WZ_WE_OriginalOrderLine.IsValid
					? taskPickLinesForTask1.WZ_WE_OriginalOrderLine
					: taskPickLinesForTask1.WZ_WE_TransactionLine;
				AssertEquals("SN:002", taskPickLinesForTask1.Inventory.WI_SerialNumber);
				AssertEquals("Line should be for OrderLine1", lineInOrder1.PK, orderLineForPickLine1);

				var orderLineForPickLine2 = taskPickLinesForTask2.WZ_WE_OriginalOrderLine.IsValid
					? taskPickLinesForTask2.WZ_WE_OriginalOrderLine
					: taskPickLinesForTask2.WZ_WE_TransactionLine;
				AssertEquals("SN:001", taskPickLinesForTask2.Inventory.WI_SerialNumber);
				AssertEquals("Line should be for OrderLine2", lineInOrder2.PK, orderLineForPickLine2);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SetIsPicking

		public void TestReattachAndConfirmPickLines_AttributeNeutrals_SetIsPicking()
		{
			// change pickline
			ReattachAndConfirmPickLines_GetWhsPickCore("SN:002");
		}

		public void TestReattachAndConfirmPickLines_AttributeSpecified_SetIsPicking()
		{
			// no change picklineAll Pick Lines should be successfully picked.
			ReattachAndConfirmPickLines_GetWhsPickCore();
		}

		void ReattachAndConfirmPickLines_GetWhsPickCore(string picklineSerial = "")
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine1.WI_SerialNumber = "SN:001";
			var inventoryLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventoryLine2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// create pick for SN1
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderLine = order.Lines[0];
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:001");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertEquals("Precondition - Start picking.", true, pickLine.WZ_IsPicking);

			var pickInfo = new WhsPickInfo(pick, orderLine.PickLines, new List<string>());
			var pickLineInfo = new WhsPickLineInfo(orderLine.PickLines[0], pickInfo);
			if (!string.IsNullOrEmpty(picklineSerial))
			{
				pickLineInfo.SerialNumber = picklineSerial; // force to change the pickline
			}

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService2);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("PickLine still should have the correct Serial Number.", string.IsNullOrEmpty(picklineSerial) ? "SN:001" : "SN:002", orderLine.PickLines[0].Inventory.WI_SerialNumber);
				AssertEquals("PickLine should be picked now.", true, orderLine.PickLines[0].IsPickedFromPutawayLocation);
				AssertEquals("All Pick Lines should be sucessfuly picked", false, response.Pick.Lines.Any());
				AssertEquals("Successfully picked, should set is picking to false.", false, pick.GetAllPickLines().All(pl => pl.WZ_IsPicking));
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SwapPackagesWithinSameOrderAndPick

		public void TestReattachAndConfirmPickLines_SwapPackagesWithinSameOrderAndPick()
		{
			TestReattachAndConfirmPickLines_SwapPackagesWithinSameOrderAndPickCore(hasReleaseCapturedAttribute: false);
		}

		public void TestReattachAndConfirmPickLines_SwapPackagesWithinSameOrderAndPick_WithReleaseCapturedAttributes()
		{
			TestReattachAndConfirmPickLines_SwapPackagesWithinSameOrderAndPickCore(hasReleaseCapturedAttribute: true);
		}

		void TestReattachAndConfirmPickLines_SwapPackagesWithinSameOrderAndPickCore(bool hasReleaseCapturedAttribute)
		{
			// if I have order with 2 packages and 2xProduct with Serial Numbers
			// allocated as: PKG-1 with SN1 and PKG-2 with SN2
			// if I pick SN2 in place of SN1, then result should be
			// result: PKG-1 with SN2 and PKG-2 with SN1
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			var location = data.Whs1.FindLocation("A");

			if (hasReleaseCapturedAttribute)
			{
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, use: true, setReleaseCaptured: true);
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory2.WI_SerialNumber = "SN2";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single().AvailableInventories.Cast<WhsPickAvailableInventory>().Single().PickLines;
			var pickLine1 = pickLines.Single(pl => pl.Inventory.WI_SerialNumber == "SN1");
			var pickLine2 = pickLines.Single(pl => pl.Inventory.WI_SerialNumber == "SN2");
			AssertEquals("Precondition", 1m, pickLine1.WZ_Units);
			AssertEquals("Precondition", 1m, pickLine2.WZ_Units);
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package1, pickLine1);
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			// pick SN2 in place of SN1
			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN2", "A", pickLine1.PK, data.Org1.PK, data.Part1.PK);
			pickLineInfo.Attribute2 = hasReleaseCapturedAttribute ? "RED" : "";

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("PKG1", package1.KP_PackageID);
				AssertEquals("PKG2", package2.KP_PackageID);
				AssertEquals("Underlying inventories for the pick lines should have swapped.", "SN2", pickLine1.InventoryLine.WE_SerialNumber);
				AssertEquals("Underlying inventories for the pick lines should have swapped.", "SN1", pickLine2.InventoryLine.WE_SerialNumber);
				if (hasReleaseCapturedAttribute)
				{
					AssertEquals("Should record accurate release captured attribute.", "RED", pickLine1.WZ_ReleaseCapturedPartAttrib2);
					AssertEquals("Release captured attribute should be linked to the package.", pickLine1.PK, package1.PackedItemDivots.Single().PackedItem.PK);
				}
				else
				{
					AssertEquals("Original pick line should be linked to original package.", pickLine1.PK, package1.PackedItemDivots.Single().PackedItem.PK);
				}
				AssertEquals("Original pick line should be linked to original package.", pickLine2.PK, package2.PackedItemDivots.Single().PackedItem.PK);
			});
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SerialNeutral_GroupByPickOrderedInventory

		public void TestReattachAndConfirmPickLines_SerialNeutral_GroupByPickOrderedInventory()
		{
			using (WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory);
				var packingHelper = new PackingTestHelper(Helper.Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var partRelation = data.Part1.RelatedOrganisations[0];
				partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
				partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventory1.WI_SerialNumber = "SN:001";
				var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventory2.WI_SerialNumber = "SN:002";
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);

				Helper.Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1);
				order.WD_DocketSubType = OrderType.Codes.Customs;
				var orderLine1 = order.Lines[0];
				// when group by PickOrderedInventory it should ignore custom attribute
				orderLine1.WE_CustomAttrib1 = "1";
				orderLine2.WE_CustomAttrib1 = "2";
				Helper.SetOutwardsEntryKeyForOrderLine(orderLine1, "ABC");
				Helper.SetOutwardsEntryKeyForOrderLine(orderLine2, "DEF");
				var pick = Helper.CreatePickNew(order);
				var pickLine1 = pick.GetAllPickLines().Single(l => l.Inventory.WI_SerialNumber == "SN:001");
				Helper.Factory.Save();

				var pickLine1Info = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLine1.PK, data.Org1.PK, data.Part1.PK);

				Helper.Factory.Save();

				var webService = GetNewWebService(data.Whs1);
				var response = webService.ReattachAndConfirmPickLines(new[] { pickLine1Info }, null);
				AssertSuccessfulResponse(response, webService);
				AssertNotNull(response.Pick);
				AssertEquals("All Pick Lines should be successfully picked.", false, response.Pick.Lines.Any());

				var pickLine2 = orderLine1.PickLines.Single(p => p.Inventory.WI_SerialNumber == "SN:002");
				var releaseLine2 = orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(a => a.SerialNumber == "SN:002");
				var pickedInventory = pickLine2.InventoryLineForAvailableInventory;
				CombineAssertions(() =>
				{
					AssertNotNull("There should be a pickline with Serial Number SN:002.", pickLine2);
					AssertNotNull("There should be an ReleaseLine SN:002", releaseLine2);
					AssertEquals("Inventory with Serial Number SN:001 should remain 1 unit", 1m, inventory1.WI_TotalUnits);
					AssertEquals("Inventory with Serial Number SN:002 should be picked", inventory2.InDocketLine, pickedInventory);
					AssertEquals("Inventory with Serial Number SN:002 should reduce to 0 unit", 0m, inventory2.WI_TotalUnits);
				});
			}
		}

		#endregion

		#region TestReattachAndConfirmPickLines_SwappingSerialNumber_SerialNumberPickedFromAnotherFactory

		public void TestReattachAndConfirmPickLines_SwappingSerialNumber_SerialNumberPickedFromAnotherFactory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);

			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			try
			{
				AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
				AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);
			}
			catch (Exception)
			{
				Assert($"Docket: {receive.WD_FinalisedDate.ToString("o")}\nLine1: {receive.Lines[0].WE_FinalisedDate.ToString("o")}\nLine2: {receive.Lines[1].WE_FinalisedDate.ToString("o")}", false);
				throw;
			}

			Helper.Factory.Save();

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);
			var loadCount = 0;
			var webService = GetNewWebService(data.Whs1);
			webService.Factory.RowsLoaded += (s, e) =>
			{
				if (e.TableOrViewName == WhsPickLineSchema.Constants.TableName && e.Query.LiteralTextADO.Contains($"WZ_WE_InventoryLine = CONVERT('{inventory2.PK}', 'System.Guid') and CONVERT(CONVERT(WZ_PickedDateTime, System.String), System.DateTime) is null"))
				{
					if (loadCount != 0)
					{
						throw new InvalidOperationException("PickLine Load should have been done once only.");
					}

					loadCount++;
					var newFactory = new BusinessObjectFactory();
					var pickLineForOrder2InNewFactory = newFactory.Load<WhsPickLine>(pickLineForOrder2.PK);
					var orderLine = pickLineForOrder2InNewFactory.DocketLine;
					pickLineForOrder2InNewFactory.WZ_PickedDateTime = ZDateTimeOffset.Now;
					using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
					{
						newFactory.Save();
						AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
					}
				}
			};
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);

			CombineAssertions(() =>
			{
				AssertEquals("SN:001", pickForOrder1.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("SN:002", pickForOrder2.GetAllPickLines().Single().Inventory.WI_SerialNumber);
				AssertEquals("Unsuccessful", 1, response.Pick.Lines.Count);
			});

			var responsePickLineInfo = response.Pick.Lines.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Should show correct OrderNo.", order2.WD_ExternalReference, responsePickLineInfo.AttachedToOrder);
				AssertEquals("Should show correct PickNo.", pickForOrder2.WP_PickNo, responsePickLineInfo.AttachedToPick);
			});
		}

		public void TestReattachAndConfirmPickLines_SwappingSerialNumber_SerialNumberPickedFromAnotherFactoryOnSave()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);

			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);

			Helper.Factory.Save();

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);
			var webService = GetNewWebService(data.Whs1);
			webService.Factory.Saving += delegate
			{
				var newFactory = new BusinessObjectFactory();
				var pickLineForOrder2InNewFactory = newFactory.Load<WhsPickLine>(pickLineForOrder2.PK);
				var orderLine = pickLineForOrder2InNewFactory.DocketLine;
				pickLineForOrder2InNewFactory.WZ_PickedDateTime = ZDateTimeOffset.Now;
				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					newFactory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			};
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertEquals("Save Exception should be logged as an Error.", "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes.", response.ErrorMessage);
			AssertEquals("Save Exception should be logged as an Error.", ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestReattachAndConfirmPickLines_SwappingSerialNumber_ConcurrencyExceptionOnSave()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;
			partRelation.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, location, "", ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN:001";
			inventory2.WI_SerialNumber = "SN:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var lineInOrder1 = order1.Lines[0];
			var lineInOrder2 = order2.Lines[0];
			var pickForOrder1 = Helper.CreatePickNew(order1);
			var pickForOrder2 = Helper.CreatePickNew(order2);

			var pickLineForOrder1 = pickForOrder1.GetAllPickLines().Single();
			var pickLineForOrder2 = pickForOrder2.GetAllPickLines().Single();
			AssertEquals("Precondition", "SN:001", pickLineForOrder1.Inventory.WI_SerialNumber);
			AssertEquals("Precondition", "SN:002", pickLineForOrder2.Inventory.WI_SerialNumber);

			Helper.Factory.Save();

			var pickLineInfo = CreatePickLineInfo(AttributeNumber.Serial, "SN:002", "A", pickLineForOrder1.PK, data.Org1.PK, data.Part1.PK);
			var webService = GetNewWebService(data.Whs1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLineForOrder2).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);
			var response = webService.ReattachAndConfirmPickLines(new[] { pickLineInfo }, null);
			AssertEquals("Save Exception should be logged as an Error.", "While you have been working with this job another user has made changes. Please restart the operation and try again.", response.ErrorMessage);
			AssertEquals("Save Exception should be logged as an Error.", ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#endregion

		#region CreatePickLineInfo

		WhsPickLineInfo CreatePickLineInfo(AttributeNumber number, string attribValueString, string locationString, ZGuid pickLinePK, ZGuid clientPK, ZGuid productPK)
		{
			var pickLineInfo = new WhsPickLineInfo
			{
				PKs = new[] { pickLinePK.ToGuid() },
				Location = locationString,
				ClientPK = clientPK.ToGuid(),
				ProductPK = productPK.ToGuid()
			};

			switch (number)
			{
				case AttributeNumber.One:
					pickLineInfo.Attribute1 = attribValueString;
					break;
				case AttributeNumber.Two:
					pickLineInfo.Attribute2 = attribValueString;
					break;
				case AttributeNumber.Three:
					pickLineInfo.Attribute3 = attribValueString;
					break;
				case AttributeNumber.Serial:
					pickLineInfo.SerialNumber = attribValueString;
					break;
				default:
					throw new InvalidOperationException("Use a valid Attribute Number.");
			}

			return pickLineInfo;
		}

		#endregion
	}
}

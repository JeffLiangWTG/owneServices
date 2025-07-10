using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ValidatePalletIDForMultiplePutawayTest : ValidatePalletIDOnPutawaySharedTest
	{
		#region TestValidatePalletIDForMultiplePutaway

		[TestDate(2009, 1, 1)]
		public void TestValidatePalletIDForMultiplePutaway()
		{
			var year = ZDateTime.Today.Year - 2;
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3");
			data.Org1.MiscServ.OM_IMUsePackingDate = true;
			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "12345";
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(year, 04, 02),
				new ZDate(year, 04, 01), "P1A1", "P1A2", "P1A3", "");
			inventory1.WI_PalletID = "PalletID1";
			inventory1.WI_WL = data.Whs1.WW_DefaultInboundDockDoor;
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, new ZDate(year, 05, 02),
				new ZDate(year, 05, 01), "P1A1", "P1A2", "P1A3", "");
			inventory2.WI_PalletID = "PalletID2";
			inventory2.WI_WL = data.Whs1.WW_DefaultInboundDockDoor;
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "" }, false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("Provide Pallet ID.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ValidatePalletIDsForMultiplePutaway(new[] { "PalletID0" }, false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("The Pallet ID(s) 'PalletID0' do not exist on an Un-Finalized Receipt.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.ValidatePalletIDsForMultiplePutaway(new[] { "PalletID1" }, false);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals(null, response3.ErrorMessage);
			AssertEquals(ErrorTypes.None, response3.Error);

			var putawayTransfer = ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransfer;
			AssertNotNull(putawayTransfer);
			AssertEquals("Putaway Transfer has correct lines count", 1, putawayTransfer.Lines.Count);
			var putawayTransferLine = putawayTransfer.Lines[0];
			AssertEquals("Putaway TransferLine WE_GS_PutawayBy correct.", staff.GS_Code, putawayTransferLine.WE_GS_NKPutawayBy);

			var putawayJob1 = PutawayHelper.LoadUnfinalisedPutawayJob(Helper.Factory, data.Whs1, staff);
			AssertNotNull(putawayJob1);
			var putawayLines1 = putawayJob1.Lines;
			AssertEquals("PutawayJob Lines count correct.", 1, putawayLines1.Count);
			var putawayLine1 = putawayLines1[0];
			AssertEquals("WPL_PalletID correct", "PalletID1", putawayLine1.WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", true, putawayLine1.WPL_IsPuttingAway);

			var webService4 = GetNewWebService(data.Whs1, staff);
			var response4 = webService4.ValidatePalletIDsForMultiplePutaway(new[] { "PalletID2" }, false);
			AssertSuccessfulResponse(response4, webService4);
			AssertEquals(null, response4.ErrorMessage);
			AssertEquals(ErrorTypes.None, response4.Error);

			var putawayJob2 = PutawayHelper.LoadUnfinalisedPutawayJob(Helper.Factory, data.Whs1, staff);
			AssertNotNull(putawayJob2);
			AssertEquals("Should be same putaway job", putawayJob2, putawayJob1);
			AssertEquals("PutawayJob Lines count correct.", 2, putawayJob2.Lines.Count);
			var putawayLine2 = putawayJob2.Lines[0];
			AssertEquals("WPL_PalletID correct", "PalletID1", putawayLine2.WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", true, putawayLine2.WPL_IsPuttingAway);
			var putawayLine3 = putawayJob2.Lines[1];
			AssertEquals("WPL_PalletID correct", "PalletID2", putawayLine3.WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", true, putawayLine3.WPL_IsPuttingAway);
		}

		public void TestValidatePalletIDForMultiplePutaway_PalletIDsAndDestinationLocations()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_3");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_4");
			Helper.Factory.Save();

			var saveCount = 0;
			var webService = GetNewWebService(data.Whs1, staff);
			webService.Factory.Saving += _ => saveCount++;

			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);
			AssertEquals("Should be 1 Save", 1, saveCount);
		}

		public void TestValidatePalletIDForMultiplePutaway_OneFactorySave()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_3");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_4");
			Helper.Factory.Save();

			var putawayTransfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_2", 10m);
			transferLine1.RunPreSaveValidation();

			var putawayTransfer2 = Helper.CreateWhsTransfer(client2, data.Whs1, "TR1");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT_3", 10m);
			transferLine2.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);

			CombineAssertions(() =>
			{
				var orderedInfos = response.PalletInfos.OrderBy(pi => pi.PalletID);
				AssertEquals("Should be 4 palletInfos", 4, response.PalletInfos.Length);

				var sortedInfos = response.PalletInfos.OrderBy(i => i.PalletID).ToArray();
				AssertEquals("PalletID (1) should be correct", "PLT_1", sortedInfos[0].PalletID);
				AssertEquals("Location (1) should be correct", "", sortedInfos[0].Location);
				AssertEquals("ClientCode (1) should be correct", data.Org1.OH_Code, sortedInfos[0].ClientCode);
				AssertEquals("DocketPK (1) should be correct", receive1.PK, sortedInfos[0].DocketPK);

				AssertEquals("PalletID (2) should be correct", "PLT_2", sortedInfos[1].PalletID);
				AssertEquals("Location (2) should be correct", "A-2", sortedInfos[1].Location);
				AssertEquals("ClientCode (2) should be correct", data.Org1.OH_Code, sortedInfos[1].ClientCode);
				AssertEquals("DocketPK (2) should be correct", receive1.PK, sortedInfos[1].DocketPK);

				AssertEquals("PalletID (3) should be correct", "PLT_3", sortedInfos[2].PalletID);
				AssertEquals("Location (3) should be correct", "A-1", sortedInfos[2].Location);
				AssertEquals("ClientCode (3) should be correct", client2.OH_Code, sortedInfos[2].ClientCode);
				AssertEquals("DocketPK (3) should be correct", receive2.PK, sortedInfos[2].DocketPK);

				AssertEquals("PalletID (4) should be correct", "PLT_4", sortedInfos[3].PalletID);
				AssertEquals("Location (4) should be correct", "", sortedInfos[3].Location);
				AssertEquals("ClientCode (4) should be correct", client2.OH_Code, sortedInfos[3].ClientCode);
				AssertEquals("DocketPK (4) should be correct", receive2.PK, sortedInfos[3].DocketPK);
			});
		}

		public void TestValidatePalletIDForMultiplePutaway_DifferentClientsAndDockets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var client1 = data.Org1;
			var client2 = Helper.CreateClient("Org2");
			var client3 = Helper.CreateClient("Org3");

			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client3, data.Part1);

			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_3");

			var receive3 = Helper.CreateWhsReceive(client3, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_4");

			Helper.Factory.Save();

			var putawayTransfer1 = Helper.CreateWhsTransfer(client1, data.Whs1, "TR0");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_2", 10m);
			transferLine1.RunPreSaveValidation();

			var putawayTransfer2 = Helper.CreateWhsTransfer(client2, data.Whs1, "TR0");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT_3", 10m);
			transferLine2.RunPreSaveValidation();

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2", "PLT_3", "PLT_4" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);

			CombineAssertions(() =>
			{
				var orderedInfos = response.PalletInfos.OrderBy(pi => pi.PalletID).ToArray();
				AssertEquals("Should be 4 palletInfos", 4, response.PalletInfos.Length);
				AssertEquals("PalletID (1) should be correct", "PLT_1", orderedInfos[0].PalletID);
				AssertEquals("Location (1) should be correct", "", orderedInfos[0].Location);
				AssertEquals("ClientCode (1) should be correct", client1.OH_Code, orderedInfos[0].ClientCode);
				AssertEquals("DocketPK (1) should be correct", receive1.PK, orderedInfos[0].DocketPK);
				AssertEquals("PalletID (2) should be correct", "PLT_2", orderedInfos[1].PalletID);
				AssertEquals("Location (2) should be correct", "A-2", orderedInfos[1].Location);
				AssertEquals("ClientCode (2) should be correct", client1.OH_Code, orderedInfos[1].ClientCode);
				AssertEquals("DocketPK (2) should be correct", receive1.PK, orderedInfos[1].DocketPK);
				AssertEquals("PalletID (3) should be correct", "PLT_3", orderedInfos[2].PalletID);
				AssertEquals("Location (3) should be correct", "A-1", orderedInfos[2].Location);
				AssertEquals("ClientCode (3) should be correct", client2.OH_Code, orderedInfos[2].ClientCode);
				AssertEquals("DocketPK (3) should be correct", receive2.PK, orderedInfos[2].DocketPK);
				AssertEquals("PalletID (4) should be correct", "PLT_4", orderedInfos[3].PalletID);
				AssertEquals("Location (4) should be correct", "", orderedInfos[3].Location);
				AssertEquals("ClientCode (4) should be correct", client3.OH_Code, orderedInfos[3].ClientCode);
				AssertEquals("DocketPK (4) should be correct", receive3.PK, orderedInfos[3].DocketPK);
			});
		}

		public void TestValidatePalletIDForMultiplePutaway_Twice()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT-1" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);
			AssertEquals("User exists on transfer line", "S1", transferLine.WE_GS_NKPutawayBy);

			webService.RemovePalletIDFromPutawayJob("PLT-1");
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			AssertEquals("User has been cleared from transfer line", "", transferLine.WE_GS_NKPutawayBy);

			webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT-1" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);
			AssertEquals("User exists on transfer line", "S1", transferLine.WE_GS_NKPutawayBy);

			webService.RemovePalletIDFromPutawayJob("PLT-1");
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			AssertEquals("User has been cleared from transfer line", "", transferLine.WE_GS_NKPutawayBy);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_WithCancelledPalletIDs

		public void TestValidatePalletIDForMultiplePutaway_WithCancelledPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locations[0], "PLT_3");
			Helper.Factory.Save();

			var cancelledReceive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R3");
			var cancelledReceiveLine = Helper.CreateWhsReceiveLine(cancelledReceive, data.Part1, 5m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT_3");
			cancelledReceive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			cancelledReceiveLine.WE_StockOnHand = 0;
			Helper.Factory.Save();
			AssertEquals(DocketStatus.Codes.Cancelled, cancelledReceive.WD_DocketStatus);
			AssertEquals(DocketLineStatus.Codes.Cancelled, cancelledReceiveLine.WE_DocketLineStatus);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_3" }, false);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("No error in the response.", null, response1.ErrorMessage);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_ConsolidatedPallet

		public void TestValidatePalletIDForMultiplePutaway_ConsolidatedPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT-1" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);
			AssertEquals("User exists on transfer line", "S1", transferLine.WE_GS_NKPutawayBy);

			var response2 = webService.RemovePalletIDFromPutawayJob("PLT-1");
			AssertSuccessfulResponse(response2, webService);
			AssertNoResponseError(response2);
			AssertEquals("User has been cleared from transfer line", "", transferLine.WE_GS_NKPutawayBy);

			transferLine.WE_PalletID = "PLT-2"; //Create consolidated pallet datashape.
			Helper.Factory.Save();

			var response3 = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT-1" }, false);
			AssertSuccessfulResponse(response3, webService);
			AssertNoResponseError(response3);
			AssertEquals("User exists on transfer line", "S1", transferLine.WE_GS_NKPutawayBy);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_OnePalletFinalisedOneNot

		public void TestValidatePalletIDForMultiplePutaway_OnePalletFinalisedOneNot()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, null, "PLT_2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT_3");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT_4");
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2", "PLT_4", "PLT_3" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("The Pallet ID(s) 'PLT_4, PLT_3' do not exist on an Un-Finalized Receipt.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_PalletFromDifferentDDL_ToOnesOnPutawayJob

		public void TestValidatePalletIDForMultiplePutaway_PalletFromDifferentDDL_ToOnesOnPutawayJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var dockDoorLocation1 = data.Whs1.DefaultInboundDockDoorLocation;
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation2 = data.Whs1.FindLocation("A-1");
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation2, "PLT_2", 20m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation2, "PLT_3", 30m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_2");
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_3");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals($"The pallet ID 'PLT_2' is not in the same dock door as those previously scanned and those on the current Putaway Job.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestValidatePalletIDForMultiplePutaway_PalletFromDifferentDDL_ToOnesOnPutawayJob_EmptyLoc()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var dockDoorLocation1 = data.Whs1.DefaultInboundDockDoorLocation;
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation2 = data.Whs1.FindLocation("A-1");
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inv = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation1, "PLT_2", 20m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_2");

			inv.InDocketLine.WE_WL = ZGuid.Empty;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);
		}

		public void TestValidatePalletIDForMultiplePutaway_PalletFromDifferentDDL_ToOnesOnPutawayJob_NoLocs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var dockDoorLocation1 = data.Whs1.DefaultInboundDockDoorLocation;
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation2 = data.Whs1.FindLocation("A-1");
			dockDoorLocation2.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inv1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation1, "PLT_1", 10m);
			var inv2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation1, "PLT_2", 20m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_2");

			inv1.InDocketLine.WE_WL = ZGuid.Empty;
			inv2.InDocketLine.WE_WL = ZGuid.Empty;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.ErrorMessage);
			AssertEquals(ErrorTypes.None, response.Error);
		}

		public void TestValidatePalletIDForMultiplePutaway_PalletIDOnDifferentJobIsPuttingAway_NotReassigning()
		{
			TestValidatePalletIDForMultiplePutaway_PalletIDOnDifferentJobIsPuttingAwayCore(isReassigning: false);
		}

		public void TestValidatePalletIDForMultiplePutaway_PalletIDOnDifferentJobIsPuttingAway_IsReassigning()
		{
			TestValidatePalletIDForMultiplePutaway_PalletIDOnDifferentJobIsPuttingAwayCore(isReassigning: true);
		}

		public void TestValidatePalletIDForMultiplePutaway_PalletIDOnDifferentJobIsPuttingAwayCore(bool isReassigning)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2", 20m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, "PLT_3", 30m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_2", true);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_3", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2", "PLT_3" }, isReassigning);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Entered Pallet ID(s) 'PLT_2, PLT_3' are already assigned to another user and are currently being put away.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_OnePutawayTransfer_DBHits

		public void TestValidatePalletIDForMultiplePutaway_OnePutawayTransfer_DBHits()
		{
			TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHits(expectedDockets: 21, expectedInvs: 31, () =>
			{
				return new Dictionary<string, int>()
				{
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 2 }, // second hit coming from a fetch hint
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 2 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
					{ StmALogSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 6 },
					{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 3 },
					{ WhsLocationTypeSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 3 },
					{ WhsPutawayJobSchema.Constants.TableName, 1 },
					{ WhsPutawayLineSchema.Constants.TableName, 3 },
					{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
				};
			});
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_PopulatesPalletInfo

		public void TestValidatePalletIDForMultiplePutaway_PopulatesPalletInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			Helper.SetClientAttributeType(client2, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Part1");
			Helper.SetClientAttributeType(client2, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Part2");
			Helper.SetClientAttributeType(client2, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Part3");
			Helper.SetClientAttributeType(client2, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(client2, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(client2, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAllAttributeUse(client2, data.Part1, true);

			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, null, "PLT_1", new ZDate(2023, 1, 1), new ZDate(2022, 1, 1), "A1", "A2", "A3", "");
			line1.WI_SerialNumber = "S1";

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2", Notify);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, null, "PLT_2", new ZDate(2023, 1, 1), new ZDate(2022, 1, 1), "A4", "A5", "A6", "");
			line2.WI_SerialNumber = "S2";

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response1 = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2" }, false);
			AssertSuccessfulResponse(response1, webService);

			AssertEquals(2, response1.PalletInfos.Length);

			var pallet1 = response1.PalletInfos.Single(p => p.SerialNumber == "S1");
			AssertEquals("P1", pallet1.ProductCode);
			AssertEquals("Attr1", pallet1.PartAttrib1Name);
			AssertEquals("A1", pallet1.PartAttrib1);
			AssertEquals("Attr2", pallet1.PartAttrib2Name);
			AssertEquals("A2", pallet1.PartAttrib2);
			AssertEquals("Attr3", pallet1.PartAttrib3Name);
			AssertEquals("A3", pallet1.PartAttrib3);
			AssertEquals(new DateTime(2023, 1, 1).ToShortDateString(), pallet1.ExpiryDate);
			AssertEquals(new DateTime(2022, 1, 1).ToShortDateString(), pallet1.PackingDate);

			var pallet2 = response1.PalletInfos.Single(p => p.SerialNumber == "S2");
			AssertEquals("P1", pallet2.ProductCode);
			AssertEquals("Part1", pallet2.PartAttrib1Name);
			AssertEquals("A4", pallet2.PartAttrib1);
			AssertEquals("Part2", pallet2.PartAttrib2Name);
			AssertEquals("A5", pallet2.PartAttrib2);
			AssertEquals("Part3", pallet2.PartAttrib3Name);
			AssertEquals("A6", pallet2.PartAttrib3);
			AssertEquals(new DateTime(2023, 1, 1).ToShortDateString(), pallet2.ExpiryDate);
			AssertEquals(new DateTime(2022, 1, 1).ToShortDateString(), pallet2.PackingDate);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_FormattedCheckDigit

		public void TestValidatePalletIDForMultiplePutaway_FormattedCheckDigit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var client1 = data.Org1;
			var client2 = Helper.CreateClient("Org2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);

			var receive1 = Helper.CreateWhsReceive(client1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2");

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_3");

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.FormattedCheckDigit = "11";
			location2.FormattedCheckDigit = "22";

			var putawayTransfer1 = Helper.CreateWhsTransfer(client1, data.Whs1, "TR0");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, location1, "PLT_2", 10m);
			transferLine1.RunPreSaveValidation();

			var putawayTransfer2 = Helper.CreateWhsTransfer(client2, data.Whs1, "TR0");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, location2, "PLT_3", 10m);
			transferLine2.RunPreSaveValidation();

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2", "PLT_3" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.None, response.Error);
			AssertNull(response.ErrorMessage);

			var pallet1 = response.PalletInfos.Single(p => p.PalletID == "PLT_1");
			AssertEquals("", pallet1.LocationFormattedCheckDigit);

			var pallet2 = response.PalletInfos.Single(p => p.PalletID == "PLT_2");
			AssertEquals("11", pallet2.LocationFormattedCheckDigit);

			var pallet3 = response.PalletInfos.Single(p => p.PalletID == "PLT_3");
			AssertEquals("22", pallet3.LocationFormattedCheckDigit);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_ReassignPalletID

		public void TestValidatePalletIDForMultiplePutaway_ReassignPalletID_QuestionAsked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1", 10m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, "PLT_2", 20m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, "PLT_3", 30m);
			Helper.Factory.Save();

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_2");
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_3");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_2", "PLT_3" }, false);
			AssertSuccessfulResponse(response, webService);
			AssertEquals($"Entered Pallet ID(s) 'PLT_2, PLT_3' are already assigned to another user. Do you want to reassign the ID(s) to your Putaway Job?", response.ErrorMessage);
			AssertEquals(ErrorTypes.YesNoEnquiry, response.Error);
		}

		public void TestValidatePalletIDForMultiplePutaway_ReassignPalletID_YesResponse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inv1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT_8");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_1", 10m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff2.GS_Code;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT_8", 10m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff2.GS_Code;
			Helper.Factory.Save();
			AssertEquals("Putaway TransferLine1 WE_GS_PutawayBy correct.", staff2.GS_Code, transferLine1.WE_GS_NKPutawayBy);
			AssertEquals("Putaway TransferLine2 WE_GS_PutawayBy correct.", staff2.GS_Code, transferLine2.WE_GS_NKPutawayBy);

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_1");
			Helper.CreateWhsPutawayLine(putawayJob, "PLT_8");
			Helper.Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayJob1 = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory, data.Whs1, staff2);
			AssertNotNull(putawayJob1);
			var putawayLines1 = putawayJob1.Lines;
			AssertEquals("PutawayJob Lines count correct.", 2, putawayLines1.Count);
			AssertEquals("WPL_PalletID correct", "PLT_1", putawayLines1[0].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", false, putawayLines1[0].WPL_IsPuttingAway);
			AssertEquals("WPL_PalletID correct", "PLT_8", putawayLines1[1].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", false, putawayLines1[1].WPL_IsPuttingAway);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PLT_1", "PLT_8" }, true);
			AssertSuccessfulResponse(response, webService);
			AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals(ErrorTypes.None, response.Error);

			var putawayTransferS1 = ((WhsReceiveLine)inv1.InDocketLine).PutawayTransfer;
			AssertNotNull(putawayTransferS1);
			AssertEquals("Putaway Transfer has correct lines count", 2, putawayTransferS1.Lines.Count);
			var putawayTransferLine1S1 = putawayTransferS1.Lines[0];
			AssertEquals("Putaway TransferLine WE_GS_PutawayBy correct.", staff1.GS_Code, putawayTransferLine1S1.WE_GS_NKPutawayBy);
			var putawayTransferLine2S1 = putawayTransferS1.Lines[1];
			AssertEquals("Putaway TransferLine WE_GS_PutawayBy correct.", staff1.GS_Code, putawayTransferLine2S1.WE_GS_NKPutawayBy);

			var putawayJob2 = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory, data.Whs1, staff1);
			AssertNotNull(putawayJob2);
			var putawayLines2 = putawayJob2.Lines;
			AssertEquals("PutawayJob Lines count correct.", 2, putawayLines2.Count);
			AssertEquals("WPL_PalletID correct", "PLT_1", putawayLines2[0].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", true, putawayLines2[0].WPL_IsPuttingAway);
			AssertEquals("WPL_PalletID correct", "PLT_8", putawayLines2[1].WPL_PalletID);
			AssertEquals("WPL_IsPuttingAway correct", true, putawayLines2[1].WPL_IsPuttingAway);

			var otherFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var putawayJob1After = PutawayHelper.LoadUnfinalisedPutawayJob(otherFactory2, data.Whs1, staff2);
			AssertNotNull(putawayJob1After);
			var putawayLines1After = putawayJob1After.Lines;
			AssertEquals("PutawayJob Lines count correct.", 0, putawayLines1After.Count);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_CrossDock

		public void TestValidatePalletIDForMultiplePutaway_CrossDock()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, null, "PL2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order1.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine1.ReservedQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 15m);
			order2.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Stock is reserved.", 15m, reservedLine2.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory1.Location);
			AssertNull("Precondition: location should be empty.", inventory2.Location);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response.Error);

			AssertEquals("Inventory has putaway transfer.", true, inventory1.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory1.Location);
			AssertEquals("Inventory has putaway transfer.", true, inventory2.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory2.Location);
		}

		public void TestValidatePalletIDOnPutaway_CrossDock_WithAllocatedCrossDockLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("ST1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, null, "PL2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var crossDockLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK").Locations.Single();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			crossDockLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			order1.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory1);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedLine1.ReservedQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 15m);
			order2.WD_WL_CrossDock = crossDockLocation.PK;
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Stock is reserved.", 15m, reservedLine2.ReservedQuantity);

			inventory1.WI_WL = crossDockLocation.PK;
			inventory2.WI_WL = crossDockLocation.PK;
			Helper.Factory.Save();

			AssertEquals("Precondition: location should not be empty.", crossDockLocation, inventory1.Location);
			AssertEquals("Precondition: location should not be empty.", crossDockLocation, inventory2.Location);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response.Error);

			AssertEquals("Inventory has putaway transfer.", true, inventory1.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory1.Location);
			AssertEquals("Inventory has putaway transfer.", true, inventory2.HasPutawayTransfer);
			AssertEquals("location should not be empty.", data.Whs1.DefaultInboundDockDoorLocation, inventory2.Location);

			var transfer = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			AssertEquals(crossDockLocation.PK, transfer.Lines[0].WE_WL);
			AssertEquals(crossDockLocation.PK, transfer.Lines[1].WE_WL);
		}

		#endregion

		#region TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity

		public void TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity_DGExceedsWarehouseLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 5m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-2", 6m);
			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService(data.Whs1);

			// Assert
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "Pallet-1", "Pallet-2" }, false);
			AssertBusinessValidationError(webService, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", response);
		}

		public void TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity_DGUnderWarehouseLimit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 5m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_ArrivalDate = ZDateTimeOffset.Now;
			Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-2", 5m);
			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService(data.Whs1);

			// Assert
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "Pallet-1", "Pallet-2" }, false);
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		public void TestValidatePalletIDForMultiplePutaway_CheckDGLimitCapacity_DGExceedsWarehouseLimit_MultipleSourceDocket()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit3);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 3m);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-2", 4m);

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive3, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-3", 5m);
			Helper.Factory.Save();

			// Act
			var webService = GetNewWebService(data.Whs1);

			// Assert
			var response = webService.ValidatePalletIDsForMultiplePutaway(new[] { "Pallet-1", "Pallet-2", "Pallet-3" }, false);
			AssertBusinessValidationError(webService, @"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", response);
		}

		#endregion

		#region Implementation

		protected override WebServiceResponse GetValidatePalletIDWebServiceResponse(WhsSecureService secureService, string palletID)
			=> secureService.ValidatePalletIDsForMultiplePutaway(new[] { palletID }, false);

		protected override string UnfinalisedReceiptError() => "The Pallet ID(s) '12345' do not exist on an Un-Finalized Receipt.";

		protected override string BondedPalletError() => "Bonded Pallet 'PLT1' should be unloaded on the desktop.";

		protected override void AssertPutawayTransferLine(WhsTransferLine line, OrgSupplierPart product, WhsLocation sourceLocation, WhsLocation destinationLocation, string palletID, decimal quantity, GlbStaff pickedBy, ZDateTimeOffset pickedTime)
		{
			CombineAssertions(() =>
			{
				AssertEquals(product.PK, line.WE_OP);
				AssertEquals(sourceLocation.PK, line.WE_WL_TransferFrom);
				AssertEquals(destinationLocation?.PK ?? ZGuid.Empty, line.WE_WL);
				AssertEquals(palletID, line.WE_TransferFromPalletId);
				AssertEquals(palletID, line.WE_PalletID);
				AssertEquals(InventoryStatus.Codes.PuttingAway, line.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.PuttingAway, line.WE_CurrentInventoryStatus);
				AssertEquals(quantity, line.QtyCommittedIncludingMatchingLines);
				AssertEquals(pickedBy, line.PickedBy);
				AssertEquals(pickedBy, line.PutawayBy);
				AssertEquals(pickedTime, line.PickedTime);
			});
		}

		#endregion
	}
}

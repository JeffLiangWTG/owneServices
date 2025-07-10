using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class AllocateMultiplePalletPutawayLocationsTest : WhsSecureServiceTestCase
	{
		public void TestAllocateMultiplePalletPutawayLocations()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL1");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			var transferLine4 = receiveLine4.PutawayTransferLine;
			var transferLine5 = receiveLine5.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);
			AssertNull("Precondition: TransferLine4 location should be empty.", transferLine4.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine5.Location);

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					AssertEquals("Should use useLocationConcurrencyHandling.", true, useLocationConcurrencyHandling);

					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, string.Empty);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				var locString = location.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", locString, transferLine3.Location.ToLocationString());
				AssertEquals("TransferLine4 location should be correct.", locString, transferLine4.Location.ToLocationString());
				AssertEquals("TransferLine5 location should be correct.", locString, transferLine5.Location.ToLocationString());

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", locString, palletInfo1.Location);
				AssertEquals("PalletInfo1 Location PK correct", location.PK, palletInfo1.LocationPK);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PL1", palletInfo1.AllocatedPalletID);

				var palletInfo2 = orderedInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", locString, palletInfo2.Location);
				AssertEquals("PalletInfo1 Location PK correct", location.PK, palletInfo2.LocationPK);
				AssertEquals("PalletInfo2 ClientCode correct", client2.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive2.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", "PL2", palletInfo2.AllocatedPalletID);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_ConsolidatedPallets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);

			// Allocation/Consolidate Pallet 1
			var location1 = data.Whs1.FindLocation("A-1");
			transferLine1.WE_WL = location1.PK;
			transferLine1.WE_PalletID = "PLT5";
			Helper.Factory.Save();

			var location2 = data.Whs1.FindLocation("A-2");
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines.Where(i => i.PutawayTransferLine.WE_WL.IsEmpty))
					{
						inv.PutawayTransferLine.WE_WL = location2.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, string.Empty);
				AssertNoResponseError(response2);
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				var locString1 = location1.ToLocationString();
				var locString2 = location2.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString1, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString2, transferLine2.Location.ToLocationString());

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", locString1, palletInfo1.Location);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PLT5", palletInfo1.AllocatedPalletID);

				var palletInfo2 = orderedInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", locString2, palletInfo2.Location);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", "PL2", palletInfo2.AllocatedPalletID);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_OnePalletPerClientAndDocket()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var client1 = data.Org1;
			var client2 = Helper.CreateClient("Org2");
			var client3 = Helper.CreateClient("Org3");

			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client3, data.Part1);

			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "INW2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL2");

			var receive3 = Helper.CreateWhsReceive(client3, data.Whs1, "INW2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 30m, dockdoorLocation, "PL3");
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 30m, dockdoorLocation, "PL3");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			var transferLine4 = receiveLine4.PutawayTransferLine;
			var transferLine5 = receiveLine5.PutawayTransferLine;
			var transferLine6 = receiveLine5.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);
			AssertNull("Precondition: TransferLine4 location should be empty.", transferLine4.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine5.Location);
			AssertNull("Precondition: TransferLine6 location should be empty.", transferLine6.Location);

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2", "PL3" }, string.Empty);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				var locString = location.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", locString, transferLine3.Location.ToLocationString());
				AssertEquals("TransferLine4 location should be correct.", locString, transferLine4.Location.ToLocationString());
				AssertEquals("TransferLine5 location should be correct.", locString, transferLine5.Location.ToLocationString());
				AssertEquals("TransferLine6 location should be correct.", locString, transferLine6.Location.ToLocationString());

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 3, orderedInfos.Count());
				var palletInfo1 = orderedInfos.Single(p => p.PalletID == "PL1");
				AssertEquals("PalletInfo1 Location correct", locString, palletInfo1.Location);
				AssertEquals("PalletInfo1 ClientCode correct", client1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PL1", palletInfo1.AllocatedPalletID);

				var palletInfo2 = orderedInfos.Single(p => p.PalletID == "PL2");
				AssertEquals("PalletInfo2 Location correct", locString, palletInfo2.Location);
				AssertEquals("PalletInfo2 ClientCode correct", client2.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive2.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", "PL2", palletInfo2.AllocatedPalletID);

				var palletInfo3 = orderedInfos.Single(p => p.PalletID == "PL3");
				AssertEquals("PalletInfo3 Location correct", locString, palletInfo3.Location);
				AssertEquals("PalletInfo3 ClientCode correct", client3.OH_Code, palletInfo3.ClientCode);
				AssertEquals("PalletInfo3 DocketPK correct", receive3.PK, palletInfo3.DocketPK);
				AssertEquals("PalletInfo3 ConsolidatedPalletID correct", "PL3", palletInfo3.AllocatedPalletID);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_OnePalletPerLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL3");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine3.Location);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var allocatedLocations = new[] { location1, location2, location3 };
			var index = 0;

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					AssertEquals("All receives should be passed in at the same time.", 2, receives.Count());
					AssertEquals("All receivelines should be passed in at the same time for the receives.", 3, receiveLines.Count());

					foreach (var inv in receiveLines.OrderBy(i => i.WE_PalletID))
					{
						inv.PutawayTransferLine.WE_WL = allocatedLocations[index].PK;
						index++;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2", "PL3" }, string.Empty);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				var loc1String = location1.ToLocationString();
				var loc2String = location2.ToLocationString();
				var loc3String = location3.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", loc1String, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", loc2String, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", loc3String, transferLine3.Location.ToLocationString());

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 3, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", palletInfo1.Location, loc1String);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", palletInfo1.AllocatedPalletID, "PL1");

				var palletInfo2 = orderedInfos.Skip(1).First();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", palletInfo2.Location, loc2String);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive1.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", palletInfo2.AllocatedPalletID, "PL2");

				var palletInfo3 = orderedInfos.Last();
				AssertEquals("PalletInfo3 PalletID correct", palletInfo3.PalletID, "PL3");
				AssertEquals("PalletInfo3 Location correct", palletInfo3.Location, loc3String);
				AssertEquals("PalletInfo3 ClientCode correct", data.Org1.OH_Code, palletInfo3.ClientCode);
				AssertEquals("PalletInfo3 DocketPK correct", receive2.PK, palletInfo3.DocketPK);
				AssertEquals("PalletInfo3 ConsolidatedPalletID correct", palletInfo3.AllocatedPalletID, "PL3");
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_Equipment()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var equipment = Helper.CreateEquipment("TRK", 1m, "KG", 1m, "M3");
			equipment.RQ_Registration = "TR1";

			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);

			var location = data.Whs1.DefaultLocation;
			var invPKs = new[] { receiveLine1.PK, receiveLine2.PK, receiveLine3.PK };
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				refEquipmentCondition: refEquipment => refEquipment.PK == equipment.PK,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					receiveLines.ForEach(i => i.PutawayTransferLine.WE_WL = location.PK);
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, "TR1");

				var locString = location.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", palletInfo1.Location, locString);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", palletInfo1.AllocatedPalletID, "PL1");

				var palletInfo2 = orderedInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", palletInfo2.Location, locString);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", palletInfo2.AllocatedPalletID, "PL2");
			}

			mockedEngine.Verify(re => re.Putaway(
				It.Is<IEnumerable<WhsReceive>>(r => r.Any(d => d.PK == receive.PK)),
				It.IsAny<IEnumerable<WhsReceiveLine>>(),
				It.IsAny<INotifications>(),
				It.Is<RefEquipment>(e => e.PK == equipment.PK),
				It.IsAny<IEnumerable<ZGuid>>(),
				It.IsAny<bool>(),
				It.IsAny<bool>()), Times.Exactly(1));
		}

		public void TestAllocateMultiplePalletPutawayLocations_IgnoresOtherPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, dockdoorLocation, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, dockdoorLocation, "PL11");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL12");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			var transferLine4 = receiveLine4.PutawayTransferLine;
			var transferLine5 = receiveLine5.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);
			AssertNull("Precondition: TransferLine4 should be null.", transferLine4);
			AssertNull("Precondition: TransferLine5 should be null.", transferLine5);

			var defaultLocation = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = defaultLocation.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, string.Empty);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(response2.PalletInfos);

				var locString = defaultLocation.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", locString, transferLine3.Location.ToLocationString());
				AssertNull("TransferLine4 should still be null.", transferLine4);
				AssertNull("TransferLine5 should still be null.", transferLine5);

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", palletInfo1.Location, locString);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", palletInfo1.AllocatedPalletID, "PL1");

				var palletInfo2 = orderedInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", palletInfo2.Location, locString);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", palletInfo2.AllocatedPalletID, "PL2");
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_IgnoresDuplicatePalletIDs_OtherWhs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var whs2 = Helper.CreateWarehouse("W22");

			var wh1DDL = data.Whs1.DefaultInboundDockDoorLocation;
			var wh2DDL = whs2.DefaultInboundDockDoorLocation;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, wh1DDL, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, wh1DDL, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, wh1DDL, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, whs2, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, wh2DDL, "PL1");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, wh2DDL, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals(ErrorTypes.None, response1.Error);

			var webService2 = GetNewWebService(whs2, staff1);
			var response2 = webService2.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			AssertEquals(ErrorTypes.None, response2.Error);

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			var transferLine4 = receiveLine4.PutawayTransferLine;
			var transferLine5 = receiveLine5.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);
			AssertNull("Precondition: TransferLine4 location should be empty.", transferLine4.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine5.Location);

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService3 = GetNewWebService(data.Whs1, staff1);
				var response3 = webService3.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, string.Empty);
				AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
				AssertEquals(ErrorTypes.None, response3.Error);
				AssertNotNull(response3.PalletInfos);

				var locString = location.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", locString, transferLine3.Location.ToLocationString());
				AssertNull("TransferLine4 location should still be null.", transferLine4.Location);
				AssertNull("TransferLine5 location should still be null.", transferLine5.Location);

				var orderedInfos = response3.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", "PL1", palletInfo1.PalletID);
				AssertEquals("PalletInfo1 Location correct", locString, palletInfo1.Location);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PL1", palletInfo1.AllocatedPalletID);

				var palletInfo2 = orderedInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", "PL2", palletInfo2.PalletID);
				AssertEquals("PalletInfo2 Location correct", locString, palletInfo2.Location);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", "PL2", palletInfo2.AllocatedPalletID);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_AllocationErrors()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var ddl = data.Whs1.DefaultInboundDockDoorLocation;
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var equipment = Helper.CreateEquipment("TRk", 1m, "KG", 1m, "M3");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, ddl, "PL1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, ddl, "PL2");

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW3");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 20m, ddl, "PL3");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3" }, false);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals(ErrorTypes.None, response1.Error);

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive1.PK),
				receiveLinesCondition: receiveLines => receiveLines.All(i => i.PK.IsValid),
				refEquipmentCondition: refEquipment => refEquipment.PK == equipment.PK,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					notifications.AddError("Error1");

					receiveLines.ForEach(i => i.WE_WL = location.PK);
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL2", "PL1", "PL3" }, "TRK");
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error1", response.ErrorMessage);
				AssertNull("No pallets putaway", response.PalletInfos);
			}

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transferLine1InNewFactory = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			var transferLine2InNewFactory = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			var transferLine3InNewFactory = newFactory.Load<WhsTransferLine>(transferLine3.PK);
			AssertNull("Should *not* have saved if there was an error.", transferLine1InNewFactory.Location);
			AssertNull("Should *not* have saved if there was an error.", transferLine2InNewFactory.Location);
			AssertNull("Should *not* have saved if there was an error.", transferLine3InNewFactory.Location);

			mockedEngine.Verify(re => re.Putaway(
				It.Is<IEnumerable<WhsReceive>>(r => r.Any(d => d.PK == receive1.PK)),
				It.Is<IEnumerable<WhsReceiveLine>>(invs => invs.All(i => i.PK.IsValid)),
				It.IsAny<INotifications>(),
				It.Is<RefEquipment>(e => e.PK == equipment.PK),
				It.IsAny<IEnumerable<ZGuid>>(),
				It.IsAny<bool>(),
				It.IsAny<bool>())
			, Times.Exactly(2));
		}

		public void TestAllocateMultiplePalletPutawayLocations_AllocationWarnings()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL3");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine3.Location);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var allocatedLocations = new[] { location1, location2, location3 };

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					AssertEquals("All receives should be passed in at the same time.", 2, receives.Count());
					AssertEquals("All receivelines should be passed in at the same time for the receives.", 3, receiveLines.Count());

					notifications.AddWarning("Warning1");
					notifications.AddWarning("Warning2");
					var index = 0;
					foreach (var inv in receiveLines.OrderBy(i => i.WE_PalletID))
					{
						inv.PutawayTransferLine.WE_WL = allocatedLocations[index].PK;
						index++;
					}
				});

			var expectedWarning = @"Warning1
Warning2";

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2", "PL3" }, string.Empty);
				AssertEquals(ErrorTypes.WarningOnly, response2.Error);
				AssertEquals(expectedWarning, response2.ErrorMessage);
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				var loc1String = location1.ToLocationString();
				var loc2String = location2.ToLocationString();
				var loc3String = location3.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", loc1String, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", loc2String, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", loc3String, transferLine3.Location.ToLocationString());

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 3, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", palletInfo1.Location, loc1String);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", palletInfo1.AllocatedPalletID, "PL1");

				var palletInfo2 = orderedInfos.Skip(1).First();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", palletInfo2.Location, loc2String);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive1.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", palletInfo2.AllocatedPalletID, "PL2");

				var palletInfo3 = orderedInfos.Last();
				AssertEquals("PalletInfo3 PalletID correct", palletInfo3.PalletID, "PL3");
				AssertEquals("PalletInfo3 Location correct", palletInfo3.Location, loc3String);
				AssertEquals("PalletInfo3 ClientCode correct", data.Org1.OH_Code, palletInfo3.ClientCode);
				AssertEquals("PalletInfo3 DocketPK correct", receive2.PK, palletInfo3.DocketPK);
				AssertEquals("PalletInfo3 ConsolidatedPalletID correct", palletInfo3.AllocatedPalletID, "PL3");
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_ConsolidatedPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var equipment = Helper.CreateEquipment("TRK", 1m, "KG", 1m, "M3");
			equipment.RQ_Registration = "TR1";

			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals(ErrorTypes.None, response1.Error);

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);

			var location = data.Whs1.DefaultLocation;
			var consolidatedPalletID = "PLT8";
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				receivesCondition: receives => receives.Any(d => d.PK == receive.PK),
				refEquipmentCondition: refEquipment => refEquipment.PK == equipment.PK,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
						inv.PutawayTransferLine.WE_PalletID = consolidatedPalletID;
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, "TR1");

				var locString = location.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine1 palletID should be correct.", consolidatedPalletID, transferLine1.WE_PalletID);
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine2 palletID should be correct.", consolidatedPalletID, transferLine2.WE_PalletID);

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", palletInfo1.Location, locString);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", palletInfo1.AllocatedPalletID, consolidatedPalletID);

				var palletInfo2 = orderedInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", palletInfo2.Location, locString);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", palletInfo2.AllocatedPalletID, consolidatedPalletID);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_PalletInfosOrderedByLocationPutawaySequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var whs = Helper.CreateWarehouse("Warehouse");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 3, 2);
			row3.UpdatePathSequenceOnLocations();
			Helper.Factory.Save();

			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = whs.DefaultInboundDockDoorLocation;
			var location1 = whs.FindLocation("A-1-3-1");
			location1.WLV_PutawayPathSequence = 3;
			var location2 = whs.FindLocation("A-1-2-1");
			location2.WLV_PutawayPathSequence = 3;
			var location3 = whs.FindLocation("A-2-1-1");
			location3.WLV_PutawayPathSequence = 2;
			var location4 = whs.FindLocation("B-1-1-1");
			location4.WLV_PutawayPathSequence = 1;
			var location5 = whs.FindLocation("C-2-2-2");
			location5.WLV_PutawayPathSequence = 1;
			var location6 = whs.FindLocation("C-2-2-1");
			location6.WLV_PutawayPathSequence = 3;
			var location7 = whs.FindLocation("B-1-1-1");
			location7.WLV_PutawayPathSequence = 1;

			var receive1 = Helper.CreateWhsReceive(data.Org1, whs, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL3");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL4");
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 60m, dockdoorLocation, "PL5");
			var receiveLine7 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL5");
			var receiveLine8 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 70m, dockdoorLocation, "PL6");
			var receiveLine9 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL7");
			var receiveLine10 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 90m, dockdoorLocation, "PL7");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(whs, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3", "PL4", "PL5", "PL6", "PL7" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var consolidatedPalletID = "PLT8";
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						var transfer = inv.PutawayTransferLine;
						switch (transfer.WE_PalletID)
						{
							case "PL1":
								transfer.WE_WL = location2.PK;
								break;
							case "PL2":
								transfer.WE_WL = location1.PK;
								break;
							case "PL3":
								transfer.WE_WL = location3.PK;
								break;
							case "PL4":
								transfer.WE_WL = location7.PK;
								transfer.WE_PalletID = consolidatedPalletID;
								break;
							case "PL5":
								transfer.WE_WL = location5.PK;
								break;
							case "PL6":
								transfer.WE_WL = location6.PK;
								break;
							case "PL7":
								transfer.WE_WL = location4.PK;
								transfer.WE_PalletID = consolidatedPalletID;
								break;
							default:
								break;
						}
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(whs, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2", "PL3", "PL4", "PL5", "PL6", "PL7" }, string.Empty);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				AssertEquals("PalletInfos count correct", 7, response2.PalletInfos.Length);
				var palletInfo1 = response2.PalletInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", "PL3", palletInfo1.PalletID);
				AssertEquals("PalletInfo1 Location correct", location3.ToLocationString(), palletInfo1.Location);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PL3", palletInfo1.AllocatedPalletID);

				var palletInfo2 = response2.PalletInfos[1];
				AssertEquals("palletInfo2 PalletID correct", "PL1", palletInfo2.PalletID);
				AssertEquals("palletInfo2 Location correct", location2.ToLocationString(), palletInfo2.Location);
				AssertEquals("palletInfo2 ConsolidatedPalletID correct", "PL1", palletInfo2.AllocatedPalletID);

				var palletInfo3 = response2.PalletInfos[2];
				AssertEquals("palletInfo3 PalletID correct", "PL2", palletInfo3.PalletID);
				AssertEquals("palletInfo3 Location correct", location1.ToLocationString(), palletInfo3.Location);
				AssertEquals("palletInfo3 ConsolidatedPalletID correct", "PL2", palletInfo3.AllocatedPalletID);

				var palletInfo4 = response2.PalletInfos[3];
				AssertEquals("palletInfo4 PalletID correct", "PL4", palletInfo4.PalletID);
				AssertEquals("palletInfo4 Location correct", location7.ToLocationString(), palletInfo4.Location);
				AssertEquals("palletInfo4 ConsolidatedPalletID correct", consolidatedPalletID, palletInfo4.AllocatedPalletID);

				var palletInfo5 = response2.PalletInfos[4];
				AssertEquals("palletInfo5 PalletID correct", "PL7", palletInfo5.PalletID);
				AssertEquals("palletInfo5 Location correct", location4.ToLocationString(), palletInfo5.Location);
				AssertEquals("palletInfo5 ConsolidatedPalletID correct", consolidatedPalletID, palletInfo5.AllocatedPalletID);

				var palletInfo6 = response2.PalletInfos[5];
				AssertEquals("palletInfo6 PalletID correct", "PL5", palletInfo6.PalletID);
				AssertEquals("palletInfo6 Location correct", location5.ToLocationString(), palletInfo6.Location);
				AssertEquals("palletInfo6 ConsolidatedPalletID correct", "PL5", palletInfo6.AllocatedPalletID);

				var palletInfo7 = response2.PalletInfos.Last();
				AssertEquals("palletInfo7 PalletID correct", "PL6", palletInfo7.PalletID);
				AssertEquals("palletInfo7 Location correct", location6.ToLocationString(), palletInfo7.Location);
				AssertEquals("palletInfo7 ConsolidatedPalletID correct", "PL6", palletInfo7.AllocatedPalletID);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_PalletInfosOrder_PutawaySequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_PutawayPathSequence = 1;
			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_PutawayPathSequence = 2;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						var transfer = inv.PutawayTransferLine;
						transfer.WE_WL = transfer.WE_PalletID == "PL2" ? location1.PK : location2.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, string.Empty);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				AssertEquals("TransferLine1 location should be correct.", location2.ToLocationString(), transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", location2.ToLocationString(), transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", location1.ToLocationString(), transferLine3.Location.ToLocationString());

				AssertEquals("PalletInfos count correct", 2, response2.PalletInfos.Length);
				var palletInfo1 = response2.PalletInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", "PL2", palletInfo1.PalletID);
				AssertEquals("PalletInfo1 Location correct", location1.ToLocationString(), palletInfo1.Location);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PL2", palletInfo1.AllocatedPalletID);

				var palletInfo2 = response2.PalletInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", "PL1", palletInfo2.PalletID);
				AssertEquals("PalletInfo2 Location correct", location2.ToLocationString(), palletInfo2.Location);
				AssertEquals("PalletInfo2 ClientCode correct", data.Org1.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive1.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", "PL1", palletInfo2.AllocatedPalletID);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_MissingPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL3");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			var transferLine4 = receiveLine4.PutawayTransferLine;
			var transferLine5 = receiveLine5.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);
			AssertNull("Precondition: TransferLine4 should be null.", transferLine4);
			AssertNull("Precondition: TransferLine5 should be null.", transferLine5);

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2", "PL3" }, string.Empty);
				AssertEquals("Pallet ID(s) 'PL2, PL3' are missing an un-finalized putaway transfer.", response2.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_NoJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AllocateMultiplePalletPutawayLocations(new[] { "PL1" }, string.Empty);
			AssertEquals("Putaway Job cannot be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestAllocateMultiplePalletPutawayLocations_NoInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.AllocateMultiplePalletPutawayLocations(new[] { "PL1" }, string.Empty);
			AssertEquals("Pallet ID(s) cannot be found.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		public void TestAllocateMultiplePalletPutawayLocations_PopulatesPalletInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3");
			data.Org1.MiscServ.OM_IMUsePackingDate = true;
			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1", new ZDate(2023, 1, 1), new ZDate(2022, 1, 1), "A1", "A2", "A3", "");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1" }, string.Empty);
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
				AssertNotNull(nameof(response2.PalletInfos), response2.PalletInfos);

				AssertEquals("P1", response2.PalletInfos[0].ProductCode);
				AssertEquals("Attr1", response2.PalletInfos[0].PartAttrib1Name);
				AssertEquals("A1", response2.PalletInfos[0].PartAttrib1);
				AssertEquals("Attr2", response2.PalletInfos[0].PartAttrib2Name);
				AssertEquals("A2", response2.PalletInfos[0].PartAttrib2);
				AssertEquals("Attr3", response2.PalletInfos[0].PartAttrib3Name);
				AssertEquals("A3", response2.PalletInfos[0].PartAttrib3);
				AssertEquals("", response2.PalletInfos[0].SerialNumber);
				AssertEquals(new DateTime(2023, 1, 1).ToShortDateString(), response2.PalletInfos[0].ExpiryDate);
				AssertEquals(new DateTime(2022, 1, 1).ToShortDateString(), response2.PalletInfos[0].PackingDate);
			}
		}

		public void TestAllocateMultiplePalletPutawayLocations_LocationFormattedCheckDigit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);

			// Allocation/Consolidate Pallet 1
			var location1 = data.Whs1.FindLocation("A-1");
			location1.FormattedCheckDigit = "11";
			transferLine1.WE_WL = location1.PK;
			transferLine1.WE_PalletID = "PLT5";

			var location2 = data.Whs1.FindLocation("A-2");
			location2.FormattedCheckDigit = "22";
			Helper.Factory.Save();

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines.Where(i => i.PutawayTransferLine.WE_WL.IsEmpty))
					{
						inv.PutawayTransferLine.WE_WL = location2.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, string.Empty);
				AssertNoResponseError(response2);

				var orderedInfos = response2.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());

				var palletInfo1 = orderedInfos.Single(p => p.PalletID == "PL1");
				AssertEquals("PalletInfo1 PalletID correct", "11", palletInfo1.LocationFormattedCheckDigit);

				var palletInfo2 = orderedInfos.Single(p => p.PalletID == "PL2");
				AssertEquals("PalletInfo2 PalletID correct", "22", palletInfo2.LocationFormattedCheckDigit);
			}
		}

		public void TestReallocateMultiplePalletPutawayLocations_WithSkipLocationPKs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL1");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var transferLine1 = receiveLine1.PutawayTransferLine;
			var transferLine2 = receiveLine2.PutawayTransferLine;
			var transferLine3 = receiveLine3.PutawayTransferLine;
			var transferLine4 = receiveLine4.PutawayTransferLine;
			var transferLine5 = receiveLine5.PutawayTransferLine;
			AssertNull("Precondition: TransferLine1 location should be empty.", transferLine1.Location);
			AssertNull("Precondition: TransferLine2 location should be empty.", transferLine2.Location);
			AssertNull("Precondition: TransferLine3 location should be empty.", transferLine3.Location);
			AssertNull("Precondition: TransferLine4 location should be empty.", transferLine4.Location);
			AssertNull("Precondition: TransferLine5 location should be empty.", transferLine5.Location);

			IEnumerable<Guid> skipLocationPKsPassedIn = null;
			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					AssertEquals("Should use useLocationConcurrencyHandling.", true, useLocationConcurrencyHandling);

					foreach (var inv in receiveLines)
					{
						AssertEquals(ZGuid.Empty, inv.PutawayTransferLine.WE_WL);
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
					skipLocationPKsPassedIn = skipLocations.Select(x => x.ToGuid()).ToArray();
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff1);
				var skipLocationPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
				var response = webService.ReallocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, null, string.Empty, skipLocationPKs);

				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertNotNull(nameof(response.PalletInfos), response.PalletInfos);

				var locString = location.ToLocationString();
				AssertEquals("TransferLine1 location should be correct.", locString, transferLine1.Location.ToLocationString());
				AssertEquals("TransferLine2 location should be correct.", locString, transferLine2.Location.ToLocationString());
				AssertEquals("TransferLine3 location should be correct.", locString, transferLine3.Location.ToLocationString());
				AssertEquals("TransferLine4 location should be correct.", locString, transferLine4.Location.ToLocationString());
				AssertEquals("TransferLine5 location should be correct.", locString, transferLine5.Location.ToLocationString());

				var orderedInfos = response.PalletInfos.OrderBy(i => i.PalletID);
				AssertEquals("PalletInfos count correct", 2, orderedInfos.Count());
				var palletInfo1 = orderedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", palletInfo1.PalletID, "PL1");
				AssertEquals("PalletInfo1 Location correct", locString, palletInfo1.Location);
				AssertEquals("PalletInfo1 Location PK correct", location.PK, palletInfo1.LocationPK);
				AssertEquals("PalletInfo1 ClientCode correct", data.Org1.OH_Code, palletInfo1.ClientCode);
				AssertEquals("PalletInfo1 DocketPK correct", receive1.PK, palletInfo1.DocketPK);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PL1", palletInfo1.AllocatedPalletID);
				AssertContainsExactElementsInExactOrder(skipLocationPKs, skipLocationPKsPassedIn);

				var palletInfo2 = orderedInfos.Last();
				AssertEquals("PalletInfo2 PalletID correct", palletInfo2.PalletID, "PL2");
				AssertEquals("PalletInfo2 Location correct", locString, palletInfo2.Location);
				AssertEquals("PalletInfo1 Location PK correct", location.PK, palletInfo2.LocationPK);
				AssertEquals("PalletInfo2 ClientCode correct", client2.OH_Code, palletInfo2.ClientCode);
				AssertEquals("PalletInfo2 DocketPK correct", receive2.PK, palletInfo2.DocketPK);
				AssertEquals("PalletInfo2 ConsolidatedPalletID correct", "PL2", palletInfo2.AllocatedPalletID);
			}
		}

		public void TestReallocateMultiplePalletPutawayLocations_ClearPalletIDForConsolidatedPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL1");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var client3 = Helper.CreateClient("C3");
			Helper.CreateProductClientRelationShip(client3, data.Part1);
			var receive3 = Helper.CreateWhsReceive(client3, data.Whs1, "INW3");
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive3, data.Part1, 50m, dockdoorLocation, "PL3");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			receiveLine1.PutawayTransferLine.WE_PalletID = "PL3";
			receiveLine4.PutawayTransferLine.WE_PalletID = "PL4";
			receiveLine6.PutawayTransferLine.WE_PalletID = "PL5";

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService = GetNewWebService(data.Whs1, staff1);
				var skipLocationPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
				var response = webService.ReallocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, null, string.Empty, skipLocationPKs);
				AssertEquals("Consolidated pallet ID should be cleared", "PL1", receiveLine1.PutawayTransferLine.WE_PalletID);
				AssertEquals("Consolidated pallet ID should be cleared", "PL2", receiveLine4.PutawayTransferLine.WE_PalletID);
				AssertEquals("Consolidated pallet ID should not be cleared as it's not reallocated.", "PL5", receiveLine6.PutawayTransferLine.WE_PalletID);
			}
		}

		public void TestReallocateMultiplePalletPutawayLocations_ValidateTransferLineFinalized()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL1");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			var transferLine = receiveLine1.PutawayTransferLine;
			AssertNotNull("Precondition: putaway transfer line is not null.", transferLine);
			Helper.Factory.Save();

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, string.Empty);

				transferLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(transferLine);

				Helper.Factory.Save();
				var reallocateResponse = webService2.ReallocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, null, "", null);
				AssertEquals("Putaway has been already completed for one or more pallet ids", reallocateResponse.ErrorMessage);
			}
		}

		public void TestReallocateMultiplePalletPutawayLocations_UpdateLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL2");

			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL1");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var location = data.Whs1.DefaultLocation;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var reallocateResponse = webService2.ReallocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, null, "", null);
				AssertEquals(location.PK, receiveLine1.PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, receiveLine2.PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, receiveLine3.PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, receiveLine4.PutawayTransferLine.Location.PK);
			}
		}

		public void TestReallocateMultiplePalletPutawayLocations_EndToEnd()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(data.Org1, data.Whs1);
			whsClientParameterByWarehouse1.WY_CycleCountOnAlternatePutaway = true;
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var palletIDs = new[] { "PL1", "PL2" };

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL1");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "INW2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 30m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(palletIDs, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService = GetNewWebService(data.Whs1, staff1);
				var response = webService.AllocateMultiplePalletPutawayLocations(palletIDs, string.Empty);
				var infos = response.PalletInfos;

				AssertEquals("PalletInfos count correct", 2, infos.Length);

				var palletIDsInInfo = infos.Select(x => x.PalletID).ToArray();
				var allocatedLocations = infos.Select(x => x.Location).ToArray();
				var allocatedLocationPKs = infos.Select(x => x.LocationPK).ToArray();

				AssertContainsExactElementsInAnyOrder(palletIDs, palletIDsInInfo);
				AssertContainsExactElementsInAnyOrder(new[] { "A-1", "A-2" }, allocatedLocations);
				AssertContainsExactElementsInAnyOrder(new[] { location1.PK.ToGuid(), location2.PK.ToGuid() }, allocatedLocationPKs);

				Helper.Factory.Save();

				var webService2 = GetNewWebService(data.Whs1, staff1);
				var reallocateResponse = webService2.ReallocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, null, "", allocatedLocationPKs);
				infos = reallocateResponse.PalletInfos;

				AssertEquals("PalletInfos count correct", 2, infos.Length);

				palletIDsInInfo = infos.Select(x => x.PalletID).ToArray();
				allocatedLocations = infos.Select(x => x.Location).ToArray();
				allocatedLocationPKs = infos.Select(x => x.LocationPK).ToArray();

				AssertContainsExactElementsInAnyOrder(palletIDs, palletIDsInInfo);
				AssertContainsExactElementsInAnyOrder(new[] { "A-3", "A-4" }, allocatedLocations);
				AssertContainsExactElementsInAnyOrder(new[] { location3.PK.ToGuid(), location4.PK.ToGuid() }, allocatedLocationPKs);

				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;
				var createCycleCounts = factory.Load<WhsCycleCountLocation>(new ZQuery());
				AssertEquals("Only 2 task should be created", 2, createCycleCounts.Length);
				AssertContainsExactElementsInAnyOrder(new[] { location1.PK, location2.PK }, new[] { createCycleCounts[0].WCL_WL_Location, createCycleCounts[1].WCL_WL_Location });
				AssertEquals("Cycle Count 1 should be created to the specified priority", (byte)1, createCycleCounts[0].WCL_Priority);
				AssertEquals("Cycle Count 2 should be created to the specified priority", (byte)1, createCycleCounts[1].WCL_Priority);
			}
		}

		public void TestReallocateMultiplePalletPutawayLocations_PalletInfosOrderedByLocationPutawaySequence()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var whs = Helper.CreateWarehouse("Warehouse");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 3, 2);
			row3.UpdatePathSequenceOnLocations();
			Helper.Factory.Save();

			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = whs.DefaultInboundDockDoorLocation;
			var location1 = whs.FindLocation("A-1-3-1");
			location1.WLV_PutawayPathSequence = 3;
			var location2 = whs.FindLocation("A-1-2-1");
			location2.WLV_PutawayPathSequence = 3;
			var location3 = whs.FindLocation("A-2-1-1");
			location3.WLV_PutawayPathSequence = 2;
			var location4 = whs.FindLocation("B-1-1-1");
			location4.WLV_PutawayPathSequence = 1;
			var location5 = whs.FindLocation("C-2-2-2");
			location5.WLV_PutawayPathSequence = 1;
			var location6 = whs.FindLocation("C-2-2-1");
			location6.WLV_PutawayPathSequence = 3;
			var location7 = whs.FindLocation("B-1-1-1");
			location7.WLV_PutawayPathSequence = 1;

			// Lowest priority location
			var location8 = whs.FindLocation("C-1-2-1");
			location8.WLV_PutawayPathSequence = 4;

			var receive1 = Helper.CreateWhsReceive(data.Org1, whs, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 40m, dockdoorLocation, "PL3");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL4");
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 60m, dockdoorLocation, "PL5");
			var receiveLine7 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL5");
			var receiveLine8 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 70m, dockdoorLocation, "PL6");
			var receiveLine9 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 20m, dockdoorLocation, "PL7");
			var receiveLine10 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 90m, dockdoorLocation, "PL7");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(whs, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2", "PL3", "PL4", "PL5", "PL6", "PL7" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var consolidatedPalletID = "PLT8";
			var isInReallocate = false;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						var transfer = inv.PutawayTransferLine;
						switch (transfer.WE_PalletID)
						{
							case "PL1":
								transfer.WE_WL = isInReallocate ? location8.PK : location2.PK;
								break;
							case "PL2":
								transfer.WE_WL = location1.PK;
								break;
							case "PL3":
								transfer.WE_WL = location3.PK;
								break;
							case "PL4":
								transfer.WE_WL = location7.PK;
								transfer.WE_PalletID = consolidatedPalletID;
								break;
							case "PL5":
								transfer.WE_WL = location5.PK;
								break;
							case "PL6":
								transfer.WE_WL = location6.PK;
								break;
							case "PL7":
								transfer.WE_WL = location4.PK;
								transfer.WE_PalletID = consolidatedPalletID;
								break;
							default:
								break;
						}
					}
				});

			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(whs, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2", "PL3", "PL4", "PL5", "PL6", "PL7" }, string.Empty);
				var infos = response2.PalletInfos.ToList();

				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));

				// Assert PL1 is the second in the pallet infos.
				var palletInfo = infos[1];
				AssertEquals("palletInfo PalletID correct", "PL1", palletInfo.PalletID);
				AssertEquals("palletInfo Location correct", location2.ToLocationString(), palletInfo.Location);
				AssertEquals("palletInfo ConsolidatedPalletID correct", "PL1", palletInfo.AllocatedPalletID);

				// Removes pallet info of PL1 and reallocate it to location8.
				infos.RemoveAt(1);
				isInReallocate = true;
				var response3 = webService2.ReallocateMultiplePalletPutawayLocations(new[] { "PL1" }, infos.ToArray(), string.Empty, null);
				var reallocatedInfos = response3.PalletInfos;

				AssertEquals("PalletInfos count correct", 7, reallocatedInfos.Length);
				var palletInfo1 = reallocatedInfos.First();
				AssertEquals("PalletInfo1 PalletID correct", "PL3", palletInfo1.PalletID);
				AssertEquals("PalletInfo1 Location correct", location3.ToLocationString(), palletInfo1.Location);
				AssertEquals("PalletInfo1 ConsolidatedPalletID correct", "PL3", palletInfo1.AllocatedPalletID);

				var palletInfo2 = reallocatedInfos[1];
				AssertEquals("palletInfo2 PalletID correct", "PL2", palletInfo2.PalletID);
				AssertEquals("palletInfo2 Location correct", location1.ToLocationString(), palletInfo2.Location);
				AssertEquals("palletInfo2 ConsolidatedPalletID correct", "PL2", palletInfo2.AllocatedPalletID);

				var palletInfo3 = reallocatedInfos[2];
				AssertEquals("palletInfo3 PalletID correct", "PL4", palletInfo3.PalletID);
				AssertEquals("palletInfo3 Location correct", location7.ToLocationString(), palletInfo3.Location);
				AssertEquals("palletInfo3 ConsolidatedPalletID correct", consolidatedPalletID, palletInfo3.AllocatedPalletID);

				var palletInfo4 = reallocatedInfos[3];
				AssertEquals("palletInfo4 PalletID correct", "PL7", palletInfo4.PalletID);
				AssertEquals("palletInfo4 Location correct", location4.ToLocationString(), palletInfo4.Location);
				AssertEquals("palletInfo4 ConsolidatedPalletID correct", consolidatedPalletID, palletInfo4.AllocatedPalletID);

				var palletInfo5 = reallocatedInfos[4];
				AssertEquals("palletInfo5 PalletID correct", "PL5", palletInfo5.PalletID);
				AssertEquals("palletInfo5 Location correct", location5.ToLocationString(), palletInfo5.Location);
				AssertEquals("palletInfo5 ConsolidatedPalletID correct", "PL5", palletInfo5.AllocatedPalletID);

				var palletInfo6 = reallocatedInfos[5];
				AssertEquals("palletInfo6 PalletID correct", "PL6", palletInfo6.PalletID);
				AssertEquals("palletInfo6 Location correct", location6.ToLocationString(), palletInfo6.Location);
				AssertEquals("palletInfo6 ConsolidatedPalletID correct", "PL6", palletInfo6.AllocatedPalletID);

				var palletInfo7 = reallocatedInfos.Last();
				AssertEquals("palletInfo7 PalletID correct", "PL1", palletInfo7.PalletID);
				AssertEquals("palletInfo7 Location correct", location8.ToLocationString(), palletInfo7.Location);
				AssertEquals("palletInfo7 ConsolidatedPalletID correct", "PL1", palletInfo7.AllocatedPalletID);
			}
		}

		public void TestReallocateMultiplePalletPutawayLocations_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 10, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");

			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var location = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var location4 = data.Whs1.FindLocation("A-4");
			var location5 = data.Whs1.FindLocation("A-5");
			var location6 = data.Whs1.FindLocation("A-6");
			var location7 = data.Whs1.FindLocation("A-7");
			var location8 = data.Whs1.FindLocation("A-8");
			var location9 = data.Whs1.FindLocation("A-9");
			var location10 = data.Whs1.FindLocation("A-10");
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					foreach (var inv in receiveLines)
					{
						inv.PutawayTransferLine.WE_WL = location.PK;
					}
				});

			var putawayPalletInfos = new PutawayPalletInfo[]
			{
				new PutawayPalletInfo() { LocationPK = location2.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location3.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location4.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location5.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location6.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location7.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location8.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location9.PK.ToGuid() },
				new PutawayPalletInfo() { LocationPK = location10.PK.ToGuid() },
			};

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var expectedDBHits = new Dictionary<string, int>()
				{
					{ WhsDocketLineSchema.Constants.TableName, 7 },
					{ WhsLocationViewSchema.Constants.TableName, 3 },
					{ WhsPickLineSchema.Constants.TableName, 4 },
				};

				using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService2.Factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
				{
					var reallocateResponse = webService2.ReallocateMultiplePalletPutawayLocations(new[] { "PL1", "PL2" }, putawayPalletInfos, "", null);
					Assert("Should be no error message.", string.IsNullOrEmpty(reallocateResponse.ErrorMessage));
					AssertEquals("Should be no error", ErrorTypes.None, reallocateResponse.Error);
				}
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestReallocateMultiplePalletPutawayLocations_ChangeIDInLocationCacheMaxRetry()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");

			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					throw new PutawayAllocateLocationConcurrencyException();
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.ReallocateMultiplePalletPutawayLocations(new string[] { "PL1", "PL2" }, null, "", null);
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Another user has changed the Receive Job while you have been working on it. Please restart the operation and try again.", response2.ErrorMessage);

				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(5));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestReallocateMultiplePalletPutawayLocations_ChangeIDInLocationCacheRetryThreeTimes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");

			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var location = data.Whs1.DefaultLocation;
			var exceptionToThrow = 2;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					if (exceptionToThrow-- > 0)
					{
						throw new PutawayAllocateLocationConcurrencyException();
					}
					else
					{
						foreach (var inv in receiveLines)
						{
							inv.PutawayTransferLine.WE_WL = location.PK;
						}
					}
				});
			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.ReallocateMultiplePalletPutawayLocations(new string[] { "PL1", "PL2" }, null, "", null);
				AssertEquals(location.PK, receiveLine1.PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, receiveLine2.PutawayTransferLine.Location.PK);

				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(3));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestReallocateMultiplePalletPutawayLocations_AllcateOnTwoConnections()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");

			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var location = data.Whs1.DefaultLocation;

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var allocatedInSecondConnection = false;
				var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
					refEquipmentCondition: refEquipment => refEquipment is null,
					action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
					{
						foreach (var inv in receiveLines)
						{
							// Mimic setting location ConcurrencyPolicy to strict as PutawayEngineManager does.
							var locationInInventoryFactory = inv.Factory.Load<WhsLocation>(location.PK);
							ConcurrencyInfo.SetConcurrencyPolicy(locationInInventoryFactory, nameof(WhsLocationViewSchema.WLV_LastAllocatedOrChangedID), ConcurrencyPolicy.Strict);

							inv.PutawayTransferLine.WE_WL = location.PK;
						}

						if (!allocatedInSecondConnection)
						{
							allocatedInSecondConnection = true;
							var webService2 = GetNewWebService(data.Whs1, staff1);
							webService2.Factory.RefreshEnabled = false;
							var response2 = webService2.ReallocateMultiplePalletPutawayLocations(new string[] { "PL1", "PL2" }, null, "", null);
						}
					});

				using (ObjectFactory.Substitute(mockedEngine.Object))
				{
					webService1.ReallocateMultiplePalletPutawayLocations(new string[] { "PL1", "PL2" }, null, "", null);
					var inventory1 = webService1.Factory.Load<WhsReceiveLine>(receiveLine1.PK).Inventory[0];
					AssertEquals(location.PK, ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransferLine.Location.PK);

					var inventory2 = webService1.Factory.Load<WhsReceiveLine>(receiveLine2.PK).Inventory[0];
					AssertEquals(location.PK, ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransferLine.Location.PK);

					mockedEngine.Verify(re => re.Putaway(
						It.IsAny<IEnumerable<WhsReceive>>(),
						It.IsAny<IEnumerable<WhsReceiveLine>>(),
						It.IsAny<INotifications>(),
						null,
						It.IsAny<IEnumerable<ZGuid>>(),
						It.IsAny<bool>(),
						It.IsAny<bool>()),
						Times.Exactly(3),
						"Putaway should happen 3 times, 1 for connection2 and succeeds, 2 for connection1 and fails, 3 for connection1 and retry succeeds.");
				}
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestReallocateMultiplePalletPutawayLocations_SkipLocationCacheUpdate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");

			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var location = data.Whs1.DefaultLocation;
			var runTimes = 0;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					runTimes++;
					if (runTimes == 1)
					{
						Assert("should shortcircuit rebuilding the cache on the first run of Allocate when it is on RF device", !needRebuildLocationCache);
						throw new PutawayAllocateLocationConcurrencyException();
					}
					else
					{
						Assert(needRebuildLocationCache);
						foreach (var inv in receiveLines)
						{
							inv.PutawayTransferLine.WE_WL = location.PK;
						}
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.ReallocateMultiplePalletPutawayLocations(new string[] { "PL1", "PL2" }, null, "", null);
				AssertEquals(location.PK, receiveLine1.PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, receiveLine2.PutawayTransferLine.Location.PK);

				AssertEquals("Should execute allocate twice in total", 2, runTimes);
				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(2));
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestAllocateMultiplePalletPutawayLocations_SkipLocationCacheUpdate()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var dockdoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, dockdoorLocation, "PL1");

			Helper.CreateProductClientRelationShip(data.Org1, data.Part1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 50m, dockdoorLocation, "PL2");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.ValidatePalletIDsForMultiplePutaway(new[] { "PL1", "PL2" }, false);
			AssertEquals(ErrorTypes.None, response1.Error);
			AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));

			var location = data.Whs1.DefaultLocation;
			var runTimes = 0;
			var mockedEngine = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive(
				refEquipmentCondition: refEquipment => refEquipment is null,
				action: (receives, receiveLines, notifications, refEquipment, skipLocations, useLocationConcurrencyHandling, needRebuildLocationCache) =>
				{
					runTimes++;
					if (runTimes == 1)
					{
						Assert("should shortcircuit rebuilding the cache on the first run of Allocate when it is on RF device", !needRebuildLocationCache);
						throw new PutawayAllocateLocationConcurrencyException();
					}
					else
					{
						Assert(needRebuildLocationCache);
						foreach (var inv in receiveLines)
						{
							inv.PutawayTransferLine.WE_WL = location.PK;
						}
					}
				});

			using (Globals.SetIsUserInteractiveForTest(false))
			using (ObjectFactory.Substitute(mockedEngine.Object))
			{
				var webService2 = GetNewWebService(data.Whs1, staff1);
				var response2 = webService2.AllocateMultiplePalletPutawayLocations(new string[] { "PL1", "PL2" }, "");
				AssertEquals(location.PK, receiveLine1.PutawayTransferLine.Location.PK);
				AssertEquals(location.PK, receiveLine2.PutawayTransferLine.Location.PK);

				AssertEquals("Should execute allocate twice in total", 2, runTimes);
				mockedEngine.Verify(re => re.Putaway(
					It.IsAny<IEnumerable<WhsReceive>>(),
					It.IsAny<IEnumerable<WhsReceiveLine>>(),
					It.IsAny<INotifications>(),
					null,
					It.IsAny<IEnumerable<ZGuid>>(),
					It.IsAny<bool>(),
					It.IsAny<bool>()), Times.Exactly(2));
			}
		}
	}
}

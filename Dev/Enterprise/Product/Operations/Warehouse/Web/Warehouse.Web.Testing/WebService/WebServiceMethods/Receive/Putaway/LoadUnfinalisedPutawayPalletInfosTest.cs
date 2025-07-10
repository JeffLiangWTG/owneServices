using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class LoadUnfinalisedPutawayPalletInfosTest : WhsSecureServiceTestCase
	{
		public void TestLoadUnfinalisedPutawayPalletInfos()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT3");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT3");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-2"), "NOTPLT");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", true);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT3", true);

			var putawayJob2 = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			Helper.CreateWhsPutawayLine(putawayJob2, "NOTPLT");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			var expectedClient = data.Org1.OH_Code.ToString();

			CombineAssertions(() =>
			{
				var palletInfoResults = response.PalletInfos.Select(p => (p.PalletID, p.Location, p.ClientCode, p.DocketPK));
				AssertEquals("Correct Pallet ID count", 3, palletInfoResults.Count());
				AssertContainsExactElementsInAnyOrder(new[] { ("PLT-1", "A-2", expectedClient, receive.PK.ToGuid()), ("PLT-2", "A-1", expectedClient, receive.PK.ToGuid()), ("PLT3", "A-1", expectedClient, receive.PK.ToGuid()) }, palletInfoResults);
			});
		}

		public void TestLoadUnfinalisedPutawayPalletInfosLocationPKs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var location1 = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, location1, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "PLT-2");
			Helper.Factory.Save();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			var expectedClient = data.Org1.OH_Code.ToString();

			CombineAssertions(() =>
			{
				var palletInfoResults = response.PalletInfos.Select(p => (p.PalletID, p.Location, p.ClientCode, p.DocketPK, p.LocationPK));
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						("PLT-1", "A-1", expectedClient, receive.PK.ToGuid(), location1.PK.ToGuid()),
						("PLT-2", "", expectedClient, receive.PK.ToGuid(), Guid.Empty),
					},
					palletInfoResults);
			});
		}

		public void TestLoadUnfinalisedPutawayPalletInfosOrder()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var whs = Helper.CreateWarehouse("Warehouse");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 3, 2);
			row3.UpdatePathSequenceOnLocations();
			Helper.Factory.Save();

			var dockdoorLocation = whs.DefaultInboundDockDoorLocation;
			var location2 = whs.FindLocation("A-1-2-1");
			location2.WLV_PutawayPathSequence = 3;
			var location3 = whs.FindLocation("A-2-1-1");
			location3.WLV_PutawayPathSequence = 2;
			var location4 = whs.FindLocation("B-1-1-1");
			location4.WLV_PutawayPathSequence = 1;
			var location6 = whs.FindLocation("C-2-2-1");
			location6.WLV_PutawayPathSequence = 3;
			var location7 = whs.FindLocation("B-1-1-1");
			location7.WLV_PutawayPathSequence = 1;

			var receive = Helper.CreateWhsReceive(data.Org1, whs, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, whs.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location2, "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location3, "PLT3");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location4, "PLT6");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location6, "PLT5");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location7, "NOTPLT");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, whs, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, whs.DefaultInboundDockDoorLocation, location7, "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob = Helper.CreateWhsPutawayJob(whs, staff1);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT-1", true);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT-2", true);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT3", true);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT6", true);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT5", true);

			Helper.Factory.Save();

			var webService = GetNewWebService(whs, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			var expectedClient = data.Org1.OH_Code.ToString();

			CombineAssertions(() =>
			{
				var palletInfoResults = response.PalletInfos.Select(p => (p.PalletID, p.Location, p.ClientCode, p.DocketPK));
				AssertEquals("Correct Pallet ID count", 5, palletInfoResults.Count());
				AssertContainsExactElementsInExactOrder(new[] {
					("PLT3", "A-2-1-1", expectedClient, receive.PK.ToGuid()),
					("PLT-2", "A-1-2-1", expectedClient, receive.PK.ToGuid()),
					("PLT-1", "B-1-1-1", expectedClient, receive.PK.ToGuid()),
					("PLT6", "B-1-1-1", expectedClient, receive.PK.ToGuid()),
					("PLT5", "C-2-2-1", expectedClient, receive.PK.ToGuid()) },
					palletInfoResults);
			});
		}

		public void TestLoadUnfinalisedPutawayPalletInfos_PalletsFromDifferentClientsAndDockets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var client1 = data.Org1;
			var client2 = Helper.CreateClient("Org2");
			var client3 = Helper.CreateClient("Org3");

			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client3, data.Part1);

			Helper.Factory.Save();

			var receive1 = Helper.CreateWhsReceive(client1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT-2");

			var receive3 = Helper.CreateWhsReceive(client3, data.Whs1, "R3", Notify);
			Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT3");
			Helper.CreateWhsReceiveLine(receive3, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT3");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(client1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", true);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT3", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			CombineAssertions(() =>
			{
				var palletInfoResults = response.PalletInfos.Select(p => (p.PalletID, p.Location, p.ClientCode, p.DocketPK));
				AssertEquals("Correct Pallet ID count", 3, palletInfoResults.Count());
				AssertContainsExactElementsInAnyOrder(new[] { ("PLT-1", "A-2", client1.OH_Code.ToString(), receive1.PK.ToGuid()), ("PLT-2", "A-1", client2.OH_Code.ToString(), receive2.PK.ToGuid()), ("PLT3", "A-1", client3.OH_Code.ToString(), receive3.PK.ToGuid()) }, palletInfoResults);
			});
		}

		public void TestLoadUnfinalisedPutawayPalletInfos_IgnoresPutawayPallets()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-3");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory
			transferLine.FinaliseDocketLine();

			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "PLT-2", 50m);
			transferLine2.RunPreSaveValidation(); // to commit inventory

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", isPuttingAway: false);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", isPuttingAway: false);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-3", isPuttingAway: false);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			var expectedClient = data.Org1.OH_Code.ToString();

			CombineAssertions(() =>
			{
				var palletInfoResults = response.PalletInfos.Select(p => (p.PalletID, p.Location, p.ClientCode, p.DocketPK));
				AssertEquals("Correct Pallet ID count", 2, palletInfoResults.Count());
				AssertContainsExactElementsInAnyOrder(new[] { ("PLT-2", "", expectedClient, receive.PK.ToGuid()), ("PLT-3", "", expectedClient, receive.PK.ToGuid()) }, palletInfoResults);
			});
		}

		public void TestLoad_UnfinalisedJobNoLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			AssertEquals("Correct Pallet ID count", 0, response.PalletInfos.Length);
		}

		public void TestLoad_NoJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory
			Helper.Factory.Save();

			AssertNull(FindUnfinalisedPutawayJob(data.Whs1.PK, staff.GS_Code));

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			AssertEquals("Correct Pallet ID count", 0, response.PalletInfos.Length);

			AssertNotNull(FindUnfinalisedPutawayJob(data.Whs1.PK, staff.GS_Code));
		}

		WhsPutawayJob FindUnfinalisedPutawayJob(ZGuid whsPK, string staffCode)
		{
			var query = new ZQuery(WhsPutawayJobSchema.WPJ_WW_Warehouse, whsPK);
			query.AddToFilter(WhsPutawayJobSchema.WPJ_GS_NKUser, staffCode);
			query.AddToFilter(WhsPutawayJobSchema.WPJ_FinalizedTimeUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);

			var putawayJob = Helper.Factory.LoadTop1<WhsPutawayJob>(query);
			return putawayJob;
		}

		public void TestLoad_UnfinalisedJobDifferentUser()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			AssertEquals("Correct Pallet ID count", 0, response.PalletInfos.Length);
		}

		public void TestLoad_UnfinalisedJobDifferentWhs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2");
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob = Helper.CreateWhsPutawayJob(whs2, staff);
			Helper.CreateWhsPutawayLine(putawayJob, "PLT1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			AssertEquals("Correct Pallet ID count", 0, response.PalletInfos.Length);
		}

		public void TestLoad_FinalisedJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.UtcNow;
			Helper.CreateWhsPutawayLine(putawayJob, "PLT-1", isFinalised: true);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(putawayTransfer);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			AssertNull(FindUnfinalisedPutawayJob(data.Whs1.PK, staff.GS_Code));

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			AssertEquals("Correct Pallet ID count", 0, response.PalletInfos.Length);
			AssertNotNull(FindUnfinalisedPutawayJob(data.Whs1.PK, staff.GS_Code));
		}

		public void TestLoad_SuspendedJob()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var dockdoor = data.Whs1.DefaultInboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, dockdoor, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoor, "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoor, "PLT3");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 50m);
			transferLine1.RunPreSaveValidation();
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT-2", 10m);
			transferLine2.RunPreSaveValidation();
			var transferLine3 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-1"), "PLT3", 10m);
			transferLine3.RunPreSaveValidation();

			// Putaway Lines are created in WPL_IsPuttingAway = false simulating a suspended PutawayJob
			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayLine1 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", isPuttingAway: false);
			var putawayLine2 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT-2", isPuttingAway: false);
			var putawayLine3 = Helper.CreateWhsPutawayLine(putawayJob1, "PLT3", isPuttingAway: false);
			Helper.Factory.Save();

			AssertEquals("Precondition: PutawayLine1 IsPuttingAway == 0", false, putawayLine1.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLine2 IsPuttingAway == 0", false, putawayLine2.WPL_IsPuttingAway);
			AssertEquals("Precondition: PutawayLine3 IsPuttingAway == 0", false, putawayLine3.WPL_IsPuttingAway);

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);

			CombineAssertions(() =>
			{
				var palletInfoResults = response.PalletInfos.Select(p => (p.PalletID, p.Location, p.ClientCode));
				AssertEquals("Correct Pallet ID count", 3, palletInfoResults.Count());
				AssertContainsExactElementsInAnyOrder(new[] { ("PLT-1", "A-2", data.Org1.OH_Code.ToString()), ("PLT-2", "A-1", data.Org1.OH_Code.ToString()), ("PLT3", "A-1", data.Org1.OH_Code.ToString()) }, palletInfoResults);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				AssertEquals("PutawayLine1 IsPuttingAway == 1", true, newFactory.Load<WhsPutawayLine>(putawayLine1.PK).WPL_IsPuttingAway);
				AssertEquals("PutawayLine2 IsPuttingAway == 1", true, newFactory.Load<WhsPutawayLine>(putawayLine2.PK).WPL_IsPuttingAway);
				AssertEquals("PutawayLine3 IsPuttingAway == 1", true, newFactory.Load<WhsPutawayLine>(putawayLine3.PK).WPL_IsPuttingAway);
			});
		}

		public void TestLoadUnfinalisedPutawayPalletInfos_DbHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 100, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var staff2 = Helper.CreateGlbStaff("S2", "Staff2");

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			var putawayJob2 = Helper.CreateWhsPutawayJob(data.Whs1, staff2);
			for (int i = 0; i < 50; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R{i}", Notify);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, $"PLT-1{i}");
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation($"A-{i + 1}"), $"PLT-2{i}");
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation($"A-{i + 1}"), $"PLT3{i}");
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation($"A-{i + 2}"), $"NOTPLT{i}");
				Helper.Factory.Save();

				var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, $"TR0{i}");
				putawayTransfer.WD_IsPutawayTransfer = true;
				var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation($"A-{i + 2}"), $"PLT-1{i}", 50m);
				transferLine.RunPreSaveValidation(); // to commit inventory

				Helper.CreateWhsPutawayLine(putawayJob1, $"PLT-1{i}");
				Helper.CreateWhsPutawayLine(putawayJob1, $"PLT-2{i}");
				Helper.CreateWhsPutawayLine(putawayJob1, $"PLT3{i}");

				Helper.CreateWhsPutawayLine(putawayJob2, $"NOTPLT{i}");
				Helper.Factory.Save();
			}

			var webService = GetNewWebService(data.Whs1, staff1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsPutawayJobSchema.Constants.TableName, 1 },
				{ WhsPutawayLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var response = webService.LoadUnfinalisedPutawayPalletInfos();
				Assert("Should be no error message.", string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			}
		}

		public void TestLoadUnfinalisedPutawayPalletInfos_ShowStockOnHandWarningOnPutaway()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var staff = Helper.CreateGlbStaff("S1", "Staff1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT-1", 10m);
			transferLine.RunPreSaveValidation(); // to commit inventory

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.Factory.Save();

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var webService1 = GetNewWebService(data.Whs1, staff);
				var response1 = webService1.LoadUnfinalisedPutawayPalletInfos();
				AssertSuccessfulResponse(response1, webService1);
				AssertEquals("ShowStockOnHandWarningOnPutaway correct", true, response1.ShowStockOnHandWarningOnPutaway);
			}

			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response2 = webService2.LoadUnfinalisedPutawayPalletInfos();
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("ShowStockOnHandWarningOnPutaway correct", false, response2.ShowStockOnHandWarningOnPutaway);
			}
		}

		public void TestLoadUnfinalisedPutawayPalletInfos_PopulatesPalletInfo()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory, "Attr3");
			data.Org1.MiscServ.OM_IMUsePackingDate = true;
			data.Org1.MiscServ.OM_IMUseExpiryDate = true;
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 50m, data.Whs1.DefaultInboundDockDoorLocation.PK, "PLT-1", new ZDate(2023, 1, 1), new ZDate(2022, 1, 1), "A1", "A2", "A3", "SN1", "");
			Helper.Factory.Save();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1", true);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response1 = webService.LoadUnfinalisedPutawayPalletInfos();
			Assert("Should be no error message.", string.IsNullOrEmpty(response1.ErrorMessage));
			AssertEquals("Should be no error", ErrorTypes.None, response1.Error);

			AssertSuccessfulResponse(response1, webService);
			AssertEquals("P1", response1.PalletInfos[0].ProductCode);
			AssertEquals("Attr1", response1.PalletInfos[0].PartAttrib1Name);
			AssertEquals("A1", response1.PalletInfos[0].PartAttrib1);
			AssertEquals("Attr2", response1.PalletInfos[0].PartAttrib2Name);
			AssertEquals("A2", response1.PalletInfos[0].PartAttrib2);
			AssertEquals("Attr3", response1.PalletInfos[0].PartAttrib3Name);
			AssertEquals("A3", response1.PalletInfos[0].PartAttrib3);
			AssertEquals("SN1", response1.PalletInfos[0].SerialNumber);
			AssertEquals(new DateTime(2023, 1, 1).ToShortDateString(), response1.PalletInfos[0].ExpiryDate);
			AssertEquals(new DateTime(2022, 1, 1).ToShortDateString(), response1.PalletInfos[0].PackingDate);
		}

		public void TestLoadUnfinalisedPutawayPalletInfos_LocationFormattedCheckDigit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 100, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "Staff1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			var location = data.Whs1.FindLocation("A-2");
			location.FormattedCheckDigit = "22";

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, location, "PLT-1", 50m);
			transferLine.RunPreSaveValidation();

			var putawayJob1 = Helper.CreateWhsPutawayJob(data.Whs1, staff1);
			Helper.CreateWhsPutawayLine(putawayJob1, "PLT-1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.LoadUnfinalisedPutawayPalletInfos();
			AssertEquals("Should be no error", ErrorTypes.None, response.Error);
			AssertEquals("22", response.PalletInfos.Single().LocationFormattedCheckDigit);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStaffProductivityReportTest : WhsTestCaseWithFactory
	{
		#region TestStaffProductivityReport_Unload

		public void TestStaffProductivityReport_Unload()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var warehouse2 = Helper.CreateWarehouse("2", "B", 20, 1);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part1 = Helper.CreateProduct(org2, "P3");
			Factory.Save();

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			var staffPKs1 = GetStaffPKList(testStaff1);
			var staffGroupPks1 = GetGroupPKList(testGroup1);

			var testGroup2 = GetGroup("GG2");
			var testStaff2 = GetStaff(testGroup2, "XX2", "X2");

			var testGroup3 = GetGroup("GG3");
			var testStaff3 = GetStaff(testGroup2, "XX3", "X3");

			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RU1", data.Part1, data.Whs1.FindLocation("A-1"),
				testStaff1, ZDateTime.Now, Events.AddedARecordToTheSystem.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RU2", org2Part1, warehouse2.FindLocation("B-1"), testStaff2,
				ZDateTime.Now.AddDays(-3), Events.AddedARecordToTheSystem.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RU3", org2Part1, warehouse2.FindLocation("B-2"), testStaff3,
				ZDateTime.Now, Events.AddedARecordToTheSystem.Code);
			Factory.Save();

			AssertResult(data.Whs1, warehouse2, data.Org1, org2, testStaff1, testStaff2, testStaff3, staffPKs1,
				staffGroupPks1, "UNL", 100m);
		}

		#endregion

		#region TestStaffProductivityReport_Putaway

		public void TestStaffProductivityReport_Putaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var warehouse2 = Helper.CreateWarehouse("2", "B", 20, 1);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part1 = Helper.CreateProduct(org2, "P3");
			Factory.Save();

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			var staffPKs1 = GetStaffPKList(testStaff1);
			var staffGroupPks1 = GetGroupPKList(testGroup1);

			var testGroup2 = GetGroup("GG2");
			var testStaff2 = GetStaff(testGroup2, "XX2", "X2");

			var testGroup3 = GetGroup("GG3");
			var testStaff3 = GetStaff(testGroup2, "XX3", "X3");

			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RP1", data.Part1, data.Whs1.FindLocation("A-1"),
				testStaff1, ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RP2", org2Part1, warehouse2.FindLocation("B-1"), testStaff2,
				ZDateTime.Now.AddDays(-3), Events.WarehouseReceiptConfirmedPutaway.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RP3", org2Part1, warehouse2.FindLocation("B-2"), testStaff3,
				ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);
			Factory.Save();

			AssertResult(data.Whs1, warehouse2, data.Org1, org2, testStaff1, testStaff2, testStaff3, staffPKs1,
				staffGroupPks1, "PUT", 100m);
		}

		#endregion

		#region CreateAndAddEventToInventory

		void CreateAndAddEventToInventory(WhsWarehouse warehouse, OrgHeader org, string reference, OrgSupplierPart part,
			WhsLocation location, GlbStaff testStaff, ZDateTime eventDateTime, string eventCode)
		{
			var receive =
				Helper.CreateWhsReceiveWithInventory(org, warehouse, reference, part, 100m, location, "", false, true);
			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single(o => o.Location == location);
			CreateInventoryLog(eventCode, testStaff, inventory.InDocketLine, eventDateTime);
		}

		#endregion

		#region TestStaffProductivityReport_Pick

		public void TestStaffProductivityReport_Pick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var warehouse2 = Helper.CreateWarehouse("2", "B", 20, 1);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part1 = Helper.CreateProduct(org2, "P3");
			Factory.Save();

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			var staffPKs1 = GetStaffPKList(testStaff1);
			var staffGroupPks1 = GetGroupPKList(testGroup1);

			var testGroup2 = GetGroup("GG2");
			var testStaff2 = GetStaff(testGroup2, "XX2", "X2");

			var testGroup3 = GetGroup("GG3");
			var testStaff3 = GetStaff(testGroup2, "XX3", "X3");

			CreateAndCompletePick(data.Whs1, data.Org1, "RPC1", data.Part1, 100m, data.Whs1.FindLocation("A-1"),
				testStaff1, ZDateTime.Now);
			CreateAndCompletePick(warehouse2, org2, "RPC2", org2Part1, 100m, warehouse2.FindLocation("B-1"), testStaff2,
				ZDateTime.Now.AddDays(-3));
			CreateAndCompletePick(warehouse2, org2, "RPC3", org2Part1, 100m, warehouse2.FindLocation("B-2"), testStaff3,
				ZDateTime.Now);

			var receive = Helper.CreateWhsReceiveWithInventory(org2, warehouse2, "RPC4", org2Part1, 100m,
				warehouse2.FindLocation("B-2"), "", false, true);
			var order = Helper.CreateWhsOrder(org2, warehouse2, "ORPC4", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, org2Part1, 100m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(receive.Inventory[0]);
			reservedPickLine.WZ_GS_NKAssignedTo =
				testStaff3.GS_Code; // shouldn't happen, but need this to test that un-picked picklines are excluded
			Factory.Save();

			AssertResult(data.Whs1, warehouse2, data.Org1, org2, testStaff1, testStaff2, testStaff3, staffPKs1,
				staffGroupPks1, "PIC", 100m);
		}

		public void TestStaffProductivityReport_Pick_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var testStaff1 = Helper.CreateGlbStaff("HIL", "Hilary Clinton");
			var testStaff2 = Helper.CreateGlbStaff("TRP", "Donald Trump");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var now = ZDateTimeOffset.Now;
			var pickLine = orderLine.PickLines.Single();
			var inTransitLine = Helper.PickAndMakeInTransitTransfer(pickLine, now.AddDays(-5));
			var newPickLine = inTransitLine.PickLines.Single();
			newPickLine.WZ_GS_NKAssignedTo = testStaff1.GS_Code;

			// Avoid creating second dock door transfer as we just want to test the correct values are used.
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				pickLine.WZ_GS_NKAssignedTo = testStaff2.GS_Code;
				pickLine.WZ_PickedDateTime = now;
				Factory.Save();
			}

			var staffPKs1 = GetStaffPKList(testStaff1);
			var results1 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs1, new List<string>(), "PIC",
				now.AddDays(-6), now.AddDays(-4));
			AssertEquals("Should return the single Pick Job.", 1, results1.Count);
			AssertColumnValues(data.Whs1.WW_WarehouseName, testStaff1.GS_Code, testStaff1.GS_FullName, "PIC", 100m, 1,
				results1[0]);

			var results2 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs1, new List<string>(), "PIC",
				now.AddDays(-6), now);
			AssertEquals("Should return the single Pick Job.", 1, results2.Count);
			AssertColumnValues(data.Whs1.WW_WarehouseName, testStaff1.GS_Code, testStaff1.GS_FullName, "PIC", 100m, 1,
				results2[0]);
		}

		public void TestStaffProductivityReport_Pick_InTransit_MultipleTransferSteps()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var testStaff1 = Helper.CreateGlbStaff("HIL", "Hilary Clinton");
			var testStaff2 = Helper.CreateGlbStaff("TRP", "Donald Trump");
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			var now = ZDateTimeOffset.Now;
			var pickLine = orderLine.PickLines.Single();

			for (var i = 0; i < 5; i++)
			{
				var inTransitLine =
					Helper.PickAndMakeInTransitTransfer(pickLine, now.AddDays(-5 + i), allowMultipleSteps: true);

				if (i < 4)
				{
					inTransitLine.WE_WL = data.Whs1.FindLocation("A-" + (i + 2)).PK;
				}

				var newPickLine = inTransitLine.PickLines.Single();
				newPickLine.WZ_GS_NKAssignedTo = i > 0 ? testStaff2.GS_Code : testStaff1.GS_Code;
				inTransitLine.FinaliseDocketLine();
				AssertIsFinalisedPrecondition(inTransitLine);
			}

			// Avoid creating second dock door transfer as we just want to test the correct values are used.
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				pickLine.WZ_GS_NKAssignedTo = testStaff2.GS_Code;
				pickLine.WZ_PickedDateTime = now;
				Factory.Save();
			}

			var staffPKs1 = GetStaffPKList(testStaff1);
			var results1 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs1, new List<string>(), "PIC",
				now.AddDays(-6), now.AddDays(-4));
			AssertEquals("Should return the single Pick Job.", 1, results1.Count);
			AssertColumnValues(data.Whs1.WW_WarehouseName, testStaff1.GS_Code, testStaff1.GS_FullName, "PIC", 100m, 1,
				results1[0]);

			var results2 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs1, new List<string>(), "PIC",
				now.AddDays(-6), now);
			AssertEquals("Should return the single Pick Job.", 1, results2.Count);
			AssertColumnValues(data.Whs1.WW_WarehouseName, testStaff1.GS_Code, testStaff1.GS_FullName, "PIC", 100m, 1,
				results2[0]);
		}

		void CreateAndCompletePick(WhsWarehouse warehouse, OrgHeader org, string reference, OrgSupplierPart part,
			ZDecimal units, WhsLocation location, GlbStaff testStaff, ZDateTime pickedDate)
		{
			Helper.CreateWhsReceiveWithInventory(org, warehouse, reference, part, units, location, "", false, true);
			var order = Helper.CreateWhsOrder(org, warehouse, "O" + reference, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, part, units);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);

			var part1OrderdInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.Single(o => o.SupplierPart == part);
			var part1AvailableInventory = part1OrderdInventory.AvailableInventories.Cast<WhsPickAvailableInventory>()
				.Single(o => o.Location == location);
			part1AvailableInventory.Allocate = true;
			Helper.SetPickedDate(part1AvailableInventory, new ZDateTimeOffset(pickedDate));
			Helper.SetAssignToPk(part1AvailableInventory, testStaff.PK);
		}

		#endregion

		#region TestStaffProductivityReport_OutboundDockDoorTransfer

		public void TestStaffProductivityReport_OutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var results = LoadView(data.Whs1.PK, data.Org1.PK, new List<string>(), new List<string>(), "TRF",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("Should not return Outbound Dock Door Transfers.", 0, results.Count);
		}

		#endregion

		#region TestStaffProductivityReport_Transfer

		public void TestStaffProductivityReport_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var warehouse2 = Helper.CreateWarehouse("2", "B", 20, 1);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part1 = Helper.CreateProduct(org2, "P3");
			Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);
			Factory.Save();

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			var staffPKs1 = GetStaffPKList(testStaff1);
			var staffGroupPks1 = GetGroupPKList(testGroup1);

			var testGroup2 = GetGroup("GG2");
			var testStaff2 = GetStaff(testGroup2, "XX2", "X2");

			var testGroup3 = GetGroup("GG3");
			var testStaff3 = GetStaff(testGroup2, "XX3", "X3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m,
				data.Whs1.FindLocation("A-1"), "", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 100m, data.Whs1.FindLocation("A-1"),
				"", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, warehouse2, "R3", org2Part1, 100m,
				warehouse2.FindLocation("B-1"), "", false, true);

			GetTransferWithSubType(data.Org1, data.Part1, data.Whs1, null, testStaff1, "TR1", "A-1", "A-2",
				TransferType.Codes.Internal, ZDateTimeOffset.Today);
			GetTransferWithSubType(data.Org1, data.Part1, warehouse2, data.Whs1, testStaff1, "TR2", "A-1", "B-4",
				TransferType.Codes.InterWhsDest, ZDateTimeOffset.Today);
			GetTransferWithSubType(org2, data.Part1, data.Whs1, warehouse2, testStaff2, "TR3", "A-1", "A-3",
				TransferType.Codes.InterWhsSource, ZDateTimeOffset.Now.AddDays(-3), "B-2");
			GetTransferWithSubType(org2, org2Part1, warehouse2, data.Whs1, testStaff1, "TR4", "B-1", "B-3",
				TransferType.Codes.InterWhsSource, ZDateTimeOffset.Today, "A-4");
			GetTransferWithSubType(org2, org2Part1, warehouse2, data.Whs1, testStaff3, "TR5", "B-1", "B-4",
				TransferType.Codes.InterWhsSource, ZDateTimeOffset.Today, "A-5");
			Factory.Save();

			AssertResultForTransfer(data.Whs1, warehouse2, data.Org1, org2, testStaff1, testStaff2, testStaff3,
				staffPKs1, staffGroupPks1, "TRF", 10m);
		}

		WhsTransfer GetTransferWithSubType(OrgHeader org, OrgSupplierPart part, WhsWarehouse sourceWhs,
			WhsWarehouse destinationWhs, GlbStaff testStaff, string reference, string source, string destination,
			string docketSubType, ZDateTimeOffset transferDate, string interWhsSourceLocationString = "")
		{
			var transfer = Helper.CreateWhsTransfer(org, sourceWhs, reference, Notify);
			transfer.WD_DocketSubType = docketSubType;

			if (TransferType.Codes.Internal == docketSubType)
			{
				Helper.CreateWhsTransferLine(transfer, part, 10m, source, destination);
			}
			else if (TransferType.Codes.InterWhsDest == docketSubType)
			{
				Helper.CreateWhsTransferLine(transfer, part, 10m, "A-1", destinationWhs.PK, destination);
			}
			else if (TransferType.Codes.InterWhsSource == docketSubType)
			{
				Helper.CreateWhsTransferLine(transfer, part, 10m, source, destination);
				transfer.Lines[0].DestinationWarehousePK = destinationWhs.PK;
				transfer.Lines[0].LocationString = interWhsSourceLocationString;
			}

			var transferLine = transfer.Lines[0];
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);

			// hack to setup data we want for test
			using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
			{
				transferLine.WE_PutawayTime = transferDate;
			}

			transferLine.WE_GS_NKPutawayBy = testStaff.GS_Code;
			transferLine.WE_FinalisedDate = transferDate;

			var childTransferLine = transferLine.ChildTransferLine;
			if (childTransferLine != null)
			{
				using (new SemaphoreManager(transferLine.FinaliseDocketLineSemaphore))
				{
					childTransferLine.WE_PutawayTime = transferDate;
				}

				childTransferLine.WE_GS_NKPutawayBy = testStaff.GS_Code;
				childTransferLine.WE_FinalisedDate = transferDate;
			}

			return transfer;
		}

		void AssertResultForTransfer(WhsWarehouse warehouse1, WhsWarehouse warehouse2, OrgHeader org1, OrgHeader org2,
			GlbStaff staff1, GlbStaff staff2, GlbStaff staff3, List<string> staffPKs,
			List<string> staffGroupPks, string jobType, ZDecimal totalUnits)
		{
			var results1 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": All empty parameters, 4 records should return.", 4, results1.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results1[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results1[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results1[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results1[3]);

			var results2 = LoadView(warehouse1.PK, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With warehouse PK only, 2 record should return.", 2, results2.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results2[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results2[1]);

			var results3 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs, new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list only, 2 record should return.", 2, results3.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results3[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results3[1]);

			var results4 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), staffGroupPks, string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff Group List only, 2 record should return.", 2, results4.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results4[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results4[1]);

			var results5 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs, staffGroupPks, string.Empty, ZDateTimeOffset.Invalid,
				ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list, Group list only, 2 record should return.", 2, results5.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results5[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results5[1]);

			var results6 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string> { staff3.PK.ToString() }, staffGroupPks,
				string.Empty, ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list, Group list only, 3 record should return.", 3, results6.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results6[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results6[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results6[2]);

			var results7 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With From Date only, 4 records should return.", 4, results7.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results7[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results7[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results7[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results7[3]);

			var results8 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-2), ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With From Date only, 3 records should return.", 3, results8.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results8[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results8[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results8[2]);

			var results9 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Now.AddDays(-2));
			AssertEquals(jobType + ": With From Date only, 1 record should return.", 1, results9.Count);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results9[0]);

			var results10 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With To Date only, 4 records should return.", 4, results10.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results10[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results10[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results10[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results10[3]);

			var results11 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With specific From and To Date, 4 records should return.", 4, results11.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results11[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results11[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results11[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results11[3]);

			var results12 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Now.AddDays(-2));
			AssertEquals(jobType + ": With specific From and To Date, 1 record should return.", 1, results12.Count);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results12[0]);

			var results13 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-5), ZDateTimeOffset.Now.AddDays(-4));
			AssertEquals(jobType + ": With wrong From and To Date range, 0 record should return.", 0, results13.Count);

			var results14 = LoadView(warehouse1.PK, ZGuid.Empty, staffPKs, staffGroupPks, jobType,
				ZDateTimeOffset.Now.AddDays(-2), ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With all parameters, 1 record should return.", 1, results14.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits * 2,
				2, results14[0]);

			var results15 = LoadView(ZGuid.Empty, org1.PK, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Org PK only, 2 record should return.", 2, results3.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results15[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results15[1]);
		}

		public void TestStaffProductivityReport_Transfer_FactorsOffsetTime()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");

			var staffMember1 = Helper.CreateGlbStaff("S1", "S1");
			var staffMember2 = Helper.CreateGlbStaff("S2", "S2");

			var finalisedDateTimeOffsetSydney = new ZDateTimeOffset(2022, 06, 01, 06, 00, 00, TimeSpan.FromHours(10));
			var finalisedDateTimeOffsetUtc = new ZDateTimeOffset(2022, 06, 01, 06, 00, 00, TimeSpan.FromHours(0));

			GetTransferWithSubType(data.Org1, data.Part1, data.Whs1, null, staffMember1, "TR1", "A-1", "A-2", TransferType.Codes.Internal, finalisedDateTimeOffsetSydney);
			GetTransferWithSubType(data.Org1, data.Part1, data.Whs1, null, staffMember2, "TR2", "A-1", "A-2", TransferType.Codes.Internal, finalisedDateTimeOffsetUtc);

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				startDate: new ZDateTimeOffset(2022, 06, 01, 05, 00, 00, TimeSpan.FromHours(0)),
				endDate: ZDateTimeOffset.Invalid);

			AssertEquals("Single record should returned.", 1, results.Count);
			AssertColumnValues(data.Whs1.WW_WarehouseName, staffMember2.GS_Code, staffMember2.GS_FullName, "TRF", 10m, 1, results[0]);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				startDate: new ZDateTimeOffset(2022, 06, 01, 05, 00, 00, TimeSpan.FromHours(10)),
				endDate: ZDateTimeOffset.Invalid);

			AssertEquals("Both records should return.", 2, results.Count);
			AssertColumnValues(data.Whs1.WW_WarehouseName, staffMember1.GS_Code, staffMember1.GS_FullName, "TRF", 10m, 1, results[0]);
			AssertColumnValues(data.Whs1.WW_WarehouseName, staffMember2.GS_Code, staffMember2.GS_FullName, "TRF", 10m, 1, results[1]);
		}

		#endregion

		#region TestStaffProductivityReport_Stocktake

		public void TestStaffProductivityReport_Stocktake()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1);
			var warehouse2 = Helper.CreateWarehouse("2", "B", 20, 1);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part1 = Helper.CreateProduct(org2, "P3");
			Factory.Save();

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			var staffPKs1 = GetStaffPKList(testStaff1);
			var staffGroupPks1 = GetGroupPKList(testGroup1);

			var testGroup2 = GetGroup("GG2");
			var testStaff2 = GetStaff(testGroup2, "XX2", "X2");

			var testGroup3 = GetGroup("GG3");
			var testStaff3 = GetStaff(testGroup2, "XX3", "X3");

			CreateAndComplete3Count(data.Whs1, data.Org1, "RST1", data.Part1, 100m, data.Whs1.FindLocation("A-1"),
				testStaff1, testStaff2, 10m, 20m, 30m);
			CreateAndComplete3Count(warehouse2, org2, "RST2", org2Part1, 100m, warehouse2.FindLocation("B-1"),
				testStaff1, testStaff2, 10m, 20m, 30m);
			CreateAndComplete3Count(warehouse2, org2, "RST3", org2Part1, 100m, warehouse2.FindLocation("B-2"),
				testStaff3, testStaff3, 10m, 20m, 30m);
			Factory.Save();

			AssertResultStockTake(data.Whs1, warehouse2, data.Org1, org2, testStaff1, testStaff2, testStaff3, staffPKs1,
				staffGroupPks1, "CYC", 50m, 10m, 20m, 30m);
		}

		void CreateAndComplete3Count(WhsWarehouse warehouse, OrgHeader org, string reference, OrgSupplierPart part,
			ZDecimal units, WhsLocation location, GlbStaff testStaff1, GlbStaff testStaff2, ZDecimal count1,
			ZDecimal count2, ZDecimal count3)
		{
			Helper.CreateWhsReceiveWithInventory(org, warehouse, reference, part, units, location, "", false, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(org, warehouse);
			stocktake.Load();

			AssertEquals("Precondition", StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);
			var line = stocktake.Lines.Single(o => o.Location == location);
			line.WU_LastCount = count1;
			line.WU_DateVerified = ZDateTime.Today;
			line.WU_GS_NKVerifiedBy = testStaff2.GS_Code;

			line.WU_Count2DateVerified = ZDateTime.Now.AddDays(-3);
			line.WU_Count2VerifiedBy = testStaff1.GS_Code;
			line.WU_Count2 = count2;

			line.WU_Count3DateVerified = ZDateTime.Today;
			line.WU_Count3VerifiedBy = testStaff1.GS_Code;
			line.WU_Count3 = count3;
		}

		void AssertResultStockTake(WhsWarehouse warehouse1, WhsWarehouse warehouse2, OrgHeader org1, OrgHeader org2,
			GlbStaff staff1, GlbStaff staff2, GlbStaff staff3, List<string> staffPKs, List<string> staffGroupPks,
			string jobType, ZDecimal totalUnits1, ZDecimal totalUnits2, ZDecimal totalUnits3, ZDecimal totalUnits4)
		{
			var results1 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": All empty parameters, 5 records should return.", 5, results1.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results1[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results1[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results1[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results1[3]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType,
				totalUnits1 + totalUnits2, 3, results1[4]);

			var results2 = LoadView(warehouse1.PK, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With warehouse PK only, 2 record should return.", 2, results2.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results2[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results2[1]);

			var results3 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs, new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list only, 2 record should return.", 2, results3.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results3[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results3[1]);

			var results4 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), staffGroupPks, string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff Group List only, 2 record should return.", 2, results4.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results4[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results4[1]);

			var results5 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs, staffGroupPks, string.Empty, ZDateTimeOffset.Invalid,
				ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list, Group list only, 2 record should return.", 2, results5.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results5[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results5[1]);

			var results6 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>() { staff3.PK.ToString() },
				staffGroupPks, string.Empty, ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list, Group list only, 3 record should return.", 3, results6.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results6[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results6[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType,
				totalUnits1 + totalUnits2, 3, results6[2]);

			var results7 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With From Date only, 5 records should return.", 5, results7.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results7[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results7[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results7[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results7[3]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType,
				totalUnits1 + totalUnits2, 3, results7[4]);

			var results8 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With To Date only, 5 records should return.", 5, results8.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results8[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results8[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results8[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results8[3]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType,
				totalUnits1 + totalUnits2, 3, results8[4]);

			var results9 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With specific From and To Date, 5 records should return.", 5, results9.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results9[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results9[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results9[2]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results9[3]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType,
				totalUnits1 + totalUnits2, 3, results9[4]);

			var results10 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Now.AddDays(-2));
			AssertEquals(jobType + ": With specific From and To Date, 3 record should return.", 3, results10.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits3, 1,
				results10[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits3, 1,
				results10[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits3, 1,
				results10[2]);

			var results11 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-5), ZDateTimeOffset.Now.AddDays(-4));
			AssertEquals(jobType + ": With wrong From and To Date range, 0 record should return.", 0, results11.Count);

			var results12 = LoadView(warehouse1.PK, ZGuid.Empty, staffPKs, staffGroupPks, jobType,
				ZDateTimeOffset.Now.AddDays(-2), ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With all parameters, 1 record should return.", 1, results12.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits4, 1,
				results12[0]);

			var results13 = LoadView(ZGuid.Empty, org1.PK, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Org PK only, 2 record should return.", 2, results13.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits1, 2,
				results13[0]);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits2, 1,
				results13[1]);
		}

		#endregion

		#region TestStaffProductivityReport_AllJobType

		public void TestStaffProductivityReport_AllJobType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1, saveFactory_doNotUseForNewTests: false);
			var warehouse2 = Helper.CreateWarehouse("2", "B", 20, 1);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part1 = Helper.CreateProduct(org2, "P3");

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			var staffPKs1 = GetStaffPKList(testStaff1);
			var staffGroupPks1 = GetGroupPKList(testGroup1);

			var testGroup2 = GetGroup("GG2");
			var testStaff2 = GetStaff(testGroup2, "XX2", "X2");

			var testGroup3 = GetGroup("GG3");
			var testStaff3 = GetStaff(testGroup2, "XX3", "X3");

			#region UNL

			Factory.Save();
			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RU1", data.Part1, data.Whs1.FindLocation("A-1"),
				testStaff1, ZDateTime.Now, Events.AddedARecordToTheSystem.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RU2", org2Part1, warehouse2.FindLocation("B-1"), testStaff2,
				ZDateTime.Now.AddDays(-3), Events.AddedARecordToTheSystem.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RU3", org2Part1, warehouse2.FindLocation("B-2"), testStaff3,
				ZDateTime.Now, Events.AddedARecordToTheSystem.Code);

			#endregion UNL

			#region PUT

			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RP3", data.Part1, data.Whs1.FindLocation("A-2"),
				testStaff1, ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RP4", org2Part1, warehouse2.FindLocation("B-3"), testStaff2,
				ZDateTime.Now.AddDays(-3), Events.WarehouseReceiptConfirmedPutaway.Code);
			CreateAndAddEventToInventory(warehouse2, org2, "RP3", org2Part1, warehouse2.FindLocation("B-4"), testStaff3,
				ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);

			#endregion PUT

			#region Pick

			CreateAndCompletePick(data.Whs1, data.Org1, "RPC5", data.Part1, 100m, data.Whs1.FindLocation("A-3"),
				testStaff1, ZDateTime.Now);
			CreateAndCompletePick(warehouse2, org2, "RPC6", org2Part1, 100m, warehouse2.FindLocation("B-5"), testStaff2,
				ZDateTime.Now.AddDays(-3));
			CreateAndCompletePick(warehouse2, org2, "RPC3", org2Part1, 100m, warehouse2.FindLocation("B-6"), testStaff3,
				ZDateTime.Now);

			#endregion Pick

			#region TRF

			Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part1, 100m,
				data.Whs1.FindLocation("A-4"), "", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R8", data.Part1, 100m, data.Whs1.FindLocation("A-4"),
				"", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, warehouse2, "R9", org2Part1, 100m,
				warehouse2.FindLocation("B-7"), "", false, true);

			GetTransferWithSubType(data.Org1, data.Part1, data.Whs1, null, testStaff1, "TR1", "A-4", "A-5",
				TransferType.Codes.Internal, ZDateTimeOffset.Today);
			GetTransferWithSubType(data.Org1, data.Part1, warehouse2, data.Whs1, testStaff1, "TR2", "A-4", "B-4",
				TransferType.Codes.InterWhsDest, ZDateTimeOffset.Today);
			GetTransferWithSubType(org2, data.Part1, data.Whs1, warehouse2, testStaff2, "TR3", "A-4", "A-6",
				TransferType.Codes.InterWhsSource, ZDateTimeOffset.Now.AddDays(-3), "B-8");
			GetTransferWithSubType(org2, org2Part1, warehouse2, data.Whs1, testStaff1, "TR4", "B-7", "B-9",
				TransferType.Codes.InterWhsSource, ZDateTimeOffset.Today, "A-7");
			GetTransferWithSubType(org2, org2Part1, warehouse2, data.Whs1, testStaff3, "TR5", "B-7", "B-10",
				TransferType.Codes.InterWhsSource, ZDateTimeOffset.Today, "A-8");

			#endregion TRF

			#region StockTake

			CreateAndComplete3Count(data.Whs1, data.Org1, "RS9", data.Part1, 100m, data.Whs1.FindLocation("A-9"),
				testStaff1, testStaff2, 10m, 20m, 30m);
			CreateAndComplete3Count(warehouse2, org2, "RS10", org2Part1, 100m, warehouse2.FindLocation("B-11"),
				testStaff1, testStaff2, 10m, 20m, 30m);
			CreateAndComplete3Count(warehouse2, org2, "RST11", org2Part1, 100m, warehouse2.FindLocation("B-12"),
				testStaff3, testStaff3, 10m, 20m, 30m);

			#endregion StockTake

			Factory.Save();

			var results1 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("All Job: All empty parameters, 18 records should return.", 18, results1.Count);

			var results2 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "UNL",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("All Job: With UNL Job type, 3 records should return.", 3, results2.Count);

			var results3 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "PUT",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("All Job: With PUT Job type, 3 records should return.", 3, results3.Count);

			var results4 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "PIC",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("All Job: With PIC Job type, 3 records should return.", 3, results4.Count);

			var results5 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "TRF",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("All Job: With TRF Job type, 4 records should return.", 4, results5.Count);

			var results6 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "CYC",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("All Job: With CYC Job type, 5 records should return.", 5, results6.Count);
		}

		#endregion

		#region TestStaffProductivityReport_ResultsNotGroupedByClient

		public void TestStaffProductivityReport_ResultsNotGroupedByClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1, saveFactory_doNotUseForNewTests: false);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part = Helper.CreateProduct(org2, "P3");

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			Factory.Save();

			#region UNL

			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RU1", data.Part1, data.Whs1.FindLocation("A-1"),
				testStaff1, ZDateTime.Now, Events.AddedARecordToTheSystem.Code);
			CreateAndAddEventToInventory(data.Whs1, org2, "RU2", org2Part, data.Whs1.FindLocation("A-2"), testStaff1,
				ZDateTime.Now, Events.AddedARecordToTheSystem.Code);

			#endregion UNL

			#region PUT

			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RP3", data.Part1, data.Whs1.FindLocation("A-2"),
				testStaff1, ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);
			CreateAndAddEventToInventory(data.Whs1, org2, "RP4", org2Part, data.Whs1.FindLocation("A-3"), testStaff1,
				ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);

			#endregion PUT

			#region Pick

			CreateAndCompletePick(data.Whs1, data.Org1, "RPC5", data.Part1, 10m, data.Whs1.FindLocation("A-3"),
				testStaff1, ZDateTime.Now);
			CreateAndCompletePick(data.Whs1, org2, "RPC6", org2Part, 20m, data.Whs1.FindLocation("A-4"), testStaff1,
				ZDateTime.Now);

			#endregion Pick

			#region TRF

			Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part1, 100m,
				data.Whs1.FindLocation("A-4"), "", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R8", org2Part, 100m, data.Whs1.FindLocation("A-5"),
				"", false, true);

			GetTransferWithSubType(data.Org1, data.Part1, data.Whs1, null, testStaff1, "TR1", "A-4", "A-6",
				TransferType.Codes.Internal, ZDateTimeOffset.Today);
			GetTransferWithSubType(org2, org2Part, data.Whs1, null, testStaff1, "TR2", "A-5", "A-7",
				TransferType.Codes.Internal, ZDateTimeOffset.Today);

			#endregion TRF

			#region StockTake

			CreateAndComplete3Count(data.Whs1, data.Org1, "RS9", data.Part1, 100m, data.Whs1.FindLocation("A-9"),
				testStaff1, testStaff1, 10m, 20m, 30m);
			CreateAndComplete3Count(data.Whs1, org2, "RS10", org2Part, 100m, data.Whs1.FindLocation("A-9"), testStaff1,
				testStaff1, 10m, 20m, 30m);

			#endregion StockTake

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("All Job: All empty parameters, 5 records should return.", 5, results.Count);
		}

		#endregion

		#region TestStaffProductivityReport_ResultsNotGroupedByClient_UNL

		public void TestStaffProductivityReport_ResultsNotGroupedByClient_UNL()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1, saveFactory_doNotUseForNewTests: false);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part = Helper.CreateProduct(org2, "P3");

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			Factory.Save();

			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RU1", data.Part1, data.Whs1.FindLocation("A-1"),
				testStaff1, ZDateTime.Now, Events.AddedARecordToTheSystem.Code);
			CreateAndAddEventToInventory(data.Whs1, org2, "RU2", org2Part, data.Whs1.FindLocation("A-2"), testStaff1,
				ZDateTime.Now, Events.AddedARecordToTheSystem.Code);
			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "UNL",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("UNL Job type: 1 record should return.", 1, results.Count);
			AssertEquals("There are 2 total lines.", 2, results[0]["TotalLine"]);
			AssertEquals("There are 200 total units, sum of the total units from the 2 UNL jobs.", 200m,
				results[0]["TotalUnits"]);
		}

		#endregion

		#region TestStaffProductivityReport_ResultsNotGroupedByClient_PUT

		public void TestStaffProductivityReport_ResultsNotGroupedByClient_PUT()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1, saveFactory_doNotUseForNewTests: false);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part = Helper.CreateProduct(org2, "P3");

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			Factory.Save();

			CreateAndAddEventToInventory(data.Whs1, data.Org1, "RP3", data.Part1, data.Whs1.FindLocation("A-2"),
				testStaff1, ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);
			CreateAndAddEventToInventory(data.Whs1, org2, "RP4", org2Part, data.Whs1.FindLocation("A-3"), testStaff1,
				ZDateTime.Now, Events.WarehouseReceiptConfirmedPutaway.Code);

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "PUT",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("PUT Job type: 1 record should return.", 1, results.Count);
			AssertEquals("There are 2 total lines.", 2, results[0]["TotalLine"]);
			AssertEquals("There are 200 total units, sum of the total units from the 2 PUT jobs.", 200m,
				results[0]["TotalUnits"]);
		}

		#endregion

		#region TestStaffProductivityReport_ResultsNotGroupedByClient_PIC

		public void TestStaffProductivityReport_ResultsNotGroupedByClient_PIC()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1, saveFactory_doNotUseForNewTests: false);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part = Helper.CreateProduct(org2, "P3");

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			Factory.Save();

			CreateAndCompletePick(data.Whs1, data.Org1, "RPC5", data.Part1, 10m, data.Whs1.FindLocation("A-3"),
				testStaff1, ZDateTime.Now);
			CreateAndCompletePick(data.Whs1, org2, "RPC6", org2Part, 20m, data.Whs1.FindLocation("A-4"), testStaff1,
				ZDateTime.Now);

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "PIC",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("PIC Job type: 1 record should return.", 1, results.Count);
			AssertEquals("There are 2 total lines.", 2, results[0]["TotalLine"]);
			AssertEquals("There are 30 total units, sum of the total units from the 2 PIC jobs.", 30m,
				results[0]["TotalUnits"]);
		}

		#endregion

		#region TestStaffProductivityReport_ResultsNotGroupedByClient_TRF

		public void TestStaffProductivityReport_ResultsNotGroupedByClient_TRF()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1, saveFactory_doNotUseForNewTests: false);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part = Helper.CreateProduct(org2, "P3");

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			Factory.Save();

			Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R7", data.Part1, 100m,
				data.Whs1.FindLocation("A-4"), "", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R8", org2Part, 100m, data.Whs1.FindLocation("A-5"),
				"", false, true);

			GetTransferWithSubType(data.Org1, data.Part1, data.Whs1, null, testStaff1, "TR1", "A-4", "A-6",
				TransferType.Codes.Internal, ZDateTimeOffset.Today);
			GetTransferWithSubType(org2, org2Part, data.Whs1, null, testStaff1, "TR2", "A-5", "A-7",
				TransferType.Codes.Internal, ZDateTimeOffset.Today);

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "TRF",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("TRF Job type: 1 record should return.", 1, results.Count);
			AssertEquals("There are 2 total lines.", 2, results[0]["TotalLine"]);
			AssertEquals("There are 20m total units, sum of the total units from the 2 TRF jobs.", 20m,
				results[0]["TotalUnits"]);
		}

		#endregion

		#region TestStaffProductivityReport_ResultsNotGroupedByClient_CYC

		public void TestStaffProductivityReport_ResultsNotGroupedByClient_CYC()
		{
			var data = new TestDataSimpleEnvironment(Factory, 20, 1, saveFactory_doNotUseForNewTests: false);
			var org2 = Helper.CreateClient("2", "2");
			var org2Part = Helper.CreateProduct(org2, "P3");
			var org3Part = Helper.CreateProduct(org2, "P4");

			var testGroup1 = GetGroup("GG1");
			var testStaff1 = GetStaff(testGroup1, "XX1", "X1");
			Factory.Save();

			CreateAndComplete3Count(data.Whs1, data.Org1, "RS9", data.Part1, 100m, data.Whs1.FindLocation("A-9"),
				testStaff1, testStaff1, 10m, 20m, 30m);
			CreateAndComplete3Count(data.Whs1, org2, "RS10", org2Part, 100m, data.Whs1.FindLocation("A-9"), testStaff1,
				testStaff1, 10m, 20m, 30m);

			// creating stocktake with no client
			var location = data.Whs1.FindLocation("A-10");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "RS11", org3Part, 100, location, "", false, true);
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(null, data.Whs1);
			stocktake.Load();

			AssertEquals("Precondition", StocktakeStatus.Codes.Loaded, stocktake.WS_StocktakeStatus);
			var line = stocktake.Lines.Single(o => o.Location == location);
			line.WU_LastCount = 10m;
			line.WU_DateVerified = ZDateTime.Today;
			line.WU_GS_NKVerifiedBy = testStaff1.GS_Code;

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), "CYC",
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals("CYC Job type: 1 record should return.", 1, results.Count);
			AssertEquals(
				"There are 7 total lines, 3 for each stocktake task with client and 1 for stocktake task without client.",
				7, results[0]["TotalLine"]);
			AssertEquals("There are 130 total units, sum of the total units from the 3 CYC jobs.", 130m,
				results[0]["TotalUnits"]);
		}

		#endregion

		#region TestStaffProductivityReport_CircularReference

		public void TestStaffProductivityReport_CircularReference()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");

			// receive <= transfer1
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA1, locationA1);
			transfer1.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer1);

			// receive <= transfer1 <= transfer2
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, locationA1, locationA1);
			transfer2.FinaliseDocket();
			AssertIsFinalisedPrecondition(transfer2);

			// hack to setup circular reference
			// simulate transferLine1 having 12 UNT and only having pick lines for 10 UNT and the only available inventory is transferLine2
			// this is not possible to setup functionaly, but may have been done by one of the old transformations, like when we created pick lines for old transfers or fix imbalances
			transferLine1.WE_TransactionQuantity = 12m;
			transferLine1.WE_StockOnHand = 2m;
			var pickLine = Helper.CreateWhsPickLine(transferLine1, transferLine2.Inventory[0], 2m);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			Factory.Save();

			// add order for remainder of inventory (2 UNT from transferLine1 and 8 UNT from transferLine2)
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().Single(pl => pl.WZ_Units == 2m).WZ_PickedDateTime = ZDateTimeOffset.Today;
			pick.GetAllPickLines().Single(pl => pl.WZ_Units == 8m).WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition - only 10 UNT should be available to pick.", 10m,
				pick.OrderedInventories[0].PickLineQuantity);
			Factory.Save();

			var staff = GlbStaff.CurrentUser;
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Staff productivity report should be able to deal with circular references.",
					() =>
					{
						var results = LoadView(data.Whs1.PK, data.Org1.PK, new List<string>(), new List<string>(), "",
							ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
						AssertEquals("Number of results returned is incorrect.", 2, results.Count);
						AssertColumnValues(data.Whs1.WW_WarehouseName, staff.GS_Code, staff.GS_FullName, "TRF", 22m, 2,
							results.Single(row => (ZString)row["JobType"] == "TRF"));
						AssertColumnValues(data.Whs1.WW_WarehouseName, staff.GS_Code, staff.GS_FullName, "PIC", 10m, 2,
							results.Single(row => (ZString)row["JobType"] == "PIC"));
					});
			});
		}

		#endregion

		#region CreateInventoryLog

		static void CreateInventoryLog(string eventCode, GlbStaff testStaff, WhsDocketLine inventoryLine,
			ZDateTime eventTime)
		{
			var addEventLog = inventoryLine.Logs.AddNew(); //Adding event manually, It gets added only from RF
			AssertEquals("Event added successfully", inventoryLine.PK, addEventLog.SL_Parent);
			using (addEventLog.LockForUpdatingKeyFieldsForTesting())
			{
				addEventLog.SL_SE_NKEvent = eventCode;
				addEventLog.SL_GS_NKUser = testStaff.GS_Code;
				addEventLog.SL_EventTime = eventTime;
			}
		}

		#endregion

		#region AssertResult

		void AssertResult(WhsWarehouse warehouse1, WhsWarehouse warehouse2, OrgHeader org1, OrgHeader org2,
			GlbStaff staff1, GlbStaff staff2, GlbStaff staff3, List<string> staffPKs, List<string> staffGroupPks,
			string jobType, ZDecimal totalUnits)
		{
			var results1 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": All empty parameters, 3 records should return.", 3, results1.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results1[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results1[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results1[2]);

			var results2 = LoadView(warehouse1.PK, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With warehouse PK only, 1 record should return.", 1, results2.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results2[0]);

			var results3 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs, new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list only, 1 record should return.", 1, results3.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results3[0]);

			var results4 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), staffGroupPks, string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff Group List only, 1 record should return.", 1, results4.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results4[0]);

			var results5 = LoadView(ZGuid.Empty, ZGuid.Empty, staffPKs, staffGroupPks, string.Empty, ZDateTimeOffset.Invalid,
				ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list, Group list only, 1 record should return.", 1, results5.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results5[0]);

			var results6 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string> { staff3.PK.ToString() }, staffGroupPks,
				string.Empty, ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Staff list, Group list only, 2 record should return.", 2, results6.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results6[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results6[1]);

			var results7 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With From Date only, 3 records should return.", 3, results7.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results7[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results7[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results7[2]);

			var results8 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-2), ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With From Date only, 2 records should return.", 2, results8.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results8[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results8[1]);

			var results9 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With To Date only, 3 records should return.", 3, results9.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results9[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results9[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results9[2]);

			var results10 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Now.AddDays(-2));
			AssertEquals(jobType + ": With To Date only, 1 record should return.", 1, results10.Count);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results10[0]);

			var results11 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With specific From and To Date, 3 records should return.", 3, results11.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results11[0]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results11[1]);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff3.GS_Code, staff3.GS_FullName, jobType, totalUnits, 1,
				results11[2]);

			var results12 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-4), ZDateTimeOffset.Now.AddDays(-2));
			AssertEquals(jobType + ": With specific From and To Date, 1 record should return.", 1, results12.Count);
			AssertColumnValues(warehouse2.WW_WarehouseName, staff2.GS_Code, staff2.GS_FullName, jobType, totalUnits, 1,
				results12[0]);

			var results13 = LoadView(ZGuid.Empty, ZGuid.Empty, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Now.AddDays(-5), ZDateTimeOffset.Now.AddDays(-4));
			AssertEquals(jobType + ": With wrong From and To Date range, 0 record should return.", 0, results13.Count);

			var results14 = LoadView(warehouse1.PK, ZGuid.Empty, staffPKs, staffGroupPks, jobType,
				ZDateTimeOffset.Now.AddDays(-2), ZDateTimeOffset.Now.AddDays(2));
			AssertEquals(jobType + ": With all parameters, 1 record should return.", 1, results14.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results14[0]);

			var results15 = LoadView(ZGuid.Empty, org1.PK, new List<string>(), new List<string>(), string.Empty,
				ZDateTimeOffset.Invalid, ZDateTimeOffset.Invalid);
			AssertEquals(jobType + ": With Org PK only, 1 record should return.", 1, results15.Count);
			AssertColumnValues(warehouse1.WW_WarehouseName, staff1.GS_Code, staff1.GS_FullName, jobType, totalUnits, 1,
				results3[0]);
		}

		void AssertColumnValues(string warehouseName, string staffCode, string fullName, string jobType,
			ZDecimal totalUnits, ZInt totalLines, DynamicBusinessObject resultRow)
		{
			AssertEquals(jobType + ": Warehouse does not match with expected value.", warehouseName,
				resultRow["WarehouseName"]);
			AssertEquals(jobType + ": StaffCode does not match with expected value.", staffCode,
				resultRow["StaffCode"]);
			AssertEquals(jobType + ": FullName does not match with expected value.", fullName, resultRow["FullName"]);
			AssertEquals(jobType + ": JobType does not match with expected value.", jobType, resultRow["JobType"]);
			AssertEquals(jobType + ": TotalUnits does not match with expected value.", totalUnits,
				resultRow["TotalUnits"]);
			AssertEquals(jobType + ": TotalLine does not match with expected value.", totalLines,
				resultRow["TotalLine"]);
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(ZGuid whsPK, ZGuid clientPK, List<string> staffCode,
			List<string> staffGroup, string jobType, ZDateTimeOffset startDate, ZDateTimeOffset endDate)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var statements = new List<string>();

			var sql = @"SELECT * FROM Report_WhsStaffProductivityReport";
			CreateParameterList(whsPK, clientPK, staffCode, staffGroup, jobType, startDate, endDate, statements);
			sql += "(" + String.Join(",", statements) + ") order by WarehouseName, JobType, StaffCode";
			result.Load(sql);

			return result;
		}

		#endregion

		#region CreateParameterList

		void CreateParameterList(ZGuid whsPK, ZGuid clientPK, List<string> staffCode, List<string> staffGroup,
			string jobType, ZDateTimeOffset startDate, ZDateTimeOffset endDate, List<string> statements)
		{
			if (whsPK != ZGuid.Empty)
			{
				statements.Add("'" + whsPK.ToString() + "'");
			}
			else
			{
				statements.Add("null");
			}

			if (clientPK != ZGuid.Empty)
			{
				statements.Add("'" + clientPK.ToString() + "'");
			}
			else
			{
				statements.Add(" null");
			}

			if (startDate != ZDateTimeOffset.Invalid)
			{
				statements.Add(" '" + startDate.ToString("yyyy-MM-dd hh:mm:ss zzz") + "'");
			}
			else
			{
				statements.Add(" null");
			}

			if (endDate != ZDateTimeOffset.Invalid)
			{
				statements.Add(" '" + endDate.ToString("yyyy-MM-dd hh:mm:ss zzz") + "'");
			}
			else
			{
				statements.Add(" null");
			}

			if (!String.IsNullOrEmpty(jobType))
			{
				statements.Add(" '" + jobType + "'");
			}
			else
			{
				statements.Add(" ''");
			}

			if (staffCode.Count > 0)
			{
				statements.Add(" '" + String.Join("','", staffCode) + "'");
			}
			else
			{
				statements.Add(" ''");
			}

			if (staffGroup.Count > 0)
			{
				statements.Add(" '" + String.Join("','", staffGroup) + "'");
			}
			else
			{
				statements.Add(" ''");
			}
		}

		#endregion

		#region Get Receive, Group & Staff

		List<string> GetGroupPKList(GlbGroup testGroup1)
		{
			var staffGroupPks1 = new List<string>();
			staffGroupPks1.Add(testGroup1.PK.ToString());
			return staffGroupPks1;
		}

		List<string> GetStaffPKList(GlbStaff testStaff1)
		{
			var staffPKs1 = new List<string>();
			staffPKs1.Add(testStaff1.PK.ToString());
			return staffPKs1;
		}

		GlbGroup GetGroup(string groupName)
		{
			var testGroup = Factory.New<GlbGroup>();
			testGroup.GG_Code = groupName;
			testGroup.GG_IsActive = true;

			return testGroup;
		}

		GlbStaff GetStaff(GlbGroup testGroup, string staffName, string staffCode)
		{
			var testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_Code = staffCode;
			testStaff.GS_FullName = staffName;

			var testLink = Factory.New<GlbGroupLink>();
			testLink.GK_GS = testStaff.PK;
			testLink.GK_GG = testGroup.PK;

			return testStaff;
		}

		#endregion
	}
}

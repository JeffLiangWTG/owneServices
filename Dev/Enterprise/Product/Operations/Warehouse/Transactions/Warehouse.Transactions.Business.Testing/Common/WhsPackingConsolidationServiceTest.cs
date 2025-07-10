using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackingConsolidationServiceTest : WhsTestCaseWithFactory
	{
		#region CreateDockDoorTransfer

		public void TestCreateDockDoorTransfer_RandomGuid()
		{
			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit not found or is already Closed.", service.CreateDockDoorTransfer(ZGuid.NewZGuid()));
		}

		[TestDate(2024, 01, 16, 13, 33, 09)]
		public void TestCreateDockDoorTransfer()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var transferLine = pick.Transfers[0].Lines[0];
			var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

			transferLine.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine.WE_WL);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.CreateDockDoorTransfer(handlingUnitPackage1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer.Lines.Count);

			var newTransferLine = transfer.Lines.Single(l => l.PK != transferLine.PK);
			AssertEquals(nameof(newTransferLine.WE_TransactionQuantity), 10m, newTransferLine.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLine.WE_OP), data.Part1.PK, newTransferLine.WE_OP);
			AssertEquals(nameof(newTransferLine.WE_StockOnHand), 10m, newTransferLine.WE_StockOnHand);
			AssertEquals(nameof(newTransferLine.WE_WL), dockDoorLocation.PK, newTransferLine.WE_WL);

			AssertEquals(1, newTransferLine.PickLines.Count);
			var newPickLine = newTransferLine.PickLines[0];
			AssertEquals(nameof(newPickLine.WZ_WE_TransactionLine), newTransferLine.PK, newPickLine.WZ_WE_TransactionLine);
			AssertEquals(nameof(newPickLine.WZ_WE_InventoryLine), transferLine.PK, newPickLine.WZ_WE_InventoryLine);
			AssertEquals(nameof(newPickLine.WZ_Units), 10m, newPickLine.WZ_Units);
			AssertEquals(nameof(newPickLine.WZ_F3_NKAllocatedPackType), transferLine.PickLines[0].WZ_F3_NKAllocatedPackType, newPickLine.WZ_F3_NKAllocatedPackType);

			AssertEquals(nameof(handlingUnitPackage1.KP_IsClosed), true, handlingUnitPackage1.KP_IsClosed);
			AssertEquals(nameof(handlingUnitPackage1.KP_ClosedTimeUtc), new ZDateTime(2024, 01, 16, 13, 33, 09), handlingUnitPackage1.KP_ClosedTimeUtc);
			AssertEquals(nameof(handlingUnitPackage1.KP_GS_NKClosedBy), GlbStaff.CurrentUser.GS_Code, handlingUnitPackage1.KP_GS_NKClosedBy);
		}

		[TestDate(2024, 01, 16, 13, 33, 09)]
		public void TestCreateDockDoorTransfer_DivotHasUnpackedRecord()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			var pickLine1 = pick1.GetAllPickLines().Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();
			AssertEquals("Precondition: Dock door transfer created for pick1.", 1, pick1.Transfers.Count);
			AssertEquals("Precondition: Dock door transfer created for pick1.", 1, pick1.Transfers[0].Lines.Count);

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var transferLine = pick.Transfers[0].Lines[0];
			var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

			transferLine.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine.WE_WL);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, pickLine1);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);

			// Pack order1 then unpack it.
			var divot = packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg2, handlingUnitPackage1);
			divot.KPD_UnpackedTime = ZDateTimeOffset.Now;
			divot.KPD_GS_NKUnpackedUser = Env.CurrentUser.Initials;
			pkg2.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
			Factory.Save();

			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.CreateDockDoorTransfer(handlingUnitPackage1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer.Lines.Count);

			var newTransferLine = transfer.Lines.Single(l => l.PK != transferLine.PK);
			AssertEquals(nameof(newTransferLine.WE_TransactionQuantity), 10m, newTransferLine.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLine.WE_OP), data.Part1.PK, newTransferLine.WE_OP);
			AssertEquals(nameof(newTransferLine.WE_StockOnHand), 10m, newTransferLine.WE_StockOnHand);
			AssertEquals(nameof(newTransferLine.WE_WL), dockDoorLocation.PK, newTransferLine.WE_WL);

			AssertEquals(1, newTransferLine.PickLines.Count);
			var newPickLine = newTransferLine.PickLines[0];
			AssertEquals(nameof(newPickLine.WZ_WE_TransactionLine), newTransferLine.PK, newPickLine.WZ_WE_TransactionLine);
			AssertEquals(nameof(newPickLine.WZ_WE_InventoryLine), transferLine.PK, newPickLine.WZ_WE_InventoryLine);
			AssertEquals(nameof(newPickLine.WZ_Units), 10m, newPickLine.WZ_Units);
			AssertEquals(nameof(newPickLine.WZ_F3_NKAllocatedPackType), transferLine.PickLines[0].WZ_F3_NKAllocatedPackType, newPickLine.WZ_F3_NKAllocatedPackType);

			AssertEquals(nameof(handlingUnitPackage1.KP_IsClosed), true, handlingUnitPackage1.KP_IsClosed);
			AssertEquals(nameof(handlingUnitPackage1.KP_ClosedTimeUtc), new ZDateTime(2024, 01, 16, 13, 33, 09), handlingUnitPackage1.KP_ClosedTimeUtc);
			AssertEquals(nameof(handlingUnitPackage1.KP_GS_NKClosedBy), GlbStaff.CurrentUser.GS_Code, handlingUnitPackage1.KP_GS_NKClosedBy);

			AssertEquals("Dock door transfer remains the same for pick1.", 1, pick1.Transfers[0].Lines.Count);
		}

		public void TestCreateDockDoorTransfer_HUIsClosed()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			handlingUnitPackage1.KP_IsClosed = true;
			handlingUnitPackage1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage1.KP_GS_NKClosedBy = "AAA";
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit not found or is already Closed.", service.CreateDockDoorTransfer(handlingUnitPackage1.PK));
		}

		public void TestCreateDockDoorTransfer_HandlingUnitIsInWrongLocation()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var transferLine = pick.Transfers[0].Lines[0];
			var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit not found or is in the wrong Location.", service.CreateDockDoorTransfer(handlingUnitPackage1.PK));
			AssertEquals(nameof(handlingUnitPackage1.KP_IsClosed), false, handlingUnitPackage1.KP_IsClosed);
		}

		public void TestCreateDockDoorTransfer_IsAPackage()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var transferLine = pick.Transfers[0].Lines[0];
			var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

			transferLine.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine.WE_WL);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit not found or is in the wrong Location.", service.CreateDockDoorTransfer(pkg1.PK));
			AssertEquals(nameof(pkg1.KP_IsClosed), false, pkg1.KP_IsClosed);
		}

		public void TestCreateDockDoorTransfer_IsLoadHU()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);
			Factory.Save();

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;
			Helper.CreateLoadPkgPackagePivot(handlingUnitPackage1.PK, load);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit is not a Consolidation Handling Unit or is already loaded.", service.CreateDockDoorTransfer(handlingUnitPackage1.PK));
			AssertEquals(nameof(handlingUnitPackage1.KP_IsClosed), false, handlingUnitPackage1.KP_IsClosed);
		}

		public void TestCreateDockDoorTransfer_IsConsolidationHUButAlreadyLoaded()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var transferLine = pick.Transfers[0].Lines[0];
			var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

			transferLine.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine.WE_WL);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var conHandlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			Factory.Save();

			var loadHandlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var loadHandlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(loadHandlingUnit);
			var loadHandlingUnitPackage = packingHelper.CreatePackage(loadHandlingUnitPackageJob, "HU2", 1, PkgUnit.Package);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;
			packingHelper.PackHandlingUnit(conHandlingUnitPackage, pkg1, loadHandlingUnitPackage);
			packingHelper.PackHandlingUnit(loadHandlingUnitPackage, conHandlingUnitPackage, loadHandlingUnitPackage);
			Helper.CreateLoadPkgPackagePivot(loadHandlingUnitPackage.PK, load);
			Helper.CreateLoadPkgPackagePivot(conHandlingUnitPackage.PK, load);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit is not a Consolidation Handling Unit or is already loaded.", service.CreateDockDoorTransfer(conHandlingUnitPackage.PK));
			AssertEquals(nameof(conHandlingUnitPackage.KP_IsClosed), false, conHandlingUnitPackage.KP_IsClosed);
		}

		public void TestCreateDockDoorTransfer_PickLineAlreadyPicked()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.Lines[0].WE_WL = conLocation.PK;
			AssertEquals("Precondition: Receive Line Location.", conLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Some Pick Lines on this Handling Unit have been picked already or is not picking from a consolidation location.", service.CreateDockDoorTransfer(handlingUnitPackage1.PK));
			AssertEquals(nameof(handlingUnitPackage1.KP_IsClosed), false, handlingUnitPackage1.KP_IsClosed);
		}

		[TestDate(2024, 01, 16, 13, 33, 09)]
		public void TestCreateDockDoorTransfer_DBHits()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");

			var size = 10;
			for (var i = 0; i < size; i++)
			{
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", data.Part1, 10m);
			}
			Factory.Save();

			var orders = new WhsOrder[size];
			for (var i = 0; i < size; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i}", data.Part1, 10m);
				order.WD_UseDirectedPackingConsolidation = true;
				orders[i] = order;
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			}
			Factory.Save();

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);

			var transferLines = new WhsTransferLine[size];
			for (var i = 0; i < size; i++)
			{
				var pick = orders[i].Pick;
				var transfer = pick.Transfers[0];
				AssertNotNull("Precondition: Transfer Created.", transfer);
				var transferLine = pick.Transfers[0].Lines[0];
				transferLines[i] = transferLine;
				var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
				AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

				transferLine.WE_WL = conLocation.PK;
				AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine.WE_WL);

				var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(orders[i]);
				var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
				packingHelper.CreatePackageDivot(pkg1, pick.GetAllPickLines().Single());
				packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			}
			Factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgHandlingUnitSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 3 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 3 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPackageLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};
			var service = new WhsPackingConsolidationService();

			using (AssertDbHitsForAllFactories(expectedHits))
			{
				var errorMessage = service.CreateDockDoorTransfer(handlingUnitPackage1.PK);
				AssertEquals(true, errorMessage.IsNullOrEmpty());
			}

			for (var i = 0; i < size; i++)
			{
				var transferLine = transferLines[i];
				var transfer = transferLine.Docket;
				AssertEquals("New Transfer Line to Dock Door created.", 2, transfer.Lines.Count);

				var newTransferLine = transfer.Lines.Single(l => l.PK != transferLine.PK);
				AssertEquals(nameof(newTransferLine.WE_TransactionQuantity), 10m, newTransferLine.WE_TransactionQuantity);
				AssertEquals(nameof(newTransferLine.WE_OP), data.Part1.PK, newTransferLine.WE_OP);
				AssertEquals(nameof(newTransferLine.WE_StockOnHand), 10m, newTransferLine.WE_StockOnHand);
				AssertEquals(nameof(newTransferLine.WE_WL), dockDoorLocation.PK, newTransferLine.WE_WL);

				AssertEquals(1, newTransferLine.PickLines.Count);
				var newPickLine = newTransferLine.PickLines[0];
				AssertEquals(nameof(newPickLine.WZ_WE_TransactionLine), newTransferLine.PK, newPickLine.WZ_WE_TransactionLine);
				AssertEquals(nameof(newPickLine.WZ_WE_InventoryLine), transferLine.PK, newPickLine.WZ_WE_InventoryLine);
				AssertEquals(nameof(newPickLine.WZ_Units), 10m, newPickLine.WZ_Units);
				AssertEquals(nameof(newPickLine.WZ_F3_NKAllocatedPackType), transferLine.PickLines[0].WZ_F3_NKAllocatedPackType, newPickLine.WZ_F3_NKAllocatedPackType);

				AssertEquals(nameof(handlingUnitPackage1.KP_IsClosed), true, handlingUnitPackage1.KP_IsClosed);
				AssertEquals(nameof(handlingUnitPackage1.KP_ClosedTimeUtc), new ZDateTime(2024, 01, 16, 13, 33, 09), handlingUnitPackage1.KP_ClosedTimeUtc);
				AssertEquals(nameof(handlingUnitPackage1.KP_GS_NKClosedBy), GlbStaff.CurrentUser.GS_Code, handlingUnitPackage1.KP_GS_NKClosedBy);
			}
		}

		#endregion

		#region PutawayStockInDockDoor

		public void TestPutawayStockInDockDoor_InvalidWarehousePK()
		{
			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(ZGuid.NewZGuid(), ZGuid.NewZGuid(), ZGuid.NewZGuid());

			AssertEquals("Warehouse not found.", errorMessage);
		}

		public void TestPutawayStockInDockDoor_LocationNotExists()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(ZGuid.NewZGuid(), ZGuid.NewZGuid(), data.Whs1.PK);

			AssertEquals("Location not found.", errorMessage);
		}

		public void TestPutawayStockInDockDoor_LocationIsNotADockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			var pick1 = Helper.CreatePickNew(order1);
			var pickLine1 = pick1.GetAllPickLines().Single();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(pkg1.PK, data.Whs1.FindLocation("A-1").PK, data.Whs1.PK);

			AssertEquals("Location A-1 is not an Outbound Dock Door Location.", errorMessage);
		}

		public void TestPutawayStockInDockDoor_HasPlannedLoad_PassedInAnotherLocation()
		{
			var (data, order1, order2, transfer1, transfer2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck);
			order1.WD_WLO_PlannedLoad = load.PK;
			order2.WD_WLO_PlannedLoad = load.PK;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.FindLocation("A-2").PK, data.Whs1.PK);

			AssertEquals("You need to Putaway to the Planned Load configured Dock Door Location.", errorMessage);
		}

		public void TestPutawayStockInDockDoor_HasPlannedLoad_PassedInTheSameLocation()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Weight.Kilograms, 1m, Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck);
			order1.WD_WLO_PlannedLoad = load.PK;
			order2.WD_WLO_PlannedLoad = load.PK;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine1.WE_WL);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", order1.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
			AssertEquals("Order2 Pick DDL", order2.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
		}

		public void TestPutawayStockInDockDoor_WithoutPlannedLoadAndHaveSameDockDoorLocationOnPick_PassedInThatLocation()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			order1.Pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			order2.Pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine1.WE_WL);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", order1.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
			AssertEquals("Order2 Pick DDL", order2.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
		}

		public void TestPutawayStockInDockDoor_WithoutPlannedLoadAndHaveSameDockDoorLocationOnPick_PassedInAnotherLocation()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			order1.Pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			order2.Pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;

			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WLT_LocationType = ddlLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, location.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals("Location overridden.", location.PK, newTransferLine1.WE_WL);
			AssertEquals("Location overridden.", location.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", order1.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, location.PK);
			AssertEquals("Order2 Pick DDL", order2.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, location.PK);
		}

		public void TestPutawayStockInDockDoor_WithoutPlannedLoadAndHaveDifferentDockDoorLocationOnPick_PassInTheFirstLocation()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WLT_LocationType = ddlLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			newTransferLine1.WE_WL = location.PK;
			order1.Pick.WP_WL_DockDoor = location.PK;

			newTransferLine2.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			order2.Pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, location.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals("Location overridden.", location.PK, newTransferLine1.WE_WL);
			AssertEquals("Location overridden.", location.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", order1.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, location.PK);
			AssertEquals("Order2 Pick DDL", order2.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, location.PK);
		}

		public void TestPutawayStockInDockDoor_WithoutPlannedLoadAndHaveDifferentDockDoorLocationOnPick_PassInTheSecondLocation()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WLT_LocationType = ddlLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			newTransferLine1.WE_WL = location.PK;
			order1.Pick.WP_WL_DockDoor = location.PK;

			newTransferLine2.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			order2.Pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals("Location overridden.", data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine1.WE_WL);
			AssertEquals("Location overridden.", data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", order1.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
			AssertEquals("Order2 Pick DDL", order2.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
		}

		public void TestPutawayStockInDockDoor_WithoutPlannedLoadAndHaveDifferentDockDoorLocationOnPick_PassInAnotherLocation()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);
			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var location1 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = ddlLocationType.PK;
			var location2 = data.Whs1.FindLocation("B-1");
			location2.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			newTransferLine1.WE_WL = location1.PK;
			order1.Pick.WP_WL_DockDoor = location1.PK;

			newTransferLine2.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			order2.Pick.WP_WL_DockDoor = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, location2.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals("Location overridden.", location2.PK, newTransferLine1.WE_WL);
			AssertEquals("Location overridden.", location2.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", order1.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, location2.PK);
			AssertEquals("Order2 Pick DDL", order2.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, location2.PK);
		}

		public void TestPutawayStockInDockDoor_WithoutPlannedLoadAndHaveDifferentDockDoorLocationOnPick_DDA()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var location1 = data.Whs1.FindLocation("A-2");
			location1.WLV_WLT_LocationType = ddlLocationType.PK;
			var location2 = data.Whs1.FindLocation("B-1");
			location2.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			newTransferLine1.WE_WL = location1.PK;
			newTransferLine2.WE_WL = location1.PK;
			var dda = Helper.CreateWhsDockDoorAssignment(location1, order1.Pick, order2.Pick);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, location2.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals("Location overridden.", location2.PK, newTransferLine1.WE_WL);
			AssertEquals("Location overridden.", location2.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", ZGuid.Empty, order1.Pick.WP_WL_DockDoor);
			AssertEquals("Order2 Pick DDL", ZGuid.Empty, order2.Pick.WP_WL_DockDoor);
			AssertEquals("DDA DDL", location2.PK, dda.WDA_WL_AssignedDockDoor);
		}

		public void TestPutawayStockInDockDoor_DifferentDockDoor_OverrideNotAllowed()
		{
			var (data, order1, order2, transfer1, transfer2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			var ddlLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "DOC"));
			var location = data.Whs1.FindLocation("A-2");
			location.WLV_WLT_LocationType = ddlLocationType.PK;
			Factory.Save();

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, location.PK, data.Whs1.PK);

			AssertEquals($"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides", errorMessage);
		}

		public void TestPutawayStockInDockDoor_HandlingUnitIsInWrongLocation()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			receive.Lines[0].WE_WL = conLocation.PK;
			AssertEquals("Precondition: Receive Line Location.", conLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, pkg1, handlingUnitPackage);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit not found or is in the wrong Location.", service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK));
		}

		public void TestPutawayStockInDockDoor_PassedInAPackagePK()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit not found or is in the wrong Location.", service.PutawayStockInDockDoor(order1.PackageJob.Packages[0].PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK));
		}

		public void TestPutawayStockInDockDoor_PassedInALoadHU()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			Factory.Save();

			var loadHandlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var loadHandlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(loadHandlingUnit);
			var loadHandlingUnitPackage = packingHelper.CreatePackage(loadHandlingUnitPackageJob, "HU2", 1, PkgUnit.Package);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order1.WD_WLO_PlannedLoad = load.PK;
			Helper.CreateLoadPkgPackagePivot(loadHandlingUnitPackage.PK, load);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Handling Unit is not a Consolidation Handling Unit or is already loaded.", service.PutawayStockInDockDoor(loadHandlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK));
		}

		public void TestPutawayStockInDockDoor_ConsolidationHUIsAlreadyLoaded()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer1 = pick1.Transfers[0];
			var transfer2 = pick2.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer1);
			AssertNotNull("Precondition: Transfer Created.", transfer2);
			var transferLine1 = pick1.Transfers[0].Lines[0];
			var transferPickLine1 = pick1.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine1.WE_WL);
			var transferLine2 = pick2.Transfers[0].Lines[0];
			var transferPickLine2 = pick2.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine2.WE_WL);

			transferLine1.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine1.WE_WL);
			transferLine2.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine2.WE_WL);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, pickLine2);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg2, handlingUnitPackage1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage1 = service.CreateDockDoorTransfer(handlingUnitPackage1.PK);
			AssertEquals(true, errorMessage1.IsNullOrEmpty());
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer1.Lines.Count);
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer2.Lines.Count);
			var newTransferLine1 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.PK != transferLine1.PK);
			AssertEquals("Precondition: newTransferLine1 is not Finalised.", false, newTransferLine1.IsFinalised);
			var newTransferLine2 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.PK != transferLine2.PK);
			AssertEquals("Precondition: newTransferLine1 is not Finalised.", false, newTransferLine2.IsFinalised);

			var loadHandlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var loadHandlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(loadHandlingUnit);
			var loadHandlingUnitPackage = packingHelper.CreatePackage(loadHandlingUnitPackageJob, "HU2", 1, PkgUnit.Package);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order1.WD_WLO_PlannedLoad = load.PK;
			order2.WD_WLO_PlannedLoad = load.PK;
			packingHelper.PackHandlingUnit(loadHandlingUnitPackage, handlingUnitPackage1, loadHandlingUnitPackage);
			Helper.CreateLoadPkgPackagePivot(loadHandlingUnitPackage.PK, load);
			Helper.CreateLoadPkgPackagePivot(handlingUnitPackage1.PK, load);
			pkg1.KP_KP_TopHandlingUnitPackage = loadHandlingUnitPackage.PK;
			pkg2.KP_KP_TopHandlingUnitPackage = loadHandlingUnitPackage.PK;
			Factory.Save();

			AssertEquals("Handling Unit is not a Consolidation Handling Unit or is already loaded.", service.PutawayStockInDockDoor(loadHandlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK));
		}

		public void TestPutawayStockInDockDoor_OneTransferLineHasBeenAssignedToAnotherUser()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();

			var anotherStaff = Helper.CreateGlbStaff("LOL", "LOL");
			newTransferLine1.WE_GS_NKPutawayBy = anotherStaff.GS_Code;
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Wrong status of Handling Unit. Either is In-Transit by another staff, or Put has already been completed.", service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK));
		}

		public void TestPutawayStockInDockDoor_OneTransferLineHasBeenFinalised()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();
			newTransferLine1.FinaliseDocketLine();
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			AssertEquals("Wrong status of Handling Unit. Either is In-Transit by another staff, or Put has already been completed.", service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK));
		}

		public void TestPutawayStockInDockDoor()
		{
			var (data, order1, order2, newTransferLine1, newTransferLine2, dockdoorLocation, handlingUnitPackage) = PrepareTestDate();

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine1.WE_WL);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine2.WE_WL);
			AssertEquals("Order1 Pick DDL", order1.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
			AssertEquals("Order2 Pick DDL", order2.Pick.DockDoorAssignment.WDA_WL_AssignedDockDoor, data.Whs1.DefaultOutboundDockDoorLocation.PK);
		}

		public void TestPutawayStockInDockDoor_DivotHasUnpackedRecord()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			var order0 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O0", data.Part1, 10m);
			var pick0 = Helper.CreatePickNew(order0);
			var pickLine0 = pick0.GetAllPickLines().Single();
			pickLine0.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine0.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var transfer0 = pick0.Transfers[0];
			var transfer1 = pick1.Transfers[0];
			var transfer2 = pick2.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer0);
			AssertNotNull("Precondition: Transfer Created.", transfer1);
			AssertNotNull("Precondition: Transfer Created.", transfer2);
			var transferLine1 = pick1.Transfers[0].Lines[0];
			var transferPickLine1 = pick1.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine1.WE_WL);
			var transferLine2 = pick2.Transfers[0].Lines[0];
			var transferPickLine2 = pick2.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine2.WE_WL);

			transferLine1.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine1.WE_WL);
			transferLine2.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine2.WE_WL);

			var pkgJob0 = PkgPackageJob.LoadOrCreatePackageJob(order0);
			var pkg0 = packingHelper.CreatePackage(pkgJob0, "PKG0", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg0, pickLine0);
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, pickLine2);

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);

			// Pack order0 then unpack it.
			var divot = packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg0, handlingUnitPackage1);
			divot.KPD_UnpackedTime = ZDateTimeOffset.Now;
			divot.KPD_GS_NKUnpackedUser = Env.CurrentUser.Initials;
			pkg0.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;
			Factory.Save();

			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg2, handlingUnitPackage1);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage1 = service.CreateDockDoorTransfer(handlingUnitPackage1.PK);
			AssertEquals(true, errorMessage1.IsNullOrEmpty());
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer1.Lines.Count);
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer2.Lines.Count);
			var newTransferLine1 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.PK != transferLine1.PK);
			AssertEquals("Precondition: newTransferLine1 is not Finalised.", false, newTransferLine1.IsFinalised);
			var newTransferLine2 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.PK != transferLine2.PK);
			AssertEquals("Precondition: newTransferLine1 is not Finalised.", false, newTransferLine2.IsFinalised);

			var service2 = new WhsPackingConsolidationService();
			var errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK);

			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, newTransferLine1.IsFinalised);
			AssertEquals(true, newTransferLine2.IsFinalised);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine1.WE_WL);
			AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, newTransferLine2.WE_WL);

			AssertEquals("Dock door transfer remains the same for pick0.", 1, pick0.Transfers[0].Lines.Count);
			AssertEquals("The unpacked Line remains unfinalised.", false, pick0.Transfers[0].Lines[0].IsFinalised);
		}

		(TestDataSimpleEnvironment, WhsOrder, WhsOrder, WhsTransferLine, WhsTransferLine, WhsLocation, PkgPackage) PrepareTestDate()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			AssertEquals("Precondition: Receive Line Location.", firstLocation.PK, receive.Lines[0].WE_WL);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine1.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var transfer1 = pick1.Transfers[0];
			var transfer2 = pick2.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer1);
			AssertNotNull("Precondition: Transfer Created.", transfer2);
			var transferLine1 = pick1.Transfers[0].Lines[0];
			var transferPickLine1 = pick1.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine1.WE_WL);
			var transferLine2 = pick2.Transfers[0].Lines[0];
			var transferPickLine2 = pick2.Transfers[0].Lines[0].PickLines[0];
			AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine2.WE_WL);

			transferLine1.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine1.WE_WL);
			transferLine2.WE_WL = conLocation.PK;
			AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine2.WE_WL);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, pickLine2);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, pkg1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, pkg2, handlingUnitPackage);
			Factory.Save();

			var service = new WhsPackingConsolidationService();
			var errorMessage1 = service.CreateDockDoorTransfer(handlingUnitPackage.PK);
			AssertEquals(true, errorMessage1.IsNullOrEmpty());
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer1.Lines.Count);
			AssertEquals("New Transfer Line to Dock Door created.", 2, transfer2.Lines.Count);
			var newTransferLine1 = transfer1.Lines.Cast<WhsTransferLine>().Single(l => l.PK != transferLine1.PK);
			AssertEquals("Precondition: newTransferLine1 is not Finalised.", false, newTransferLine1.IsFinalised);
			var newTransferLine2 = transfer2.Lines.Cast<WhsTransferLine>().Single(l => l.PK != transferLine2.PK);
			AssertEquals("Precondition: newTransferLine1 is not Finalised.", false, newTransferLine2.IsFinalised);

			return (data, order1, order2, newTransferLine1, newTransferLine2, dockDoorLocation, handlingUnitPackage);
		}

		public void TestPutawayStockInDockDoor_DBHits()
		{
			var size = 10;
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var conLocationType = Factory.NewWithValidTestData<WhsLocationType>();
			conLocationType.WLT_LocationClass = LocationClasses.Codes.CON;
			var firstLocation = data.Whs1.FindLocation("A-1");
			var conLocation = data.Whs1.FindLocation("A-3");
			conLocation.WLV_WLT_LocationType = conLocationType.PK;
			var dockDoorLocation = data.Whs1.FindLocation("DOCKDOOR");

			for (var i = 0; i < size; i++)
			{
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", data.Part1, 10m);
			}
			Factory.Save();

			var orders = new WhsOrder[size];
			for (var i = 0; i < size; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i}", data.Part1, 10m);
				order.WD_UseDirectedPackingConsolidation = true;
				orders[i] = order;
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			}
			Factory.Save();

			var handlingUnit1 = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob1 = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit1);
			var handlingUnitPackage1 = packingHelper.CreatePackage(handlingUnitPackageJob1, "HU1", 1, PkgUnit.Package);

			var transferLines = new WhsTransferLine[size];
			for (var i = 0; i < size; i++)
			{
				var pick = orders[i].Pick;
				var transfer = pick.Transfers[0];
				AssertNotNull("Precondition: Transfer Created.", transfer);
				var transferLine = pick.Transfers[0].Lines[0];
				transferLines[i] = transferLine;
				var transferPickLine = pick.Transfers[0].Lines[0].PickLines[0];
				AssertEquals("Precondition: Transfer to Dock Door.", dockDoorLocation.PK, transferLine.WE_WL);

				transferLine.WE_WL = conLocation.PK;
				AssertEquals("Precondition: Changed to Transfer to CON Location.", conLocation.PK, transferLine.WE_WL);

				var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(orders[i]);
				var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
				packingHelper.CreatePackageDivot(pkg1, pick.GetAllPickLines().Single());
				packingHelper.PackHandlingUnit(handlingUnitPackage1, pkg1, handlingUnitPackage1);
			}
			Factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPackageLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
			};

			var service = new WhsPackingConsolidationService();
			var errorMessage = service.CreateDockDoorTransfer(handlingUnitPackage1.PK);
			AssertEquals(true, errorMessage.IsNullOrEmpty());
			AssertEquals(true, handlingUnitPackage1.IsClosed);

			using (AssertDbHitsForAllFactories(expectedHits))
			{
				errorMessage = service.PutawayStockInDockDoor(handlingUnitPackage1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, data.Whs1.PK);
				AssertEquals(true, errorMessage.IsNullOrEmpty());
			}

			for (var i = 0; i < size; i++)
			{
				var transferLine = transferLines[i];
				var transfer = transferLine.Docket;
				var ddlTransferLine = transfer.Lines.Single(l => l.PK != transferLine.PK);

				AssertEquals(true, ddlTransferLine.IsFinalised);
				AssertEquals(data.Whs1.DefaultOutboundDockDoorLocation.PK, ddlTransferLine.WE_WL);
			}
		}

		#endregion

		#region GenerateHandlingUnit

		public void TestGenerateHandlingUnit()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var load = Factory.NewWithValidTestData<WhsLoad>();
			load.WLO_StartTime = DateTimeOffset.Now;
			load.WLO_TransportationUnitNumber = "ABC456";
			load.WLO_WL_PlannedDockDoor = warehouse.WW_DefaultOutboundDockDoor;
			Factory.Save();
			var handlingUnitPK = ZGuid.NewZGuid();

			// Act
			var service = new WhsPackingConsolidationService();
			var errorMessage = service.GenerateHandlingUnit(handlingUnitPK, warehouse.PK, load.PK);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			AssertNotNull(newFactory.Load<PkgHandlingUnit>(handlingUnitPK));
			var packageJob = QueryHandlingUnitPackageJob(newFactory, handlingUnitPK);
			AssertNotNull(packageJob);
			var package = QueryHandlingUnitPackage(newFactory, packageJob.PK);
			AssertNotNull(package);
			var packageHeader = QueryHandlingUnitPackageHeader(newFactory, package.KP_KPH_PackageHeader);
			AssertNotNull(packageHeader);
			var loadPivot = QueryHandlingUnitLoadPkgPackagePivot(newFactory, package.PK, load.PK);
			AssertNotNull(loadPivot);
		}

		public void TestGenerateHandlingUnit_WarehouseNotFound()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var load = Factory.NewWithValidTestData<WhsLoad>();
			load.WLO_StartTime = DateTimeOffset.Now;
			load.WLO_TransportationUnitNumber = "ABC456";
			load.WLO_WL_PlannedDockDoor = warehouse.WW_DefaultOutboundDockDoor;
			Factory.Save();
			var handlingUnitPK = ZGuid.NewZGuid();

			// Act
			var service = new WhsPackingConsolidationService();
			var errorMessage = service.GenerateHandlingUnit(handlingUnitPK, ZGuid.NewZGuid(), load.PK); // Wrong warehouse ID

			// Assert
			AssertEquals("Should have an error message.", "Warehouse not found.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<PkgHandlingUnit>(handlingUnitPK));
		}

		public void TestGenerateHandlingUnit_HandlingUnitAlreadyExists()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var load = Factory.NewWithValidTestData<WhsLoad>();
			load.WLO_StartTime = DateTimeOffset.Now;
			load.WLO_TransportationUnitNumber = "ABC456";
			load.WLO_WL_PlannedDockDoor = warehouse.WW_DefaultOutboundDockDoor;

			var handlingUnit = Factory.NewWithValidTestData<PkgHandlingUnit>();
			Factory.Save();

			// Act
			var service = new WhsPackingConsolidationService();
			var errorMessage = service.GenerateHandlingUnit(handlingUnit.PK, warehouse.PK, load.PK);

			// Assert
			AssertEquals("Should have an error message.", "Handling Unit already exists.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			AssertNotNull(newFactory.Load<PkgHandlingUnit>(handlingUnit.PK));
		}

		public void TestGenerateHandlingUnit_LoadNotFound()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var loadPK = ZGuid.NewZGuid();
			Factory.Save();
			var handlingUnitPK = ZGuid.NewZGuid();

			// Act
			var service = new WhsPackingConsolidationService();
			var errorMessage = service.GenerateHandlingUnit(handlingUnitPK, warehouse.PK, loadPK);

			// Assert
			AssertEquals("Should have an error message.", "Load not found.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<PkgHandlingUnit>(handlingUnitPK));
		}

		public void TestGenerateHandlingUnit_LoadPKIsNull() => TestGenerateHandlingUnit_LoadPKIsNullOrEmptyCore(null);

		public void TestGenerateHandlingUnit_LoadPKIsEmpty() => TestGenerateHandlingUnit_LoadPKIsNullOrEmptyCore(ZGuid.Empty);

		void TestGenerateHandlingUnit_LoadPKIsNullOrEmptyCore(ZGuid? loadPK)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			Factory.Save();
			var handlingUnitPK = ZGuid.NewZGuid();

			// Act
			var service = new WhsPackingConsolidationService();
			var errorMessage = service.GenerateHandlingUnit(handlingUnitPK, warehouse.PK, loadPK);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			AssertNotNull(newFactory.Load<PkgHandlingUnit>(handlingUnitPK));
			var packageJob = QueryHandlingUnitPackageJob(newFactory, handlingUnitPK);
			AssertNotNull(packageJob);
			var package = QueryHandlingUnitPackage(newFactory, packageJob.PK);
			AssertNotNull(package);
			var packageHeader = QueryHandlingUnitPackageHeader(newFactory, package.KP_KPH_PackageHeader);
			AssertNotNull(packageHeader);
			var loadPivot = QueryHandlingUnitLoadPkgPackagePivot(newFactory, package.PK);
			AssertNull("Should NOT create a load pivot", loadPivot);
		}

		PkgPackageJob QueryHandlingUnitPackageJob(BusinessObjectFactory factory, ZGuid handlingUnitPK)
		{
			var packageJobQuery = new ZQuery();
			packageJobQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentID, handlingUnitPK);
			packageJobQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, "KPU");
			return factory.LoadTop1<PkgPackageJob>(packageJobQuery);
		}

		PkgPackage QueryHandlingUnitPackage(BusinessObjectFactory factory, ZGuid packageJobPK)
		{
			var packageQuery = new ZQuery();
			packageQuery.AddToFilter(PkgPackageSchema.KP_F3_NKPackType, "PKG");
			packageQuery.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobPK);
			return factory.LoadTop1<PkgPackage>(packageQuery);
		}

		PkgPackageHeader QueryHandlingUnitPackageHeader(BusinessObjectFactory factory, ZGuid packageHeaderPK)
		{
			return factory.Load<PkgPackageHeader>(packageHeaderPK);
		}

		WhsLoadPkgPackagePivot QueryHandlingUnitLoadPkgPackagePivot(BusinessObjectFactory factory, ZGuid packagePK, ZGuid? loadPK = null)
		{
			var loadPivotQuery = new ZQuery();
			loadPivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, packagePK);
			if (loadPK != null)
			{
				loadPivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_WLO_Load, loadPK);
			}
			return factory.LoadTop1<WhsLoadPkgPackagePivot>(loadPivotQuery);
		}

		#endregion

		#region TestPutawayTransferLines

		public void TestPutawayTransferLines_SetLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLine = transfer.Lines.Single();
			AssertEquals("Precondition: in-transit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var errorMessage = WhsPackingConsolidationService.PutawayTransferLines(dockdoorLocation, transfer.Lines.Cast<WhsTransferLine>());
			AssertEquals("", errorMessage);
			AssertEquals(dockdoorLocation.PK, transferLine.Location.PK);
		}

		public void TestPutawayTransferLines_DoesNotSetLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location1);
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 2m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var pickLine1 = pick.GetAllPickLines().Single(l => l.WZ_Units == 1m);
			var pickLine2 = pick.GetAllPickLines().Single(l => l.WZ_Units == 2m);
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLine1 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_TransactionQuantity == 1m);
			var transferLine2 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			transferLine1.WE_WL = location2.PK;
			AssertEquals("Precondition: in-transit.", InventoryStatus.Codes.InTransit, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: in-transit.", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);

			var transferLinesToIgnoreWhenSettingLocation = new HashSet<WhsTransferLine>();
			transferLinesToIgnoreWhenSettingLocation.Add(transferLine1);
			var errorMessage = WhsPackingConsolidationService.PutawayTransferLines(dockdoorLocation, transfer.Lines.Cast<WhsTransferLine>(), transferLinesToIgnoreWhenSettingLocation);
			AssertEquals("", errorMessage);
			AssertEquals("Location was not set to dock door.", location2.PK, transferLine1.Location.PK);
			AssertEquals("Location was set to dock door.", dockdoorLocation.PK, transferLine2.Location.PK);
		}

		public void TestPutawayTransferLines_BOM_MandatoryPartAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			Helper.SetClientAllAttributeType(data.Org1, mandatoryAttributeType: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location1, "PID456", ZDate.Today.AddDays(20), ZDate.Today, "PA1", "PA2", "PA3", string.Empty);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var pickLine = order.Lines.Single(l => l.WE_OP == data.Part2.PK).PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var transfer = Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, pick.PK)).Single();
			var transferLine = (WhsTransferLine)transfer.Lines.Single();
			transferLine.WE_WL = location2.PK;
			AssertEquals("Precondition: in-transit.", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			var errorMessage = WhsPackingConsolidationService.PutawayTransferLines(dockdoorLocation, transfer.Lines.Cast<WhsTransferLine>(), new HashSet<WhsTransferLine>());
			AssertEquals("Should have no error", string.Empty, errorMessage);
			AssertEquals("Location was set to dock door.", dockdoorLocation.PK, transferLine.Location.PK);
		}

		#endregion
	}
}

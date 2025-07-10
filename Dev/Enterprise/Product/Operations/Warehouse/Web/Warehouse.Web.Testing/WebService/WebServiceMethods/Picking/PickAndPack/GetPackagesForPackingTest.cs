using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetPackagesForPackingTest : WhsSecureServiceTestCase
	{
		#region TestGetPackagesForPacking_ToteIDIsEmpty

		public void TestGetPackagesForPacking_ToteIDIsEmpty()
		{
			var webService = GetNewWebService();
			var response = webService.GetPackagesForPacking(null);
			AssertEquals("ToteInfo should be empty", 0, response.PackagesForPackingInfo.Count);
			AssertEquals("Should Error as no Tote ID given.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Please provide a Tote Number or Package ID.", response.ErrorMessage);

			var webService2 = GetNewWebService();
			var response2 = webService2.GetPackagesForPacking("");
			AssertEquals("ToteInfo should be empty", 0, response2.PackagesForPackingInfo.Count);
			AssertEquals("Should Error as no Tote ID given.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Please provide a Tote Number or Package ID.", response2.ErrorMessage);
		}

		#endregion

		#region TestGetPackagesForPacking_NoPackageMatched

		public void TestGetPackagesForPacking_NoPackageMatched()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pickedDate = ZDateTimeOffset.Now;
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Count());
			pickLines.ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_PickedDateTime = pickedDate;
				l.WZ_GS_NKAssignedTo = "E";
			});

			AssertEquals("Precondtion: Order should have 1 release line", 1, orderLine.ReleaseLines.Count);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "TOTE02";
			package1.SetIsTote(true);
			package1.Pack(orderLine.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "TOTE03";
			package2.SetIsTote(true);
			package2.Pack(orderLine.ReleaseLines[0], 5m);

			Helper.Factory.Save();

			AssertEquals("Precondition: Package1 is Tote", true, package1.GetIsTote());
			AssertEquals("Precondition: Package2 is Tote", true, package2.GetIsTote());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("TOTE01");
			AssertEquals("ToteInfo should  be empty", 0, response.PackagesForPackingInfo.Count);
			AssertEquals("Should Error as no Tote given.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Tote 'TOTE01' cannot be found or is not valid for Packing.", response.ErrorMessage);
		}

		#endregion

		#region TestGetPackagesForPacking_PickIsFinalised

		public void TestGetPackagesForPacking_PickIsFinalised()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var printer = Helper.CreatePrintQueue("PrintQueue", "TestPrinter");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pickedDate = ZDateTimeOffset.Now;
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			pickLines.ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_PickedDateTime = pickedDate;
				l.WZ_GS_NKAssignedTo = "E";
			});

			AssertEquals("Precondtion: Order should have 1 release line", 1, orderLine.ReleaseLines.Count);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "TOTE02";
			package2.Pack(orderLine.ReleaseLines[0], 5m);

			pick.FinaliseAllOrders();
			pick.WP_IsCartonised = true;
			pick.FinalisePickFailure += Pick_FinalisePickFailure;
			pick.FinalisePick();

			Assert("Should not finalise pick", !pick.IsFinalised);

			void Pick_FinalisePickFailure(object sender, WhsPick.FinalisePickEventArgs e)
			{
				AssertEquals("Cannot Finalize this pick as the following Orders have not finished Packing Totes:\r\nO1", e.ErrorMessage);
			}

			Helper.Factory.Save();

			AssertEquals("Precondition: Pick should not be finalise when there is a tote", false, pick.IsFinalised);
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition: Package2 is not Tote", false, package2.GetIsTote());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("TOTE01");
			AssertEquals("Response should return 1 package.", 1, response.PackagesForPackingInfo.Count);
			AssertEquals("1st package is selected.", package.PK, response.PackagesForPackingInfo[0].PK);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.GetPackagesForPacking("TOTE02");
			AssertEquals("Response should return 1 package.", 1, response2.PackagesForPackingInfo.Count);
			AssertEquals("1st package is selected.", package2.PK, response2.PackagesForPackingInfo[0].PK);

			var packageInfo = response.PackagesForPackingInfo[0];
			var responseClose = webService.ClosePackageForPacking(packageInfo, printer.PK.ToGuid());
			var toteInfo = responseClose.PackageForPackingInfo;
			AssertNotNull("PackageInfo should not be null", toteInfo);
			AssertEquals("Should have no error as Tote is valid.", ErrorTypes.None, response.Error);

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);

			pickInNewFactory.FinalisePickFailure += (s, e) => Fail("This event should not be called when there is no error in finalise pick.");
			pickInNewFactory.FinalisePick(); // The tote has been closed, valid to finalise Pick

			newFactory.Save();
			AssertIsFinalisedPrecondition(pickInNewFactory);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response3 = webService3.GetPackagesForPacking("TOTE01");
			AssertEquals("ToteInfo should  be empty.", 0, response3.PackagesForPackingInfo.Count);
			AssertEquals("Should Error as no Tote given.", ErrorTypes.BusinessValidationError, response3.Error);
			AssertEquals("Tote 'TOTE01' cannot be found or is not valid for Packing.", response3.ErrorMessage);

			var webService4 = GetNewWebService();
			webService4.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response4 = webService4.GetPackagesForPacking("TOTE02");
			AssertEquals("ToteInfo should  be empty.", 0, response4.PackagesForPackingInfo.Count);
			AssertEquals("Should Error as no Tote given.", ErrorTypes.BusinessValidationError, response4.Error);
			AssertEquals("Tote 'TOTE02' cannot be found or is not valid for Packing.", response4.ErrorMessage);
		}

		#endregion

		#region TestGetPackagesForPacking_HasItemNotPicked

		public void TestGetPackagesForPacking_HasItemNotPicked()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pickedDate = ZDateTimeOffset.Now;
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;

			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Count());
			pickLines.ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pickLine1 = pickLines.Single(pl => pl.WZ_Units == 4m);
			pickLine1.WZ_PickedDateTime = pickedDate;
			pickLine1.WZ_GS_NKAssignedTo = "E";

			AssertEquals("Precondition: Order should have 1 release line", 1, orderLine.ReleaseLines.Count);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TOTE01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "TOTE02";
			package2.Pack(orderLine.ReleaseLines[0], 5m);

			Helper.Factory.Save();

			AssertEquals("Precondition: Has pick line not picked", true, pickLines.Any(p => !p.IsPicked));
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition: Package2 is not Tote", false, package2.GetIsTote());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("TOTE01");
			AssertEquals("ToteInfo should  be empty.", 0, response.PackagesForPackingInfo.Count);
			AssertEquals("Should Error as no Tote given.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Tote 'TOTE01' cannot be found or is not valid for Packing.", response.ErrorMessage);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.GetPackagesForPacking("TOTE02");
			AssertEquals("ToteInfo should  be empty.", 0, response2.PackagesForPackingInfo.Count);
			AssertEquals("Should Error as no Tote given.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Tote 'TOTE02' cannot be found or is not valid for Packing.", response2.ErrorMessage);
		}

		#endregion

		#region TestGetPackagesForPacking_DifferentWarehouses

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestGetPackagesForPacking_DifferentWarehouses()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			var orgMiscServ = data.Org1.MiscServ;
			orgMiscServ.OM_WhsPackageWeightTolerancePercent = 10m;
			orgMiscServ.OM_WhsPackageToleranceEnabled = true;
			orgMiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote = true;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, whs2, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			Helper.Factory.Save();

			var pickedDate = ZDateTimeOffset.Now;
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			pickLines.ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_PickedDateTime = pickedDate;
				l.WZ_GS_NKAssignedTo = "E";
			});
			Helper.Factory.Save();

			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_CartoniseSplitCases = true;
			var pickLines2 = pick2.GetAllPickLines();
			pickLines2.ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_PickedDateTime = pickedDate;
				l.WZ_GS_NKAssignedTo = "E";
			});
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderLine.ReleaseLines[0], 5m);
			package.KP_PackageID = "P1";
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_Weight = 5m;
			package.KP_Length = 1m;
			package.KP_Width = 2m;
			package.KP_Height = 3m;
			package.KP_DimensionUQ = Constants.Length.Centimetres;

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.Pack(orderLine2.ReleaseLines[0], 5m);
			package2.KP_PackageID = "P1";
			package2.KP_WeightUQ = Constants.Weight.Pounds;
			package2.KP_Weight = 10m;
			package2.KP_Length = 5m;
			package2.KP_Width = 6m;
			package2.KP_Height = 7m;
			package2.KP_DimensionUQ = Constants.Length.Inches;
			package2.SetIsTote(true);

			Helper.Factory.Save();

			AssertEquals("Precondition: Pickline ispicked", true, pickLines.Any(p => !p.IsPicked));
			AssertEquals("Precondition: Pickline2 ispicked", true, pickLines2.Any(p => !p.IsPicked));
			AssertEquals("Precondition: Package is not Tote", false, package.GetIsTote());
			AssertEquals("Precondition: Package2 is Tote", true, package2.GetIsTote());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("P1");
			var packageInfo = response.PackagesForPackingInfo;
			AssertPackageInfo(packageInfo[0], package.PK.ToGuid(), "O1", "W00000003", "P1",
				5m, 0m, "KG",
				10m, true,
				1m, 2m, 3m, "CM",
				order.WD_DocketStatus, order.WD_RequiredDate.ToDateTime(), order.PackageJob.KJ_JobID, order.Client.OH_Code,
				false,
				true,
				true,
				expectedIsConsolidationHandlingUnit: false,
				expectedIsUsingDirectedPackingConsolidation: false,
				expectedPackageRequiresPutaway: true,
				data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = whs2.WW_WarehouseCode;
			var response2 = webService2.GetPackagesForPacking("P1");
			var packageInfo2 = response2.PackagesForPackingInfo;
			AssertPackageInfo(packageInfo2[0], package2.PK.ToGuid(), "O2", "W00000004", "P1",
				10m, 0m, "LB",
				10m, true,
				5m, 6m, 7m, "IN",
				order2.WD_DocketStatus, order2.WD_RequiredDate.ToDateTime(), order2.PackageJob.KJ_JobID, order2.Client.OH_Code,
				true,
				true,
				true,
				expectedIsConsolidationHandlingUnit: false,
				expectedIsUsingDirectedPackingConsolidation: false,
				expectedPackageRequiresPutaway: true,
				whs2.DefaultOutboundDockDoorLocation.WLV_LocationString, whs2.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);
		}

		#endregion

		#region TestGetPackagesForPacking

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestGetPackagesForPacking()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var orgMiscServ = data.Org1.MiscServ;
			orgMiscServ.OM_WhsPackageWeightTolerancePercent = 10m;
			orgMiscServ.OM_WhsPackageToleranceEnabled = true;
			orgMiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote = true;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pickedDate = ZDateTimeOffset.Now;
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Count());

			pickLines.ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_PickedDateTime = pickedDate;
				l.WZ_GS_NKAssignedTo = "E";
			});

			AssertEquals("Precondtion: Order should have 1 release line", 1, orderLine.ReleaseLines.Count);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			package.KP_TareWeight = 1m;
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_DimensionUQ = Constants.Length.Metres;

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "TOTE02";
			package2.Pack(orderLine.ReleaseLines[0], 5m);
			package2.KP_TareWeight = 2m;
			package2.KP_Weight = 10m;
			package2.KP_WeightUQ = Constants.Weight.Pounds;
			package2.KP_Length = 5m;
			package2.KP_Width = 6m;
			package2.KP_Height = 7m;
			package2.KP_DimensionUQ = Constants.Length.Inches;

			Helper.Factory.Save();

			AssertEquals("Precondition: All pick lines have been picked", false, pickLines.Any(p => !p.IsPickedFromPutawayLocation));
			AssertEquals("Precondition: Package is Tote", true, package.GetIsTote());
			AssertEquals("Precondition: Package2 is not Tote", false, package2.GetIsTote());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("PKG01");
			var packageInfo = response.PackagesForPackingInfo;
			AssertPackageInfo(packageInfo[0], package.PK.ToGuid(), "O1", "W00000003", "PKG01",
				10m, 1m, "KG",
				10m, true,
				0m, 0m, 0m, "M",
				order.WD_DocketStatus, order.WD_RequiredDate.ToDateTime(), order.PackageJob.KJ_JobID, order.Client.OH_Code,
				true,
				true,
				true,
				expectedIsConsolidationHandlingUnit: false,
				expectedIsUsingDirectedPackingConsolidation: true,
				expectedPackageRequiresPutaway: true,
				data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response2 = webService2.GetPackagesForPacking("TOTE02");
			var packageInfo2 = response2.PackagesForPackingInfo;
			AssertPackageInfo(packageInfo2[0], package2.PK.ToGuid(), "O1", "W00000003", "TOTE02",
				10m, 2m, "LB",
				10m, true,
				5m, 6m, 7m, "IN",
				order.WD_DocketStatus, order.WD_RequiredDate.ToDateTime(), order.PackageJob.KJ_JobID, order.Client.OH_Code,
				false,
				true,
				true,
				expectedIsConsolidationHandlingUnit: false,
				expectedIsUsingDirectedPackingConsolidation: true,
				expectedPackageRequiresPutaway: true,
				data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);
		}

		public void TestGetPackagesForPacking_PackageInPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine1.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 6m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesForPacking("PKG1");
			var packageInfo = response.PackagesForPackingInfo.Single();
			AssertEquals(true, packageInfo.PackageRequiresPutaway);
		}

		public void TestGetPackagesForPacking_PackageInConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = consolidationLocation.PK;
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesForPacking("PKG1");
			var packageInfo = response.PackagesForPackingInfo.Single();
			AssertEquals(false, packageInfo.PackageRequiresPutaway);
		}

		#endregion

		#region TestGetPackagesForPacking_HandlingUnitForConsolidation

		public void TestGetPackagesForPacking_HandlingUnitForConsolidation()
		{
			using (WarehouseDataRegistry.Instance.CheckAndroidDeviceVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

				var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
				var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
				packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				Helper.Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				Helper.CreatePickNew(order);

				var pickLine = order.Lines[0].PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;

				transferLine.FinaliseDocketLine();
				Helper.Factory.Save();

				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

				var packingHelper = new PackingTestHelper(Helper.Factory);
				var package = packingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
				package.Pack(order.Lines[0].ReleaseLines[0], 10m);

				var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
				handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
				handlingUnit.KPU_JobContext = "3PL";

				var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
				var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

				packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

				handlingUnitPackage.KP_IsClosed = true;
				handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				handlingUnitPackage.KP_TareWeight = 3m;
				handlingUnitPackage.KP_Weight = 10m;
				handlingUnitPackage.KP_WeightUQ = Constants.Weight.Pounds;
				handlingUnitPackage.KP_Length = 5m;
				handlingUnitPackage.KP_Width = 6m;
				handlingUnitPackage.KP_Height = 7m;
				handlingUnitPackage.KP_DimensionUQ = Constants.Length.Inches;

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.IsAndroidDevice = true;
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

				var response = webService.GetPackagesForPacking("HU");
				var packageInfo = response.PackagesForPackingInfo;
				AssertPackageInfo(packageInfo[0], handlingUnitPackage.PK.ToGuid(), "", "", "HU",
					10m, 3m, "LB",
					0m, false,
					5m, 6m, 7m, "IN",
					string.Empty, DateTime.MinValue, string.Empty, string.Empty,
					false, false, false,
					expectedIsConsolidationHandlingUnit: true,
					expectedIsUsingDirectedPackingConsolidation: false,
					expectedPackageRequiresPutaway: false,
					string.Empty, string.Empty);
			}
		}

		public void TestGetPackagesForPacking_HandlingUnitForConsolidation_SupercedesLoosePackages()
		{
			using (WarehouseDataRegistry.Instance.CheckAndroidDeviceVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

				var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
				var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
				packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				Helper.Factory.Save();

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				Helper.CreatePickNew(order1);

				var pickLine1 = order1.Lines[0].PickLines.Single();
				var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
				transferLine1.WE_WL = packingLocation.PK;

				transferLine1.FinaliseDocketLine();
				Helper.Factory.Save();

				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order1);

				var packingHelper = new PackingTestHelper(Helper.Factory);
				var package = packingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
				package.Pack(order1.Lines[0].ReleaseLines[0], 10m);

				var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
				handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
				handlingUnit.KPU_JobContext = "3PL";

				var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
				var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

				packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

				handlingUnitPackage.KP_IsClosed = true;
				handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				handlingUnitPackage.KP_TareWeight = 3m;
				handlingUnitPackage.KP_Weight = 10m;
				handlingUnitPackage.KP_WeightUQ = Constants.Weight.Pounds;
				handlingUnitPackage.KP_Length = 5m;
				handlingUnitPackage.KP_Width = 6m;
				handlingUnitPackage.KP_Height = 7m;
				handlingUnitPackage.KP_DimensionUQ = Constants.Length.Inches;

				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
				var pick2 = Helper.CreatePickNew(order2);
				var pickLine2 = order2.Lines[0].PickLines.Single();
				var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				transferLine2.FinaliseDocketLine();

				var package2 = order2.PackageJob.Packages.AddNew("PKG", "HU");
				packingHelper.CreatePackageDivot(package2, pickLine2);

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.IsAndroidDevice = true;
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

				var response = webService.GetPackagesForPacking("HU");
				var packageInfo = response.PackagesForPackingInfo;
				AssertEquals("Only Handling Units should be returned if one exists.", 1, packageInfo.Count);
				AssertPackageInfo(packageInfo[0], handlingUnitPackage.PK.ToGuid(), "", "", "HU",
					10m, 3m, "LB",
					0m, false,
					5m, 6m, 7m, "IN",
					string.Empty, DateTime.MinValue, string.Empty, string.Empty,
					false, false, false,
					expectedIsConsolidationHandlingUnit: true,
					expectedIsUsingDirectedPackingConsolidation: false,
					expectedPackageRequiresPutaway: false,
					string.Empty, string.Empty);
			}
		}

		public void TestGetPackagesForPacking_HandlingUnitForConsolidation_SameCarrierLabel()
		{
			using (WarehouseDataRegistry.Instance.CheckAndroidDeviceVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

				var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
				var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
				packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				Helper.Factory.Save();

				var carrierBookingAgent1 = Helper.CreateClient("CBA1");

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				order1.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent1.PK;

				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
				order2.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent1.PK;

				Helper.CreatePickNew(order1, order2);

				var pickLine1 = order1.Lines[0].PickLines.Single();
				var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
				transferLine1.WE_WL = packingLocation.PK;

				var pickLine2 = order2.Lines[0].PickLines.Single();
				var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				transferLine2.WE_WL = packingLocation.PK;

				transferLine1.FinaliseDocketLine();
				transferLine2.FinaliseDocketLine();
				Helper.Factory.Save();

				var packingHelper = new PackingTestHelper(Helper.Factory);

				var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
				var package1 = packingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
				package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

				var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
				var package2 = packingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
				package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

				var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
				handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
				handlingUnit.KPU_JobContext = "3PL";

				var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
				var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

				packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
				packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

				handlingUnitPackage.KP_IsClosed = true;
				handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				handlingUnitPackage.KP_TareWeight = 3m;
				handlingUnitPackage.KP_Weight = 10m;
				handlingUnitPackage.KP_WeightUQ = Constants.Weight.Pounds;
				handlingUnitPackage.KP_Length = 5m;
				handlingUnitPackage.KP_Width = 6m;
				handlingUnitPackage.KP_Height = 7m;
				handlingUnitPackage.KP_DimensionUQ = Constants.Length.Inches;

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.IsAndroidDevice = true;
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

				var response = webService.GetPackagesForPacking("HU");
				var packageInfo = response.PackagesForPackingInfo;
				AssertPackageInfo(packageInfo[0], handlingUnitPackage.PK.ToGuid(), "", "", "HU",
					10m, 3m, "LB",
					0m, false,
					5m, 6m, 7m, "IN",
					string.Empty, DateTime.MinValue, string.Empty, string.Empty,
					false, false, false,
					expectedIsConsolidationHandlingUnit: true,
					expectedIsUsingDirectedPackingConsolidation: false,
					expectedPackageRequiresPutaway: false,
					string.Empty, string.Empty);

				AssertEquals("Orders have same Carrier Booking Agent, should be using Carrier Label Integration.",
					true,
					packageInfo[0].IsUsingCarrierLabelIntegration);
			}
		}

		public void TestGetPackagesForPacking_HandlingUnitForConsolidation_DifferentCarrierLabel_DifferentOrg()
		{
			TestGetPackagesForPacking_HandlingUnitForConsolidation_DifferentCarrierLabel(isDiffOrg: true);
		}

		public void TestGetPackagesForPacking_HandlingUnitForConsolidation_DifferentCarrierLabel_WithNull()
		{
			TestGetPackagesForPacking_HandlingUnitForConsolidation_DifferentCarrierLabel(isDiffOrg: false);
		}

		void TestGetPackagesForPacking_HandlingUnitForConsolidation_DifferentCarrierLabel(bool isDiffOrg)
		{
			using (WarehouseDataRegistry.Instance.CheckAndroidDeviceVersion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, temporaryValue: false))
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

				var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
				var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
				packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
				Helper.Factory.Save();

				var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				var carrierBookingAgent1 = Helper.CreateClient("CBA1");
				order1.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent1.PK;

				var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
				if (isDiffOrg)
				{
					var carrierBookingAgent2 = Helper.CreateClient("CBA2");
					order2.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent2.PK;
				}

				Helper.CreatePickNew(order1, order2);

				var pickLine1 = order1.Lines[0].PickLines.Single();
				var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
				transferLine1.WE_WL = packingLocation.PK;

				var pickLine2 = order2.Lines[0].PickLines.Single();
				var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				transferLine2.WE_WL = packingLocation.PK;

				transferLine1.FinaliseDocketLine();
				transferLine2.FinaliseDocketLine();
				Helper.Factory.Save();

				var packingHelper = new PackingTestHelper(Helper.Factory);

				var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
				var package1 = packingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
				package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

				var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
				var package2 = packingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
				package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

				var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
				handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
				handlingUnit.KPU_JobContext = "3PL";

				var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
				var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

				packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
				packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

				handlingUnitPackage.KP_IsClosed = true;
				handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				handlingUnitPackage.KP_TareWeight = 3m;
				handlingUnitPackage.KP_Weight = 10m;
				handlingUnitPackage.KP_WeightUQ = Constants.Weight.Pounds;
				handlingUnitPackage.KP_Length = 5m;
				handlingUnitPackage.KP_Width = 6m;
				handlingUnitPackage.KP_Height = 7m;
				handlingUnitPackage.KP_DimensionUQ = Constants.Length.Inches;

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.IsAndroidDevice = true;
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

				var response = webService.GetPackagesForPacking("HU");
				var packageInfo = response.PackagesForPackingInfo;
				AssertPackageInfo(packageInfo[0], handlingUnitPackage.PK.ToGuid(), "", "", "HU",
					10m, 3m, "LB",
					0m, false,
					5m, 6m, 7m, "IN",
					string.Empty, DateTime.MinValue, string.Empty, string.Empty,
					false, false, false,
					expectedIsConsolidationHandlingUnit: true,
					expectedIsUsingDirectedPackingConsolidation: false,
					expectedPackageRequiresPutaway: false,
					string.Empty, string.Empty);

				AssertEquals("Orders with different Carrier Booking Agent, should not be using Carrier Label Integration.",
					false,
					packageInfo[0].IsUsingCarrierLabelIntegration);
			}
		}

		public void TestGetPackagesForPacking_HandlingUnitForConsolidation_WinCE()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;

			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var package = packingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			handlingUnitPackage.KP_IsClosed = true;
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_TareWeight = 3m;
			handlingUnitPackage.KP_Weight = 10m;
			handlingUnitPackage.KP_WeightUQ = Constants.Weight.Pounds;
			handlingUnitPackage.KP_Length = 5m;
			handlingUnitPackage.KP_Width = 6m;
			handlingUnitPackage.KP_Height = 7m;
			handlingUnitPackage.KP_DimensionUQ = Constants.Length.Inches;

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.IsAndroidDevice = false;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.GetPackagesForPacking("HU");
			var packageInfo = response.PackagesForPackingInfo;
			AssertEquals("Should not return Load HUs.", 0, packageInfo.Count);
		}

		public void TestGetPackagesForPacking_HandlingUnitForLoad()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Helper.Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var package = packingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			Helper.CreateLoadPkgPackagePivot(handlingUnitPackage.PK, load);

			handlingUnitPackage.KP_IsClosed = false;
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.Empty;

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.IsAndroidDevice = true;
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.GetPackagesForPacking("HU");
			var packageInfo = response.PackagesForPackingInfo;
			AssertEquals("Should not return Load HUs.", 0, packageInfo.Count);
		}

		#endregion

		#region TestGetPackagesForPacking_DefaultsWeightUQ

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestGetPackagesForPacking_DefaultsWeightUQ()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var orgMiscServ = data.Org1.MiscServ;
			orgMiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote = true;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pickedDate = ZDateTimeOffset.Now;
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Count());

			pickLines.ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_PickedDateTime = pickedDate;
				l.WZ_GS_NKAssignedTo = "E";
			});

			AssertEquals("Precondtion: Order should have 1 release line", 1, orderLine.ReleaseLines.Count);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			package.KP_Weight = 0m;
			package.KP_WeightUQ = "";
			package.KP_DimensionUQ = Constants.Length.Metres;
			Helper.Factory.Save();

			AssertEquals("Precondition: All pick lines have been picked", false, pickLines.Any(p => !p.IsPickedFromPutawayLocation));

			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Weight.Hectograms))
			{
				var webService = GetNewWebService();
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response = webService.GetPackagesForPacking("PKG01");
				var packageInfo = response.PackagesForPackingInfo;
				AssertPackageInfo(packageInfo[0], package.PK.ToGuid(), "O1", "W00000003", "PKG01",
					0m, 0m, Core.Constants.Weight.Hectograms,
					0m, false,
					0m, 0m, 0m, "M",
					order.WD_DocketStatus, order.WD_RequiredDate.ToDateTime(), order.PackageJob.KJ_JobID, order.Client.OH_Code,
					true,
					true,
					true,
					expectedIsConsolidationHandlingUnit: false,
					expectedIsUsingDirectedPackingConsolidation: false,
					expectedPackageRequiresPutaway: true,
					data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);
			}
		}

		#endregion

		#region TestGetPackagesForPacking_DefaultsMeasureUQ

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestGetPackagesForPacking_DefaultsMeasureUQ()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var orgMiscServ = data.Org1.MiscServ;
			orgMiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote = true;
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 6m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			var pickedDate = ZDateTimeOffset.Now;
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			var pickLines = pick.GetAllPickLines();
			AssertEquals("Precondition: Pick should have 2 PickLines.", 2, pickLines.Count());

			pickLines.ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_PickedDateTime = pickedDate;
				l.WZ_GS_NKAssignedTo = "E";
			});

			AssertEquals("Precondtion: Order should have 1 release line", 1, orderLine.ReleaseLines.Count);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG01";
			package.SetIsTote(true);
			package.Pack(orderLine.ReleaseLines[0], 5m);
			package.KP_WeightUQ = Constants.Weight.Kilograms;
			package.KP_DimensionUQ = "";
			Helper.Factory.Save();

			AssertEquals("Precondition: All pick lines have been picked", false, pickLines.Any(p => !p.IsPickedFromPutawayLocation));

			using (PackingRegistry.Instance.DimensionUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Length.Yards))
			{
				var webService = GetNewWebService();
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response = webService.GetPackagesForPacking("PKG01");
				var packageInfo = response.PackagesForPackingInfo;
				AssertPackageInfo(packageInfo[0], package.PK.ToGuid(), "O1", "W00000003", "PKG01",
					10m, 0m, Constants.Weight.Kilograms,
					0m, false,
					0m, 0m, 0m, Core.Constants.Length.Yards,
					order.WD_DocketStatus, order.WD_RequiredDate.ToDateTime(), order.PackageJob.KJ_JobID, order.Client.OH_Code,
					true,
					true,
					true,
					expectedIsConsolidationHandlingUnit: false,
					expectedIsUsingDirectedPackingConsolidation: false,
					expectedPackageRequiresPutaway: true,
					data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);
			}
		}

		#endregion

		#region TestGetPackagesForPacking_WhenOrderHasCBA

		public void TestGetPackagesForPacking_WhenOrderHasCBA()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = Helper.CreateClient("RTUS").PK;
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG01";
			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("PKG01");
			var packageInfo = response.PackagesForPackingInfo;
			AssertEquals("Order has Carrier Booking Agent, should be using Carrier Label Integration.", true, packageInfo[0].IsUsingCarrierLabelIntegration);
		}

		#endregion

		#region TestGetPackagesForPacking_MultiplePackages

		public void TestGetPackagesForPacking_MultiplePackages()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 11m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part2, 11m);
			Helper.Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var pick1Lines = pick1.GetAllPickLines();

			var pick2 = Helper.CreatePickNew(order2);
			var pick2Lines = pick2.GetAllPickLines();

			AssertEquals("Precondition: Pick 1 should have 1 PickLine.", 1, pick1Lines.Count());
			AssertEquals("Precondition: Pick 2 should have 1 PickLine.", 1, pick2Lines.Count());

			pick1Lines.FirstOrDefault().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick1Lines.FirstOrDefault().WZ_GS_NKAssignedTo = "E";
			pick2Lines.FirstOrDefault().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick2Lines.FirstOrDefault().WZ_GS_NKAssignedTo = "E";

			AssertEquals("Precondtion: Order1 should have 1 release line", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Precondtion: Order2 should have 1 release line", 1, order2Line.ReleaseLines.Count);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P0001";

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P0001";

			var package3 = Helper.Factory.NewWithValidTestData<PkgPackage>();
			package3.KP_PackageID = "P0001"; // 3rd package with the same ID, should not be returned by the webservice
			Helper.Factory.Save();

			AssertEquals("Precondition: Both package1 and package2 have the same package IDs.", true, package1.KP_PackageID == package2.KP_PackageID);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("P0001");

			AssertEquals("Response should return 2 packages.", 2, response.PackagesForPackingInfo.Count);

			var packageInfo1 = response.PackagesForPackingInfo[0];
			AssertEquals("1st package is package1.", package1.PK, packageInfo1.PK);

			var packageInfo2 = response.PackagesForPackingInfo[1];
			AssertEquals("2nd package is package2.", package2.PK, packageInfo2.PK);
		}

		public void TestGetPackagesForPacking_PackageIsClose()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 11m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part2, 11m);
			Helper.Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var pick1Lines = pick1.GetAllPickLines();

			var pick2 = Helper.CreatePickNew(order2);
			var pick2Lines = pick2.GetAllPickLines();

			AssertEquals("Precondition: Pick 1 should have 1 PickLine.", 1, pick1Lines.Count());
			AssertEquals("Precondition: Pick 2 should have 1 PickLine.", 1, pick2Lines.Count());

			pick1Lines.FirstOrDefault().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick1Lines.FirstOrDefault().WZ_GS_NKAssignedTo = "E";
			pick2Lines.FirstOrDefault().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick2Lines.FirstOrDefault().WZ_GS_NKAssignedTo = "E";

			AssertEquals("Precondtion: Order1 should have 1 release line", 1, order1Line.ReleaseLines.Count);
			AssertEquals("Precondtion: Order2 should have 1 release line", 1, order2Line.ReleaseLines.Count);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P0001";

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P0001";
			package2.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			AssertEquals("Precondition: Both package1 and package2 have the same package IDs.", true, package1.KP_PackageID == package2.KP_PackageID);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("P0001");

			AssertEquals("Response should return 1 package.", 1, response.PackagesForPackingInfo.Count);
			AssertEquals("1st package is selected.", package1.PK, response.PackagesForPackingInfo[0].PK);
		}

		#endregion

		#region GetPackagesForPacking_ClientPickPackParamsConfiguration

		#region TestGetPackagesForPacking_NoClientPickPackParamsConfiguration

		public void TestGetPackagesForPacking_NoClientPickPackParamsConfiguration()
		{
			TestGetPackagesForPacking_ClientPickPackParamsConfigurationCore("IsUsingCartonSizes should be true.", hasClientPickPackParamsConfiguration: false, isUsingCartonSizes: true);
		}

		#endregion

		#region TestGetPackagesForPacking_WithClientPickPackParamsConfiguration_NotUsingCartonSizes

		public void TestGetPackagesForPacking_WithClientPickPackParamsConfiguration_NotUsingCartonSizes()
		{
			TestGetPackagesForPacking_ClientPickPackParamsConfigurationCore("IsUsingCartonSizes should be false.", hasClientPickPackParamsConfiguration: true, isUsingCartonSizes: false);
		}

		#endregion

		#region TestGetPackagesForPacking_WithClientPickPackParamsConfiguration_UsingCartonSizes

		public void TestGetPackagesForPacking_WithClientPickPackParamsConfiguration_UsingCartonSizes()
		{
			TestGetPackagesForPacking_ClientPickPackParamsConfigurationCore("IsUsingCartonSizes should be true.", hasClientPickPackParamsConfiguration: true, isUsingCartonSizes: true);
		}

		#endregion

		#region TestGetPackagesForPacking_ClientPickPackParamsConfigurationCore

		void TestGetPackagesForPacking_ClientPickPackParamsConfigurationCore(string assertMessage, bool hasClientPickPackParamsConfiguration, bool isUsingCartonSizes)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.Factory.Save();

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_WSH_SalesChannel = salesChannel.PK;
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG01";
			Helper.Factory.Save();

			if (hasClientPickPackParamsConfiguration)
			{
				var clientParams = WhsClientPickingParams.GetClientPickingParams(data.Org1);
				var pickPackParams = clientParams.WarehousePickPackParams.AddNew();
				pickPackParams.WPP_WW_Warehouse = data.Whs1.PK;
				pickPackParams.WPP_IsUsingCartonSizes = isUsingCartonSizes;
				Helper.Factory.Save();
			}

			var clientPickParamsByWhsQuery = new ZQuery();
			clientPickParamsByWhsQuery.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, data.Org1.PK);
			clientPickParamsByWhsQuery.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_WW_Warehouse, data.Whs1.PK);

			AssertEquals($"Precondition: client has{(hasClientPickPackParamsConfiguration ? "" : " no")} pick pack params configuration.",
				hasClientPickPackParamsConfiguration, Helper.Factory.Load<WhsClientPickPackParamsByWhs>(clientPickParamsByWhsQuery).Any());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("PKG01");
			var packageInfo = response.PackagesForPackingInfo;
			AssertEquals(assertMessage, isUsingCartonSizes, packageInfo[0].IsUsingCartonSizes);

			if (hasClientPickPackParamsConfiguration)
			{
				var clientParams = WhsClientPickingParams.GetClientPickingParams(data.Org1);
				var pickPackParamsWithSalesChannel = clientParams.WarehousePickPackParams.AddNew();
				pickPackParamsWithSalesChannel.WPP_WW_Warehouse = data.Whs1.PK;
				pickPackParamsWithSalesChannel.WPP_IsUsingCartonSizes = !isUsingCartonSizes;
				pickPackParamsWithSalesChannel.WPP_WSH_SalesChannel = salesChannel.PK;
				Helper.Factory.Save();

				var webService2 = GetNewWebService();
				webService2.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
				var response2 = webService2.GetPackagesForPacking("PKG01");
				var packageInfo2 = response2.PackagesForPackingInfo;
				AssertEquals("Sales Channel Parameter should take preference.", !isUsingCartonSizes, packageInfo2[0].IsUsingCartonSizes);
			}
		}

		#endregion

		#region TestGetPackagesForPacking_ClientPickPackParamsConfiguration_AllowDDL

		public void TestGetPackagesForPacking_WithClientPickPackParamsConfiguration_AllowDDL_IsAllowing()
		{
			TestGetPackagesForPacking_ClientPickPackParamsConfiguration_AllowDDLCore("AllowedToOverrideDockDoorLocation should be true.", hasClientPickPackParamsConfiguration: true, isAllowing: true);
		}

		public void TestGetPackagesForPacking_WithClientPickPackParamsConfiguration_AllowDDL_IsNotAllowing()
		{
			TestGetPackagesForPacking_ClientPickPackParamsConfiguration_AllowDDLCore("AllowedToOverrideDockDoorLocation should be false.", hasClientPickPackParamsConfiguration: true, isAllowing: false);
		}

		public void TestGetPackagesForPacking_WithClientPickPackParamsConfiguration_AllowDDL_NoParam()
		{
			TestGetPackagesForPacking_ClientPickPackParamsConfiguration_AllowDDLCore("AllowedToOverrideDockDoorLocation should be false.", hasClientPickPackParamsConfiguration: false, isAllowing: false);
		}

		void TestGetPackagesForPacking_ClientPickPackParamsConfiguration_AllowDDLCore(string assertMessage, bool hasClientPickPackParamsConfiguration, bool isAllowing)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 4m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "PKG01";
			Helper.Factory.Save();

			if (hasClientPickPackParamsConfiguration)
			{
				var clientParams = WhsClientPickingParams.GetClientPickingParams(data.Org1);
				var pickPackParams = clientParams.WarehousePickPackParams.AddNew();
				pickPackParams.WPP_WW_Warehouse = data.Whs1.PK;
				pickPackParams.WPP_AllowPickDockDoorLocationOverride = isAllowing;
				Helper.Factory.Save();
			}

			var clientPickParamsByWhsQuery = new ZQuery();
			clientPickParamsByWhsQuery.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_OH_Client, data.Org1.PK);
			clientPickParamsByWhsQuery.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_WW_Warehouse, data.Whs1.PK);

			AssertEquals($"Precondition: client has{(hasClientPickPackParamsConfiguration ? "" : " no")} pick pack params configuration.",
				hasClientPickPackParamsConfiguration, Helper.Factory.Load<WhsClientPickPackParamsByWhs>(clientPickParamsByWhsQuery).Any());

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackagesForPacking("PKG01");
			var packageInfo = response.PackagesForPackingInfo;
			AssertEquals(assertMessage, isAllowing, packageInfo[0].AllowedToOverrideDockDoorLocation);
		}

		#endregion

		#endregion

		#region TestGetPackagesForPacking_PackageRequiresPutaway

		public void TestGetPackagesForPacking_PackageRequiresPutaway_InTransitToDockDoor()
		{
			TestGetPackagesForPacking_PackageRequiresPutaway_DockDoorCore(isPackageInTransit: true);
		}

		public void TestGetPackagesForPacking_PackageRequiresPutaway_FinalizedAtDockDoor()
		{
			TestGetPackagesForPacking_PackageRequiresPutaway_DockDoorCore(isPackageInTransit: false);
		}

		void TestGetPackagesForPacking_PackageRequiresPutaway_DockDoorCore(bool isPackageInTransit)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			if (!isPackageInTransit)
			{
				transferLine.FinaliseDocketLine();
			}
			AssertEquals("Precondition: Outbound Transfer to dockdoor", transferLine.WE_WL, data.Whs1.DefaultInboundDockDoorLocation.PK);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", isPackageInTransit ? InventoryStatus.Codes.InTransit : InventoryStatus.Codes.Staged, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", !isPackageInTransit, transferLine.IsFinalised);
			Helper.Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var package = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesForPacking("PKG1");
			var packageInfo = response.PackagesForPackingInfo.Single();
			AssertEquals("Package requires Putaway.", isPackageInTransit, packageInfo.PackageRequiresPutaway);
		}

		public void TestGetPackagesForPacking_PackageRequiresPutaway_InTransitToConsolidationLocation()
		{
			TestGetPackagesForPacking_PackageRequiresPutaway_ConsolidationLocationCore(isPackageInTransit: true);
		}

		public void TestGetPackagesForPacking_PackageRequiresPutaway_FinalizedAtConsolidationLocation()
		{
			TestGetPackagesForPacking_PackageRequiresPutaway_ConsolidationLocationCore(isPackageInTransit: false);
		}

		void TestGetPackagesForPacking_PackageRequiresPutaway_ConsolidationLocationCore(bool isPackageInTransit)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = consolidationLocation.PK;
			if (!isPackageInTransit)
			{
				transferLine.FinaliseDocketLine();
			}
			AssertEquals("Precondition: Outbound Transfer to dockdoor", transferLine.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", isPackageInTransit ? InventoryStatus.Codes.InTransit : InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", !isPackageInTransit, transferLine.IsFinalised);
			Helper.Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var package = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesForPacking("PKG1");
			var packageInfo = response.PackagesForPackingInfo.Single();
			AssertEquals("Package requires Putaway.", isPackageInTransit, packageInfo.PackageRequiresPutaway);
		}

		#endregion

		#region TestGetPackagesForPacking_PackagePutawayFromPackingStation

		public void TestGetPackagesForPacking_PackagePutawayFromPackingStation_SiblingPackagePutawayInConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			Helper.Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			var package2 = packingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine1.WE_WL = consolidationLocation.PK;
			newTransferLine1.FinaliseDocketLine();
			newFactory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesForPacking("PKG2");
			var packageInfo = response.PackagesForPackingInfo.Single();
			AssertEquals(true, packageInfo.PackageRequiresPutaway);
			AssertEquals("PS1", packageInfo.AssignedPutawayLocationString);
			AssertEquals("PS1", packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals("CON", packageInfo.AssignedPutawayLocationClass);
		}

		public void TestGetPackagesForPacking_PackagePutawayFromPackingStation_SiblingPackagePutawayInDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			Helper.Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			var package2 = packingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine1.FinaliseDocketLine();
			newFactory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesForPacking("PKG2");
			var packageInfo = response.PackagesForPackingInfo.Single();
			AssertEquals(true, packageInfo.PackageRequiresPutaway);
			AssertEquals("DOCKDOOR", packageInfo.AssignedPutawayLocationString);
			AssertEquals("DOCKDOOR", packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals("DDL", packageInfo.AssignedPutawayLocationClass);
		}

		public void TestGetPackagesForPacking_PackagePutawayFromPackingStation_MultiplePackagesWithSameId()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;

			var pickLine3 = order2Line.PickLines.Single();
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			transferLine3.WE_WL = packingLocation.PK;
			Helper.Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			var package2 = packingHelper.CreatePackage(packageJob1, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package3 = packingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package3.Pack(order2Line.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine1.FinaliseDocketLine();
			newFactory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesForPacking("PKG2");
			var packageInfo_WithSiblingPackageInDockDoor = response.PackagesForPackingInfo.Single(p => p.OrderReference == "O1");
			AssertEquals(true, packageInfo_WithSiblingPackageInDockDoor.PackageRequiresPutaway);
			AssertEquals("DOCKDOOR", packageInfo_WithSiblingPackageInDockDoor.AssignedPutawayLocationString);
			AssertEquals("DOCKDOOR", packageInfo_WithSiblingPackageInDockDoor.AssignedPutawayLocationStringUserFriendly);
			AssertEquals("DDL", packageInfo_WithSiblingPackageInDockDoor.AssignedPutawayLocationClass);

			var packageInfo_WithNoSiblingPackages = response.PackagesForPackingInfo.Single(p => p.OrderReference == "O2");
			AssertEquals(true, packageInfo_WithNoSiblingPackages.PackageRequiresPutaway);
			AssertEquals(string.Empty, packageInfo_WithNoSiblingPackages.AssignedPutawayLocationString);
			AssertEquals(string.Empty, packageInfo_WithNoSiblingPackages.AssignedPutawayLocationStringUserFriendly);
			AssertEquals(string.Empty, packageInfo_WithNoSiblingPackages.AssignedPutawayLocationClass);
		}

		#endregion

		#region Implementation

		void AssertPackageInfo(PackageForPackingInfo packageInfo, Guid expectedPK,
			string expectedOrderReference, string expectedDocketID, string expectedPackageID,
			decimal expectedWeight, decimal expectedEmptyWeight, string expectedWeightUQ,
			decimal expectedPackageWeightTolerance, bool expectedPackageWeightToleranceEnabled,
			decimal expectedLength, decimal expectedWidth, decimal expectedHeight, string expectedDimUQ,
			string expectedDocketStatus, DateTime expectedRequiredDate, string expectedJobID, string expectedClientCode,
			bool expectedIsTote,
			bool expectedIsUsingCartonSizes,
			bool expectedClientEnforceProductScan,
			bool expectedIsConsolidationHandlingUnit,
			bool expectedIsUsingDirectedPackingConsolidation,
			bool expectedPackageRequiresPutaway,
			string dockDoorLocation, string dockDoorLocationUserFriendly)
		{
			AssertNotNull("PackageInfo should not be null", packageInfo);
			AssertEquals("PK", expectedPK, packageInfo.PK);
			AssertEquals("OrderReference", expectedOrderReference, packageInfo.OrderReference);
			AssertEquals("Docket ID", expectedDocketID, packageInfo.DocketID);
			AssertEquals("Package ID", expectedPackageID, packageInfo.ToteID);
			AssertEquals("ClientCode", expectedClientCode, packageInfo.ClientCode);
			AssertEquals("Package Weight", expectedWeight, packageInfo.Weight);
			AssertEquals("Package Empty Weight", expectedEmptyWeight, packageInfo.EmptyWeight);
			AssertEquals("Package Weight UQ", expectedWeightUQ, packageInfo.WeightUQ);
			AssertEquals("Package Weight Tolerance", expectedPackageWeightTolerance, packageInfo.PackageWeightTolerance);
			AssertEquals("Package Weight Tolerance Enabled", expectedPackageWeightToleranceEnabled, packageInfo.PackageWeightToleranceEnabled);
			AssertEquals("Package Length", expectedLength, packageInfo.Length);
			AssertEquals("Package Width", expectedWidth, packageInfo.Width);
			AssertEquals("Package Height", expectedHeight, packageInfo.Height);
			AssertEquals("Package Dimension UQ", expectedDimUQ, packageInfo.DimensionUQ);
			AssertEquals("Is Tote", expectedIsTote, packageInfo.IsTote);
			AssertEquals("Is using carton sizes", expectedIsUsingCartonSizes, packageInfo.IsUsingCartonSizes);
			AssertEquals("Docket Status", expectedDocketStatus, packageInfo.DocketStatus);
			AssertEquals("Required Date", expectedRequiredDate, packageInfo.RequiredDate);
			AssertEquals("Job ID", expectedJobID, packageInfo.JobID);
			AssertEquals("Client Enforce Product Scan", expectedClientEnforceProductScan, packageInfo.ClientEnforceProductScan);
			AssertEquals("Is Consolidation Handling Unit", expectedIsConsolidationHandlingUnit, packageInfo.IsConsolidationHandlingUnit);
			AssertEquals("Is Using Directed Packing Consolidation", expectedIsUsingDirectedPackingConsolidation, packageInfo.OrderIsUsingDirectedPackingConsolidation);
			AssertEquals("Package Requires Putaway", expectedPackageRequiresPutaway, packageInfo.PackageRequiresPutaway);
			AssertEquals("Assigned Dock Door", dockDoorLocation, packageInfo.AssignedDockDoorLocationString);
			AssertEquals("Assigned Dock Door", dockDoorLocationUserFriendly, packageInfo.AssignedDockDoorLocationStringUserFriendly);
		}

		#endregion
	}
}

using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetPackageForPackingTest : WhsSecureServiceTestCase
	{
		#region TestGetPackageForPacking

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestGetPackageForPacking()
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
			package.Pack(orderLine.ReleaseLines[0], 5m);
			package.KP_Weight = 10m;
			package.KP_WeightUQ = Constants.Weight.Pounds;
			package.KP_Length = 5m;
			package.KP_Width = 6m;
			package.KP_Height = 7m;
			package.KP_DimensionUQ = Constants.Length.Inches;

			Helper.Factory.Save();

			AssertEquals("Precondition: All pick lines have been picked", false, pickLines.Any(p => !p.IsPickedFromPutawayLocation));

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response = webService.GetPackageForPacking("PKG01", package.PK.ToGuid());
			var packageInfo = response.PackageForPackingInfo;
			AssertPackageInfo(packageInfo, package.PK.ToGuid(), "O1", "W00000003", "PKG01",
				10m, "LB",
				10m, true,
				5m, 6m, 7m, "IN",
				order.WD_DocketStatus, order.WD_RequiredDate.ToDateTime(), order.PackageJob.KJ_JobID, order.Client.OH_Code,
				false,
				true,
				true,
				true,
				true,
				data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);
		}

		public void TestGetPackageForPacking_PackageInPackingStation()
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
			var response = webService.GetPackageForPacking("PKG1", package.PK.ToGuid());
			var packageInfo = response.PackageForPackingInfo;
			AssertEquals(true, packageInfo.PackageRequiresPutaway);
			AssertEquals(string.Empty, packageInfo.AssignedPutawayLocationString);
			AssertEquals(string.Empty, packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals(string.Empty, packageInfo.AssignedPutawayLocationClass);
		}

		public void TestGetPackageForPacking_PackageInConsolidationLocation()
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
			var response = webService.GetPackageForPacking("PKG1", package.PK.ToGuid());
			var packageInfo = response.PackageForPackingInfo;
			AssertEquals(false, packageInfo.PackageRequiresPutaway);
			AssertEquals(string.Empty, packageInfo.AssignedPutawayLocationString);
			AssertEquals(string.Empty, packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals(string.Empty, packageInfo.AssignedPutawayLocationClass);
		}

		public void TestGetPackageForPacking_PackagePutawayFromPackingStation_SiblingPackagePutawayInConsolidationLocation()
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
			var response = webService.GetPackageForPacking("PKG2", package2.PK.ToGuid());
			var packageInfo = response.PackageForPackingInfo;
			AssertEquals(true, packageInfo.PackageRequiresPutaway);
			AssertEquals("PS1", packageInfo.AssignedPutawayLocationString);
			AssertEquals("PS1", packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals("CON", packageInfo.AssignedPutawayLocationClass);
		}

		public void TestGetPackageForPacking_PackagePutawayFromPackingStation_SiblingPackagePutawayInDockDoorLocation()
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
			var response = webService.GetPackageForPacking("PKG2", package2.PK.ToGuid());
			var packageInfo = response.PackageForPackingInfo;
			AssertEquals(true, packageInfo.PackageRequiresPutaway);
			AssertEquals("DOCKDOOR", packageInfo.AssignedPutawayLocationString);
			AssertEquals("DOCKDOOR", packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals("DDL", packageInfo.AssignedPutawayLocationClass);
		}

		#endregion

		#region TestGetPackageForPacking_NoPackageMatched

		public void TestGetPackageForPacking_NoPackageMatched()
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
			package1.KP_PackageID = "PACK02";
			package1.Pack(orderLine.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "PACK03";
			package2.Pack(orderLine.ReleaseLines[0], 5m);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			var response1 = webService.GetPackageForPacking("PACK01", package1.PK.ToGuid());
			AssertNull("PackageInfo should be null.", response1.PackageForPackingInfo);
			AssertEquals("Should Error as wrong Package ID given.", ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Package 'PACK01' cannot be found or is not valid for Packing.", response1.ErrorMessage);

			var response2 = webService.GetPackageForPacking("PACK03", package1.PK.ToGuid());
			AssertNull("PackageInfo should be null.", response2.PackageForPackingInfo);
			AssertEquals("Should Error as wrong Package ID given.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Package 'PACK03' cannot be found or is not valid for Packing.", response2.ErrorMessage);
		}

		#endregion

		#region Implementation

		void AssertPackageInfo(PackageForPackingInfo packageInfo, Guid expectedPK,
			string expectedOrderReference, string expectedDocketID, string expectedPackageID,
			decimal expectedWeight, string expectedWeightUQ,
			decimal expectedPackageWeightTolerance, bool expectedPackageWeightToleranceEnabled,
			decimal expectedLength, decimal expectedWidth, decimal expectedHeight, string expectedDimUQ,
			string expectedDocketStatus, DateTime expectedRequiredDate, string expectedJobID, string expectedClientCode,
			bool expectedIsTote,
			bool expectedIsUsingCartonSizes,
			bool expectedClientEnforceProductScan,
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
			AssertEquals("Is Using Directed Packing Consolidation", expectedIsUsingDirectedPackingConsolidation, packageInfo.OrderIsUsingDirectedPackingConsolidation);
			AssertEquals("Package Requires Putaway", expectedPackageRequiresPutaway, packageInfo.PackageRequiresPutaway);
			AssertEquals("Assigned Dock Door", dockDoorLocation, packageInfo.AssignedDockDoorLocationString);
			AssertEquals("Assigned Dock Door", dockDoorLocationUserFriendly, packageInfo.AssignedDockDoorLocationStringUserFriendly);
		}

		#endregion
	}
}

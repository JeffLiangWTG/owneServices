using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetPackagesToPutawayToConsolidationLocationsTest : WhsSecureServiceTestCase
	{
		#region TestJobNotFound

		public void TestJobNotFound()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			Factory.Save();

			var errorResponse1 = webService.GetPackagesToPutawayToConsolidationLocations(Guid.NewGuid(), PickJobType.Pick);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse1.Error);
			AssertEquals("Error loading job.", errorResponse1.ErrorMessage);

			var errorResponse2 = webService.GetPackagesToPutawayToConsolidationLocations(Guid.NewGuid(), PickJobType.PickByLabelJob);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse2.Error);
			AssertEquals("Error loading job.", errorResponse2.ErrorMessage);

			var errorResponse3 = webService.GetPackagesToPutawayToConsolidationLocations(Guid.NewGuid(), PickJobType.TrolleyJob);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse3.Error);
			AssertEquals("Error loading job.", errorResponse3.ErrorMessage);
		}

		#endregion

		#region TestGetPackagesToPutawayToConsolidationLocations_PickJobPick

		public void TestGetPackagesToPutawayToConsolidationLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(false, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);
			AssertEquals(order.PK, groupedPackagesToPutaway[0].Packages[0].OrderPK);

			AssertExpectedPackagesForAllocation(new[] { package }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_UnnamedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew(PkgUnit.Box, 1);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Need to put away unnamed package to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(0, groupedPackagesToPutaway.Length);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_UnpackedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Need to put away unpacked lines to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(0, groupedPackagesToPutaway.Length);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_AssignedPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation2.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation2.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order.Lines[0].ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order.Lines[1].ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation2.PK, transferLine2.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS2", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS2", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation2.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(true, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);
			AssertEquals(order.PK, groupedPackagesToPutaway[0].Packages[0].OrderPK);

			AssertExpectedPackagesForAllocation(new[] { package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_AssignedPutaway_MultipleOrders_TransferLineFinalised()
		{
			TestGetPackagesToPutawayToConsolidationLocations_AssignedPutaway_MultipleOrdersCore(isTransferLineFinalised: true);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_AssignedPutaway_MultipleOrders_TransferLineNotFinalised()
		{
			TestGetPackagesToPutawayToConsolidationLocations_AssignedPutaway_MultipleOrdersCore(isTransferLineFinalised: false);
		}

		void TestGetPackagesToPutawayToConsolidationLocations_AssignedPutaway_MultipleOrdersCore(bool isTransferLineFinalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var year = ZDate.Today.Year;
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order1, data.Part2, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order1.RequiredDate = new ZDateTime(year, 12, 1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			order2.WD_UseDirectedPackingConsolidation = true;
			order2.RequiredDate = new ZDateTime(year, 12, 1);

			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;
			if (isTransferLineFinalised)
			{
				transferLine1.FinaliseDocketLine();
			}

			var pickLine2 = order1.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var pickLine3 = order2.Lines[0].PickLines.Single();
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order1.Lines[1].ReleaseLines[0], 10m);

			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package3 = PackingHelper.CreatePackage(packageJob3, "PKG3", 1, PkgUnit.Box);
			package3.Pack(order2.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine2.WE_WL);
			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine3.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertExpectedPackagesForAllocation(isTransferLineFinalised ? new[] { package2, package3 } : new[] { package1, package2, package3 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());

			var consolidationPackageInfosOrder1 = groupedPackagesToPutaway[0].Packages.Where(pkg => pkg.OrderPK == order1.PK);
			Assert(consolidationPackageInfosOrder1.All(info => info.CannotOverrideConsolidationLocation == isTransferLineFinalised));

			var consolidationPackageInfoOrder2 = groupedPackagesToPutaway[0].Packages.Single(pkg => pkg.OrderPK == order2.PK);
			AssertEquals(false, consolidationPackageInfoOrder2.CannotOverrideConsolidationLocation);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_AssignedPutaway_AllocatedFinalisedTransferLinePrioritized()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			order.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation2.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			var pickLine3 = orderLine3.PickLines.Single();
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation2.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", consolidationLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package3 = PackingHelper.CreatePackage(packageJob3, "PKG3", 1, PkgUnit.Box);
			package3.Pack(orderLine3.ReleaseLines[0], 15m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation2.PK, transferLine2.WE_WL);
			AssertEquals("Destination for transferLine should be updated.", consolidationLocation2.PK, transferLine3.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS2", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS2", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation2.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertExpectedPackagesForAllocation(new[] { package2, package3 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());

			AssertEquals(true, groupedPackagesToPutaway[0].Packages.All(pkg => pkg.CannotOverrideConsolidationLocation));
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_NoConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", Guid.Empty, transferLine.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(Guid.Empty, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);

			AssertExpectedPackagesForAllocation(new[] { package }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PickHasLooseInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2.PK, 5m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should NOT be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Need to put away packages to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(0, groupedPackagesToPutaway.Length);
		}

		#region TestGetPackagesToPutawayToConsolidationLocations_PickAndPack

		public void TestGetPackagesToPutawayToConsolidationLocations_PickAndPack()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2.PK, 5m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PickAndPack_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 300m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 300m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD1", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order1, data.Part2.PK, 5m);
			order1.WD_UseDirectedPackingConsolidation = true;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 10m);
			Helper.CreateWhsOrderLine(order2, data.Part2.PK, 5m);
			order2.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals("Destination for transferLine should NOT be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Destination for transferLine should NOT be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory should be put away to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(0, groupedPackagesToPutaway.Length);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PickAndPack_NotUsingPackingConsolidation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2.PK, 5m);
			order.WD_UseDirectedPackingConsolidation = false;
			var pick = Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should NOT be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Need to put away packages to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(0, groupedPackagesToPutaway.Length);
		}

		#endregion

		#region TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatching

		public void TestGetPackagesToPutawayToConsolidationLocations_DistributionCentre()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var org1 = data.Org1;
			var org2 = Helper.CreateClient("222", "222");

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var now = ZDateTimeOffset.Now;

			var transportCo1 = Helper.CreateClient("X1", "X1");

			var transportCo2 = Helper.CreateClient("Y1", "Y1");

			var receive = Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org1, data.Whs1, "ORD1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order1.ConsigneeAddressPK = org1.MainAddress.PK;
			order1.WD_WLO_PlannedLoad = load1.PK;
			order1.TransportCoPK = transportCo1.PK;
			order1.WD_PL_NKCarrierServiceLevel = "SL1";
			order1.WD_RequiredDate = now.AddDays(5);

			order1.DistributionCentreAddressPK = org2.MainAddress.PK;

			var pick1 = Helper.CreatePickNew(order1);
			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(pick1.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response1, webService1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(org1, data.Whs1, "ORD2", data.Part1, 10m);

			order2.WD_UseDirectedPackingConsolidation = true;
			order2.ConsigneeAddressPK = org2.MainAddress.PK;
			order2.WD_WLO_PlannedLoad = load1.PK;
			order2.TransportCoPK = transportCo1.PK;
			order2.WD_PL_NKCarrierServiceLevel = "SL1";
			order2.WD_RequiredDate = now.AddDays(2);

			order2.DistributionCentreAddressPK = org2.MainAddress.PK;

			var pick2 = Helper.CreatePickNew(order2);

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			Factory.Save();

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pick2.PK.ToGuid(), PickJobType.Pick);

			var groupedPackagesToPutaway = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals(1, groupedPackagesToPutaway.Length);

			AssertExpectedPackagesForAllocation(new[] { package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());

			AssertEquals("Destination for transferLine should match order1.", transferLine1.WE_WL, transferLine2.WE_WL);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_OverridenDistributionCentre()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var org1 = data.Org1;
			var org2 = Helper.CreateClient("222", "222");

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var now = ZDateTimeOffset.Now;

			var transportCo1 = Helper.CreateClient("X1", "X1");

			var transportCo2 = Helper.CreateClient("Y1", "Y1");

			var receive = Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org1, data.Whs1, "ORD1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order1.ConsigneeAddressPK = org1.MainAddress.PK;
			order1.WD_WLO_PlannedLoad = load1.PK;
			order1.TransportCoPK = transportCo1.PK;
			order1.WD_PL_NKCarrierServiceLevel = "SL1";
			order1.WD_RequiredDate = now.AddDays(5);

			order1.DistributionCentreDocAddress.E2_AddressOverride = true;

			var pick1 = Helper.CreatePickNew(order1);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(pick1.PK.ToGuid(), PickJobType.Pick);

			var order2 = Helper.CreateWhsOrderWithOrderLine(org1, data.Whs1, "ORD2", data.Part1, 10m);

			order2.WD_UseDirectedPackingConsolidation = true;
			order2.ConsigneeAddressPK = org1.MainAddress.PK;
			order2.WD_WLO_PlannedLoad = load1.PK;
			order2.TransportCoPK = transportCo1.PK;
			order2.WD_PL_NKCarrierServiceLevel = "SL1";
			order2.WD_RequiredDate = now.AddDays(2);

			order2.DistributionCentreDocAddress.E2_AddressOverride = true;

			var pick2 = Helper.CreatePickNew(order2);

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			Factory.Save();

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pick2.PK.ToGuid(), PickJobType.Pick);

			var groupedPackagesToPutaway = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals(1, groupedPackagesToPutaway.Length);

			AssertExpectedPackagesForAllocation(new[] { package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());

			AssertEquals("Destination for transferLine should match order1.", transferLine1.WE_WL, transferLine2.WE_WL);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_DistributionCentreAndConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			var org1 = data.Org1;
			var org2 = Helper.CreateClient("222", "222");

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var now = ZDateTimeOffset.Now;

			var transportCo1 = Helper.CreateClient("X1", "X1");

			var transportCo2 = Helper.CreateClient("Y1", "Y1");

			var receive = Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(org1, data.Whs1, "ORD1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order1.ConsigneeAddressPK = org1.MainAddress.PK;
			order1.WD_WLO_PlannedLoad = load1.PK;
			order1.TransportCoPK = transportCo1.PK;
			order1.WD_PL_NKCarrierServiceLevel = "SL1";
			order1.WD_RequiredDate = now.AddDays(5);

			order1.DistributionCentreAddressPK = org2.MainAddress.PK;

			var pick1 = Helper.CreatePickNew(order1);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(pick1.PK.ToGuid(), PickJobType.Pick);

			var order2 = Helper.CreateWhsOrderWithOrderLine(org1, data.Whs1, "ORD2", data.Part1, 10m);

			order2.WD_UseDirectedPackingConsolidation = true;
			order2.ConsigneeAddressPK = org2.MainAddress.PK;
			order2.WD_WLO_PlannedLoad = load1.PK;
			order2.TransportCoPK = transportCo1.PK;
			order2.WD_PL_NKCarrierServiceLevel = "SL1";
			order2.WD_RequiredDate = now.AddDays(2);

			order2.DistributionCentreAddressPK = ZGuid.Empty;

			var pick2 = Helper.CreatePickNew(order2);

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			Factory.Save();

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pick2.PK.ToGuid(), PickJobType.Pick);

			var groupedPackagesToPutaway = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals(1, groupedPackagesToPutaway.Length);

			AssertExpectedPackagesForAllocation(new[] { package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());

			AssertEquals("Destination for transferLine should match order1.", transferLine1.WE_WL, transferLine2.WE_WL);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_ConsolidateAtSameConsolidationLocationForMatchingCriteria()
		{
			TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatchingCore(isMatchingCriteria: true, matchConsignee: true, matchLoad: true, matchTransportCo: true, matchCarrierServiceLevel: true, earlierRequiredDate: true);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MismatchedConsignee()
		{
			TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatchingCore(isMatchingCriteria: false, matchConsignee: false);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MismatchedLoad()
		{
			TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatchingCore(isMatchingCriteria: false, matchLoad: false);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MismatchedTransportCo()
		{
			TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatchingCore(isMatchingCriteria: false, matchTransportCo: false);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MismatchedCarrierServiceLevel()
		{
			TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatchingCore(isMatchingCriteria: false, matchCarrierServiceLevel: false);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_LaterRequiredDate()
		{
			TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatchingCore(isMatchingCriteria: false, earlierRequiredDate: false);
		}

		void TestGetPackagesToPutawayToConsolidationLocations_CriteriaMatchingCore(bool isMatchingCriteria, bool matchConsignee = true, bool matchLoad = true, bool matchTransportCo = true, bool matchCarrierServiceLevel = true, bool earlierRequiredDate = true)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02");
			var otherClient = Helper.CreateClient("C2");
			var now = ZDateTimeOffset.Now;

			var transportCo1 = Helper.CreateClient("X1", "X1");
			var service1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			service1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var service2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			service2.PL_Code = "SL2";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD1", data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;
			order1.ConsigneeAddressPK = data.Org1.MainAddress.PK;
			order1.WD_WLO_PlannedLoad = load1.PK;
			order1.TransportCoPK = transportCo1.PK;
			order1.WD_PL_NKCarrierServiceLevel = "SL1";
			order1.WD_RequiredDate = now.AddDays(5);

			var pick1 = Helper.CreatePickNew(order1);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(pick1.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response1, webService1);

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine1.WE_WL);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 10m);
			order2.WD_UseDirectedPackingConsolidation = true;
			order2.ConsigneeAddressPK = matchConsignee ? data.Org1.MainAddress.PK : otherClient.MainAddress.PK;
			order2.WD_WLO_PlannedLoad = matchLoad ? load1.PK : load2.PK;
			order2.TransportCoPK = matchTransportCo ? transportCo1.PK : transportCo2.PK;
			order2.WD_PL_NKCarrierServiceLevel = matchCarrierServiceLevel ? "SL1" : "SL2";
			order2.WD_RequiredDate = earlierRequiredDate ? now.AddDays(2) : now.AddDays(10);

			var pick2 = Helper.CreatePickNew(order2);

			var pickLine2 = order2.Lines[0].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			Factory.Save();

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pick2.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response2, webService2);

			var groupedPackagesToPutaway = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response2.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);

			AssertExpectedPackagesForAllocation(new[] { package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());

			if (isMatchingCriteria)
			{
				AssertEquals("Destination for transferLine should match order1.", consolidationLocation.PK, transferLine2.WE_WL);
				AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
				AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
				AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			}
			else
			{
				AssertEquals("Destination for transferLine should not match order1.", Guid.Empty, transferLine2.WE_WL);
				AssertEquals("", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
				AssertEquals("", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
				AssertEquals(Guid.Empty, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			}
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MultiOrderGroupingOnSinglePick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);

			var consolidationLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation3 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS3", 1, 1).Locations[0];
			consolidationLocation3.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02");
			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL03");
			var otherClient = Helper.CreateClient("C2");
			var now = ZDateTimeOffset.Now;

			var order1 = CreateOrder("1", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(3));
			var order2 = CreateOrder("2", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(3));
			var order3 = CreateOrder("3", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(2));
			var order4 = CreateOrder("4", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(2));
			var order5 = CreateOrder("5", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load3.PK, now.AddDays(5));
			var order6 = CreateOrder("6", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load3.PK, now.AddDays(5));
			var order7 = CreateOrder("7", data.Org1, data.Whs1, data.Org1.MainAddress.PK, Guid.Empty, now.AddDays(1), useDirectedConsolidation: false);

			var pick = Helper.CreatePickNew(order1, order2, order3, order4, order5, order6, order7);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var pickLine3 = order3.Lines[0].PickLines.Single();
			var pickLine4 = order4.Lines[0].PickLines.Single();
			var pickLine5 = order5.Lines[0].PickLines.Single();
			var pickLine6 = order6.Lines[0].PickLines.Single();
			var pickLine7 = order7.Lines[0].PickLines.Single();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);
			var transferLine5 = Helper.PickAndMakeInTransitTransfer(pickLine5, ZDateTimeOffset.Now);
			var transferLine6 = Helper.PickAndMakeInTransitTransfer(pickLine6, ZDateTimeOffset.Now);
			var transferLine7 = Helper.PickAndMakeInTransitTransfer(pickLine7, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine4.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine5.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine6.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine7.WE_WL);
			Factory.Save();

			CreatePackageForOrder("1", order1);
			CreatePackageForOrder("2", order2);
			CreatePackageForOrder("3", order3);
			CreatePackageForOrder("4", order4);
			CreatePackageForOrder("5", order5);
			CreatePackageForOrder("6", order6);
			CreatePackageForOrder("7", order7);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory need to be put away to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(3, groupedPackagesToPutaway.Length);

			AssertEquals("Packing Stations should be sorted.", "PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[0].Packages.Length);

			AssertEquals("Packing Stations should be sorted.", "PS2", groupedPackagesToPutaway[1].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[1].Packages.Length);

			AssertEquals("Packing Stations should be sorted.", "PS3", groupedPackagesToPutaway[2].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[2].Packages.Length);

			var pkgSet1 = string.Join("|", groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			var pkgSet2 = string.Join("|", groupedPackagesToPutaway[1].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			var pkgSet3 = string.Join("|", groupedPackagesToPutaway[2].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			AssertContainsExactElementsInAnyOrder(
				new[] { "PKG1|PKG2", "PKG3|PKG4", "PKG5|PKG6" },
				new[] { pkgSet1, pkgSet2, pkgSet3 });
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MultiOrderGroupingOnSinglePick_NoConsolidationLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02");
			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL03");
			var otherClient = Helper.CreateClient("C2");
			var now = ZDateTimeOffset.Now;

			var order1 = CreateOrder("1", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(3));
			var order2 = CreateOrder("2", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(3));
			var order3 = CreateOrder("3", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(2));
			var order4 = CreateOrder("4", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(2));
			var order5 = CreateOrder("5", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load3.PK, now.AddDays(5));
			var order6 = CreateOrder("6", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load3.PK, now.AddDays(5));
			var order7 = CreateOrder("7", data.Org1, data.Whs1, data.Org1.MainAddress.PK, Guid.Empty, now.AddDays(1), useDirectedConsolidation: false);

			var pick = Helper.CreatePickNew(order1, order2, order3, order4, order5, order6, order7);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var pickLine3 = order3.Lines[0].PickLines.Single();
			var pickLine4 = order4.Lines[0].PickLines.Single();
			var pickLine5 = order5.Lines[0].PickLines.Single();
			var pickLine6 = order6.Lines[0].PickLines.Single();
			var pickLine7 = order7.Lines[0].PickLines.Single();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);
			var transferLine5 = Helper.PickAndMakeInTransitTransfer(pickLine5, ZDateTimeOffset.Now);
			var transferLine6 = Helper.PickAndMakeInTransitTransfer(pickLine6, ZDateTimeOffset.Now);
			var transferLine7 = Helper.PickAndMakeInTransitTransfer(pickLine7, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine4.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine5.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine6.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine7.WE_WL);
			Factory.Save();

			CreatePackageForOrder("1", order1);
			CreatePackageForOrder("2", order2);
			CreatePackageForOrder("3", order3);
			CreatePackageForOrder("4", order4);
			CreatePackageForOrder("5", order5);
			CreatePackageForOrder("6", order6);
			CreatePackageForOrder("7", order7);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory need to be put away to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(3, groupedPackagesToPutaway.Length);

			AssertEquals("No Packing Station should be allocated.", "", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[0].Packages.Length);

			AssertEquals("No Packing Station should be allocated.", "", groupedPackagesToPutaway[1].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[1].Packages.Length);

			AssertEquals("No Packing Station should be allocated.", "", groupedPackagesToPutaway[2].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[2].Packages.Length);

			var pkgSet1 = string.Join("|", groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			var pkgSet2 = string.Join("|", groupedPackagesToPutaway[1].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			var pkgSet3 = string.Join("|", groupedPackagesToPutaway[2].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			AssertContainsExactElementsInAnyOrder(
				new[] { "PKG1|PKG2", "PKG3|PKG4", "PKG5|PKG6" },
				new[] { pkgSet1, pkgSet2, pkgSet3 });
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_AlwaysMatchAgainstLatestRequiredDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);

			var consolidationLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var otherClient = Helper.CreateClient("C2");
			var now = ZDateTimeOffset.Now;

			var order1 = CreateOrder("1", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load.PK, now.AddDays(1));
			var order2 = CreateOrder("2", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load.PK, now.AddDays(5));
			var pick1 = Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			Factory.Save();

			var package1 = CreatePackageForOrder("1", order1);
			var package2 = CreatePackageForOrder("2", order2);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(pick1.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response1, webService1);

			var groupedPackagesToPutaway1 = response1.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory does not need to be put away to Dock Door.", false, response1.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway1.Length);

			AssertExpectedPackagesForAllocation(new[] { package1, package2 }, groupedPackagesToPutaway1[0].Packages.Select(pkg => pkg.Package).ToArray());

			var order3 = CreateOrder("3", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load.PK, now.AddDays(3));
			var pick2 = Helper.CreatePickNew(order3);

			var pickLine3 = order3.Lines[0].PickLines.Single();
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var package3 = CreatePackageForOrder("3", order3);
			Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pick2.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response2, webService2);

			var groupedPackagesToPutaway2 = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory does not need to be put away to Dock Door.", false, response2.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway2.Length);
			AssertEquals("Should be allocated with existing packages.", "PS1", groupedPackagesToPutaway2[0].ConsolidationLocation.LocationString);

			AssertExpectedPackagesForAllocation(new[] { package3 }, groupedPackagesToPutaway2[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MultiOrderGroupingOnSinglePick_CriteriaMatchingAtLocationWithMultipleJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);

			var consolidationLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02");
			var otherClient = Helper.CreateClient("C2");
			var now = ZDateTimeOffset.Now;

			var transportCo1 = Helper.CreateClient("X1", "X1");
			var service1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			service1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var service2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			service2.PL_Code = "SL2";

			var order1 = CreateOrder("1", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(5));
			var order2 = CreateOrder("2", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(20));
			var order3 = CreateOrder("3", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(10));
			var order4 = CreateOrder("4", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(3));

			order1.TransportCoPK = transportCo1.PK;
			order1.WD_PL_NKCarrierServiceLevel = "SL1";
			order2.TransportCoPK = transportCo1.PK;
			order2.WD_PL_NKCarrierServiceLevel = "SL1";
			order3.TransportCoPK = transportCo2.PK;
			order3.WD_PL_NKCarrierServiceLevel = "SL2";
			order4.TransportCoPK = transportCo2.PK;
			order4.WD_PL_NKCarrierServiceLevel = "SL2";

			var pick = Helper.CreatePickNew(order1, order2, order3, order4);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var pickLine3 = order3.Lines[0].PickLines.Single();
			var pickLine4 = order4.Lines[0].PickLines.Single();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine4.WE_WL);
			Factory.Save();

			CreatePackageForOrder("1", order1);
			CreatePackageForOrder("2", order2);
			CreatePackageForOrder("3", order3);
			CreatePackageForOrder("4", order4);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory does not need to be put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(2, groupedPackagesToPutaway.Length);

			AssertEquals("Packing Stations should be sorted.", "PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[0].Packages.Length);

			AssertEquals("Packing Stations should be sorted.", "PS2", groupedPackagesToPutaway[1].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[1].Packages.Length);

			var pkgSet1 = string.Join("|", groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			var pkgSet2 = string.Join("|", groupedPackagesToPutaway[1].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			AssertContainsExactElementsInAnyOrder(
				new[] { "PKG1|PKG2", "PKG3|PKG4" },
				new[] { pkgSet1, pkgSet2 });
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MultiOrderGroupingOnMultiplePicks_CriteriaMatchingAtLocationWithMultipleJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);

			var consolidationLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02");
			var otherClient = Helper.CreateClient("C2");
			var now = ZDateTimeOffset.Now;
			var transportCo1 = Helper.CreateClient("X1", "X1");
			var service1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			service1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var service2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			service2.PL_Code = "SL2";

			var order1 = CreateOrder("1", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(5));
			var order2 = CreateOrder("2", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(20));
			var order3 = CreateOrder("3", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(10));
			order1.TransportCoPK = transportCo1.PK;
			order1.WD_PL_NKCarrierServiceLevel = "SL1";
			order2.TransportCoPK = transportCo1.PK;
			order2.WD_PL_NKCarrierServiceLevel = "SL1";
			order3.TransportCoPK = transportCo2.PK;
			order3.WD_PL_NKCarrierServiceLevel = "SL2";
			var pick = Helper.CreatePickNew(order1, order2, order3);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var pickLine3 = order3.Lines[0].PickLines.Single();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);

			Factory.Save();

			CreatePackageForOrder("1", order1);
			CreatePackageForOrder("2", order2);
			CreatePackageForOrder("3", order3);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory does not need to be put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(2, groupedPackagesToPutaway.Length);

			AssertEquals("Packing Stations should be sorted.", "PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[0].Packages.Length);

			AssertEquals("Packing Stations should be sorted.", "PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals(2, groupedPackagesToPutaway[0].Packages.Length);

			AssertEquals("Packing Stations should be sorted.", "PS2", groupedPackagesToPutaway[1].ConsolidationLocation.LocationString);
			AssertEquals(1, groupedPackagesToPutaway[1].Packages.Length);

			var pkgSet1 = string.Join("|", groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			var pkgSet2 = string.Join("|", groupedPackagesToPutaway[1].Packages.Select(pkg => pkg.Package.PackageID).OrderBy(pkg => pkg));
			AssertContainsExactElementsInAnyOrder(
				new[] { "PKG1|PKG2", "PKG3" },
				new[] { pkgSet1, pkgSet2 });

			var order4 = CreateOrder("4", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(3));
			order4.TransportCoPK = transportCo2.PK;
			order4.WD_PL_NKCarrierServiceLevel = "SL2";
			var pick2 = Helper.CreatePickNew(order4);
			var pickLine4 = order4.Lines[0].PickLines.Single();
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);
			Factory.Save();
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine4.WE_WL);
			CreatePackageForOrder("4", order4);
			Factory.Save();
			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pick2.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response2, webService2);

			var groupedPackagesToPutaway2 = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory does not need to be put away to Dock Door.", false, response2.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway2.Length);

			AssertEquals("Packing Stations should be sorted.", "PS2", groupedPackagesToPutaway2[0].ConsolidationLocation.LocationString);
			AssertEquals(1, groupedPackagesToPutaway2[0].Packages.Length);

			var pkgSetNew1 = groupedPackagesToPutaway2[0].Packages.Single().Package.PackageID;
			AssertContainsExactElementsInAnyOrder(new[] { "PKG4" }, new[] { pkgSetNew1 });
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MultiOrderGroupingOnSinglePick_SortedByPutawayPathSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var ps1Row = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 2);
			var consolidationLocation1 = ps1Row.Locations[0];
			var consolidationLocation2 = ps1Row.Locations[1];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation1.WLV_PutawayPathSequence = 2;
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation2.WLV_PutawayPathSequence = 1;

			var ps2Row = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1);
			ps2Row.WR_PickPathSequence = 1;

			var consolidationLocation3 = ps2Row.Locations[0];
			consolidationLocation3.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02");
			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL03");
			var otherClient1 = Helper.CreateClient("C2");
			var otherClient2 = Helper.CreateClient("C3");
			var now = ZDateTimeOffset.Now;

			var order1 = CreateOrder("1", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(5));
			var order2 = CreateOrder("2", data.Org1, data.Whs1, otherClient2.MainAddress.PK, load2.PK, now.AddDays(10));
			var order3 = CreateOrder("3", data.Org1, data.Whs1, otherClient2.MainAddress.PK, load3.PK, now.AddDays(20));

			var pick = Helper.CreatePickNew(order1, order2, order3);

			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var pickLine3 = order3.Lines[0].PickLines.Single();

			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);
			Factory.Save();

			CreatePackageForOrder("1", order1);
			CreatePackageForOrder("2", order2);
			CreatePackageForOrder("3", order3);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Inventory does not need to be put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(3, groupedPackagesToPutaway.Length);

			CombineAssertions(() =>
			{
				AssertEquals("Packing Stations should be sorted.", "PS2", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
				AssertEquals("Packing Stations should be sorted.", "PS1-1-2", groupedPackagesToPutaway[1].ConsolidationLocation.LocationString);
				AssertEquals("Packing Stations should be sorted.", "PS1-1-1", groupedPackagesToPutaway[2].ConsolidationLocation.LocationString);
			});
		}

		WhsOrder CreateOrder(string index, OrgHeader client, WhsWarehouse whs, ZGuid consigneeAddressPK, ZGuid loadPK, ZDateTimeOffset requiredDate, bool useDirectedConsolidation = true)
		{
			var part1 = Helper.CreateProduct("Part1-" + index, client);
			var part2 = Helper.CreateProduct("Part2-" + index, client);

			var receive = Helper.CreateWhsReceive(client, whs, "R" + index);
			Helper.CreateWhsReceiveLine(receive, part1, 10m);
			Helper.CreateWhsReceiveLine(receive, part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(client, whs, "ORD" + index);
			order.WD_UseDirectedPackingConsolidation = useDirectedConsolidation;
			order.ConsigneeAddressPK = consigneeAddressPK;
			order.WD_WLO_PlannedLoad = loadPK;
			order.WD_RequiredDate = requiredDate;

			Helper.CreateWhsOrderLine(order, part1, 10m);
			Helper.CreateWhsOrderLine(order, part2, 10m);

			Factory.Save();

			return order;
		}

		PkgPackage CreatePackageForOrder(string index, WhsOrder order)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG" + index, 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			package.Pack(order.Lines[1].ReleaseLines[0], 10m);

			return package;
		}

		#endregion

		#endregion

		#region TestGetPackagesToPutawayToConsolidationLocations_TrolleyPicking

		public void TestGetPackagesToPutawayToConsolidationLocations_TrolleyPicking_PickByCarton_UsingDirectedPackingConsolidation()
		{
			TestGetPackagesToPutawayToConsolidationLocations_TrolleyPickingCore(isPickByTote: false, useDirectedPackingConsolidation: true);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_TrolleyPicking_PickByCarton_NotUsingDirectedPackingConsolidation()
		{
			TestGetPackagesToPutawayToConsolidationLocations_TrolleyPickingCore(isPickByTote: false, useDirectedPackingConsolidation: false);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_TrolleyPicking_PickByTote_UsingDirectedPackingConsolidation()
		{
			TestGetPackagesToPutawayToConsolidationLocations_TrolleyPickingCore(isPickByTote: true, useDirectedPackingConsolidation: true);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_TrolleyPicking_PickByTote_NotUsingDirectedPackingConsolidation()
		{
			TestGetPackagesToPutawayToConsolidationLocations_TrolleyPickingCore(isPickByTote: true, useDirectedPackingConsolidation: false);
		}

		void TestGetPackagesToPutawayToConsolidationLocations_TrolleyPickingCore(bool isPickByTote, bool useDirectedPackingConsolidation)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 4; i++) // For 4 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			order1.WD_UseDirectedPackingConsolidation = useDirectedPackingConsolidation;
			var pick1 = Helper.CreatePickNew(order1);
			var pick1Lines = pick1.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pick1Lines.Length);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			order2.WD_UseDirectedPackingConsolidation = useDirectedPackingConsolidation;
			var pick2 = Helper.CreatePickNew(order2);
			var pick2Lines = pick2.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pick2Lines.Length);
			Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg_OnTrolley1 = PackingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pkg_NotOnTrolley = PackingHelper.CreatePackage(pkgJob1, "PKG2", 1, PkgUnit.Box);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg_OnTrolley2 = PackingHelper.CreatePackage(pkgJob2, "PKG3", 1, PkgUnit.Box);
			var pkg_OnTrolley_NotPicked = PackingHelper.CreatePackage(pkgJob2, "PKG4", 1, PkgUnit.Box);

			pkg_OnTrolley1.SetIsTote(isPickByTote);
			pkg_OnTrolley2.SetIsTote(isPickByTote);
			pkg_NotOnTrolley.SetIsTote(isPickByTote);
			pkg_OnTrolley_NotPicked.SetIsTote(isPickByTote);

			PackingHelper.CreatePackageDivot(pkg_OnTrolley1, pick1Lines[0]);
			PackingHelper.CreatePackageDivot(pkg_NotOnTrolley, pick1Lines[1]);

			PackingHelper.CreatePackageDivot(pkg_OnTrolley2, pick2Lines[0]);
			PackingHelper.CreatePackageDivot(pkg_OnTrolley_NotPicked, pick2Lines[1]);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley_NotPicked, 3);
			Factory.Save();

			pick1Lines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			pick2Lines.Except(new[] { pick2Lines[1] }).ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.GetPackagesToPutawayToConsolidationLocations(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferLine1 = pick1Lines[0].InventoryLine as WhsTransferLine;
			var transferLine2 = pick2Lines[0].InventoryLine as WhsTransferLine;

			var transferLine3_NotOnTrolley = pick1Lines[1].InventoryLine as WhsTransferLine;
			AssertEquals(false, pick2Lines[1].IsPickedFromPutawayLocation);

			if (useDirectedPackingConsolidation && !isPickByTote)
			{
				AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine1.WE_WL);
				AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine2.WE_WL);
				AssertEquals("Destination for transferLine should not be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3_NotOnTrolley.WE_WL);

				var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
				AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
				AssertEquals(1, groupedPackagesToPutaway.Length);
				AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
				AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
				AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
				AssertEquals(false, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);

				AssertExpectedPackagesForAllocation(new[] { pkg_OnTrolley1, pkg_OnTrolley2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
			}
			else
			{
				AssertEquals("Destination for transferLine should not be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
				AssertEquals("Destination for transferLine should not be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
				AssertEquals("Destination for transferLine should not be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3_NotOnTrolley.WE_WL);

				var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
				AssertEquals("No inventory to put away to Dock Door.", true, response.RequiresDockDoorPutaway);
				AssertEquals(0, groupedPackagesToPutaway.Length);
			}
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_TrolleyPicking_PickWithLooseInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 10; i++) // For 10 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			order1.WD_UseDirectedPackingConsolidation = true;
			var pick1 = Helper.CreatePickNew(order1);
			var pick1Lines = pick1.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 5 pick lines.", 2, pick1Lines.Length);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			order2.WD_UseDirectedPackingConsolidation = true;
			var pick2 = Helper.CreatePickNew(order2);
			var pick2Lines = pick2.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 5 pick lines.", 5, pick2Lines.Length);
			Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg_OnTrolley1 = PackingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pkg_OnTrolley2 = PackingHelper.CreatePackage(pkgJob1, "PKG2", 1, PkgUnit.Box);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg_OnTrolley3 = PackingHelper.CreatePackage(pkgJob2, "PKG3", 1, PkgUnit.Box);
			var pkg_OnTrolley4 = PackingHelper.CreatePackage(pkgJob2, "PKG4", 1, PkgUnit.Box);

			PackingHelper.CreatePackageDivot(pkg_OnTrolley1, pick1Lines[0]);
			PackingHelper.CreatePackageDivot(pkg_OnTrolley2, pick1Lines[1]);

			PackingHelper.CreatePackageDivot(pkg_OnTrolley3, pick2Lines[0]);
			PackingHelper.CreatePackageDivot(pkg_OnTrolley4, pick2Lines[1]);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley2, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley3, 3);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley4, 4);
			Factory.Save();

			pick1Lines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			pick2Lines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.GetPackagesToPutawayToConsolidationLocations(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Loose inventory pick packages need to put away to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(false, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);

			AssertExpectedPackagesForAllocation(new[] { pkg_OnTrolley1, pkg_OnTrolley2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		#endregion

		#region TestGetPackagesToPutawayToConsolidationLocations_PickByLabel

		public void TestGetPackagesToPutawayToConsolidationLocations_PickByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 3; i++) // For 3 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 3m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 3 pick lines.", 3, pick.GetAllPickLines().Count());
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			var otherPackage = PackingHelper.CreatePackage(pkgJob, "PKG3", 1, PkgUnit.Box);
			PackingHelper.CreatePackageDivot(package1, pickLines[0]);
			PackingHelper.CreatePackageDivot(package2, pickLines[1]);
			PackingHelper.CreatePackageDivot(otherPackage, pickLines[2]);
			Factory.Save();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferLine1 = pickLines[0].InventoryLine as WhsTransferLine;
			var transferLine2 = pickLines[1].InventoryLine as WhsTransferLine;
			var transferLine3_NotOnJob = pickLines[2].InventoryLine as WhsTransferLine;

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine2.WE_WL);
			AssertEquals("Destination for transferLine should not be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3_NotOnJob.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(false, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);

			AssertExpectedPackagesForAllocation(new[] { package1, package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PickByLabel_MultiOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 3; i++) // For 3 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order1.WD_UseDirectedPackingConsolidation = true;

			var pick1 = Helper.CreatePickNew(order1);
			var pickLines1 = pick1.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines1.Length);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			order2.WD_UseDirectedPackingConsolidation = true;

			var pick2 = Helper.CreatePickNew(order2);
			var pickLines2 = pick2.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines2.Length);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 1m);
			order3.WD_UseDirectedPackingConsolidation = true;

			var pick3 = Helper.CreatePickNew(order3);
			var pickLines3 = pick3.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines3.Length);

			Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var package3 = PackingHelper.CreatePackage(pkgJob3, "PKG3", 1, PkgUnit.Box);
			PackingHelper.CreatePackageDivot(package1, pickLines1[0]);
			PackingHelper.CreatePackageDivot(package2, pickLines2[0]);
			PackingHelper.CreatePackageDivot(package3, pickLines3[0]);
			Factory.Save();

			pickLines1.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			pickLines2.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package3.PK);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferLine1 = pickLines1[0].InventoryLine as WhsTransferLine;
			var transferLine2 = pickLines2[0].InventoryLine as WhsTransferLine;
			AssertEquals(false, pickLines3[0].IsPickedFromPutawayLocation);

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine2.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(false, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);

			AssertExpectedPackagesForAllocation(new[] { package1, package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PickByLabel_MultiOrder_DifferentCriteria()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var consignee1 = Helper.CreateClient("CON1");
			var consignee2 = Helper.CreateClient("CON2");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order1.ConsigneeAddressPK = consignee1.MainAddress.PK;
			order1.WD_UseDirectedPackingConsolidation = true;

			var pick1 = Helper.CreatePickNew(order1);
			var pickLines1 = pick1.GetAllPickLines().ToArray();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 5m);
			order2.ConsigneeAddressPK = consignee2.MainAddress.PK;
			order2.WD_UseDirectedPackingConsolidation = true;

			var pick2 = Helper.CreatePickNew(order2);
			var pickLines2 = pick2.GetAllPickLines().ToArray();

			Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);

			PackingHelper.CreatePackageDivot(package1, pickLines1[0]);
			PackingHelper.CreatePackageDivot(package2, pickLines2[0]);
			Factory.Save();

			pickLines1.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			pickLines2.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(2, groupedPackagesToPutaway.Length);

			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(1, groupedPackagesToPutaway[0].Packages.Length);

			AssertEquals("", groupedPackagesToPutaway[1].ConsolidationLocation.LocationString);
			AssertEquals("", groupedPackagesToPutaway[1].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(Guid.Empty, groupedPackagesToPutaway[1].ConsolidationLocation.LocationPK);
			AssertEquals(1, groupedPackagesToPutaway[1].Packages.Length);

			AssertContainsExactElementsInAnyOrder(new[] { package1.PK, package2.PK }, new[] { groupedPackagesToPutaway[0].Packages[0].Package.PK, groupedPackagesToPutaway[1].Packages[0].Package.PK });
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PickByLabel_PickWithLooseInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 10; i++) // For 10 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 10 pick lines.", 10, pick.GetAllPickLines().Count());
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			PackingHelper.CreatePackageDivot(package1, pickLines[0]);
			Factory.Save();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Need to put away packages to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(0, groupedPackagesToPutaway.Length);
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PickByLabel_PickWithLooseInventory_MultiOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 3; i++) // For 3 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order1.WD_UseDirectedPackingConsolidation = true;

			var pick1 = Helper.CreatePickNew(order1);
			var pickLines1 = pick1.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 1 pick line.", 1, pickLines1.Length);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			order2.WD_UseDirectedPackingConsolidation = true;

			var pick2 = Helper.CreatePickNew(order2);
			var pickLines2 = pick2.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pickLines2.Length);

			Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package2 = PackingHelper.CreatePackage(pkgJob2, "PKG2", 1, PkgUnit.Box);
			PackingHelper.CreatePackageDivot(package1, pickLines1[0]);
			PackingHelper.CreatePackageDivot(package2, pickLines2[0]);
			Factory.Save();

			pickLines1.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			pickLines2.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);

			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.SecurityHeader.UserName = GlbStaff.CurrentUser.GS_LoginName;
			var response = webService.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferLine1 = pickLines1[0].InventoryLine as WhsTransferLine;
			var transferLine2 = pickLines2[0].InventoryLine as WhsTransferLine;

			AssertEquals("Destination for transferLine should be updated.", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("Destination for transferLine should be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("Loose inventory needs to be put away to Dock Door.", true, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(false, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);

			AssertExpectedPackagesForAllocation(new[] { package1 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		#endregion

		#region DBHits

		public void TestGetPackagesToPutawayToConsolidationLocations_MultiOrderOnSinglePick_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var ps1Row = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 4);
			var consolidationLocation1 = ps1Row.Locations[0];
			var consolidationLocation2 = ps1Row.Locations[0];
			var consolidationLocation3 = ps1Row.Locations[0];
			var consolidationLocation4 = ps1Row.Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation3.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation4.WLV_WLT_LocationType = consolidationLocationType.PK;

			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02");
			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL03");
			var otherClient = Helper.CreateClient("C2");
			var now = ZDateTimeOffset.Now;

			var order0 = CreateOrder("0", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(3));
			var order1 = CreateOrder("1", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(3));
			var order2 = CreateOrder("2", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load1.PK, now.AddDays(3));
			var order3 = CreateOrder("3", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(2));
			var order4 = CreateOrder("4", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(2));
			var order5 = CreateOrder("5", data.Org1, data.Whs1, otherClient.MainAddress.PK, load2.PK, now.AddDays(2));
			var order6 = CreateOrder("6", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load3.PK, now.AddDays(5));
			var order7 = CreateOrder("7", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load3.PK, now.AddDays(5));
			var order8 = CreateOrder("8", data.Org1, data.Whs1, data.Org1.MainAddress.PK, load3.PK, now.AddDays(5));
			var order9 = CreateOrder("9", data.Org1, data.Whs1, data.Org1.MainAddress.PK, Guid.Empty, now.AddDays(1));

			var pick = Helper.CreatePickNew(order0, order1, order2, order3, order4, order5, order6, order7, order8, order9);

			var pickLine0 = order0.Lines[0].PickLines.Single();
			var pickLine1 = order1.Lines[0].PickLines.Single();
			var pickLine2 = order2.Lines[0].PickLines.Single();
			var pickLine3 = order3.Lines[0].PickLines.Single();
			var pickLine4 = order4.Lines[0].PickLines.Single();
			var pickLine5 = order5.Lines[0].PickLines.Single();
			var pickLine6 = order6.Lines[0].PickLines.Single();
			var pickLine7 = order7.Lines[0].PickLines.Single();
			var pickLine8 = order8.Lines[0].PickLines.Single();
			var pickLine9 = order9.Lines[0].PickLines.Single();

			var transferLine0 = Helper.PickAndMakeInTransitTransfer(pickLine0, ZDateTimeOffset.Now);
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, ZDateTimeOffset.Now);
			var transferLine5 = Helper.PickAndMakeInTransitTransfer(pickLine5, ZDateTimeOffset.Now);
			var transferLine6 = Helper.PickAndMakeInTransitTransfer(pickLine6, ZDateTimeOffset.Now);
			var transferLine7 = Helper.PickAndMakeInTransitTransfer(pickLine7, ZDateTimeOffset.Now);
			var transferLine8 = Helper.PickAndMakeInTransitTransfer(pickLine8, ZDateTimeOffset.Now);
			var transferLine9 = Helper.PickAndMakeInTransitTransfer(pickLine9, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine0.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine4.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine5.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine6.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine7.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine8.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine9.WE_WL);
			Factory.Save();

			CreatePackageForOrder("0", order0);
			CreatePackageForOrder("1", order1);
			CreatePackageForOrder("2", order2);
			CreatePackageForOrder("3", order3);
			CreatePackageForOrder("4", order4);
			CreatePackageForOrder("5", order5);
			CreatePackageForOrder("6", order6);
			CreatePackageForOrder("7", order7);
			CreatePackageForOrder("8", order8);
			CreatePackageForOrder("9", order9);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketLineSchema.Constants.TableName, 6 }, // all fetch hinted queries
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 6 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_MultiPackageOnTrolleyPick_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var ps1Row = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 4);
			var consolidationLocation1 = ps1Row.Locations[0];
			var consolidationLocation2 = ps1Row.Locations[0];
			var consolidationLocation3 = ps1Row.Locations[0];
			var consolidationLocation4 = ps1Row.Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation3.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation4.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 10; i++) // For 10 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);

			CreateOrderWithTwoPackagesForTrolleyPicking(trolleyJob, 1);
			CreateOrderWithTwoPackagesForTrolleyPicking(trolleyJob, 2);
			CreateOrderWithTwoPackagesForTrolleyPicking(trolleyJob, 3);
			CreateOrderWithTwoPackagesForTrolleyPicking(trolleyJob, 4);
			CreateOrderWithTwoPackagesForTrolleyPicking(trolleyJob, 5);

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 6 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.GetPackagesToPutawayToConsolidationLocations(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			void CreateOrderWithTwoPackagesForTrolleyPicking(WhsPickTrolleyJob job, int index)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + index, data.Part1, 2m);
				order.WD_UseDirectedPackingConsolidation = true;
				var pick = Helper.CreatePickNew(order);
				var pickLines = pick.GetAllPickLines().ToArray();

				Factory.Save();

				var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var pkg1 = PackingHelper.CreatePackage(pkgJob, "PKG1-" + index, 1, PkgUnit.Box);
				var pkg2 = PackingHelper.CreatePackage(pkgJob, "PKG2-" + index, 1, PkgUnit.Box);

				PackingHelper.CreatePackageDivot(pkg1, pickLines[0]);
				PackingHelper.CreatePackageDivot(pkg2, pickLines[1]);

				var slotToUse = index * 2;
				Helper.CreateWhsPickTrolleySlot(job, pkg1, (ZShort)slotToUse);
				Helper.CreateWhsPickTrolleySlot(job, pkg2, (ZShort)slotToUse + 1);

				pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
				Factory.Save();
			}
		}

		[TestDate(2017, 10, 03)]
		public void TestGetPackagesToPutawayToConsolidationLocations_MultiPackageOnPickByLabelJob_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var ps1Row = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 4);
			var consolidationLocation1 = ps1Row.Locations[0];
			var consolidationLocation2 = ps1Row.Locations[0];
			var consolidationLocation3 = ps1Row.Locations[0];
			var consolidationLocation4 = ps1Row.Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation3.WLV_WLT_LocationType = consolidationLocationType.PK;
			consolidationLocation4.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			for (var i = 0; i < 10; i++) // For 10 pick lines later on
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var package0 = CreatePackageForPickByLabelPicking(0);
			var package1 = CreatePackageForPickByLabelPicking(1);
			var package2 = CreatePackageForPickByLabelPicking(2);
			var package3 = CreatePackageForPickByLabelPicking(3);
			var package4 = CreatePackageForPickByLabelPicking(4);
			var package5 = CreatePackageForPickByLabelPicking(5);
			var package6 = CreatePackageForPickByLabelPicking(6);
			var package7 = CreatePackageForPickByLabelPicking(7);
			var package8 = CreatePackageForPickByLabelPicking(8);
			var package9 = CreatePackageForPickByLabelPicking(9);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package0.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package2.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package3.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package4.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package5.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package6.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package7.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package8.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, GlbStaff.CurrentUser.GS_Code, package9.PK);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 6 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			PkgPackage CreatePackageForPickByLabelPicking(int index)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + index, data.Part1, 1m);
				order.WD_UseDirectedPackingConsolidation = true;
				var pick = Helper.CreatePickNew(order);
				var pickLines = pick.GetAllPickLines().ToArray();
				Factory.Save();

				var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var pkg = PackingHelper.CreatePackage(pkgJob, "PKG1-" + index, 1, PkgUnit.Box);
				PackingHelper.CreatePackageDivot(pkg, pickLines[0]);
				Factory.Save();

				foreach (var pickLine in pickLines)
				{
					pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
				}
				Factory.Save();

				return pkg;
			}
		}

		#endregion

		#region TestGetPackagesToPutawayToConsolidationLocations_PackagesAssignedToDifferentUsers

		public void TestGetPackagesToPutawayToConsolidationLocations_PackagesAssignedToDifferentUsers()
		{
			var user1 = Helper.CreateGlbStaff("AAA", "AAAAA");
			var user2 = Helper.CreateGlbStaff("BBB", "BBBBB");
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			pickLine1.WZ_GS_NKAssignedTo = "AAA";
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var pickLine2 = orderLine2.PickLines.Single();
			pickLine2.WZ_GS_NKAssignedTo = "BBB";
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", "AAA", transferLine1.WE_GS_NKPutawayBy);
			AssertEquals("Precondition", "BBB", transferLine2.WE_GS_NKPutawayBy);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);
			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff: user1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response1, webService1);

			var groupedPackagesToPutaway1 = response1.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals(1, groupedPackagesToPutaway1.Length);
			AssertExpectedPackagesForAllocation(new[] { package1 }, groupedPackagesToPutaway1[0].Packages.Select(pkg => pkg.Package).ToArray());

			var webService2 = GetNewWebService(data.Whs1, staff: user2);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pick.PK.ToGuid(), PickJobType.Pick);
			AssertSuccessfulResponseWithNoErrors(response2, webService2);

			var groupedPackagesToPutaway2 = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals(1, groupedPackagesToPutaway2.Length);
			AssertExpectedPackagesForAllocation(new[] { package2 }, groupedPackagesToPutaway2[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PackagesAssignedToDifferentUsers_PickByLabel()
		{
			var user1 = Helper.CreateGlbStaff("AAA", "AAAAA");
			var user2 = Helper.CreateGlbStaff("BBB", "BBBBB");
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pick.GetAllPickLines().Count());
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			PackingHelper.CreatePackageDivot(package1, pickLines[0]);
			PackingHelper.CreatePackageDivot(package2, pickLines[1]);
			Factory.Save();

			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			pickLines[0].WZ_GS_NKAssignedTo = "AAA";
			pickLines[1].WZ_GS_NKAssignedTo = "BBB";
			Factory.Save();

			var pickByLabelJob1 = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);
			var pickByLabelJob2 = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff: user1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob);
			AssertSuccessfulResponseWithNoErrors(response1, webService1);

			var groupedPackagesToPutaway1 = response1.PackagesToConsolidationLocationGroupingInfos;
			AssertExpectedPackagesForAllocation(new[] { package1 }, groupedPackagesToPutaway1[0].Packages.Select(pkg => pkg.Package).ToArray());

			var webService2 = GetNewWebService(data.Whs1, staff: user2);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(pickByLabelJob2.PK.ToGuid(), PickJobType.PickByLabelJob);
			AssertSuccessfulResponseWithNoErrors(response2, webService2);

			var groupedPackagesToPutaway2 = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertExpectedPackagesForAllocation(new[] { package2 }, groupedPackagesToPutaway2[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPackagesToPutawayToConsolidationLocations_PackagesAssignedToDifferentUsers_TrolleyPicking_PickByCarton()
		{
			var user1 = Helper.CreateGlbStaff("AAA", "AAAAA");
			var user2 = Helper.CreateGlbStaff("BBB", "BBBBB");
			var data = new TestDataSimpleEnvironment(Factory);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			var pickLines = pick.GetAllPickLines().ToArray();
			AssertEquals("Precondition: 2 pick lines.", 2, pickLines.Length);
			Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg_OnTrolley1 = PackingHelper.CreatePackage(pkgJob1, "PKG1", 1, PkgUnit.Box);
			var pkg_OnTrolley2 = PackingHelper.CreatePackage(pkgJob1, "PKG2", 1, PkgUnit.Box);

			PackingHelper.CreatePackageDivot(pkg_OnTrolley1, pickLines[0]);
			PackingHelper.CreatePackageDivot(pkg_OnTrolley2, pickLines[1]);

			var trolley1 = Helper.CreateTrolley("T001");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1, pkg_OnTrolley1, 1);

			var trolley2 = Helper.CreateTrolley("T002");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2, pkg_OnTrolley2, 1);
			Factory.Save();

			pickLines.ForEach(pl => pl.WZ_PickedDateTime = ZDateTimeOffset.Today);
			pickLines[0].WZ_GS_NKAssignedTo = "AAA";
			pickLines[1].WZ_GS_NKAssignedTo = "BBB";
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff: user1);
			var response1 = webService1.GetPackagesToPutawayToConsolidationLocations(trolleyJob1.PK.ToGuid(), PickJobType.TrolleyJob);
			AssertSuccessfulResponseWithNoErrors(response1, webService1);
			var groupedPackagesToPutaway1 = response1.PackagesToConsolidationLocationGroupingInfos;
			AssertExpectedPackagesForAllocation(new[] { pkg_OnTrolley1 }, groupedPackagesToPutaway1[0].Packages.Select(pkg => pkg.Package).ToArray());

			var webService2 = GetNewWebService(data.Whs1, staff: user2);
			var response2 = webService2.GetPackagesToPutawayToConsolidationLocations(trolleyJob2.PK.ToGuid(), PickJobType.TrolleyJob);
			AssertSuccessfulResponseWithNoErrors(response2, webService2);
			var groupedPackagesToPutaway2 = response2.PackagesToConsolidationLocationGroupingInfos;
			AssertExpectedPackagesForAllocation(new[] { pkg_OnTrolley2 }, groupedPackagesToPutaway2[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		#endregion

		#region AssertExpectedPackagesForAllocation

		void AssertExpectedPackagesForAllocation(PkgPackage[] expectedPackages, PackageInfo[] actualPackageInfos)
		{
			AssertEquals("Packages Count should match.", expectedPackages.Length, actualPackageInfos.Length);

			var expectedPackagePKs = string.Join("|", expectedPackages.Select(p => p.PK));
			var expectedPackageIDs = string.Join("|", expectedPackages.Select(p => p.KP_PackageID));

			AssertEquals("Package PKs should match.", expectedPackagePKs, string.Join("|", actualPackageInfos.Select(p => p.PK)));
			AssertEquals("Package IDs should match.", expectedPackageIDs, string.Join("|", actualPackageInfos.Select(p => p.PackageID)));
		}

		#endregion

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}

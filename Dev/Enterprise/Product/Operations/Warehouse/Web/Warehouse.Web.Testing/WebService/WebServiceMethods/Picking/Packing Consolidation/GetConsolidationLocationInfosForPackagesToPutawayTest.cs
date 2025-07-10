using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetConsolidationLocationInfosForPackagesToPutawayTest : WhsSecureServiceTestCase
	{
		public void TestGetPutawayPackagesToConsolidationLocationInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 2, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { package1.PK.ToGuid(), package2.PK.ToGuid() }, order.WD_DocketID);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(4, transferInNewFactory.Lines.Count);

			var newTransferLinePart1 = transfer.Lines.Single(l => l.PK != transferLine1.PK && l.WE_OP == data.Part1.PK);
			AssertEquals(nameof(newTransferLinePart1.WE_TransactionQuantity), 10m, newTransferLinePart1.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLinePart1.WE_WL), consolidationLocation.PK, newTransferLinePart1.WE_WL);

			var newTransferLinePart2 = transfer.Lines.Single(l => l.PK != transferLine2.PK && l.WE_OP == data.Part2.PK);
			AssertEquals(nameof(newTransferLinePart2.WE_TransactionQuantity), 15m, newTransferLinePart2.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLinePart2.WE_WL), consolidationLocation.PK, newTransferLinePart2.WE_WL);

			var groupedPackagesToPutaway = response.PackagesToConsolidationLocationGroupingInfos;
			AssertEquals("No inventory to put away to Dock Door.", false, response.RequiresDockDoorPutaway);
			AssertEquals(1, groupedPackagesToPutaway.Length);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString);
			AssertEquals("PS1", groupedPackagesToPutaway[0].ConsolidationLocation.LocationString_UserFriendly);
			AssertEquals(consolidationLocation.PK, groupedPackagesToPutaway[0].ConsolidationLocation.LocationPK);
			AssertEquals(false, groupedPackagesToPutaway[0].Packages[0].CannotOverrideConsolidationLocation);
			AssertEquals(order.PK, groupedPackagesToPutaway[0].Packages[0].OrderPK);

			AssertExpectedPackagesForAllocation(new[] { package1, package2 }, groupedPackagesToPutaway[0].Packages.Select(pkg => pkg.Package).ToArray());
		}

		public void TestGetPutawayPackagesToConsolidationLocationInfo_WithAnotherPackageOnAConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation2.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 2, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { package2.PK.ToGuid() }, order.WD_DocketID);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(3, transferInNewFactory.Lines.Count);

			var newTransferLine = transfer.Lines.Single(l => l.PK != transferLine2.PK && l.WE_OP == data.Part2.PK);
			AssertEquals(nameof(newTransferLine.WE_TransactionQuantity), 15m, newTransferLine.WE_TransactionQuantity);
			AssertEquals(nameof(newTransferLine.WE_WL), consolidationLocation2.PK, newTransferLine.WE_WL);

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

		public void TestGetPutawayPackagesToConsolidationLocationInfo_WithExistingOutboundTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = consolidationLocation.PK;
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 1, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { package.PK.ToGuid() }, order.WD_DocketID);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(1, transferInNewFactory.Lines.Count);

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

		public void TestGetPutawayPackagesToConsolidationLocationInfo_WithExistingOutboundTransferLine_TransferToDockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			AssertEquals("Precondition: Outbound Transfer to dockdoor", transferLine.WE_WL, data.Whs1.DefaultInboundDockDoorLocation.PK);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 1, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { package.PK.ToGuid() }, order.WD_DocketID);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(1, transferInNewFactory.Lines.Count);

			var transferLineInNewFactory = transferInNewFactory.Lines[0];
			AssertEquals(transferLine.WE_WL, consolidationLocation.PK);

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

		public void TestGetPutawayPackagesToConsolidationLocationInfo_OrderDoesNotExist()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var errorResponse = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { Guid.NewGuid() }, "SomeID");
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("No Warehouse Order found with Docket ID: SomeID.", errorResponse.ErrorMessage);
		}

		public void TestGetPutawayPackagesToConsolidationLocationInfo_PackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var errorResponse = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { package.PK.ToGuid(), Guid.NewGuid() }, order.WD_DocketID);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("Some Package(s) were not found in Warehouse. 2 were expected, but 1 packages were found.", errorResponse.ErrorMessage);
		}

		public void TestGetPutawayPackagesToConsolidationLocationInfo_PickLineAlreadyPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			
			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			pickLine.WZ_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var errorResponse = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { package.PK.ToGuid() }, order.WD_DocketID);
			AssertEquals("Should show an error.", ErrorTypes.BusinessValidationError, errorResponse.Error);
			AssertEquals("Some Pick Lines on the package have been picked already or is not picking from a packing station location.", errorResponse.ErrorMessage);
		}

		public void TestGetPutawayPackagesToConsolidationLocationInfo_ConcurrencyException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine1.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 1, transfer.Lines.Count);

			var webService = GetNewWebService(data.Whs1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)package).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Factory);

			var response = webService.GetConsolidationLocationInfosForPackagesToPutaway(new[] { package.PK.ToGuid() }, order.WD_DocketID);
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Another user has made changes while you have been working on the packages. Please restart the operation and try again.", response.ErrorMessage);

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals("No new oubound transfer lines", 1, transferInNewFactory.Lines.Count);
		}

		public void TestGetPutawayPackagesToConsolidationLocationInfo_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct($"P1{i}", data.Org1);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", product, 10m);
				products.Add(product);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLines = new List<WhsOrderLine>();
			for (var i = 0; i < 10; i++)
			{
				var product = products[i];
				var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
				orderLines.Add(orderLine);
			}
			var pick = Helper.CreatePickNew(order);

			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packages = new List<PkgPackage>();
			foreach (var orderLine in orderLines)
			{
				var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
				package.Pack(orderLine.ReleaseLines[0], 10m);
				packages.Add(package);
			}
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			AssertEquals("Precondition: Existing transfer line.", 10, transfer.Lines.Count);

			var expectedDBHits = new Dictionary<string, int>
			{
				{ CusClassPartPivotSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 4 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 4 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 4 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService(data.Whs1);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.GetConsolidationLocationInfosForPackagesToPutaway(packages.Select(p => p.PK.ToGuid()).ToArray(), order.WD_DocketID);
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);
			}

			var transferInNewFactory = new BusinessObjectFactory().Load<WhsTransfer>(transfer.PK);
			AssertEquals(20, transferInNewFactory.Lines.Count);
		}

		#region Implementation

		void AssertExpectedPackagesForAllocation(PkgPackage[] expectedPackages, PackageInfo[] actualPackageInfos)
		{
			AssertEquals("Packages Count should match.", expectedPackages.Length, actualPackageInfos.Length);

			var expectedPackagePKs = string.Join("|", expectedPackages.Select(p => p.PK));
			var expectedPackageIDs = string.Join("|", expectedPackages.Select(p => p.KP_PackageID));

			AssertEquals("Package PKs should match.", expectedPackagePKs, string.Join("|", actualPackageInfos.Select(p => p.PK)));
			AssertEquals("Package IDs should match.", expectedPackageIDs, string.Join("|", actualPackageInfos.Select(p => p.PackageID)));
		}

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}

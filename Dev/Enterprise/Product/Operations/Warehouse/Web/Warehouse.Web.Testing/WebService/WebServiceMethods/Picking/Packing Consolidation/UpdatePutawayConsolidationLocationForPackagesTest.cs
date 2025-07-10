using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class UpdatePutawayConsolidationLocationForPackagesTest : WhsSecureServiceTestCase
	{
		#region TestPackageNotFound

		public void TestPackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { Guid.NewGuid() }, consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Some Package(s) were not found in Warehouse. 1 were expected, but 0 packages were found.", response);
		}

		public void TestPackageNotFound_PartialMatches()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package.PK.ToGuid(), Guid.NewGuid() }, consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Some Package(s) were not found in Warehouse. 2 were expected, but 1 packages were found.", response);
		}

		#endregion

		#region LocationNotFound

		public void TestLocationNotFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package.PK.ToGuid() }, Guid.NewGuid());
			AssertBusinessValidationError(webService, "Packing Consolidation could not be found.", response);
		}

		public void TestLocationNotFound_IsNotAPackingConsolidation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationType = Helper.CreateLocationType("NPK", "Not-Packing", false, 0, LocationClasses.Codes.NOR);
			var location = Helper.CreateRowAndGenerateLocations(data.Whs1, "NP1", 1, 1).Locations[0];
			location.WLV_WLT_LocationType = locationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package.PK.ToGuid() }, location.PK.ToGuid());
			AssertBusinessValidationError(webService, "Packing Consolidation could not be found.", response);
		}

		#endregion

		#region TestRejectsPackages

		public void TestRejectsPackages_WithUnpickedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var pickLine2 = orderLine2.PickLines.Single();

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot update partially processed packages.", response);
		}

		public void TestRejectsPackages_WithFinalisedLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot update partially processed packages.", response);
		}

		#endregion

		#region TestUpdatePutawayConsolidationLocationForPackages_DBHits

		public void TestUpdatePutawayConsolidationLocationForPackages_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order0 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O0");
			var orderLine0 = Helper.CreateWhsOrderLine(order0, data.Part1, 1m);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 1m);
			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			var orderLine4 = Helper.CreateWhsOrderLine(order4, data.Part1, 1m);
			var order5 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O5");
			var orderLine5 = Helper.CreateWhsOrderLine(order5, data.Part1, 1m);
			var order6 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O6");
			var orderLine6 = Helper.CreateWhsOrderLine(order6, data.Part1, 1m);
			var orderLine7 = Helper.CreateWhsOrderLine(order6, data.Part1, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order6, data.Part1, 1m);
			var orderLine9 = Helper.CreateWhsOrderLine(order6, data.Part1, 1m);
			Helper.CreatePickNew(order0, order1, order2, order3, order4, order5, order6);

			var pickLine0 = orderLine0.PickLines.Single();
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			var pickLine3 = orderLine3.PickLines.Single();
			var pickLine4 = orderLine4.PickLines.Single();
			var pickLine5 = orderLine5.PickLines.Single();
			var pickLine6 = orderLine6.PickLines.Single();
			var pickLine7 = orderLine7.PickLines.Single();
			var pickLine8 = orderLine8.PickLines.Single();
			var pickLine9 = orderLine9.PickLines.Single();

			var now = ZDateTimeOffset.Now;
			Helper.PickAndMakeInTransitTransfer(pickLine0, now);
			Helper.PickAndMakeInTransitTransfer(pickLine1, now);
			Helper.PickAndMakeInTransitTransfer(pickLine2, now);
			Helper.PickAndMakeInTransitTransfer(pickLine3, now);
			Helper.PickAndMakeInTransitTransfer(pickLine4, now);
			Helper.PickAndMakeInTransitTransfer(pickLine5, now);
			Helper.PickAndMakeInTransitTransfer(pickLine6, now);
			Helper.PickAndMakeInTransitTransfer(pickLine7, now);
			Helper.PickAndMakeInTransitTransfer(pickLine8, now);
			Helper.PickAndMakeInTransitTransfer(pickLine9, now);

			Factory.Save();

			var package1 = CreatePackage(new[] { orderLine1.ReleaseLines[0] }, 1, order1);
			var package2 = CreatePackage(new[] { orderLine2.ReleaseLines[0] }, 2, order2);
			var package3 = CreatePackage(new[] { orderLine3.ReleaseLines[0] }, 3, order3);
			var package4 = CreatePackage(new[] { orderLine4.ReleaseLines[0] }, 4, order4);
			var package5 = CreatePackage(new[] { orderLine5.ReleaseLines[0] }, 5, order5);
			var package6 = CreatePackage(new[] { orderLine6.ReleaseLines[0], orderLine7.ReleaseLines[0], orderLine8.ReleaseLines[0], orderLine9.ReleaseLines[0] }, 6, order6);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 }, //1 for transfers + 1 for receive (for validation) + 1 for orders (packing job parent)
				{ WhsDocketLineSchema.Constants.TableName, 5 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var packages = new[] { package1.PK.ToGuid(), package2.PK.ToGuid(), package3.PK.ToGuid(), package4.PK.ToGuid(), package5.PK.ToGuid(), package6.PK.ToGuid() };
				var response = webService.UpdatePutawayConsolidationLocationForPackages(packages, consolidationLocation.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			PkgPackage CreatePackage(IEnumerable<WhsReleaseLine> itemsToPack, int index, WhsOrder order)
			{
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var package = PackingHelper.CreatePackage(packageJob, "PKG" + index, 1, PkgUnit.Box);
				itemsToPack.ForEach(item => package.Pack(item, 1m));
				return package;
			}
		}

		#endregion

		#region TestSuccessfullyUpdatesPutawayLocationForPackage

		public void TestSuccessfullyUpdatesPutawayLocationForPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", transferLine.WE_WL, consolidationLocation.PK);
			AssertEquals("transferLine should not be finalised.", false, transferLine.IsFinalised);
		}

		public void TestSuccessfullyUpdatesPutawayLocationForPackage_MultiplePackages()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var pickLine3 = orderLine3.PickLines.Single();
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);

			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
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
			package3.Pack(orderLine3.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package1.PK.ToGuid(), package2.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for TransferLine should be updated.", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("TransferLine should not be finalised.", false, transferLine1.IsFinalised);

			AssertEquals("Destination for TransferLine should be updated.", consolidationLocation.PK, transferLine2.WE_WL);
			AssertEquals("TransferLine should not be finalised.", false, transferLine2.IsFinalised);

			AssertEquals("Destination for TransferLine should not be updated.", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);
			AssertEquals("TransferLine should not be finalised.", false, transferLine3.IsFinalised);
		}

		#endregion

		public void TestUpdatePutawayConsolidationLocationForPackages_OtherPackagesInAnotherConsolidationLocation_TransferLineUnfinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation2.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation2.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package2.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("transferLine should not be finalised.", false, transferLine2.IsFinalised);
		}

		public void TestUpdatePutawayConsolidationLocationForPackages_OtherPackagesInAnotherConsolidationLocation_TransferLineFinalised_InvalidLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation2.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation2.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package2.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot update to consolidation location 'PS1' as some packages on the same order are in a different consolidation location.", response);
		}

		public void TestUpdatePutawayConsolidationLocationForPackages_InvalidLocation_OtherPackagesInAnotherConsolidationLocation_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			order2.WD_UseDirectedPackingConsolidation = true;

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation2.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var pickLine3 = orderLine3.PickLines.Single();
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation2.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package3 = PackingHelper.CreatePackage(packageJob3, "PKG3", 1, PkgUnit.Box);
			package3.Pack(orderLine3.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package2.PK.ToGuid(), package3.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot update to consolidation location 'PS1' as some packages on the same order are in a different consolidation location.", response);
		}

		public void TestUpdatePutawayConsolidationLocationForPackages_OtherPackagesInAnotherConsolidationLocation_MultipleOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			order1.WD_UseDirectedPackingConsolidation = true;

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			order2.WD_UseDirectedPackingConsolidation = true;

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation2.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation2.PK;
			transferLine2.FinaliseDocketLine();

			var pickLine3 = orderLine3.PickLines.Single();
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, ZDateTimeOffset.Now);

			AssertEquals("Precondition", consolidationLocation2.PK, transferLine1.WE_WL);
			AssertEquals("Precondition", consolidationLocation2.PK, transferLine2.WE_WL);
			AssertEquals("Precondition", data.Whs1.DefaultOutboundDockDoorLocation.PK, transferLine3.WE_WL);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var package3 = PackingHelper.CreatePackage(packageJob3, "PKG3", 1, PkgUnit.Box);
			package3.Pack(orderLine3.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.UpdatePutawayConsolidationLocationForPackages(new[] { package3.PK.ToGuid() }, consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for transferLine should be updated.", transferLine3.WE_WL, consolidationLocation.PK);
			AssertEquals("transferLine should not be finalised.", false, transferLine3.IsFinalised);
		}

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}

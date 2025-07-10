using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PutawayPackageInConsolidationLocationTest : WhsSecureServiceTestCase
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
			var response = webService.PutawayPackageInConsolidationLocation(Guid.NewGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "In-Transit Package was not found in Warehouse.", response);
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
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), Guid.NewGuid());
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
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), location.PK.ToGuid());
			AssertBusinessValidationError(webService, "Packing Consolidation could not be found.", response);
		}

		#endregion

		#region TestSuccessfullyPutawayPackageToLocation

		public void TestSuccessfullyPutawayPackageToLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine.IsFinalised);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for TransferLine should be correct.", transferLine.WE_WL, consolidationLocation.PK);
			AssertEquals("TransferLine should be finalised.", true, transferLine.IsFinalised);
		}

		public void TestSuccessfullyUpdatesPutawayLocationForPackage_MultipleTransitLinesForPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine1.IsFinalised);

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine2.IsFinalised);

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation.PK);
			AssertEquals("TransferLine should be finalised.", true, transferLine1.IsFinalised);

			AssertEquals("Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("TransferLine should be finalised.", true, transferLine2.IsFinalised);
		}

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
			transferLine1.WE_WL = consolidationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine1.IsFinalised);

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot putaway partially processed packages.", response);
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
			transferLine1.WE_WL = consolidationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine1.IsFinalised);

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine2.IsFinalised);

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			package.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			transferLine2.FinaliseDocketLine();
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot putaway partially processed packages.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine0 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine6 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine7 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine9 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

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
			var transferLine0 = Helper.PickAndMakeInTransitTransfer(pickLine0, now);
			transferLine0.WE_WL = consolidationLocation.PK;
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, now);
			transferLine1.WE_WL = consolidationLocation.PK;
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, now);
			transferLine2.WE_WL = consolidationLocation.PK;
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, now);
			transferLine3.WE_WL = consolidationLocation.PK;
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, now);
			transferLine4.WE_WL = consolidationLocation.PK;
			var transferLine5 = Helper.PickAndMakeInTransitTransfer(pickLine5, now);
			transferLine5.WE_WL = consolidationLocation.PK;
			var transferLine6 = Helper.PickAndMakeInTransitTransfer(pickLine6, now);
			transferLine6.WE_WL = consolidationLocation.PK;
			var transferLine7 = Helper.PickAndMakeInTransitTransfer(pickLine7, now);
			transferLine7.WE_WL = consolidationLocation.PK;
			var transferLine8 = Helper.PickAndMakeInTransitTransfer(pickLine8, now);
			transferLine8.WE_WL = consolidationLocation.PK;
			var transferLine9 = Helper.PickAndMakeInTransitTransfer(pickLine9, now);
			transferLine9.WE_WL = consolidationLocation.PK;

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine0.ReleaseLines[0], 1m);
			package.Pack(orderLine1.ReleaseLines[0], 1m);
			package.Pack(orderLine2.ReleaseLines[0], 1m);
			package.Pack(orderLine3.ReleaseLines[0], 1m);
			package.Pack(orderLine4.ReleaseLines[0], 1m);
			package.Pack(orderLine5.ReleaseLines[0], 1m);
			package.Pack(orderLine6.ReleaseLines[0], 1m);
			package.Pack(orderLine7.ReleaseLines[0], 1m);
			package.Pack(orderLine8.ReleaseLines[0], 1m);
			package.Pack(orderLine9.ReleaseLines[0], 1m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 }, // 1 for transfer, 1 receive (for validaiton) and 1 for order (packing job parent)
				{ WhsDocketLineSchema.Constants.TableName, 6 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}

		public void TestPutawayPackageInConsolidationLocation_WithSiblingPackages_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocations = new List<ZGuid>();
			for (var i = 0; i < 10; i++)
			{
				var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, $"PS{i}", 1, 1).Locations[0];
				consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
				consolidationLocations.Add(consolidationLocation.PK);
			}

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine0 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine5 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine6 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine7 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine8 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine9 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

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
			var transferLine0 = Helper.PickAndMakeInTransitTransfer(pickLine0, now);
			transferLine0.WE_WL = consolidationLocations[0];
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, now);
			transferLine1.WE_WL = consolidationLocations[1];
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, now);
			transferLine2.WE_WL = consolidationLocations[2];
			var transferLine3 = Helper.PickAndMakeInTransitTransfer(pickLine3, now);
			transferLine3.WE_WL = consolidationLocations[3];
			var transferLine4 = Helper.PickAndMakeInTransitTransfer(pickLine4, now);
			transferLine4.WE_WL = consolidationLocations[4];
			var transferLine5 = Helper.PickAndMakeInTransitTransfer(pickLine5, now);
			transferLine5.WE_WL = consolidationLocations[5];
			var transferLine6 = Helper.PickAndMakeInTransitTransfer(pickLine6, now);
			transferLine6.WE_WL = consolidationLocations[6];
			var transferLine7 = Helper.PickAndMakeInTransitTransfer(pickLine7, now);
			transferLine7.WE_WL = consolidationLocations[7];
			var transferLine8 = Helper.PickAndMakeInTransitTransfer(pickLine8, now);
			transferLine8.WE_WL = consolidationLocations[8];
			var transferLine9 = Helper.PickAndMakeInTransitTransfer(pickLine9, now);
			transferLine9.WE_WL = consolidationLocations[9];

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package0 = PackingHelper.CreatePackage(packageJob, "PKG0", 1, PkgUnit.Box);
			package0.Pack(orderLine0.ReleaseLines[0], 1m);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 1m);
			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 1m);
			var package3 = PackingHelper.CreatePackage(packageJob, "PKG3", 1, PkgUnit.Box);
			package3.Pack(orderLine3.ReleaseLines[0], 1m);
			var package4 = PackingHelper.CreatePackage(packageJob, "PKG4", 1, PkgUnit.Box);
			package4.Pack(orderLine4.ReleaseLines[0], 1m);
			var package5 = PackingHelper.CreatePackage(packageJob, "PKG5", 1, PkgUnit.Box);
			package5.Pack(orderLine5.ReleaseLines[0], 1m);
			var package6 = PackingHelper.CreatePackage(packageJob, "PKG6", 1, PkgUnit.Box);
			package6.Pack(orderLine6.ReleaseLines[0], 1m);
			var package7 = PackingHelper.CreatePackage(packageJob, "PKG7", 1, PkgUnit.Box);
			package7.Pack(orderLine7.ReleaseLines[0], 1m);
			var package8 = PackingHelper.CreatePackage(packageJob, "PKG8", 1, PkgUnit.Box);
			package8.Pack(orderLine8.ReleaseLines[0], 1m);
			var package9 = PackingHelper.CreatePackage(packageJob, "PKG9", 1, PkgUnit.Box);
			package9.Pack(orderLine9.ReleaseLines[0], 1m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 }, // 1 for transfer, 1 receive (for validation) and 1 for order (packing job parent)
				{ WhsDocketLineSchema.Constants.TableName, 9 }, // 2 for the main package then 2 for all sibling packages
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationTypeSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 4 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 5 },
				{ WhsRowSchema.Constants.TableName, 2 },
				{ WhsVASOrderSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PutawayPackageInConsolidationLocation(package1.PK.ToGuid(), consolidationLocations[1].ToGuid());
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}

			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine0.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine2.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine3.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine4.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine5.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine6.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine7.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine8.WE_WL);
			AssertEquals("All transfer line's location got updated.", consolidationLocations[1], transferLine9.WE_WL);
		}

		#endregion

		public void TestPutawayPackageInConsolidationLocation_PickWithLooseInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot put away the package to consolidation location 'CON' as the associated pick does not support consolidation putaway.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_PickWithLooseInventoryAlreadyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.FinaliseDocketLine();

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot put away the package to consolidation location 'CON' as the associated pick does not support consolidation putaway.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_PickAndPack()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_IsPickAndPackEnabled = true;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
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
			transferLine.WE_WL = consolidationLocation.PK;

			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for TransferLine should be correct.", transferLine.WE_WL, consolidationLocation.PK);
			AssertEquals("TransferLine should be finalised.", true, transferLine.IsFinalised);
		}

		public void TestPutawayPackageInConsolidationLocation_OtherPackagesInAnotherConsolidationLocation_TransferLineUnfinalised()
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
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine2.IsFinalised);

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation2.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine1.IsFinalised);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("TransferLine should be finalised.", true, transferLine2.IsFinalised);
			AssertEquals("Sibling transfer line's location got updated.", consolidationLocation.PK, transferLine1.WE_WL);
			AssertEquals("TransferLine should not be finalised.", false, transferLine1.IsFinalised);
		}

		public void TestPutawayPackageInConsolidationLocation_InvalidLocation_OtherPackagesInAnotherConsolidationLocation_TransferLineFinalised_InvalidLocation()
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
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine2.IsFinalised);

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation2.PK);
			AssertEquals("Precondition: TransferLine should be finalised.", true, transferLine1.IsFinalised);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot put away the package to consolidation location 'PS1' as some packages on the same order are in a different consolidation location.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_WrongLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var packingLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			packingLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation2.PK;

			AssertEquals("Precondition", packingLocation2.PK, transferLine1.WE_WL);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), packingLocation.PK.ToGuid());
			AssertBusinessValidationError(webService, "Cannot put away the package to consolidation location 'PS1'. The package needs to go to 'PS2' consolidation location.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_OtherPackagesInAnotherConsolidationLocation_ConcurrencyError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = packingStationLocationType.PK;

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
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine2.IsFinalised);

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation2.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine1.IsFinalised);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.Factory.Saving += delegate
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var transferLineInOtherFactory = otherFactory.Load<WhsTransferLine>(transferLine2.PK);
				transferLineInOtherFactory.WE_WL = consolidationLocation2.PK;
				var siblingTransferLineInOtherFactory = otherFactory.Load<WhsTransferLine>(transferLine1.PK);
				siblingTransferLineInOtherFactory.WE_WL = consolidationLocation2.PK;
				siblingTransferLineInOtherFactory.FinaliseDocketLine();

				otherFactory.Save();
			};

			WebServiceResponse result = null;
			AssertNoExceptionThrown(() => result = webService.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid()));
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has made a change while you have been working on it. Please restart the operation and try again.", result.ErrorMessage);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", ErrorTypes.BusinessValidationError, result.Error);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transferLine2InNewFactory = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals("Destination for TransferLine has been updated.", transferLine2InNewFactory.WE_WL, consolidationLocation2.PK);
		}

		public void TestPutawayPackageInConsolidationLocation_OtherPackagesInAnotherDockDoorLocation_ConcurrencyError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine2.IsFinalised);

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, data.Whs1.DefaultOutboundDockDoorLocation.PK);
			AssertEquals("Precondition: TransferLine should not be finalised.", false, transferLine1.IsFinalised);

			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);

			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			webService.Factory.Saving += delegate
			{
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var transferLineInOtherFactory = otherFactory.Load<WhsTransferLine>(transferLine2.PK);
				transferLineInOtherFactory.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
				var siblingTransferLineInOtherFactory = otherFactory.Load<WhsTransferLine>(transferLine1.PK);
				siblingTransferLineInOtherFactory.WE_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
				siblingTransferLineInOtherFactory.FinaliseDocketLine();

				otherFactory.Save();
			};

			WebServiceResponse result = null;
			AssertNoExceptionThrown(() => result = webService.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid()));
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has made a change while you have been working on it. Please restart the operation and try again.", result.ErrorMessage);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", ErrorTypes.BusinessValidationError, result.Error);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var transferLine2InNewFactory = newFactory.Load<WhsTransferLine>(transferLine2.PK);
			AssertEquals("Destination for TransferLine has been updated.", transferLine2InNewFactory.WE_WL, data.Whs1.DefaultOutboundDockDoorLocation.PK);
		}

		public void TestPutawayPackageInConsolidationLocation_PickByLabel_WithSiblingPickByLabelJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = consolidationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			PackingHelper.CreatePackageDivot(package1, pickLine1);

			var pickByLabelJob1 = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "AAA", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package1.PK);

			var package2 = PackingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			PackingHelper.CreatePackageDivot(package2, pickLine2);

			WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, "BBB", data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Helper.Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "BBB", package2.PK);
			Factory.Save();

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, data.Whs1.DefaultOutboundDockDoorLocation.PK);
			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, consolidationLocation.PK);
			Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pickByLabelJob1.PK.ToGuid(), PickJobType.PickByLabelJob, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService2, "Cannot put away the package to consolidation location 'PS1' as the associated pick does not support consolidation putaway.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_OtherPackagesFinalisedInDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, data.Whs1.DefaultOutboundDockDoorLocation.PK);
			Assert("Precondition: TransferLine is finalised.", transferLine1.IsFinalised);
			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService2, "Cannot put away the package to consolidation location 'PS1' as the associated pick does not support consolidation putaway.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_OtherPackagesFinalisedInPackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;
			transferLine1.FinaliseDocketLine();
			pick.WP_WL_PackingStation = packingStationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, packingStationLocation.PK);
			Assert("Precondition: TransferLine is finalised.", transferLine1.IsFinalised);
			AssertEquals("Precondition: Pick has a packing station location.", pick.PackingStationLocation.PK, packingStationLocation.PK);
			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, consolidationLocation.PK);
			Factory.Save();

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService2, "Cannot put away the package to consolidation location 'PS1' as the associated pick does not support consolidation putaway.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_PutawayFromPackingStation_PickJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

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
			transferLine.WE_WL = packingStationLocation.PK;
			Factory.Save();

			var putawayToPackingStationWebService = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = putawayToPackingStationWebService.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingStationLocation.WLV_LocationString_UserFriendly);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
				AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);
			});

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLineInNewFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
			var pickLineInNewFactory = orderLineInNewFactory.PickLines.Single();
			var newTransferLine = Helper.PickAndMakeInTransitTransfer(pickLineInNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine.WE_WL = consolidationLocation.PK;
			newFactory.Save();

			var putawayToConsolidationLocationWebService = GetNewWebService(data.Whs1);
			var putawayToConsolidationResponse = putawayToConsolidationLocationWebService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, putawayToConsolidationResponse.Error);
				AssertNull("Should be no error.", putawayToConsolidationResponse.ErrorMessage);
			});

			var newFactory2 = new BusinessObjectFactory();
			var newTransferLineInNewFactory = newFactory2.Load<WhsTransferLine>(newTransferLine.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Destination for new TransferLine should be correct.", newTransferLineInNewFactory.WE_WL, consolidationLocation.PK);
				AssertEquals("New TransferLine should be finalised.", true, newTransferLineInNewFactory.IsFinalised);
			});
		}

		public void TestPutawayPackageInConsolidationLocation_PutawayFromPackingStation_TrolleyJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingStationLocation.PK;
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			package.SetIsTote(true);
			PackingHelper.CreatePackageDivot(package, pickLine);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var putawayToPackingStationWebService = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = putawayToPackingStationWebService.PutawayStockInDockDoorOrPackingStation(trolleyJob.PK.ToGuid(), PickJobType.TrolleyJob, packingStationLocation.WLV_LocationString_UserFriendly);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
				AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory();
			var orderLineInNewFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
			var pickLineInNewFactory = orderLineInNewFactory.PickLines.Single();
			var newTransferLine = Helper.PickAndMakeInTransitTransfer(pickLineInNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine.WE_WL = consolidationLocation.PK;
			newFactory.Save();

			var putawayToConsolidationLocationWebService = GetNewWebService(data.Whs1);
			var putawayToConsolidationResponse = putawayToConsolidationLocationWebService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, putawayToConsolidationResponse.Error);
				AssertNull("Should be no error.", putawayToConsolidationResponse.ErrorMessage);
			});

			var newFactory2 = new BusinessObjectFactory();
			var newTransferLineInNewFactory = newFactory2.Load<WhsTransferLine>(newTransferLine.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Destination for new TransferLine should be correct.", newTransferLineInNewFactory.WE_WL, consolidationLocation.PK);
				AssertEquals("New TransferLine should be finalised.", true, newTransferLineInNewFactory.IsFinalised);
			});
		}

		public void TestPutawayPackageInConsolidationLocation_PutawayFromPackingStation_PickByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingStationLocation.PK;
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			package.SetIsTote(true);
			PackingHelper.CreatePackageDivot(package, pickLine);
			package.KP_ClosedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, data.Whs1.WW_DefaultOutboundDockDoor, "AAA", package.PK);
			Factory.Save();

			var putawayToPackingStationWebService = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = putawayToPackingStationWebService.PutawayStockInDockDoorOrPackingStation(pickByLabelJob.PK.ToGuid(), PickJobType.PickByLabelJob, packingStationLocation.WLV_LocationString_UserFriendly);
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
				AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);
			});

			var newFactory = new BusinessObjectFactory();
			var orderLineInNewFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
			var pickLineInNewFactory = orderLineInNewFactory.PickLines.Single();
			var newTransferLine = Helper.PickAndMakeInTransitTransfer(pickLineInNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine.WE_WL = consolidationLocation.PK;
			newFactory.Save();

			var putawayToConsolidationLocationWebService = GetNewWebService(data.Whs1);
			var putawayToConsolidationResponse = putawayToConsolidationLocationWebService.PutawayPackageInConsolidationLocation(package.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			CombineAssertions(() =>
			{
				AssertEquals("Should be no error.", ErrorTypes.None, putawayToConsolidationResponse.Error);
				AssertNull("Should be no error.", putawayToConsolidationResponse.ErrorMessage);
			});

			var newFactory2 = new BusinessObjectFactory();
			var newTransferLineInNewFactory = newFactory2.Load<WhsTransferLine>(newTransferLine.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Destination for new TransferLine should be correct.", newTransferLineInNewFactory.WE_WL, consolidationLocation.PK);
				AssertEquals("New TransferLine should be finalised.", true, newTransferLineInNewFactory.IsFinalised);
			});
		}

		public void TestPutawayPackageInConsolidationLocation_PutawayFromPackingStation_OtherPackagesFinalisedInDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingStationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, packingStationLocation.PK);
			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, packingStationLocation.PK);
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingStationLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine1.FinaliseDocketLine();

			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			var pickLine2InNewFactory = orderLine2InNewFactory.PickLines.Single();
			var newTransferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine2.WE_WL = consolidationLocation.PK;
			newFactory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation.PK.ToGuid());
			AssertBusinessValidationError(webService2, "Package cannot be putaway to 'PS1' as some packages on the order is in 'DOCKDOOR'.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_PutawayFromPackingStation_OtherPackagesFinalisedInAnotherConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation1.WLV_WLT_LocationType = consolidationLocationType.PK;

			var consolidationLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS2", 1, 1).Locations[0];
			consolidationLocation2.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingStationLocation.PK;

			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine2.WE_WL, packingStationLocation.PK);
			AssertEquals("Precondition: Destination for TransferLine should be correct.", transferLine1.WE_WL, packingStationLocation.PK);
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingStationLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob1, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package2 = PackingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine1.WE_WL = consolidationLocation2.PK;
			newTransferLine1.FinaliseDocketLine();

			var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			var pickLine2InNewFactory = orderLine2InNewFactory.PickLines.Single();
			var newTransferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine2.WE_WL = consolidationLocation1.PK;
			newFactory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PutawayPackageInConsolidationLocation(package2.PK.ToGuid(), consolidationLocation1.PK.ToGuid());
			AssertBusinessValidationError(webService2, "Package cannot be putaway to 'PS1' as some packages on the order is in 'PS2'.", response);
		}

		public void TestPutawayPackageInConsolidationLocation_WithUnpickedSiblingPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine0 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);

			var pickLine0 = orderLine0.PickLines.Single();
			var pickLine1 = orderLine1.PickLines.Single();
			var pickLine2 = orderLine2.PickLines.Single();
			var pickLine3 = orderLine3.PickLines.Single();
			var pickLine4 = orderLine4.PickLines.Single();

			var now = ZDateTimeOffset.Now;
			var transferLine0 = Helper.PickAndMakeInTransitTransfer(pickLine0, now);
			transferLine0.WE_WL = consolidationLocation.PK;
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package0 = PackingHelper.CreatePackage(packageJob, "PKG0", 1, PkgUnit.Box);
			package0.Pack(orderLine0.ReleaseLines[0], 1m);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 1m);
			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 1m);
			var package3 = PackingHelper.CreatePackage(packageJob, "PKG3", 1, PkgUnit.Box);
			package3.Pack(orderLine3.ReleaseLines[0], 1m);
			var package4 = PackingHelper.CreatePackage(packageJob, "PKG4", 1, PkgUnit.Box);
			package4.Pack(orderLine4.ReleaseLines[0], 1m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			WebServiceResponse response = null;
			AssertNoExceptionThrown(() => response = webService.PutawayPackageInConsolidationLocation(package0.PK.ToGuid(), consolidationLocation.PK.ToGuid()));
			AssertSuccessfulResponseWithNoErrors(response, webService);
		}

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}

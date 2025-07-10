using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PrintPackageLabelForHandlingUnitTest : WhsSecureServiceTestCase
	{
		#region TestInvalidArguments

		public void TestInvalidArguments_PackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.PrintPackageLabelForHandlingUnit(Guid.NewGuid(), Guid.NewGuid(), 5);
			AssertBusinessValidationError(webService, "Handling Unit was not found.", response);
		}

		public void TestInvalidArguments_PrinterNotFound()
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

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);

			var response = webService.PrintPackageLabelForHandlingUnit(handlingUnitPackage.PK.ToGuid(), Guid.NewGuid(), 5);
			AssertBusinessValidationError(webService, "No valid Printer provided.", response);
		}

		#endregion

		#region TestPrintPackageLabelForHandlingUnit

		public void TestPrintPackageLabelForHandlingUnit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var printer = Helper.CreatePrintQueue("Warehouse");

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

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);

			var response = webService.PrintPackageLabelForHandlingUnit(handlingUnitPackage.PK.ToGuid(), printer.PK.ToGuid(), 5);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var printJobQuery = new ZQuery();
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_SQ, printer.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_ParentGuid, handlingUnitPackageJob.PK);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_JobType, "PRN");
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_Copies, (short)5);
			printJobQuery.AddToFilter(StmPrintJobSchema.SP_DocumentName, "Basic Package Label" + StmPrintJob.LanguageDelimiter + DataRegistry.Instance.EnglishSpelling);
			AssertNotNull("Should have created the correct Print Job.", webService.Factory.LoadTop1<IStmPrintJob>(printJobQuery));
		}

		public void TestPrintPackageLabelForHandlingUnit_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var printer = Helper.CreatePrintQueue("Warehouse");

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

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ PkgHandlingUnitSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ StmMenuItemSchema.Constants.TableName, 1 },
				{ StmPrintQueueSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.PrintPackageLabelForHandlingUnit(handlingUnitPackage.PK.ToGuid(), printer.PK.ToGuid(), 5);
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}

		#endregion

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		#endregion
	}
}

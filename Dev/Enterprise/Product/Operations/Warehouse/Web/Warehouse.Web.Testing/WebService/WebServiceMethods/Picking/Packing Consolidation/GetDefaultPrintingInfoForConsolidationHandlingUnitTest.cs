using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetDefaultPrintingInfoForConsolidationHandlingUnitTest : WhsSecureServiceTestCase
	{
		#region TestInvalidArguments

		public void TestInvalidArguments_NullInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(null);
			AssertBusinessValidationError(webService, "Invalid Request for Printing Information.", response);
		}
		public void TestInvalidArguments_PackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(new PackageForPrintingInfo { PK = Guid.NewGuid() });
			AssertBusinessValidationError(webService, "Handling Unit was not found.", response);
		}

		public void TestInvalidArguments_PrintersNotFound()
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

			var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(new PackageForPrintingInfo { PK = handlingUnitPackage.PK.ToGuid() });
			AssertBusinessValidationError(webService, "No printers exist in the system.", response);
		}

		#endregion

		#region TestGetDefaultPrintingInfoForConsolidationHandlingUnit

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit(false);
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_SupportsCBA()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit(true);
		}

		void TestGetDefaultPrintingInfoForConsolidationHandlingUnit(bool supportsCBALabel)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var printer1 = Helper.CreatePrintQueue("Warehouse");
			var printer2 = Helper.CreatePrintQueue("Putaway Area");

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			data.Whs1.RFPickPackPrinterPK = printer1.PK;
			packingLocation.PutawayArea.RFPickPackPrinterPK = printer2.PK;

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

			var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(new PackageForPrintingInfo { PK = handlingUnitPackage.PK.ToGuid(), SupportsCarrierLabelIntegration = supportsCBALabel });
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals(0, response.NumberOfLabelsToPrintOnNew);
			AssertEquals(0, response.NumberOfLabelsToPrintOnClose);
			AssertEquals("Should default to the printer for package location.", printer2.PK, response.DefaultPrinter);
			AssertContainsExactElementsInAnyOrder(new[] { printer1.PK.ToGuid(), printer2.PK.ToGuid() }, response.Printers.Select(p => p.PK));
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoPrinterForPutawayArea()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoPrinterForPutawayArea(false);
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoPrinterForPutawayArea_SupportsCBA()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoPrinterForPutawayArea(true);
		}

		void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoPrinterForPutawayArea(bool supportsCBALabel)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var printer1 = Helper.CreatePrintQueue("Warehouse");
			var printer2 = Helper.CreatePrintQueue("Other");

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			data.Whs1.RFPickPackPrinterPK = printer1.PK;

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

			var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(new PackageForPrintingInfo { PK = handlingUnitPackage.PK.ToGuid(), SupportsCarrierLabelIntegration = supportsCBALabel });
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals(0, response.NumberOfLabelsToPrintOnNew);
			AssertEquals(0, response.NumberOfLabelsToPrintOnClose);
			AssertEquals("Should default to the printer for Warehouse location.", printer1.PK, response.DefaultPrinter);
			AssertContainsExactElementsInAnyOrder(new[] { printer1.PK.ToGuid(), printer2.PK.ToGuid() }, response.Printers.Select(p => p.PK));
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_FallbackToBasicLabel()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit_FallbackToBasicLabel(false);
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_FallbackToBasicLabel_SupportsCBA()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit_FallbackToBasicLabel(true);
		}

		void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_FallbackToBasicLabel(bool supportsCBALabel)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var printer1 = Helper.CreatePrintQueue("UserPrinter");
			var printer2 = Helper.CreatePrintQueue("Other");

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var basicLabelQuery = new DocumentZQuery(BusinessContext.PackageHandlingUnit, "Basic Label");
			var basicLabel = Factory.LoadTop1<DocumentCommand>(basicLabelQuery);

			Helper.CreateDefaultPrinter(basicLabel.PK, GlbStaff.CurrentUser, printer1.PK, 1);
			Factory.Save();

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

			var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(new PackageForPrintingInfo { PK = handlingUnitPackage.PK.ToGuid(), SupportsCarrierLabelIntegration = supportsCBALabel });
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals(0, response.NumberOfLabelsToPrintOnNew);
			AssertEquals(0, response.NumberOfLabelsToPrintOnClose);
			AssertEquals("Should default to the user default printer for non-CBA.", supportsCBALabel ? Guid.Empty : printer1.PK, response.DefaultPrinter);
			AssertContainsExactElementsInAnyOrder(new[] { printer1.PK.ToGuid(), printer2.PK.ToGuid() }, response.Printers.Select(p => p.PK));
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoDefaultPrinterFallbackForStaff_WithDefaultPrinterEntryInDB()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoDefaultPrinterFallbackForStaff(true);
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoDefaultPrinterFallbackForStaff_WithoutDefaultPrinterEntryInDB()
		{
			TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoDefaultPrinterFallbackForStaff(false);
		}

		void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_NoDefaultPrinterFallbackForStaff(bool hasDefaultPrinterEntry)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var printer1 = Helper.CreatePrintQueue("UserPrinter");
			var printer2 = Helper.CreatePrintQueue("Other");

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			if (hasDefaultPrinterEntry)
			{
				Helper.CreateDefaultPrinter(Guid.Empty, GlbStaff.CurrentUser, printer1.PK, 1);
			}

			Factory.Save();

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

			var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(new PackageForPrintingInfo { PK = handlingUnitPackage.PK.ToGuid(), SupportsCarrierLabelIntegration = false });
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals(0, response.NumberOfLabelsToPrintOnNew);
			AssertEquals(0, response.NumberOfLabelsToPrintOnClose);
			AssertEquals("Should not default if no default printer specified.", Guid.Empty, response.DefaultPrinter);
			AssertContainsExactElementsInAnyOrder(new[] { printer1.PK.ToGuid(), printer2.PK.ToGuid() }, response.Printers.Select(p => p.PK));
		}

		public void TestGetDefaultPrintingInfoForConsolidationHandlingUnit_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);

			var printer0 = Helper.CreatePrintQueue("P0");
			var printer1 = Helper.CreatePrintQueue("P1");
			var printer2 = Helper.CreatePrintQueue("P2");
			var printer3 = Helper.CreatePrintQueue("P3");
			var printer4 = Helper.CreatePrintQueue("P4");
			var printer5 = Helper.CreatePrintQueue("P5");
			var printer6 = Helper.CreatePrintQueue("P6");
			var printer7 = Helper.CreatePrintQueue("P7");
			var printer8 = Helper.CreatePrintQueue("P8");
			var printer9 = Helper.CreatePrintQueue("P9");

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			data.Whs1.RFPickPackPrinterPK = printer0.PK;
			packingLocation.PutawayArea.RFPickPackPrinterPK = printer1.PK;

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
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ StmDefaultPrinterSchema.Constants.TableName, 1 },
				{ StmPrintQueueSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPackageLocationViewSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.GetDefaultPrintingInfoForConsolidationHandlingUnit(new PackageForPrintingInfo { PK = handlingUnitPackage.PK.ToGuid(), SupportsCarrierLabelIntegration = true });
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}

		#endregion

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		#endregion
	}
}

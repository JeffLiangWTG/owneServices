using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CalculateWeightAndDimsForHandlingUnitTest : WhsSecureServiceTestCase
	{
		#region TestInvalidArguments

		public void TestInvalidArguments_PackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.CalculateWeightAndDimsForHandlingUnit(Guid.NewGuid(), "PLT");
			AssertBusinessValidationError(webService, "Handling Unit was not found.", response);
		}

		public void TestInvalidArguments_PackTypeNotFound()
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

				Helper.Factory.Save();

				var webService = GetNewWebService();
				webService.SecurityHeader.IsAndroidDevice = true;
				webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

				var response = webService.CalculateWeightAndDimsForHandlingUnit(handlingUnitPackage.PK.ToGuid(), "ABCDEF");
				AssertBusinessValidationError(webService, "Pack Type was not found.", response);
			}
		}

		#endregion

		#region TestCalculateWeightAndDimsForHandlingUnit

		public void TestCalculateWeightAndDimsForHandlingUnit()
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

				package1.KP_WeightUQ = Constants.Weight.Grams;
				package1.KP_Weight = 4000m;

				var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
				var package2 = packingHelper.CreatePackage(packageJob2, "PKG2", 1, PkgUnit.Box);
				package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

				package2.KP_WeightUQ = Constants.Weight.Kilograms;
				package2.KP_Weight = 8m;

				Helper.Factory.Save();

				var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
				handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
				handlingUnit.KPU_JobContext = "3PL";

				var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
				var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

				packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
				packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

				handlingUnitPackage.KP_IsClosed = true;
				handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				handlingUnitPackage.KP_WeightUQ = Constants.Weight.Kilograms;
				handlingUnitPackage.KP_TareWeight = 3m;
				handlingUnitPackage.KP_Weight = 5m;
				handlingUnitPackage.KP_DimensionUQ = Constants.Length.Centimetres;
				handlingUnitPackage.KP_Length = 100m;
				handlingUnitPackage.KP_Width = 100m;
				handlingUnitPackage.KP_Height = 100m;

				Helper.Factory.Save();

				PackingHelper.CreateRefPackType("PAC", "PACK", 2m, 4m, 8m, Constants.Length.Metres, 3m, Constants.Weight.Kilograms);
				Helper.Factory.Save();

				var webService = GetNewWebService(data.Whs1);
				var response = webService.CalculateWeightAndDimsForHandlingUnit(handlingUnitPackage.PK.ToGuid(), "PAC");
				AssertSuccessfulResponseWithNoErrors(response, webService);

				AssertEquals(3m, response.PackageDimensions.EmptyWeight);
				AssertEquals(15m, response.PackageDimensions.Weight);
				AssertEquals(Constants.Weight.Kilograms, response.PackageDimensions.WeightUQ);
				AssertEquals(2m, response.PackageDimensions.Height);
				AssertEquals(4m, response.PackageDimensions.Length);
				AssertEquals(8m, response.PackageDimensions.Width);
				AssertEquals(Constants.Length.Metres, response.PackageDimensions.DimensionUQ);
			}
		}

		public void TestCalculateWeightAndDimsForHandlingUnit_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
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

			var handlingUnit = Helper.Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);

			packingHelper.PackHandlingUnit(handlingUnitPackage, package0, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package3, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package4, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package5, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package6, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package7, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package8, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package9, handlingUnitPackage);
			Factory.Save();

			PackingHelper.CreateRefPackType("PAC", "PACK", 2m, 4m, 8m, Constants.Length.Metres, 3m, Constants.Weight.Kilograms);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var expectedDBHits = new Dictionary<string, int>()
			{
				{ PkgPackageHandlingUnitDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.CalculateWeightAndDimsForHandlingUnit(handlingUnitPackage.PK.ToGuid(), "PAC");
				AssertSuccessfulResponseWithNoErrors(response, webService);
			}
		}

		#endregion

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService;
using Enterprise.Warehouse.Web.WebService.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.Testing
{
	class GetTrolleyTest : WhsSecureServiceTestCase
	{
		#region TestGetTrolley

		#region TestGetTrolley_Building

		public void TestGetTrolley_Building_Carton()
		{
			TestGetTrolley_BuildingCore(TrolleyPickingType.Carton);
		}

		public void TestGetTrolley_Building_Tote()
		{
			TestGetTrolley_BuildingCore(TrolleyPickingType.Tote);
		}

		void TestGetTrolley_BuildingCore(TrolleyPickingType trolleyType)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// Create new TrolleyJob to build
			Helper.CreateTrolley("T001");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Building, true, trolleyType);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Should create new Trolley Job and return it to RF gun.", response1.Job);

			CombineAssertions(() =>
			{
				AssertEquals("No pick lines should be returned with the job.", false, response1.Job.Lines.Any());
				AssertEquals("No slots should be created for new job.", false, response1.Job.Slots.Any());
				AssertEquals("Trolley picking types should be the same even though the Job is None in the db.", trolleyType, response1.Job.TrolleyPickType);
			});

			var trolleyJob = Helper.Factory.LoadTop1<WhsPickTrolleyJob>(new ZQuery());
			AssertEquals("New trolley job should be created and saved to DB.", true, trolleyJob.IsInDatabase);

			// Continue building existing TrolleyJob
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock for picking
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition", 10m, pickLine.WZ_Units);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			pkg1.SetIsTote(trolleyType == TrolleyPickingType.Tote);
			packingHelper.CreatePackageDivot(pkg1, pickLine);

			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 3);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetTrolley("T001", TrolleyJobStatus.Building, true, trolleyType);
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull(response2.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should find existing Trolley Job and return it to RF gun.", trolleyJob.PK, response2.Job.PK);
				AssertEquals("No pick lines should be returned with the job.", 0, response2.Job.Lines.Count);
				AssertEquals("RF gun should receive all existing trolley slots.", 1, response2.Job.Slots.Count);
				AssertNoExceptionThrown(() => response2.Job.Slots.Single(s => s.PackageID == "PKG1" && s.PackagePK == pkg1.PK && s.SlotNumber == 3));
				AssertEquals("Trolley picking types should be the same.", trolleyType, response2.Job.TrolleyPickType);
			});
		}

		public void TestGetTrolley_Building_None()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// Create new TrolleyJob to build
			Helper.CreateTrolley("T001");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.None);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Should create new Trolley Job and return it to RF gun.", response1.Job);

			CombineAssertions(() =>
			{
				AssertEquals("No pick lines should be returned with the job.", false, response1.Job.Lines.Any());
				AssertEquals("No slots should be created for new job.", false, response1.Job.Slots.Any());
			});

			var trolleyJob = Helper.Factory.LoadTop1<WhsPickTrolleyJob>(new ZQuery());
			AssertEquals("New trolley job should be created and saved to DB.", true, trolleyJob.IsInDatabase);

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.None);
			AssertSuccessfulResponse(response2, webService2);
			AssertNotNull("Trolley was not created, but should have been.", response2.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should create new Trolley Job and return it to RF gun.", trolleyJob.PK, response2.Job.PK);
				AssertEquals("No pick lines should be returned with the job.", false, response2.Job.Lines.Any());
				AssertEquals("No slots should exist on this trolley.", false, response2.Job.Slots.Any());
				AssertEquals("Trolley picking types should be None.", TrolleyPickingType.None, response2.Job.TrolleyPickType);
			});
		}

		#endregion

		#region TestGetTrolley_Building_CorrectType

		public void TestGetTrolley_Building_CorrectType_Carton()
		{
			TestGetTrolley_Building_CorrectType_Core(TrolleyPickingType.Carton);
		}

		public void TestGetTrolley_Building_CorrectType_Tote()
		{
			TestGetTrolley_Building_CorrectType_Core(TrolleyPickingType.Tote);
		}

		void TestGetTrolley_Building_CorrectType_Core(TrolleyPickingType trolleyType)
		{
			AssertNotEquals("This test is not designed for the None picking type", TrolleyPickingType.None, trolleyType);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock for picking

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition", 10m, pickLine.WZ_Units);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			pkg.SetIsTote(trolleyType == TrolleyPickingType.Tote);
			packingHelper.CreatePackageDivot(pkg, pickLine);
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, pkg.PK, 5);
			AssertEquals("Precondition", trolleyType, trolleyJob.PickingType);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Building, true, trolleyType);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Should find Trolley Job and return it to RF gun.", response1.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should return no error", ErrorTypes.None, response1.Error);
				AssertEquals($"Should return '{trolleyType}' Trolley", trolleyType, response1.Job.TrolleyPickType);
			});

			// flips the trolley pick type to the 'opposite' type
			var oppositeTrolleyPickType = trolleyType == TrolleyPickingType.Tote ? TrolleyPickingType.Carton : TrolleyPickingType.Tote;

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetTrolley("T001", TrolleyJobStatus.Building, true, oppositeTrolleyPickType);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Should return BusinessValidationError", ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Should have identical error messages", $"Trolley should be of type '{oppositeTrolleyPickType}'.", response2.ErrorMessage);
				AssertNull("Response job should be null.", response2.Job);
			});
		}

		public void TestGetTrolley_Building_CorrectType_NoneType()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock for picking

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition", 10m, pickLine.WZ_Units);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, pickLine);
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, pkg.PK, 5);
			Helper.Factory.Save();

			// valid only for picking - should not be a valid case/input during building but we'll check the correct response anyways
			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.None);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals("Should return BusinessValidationError", ErrorTypes.BusinessValidationError, response3.Error);
				AssertEquals("Should have identical error messages", "Trolley should be of type 'None'.", response3.ErrorMessage);
				AssertNull("Response job should be null.", response3.Job);
			});
		}

		public void TestGetTrolley_Building_CorrectType_Tote_NoResults()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// don't need to add orders, picks or packages here since the trolley is empty

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			AssertEquals("Precondition", trolleyJob.PickingType, TrolleyPickingType.None);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Should find Trolley Job and return it to RF gun.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should return no error", ErrorTypes.None, response.Error);
				AssertEquals("Should return Tote Trolley", TrolleyPickingType.Tote, response.Job.TrolleyPickType);
			});
		}

		public void TestGetTrolley_Building_CorrectType_Carton_NoResults()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// don't need to add orders, picks or packages here since the trolley is empty

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			AssertEquals("Precondition", trolleyJob.PickingType, TrolleyPickingType.None);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Should find Trolley Job and return it to RF gun.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should return no error", ErrorTypes.None, response.Error);
				AssertEquals("Should return Carton Trolley", TrolleyPickingType.Carton, response.Job.TrolleyPickType);
			});
		}

		public void TestGetTrolley_Building_CorrectType_NoneType_NoResults()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// don't need to add orders, picks or packages here since the trolley is empty

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			AssertEquals("Precondition", trolleyJob.PickingType, TrolleyPickingType.None);

			// valid only for picking - should not be a valid case/input during building but we'll check the correct response anyways
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.None);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("Should find Trolley Job and return it to RF gun.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should return no error", ErrorTypes.None, response.Error);
				AssertEquals("Should return Empty Trolley", TrolleyPickingType.None, response.Job.TrolleyPickType);
			});
		}

		#endregion

		#region TestGetTrolley_Building_WrongWarehouse

		public void TestGetTrolley_Building_WrongWarehouse()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// Create Trolley that is being build for Whs1
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition", 10m, pickLine.WZ_Units);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, pickLine);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 3);

			// Try to continue building Trolley from Whs1 while logged in from Whs2
			var whs2 = Helper.CreateWarehouse("WH2");
			Helper.Factory.Save();

			var webService = GetNewWebService(whs2);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertNull("No trolley job should be returned.", response.Job);
				AssertEquals("Should have error if trolley is used by another warehouse.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should have error if trolley is used by another warehouse.", "Trolley 'T001' is being used by another warehouse.", response.ErrorMessage);
			});
		}

		public void TestGetTrolley_Building_WrongWarehouse_FinalisedJob()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// Create Trolley that is being build for Whs1
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition", 10m, pickLine.WZ_Units);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, pickLine);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 3);

			// Try to continue building Trolley from Whs1 while logged in from Whs2
			var whs2 = Helper.CreateWarehouse("WH2");
			Helper.Factory.Save();

			// When all trolley jobs are finalised we can use it in any warehouse.
			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Finalised;
			trolleyJob.WTJ_FinalisedDateUtc = ZDateTime.UtcNow;
			Helper.Factory.Save();

			var webService = GetNewWebService(whs2);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			AssertNotNull("New trolley job should be created.", response.Job);

			CombineAssertions(() =>
			{
				AssertEquals("If trolley have no ongoing trolley picking jobs, it can be used by any warehouse.", ErrorTypes.None, response.Error);
				AssertEquals("If trolley have no ongoing trolley picking jobs, it can be used by any warehouse.", true, string.IsNullOrEmpty(response.ErrorMessage));
			});
		}

		#endregion

		#region TestGetTrolley_Building_RequiresPicking

		public void TestGetTrolley_Building_RequiresPicking()
		{
			TestGetTrolley_Building_JobInPickingStatus(hasBeenPicked: false, hasBeenPutaway: false);
		}

		#endregion

		#region TestGetTrolley_Building_RequiresPutaway

		public void TestGetTrolley_Building_RequiresPutaway()
		{
			TestGetTrolley_Building_JobInPickingStatus(hasBeenPicked: true, hasBeenPutaway: false);
		}

		#endregion

		#region TestGetTrolley_Building_OldJobShouldBeFinalised

		public void TestGetTrolley_Building_OldJobShouldBeFinalised()
		{
			// Occurs when user putsaway or picks the job via the desktop
			// This may be a regular occurence for warehouses not using outbound dock door tracking
			TestGetTrolley_Building_JobInPickingStatus(hasBeenPicked: true, hasBeenPutaway: true);
		}

		#endregion

		#region TestGetTrolley_Building_JobInPickingStatus

		void TestGetTrolley_Building_JobInPickingStatus(bool hasBeenPicked, bool hasBeenPutaway)
		{
			// setup
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");

			// create inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// setup orders and picks
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_GS_NKAssignedTo = staff.GS_Code;
			});

			// setup packages for order 1
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, orderLine.PickLines.Single());

			// create trolley and trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Picking;

			// links a package with a trolley/trolleyjob
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1); // with Product P1
			Helper.Factory.Save();

			if (hasBeenPicked)
			{
				var transferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);

				if (hasBeenPutaway)
				{
					transferLine.FinaliseDocketLine();
					AssertEquals("Should have finalised DockDoorTransfer line.", true, transferLine.IsFinalised);
				}

				Helper.Factory.Save();
			}
			else if (hasBeenPutaway)
			{
				throw new ArgumentException("Invalid arguments!");
			}

			// Getting trolley for Building
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);

			if (!hasBeenPutaway)
			{
				AssertNull("New trolley job should not have been created.", response.Job);

				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
					AssertEquals("Trolley 'T001' is not available for building. You need to finish Picking this Trolley first, or investigate the status of the trolley by looking up the Trolley # in CW1.", response.ErrorMessage);
					AssertEquals("Should *not* have updated trolley status.", PickTrolleyStatus.Codes.Picking, trolleyJob.WTJ_Status);
				});
			}
			else
			{
				AssertNotNull("New trolley job should be created.", response.Job);

				CombineAssertions(() =>
				{
					AssertEquals("Should have no errors.", ErrorTypes.None, response.Error);
					AssertEquals("Should have no errors.", true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertNotEquals("New trolley job should be created.", trolleyJob.PK, response.Job.PK);
					AssertEquals("Should have updated trolley status.", PickTrolleyStatus.Codes.Finalised, trolleyJob.WTJ_Status);
				});

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var trolleyJobs = factory2.Load<WhsPickTrolleyJob>(new ZQuery());

				CombineAssertions(() =>
				{
					AssertEquals("Should be two trolley jobs in the db.", 2, trolleyJobs.Length);
					AssertEquals("Should have updated trolley status.", true, trolleyJobs.Any(tj => tj.PK == trolleyJob.PK && trolleyJob.WTJ_Status == PickTrolleyStatus.Codes.Finalised));
				});
			}
		}

		#endregion

		#region TestGetTrolley_Picking

		public void TestGetTrolley_Picking_Tote_None()
		{
			TestGetTrolley_Picking_Core(TrolleyPickingType.Tote, TrolleyPickingType.None);
		}

		public void TestGetTrolley_Picking_Carton_None()
		{
			TestGetTrolley_Picking_Core(TrolleyPickingType.Carton, TrolleyPickingType.None);
		}

		public void TestGetTrolley_Picking_Carton_Carton()
		{
			TestGetTrolley_Picking_Core(TrolleyPickingType.Carton, TrolleyPickingType.Carton);
		}

		public void TestGetTrolley_Picking_Tote_Tote()
		{
			TestGetTrolley_Picking_Core(TrolleyPickingType.Tote, TrolleyPickingType.Tote);
		}

		// This test sets up a trolley and job with the 'trolleyInDB' pick type and then asks the webService for a
		// trolley of type 'requestedTrolleyType', and then checks the webService reply has all the correct internals.
		void TestGetTrolley_Picking_Core(TrolleyPickingType trolleyInDB, TrolleyPickingType requestedTrolleyType)
		{
			// setup
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// create inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Helper.Factory.Save(); // to create stock

			// setup orders and picks
			// create order 1 with one picked and one unpicked package
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var totalPickLineQuantity1 = Helper.GetTotalPickLineQuantity(pick1);
			AssertEquals("Precondition", 3m, totalPickLineQuantity1);

			// setup packages for order 1
			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1a = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			var pkg1b = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1a, order1Line1.PickLines.Single());
			packingHelper.CreatePackageDivot(pkg1b, order1Line2.PickLines.Single()); // package not assigned to trolley

			// create order 2 with multiple pick lines in a package with some already picked
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line1 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			var order2Line3 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			order2Line2.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Today; // simulate one line already picked.
			var totalPickLineQuantity2 = Helper.GetTotalPickLineQuantity(pick2);
			AssertEquals("Precondition", 12m, totalPickLineQuantity2);

			// setup packages for order 2
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG3", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg2, order2Line1.PickLines.Single());
			packingHelper.CreatePackageDivot(pkg2, order2Line2.PickLines.Single()); // package assigned to trolley but this line had already been picked
			packingHelper.CreatePackageDivot(pkg2, order2Line3.PickLines.Single());

			// create order 3 that is not related to TrolleyJob and order 4 that is already fully picked on the trolley and have different product
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var order3Line1 = Helper.CreateWhsOrderLine(order3, data.Part1, 6m);
			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			var order4Line1 = Helper.CreateWhsOrderLine(order4, data.Part2, 7m);
			var pick3 = Helper.CreatePickNew(order3, order4);
			order4Line1.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Today; // simulate line already picked.
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var totalPickLineQuantity3 = Helper.GetTotalPickLineQuantity(pick3);
			AssertEquals("Precondition", 13m, totalPickLineQuantity3);

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var pkg3 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg3, order3Line1.PickLines.Single());  // package not assigned to trolley

			var pkgJob4 = PkgPackageJob.LoadOrCreatePackageJob(order4);
			var pkg4 = packingHelper.CreatePackage(pkgJob4, "PKG4", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg4, order4Line1.PickLines.Single());

			// set up totes
			bool isToteTrolley = trolleyInDB == TrolleyPickingType.Tote;
			pkg1a.SetIsTote(isToteTrolley);
			pkg2.SetIsTote(isToteTrolley);
			pkg4.SetIsTote(isToteTrolley);

			// create trolley and trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// links a package with a trolley/trolleyjob		
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1a, 1); // with Product P1
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2); // with Product P1
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg4, 3); // with Product P2

			Helper.Factory.Save();

			AssertEquals("Precondition", trolleyInDB, trolleyJob.PickingType);

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, requestedTrolleyType);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);

				if (requestedTrolleyType == TrolleyPickingType.None)
				{
					AssertEquals("When requesting None-type trolley we should return whatever exists in the DB", trolleyInDB, trolleyJobInfo.TrolleyPickType);
				}
				else
				{
					AssertEquals("Existing and requested trolley pick types should match", requestedTrolleyType, trolleyJobInfo.TrolleyPickType);
				}

				// check slots
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(3, trolleyJobInfo.Slots.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Slots.Single(s => s.PackageID == pkg1a.KP_PackageID && s.SlotNumber == 1));
				AssertNoExceptionThrown(() => trolleyJobInfo.Slots.Single(s => s.PackageID == pkg2.KP_PackageID && s.SlotNumber == 2));
				AssertNoExceptionThrown(() => trolleyJobInfo.Slots.Single(s => s.PackageID == pkg4.KP_PackageID && s.SlotNumber == 3));
				AssertNoExceptionThrown(() => trolleyJobInfo.Slots.Single(s => s.PackagePK == pkg1a.PK && s.SlotNumber == 1));
				AssertNoExceptionThrown(() => trolleyJobInfo.Slots.Single(s => s.PackagePK == pkg2.PK && s.SlotNumber == 2));
				AssertNoExceptionThrown(() => trolleyJobInfo.Slots.Single(s => s.PackagePK == pkg4.PK && s.SlotNumber == 3));

				// check pick lines
				AssertEquals("All non-picked lines attached to the trolley job should be returned.", 2, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 1m));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 8m)); // rolled up of 2 unpicked picklines from pkg2

				// check orders
				AssertEquals("All orders associated with the trolley should be returned.", 3, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single(o => o.ExternalReference == "O1" && !o.Lines.Any()));
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single(o => o.ExternalReference == "O2" && !o.Lines.Any()));
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single(o => o.ExternalReference == "O4" && !o.Lines.Any()));

				// Check products
				AssertEquals("All non-picked Products associated with the trolley should be returned.", 1, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "P1"));

				AssertEquals("All non-picked Products Attributes associated with the trolley should be returned.", 1, trolleyJobInfo.ProductPartAttributesInfos.Count);

				// Check unit conversions
				AssertEquals("Unit conversions need to be populated for picking trolley, but only for non-picked products.", 1, trolleyJobInfo.UnitConversionsPerProduct.Count);
				AssertEquals("All Unit conversions need to be populated for Not Picked products.", 2, trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Unit && c.Qty == 1m));
				AssertNoExceptionThrown(() => trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Carton && c.Qty == 12m));

				// Check trolley job attributes
				AssertEquals("IsPickByBiggestPackTypeEnabled should be true for picking trolley.", true, trolleyJobInfo.IsPickByBiggestPackTypeEnabled);
				AssertEquals("IsPickByUOMTypeEnabled should be true for picking trolley.", true, trolleyJobInfo.IsPickByUOMTypeEnabled);
				AssertEquals("IsMultiOrder should be true for picking trolley.", true, trolleyJobInfo.IsMultiOrder);
			});
		}

		public void TestGetTrolley_Picking_NotAffectedByCapacity()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Helper.Factory.Save(); // to create stock

			// create order 1 with one picked and one unpicked package
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var totalPickLineQuantity1 = Helper.GetTotalPickLineQuantity(pick1);
			AssertEquals("Precondition", 3m, totalPickLineQuantity1);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			var pkg2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, order1Line1.PickLines.Single());
			packingHelper.CreatePackageDivot(pkg2, order1Line2.PickLines.Single()); // package not assigned to trolley

			// create order 2 with multiple pick lines in a package some already picked
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order2Line1 = Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			var order2Line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			var order2Line3 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			order2Line2.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Today; // simulate one line already picked.
			var totalPickLineQuantity2 = Helper.GetTotalPickLineQuantity(pick2);
			AssertEquals("Precondition", 12m, totalPickLineQuantity2);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg3 = packingHelper.CreatePackage(pkgJob2, "PKG3", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg3, order2Line1.PickLines.Single());
			packingHelper.CreatePackageDivot(pkg3, order2Line2.PickLines.Single()); // package assigned to trolley but this line had already been picked
			packingHelper.CreatePackageDivot(pkg3, order2Line3.PickLines.Single());

			// create order 3 that is not related to TrolleyJob and order 4 that is already fully picked on the trolley and have different product
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var order3Line1 = Helper.CreateWhsOrderLine(order3, data.Part1, 6m);
			var order4 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O4");
			var order4Line1 = Helper.CreateWhsOrderLine(order4, data.Part2, 7m);
			var pick3 = Helper.CreatePickNew(order3, order4);
			order4Line1.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Today; // simulate line already picked.
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var totalPickLineQuantity3 = Helper.GetTotalPickLineQuantity(pick3);
			AssertEquals("Precondition", 13m, totalPickLineQuantity3);

			var pkgJob3 = PkgPackageJob.LoadOrCreatePackageJob(order3);
			var pkg4 = packingHelper.CreatePackage(pkgJob3, "PKG3", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg4, order3Line1.PickLines.Single());  // package not assigned to trolley

			var pkgJob4 = PkgPackageJob.LoadOrCreatePackageJob(order4);
			var pkg5 = packingHelper.CreatePackage(pkgJob4, "PKG4", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg5, order4Line1.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1); // with Product P1
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg3, 2); // with Product P1
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg5, 3); // with Product P2
			Helper.Factory.Save();

			// Getting trolley for picking

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(3, trolleyJobInfo.Slots.Count);

				AssertEquals("All not picked lines attached to the trolley job should be returned.", 2, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 1m));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 8m)); // rolled up of 2 unpicked picklines from pkg3

				AssertEquals("All orders associated with the trolley should be returned.", 3, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single(o => o.ExternalReference == "O1" && !o.Lines.Any()));
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single(o => o.ExternalReference == "O2" && !o.Lines.Any()));
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single(o => o.ExternalReference == "O4" && !o.Lines.Any()));

				AssertEquals("All not picked Products associated with the trolley should be returned.", 1, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "P1"));

				AssertEquals("All not picked Products Attributes associated with the trolley should be returned.", 1, trolleyJobInfo.ProductPartAttributesInfos.Count);

				AssertEquals("Unit conversions need to be populated for picking trolley, but only for Not Picked products.", 1, trolleyJobInfo.UnitConversionsPerProduct.Count);
				AssertEquals("All Unit conversions need to be populated for Not Picked products.", 2, trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Unit && c.Qty == 1m));
				AssertNoExceptionThrown(() => trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Carton && c.Qty == 12m));

				AssertEquals("Picking information should be populated for picking trolley.", true, trolleyJobInfo.IsPickByBiggestPackTypeEnabled);
				AssertEquals("Picking information should be populated for picking trolley.", true, trolleyJobInfo.IsPickByUOMTypeEnabled);
				AssertEquals("Picking information should be populated for picking trolley.", true, trolleyJobInfo.IsMultiOrder);
			});
		}

		public void TestGetTrolley_Picking_PickByBOM()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);

			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var totalPickLineQuantity1 = Helper.GetTotalPickLineQuantity(pick);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, kitOrderLine1.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(1, trolleyJobInfo.Slots.Count);

				AssertEquals("Should return 2 Component Pick Lines.", 2, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 20m && l.ProductPK == wheel.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 10m && l.ProductPK == frame.PK));

				AssertEquals("All orders associated with the trolley should be returned.", 1, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single());

				AssertEquals("All not picked Products associated with the trolley should be returned, including BOM Product.", 3, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "WHEEL"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "FRAME"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "BIKE"));
			});
		}

		public void TestGetTrolley_Picking_PickByBOM_SplitComponentPickLine()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);

			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			var pkg2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, Constants.PkgUnit.Box);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(5m);
			packingHelper.CreatePackageDivot(pkg1, kitPickLine1);
			packingHelper.CreatePackageDivot(pkg2, kitPickLine2);

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(2, trolleyJobInfo.Slots.Count);

				AssertEquals("Should return 4 Component Pick Lines.", 4, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 10m && l.PackageID == "PKG1" && l.ProductPK == wheel.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 5m && l.PackageID == "PKG1" && l.ProductPK == frame.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 10m && l.PackageID == "PKG2" && l.ProductPK == wheel.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 5m && l.PackageID == "PKG2" && l.ProductPK == frame.PK));

				AssertEquals("All orders associated with the trolley should be returned.", 1, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single());

				AssertEquals("All not picked Products associated with the trolley should be returned, including BOM Product.", 3, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "WHEEL"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "FRAME"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "BIKE"));
			});
		}

		public void TestGetTrolley_Picking_PickByBOM_GroupComponentPickLines()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location1);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 5m, location2);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 3m, location2);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 2m, location2);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);

			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			var pkg2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, Constants.PkgUnit.Box);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(5m);
			packingHelper.CreatePackageDivot(pkg1, kitPickLine1);
			packingHelper.CreatePackageDivot(pkg2, kitPickLine2);

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(2, trolleyJobInfo.Slots.Count);

				AssertEquals("Should return 5 Component Pick Lines.", 4, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 10m && l.PackageID == "PKG1" && l.ProductPK == wheel.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 5m && l.PackageID == "PKG1" && l.ProductPK == frame.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 10m && l.PackageID == "PKG2" && l.ProductPK == wheel.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 5m && l.PackageID == "PKG2" && l.ProductPK == frame.PK));

				AssertEquals("All orders associated with the trolley should be returned.", 1, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single());

				AssertEquals("All not picked Products associated with the trolley should be returned, including BOM Product.", 3, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "WHEEL"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "FRAME"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "BIKE"));
			});
		}

		public void TestGetTrolley_Picking_PickByBOM_MixedWithOrderedComponents()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 22m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = order.Lines[0];
			var orderedWheelLine = Helper.CreateWhsOrderLine(order, wheel, 2m);
			var part1Line = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);

			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var totalPickLineQuantity1 = Helper.GetTotalPickLineQuantity(pick);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			var pkg2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, Constants.PkgUnit.Box);
			var kitPickLine1 = kitOrderLine1.PickLines.Single();
			var kitPickLine2 = kitPickLine1.Split(4m);
			var orderedWheelPickLine = orderedWheelLine.PickLines.Single();
			var part1PickLine = part1Line.PickLines.Single();
			packingHelper.CreatePackageDivot(pkg1, kitPickLine1);
			packingHelper.CreatePackageDivot(pkg1, part1PickLine);
			packingHelper.CreatePackageDivot(pkg2, kitPickLine2);
			packingHelper.CreatePackageDivot(pkg2, orderedWheelPickLine);

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(2, trolleyJobInfo.Slots.Count);

				AssertEquals("Should return 6 Pick Lines, the ordered component line is group separately.", 6, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 12m && l.PackageID == "PKG1" && l.ProductPK == wheel.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 6m && l.PackageID == "PKG1" && l.ProductPK == frame.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 3m && l.PackageID == "PKG1" && l.ProductPK == data.Part1.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 8m && l.PackageID == "PKG2" && l.ProductPK == wheel.PK && l.BOMProductPK == bike.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 2m && l.PackageID == "PKG2" && l.ProductPK == wheel.PK && l.BOMProductPK == Guid.Empty));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 4m && l.PackageID == "PKG2" && l.ProductPK == frame.PK));

				AssertEquals("All orders associated with the trolley should be returned.", 1, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single());

				AssertEquals("All not picked Products associated with the trolley should be returned, including BOM Product.", 4, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "WHEEL"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "FRAME"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "BIKE"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == data.Part1.OP_PartNum));
			});
		}

		public void TestGetTrolley_Picking_PickByBOM_Tote_ComponentsAreNotEvenToBuildKitsInCurrentRun()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 9m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 1m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);
			var framePickLine1 = kitOrder.Lines.Single(l => l.WE_OP == frame.PK).PickLines.Single(pl => pl.WZ_Units == 1m);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			framePickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			pkg1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg1, kitOrderLine1.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(1, trolleyJobInfo.Slots.Count);

				AssertEquals("Should return 2 Component Pick Lines.", 2, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 20m && l.ProductPK == wheel.PK));
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 9m && l.ProductPK == frame.PK));

				AssertEquals("All orders associated with the trolley should be returned.", 1, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single());

				AssertEquals("All not picked Products associated with the trolley should be returned, including BOM Product.", 3, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "WHEEL"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "FRAME"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "BIKE"));
			});
		}

		public void TestGetTrolley_Picking_PickByBOM_Tote_OneKindOfComponentsWereAllPickedAlready()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);
			var framePickLine = kitOrder.Lines.Single(l => l.WE_OP == frame.PK).PickLines.Single();
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			pkg1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg1, kitOrderLine1.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(1, trolleyJobInfo.Slots.Count);

				AssertEquals("Should return 1 Component Pick Lines.", 1, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 20m && l.ProductPK == wheel.PK));

				AssertEquals("All orders associated with the trolley should be returned.", 1, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single());

				AssertEquals("All not picked Products associated with the trolley should be returned, including BOM Product.", 2, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "WHEEL"));
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "BIKE"));
			});
		}

		public void TestGetTrolley_Picking_PickByBOM_Tote_AllComponentsWereAllPickedAlready()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 20m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 10m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Helper.Factory.Save();
			var kitOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT1", bike, 10m);
			var kitOrderLine1 = kitOrder.Lines[0];
			var pick = Helper.CreatePickNew(kitOrder);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var framePickLine = kitOrder.Lines.Single(l => l.WE_OP == frame.PK).PickLines.Single();
			var wheelPickLine = kitOrder.Lines.Single(l => l.WE_OP == wheel.PK).PickLines.Single();
			framePickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Helper.Factory.Save();

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(kitOrder);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			pkg1.SetIsTote(true);
			packingHelper.CreatePackageDivot(pkg1, kitOrderLine1.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should be putaway only.", true, trolleyJobInfo.IsPutawayOnly);
			});
		}

		#endregion

		#region TestGetTrolley_Picking_RequirePutaway

		public void TestGetTrolley_Picking_RequirePutaway_Tote()
		{
			TestGetTrolley_Picking_RequirePutaway(TrolleyPickingType.Tote);
		}

		public void TestGetTrolley_Picking_RequirePutaway_Carton()
		{
			TestGetTrolley_Picking_RequirePutaway(TrolleyPickingType.Carton);
		}

		void TestGetTrolley_Picking_RequirePutaway(TrolleyPickingType trolleyType)
		{
			// setup
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");

			// create inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// setup orders and picks
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_GS_NKAssignedTo = staff.GS_Code;
			});

			// setup packages for order 1
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, orderLine.PickLines.Single());

			// set up totes
			bool isToteTrolley = trolleyType == TrolleyPickingType.Tote;
			package.SetIsTote(isToteTrolley);

			// create trolley and trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// links a package with a trolley/trolleyjob		
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1); // with Product P1
			Helper.Factory.Save();

			Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, trolleyType);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should be putaway only.", true, trolleyJobInfo.IsPutawayOnly);
			});
		}

		#endregion

		#region TestGetTrolley_Picking_RequirePutaway_AssignedToOtherStaff

		public void TestGetTrolley_Picking_RequirePutaway_AssignedToOtherStaff_Tote()
		{
			TestGetTrolley_Picking_RequirePutaway_AssignedToOtherStaff(TrolleyPickingType.Tote);
		}

		public void TestGetTrolley_Picking_RequirePutaway_AssignedToOtherStaff_Carton()
		{
			TestGetTrolley_Picking_RequirePutaway_AssignedToOtherStaff(TrolleyPickingType.Carton);
		}

		void TestGetTrolley_Picking_RequirePutaway_AssignedToOtherStaff(TrolleyPickingType trolleyType)
		{
			// setup
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("A", "A");
			var staff2 = Helper.CreateGlbStaff("B", "B");

			// create inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// setup orders and picks
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_GS_NKAssignedTo = staff1.GS_Code;
			});

			// setup packages for order 1
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, orderLine.PickLines.Single());

			// set up totes
			bool isToteTrolley = trolleyType == TrolleyPickingType.Tote;
			package.SetIsTote(isToteTrolley);

			// create trolley and trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// links a package with a trolley/trolleyjob		
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1); // with Product P1
			Helper.Factory.Save();

			var transferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			transferLine.WE_GS_NKPutawayBy = staff2.GS_Code;
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, trolleyType);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertNull(response.Job);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All packages for this trolley are already picked. You need to build a new trolley.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetTrolley_Picking_AlreadyPutaway

		public void TestGetTrolley_Picking_AlreadyPutaway_Tote()
		{
			TestGetTrolley_Picking_AlreadyPutaway(TrolleyPickingType.Tote);
		}

		public void TestGetTrolley_Picking_AlreadyPutaway_Carton()
		{
			TestGetTrolley_Picking_AlreadyPutaway(TrolleyPickingType.Carton);
		}

		void TestGetTrolley_Picking_AlreadyPutaway(TrolleyPickingType trolleyType)
		{
			// setup
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");

			// create inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// setup orders and picks
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_GS_NKAssignedTo = staff.GS_Code;
			});

			// setup packages for order 1
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, orderLine.PickLines.Single());

			// set up totes
			bool isToteTrolley = trolleyType == TrolleyPickingType.Tote;
			package.SetIsTote(isToteTrolley);

			// create trolley and trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// links a package with a trolley/trolleyjob		
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1); // with Product P1
			Helper.Factory.Save();

			var transferLine = Helper.PickAndMakeInTransitTransfer(orderLine.PickLines.Single(), ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, trolleyType);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertNull(response.Job);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("All packages for this trolley are already picked. You need to build a new trolley.", response.ErrorMessage);
			});
		}

		#endregion

		#region TestGetTrolley_Picking_RequirePutaway_PartiallyPicked

		public void TestGetTrolley_Picking_RequirePutaway_PartiallyPicked_Tote()
		{
			TestGetTrolley_Picking_RequirePutaway_PartiallyPicked(TrolleyPickingType.Tote);
		}

		public void TestGetTrolley_Picking_RequirePutaway_PartiallyPicked_Carton()
		{
			TestGetTrolley_Picking_RequirePutaway_PartiallyPicked(TrolleyPickingType.Carton);
		}

		void TestGetTrolley_Picking_RequirePutaway_PartiallyPicked(TrolleyPickingType trolleyType)
		{
			// setup
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");

			// create inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// setup orders and picks
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_GS_NKAssignedTo = staff.GS_Code;
			});

			// setup packages for order 1
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(package, orderLine1.PickLines.Single());
			packingHelper.CreatePackageDivot(package, orderLine2.PickLines.Single());

			// set up totes
			bool isToteTrolley = trolleyType == TrolleyPickingType.Tote;
			package.SetIsTote(isToteTrolley);

			// create trolley and trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			// links a package with a trolley/trolleyjob		
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1); // with Product P1
			Helper.Factory.Save();

			Helper.PickAndMakeInTransitTransfer(orderLine1.PickLines.Single(), ZDateTimeOffset.Now);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, trolleyType);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("Reference should be populated.", "T001", trolleyJobInfo.Reference);
				AssertEquals("Should *not* be putaway only.", false, trolleyJobInfo.IsPutawayOnly);
				AssertNotNull("Slots have returned slots.", trolleyJobInfo.Slots);
			});
		}

		#endregion

		#region TestGetTrolley_Picking_CorrectType

		public void TestGetTrolley_Picking_CorrectType_Tote_NonePickType()
		{
			var whs = Helper.CreateWarehouse("WH1");

			// don't need to add orders, picks or packages here since the trolley is empty
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.Factory.Save();

			AssertEquals("Precondition", trolleyJob.PickingType, TrolleyPickingType.None);

			var webService = GetNewWebService(whs);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should return BusinessValidationError", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should have identical error messages", $"Couldn't find existing trolley with pick type '{TrolleyPickingType.Tote}'.", response.ErrorMessage);
				AssertNull("Response job should be null.", response.Job);
			});
		}

		public void TestGetTrolley_Picking_CorrectType_Carton_NonePickType()
		{
			var whs = Helper.CreateWarehouse("WH1");

			// don't need to add orders, picks or packages here since the trolley is empty
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.Factory.Save();

			AssertEquals("Precondition", trolleyJob.PickingType, TrolleyPickingType.None);

			var webService = GetNewWebService(whs);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should return BusinessValidationError", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should have identical error messages", $"Couldn't find existing trolley with pick type '{TrolleyPickingType.Carton}'.", response.ErrorMessage);
				AssertNull("Response job should be null.", response.Job);
			});
		}

		public void TestGetTrolley_Picking_CorrectType_None_NonePickType()
		{
			var whs = Helper.CreateWarehouse("WH1");

			// don't need to add orders, picks or packages here since the trolley is empty
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.Factory.Save();

			AssertEquals("Precondition", trolleyJob.PickingType, TrolleyPickingType.None);

			var webService = GetNewWebService(whs);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.None);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Should return BusinessValidationError", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Should have identical error messages", "All packages for this trolley are already picked. You need to build a new trolley.", response.ErrorMessage); // none-type empty trolleys are auto-finalised
				AssertNull("Response job should be null.", response.Job);
			});
		}

		public void TestGetTrolley_Picking_CorrectType_Carton()
		{
			TestGetTrolley_Picking_CorrectType_Core(TrolleyPickingType.Carton);
		}

		public void TestGetTrolley_Picking_CorrectType_Tote()
		{
			TestGetTrolley_Picking_CorrectType_Core(TrolleyPickingType.Tote);
		}

		void TestGetTrolley_Picking_CorrectType_Core(TrolleyPickingType trolleyType)
		{
			AssertNotEquals("This test is not designed for the None picking type", TrolleyPickingType.None, trolleyType);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// Continue building existing TrolleyJob
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save(); // to create stock for picking

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition", 10m, pickLine.WZ_Units);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			pkg.SetIsTote(trolleyType == TrolleyPickingType.Tote);
			packingHelper.CreatePackageDivot(pkg, pickLine);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, pkg.PK, 5);

			Helper.Factory.Save(); // to create stock
			AssertEquals("Precondition", trolleyJob.PickingType, trolleyType);

			// request whats in the DB - success
			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, true, trolleyType);
			AssertSuccessfulResponse(response1, webService1);
			AssertNotNull("Should find Trolley Job and return it to RF gun.", response1.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should return no error", ErrorTypes.None, response1.Error);
				AssertEquals($"Should return '{trolleyType}' Trolley", trolleyType, response1.Job.TrolleyPickType);
			});

			// flips the trolley pick type to the 'opposite' type
			var oppositeTrolleyPickType = trolleyType == TrolleyPickingType.Tote ? TrolleyPickingType.Carton : TrolleyPickingType.Tote;

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetTrolley("T001", TrolleyJobStatus.Picking, true, oppositeTrolleyPickType);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Should return BusinessValidationError", ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Should have identical error messages", $"Couldn't find existing trolley with pick type '{oppositeTrolleyPickType}'.", response2.ErrorMessage);
				AssertNull("Response job should be null.", response2.Job);
			});

			// None type requested
			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.None);
			AssertSuccessfulResponse(response3, webService3);
			AssertNotNull("Should find Trolley Job and return it to RF gun.", response3.Job);

			CombineAssertions(() =>
			{
				AssertEquals("Should return no error", ErrorTypes.None, response3.Error);
				AssertEquals($"Should return '{trolleyType}' Trolley", trolleyType, response3.Job.TrolleyPickType);
			});
		}

		#region TestGetTrolley_Picking_ShouldOverrideOtherUsers

		public void TestGetTrolley_Picking_ShouldOverrideOtherUsers()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");
			var staff2 = Helper.CreateGlbStaff("BBB", "B.B");
			var staff3 = Helper.CreateGlbStaff("CCC", "C.C");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// create order 1 with one picked and one unpicked package
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity = Helper.GetTotalPickLineQuantity(pick);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			orderLine1.PickLines.Single().WZ_GS_NKAssignedTo = staff1.GS_Code;
			orderLine2.PickLines.Single().WZ_GS_NKAssignedTo = staff1.GS_Code;
			AssertEquals("Precondition", 3m, totalPickLineQuantity);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, orderLine1.PickLines.Single());
			packingHelper.CreatePackageDivot(pkg1, orderLine2.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.Factory.Save();

			var expectedErrorSingleUserMessage = "Some of the items are assigned to be picked by 'AAA'. Would you like to assign all items to yourself?";
			var expectedErrorMultiUserMessage = "Some of the items are assigned to be picked by 'AAA', 'BBB'. Would you like to assign all items to yourself?";

			// Without override single user
			var webService1 = GetNewWebService(data.Whs1, staff3);
			var response = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.YesNoEnquiry, response.Error);
				AssertEquals("No errors should be returned.", expectedErrorSingleUserMessage, response.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response.Job);
			});

			// Without override multi user
			orderLine2.PickLines.Single().WZ_GS_NKAssignedTo = staff2.GS_Code;
			Helper.Factory.Save();
			var webService2 = GetNewWebService(data.Whs1, staff3);
			var response2 = webService2.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.YesNoEnquiry, response2.Error);
				AssertEquals("No errors should be returned.", expectedErrorMultiUserMessage, response2.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response2.Job);
			});

			// check that pick lines are still allocated to previous users.
			var allPicklinesQuery = new ZQuery();
			allPicklinesQuery.AddToFilter(WhsPickLineSchema.PK, order.Lines.SelectMany(l => l.PickLines).Select(l => l.PK));

			var otherFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLinesInOtherFactory1 = otherFactory1.Load<WhsPickLine>(allPicklinesQuery);
			AssertEquals("Precondition", 2, pickLinesInOtherFactory1.Length);
			AssertNoExceptionThrown("Precondition", () => pickLinesInOtherFactory1.Single(l => l.WZ_Units == 1m && l.WZ_GS_NKAssignedTo == staff1.GS_Code));
			AssertNoExceptionThrown("Precondition", () => pickLinesInOtherFactory1.Single(l => l.WZ_Units == 2m && l.WZ_GS_NKAssignedTo == staff2.GS_Code));

			// with override
			var webService3 = GetNewWebService(data.Whs1, staff3);
			var response3 = webService3.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response3, webService3);
			var trolleyJobInfo = response3.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response3.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response3.ErrorMessage));
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(1, trolleyJobInfo.Slots.Count);

				AssertEquals("All not picked lines attached to the trolley job should be returned.", 1, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 3m)); // pick lines should be grouped

				AssertEquals("All orders associated with the trolley should be returned.", 1, trolleyJobInfo.Orders.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Orders.Single(o => o.ExternalReference == "O1" && o.Lines.Count == 0));

				AssertEquals("All not picked Products associated with the trolley should be returned.", 1, trolleyJobInfo.ProductInfos.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.ProductInfos.Single(p => p.Code == "P1"));

				AssertEquals("All not picked Products Attributes associated with the trolley should be returned.", 1, trolleyJobInfo.ProductPartAttributesInfos.Count);

				AssertEquals("Unit conversions need to be populated for picking trolley, but only for Not Picked products.", 1, trolleyJobInfo.UnitConversionsPerProduct.Count);
				AssertEquals("All Unit conversions need to be populated for Not Picked products.", 2, trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Unit && c.Qty == 1m));
				AssertNoExceptionThrown(() => trolleyJobInfo.UnitConversionsPerProduct.Single().Conversions.Single(c => c.PackType == Constants.PkgUnit.Carton && c.Qty == 12m));

				AssertEquals("Picking information should be populated for picking trolley.", true, trolleyJobInfo.IsPickByBiggestPackTypeEnabled);
				AssertEquals("Picking information should be populated for picking trolley.", true, trolleyJobInfo.IsPickByUOMTypeEnabled);
			});

			// check that pick lines are now allocated to RF user.
			var otherFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickLinesInOtherFactory2 = otherFactory2.Load<WhsPickLine>(allPicklinesQuery);

			CombineAssertions(() =>
			{
				AssertEquals(2, pickLinesInOtherFactory2.Length);
				AssertNoExceptionThrown(() => pickLinesInOtherFactory2.Single(l => l.WZ_Units == 1m && l.WZ_GS_NKAssignedTo == staff3.GS_Code));
				AssertNoExceptionThrown(() => pickLinesInOtherFactory2.Single(l => l.WZ_Units == 2m && l.WZ_GS_NKAssignedTo == staff3.GS_Code));
			});
		}

		#endregion

		#region TestGetTrolley_Picking_ShouldOverrideOtherUser_IsPicking

		public void TestGetTrolley_Picking_ShouldOverrideOtherUser_IsPicking()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("AAA", "A.A");
			var staff2 = Helper.CreateGlbStaff("BBB", "B.B");
			var staff3 = Helper.CreateGlbStaff("CCC", "C.C");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// create order 1 with one picked and one unpicked package
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var totalPickLineQuantity1 = Helper.GetTotalPickLineQuantity(pick);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			orderLine1.PickLines.Single().WZ_GS_NKAssignedTo = staff1.GS_Code;
			orderLine1.PickLines.Single().WZ_IsPicking = true;
			orderLine2.PickLines.Single().WZ_GS_NKAssignedTo = staff1.GS_Code;
			orderLine2.PickLines.Single().WZ_IsPicking = true;
			AssertEquals("Precondition", 3m, totalPickLineQuantity1);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, orderLine1.PickLines.Single());
			packingHelper.CreatePackageDivot(pkg, orderLine2.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 1);
			Helper.Factory.Save();

			var expectedErrorSingleUserMessage = "Some of the items are in process of being picked by 'AAA'. All items should be picked by the same person.";
			var expectedErrorMultiUserMessage = "Some of the items are in process of being picked by 'AAA', 'BBB'. All items should be picked by the same person.";

			var webService1 = GetNewWebService(data.Whs1, staff3);
			var response1 = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("User 'AAA' is picking. 'CCC' sould not be able to start picking.", ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("User 'AAA' is picking. 'CCC' sould not be able to start picking.", expectedErrorSingleUserMessage, response1.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response1.Job);
			});

			// reassign to different user
			orderLine2.PickLines.Single().WZ_GS_NKAssignedTo = "";
			orderLine2.PickLines.Single().WZ_IsPicking = false;
			Helper.Factory.Save();
			orderLine2.PickLines.Single().WZ_GS_NKAssignedTo = staff2.GS_Code;
			orderLine2.PickLines.Single().WZ_IsPicking = true;
			Helper.Factory.Save();
			// start picking 

			var webService2 = GetNewWebService(data.Whs1, staff3);
			var response2 = webService2.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("User 'AAA','BBB' are picking. 'CCC' sould not be able to start picking.", ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("User 'AAA','BBB' are picking. 'CCC' sould not be able to start picking.", expectedErrorMultiUserMessage, response2.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response2.Job);
			});
		}

		#endregion

		#region TestGetTrolley_Picking_WithPickLinesCommittingDockDoorStock

		public void TestGetTrolley_Picking_WithPickLinesCommittingDockDoorStock()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// create order 1 with one picked and one unpicked package
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m);
			var pick = Helper.CreatePickNew(order1, order2);

			pick.GetAllPickLines().ToArray().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			var pkg2 = packingHelper.CreatePackage(pkgJob2, "PKG2", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, order1.Lines[0].PickLines.Single());
			packingHelper.CreatePackageDivot(pkg2, order2.Lines[0].PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			Helper.PickAndMakeInTransitTransfer(order2.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertNotNull("Slots should be returned for picking.", trolleyJobInfo.Slots);
				AssertEquals(2, trolleyJobInfo.Slots.Count);
				AssertEquals("Should include all orders related to the trolley.", 2, trolleyJobInfo.Orders.Count);

				AssertEquals("All not picked lines attached to the trolley job should be returned.", 1, trolleyJobInfo.Lines.Count);
				AssertNoExceptionThrown(() => trolleyJobInfo.Lines.Single(l => l.Units == 2m));
			});
		}

		#endregion

		#region TestGetTrolley_Picking_WhenAllPickLinesAlreadyPicked

		public void TestGetTrolley_Picking_WhenAllPickLinesAlreadyPicked()
		{
			TestGetTrolley_Picking_WhenAllPickLinesAlreadyPicked(usingInTransitTransfer: false);
		}

		public void TestGetTrolley_Picking_WhenAllPickLinesAlreadyPicked_WithInTransitTransfers()
		{
			TestGetTrolley_Picking_WhenAllPickLinesAlreadyPicked(usingInTransitTransfer: true);
		}

		void TestGetTrolley_Picking_WhenAllPickLinesAlreadyPicked(bool usingInTransitTransfer)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// create order 1 with one picked and one unpicked package
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);

			pick.GetAllPickLines().ToArray().ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;

				if (usingInTransitTransfer)
				{
					var transferLine = Helper.PickAndMakeInTransitTransfer(l, ZDateTimeOffset.Now);
					transferLine.FinaliseDocketLine();
					AssertIsFinalisedPrecondition(transferLine);
				}
				else
				{
					l.WZ_PickedDateTime = ZDateTimeOffset.Now;
				}
			});

			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Helper.Factory.Save();
			}

			if (!usingInTransitTransfer)
			{
				AssertEquals("Precondition: No dock door transfers created.", 0, Helper.Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForTransfer, SQLComparisonOperator.NotEqual, ZGuid.Empty)).Length);
			}

			var totalPickLineQuantity1 = Helper.GetTotalPickLineQuantity(pick);
			AssertEquals("Precondition", 1m, totalPickLineQuantity1);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg, orderLine.PickLines.Single());

			// assign packages to trolley
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg, 1);
			Helper.Factory.Save();

			// Trying to pick a trolley that have all packages fully picked.
			var expectedInformationMessage = "All packages for this trolley are already picked. You need to build a new trolley.";

			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Error should be returned.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error should be returned.", expectedInformationMessage, response.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response.Job);
			});
		}

		#endregion

		#region TestGetTrolley_Picking_SortPickLines

		public void TestGetTrolley_Picking_SortPickLines()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[0]);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, locations[1]);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, locations[1]);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, locations[1]);
			receive.FinaliseDocket();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			var pickLine1 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 1m); // priority location but not trolley slot
			var pickLine2 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 2m); // slot 4
			var pickLine3 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 3m); // slot 2
			var pickLine4 = order.Lines[0].PickLines.Single(p => p.WZ_Units == 4m); // slot 3

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box); // pick line 1 & 2
			var pkg2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Box); // pick line 3
			var pkg3 = packingHelper.CreatePackage(pkgJob, "PKG3", 1, Constants.PkgUnit.Box); // pick line 4
			packingHelper.CreatePackageDivot(pkg1, pickLine1);
			packingHelper.CreatePackageDivot(pkg1, pickLine2);
			packingHelper.CreatePackageDivot(pkg2, pickLine3);
			packingHelper.CreatePackageDivot(pkg3, pickLine4);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			var trolleySlot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg1, 4);
			var trolleySlot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg2, 2);
			var trolleySlot3 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg3, 3);
			Helper.Factory.Save();

			// ensuring that pick lines are sorted before being send to the RF gun.
			var webService = GetNewWebService(data.Whs1);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertSuccessfulResponse(response, webService);
			var trolleyJobInfo = response.Job;
			AssertNotNull(trolleyJobInfo);

			CombineAssertions(() =>
			{
				AssertEquals("No errors should be returned.", ErrorTypes.None, response.Error);
				AssertEquals("No errors should be returned.", true, string.IsNullOrEmpty(response.ErrorMessage));
				AssertEquals("All not picked lines attached to the trolley job should be returned.", 4, trolleyJobInfo.Lines.Count);
				AssertEquals(pickLine1.PK, trolleyJobInfo.Lines[0].PKs.Single()); // priority location
				AssertEquals(pickLine3.PK, trolleyJobInfo.Lines[1].PKs.Single()); // slot 2
				AssertEquals(pickLine4.PK, trolleyJobInfo.Lines[2].PKs.Single()); // slot 3
				AssertEquals(pickLine2.PK, trolleyJobInfo.Lines[3].PKs.Single()); // slot 4
			});
		}

		#endregion

		#region TestGetTrolley_InvalidParameters

		public void TestGetTrolley_InvalidParameters_Carton()
		{
			TestGetTrolley_InvalidParameters_Core(TrolleyPickingType.Carton);
		}

		public void TestGetTrolley_InvalidParameters_Tote()
		{
			TestGetTrolley_InvalidParameters_Core(TrolleyPickingType.Tote);
		}

		public void TestGetTrolley_InvalidParameters_None()
		{
			TestGetTrolley_InvalidParameters_Core(TrolleyPickingType.None);
		}

		void TestGetTrolley_InvalidParameters_Core(TrolleyPickingType trolleyType)
		{
			var whs = Helper.CreateWarehouse("WH1");

			var webService1 = GetNewWebService(whs);
			var response1 = webService1.GetTrolley(null, TrolleyJobStatus.Building, true, trolleyType);
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Please provide a Trolley Number.", response1.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response1.Job);
			});

			var webService2 = GetNewWebService(whs);
			var response2 = webService2.GetTrolley("", TrolleyJobStatus.Building, true, trolleyType);
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Please provide a Trolley Number.", response2.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response2.Job);
			});

			var webService3 = GetNewWebService(whs);
			var response3 = webService3.GetTrolley("T001", TrolleyJobStatus.Building, true, trolleyType);
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response3.Error);
				AssertEquals("Trolley 'T001' cannot be found.", response3.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response3.Job);
			});

			// Cannot Pick trolley if it is Building or all jobs for the Trolley are Finalised.
			var trolley1 = Helper.CreateTrolley("T1");
			Helper.CreateWhsPickTrolleyJob(trolley1, PickTrolleyStatus.Codes.Finalised);
			Helper.Factory.Save();

			var webService4 = GetNewWebService(whs);
			var response4 = webService4.GetTrolley("T1", TrolleyJobStatus.Picking, true, trolleyType);
			AssertSuccessfulResponse(response4, webService4);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response4.Error);
				AssertEquals("Trolley 'T1' is not available for picking. You need to finish Building that Trolley first.", response4.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response4.Job);
			});

			Helper.CreateWhsPickTrolleyJob(trolley1, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var webService5 = GetNewWebService(whs);
			var response5 = webService5.GetTrolley("T1", TrolleyJobStatus.Picking, true, trolleyType);
			AssertSuccessfulResponse(response5, webService5);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response5.Error);
				AssertEquals("Trolley 'T1' is not available for picking. You need to finish Building that Trolley first.", response5.ErrorMessage);
				AssertNull("No trolley job should be returned for picking.", response5.Job);
			});
		}

		#endregion

		#endregion

		#region TestGetTrolley_DeleteFinalEmptyPackageFromTrolley_ThenTrolleyCanBeFound

		public void TestGetTrolley_DeleteFinalEmptyPackageFromTrolley_ThenTrolleyCanBeFound()
		{
			// setup
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");

			// create inventory
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.Factory.Save(); // to create stock

			// setup orders and picks
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.GetAllPickLines().ForEach(l =>
			{
				l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit;
				l.WZ_GS_NKAssignedTo = staff.GS_Code;
			});

			// setup packages for order 1
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package.SetIsTote(true);
			var divot = packingHelper.CreatePackageDivot(package, orderLine.PickLines.Single());

			// create trolley and trolley job
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Picking;

			// links a package with a trolley/trolleyjob
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 1); // with Product P1
			Helper.Factory.Save();

			// Getting trolley for Building
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response, webService);
			AssertNull("New trolley job should Not have been created.", response.Job);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Trolley 'T001' is not available for building. You need to finish Picking this Trolley first, or investigate the status of the trolley by looking up the Trolley # in CW1.", response.ErrorMessage);
			});

			// Delete divot and Package
			divot.Delete();
			package.Delete();
			Helper.Factory.Save();

			// Getting trolley for Building again
			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.GetTrolley("T001", TrolleyJobStatus.Building, true, TrolleyPickingType.Tote);
			AssertSuccessfulResponse(response2, webService2);

			var newTrolleyJobInfo = response2.Job;
			AssertNotNull("New trolley job should have been created.", newTrolleyJobInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Successful trolley job creation should have no Error.", ErrorTypes.None, response2.Error);
				AssertEquals("No error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

				AssertEquals("New TrolleyJob created is different from old TrolleyJob", true, newTrolleyJobInfo.PK != trolleyJob.PK);
				AssertEquals("Old TrolleyJob is finalised", PickTrolleyStatus.Codes.Finalised, trolleyJob.WTJ_Status);
			});
		}

		#endregion

		#region TestGetTrolley_DBHits

		public void TestGetTrolley_DBHits()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 10, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var products = new List<OrgSupplierPart>(10);
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct(data.Org1, "PP" + i);
				products.Add(product);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, data.Whs1.FindLocation("A-" + (i + 1)));
				receive.FinaliseDocketWithoutUserConfirmation();
			}
			Helper.Factory.Save();

			var orders = new List<WhsOrder>(100);
			var pickLines = new List<WhsPickLine>(100);
			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i, Notify);
				for (var j = 0; j < 10; j++)
				{
					Helper.CreateWhsOrderLine(order, products[i], 1m);
				}
				orders.Add(order);
				var pick = Helper.CreatePickNew(order);
				pickLines.AddRange(pick.GetAllPickLines());
			}
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);

			for (var i = 0; i < 10; i++)
			{
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(orders[i]);
				var packageTote = packingHelper.CreatePackage(packageJob, "Tote" + i, 1, Constants.PkgUnit.Box);
				packageTote.SetIsTote(true);
				Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageTote.PK, (short)(i + 1));

				for (var j = 0; j < 10; j++)
				{
					var pickLine = pickLines[i * 10 + j];
					packingHelper.CreatePackageDivot(packageTote, pickLine);

					pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
					pickLine.WZ_IsPicking = true;
				}
			}

			Helper.Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 2 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ RefEquipmentSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 6 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsPickTrolleyJobSchema.Constants.TableName, 1 },
				{ WhsPickTrolleySlotSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
			};

			var webService1 = GetNewWebService(data.Whs1, staff);

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService1.Factory))
			{
				var response = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Tote);
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals("T001", response.Job.Reference);
			}
		}

		public void TestGetTrolley_DBHits_PackingConsolidationCheck()
		{
			var staff = Helper.CreateGlbStaff("GS1", "GS1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 10, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var products = new List<OrgSupplierPart>(10);
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct(data.Org1, "PP" + i);
				products.Add(product);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				Helper.CreateWhsReceiveInventoryLine(receive, product, 10m, data.Whs1.FindLocation("A-" + (i + 1)));
				receive.FinaliseDocketWithoutUserConfirmation();
			}
			Helper.Factory.Save();

			var orders = new List<WhsOrder>(100);
			var pickLines = new List<WhsPickLine>(100);
			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i, Notify);
				order.WD_UseDirectedPackingConsolidation = true;

				for (var j = 0; j < 10; j++)
				{
					Helper.CreateWhsOrderLine(order, products[i], 1m);
				}

				orders.Add(order);
				var pick = Helper.CreatePickNew(order);
				pickLines.AddRange(pick.GetAllPickLines());
			}
			Helper.Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, PickTrolleyStatus.Codes.Picking);

			for (var i = 0; i < 10; i++)
			{
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(orders[i]);
				var packageCarton = packingHelper.CreatePackage(packageJob, "Tote" + i, 1, Constants.PkgUnit.Box);
				Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, packageCarton.PK, (short)(i + 1));

				for (var j = 0; j < 10; j++)
				{
					var pickLine = pickLines[i * 10 + j];
					packingHelper.CreatePackageDivot(packageCarton, pickLine);

					pickLine.WZ_GS_NKAssignedTo = staff.GS_Code;
					pickLine.WZ_IsPicking = true;
				}
			}

			Helper.Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageHeaderSchema.Constants.TableName, 2 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ RefEquipmentSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 7 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsPickTrolleyJobSchema.Constants.TableName, 1 },
				{ WhsPickTrolleySlotSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
			};

			var webService1 = GetNewWebService(data.Whs1, staff);

			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService1.Factory))
			{
				var response = webService1.GetTrolley("T001", TrolleyJobStatus.Picking, false, TrolleyPickingType.Carton);
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals("T001", response.Job.Reference);
			}
		}
		#endregion

		#region TestGetTrolley_FactoryConcurrencySaveError

		public void TestGetTrolley_FactoryConcurrencySaveError()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Helper.Factory.Save(); // to create stock

			// create order 1 with one picked and one unpicked package
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var totalPickLineQuantity1 = Helper.GetTotalPickLineQuantity(pick1);
			AssertEquals("Precondition", 3m, totalPickLineQuantity1);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var pkg1 = packingHelper.CreatePackage(pkgJob1, "PKG1", 1, Constants.PkgUnit.Box);
			var pkg2 = packingHelper.CreatePackage(pkgJob1, "PKG2", 1, Constants.PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg1, order1Line1.PickLines.Single());
			packingHelper.CreatePackageDivot(pkg2, order1Line2.PickLines.Single()); // package not assigned to trolley

			// assign packages to trolley
			var trolley = Helper.CreateEquipment("T001", 1, Constants.Weight.Kilograms, 1, Constants.Volume.Litre);
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.Factory.Save();

			// Getting trolley for picking
			var webService = GetNewWebService(data.Whs1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pick1.GetAllPickLines().First()).Row, TestConnection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.GetTrolley("T001", TrolleyJobStatus.Picking, true, TrolleyPickingType.Carton);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has modified the trolley job. Please restart the operation and try again.", response.ErrorMessage);
		}

		#endregion

		#endregion
	}
}

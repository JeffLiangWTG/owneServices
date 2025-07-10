using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.Warehouse.Web.WebService;
using Enterprise.Warehouse.Web.WebService.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Web.Testing
{
	class RemovePackageFromTrolleyUsingPackageIDTest : WhsSecureServiceTestCase
	{
		#region TestRemovePackageFromTrolleyUsingPackageID

		public void TestRemovePackageFromTrolleyUsingPackageID()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 10m, order.Lines[0].PickLineQuantity);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box); // some other package to make sure that correct one will be used
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			var packageCCC = packingHelper.CreatePackage(pkgJob, "CCC", 1, Constants.PkgUnit.Box);
			var packageDDD = packingHelper.CreatePackage(pkgJob, "DDD", 1, Constants.PkgUnit.Box);

			packageCCC.SetIsTote(true);
			packageDDD.SetIsTote(true);

			var picklines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(packageABC, picklines[0]);
			packingHelper.CreatePackageDivot(packageDDD, picklines[1]);

			var releaseLine = (IPackableItemParent)order.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();
			Assert("The package is packed", packageDDD.IsPacked(releaseLine));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2, PickTrolleyStatus.Codes.Building);

			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageAAA, 1);
			var slot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageABC, 2);
			var slot3 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageDDD, 3);

			Helper.Factory.Save();

			AssertEquals(3, trolleyJob.Slots.Count);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "DDD");
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response1.Error);
				AssertEquals(true, string.IsNullOrEmpty(response1.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newTrolleyJob1 = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new[] { "ABC", "AAA" }, newTrolleyJob1.Slots.Select(sl => sl.Package.KP_PackageID.ToString()));
				AssertEquals("The package is unpacked", false, packageDDD.IsPacked(releaseLine));
				AssertEquals("The row should have been deleted", false, newFactory.ExistsInDatabase(GenAddOnColumnSchema.Constants.TableName, new ZQuery(GenAddOnColumnSchema.XA_ParentID, packageDDD.PK)));
				AssertEquals("The pkg should have been deleted", false, newFactory.ExistsInDatabase(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.PK, packageDDD.PK)));
			});

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "AAA");
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response2.Error);
				AssertEquals(true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var newTrolleyJob = newFactory1.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			AssertContainsExactElementsInAnyOrder(new[] { "ABC" }, newTrolleyJob.Slots.Select(sl => sl.Package.KP_PackageID.ToString()));

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC");
			AssertSuccessfulResponse(response3, webService3);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response3.Error);
				AssertEquals(true, string.IsNullOrEmpty(response3.ErrorMessage));
			});

			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			newTrolleyJob = newFactory2.Load<WhsPickTrolleyJob>(trolleyJob.PK);
			AssertEquals("No packages in Trolley", 0, newTrolleyJob.Slots.Count);
		}

		public void TestRemovePackageFromTrolleyUsingPackageID_ToteNotOnTrolley()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 10m, order.Lines[0].PickLineQuantity);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box); // some other package to make sure that correct one will be used
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			var packageCCC = packingHelper.CreatePackage(pkgJob, "CCC", 1, Constants.PkgUnit.Box);
			var packageDDD = packingHelper.CreatePackage(pkgJob, "DDD", 1, Constants.PkgUnit.Box);

			packageCCC.SetIsTote(true);
			packageDDD.SetIsTote(true);

			var picklines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(packageABC, picklines[0]);
			packingHelper.CreatePackageDivot(packageDDD, picklines[1]);

			var releaseLine = (IPackableItemParent)order.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();
			Assert("The package is packed", packageDDD.IsPacked(releaseLine));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2, PickTrolleyStatus.Codes.Building);

			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageAAA, 1);
			var slot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageABC, 2);
			var slot3 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageDDD, 3);

			Helper.Factory.Save();

			AssertEquals(3, trolleyJob.Slots.Count);

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.RemovePackageFromTrolleyUsingPackageID(trolleyJob2.PK.ToGuid(), "EEE");
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Package 'EEE' was not on this Trolley.", response1.ErrorMessage);

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "CCC");
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Tote 'CCC' was not on this Trolley.", response2.ErrorMessage);

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "EEE");
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals(ErrorTypes.BusinessValidationError, response3.Error);
			AssertEquals("Tote 'EEE' was not on this Trolley.", response3.ErrorMessage);
		}

		public void TestRemovePackageFromTrolleyUsingPackageID_DockDoorAssignment()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			packageABC.SetIsTote(true);

			packingHelper.CreatePackageDivot(packageABC, pick.GetAllPickLines().Single());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, packageABC, 1);
			Helper.Factory.Save();

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Helper.Factory.Save();

			AssertEquals(1, trolleyJob.Slots.Count);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newTrolleyJob = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);

			CombineAssertions(() =>
			{
				AssertEquals("No slots should be left", 0, newTrolleyJob.Slots.Count);
				AssertEquals("The row should have been deleted", false, newFactory.ExistsInDatabase(GenAddOnColumnSchema.Constants.TableName, new ZQuery(GenAddOnColumnSchema.XA_ParentID, packageABC.PK)));
				AssertEquals("The pkg should have been deleted", false, newFactory.ExistsInDatabase(PkgPackageSchema.Constants.TableName, new ZQuery(PkgPackageSchema.PK, packageABC.PK)));

				var dockDoorAssignments = webService.Factory.Load<WhsDockDoorAssignment>(new ZQuery());
				AssertEquals("No DDA exists", 0, dockDoorAssignments.Length);

				AssertEquals("Pick DDA cleared", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
			});
		}

		public void TestRemovePackageFromTrolleyUsingPackageID_DockDoorAssignment_Error()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			packageABC.SetIsTote(true);

			packingHelper.CreatePackageDivot(packageABC, pick.GetAllPickLines().Single());

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, packageABC, 1);
			Helper.Factory.Save();

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Helper.Factory.Save();

			AssertEquals(1, trolleyJob.Slots.Count);

			var ddaService = new Mock<IWhsPickDockDoorAssignmentService>();
			ddaService.Setup(m =>
				m.RemovePickDockDoorAssignment(
					It.IsAny<ZGuid>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns("Error");

			using (ObjectFactory.Substitute(ddaService.Object))
			{
				var webService = GetNewWebService(data.Whs1);
				var response = webService.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC");
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error", response.ErrorMessage);
			}
		}

		#endregion

		#region TestRemovePackageFromTrolleyUsingPackageID_TrolleyNotExist

		public void TestRemovePackageFromTrolleyUsingPackageID_TrolleyNotExist()
		{
			var whs = Helper.CreateWarehouse("WHS");

			var webService = GetNewWebService(whs);
			var response = webService.RemovePackageFromTrolleyUsingPackageID(Guid.NewGuid(), "ABC");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Trolley job was not found. Please start building Trolley again.", response.ErrorMessage);
		}

		#endregion

		#region TestRemovePackageFromTrolleyUsingPackageID_WrongTrolleyState

		public void TestRemovePackageFromTrolleyUsingPackageID_WrongTrolleyState()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(whs);
			var response1 = webService1.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC");
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("This Trolley job is in PIC state. Cannot remove Package from Trolley.", response1.ErrorMessage);

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Finalised;
			trolleyJob.WTJ_FinalisedDateUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			var webService2 = GetNewWebService(whs);
			var response2 = webService2.RemovePackageFromTrolleyUsingPackageID(trolleyJob.PK.ToGuid(), "ABC");
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("This Trolley job is in FIN state. Cannot remove Package from Trolley.", response2.ErrorMessage);
		}

		#endregion
	}
}

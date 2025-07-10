using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CustomValues;
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
using Moq;

namespace Enterprise.Warehouse.Web.Testing
{
	class RemoveAllPackagesFromTrolleyTest : WhsSecureServiceTestCase
	{
		#region TestRemoveAllPackagesFromTrolley

		public void TestRemoveAllPackagesFromTrolley()
		{
			AssertContainsExactElementsInAnyOrder("Precondition: GenAddOnColumn table should be empty.", Enumerable.Empty<string>(), Helper.Factory.Load<GenAddOnColumn>(new ZQuery()).Select(g => $"{g.XA_ParentTableCode}-{g.XA_Name}"));

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
			var packageAAA = packingHelper.CreatePackage(pkgJob, "AAA", 1, Constants.PkgUnit.Box);
			var packageABC = packingHelper.CreatePackage(pkgJob, "ABC", 1, Constants.PkgUnit.Box);
			packageAAA.SetIsTote(true);
			packageABC.SetIsTote(true);

			var releaseLine = (IPackableItemParent)order.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();

			var picklines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(packageABC, picklines[0]);
			packingHelper.CreatePackageDivot(packageABC, picklines[1]);
			Assert("Precondition: The package is packed", packageABC.IsPacked(releaseLine));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageAAA, 1);
			var slot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageABC, 2);

			AssertEquals(2, trolleyJob.Slots.Count);
			AssertEquals("Precondition", 2, Helper.Factory.Load<GenAddOnColumn>(new ZQuery()).Length);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.RemoveAllPackagesFromTrolley(trolleyJob.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			CombineAssertions(() =>
			{
				AssertEquals("The packageAAA is unpacked", false, packageAAA.IsPacked(releaseLine));

				AssertEquals("All packages must be removed.", false, newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK).Slots.Any());

				AssertNull("All packages must be deleted.", newFactory.Load<PkgPackage>(packageAAA.PK));
				AssertNull("All packages must be deleted.", newFactory.Load<PkgPackage>(packageABC.PK));

				AssertEquals("All GenAddOnColumns created for totes should have been deleted", false, newFactory.Load<GenAddOnColumn>(new ZQuery()).Any());
			});
		}

		public void TestRemoveAllPackagesFromTrolley_DockDoorAssignment()
		{
			AssertContainsExactElementsInAnyOrder("Precondition: GenAddOnColumn table should be empty.", Enumerable.Empty<string>(), Helper.Factory.Load<GenAddOnColumn>(new ZQuery()).Select(g => $"{g.XA_ParentTableCode}-{g.XA_Name}"));

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 5m, order1.Lines[0].PickLineQuantity);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_CartoniseSplitCases = true;
			pick2.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 5m, order2.Lines[0].PickLineQuantity);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			packageAAA.SetIsTote(true);

			var pkgJob2 = PkgPackageJob.LoadOrCreatePackageJob(order2);
			var packageABC = packingHelper.CreatePackage(pkgJob2, "ABC", 1, Constants.PkgUnit.Box);
			packageABC.SetIsTote(true);

			var releaseLine1 = (IPackableItemParent)order1.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();
			var releaseLine2 = (IPackableItemParent)order2.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();

			packingHelper.CreatePackageDivot(packageAAA, pick1.GetAllPickLines().Single());
			Assert("Precondition: The packageAAA is packed", packageAAA.IsPacked(releaseLine1));

			packingHelper.CreatePackageDivot(packageABC, pick2.GetAllPickLines().Single());
			Assert("Precondition: The packageABC is packed", packageABC.IsPacked(releaseLine2));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageAAA, 1);
			var slot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageABC, 2);
			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);

			AssertEquals(2, trolleyJob.Slots.Count);
			AssertEquals("Precondition", 2, Helper.Factory.Load<GenAddOnColumn>(new ZQuery()).Length);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.RemoveAllPackagesFromTrolley(trolleyJob.PK.ToGuid());
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
			});

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			CombineAssertions(() =>
			{
				AssertEquals("The packageAAA is unpacked", false, packageAAA.IsPacked(releaseLine1));
				AssertEquals("The packageABC is unpacked", false, packageABC.IsPacked(releaseLine2));

				AssertEquals("All packages must be removed.", false, newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK).Slots.Any());

				AssertNull("All packages must be deleted.", newFactory.Load<PkgPackage>(packageAAA.PK));
				AssertNull("All packages must be deleted.", newFactory.Load<PkgPackage>(packageABC.PK));

				AssertEquals("All GenAddOnColumns created for totes should have been deleted", false, newFactory.Load<GenAddOnColumn>(new ZQuery()).Any());

				var dockDoorAssignments = webService.Factory.Load<WhsDockDoorAssignment>(new ZQuery());
				AssertEquals("No DDA exists", 0, dockDoorAssignments.Length);

				AssertEquals("Pick1 DDA cleared", ZGuid.Empty, pick1.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick1 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick1.WP_WL_DockDoor);

				AssertEquals("Pick2 DDA cleared", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick2 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick2.WP_WL_DockDoor);
			});
		}

		public void TestRemoveAllPackagesFromTrolley_DockDoorAssignment_Error()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.SplitCase);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save(); // to create stock

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_CartoniseSplitCases = true;
			pick1.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);
			AssertEquals("Precondition:", 5m, order1.Lines[0].PickLineQuantity);

			var pkgJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var packageAAA = packingHelper.CreatePackage(pkgJob1, "AAA", 1, Constants.PkgUnit.Box);
			packageAAA.SetIsTote(true);

			var releaseLine1 = (IPackableItemParent)order1.Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines).Single();

			packingHelper.CreatePackageDivot(packageAAA, pick1.GetAllPickLines().Single());
			Assert("Precondition: The packageAAA is packed", packageAAA.IsPacked(releaseLine1));

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var slot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, packageAAA, 1);
			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1);

			AssertEquals(1, trolleyJob.Slots.Count);
			AssertEquals("Precondition", 1, Helper.Factory.Load<GenAddOnColumn>(new ZQuery()).Length);
			Helper.Factory.Save();

			var ddaService = new Mock<IWhsPickDockDoorAssignmentService>();
			ddaService.Setup(m =>
				m.RemovePickDockDoorAssignment(
					It.IsAny<ZGuid>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns("Error");

			using (ObjectFactory.Substitute(ddaService.Object))
			{
				var webService = GetNewWebService(data.Whs1);
				var response = webService.RemoveAllPackagesFromTrolley(trolleyJob.PK.ToGuid());
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error", response.ErrorMessage);
			}
		}

		#endregion

		#region TestRemoveAllPackagesFromTrolley_TrolleyNotExist

		public void TestRemoveAllPackagesFromTrolley_TrolleyNotExist()
		{
			var whs = Helper.CreateWarehouse("WHS");

			var webService = GetNewWebService(whs);
			var response = webService.RemoveAllPackagesFromTrolley(Guid.NewGuid());
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Trolley job was not found. Please start building Trolley again.", response.ErrorMessage);
		}

		#endregion

		#region TestRemoveAllPackagesFromTrolley_WrongTrolleyState

		public void TestRemoveAllPackagesFromTrolley_WrongTrolleyState()
		{
			var whs = Helper.CreateWarehouse("WHS");

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(whs);
			var response1 = webService1.RemoveAllPackagesFromTrolley(trolleyJob.PK.ToGuid());
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("This Trolley job is in PIC state. Cannot remove Package from Trolley.", response1.ErrorMessage);

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Finalised;
			trolleyJob.WTJ_FinalisedDateUtc = ZDateTime.UtcNow;

			Helper.Factory.Save();

			var webService2 = GetNewWebService(whs);
			var response2 = webService2.RemoveAllPackagesFromTrolley(trolleyJob.PK.ToGuid());
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("This Trolley job is in FIN state. Cannot remove Package from Trolley.", response2.ErrorMessage);
		}

		#endregion
	}
}

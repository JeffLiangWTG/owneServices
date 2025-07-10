using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(TrolleyJobInfo))]
	public class TrolleyJobInfoTestCase : WhsPickJobInfoTestCase<TrolleyJobInfo>
	{
		#region TestConstructors

		public void TestConstructors_TrolleyJobStatus()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			var trolleySlot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			var trolleySlot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			var trolleyJobInfo1 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("Slots should be returned for Building.", 2, trolleyJobInfo1.Slots.Count);

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Picking;
			var trolleyJobInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("Slots should be returned for Picking.", 2, trolleyJobInfo2.Slots.Count);

			trolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Finalised;
			var trolleyJobInfo3 = new TrolleyJobInfo(whs, trolleyJob);
			AssertNull("No slots should be returned for Building.", trolleyJobInfo3.Slots);
		}

		#endregion

		#region TestConstructor_ScannedRCASerialNumbers

		public void TestConstructor_ScannedRCASerialNumbers()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 2m);
			Helper.CreatePickNew(order1);
			Helper.CreatePickNew(order2);
			Factory.Save();

			var originalReleaseLine1 = order1.Lines[0].ReleaseLines[0];
			var releaseLine1 = order1.Lines[0].ReleaseLines.AddNew("", "", "", "S1", ZDate.Empty, ZDate.Empty, 1m);
			var releaseLine2 = order1.Lines[0].ReleaseLines.AddNew("", "", "", "S2", ZDate.Empty, ZDate.Empty, 1m);
			originalReleaseLine1.Delete(); // this release line is no longer valid as the Order Line is for 2 units

			var originalReleaseLine2 = order2.Lines[0].ReleaseLines[0];
			var releaseLine3 = order2.Lines[0].ReleaseLines.AddNew("", "", "", "S3", ZDate.Empty, ZDate.Empty, 1m);
			var releaseLine4 = order2.Lines[0].ReleaseLines.AddNew("", "", "", "S4", ZDate.Empty, ZDate.Empty, 1m);
			originalReleaseLine2.Delete(); // this release line is no longer valid as the Order Line is for 2 units

			var package1 = order1.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package1.Pack(releaseLine1, 1);
			var package2 = order2.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			package2.Pack(releaseLine3, 1);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 2);

			Factory.Save();

			var trolleyJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			var scannedRCASerialNumbersForPart1 = trolleyJobInfo.ScannedRCASerialNumbersPerProduct.Single(s => s.ProductPK == data.Part1.PK.ToGuid());
			AssertContainsExactElementsInAnyOrder(new[] { "S1", "S2", "S3", "S4" }, scannedRCASerialNumbersForPart1.ScannedRCASerialNumbers);
		}

		#endregion

		#region TestLines

		public void TestSetPickLines()
		{
			var pickLinesCollection = new WhsPickLineInfoCollection();
			pickLinesCollection.Add(new WhsPickLineInfo());
			pickLinesCollection.Add(new WhsPickLineInfo());

			var trolleyJob = new TrolleyJobInfo();
			AssertEquals("Precondition", 0, trolleyJob.Lines.Count);

			trolleyJob.SetLines(pickLinesCollection);
			AssertEquals(pickLinesCollection, trolleyJob.Lines);
		}

		#endregion

		#region TestPickPKs

		protected override void TestPickPKsCore()
		{
			AssertContainsExactElementsInAnyOrder(Array.Empty<Guid>(), GetNewPickInfo().PickPKs);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var pkgPackageJob1 = order1.PackageJob;
			var pkgPackageJob2 = order2.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob1.Packages);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob2.Packages);
			var pkgPackage3 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG3", pkgPackageJob2.Packages);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			var trolleySlot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 1);
			var trolleySlot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 2);
			var trolleySlot3 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage3, 3);

			Factory.Save();

			var pickInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertContainsExactElementsInAnyOrder("PickPK should be set to the Pick's PK.", new[] { pick1.PK, pick2.PK }, pickInfo2.PickPKs);
		}

		#endregion

		#region TestIsPutawayOnly

		protected override void TestIsPutawayOnlyCore()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals(false, pickInfo1.IsPutawayOnly);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			var pickJobInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals(false, pickJobInfo2.IsPutawayOnly);
		}

		#endregion

		#region TestIsUsingDirectedPackingConsolidation

		public void TestIsUsingDirectedPackingConsolidation_EmptyTrolley()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			var trolleyJobInfo = new TrolleyJobInfo(whs, trolleyJob);

			AssertEquals("Should *not* be using directed packing consolidation.", false, trolleyJobInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_TotePicking_UsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_TrolleyJobCore(TrolleyPickingType.Tote, true);
		}

		public void TestIsUsingDirectedPackingConsolidation_CartonPicking_UsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_TrolleyJobCore(TrolleyPickingType.Carton, true);
		}

		public void TestIsUsingDirectedPackingConsolidation_TotePicking_NotUsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_TrolleyJobCore(TrolleyPickingType.Tote, false);
		}

		public void TestIsUsingDirectedPackingConsolidation_CartonPicking_NotUsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_TrolleyJobCore(TrolleyPickingType.Carton, false);
		}

		void TestIsUsingDirectedPackingConsolidation_TrolleyJobCore(TrolleyPickingType trolleyType, bool isUsingDirectedPackingConsolidation)
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = isUsingDirectedPackingConsolidation;

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition", 10m, pickLine.WZ_Units);
			pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pkg = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);

			var isTotePicking = trolleyType == TrolleyPickingType.Tote;
			pkg.SetIsTote(isTotePicking);
			pkg.Pack(orderLine.ReleaseLines[0], 10m);

			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, pkg.PK, 5);

			Factory.Save();

			var orderPickInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Should only be using directed packing consolidation if order on pick is and is not tote picking.",
				isUsingDirectedPackingConsolidation && !isTotePicking,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_MultiOrderTotePicking_UsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_MultiOrder_TrolleyJobCore(TrolleyPickingType.Tote, true);
		}

		public void TestIsUsingDirectedPackingConsolidation_MultiOrderCartonPicking_UsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_MultiOrder_TrolleyJobCore(TrolleyPickingType.Carton, true);
		}

		public void TestIsUsingDirectedPackingConsolidation_MultiOrderTotePicking_NotUsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_MultiOrder_TrolleyJobCore(TrolleyPickingType.Tote, false);
		}

		public void TestIsUsingDirectedPackingConsolidation_MultiOrderCartonPicking_NotUsingPackingConsolidation()
		{
			TestIsUsingDirectedPackingConsolidation_MultiOrder_TrolleyJobCore(TrolleyPickingType.Carton, false);
		}

		void TestIsUsingDirectedPackingConsolidation_MultiOrder_TrolleyJobCore(TrolleyPickingType trolleyType, bool isUsingDirectedPackingConsolidation)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var isTotePicking = trolleyType == TrolleyPickingType.Tote;
			var (pkg1, pickLine1) = CreateOrderWithPackageAndPickLine("O1", "P1", false);
			var (pkg2, pickLine2) = CreateOrderWithPackageAndPickLine("O2", "P2", false);
			var (pkg3, pickLine3) = CreateOrderWithPackageAndPickLine("O3", "P3", isUsingDirectedPackingConsolidation);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, pkg1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, pkg2.PK, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, pkg3.PK, 3);

			Factory.Save();

			var orderPickInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Should only be using directed packing consolidation if some order on pick is and is not tote picking.",
				isUsingDirectedPackingConsolidation && !isTotePicking,
				orderPickInfo.IsUsingDirectedPackingConsolidation);

			(PkgPackage, WhsPickLine) CreateOrderWithPackageAndPickLine(string orderId, string packageID, bool useDirectedPackingConsolidation)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, orderId);
				order.WD_UseDirectedPackingConsolidation = useDirectedPackingConsolidation;

				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				var pick = Helper.CreatePickNew(order);
				var pickLine = pick.GetAllPickLines().Single();
				AssertEquals("Precondition", 1m, pickLine.WZ_Units);
				pick.GetAllPickLines().ForEach(l => l.WZ_F3_NKAllocatedPackType = Constants.PkgUnit.Unit);

				var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
				var pkg = packingHelper.CreatePackage(pkgJob, packageID, 1, Constants.PkgUnit.Box);
				pkg.SetIsTote(isTotePicking);
				pkg.Pack(orderLine.ReleaseLines[0], 1m);

				return (pkg, pickLine);
			}
		}

		protected override void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderline1.ReleaseLines[0], 5m);

			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 5);

			Factory.Save();

			var orderPickInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Should not use directed packing consolidation if order on pick has loose inventory.",
				false,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		protected override void TestIsUsingDirectedPackingConsolidation_PickWithLooseInventoryAlreadyPutawayCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderline1.ReleaseLines[0], 5m);

			Factory.Save();

			var pickLine = orderline2.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();

			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 5);

			Factory.Save();

			var orderPickInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Should not use directed packing consolidation if order on pick has loose inventory.",
				false,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		public void TestIsUsingDirectedPackingConsolidation_WithSomePartsAlreadyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			order.WD_UseDirectedPackingConsolidation = true;

			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			package1.Pack(orderline1.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.Pack(orderline2.ReleaseLines[0], 5m);
			Factory.Save();

			var pickLine = orderline2.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Picking);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 5);

			Factory.Save();

			var orderPickInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Should not use directed packing consolidation if order on pick has loose inventory.",
				false,
				orderPickInfo.IsUsingDirectedPackingConsolidation);
		}

		#endregion

		#region TestDockDoorLocation

		protected override void TestDockDoorLocationCore()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("DockDoorLocation", string.Empty, pickInfo1.DockDoorLocation);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			var trolleySlot1 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			var trolleySlot2 = Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Factory.Save();

			pick.WP_WL_DockDoor = ZGuid.Empty;

			var pickInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("DockDoorLocation", string.Empty, pickInfo2.DockDoorLocation);

			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			AssertEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo3 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("DockDoorLocation", pick.DockDoorLocation.WLV_LocationString, pickJobInfo3.DockDoorLocation);

			pick.WP_WL_DockDoor = data.Whs1.DefaultLocation.PK;
			AssertNotEquals("Precondition", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var pickJobInfo4 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("DockDoorLocation", pick.DockDoorLocation.WLV_LocationString, pickJobInfo4.DockDoorLocation);
		}

		protected override void TestDockDoorLocation_FixedWidthLocationCore()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("DockDoorLocation", string.Empty, pickInfo1.DockDoorLocation);

			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("ZZ", 2, 2, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "Z", 4, 3, 2);
			Factory.Save();

			var dockdoorLocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, "DDL"));
			var location1 = warehouse.FindLocation("Z030201");
			location1.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location1.WLV_LocationStatus = "NOR";
			var location2 = warehouse.FindLocation("Z040302");
			location2.WLV_WLT_LocationType = dockdoorLocationType.PK;
			location2.WLV_LocationStatus = "NOR";
			Factory.Save();

			warehouse.WW_DefaultInboundDockDoor = location1.PK;
			warehouse.WW_DefaultOutboundDockDoor = location1.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Factory.Save();

			pick.WP_WL_DockDoor = ZGuid.Empty;

			var pickInfo2 = new TrolleyJobInfo(warehouse, trolleyJob);
			AssertEquals("DockDoorLocation", string.Empty, pickInfo2.DockDoorLocation);

			pick.WP_WL_DockDoor = warehouse.WW_DefaultOutboundDockDoor;
			AssertEquals("Precondition", location1.PK, pick.WP_WL_DockDoor);

			var pickJobInfo3 = new TrolleyJobInfo(warehouse, trolleyJob);
			AssertEquals("DockDoorLocation", "Z030201", pickJobInfo3.DockDoorLocation);
			AssertEquals("DockDoorLocation_UserFriendly", "Z-03-02-01", pickJobInfo3.DockDoorLocation_UserFriendly);

			pick.WP_WL_DockDoor = location2.PK;
			AssertNotEquals("Precondition", location1.PK, pick.WP_WL_DockDoor);

			var pickJobInfo4 = new TrolleyJobInfo(warehouse, trolleyJob);
			AssertEquals("DockDoorLocation", "Z040302", pickJobInfo4.DockDoorLocation);
			AssertEquals("DockDoorLocation_UserFriendly", "Z-04-03-02", pickJobInfo4.DockDoorLocation_UserFriendly);
		}

		#endregion

		#region TestIsPackingStationAllowed

		protected override void TestIsPackingStationAllowedCore(bool hasPackingStationLocation)
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("HasPackingStation", false, pickInfo1.IsPackingStationAllowed);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			pkgPackage1.SetIsTote(true);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);
			pkgPackage2.SetIsTote(true);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);
			Factory.Save();

			if (hasPackingStationLocation)
			{
				var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
				var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
				var packingLocation = newRow.Locations[0];
				packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
				Factory.Save();
			}

			var pickInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("PickingType is tote.", TrolleyPickingType.Tote, trolleyJob.PickingType);
			AssertEquals("HasPackingStation", hasPackingStationLocation, pickInfo2.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PackingStationIsInvalidLocationStatusCore(string locationStatus)
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("HasPackingStation", false, pickInfo1.IsPackingStationAllowed);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			pkgPackage1.SetIsTote(true);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);
			pkgPackage2.SetIsTote(true);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);
			Factory.Save();

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			packingLocation.WLV_LocationStatus = locationStatus;
			Factory.Save();

			var pickInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("PickingType is tote.", TrolleyPickingType.Tote, trolleyJob.PickingType);
			AssertEquals("HasPackingStation", false, pickInfo2.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PickWithAssignedPackingStationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			pick.WP_WL_PackingStation = packingLocation.PK;

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			pkgPackage1.SetIsTote(true);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);
			pkgPackage2.SetIsTote(true);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Factory.Save();

			var pickInfo = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("PickingType is tote.", TrolleyPickingType.Tote, trolleyJob.PickingType);
			AssertEquals("HasPackingStation", true, pickInfo.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_PickByBOMCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			var whs = data.Whs1;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(whs, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part2, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			pkgPackage1.SetIsTote(true);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);
			pkgPackage2.SetIsTote(true);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);
			Factory.Save();

			var pickInfo = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("PickingType is tote.", TrolleyPickingType.Tote, trolleyJob.PickingType);
			AssertEquals("Pick by BOM lines can be sent to a Packing Station.", true, pickInfo.IsPackingStationAllowed);
		}

		protected override void TestIsPackingStationAllowed_DBHitsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var count = 10;
			var mainProducts = new List<OrgSupplierPart>();
			var subProducts = new List<OrgSupplierPart>();
			var pkgPackages = new List<PkgPackage>();

			for (var i = 0; i < count; i++)
			{
				var mainProduct = Helper.CreateProduct(data.Org1, $"P1{i}_main");
				var subProduct = Helper.CreateProduct(data.Org1, $"P1{i}_sub");
				Helper.CreateProductBOM(mainProduct, subProduct);
				mainProduct.OP_IsComponentPickedOnSalesOrder = true;

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}_main", mainProduct, 10m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}_sub", subProduct, 10m);

				mainProducts.Add(mainProduct);
				subProducts.Add(subProduct);
			}
			Factory.Save();

			for (var i = 0; i < count; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, $"O{i}", mainProducts[i], 20m);
				Helper.CreatePickNew(order);

				var pkgPackageJob = order.PackageJob;
				var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, $"PKG1{i}", pkgPackageJob.Packages);
				pkgPackage1.SetIsTote(true);
				pkgPackages.Add(pkgPackage1);
				var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, $"PKG2{i}", pkgPackageJob.Packages);
				pkgPackage2.SetIsTote(true);
				pkgPackages.Add(pkgPackage2);
			}

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			for (var i = 0; i < pkgPackages.Count; i++)
			{
				Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackages[i], (short)(i + 1));
			}
			AssertEquals("Precondition", 20, trolleyJob.Slots.Count);
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
				{
					{ GenAddOnColumnSchema.Constants.TableName, 1 },
					{ PkgPackageHeaderSchema.Constants.TableName, 2 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ PkgPackageSchema.Constants.TableName, 2 }
				};

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var whsInNewFactory = newFactory.Load<WhsWarehouse>(data.Whs1.PK);
			var trolleyJobInNewFactory = newFactory.Load<WhsPickTrolleyJob>(trolleyJob.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var pickInfo = new TrolleyJobInfo(whsInNewFactory, trolleyJobInNewFactory);
				AssertEquals("PickingType is tote.", TrolleyPickingType.Tote, trolleyJob.PickingType);
				AssertEquals("Pick by BOM lines can be sent to a Packing Station.", true, pickInfo.IsPackingStationAllowed);
			}
		}

		public void TestIsPackingStationAllowed_CartonPicking()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("HasPackingStation", false, pickInfo1.IsPackingStationAllowed);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var pickInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("PickingType is Carton.", TrolleyPickingType.Carton, trolleyJob.PickingType);
			AssertEquals("HasPackingStation", false, pickInfo2.IsPackingStationAllowed);
		}

		public void TestIsPackingStationAllowed_CartonPicking_SomePartOfOrderInPackingStation()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			pick.WP_WL_PackingStation = packingLocation.PK;
			Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var pickByLabelPackage = packingHelper.CreatePackage(pkgJob, "PKG1", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pickByLabelPackage, pickLine1);
			var pkg_OnTrolley = packingHelper.CreatePackage(pkgJob, "PKG2", 1, PkgUnit.Box);
			packingHelper.CreatePackageDivot(pkg_OnTrolley, pickLine2);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkg_OnTrolley, 1);
			Factory.Save();

			var trolleyJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("PickingType is Carton.", TrolleyPickingType.Carton, trolleyJob.PickingType);
			AssertEquals("PackingStation is allowed", true, trolleyJobInfo.IsPackingStationAllowed);
			AssertEquals("Putaway Station Location String is Correct", packingLocation.WLV_LocationString, trolleyJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", packingLocation.WLV_LocationString_UserFriendly, trolleyJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", packingLocation.WLV_LocationClass, trolleyJobInfo.AssignedPutawayLocationClass);
		}

		public void TestIsPackingStationAllowed_NotToteNorCartonPicking()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("HasPackingStation", false, pickInfo1.IsPackingStationAllowed);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var pickInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("PickingType is None.", TrolleyPickingType.None, trolleyJob.PickingType);
			AssertEquals("HasPackingStation", false, pickInfo2.IsPackingStationAllowed);
		}

		#endregion

		#region TestPSTLocationPK

		protected override void TestPackingStationPKCore()
		{
			var pickInfo1 = GetNewPickInfo();
			AssertEquals("No Packing Station for Default", Guid.Empty, pickInfo1.PackingStationPK);

			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			pick.WP_WL_PackingStation = packingLocation.PK;

			var pkgPackageJob = order.PackageJob;
			var pkgPackage1 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG1", pkgPackageJob.Packages);
			pkgPackage1.SetIsTote(true);
			var pkgPackage2 = Helper.CreatePackage(Constants.PkgUnit.Box, "PKG2", pkgPackageJob.Packages);
			pkgPackage2.SetIsTote(true);

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, pkgPackage2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Factory.Save();

			var pickInfo2 = new TrolleyJobInfo(whs, trolleyJob);
			AssertEquals("PickingType is tote.", TrolleyPickingType.Tote, trolleyJob.PickingType);
			AssertEquals("Packing Station PK is Correct", packingLocation.PK.ToGuid(), pickInfo2.PackingStationPK);
		}

		#endregion

		#region TestAssignedPutawayLocationCore

		protected override void TestAssignedPutawayLocation_PackingStationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var packingHelper = new PackingTestHelper(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			order.WD_UseDirectedPackingConsolidation = true;
			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_PackingStation = packingLocation.PK;

			var pickLine1 = order.Lines[0].PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);
			Factory.Save();

			var pickJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Putaway Station Location String is Correct", packingLocation.WLV_LocationString, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", packingLocation.WLV_LocationString_UserFriendly, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", packingLocation.WLV_LocationClass, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_DockDoorCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var dockdoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

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
			transferLine1.WE_WL = dockdoorLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);
			Factory.Save();

			var pickJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Putaway Station Location String is Correct", dockdoorLocation.WLV_LocationString, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", dockdoorLocation.WLV_LocationString_UserFriendly, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", dockdoorLocation.WLV_LocationClass, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_ConsolidationLocationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			var packingHelper = new PackingTestHelper(Factory);

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
			transferLine1.WE_WL = consolidationLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Factory.Save();

			var pickJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		protected override void TestAssignedPutawayLocation_NothingPutawayYetCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

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

			var pickLine2 = order.Lines[1].PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var package1 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package1, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package2, 5);
			AssertEquals("Precondition", 2, trolleyJob.Slots.Count);

			Factory.Save();

			var pickJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PickByTote()
		{
			TestAssignedPutawayLocationClass_NothingPutawayYetCore(isPickByTote: true);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_PickByCarton()
		{
			TestAssignedPutawayLocationClass_NothingPutawayYetCore(isPickByTote: false);
		}

		void TestAssignedPutawayLocationClass_NothingPutawayYetCore(bool isPickByTote)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package.SetIsTote(isPickByTote);
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 2);
			Factory.Save();

			var pickJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", isPickByTote ? LocationClasses.Codes.PST : string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		public void TestAssignedPutawayLocationClass_NothingPutawayYet_NoPackingStations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			package.SetIsTote(true);
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T001");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.CreateWhsPickTrolleySlot(trolleyJob, package, 2);
			Factory.Save();

			var pickJobInfo = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("Putaway Station Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation);
			AssertEquals("Putaway Station User Friendly Location String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocation_UserFriendly);
			AssertEquals("Putaway Station Location Class String is Correct", string.Empty, pickJobInfo.AssignedPutawayLocationClass);
		}

		#endregion

		#region TestAllowPickDockDoorLocationOverride

		protected override void TestAllowPickDockDoorLocationOverrideCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			var pickInfo1 = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("AllowPickDockDoorLocationOverride should be false", false, pickInfo1.AllowPickDockDoorLocationOverride);

			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickInfo2 = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("AllowPickDockDoorLocationOverride should be true", true, pickInfo2.AllowPickDockDoorLocationOverride);
		}

		protected override void TestAllowPickDockDoorLocationOverride_LinkedPicksCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var packingHelper = new PackingTestHelper(Factory);

			var pickPackParam1 = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);

			var client2 = Helper.CreateClient("CL2");
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			var pickPackParam2 = Helper.CreatePickPackParameter(client2, data.Whs1);
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, data.Part2, 20m);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 20m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 20m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			var pickJobInfo1 = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", false, pickJobInfo1.AllowPickDockDoorLocationOverride);

			// Enable 1 param
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickJobInfo2 = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", false, pickJobInfo2.AllowPickDockDoorLocationOverride);

			// Enable both params
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			var pickJobInfo3 = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("AllowPickDockDoorLocationOverride pick1", true, pickJobInfo3.AllowPickDockDoorLocationOverride);

			dda.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
			Factory.Save();

			var pickJobInfo4 = new TrolleyJobInfo(data.Whs1, trolleyJob);
			AssertEquals("AllowPickDockDoorLocationOverride false if DDA WDA_FirstPutawayToDockDoorUtc set.", false, pickJobInfo4.AllowPickDockDoorLocationOverride);
		}

		#endregion

		#region Implementation

		protected override TrolleyJobInfo GetNewPickInfo()
		{
			return new TrolleyJobInfo();
		}

		#endregion
	}
}

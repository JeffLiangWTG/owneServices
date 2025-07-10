using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickDockDoorAssignmentServiceTest : WhsTestCaseWithFactory
	{
		#region TestService_GeneratePickDockDoorAssignment

		public void TestService_GeneratePickDockDoorAssignment_PKNotPackage()
		{
			var pkgPK = ZGuid.NewZGuid();
			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(pkgPK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			AssertEquals("Correct message reported.", $"Package could NOT be loaded using '{pkgPK}'.", errorMessage);
		}

		public void TestService_GeneratePickDockDoorAssignment_Empty()
		{
			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(ZGuid.Empty, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			AssertEquals("Correct message reported.", $"Package could NOT be loaded using '{ZGuid.Empty}'.", errorMessage);
		}

		public void TestService_GeneratePickDockDoorAssignment_NoPick()
		{
			var package = Factory.NewWithValidTestData<PkgPackage>();
			Factory.Save();

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			AssertEquals("Correct message reported.", $"Pick linked to Package from '{package.PK}' could not be found.", errorMessage);
		}

		public void TestService_GeneratePickDockDoorAssignment_PackageAlreadyAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package1.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			AssertEquals("No exceptions were thrown.", string.Empty, errorMessage);
			AssertEquals("Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);
		}

		public void TestService_GeneratePickDockDoorAssignment_WrongLinkTypeProvided_PBL()
		{
			TestService_GeneratePickDockDoorAssignment_WrongLinkTypeProvidedCore(DockDoorAssignmentLinkType.PickByLabel);
		}

		public void TestService_GeneratePickDockDoorAssignment_WrongLinkTypeProvided_HU()
		{
			TestService_GeneratePickDockDoorAssignment_WrongLinkTypeProvidedCore(DockDoorAssignmentLinkType.HandlingUnit);
		}

		void TestService_GeneratePickDockDoorAssignment_WrongLinkTypeProvidedCore(DockDoorAssignmentLinkType wrongLinkType)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick1.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick1 DockDoorAssignment", ZGuid.Empty, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick2.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick2 DockDoorAssignment", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package1.PK, wrongLinkType, ZGuid.Empty, Factory);

			AssertEquals("No exceptions were thrown.", string.Empty, errorMessage);
			AssertEquals("Pick1 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick1.WP_WL_DockDoor);
			AssertEquals("Pick1 DockDoorAssignment", ZGuid.Empty, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick2.WP_WL_DockDoor);
			AssertEquals("Pick2 DockDoorAssignment", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);
		}

		public void TestService_GeneratePickDockDoorAssignment_WrongLinkTypeProvided_Trolley()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			var userCode = GlbStaff.CurrentUser.GS_Code;
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package1.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package2.PK);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick1.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick1 DockDoorAssignment", ZGuid.Empty, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick2.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick2 DockDoorAssignment", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package1.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			AssertEquals("No exceptions were thrown.", string.Empty, errorMessage);
			AssertEquals("Pick1 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick1.WP_WL_DockDoor);
			AssertEquals("Pick1 DockDoorAssignment", ZGuid.Empty, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick2.WP_WL_DockDoor);
			AssertEquals("Pick2 DockDoorAssignment", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);
		}

		public void TestService_GeneratePickDockDoorAssignment_NoLinkFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "P01";
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			AssertEquals("Precondition: Pick DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick DockDoorAssignment", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			AssertEquals("No exceptions were thrown.", string.Empty, errorMessage);
			AssertEquals("Pick DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick.WP_WL_DockDoor);
			AssertEquals("Pick DDA", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
		}

		public void TestService_GeneratePickDockDoorAssignment_PackageNotOnLink()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "P01";
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();

			AssertEquals("Precondition: Pick DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick DDA", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			AssertEquals("No errors.", string.Empty, errorMessage);
			AssertEquals("Precondition: Pick DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick DDA", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
		}

		public void TestService_GeneratePickDockDoorAssignment_Trolley()
		{
			TestService_GeneratePickDockDoorAssignmentCore(
				(_, package1, package2, package3) =>
				{
					var trolley1 = Helper.CreateTrolley("T1");
					var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");
					Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);
					Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package2.PK, 2);

					var trolley2 = Helper.CreateTrolley("T2");
					var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "PIC");
					Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package3.PK, 1);

					return trolleyJob1.PK;
				},
				DockDoorAssignmentLinkType.Trolley);
		}

		public void TestService_GeneratePickDockDoorAssignment_PBL()
		{
			TestService_GeneratePickDockDoorAssignmentCore(
				(whs, package1, package2, package3) =>
				{
					var userCode = GlbStaff.CurrentUser.GS_Code;
					var otherUser = Helper.CreateGlbStaff("OTH", "OtherUser");
					var pblJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, whs.PK,
						whs.WW_DefaultOutboundDockDoor, userCode, package1.PK);
					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, whs.PK,
						whs.WW_DefaultOutboundDockDoor, userCode, package2.PK);

					WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, whs.PK,
						whs.WW_DefaultOutboundDockDoor, otherUser.GS_Code, package3.PK);

					return pblJob.PK;
				},
				DockDoorAssignmentLinkType.PickByLabel);
		}

		void TestService_GeneratePickDockDoorAssignmentCore(
			Func<WhsWarehouse, PkgPackage, PkgPackage, PkgPackage, ZGuid> linkPkgs1And2AddPkg3ToItsOwn,
			DockDoorAssignmentLinkType linkType)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();

			var parentJobPK = linkPkgs1And2AddPkg3ToItsOwn(data.Whs1, package1, package2, package3);

			AssertEquals("Precondition: Pick1 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick1.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick2 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick2.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick3 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick3.WP_WL_DockDoor);

			var errorMessage = string.Empty;
			errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package1.PK, linkType, parentJobPK, Factory);
			var checkFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var ddasInDB = checkFactory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Should not be in DB yet", 0, ddasInDB.Length);
			Factory.Save();

			AssertEquals("No exceptions were thrown.", string.Empty, errorMessage);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dockDoorAssignments = newFactory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA created", 1, dockDoorAssignments.Length);

			var dda = dockDoorAssignments[0];
			AssertEquals("DDA correct DDL", dda.WDA_WL_AssignedDockDoor, data.Whs1.WW_DefaultOutboundDockDoor);
			AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

			var pick1InNewFactory = newFactory.Load<WhsPick>(pick1.PK);
			AssertEquals("Pick1 DDA set", dda.PK, pick1InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1InNewFactory.WP_WL_DockDoor);

			var pick2InNewFactory = newFactory.Load<WhsPick>(pick2.PK);
			AssertEquals("Pick2 DDA set", dda.PK, pick2InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL cleared", ZGuid.Empty, pick2InNewFactory.WP_WL_DockDoor);

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package2.PK, linkType, ZGuid.Empty, Factory);
			AssertEquals("Still only 1 DDA created", 1, new BusinessObjectFactory().Load<WhsDockDoorAssignment>(new ZQuery()).Length);

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package3.PK, linkType, ZGuid.Empty, Factory);
			AssertEquals("Still only 1 DDA created", 1, new BusinessObjectFactory().Load<WhsDockDoorAssignment>(new ZQuery()).Length);

			// Pick 3 has no links so it should not need a DockDoorAssignment
			var pick3InNewFactory = newFactory.Load<WhsPick>(pick3.PK);
			AssertEquals("Pick3 DDA empty", ZGuid.Empty, pick3InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL cleared", data.Whs1.WW_DefaultOutboundDockDoor, pick3InNewFactory.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_MultipleLinkersSinglePick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(orderLine1.ReleaseLines[0], 5m);

			var package2 = order.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(orderLine2.ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Factory.Save();

			AssertEquals("Precondition: Pick DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick DDA", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);

			var errorMessage1 = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package1.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			AssertEquals("Should get NO error", string.Empty, errorMessage1);
			Factory.Save();

			var userCode = GlbStaff.CurrentUser.GS_Code;
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package2.PK);

			var errorMessage2 = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package2.PK, DockDoorAssignmentLinkType.PickByLabel, ZGuid.Empty, Factory);
			AssertEquals("Should get NO error", string.Empty, errorMessage2);

			var dockDoorAssignmentsInNewFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("No DDA exists", 0, dockDoorAssignmentsInNewFactory.Length);

			AssertEquals("Pick DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
			AssertEquals("Pick DDA", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
		}

		#endregion

		#region TestService_GeneratePickDockDoorAssignment_ExistingAssignment

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Add()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package3.PK, 3);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick3.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick3 DDA", ZGuid.Empty, pick3.WP_WDA_DockDoorAssignment);
			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package3.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			var dockDoorAssignmentsInNewFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA still exists no new ones", 1, dockDoorAssignmentsInNewFactory.Length);
			var ddaInDB = dockDoorAssignmentsInNewFactory[0];
			AssertEquals("It is the existing one", dda.PK, ddaInDB.PK);

			AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

			AssertEquals("Pick1 DDA still set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA set", dda.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL cleared", ZGuid.Empty, pick3.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Add_AlreadyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3, order4);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);

			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order4.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package3.PK, 2);
			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick3);
			Factory.Save();

			dda.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 3);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDA", dda.PK, pick3.WP_WDA_DockDoorAssignment);

			AssertEquals("Precondition: Pick2 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick2.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick2 DDA", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package3.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			AssertEquals("Should get NO error", string.Empty, errorMessage);

			var dockDoorAssignmentsInNewFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA still exists no new ones", 1, dockDoorAssignmentsInNewFactory.Length);
			var ddaInDB = dockDoorAssignmentsInNewFactory[0];
			AssertEquals("It is the existing one", dda.PK, ddaInDB.PK);

			AssertEquals("DDA has PutawayTime", false, dda.WDA_FirstPutawayToDockDoorUtc.IsEmpty);

			AssertEquals("Pick1 DDA still set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA still set", dda.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL still cleared", ZGuid.Empty, pick3.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Merge_Trolley()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 5m);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			var pick4 = Helper.CreatePickNew(order4);
			var pick5 = Helper.CreatePickNew(order5, order6);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);
			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order4.Lines[0].ReleaseLines[0], 5m);

			var trolley1 = Helper.CreateTrolley("T1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package2.PK, 2);
			var dda1 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			dda1.WDA_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			Factory.Save();

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package3.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package4.PK, 2);
			var dda2 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick3, pick4);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDA", dda2.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick4 DDA", dda2.PK, pick4.WP_WDA_DockDoorAssignment);

			var package5 = order5.PackageJob.Packages.AddNew();
			package5.KP_PackageID = "P05";
			package5.Pack(order5.Lines[0].ReleaseLines[0], 5m);
			var package6 = order6.PackageJob.Packages.AddNew();
			package6.KP_PackageID = "P06";
			package6.Pack(order6.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package5.PK, 3);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package6.PK, 3);
			Factory.Save();

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package5.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package6.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			Factory.Save();

			var dockDoorAssignmentsInNewFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA still exists 1 was deleted", 1, dockDoorAssignmentsInNewFactory.Length);
			AssertEquals("Earliest created DDA still exists", dda1.PK, dockDoorAssignmentsInNewFactory[0].PK);

			AssertEquals("Pick1 DDA still set", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA still set", dda1.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL still cleared", ZGuid.Empty, pick3.WP_WL_DockDoor);

			AssertEquals("Pick4 DDA still set", dda1.PK, pick4.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick4 DDL still cleared", ZGuid.Empty, pick4.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Add_NotMatchingPlannedLoadDDLs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var dockDoorType = Helper.CreateLocationType("DDL", LocationClasses.Codes.DDL);
			var otherDDL = Helper.CreateRowAndGenerateLocations(data.Whs1, "ODD", 1, 1).Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorType.PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order1.WD_WLO_PlannedLoad = load1.PK;
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order2.WD_WLO_PlannedLoad = load2.PK;
			var pick2 = Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var load3 = Helper.CreateWhsLoad(data.Org1, otherDDL);
			order3.WD_WLO_PlannedLoad = load3.PK;
			var pick3 = Helper.CreatePickNew(order3);
			pick3.WP_WL_DockDoor = otherDDL.PK;

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package3.PK, 3);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			AssertEquals("Precondition: Pick3 DDL", otherDDL.PK, pick3.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick3 DDA", ZGuid.Empty, pick3.WP_WDA_DockDoorAssignment);

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package3.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			AssertEquals("Cannot add this package to this Job as it has a different Dock Door Location.", errorMessage);

			var dockDoorAssignmentsInNewFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("1 DDA exists", 1, dockDoorAssignmentsInNewFactory.Length);

			var ddaInDB = dockDoorAssignmentsInNewFactory.Single(d => d.PK == dda.PK);
			AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

			AssertEquals("Pick1 DDA still set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA set cleared", ZGuid.Empty, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL still set to otherDDL", otherDDL.PK, pick3.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Merge_PBL()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 5m);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			var pick4 = Helper.CreatePickNew(order4);
			var pick5 = Helper.CreatePickNew(order5, order6);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);
			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order4.Lines[0].ReleaseLines[0], 5m);

			var userCode = GlbStaff.CurrentUser.GS_Code;
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package3.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package4.PK);
			var dda1 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick3, pick4);
			dda1.WDA_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			Factory.Save();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			var dda2 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda2.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda2.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDA", dda1.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick4 DDA", dda1.PK, pick4.WP_WDA_DockDoorAssignment);

			var package5 = order5.PackageJob.Packages.AddNew();
			package5.KP_PackageID = "P05";
			package5.Pack(order5.Lines[0].ReleaseLines[0], 5m);
			var package6 = order6.PackageJob.Packages.AddNew();
			package6.KP_PackageID = "P06";
			package6.Pack(order6.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package5.PK, 3);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package6.PK);
			Factory.Save();

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package5.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package6.PK, DockDoorAssignmentLinkType.PickByLabel, ZGuid.Empty, Factory);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA still exists 1 was deleted", 1, dockDoorAssignments.Length);
			AssertEquals("Earliest created DDA still exists", dda1.PK, dockDoorAssignments[0].PK);

			AssertEquals("Pick1 DDA still set", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA still set", dda1.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL still cleared", ZGuid.Empty, pick3.WP_WL_DockDoor);

			AssertEquals("Pick4 DDA still set", dda1.PK, pick4.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick4 DDL still cleared", ZGuid.Empty, pick4.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Merge_HU()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 5m);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			var pick4 = Helper.CreatePickNew(order4);
			var pick5 = Helper.CreatePickNew(order5, order6);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);
			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order4.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			var dda1 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			dda1.WDA_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			Factory.Save();

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package3, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package4, handlingUnitPackage);
			var dda2 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick3, pick4);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDA", dda2.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick4 DDA", dda2.PK, pick4.WP_WDA_DockDoorAssignment);

			var package5 = order5.PackageJob.Packages.AddNew();
			package5.KP_PackageID = "P05";
			package5.Pack(order5.Lines[0].ReleaseLines[0], 5m);
			var package6 = order6.PackageJob.Packages.AddNew();
			package6.KP_PackageID = "P06";
			package6.Pack(order6.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package5.PK, 3);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package6, handlingUnitPackage);
			Factory.Save();

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package5.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package6.PK, DockDoorAssignmentLinkType.HandlingUnit, ZGuid.Empty, Factory);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA still exists 1 was deleted", 1, dockDoorAssignments.Length);
			AssertEquals("Earliest created DDA still exists", dda1.PK, dockDoorAssignments[0].PK);

			AssertEquals("Pick1 DDA still set", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA still set", dda1.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL still cleared", ZGuid.Empty, pick3.WP_WL_DockDoor);

			AssertEquals("Pick4 DDA still set", dda1.PK, pick4.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick4 DDL still cleared", ZGuid.Empty, pick4.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Merge_DifferentDDLs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var dockDoorType = Helper.CreateLocationType("DDL", LocationClasses.Codes.DDL);
			var otherDDL = Helper.CreateRowAndGenerateLocations(data.Whs1, "ODD", 1, 1).Locations[0];
			otherDDL.WLV_WLT_LocationType = dockDoorType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			pick3.WP_WL_DockDoor = otherDDL.PK;
			var pick4 = Helper.CreatePickNew(order4, order5);
			pick4.WP_WL_DockDoor = otherDDL.PK;

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);
			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order4.Lines[0].ReleaseLines[0], 5m);

			var trolley1 = Helper.CreateTrolley("T1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package2.PK, 2);
			var dda1 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			dda1.WDA_SystemCreateTimeUtc = DateTime.UtcNow.AddMinutes(-5);
			Factory.Save();

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package3.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package4.PK, 2);
			var dda2 = Helper.CreateWhsDockDoorAssignment(otherDDL, pick3, pick4);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDA", dda2.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick4 DDA", dda2.PK, pick4.WP_WDA_DockDoorAssignment);

			var package5 = order5.PackageJob.Packages.AddNew();
			package5.KP_PackageID = "P05";
			package5.Pack(order5.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package5.PK, 3);
			Factory.Save();

			var errorMessage = new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package5.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			AssertEquals("Cannot add this package to this Job as it has a different Dock Door Location.", errorMessage);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("Only 2 DDAs still exist", [dda1.PK, dda2.PK], dockDoorAssignments.Select(f => f.PK));

			AssertEquals("Pick1 DDA still set", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA still set", dda2.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL still cleared", ZGuid.Empty, pick3.WP_WL_DockDoor);

			AssertEquals("Pick4 DDA still set", dda2.PK, pick4.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick4 DDL still cleared", ZGuid.Empty, pick4.WP_WL_DockDoor);
		}

		public void TestService_GeneratePickDockDoorAssignment_ExistingAssignment_Merge_DockDoorAssignmentPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			var pick4 = Helper.CreatePickNew(order4, order5);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);
			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order4.Lines[0].ReleaseLines[0], 5m);

			var trolley1 = Helper.CreateTrolley("T1");
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package2.PK, 2);
			var dda1 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			dda1.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
			Factory.Save();

			var trolley2 = Helper.CreateTrolley("T2");
			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package3.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package4.PK, 2);
			var dda2 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick3, pick4);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDA", dda2.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick4 DDA", dda2.PK, pick4.WP_WDA_DockDoorAssignment);

			var package5 = order5.PackageJob.Packages.AddNew();
			package5.KP_PackageID = "P05";
			package5.Pack(order5.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package5.PK, 3);
			Factory.Save();

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package5.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA still exists", 1, dockDoorAssignments.Length);
			AssertContainsExactElementsInAnyOrder("2 DDAs still exist", [dda1.PK], dockDoorAssignments.Select(f => f.PK));

			AssertEquals("Pick1 DDA still set", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDA still set", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDA still set", dda1.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick4 DDA still set", dda1.PK, pick4.WP_WDA_DockDoorAssignment);
		}

		#endregion

		#region TestService_AddPackageToHandlingUnit

		public void TestService_AddPackageToHandlingUnit_PKNotPackage()
		{
			var pkgPK = ZGuid.NewZGuid();
			var errorMessage = new WhsPickDockDoorAssignmentService().AddPackageToHandlingUnit(pkgPK, ZGuid.NewZGuid());
			AssertEquals("Correct message reported.", $"Package could NOT be loaded using '{pkgPK}'.", errorMessage);
		}

		public void TestService_AddPackageToHandlingUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P02";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);
			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick1.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick2 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick2.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick3 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick3.WP_WL_DockDoor);

			var errorMessage = string.Empty;
			errorMessage = new WhsPickDockDoorAssignmentService().AddPackageToHandlingUnit(package2.PK, handlingUnitPackage.PK);

			AssertEquals("No exceptions were thrown.", string.Empty, errorMessage);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dockDoorAssignments = newFactory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA created", 1, dockDoorAssignments.Length);

			var dda = dockDoorAssignments[0];
			AssertEquals("DDA correct DDL", dda.WDA_WL_AssignedDockDoor, data.Whs1.WW_DefaultOutboundDockDoor);
			AssertEquals("DDA correct PutawayTime", ZDateTime.Empty, dda.WDA_FirstPutawayToDockDoorUtc);

			var pick1InNewFactory = newFactory.Load<WhsPick>(pick1.PK);
			AssertEquals("Pick1 DDA set", dda.PK, pick1InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1InNewFactory.WP_WL_DockDoor);

			var pick2InNewFactory = newFactory.Load<WhsPick>(pick2.PK);
			AssertEquals("Pick2 DDA set", dda.PK, pick2InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL cleared", ZGuid.Empty, pick2InNewFactory.WP_WL_DockDoor);

			var package2InNewFactory = newFactory.Load<PkgPackage>(package2.PK);
			AssertEquals("Pkg2 TopHU set", handlingUnitPackage.PK, package2InNewFactory.KP_KP_TopHandlingUnitPackage);

			var package2HUDivot = newFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, package2.PK)).Single();
			AssertEquals("Package2HUDivot KPD_KP_HandlingUnit set", handlingUnitPackage.PK, package2HUDivot.KPD_KP_HandlingUnit);
			AssertEquals("Package2HUDivot KPD_PackedTime set", false, package2HUDivot.KPD_PackedTime.IsEmpty);
			AssertEquals("Package2HUDivot KPD_GS_NKPackedUser set", false, package2HUDivot.KPD_GS_NKPackedUser.IsEmpty);
			AssertEquals("Package2HUDivot KPD_UnpackedTime NOT set", true, package2HUDivot.KPD_UnpackedTime.IsEmpty);
			AssertEquals("Package2HUDivot KPD_GS_NKUnpackedUser NOT set", true, package2HUDivot.KPD_GS_NKUnpackedUser.IsEmpty);

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package2.PK, DockDoorAssignmentLinkType.HandlingUnit, ZGuid.Empty, Factory);
			AssertEquals("Still only 1 DDA created", 1, new BusinessObjectFactory().Load<WhsDockDoorAssignment>(new ZQuery()).Length);

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package3.PK, DockDoorAssignmentLinkType.HandlingUnit, ZGuid.Empty, Factory);
			AssertEquals("Still only 1 DDA created", 1, new BusinessObjectFactory().Load<WhsDockDoorAssignment>(new ZQuery()).Length);

			// Pick 3 has no links so it should not need a DockDoorAssignment
			var pick3InNewFactory = newFactory.Load<WhsPick>(pick3.PK);
			AssertEquals("Pick3 DDA empty", ZGuid.Empty, pick3InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL cleared", data.Whs1.WW_DefaultOutboundDockDoor, pick3InNewFactory.WP_WL_DockDoor);
		}

		public void TestService_AddPackageToHandlingUnit_Merge()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O4", data.Part1, 5m);
			var order5 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O5", data.Part1, 5m);
			var order6 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O6", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			var pick3 = Helper.CreatePickNew(order3);
			var pick4 = Helper.CreatePickNew(order4);
			var pick5 = Helper.CreatePickNew(order5, order6);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);

			var package3 = order3.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order3.Lines[0].ReleaseLines[0], 5m);

			var package4 = order4.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order4.Lines[0].ReleaseLines[0], 5m);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			var dda1 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			dda1.WDA_SystemCreateTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			Factory.Save();

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package3, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package4, handlingUnitPackage);
			var dda2 = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick3, pick4);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick3 DDA", dda2.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick4 DDA", dda2.PK, pick4.WP_WDA_DockDoorAssignment);

			var package5 = order5.PackageJob.Packages.AddNew();
			package5.KP_PackageID = "P05";
			package5.Pack(order5.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package5.PK, 3);

			var package6 = order6.PackageJob.Packages.AddNew();
			package6.KP_PackageID = "P06";
			package6.Pack(order6.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();

			new WhsPickDockDoorAssignmentService().GeneratePickDockDoorAssignment(package5.PK, DockDoorAssignmentLinkType.Trolley, ZGuid.Empty, Factory);
			Factory.Save();

			new WhsPickDockDoorAssignmentService().AddPackageToHandlingUnit(package6.PK, handlingUnitPackage.PK);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("Only 1 DDA still exists 1 was deleted", 1, dockDoorAssignments.Length);
			AssertEquals("Earliest created DDA still exists", dda1.PK, dockDoorAssignments[0].PK);

			AssertEquals("Pick1 DDA still set", dda1.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL still cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA still set", dda1.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL still cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick3 DDA still set", dda1.PK, pick3.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick3 DDL still cleared", ZGuid.Empty, pick3.WP_WL_DockDoor);

			AssertEquals("Pick4 DDA still set", dda1.PK, pick4.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick4 DDL still cleared", ZGuid.Empty, pick4.WP_WL_DockDoor);
		}

		public void TestService_AddPackageToHandlingUnit_HasLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);
			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick1.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick2 DDL", data.Whs1.WW_DefaultOutboundDockDoor, pick2.WP_WL_DockDoor);

			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);
			Helper.CreateLoadPkgPackagePivot(handlingUnitPackage.PK, load);
			Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			var errorMessage = string.Empty;
			errorMessage = new WhsPickDockDoorAssignmentService().AddPackageToHandlingUnit(package2.PK, handlingUnitPackage.PK);

			AssertEquals("No exceptions were thrown.", string.Empty, errorMessage);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dockDoorAssignments = newFactory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("0 DDA created", 0, dockDoorAssignments.Length);

			var pick1InNewFactory = newFactory.Load<WhsPick>(pick1.PK);
			AssertEquals("Pick1 DDA cleared", ZGuid.Empty, pick1InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick1InNewFactory.WP_WL_DockDoor);

			var pick2InNewFactory = newFactory.Load<WhsPick>(pick2.PK);
			AssertEquals("Pick2 DDA cleared", ZGuid.Empty, pick2InNewFactory.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick2InNewFactory.WP_WL_DockDoor);

			var package2InNewFactory = newFactory.Load<PkgPackage>(package2.PK);
			AssertEquals("Pkg2 TopHU set", handlingUnitPackage.PK, package2InNewFactory.KP_KP_TopHandlingUnitPackage);

			var package2HUDivot = newFactory.Load<PkgPackageHandlingUnitDivot>(new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, package2.PK)).Single();
			AssertEquals("Package2HUDivot KPD_KP_HandlingUnit set", handlingUnitPackage.PK, package2HUDivot.KPD_KP_HandlingUnit);
			AssertEquals("Package2HUDivot KPD_PackedTime set", false, package2HUDivot.KPD_PackedTime.IsEmpty);
			AssertEquals("Package2HUDivot KPD_GS_NKPackedUser set", false, package2HUDivot.KPD_GS_NKPackedUser.IsEmpty);
			AssertEquals("Package2HUDivot KPD_UnpackedTime NOT set", true, package2HUDivot.KPD_UnpackedTime.IsEmpty);
			AssertEquals("Package2HUDivot KPD_GS_NKUnpackedUser NOT set", true, package2HUDivot.KPD_GS_NKUnpackedUser.IsEmpty);
		}

		#endregion

		#region TestService_RemovePickDockDoorAssignment

		public void TestService_RemovePickDockDoorAssignment_PKNotPackage()
		{
			var pkgPK = ZGuid.NewZGuid();
			var errorMessage = new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(pkgPK, Factory);
			AssertEquals("Correct message reported.", $"Package could NOT be loaded using '{pkgPK}'.", errorMessage);
		}

		public void TestService_RemovePickDockDoorAssignment_Empty()
		{
			var errorMessage = new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(ZGuid.Empty, Factory);
			AssertEquals("Correct message reported.", $"Package could NOT be loaded using '{ZGuid.Empty}'.", errorMessage);
		}

		public void TestService_RemovePickDockDoorAssignment_DockDoorAssignmentPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order2.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			dda.WDA_FirstPutawayToDockDoorUtc =	ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(package1.PK, Factory);

			AssertEquals("Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);
		}

		public void TestService_RemovePickDockDoorAssignment_NoDockDoorAssignment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);
			Factory.Save();

			AssertEquals("Precondition: Pick DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick.WP_WL_DockDoor);
			AssertEquals("Precondition: Pick DDA", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(package.PK, Factory);

			AssertEquals("Pick DDL", data.Whs1.DefaultOutboundDockDoorLocation.PK, pick.WP_WL_DockDoor);
			AssertEquals("Pick DDA", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
		}

		public void TestService_RemovePickDockDoorAssignment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package.PK, 1);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick.WP_WDA_DockDoorAssignment);

				new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(package.PK, Factory);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("No DDA exists", 0, dockDoorAssignments.Length);

			AssertEquals("Pick DDA correct", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick DDL correct", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
		}

		public void TestService_RemovePickDockDoorAssignment_OtherLinks_Trolley()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			var package12 = order1.PackageJob.Packages.AddNew();
			var package2 = order2.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package12.PK, 3);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(package1.PK, Factory);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("DDA exists", 1, dockDoorAssignments.Length);

			AssertEquals("Pick1 DDA cleared", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL set", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA cleared", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL set", ZGuid.Empty, pick2.WP_WL_DockDoor);
		}

		public void TestService_RemovePickDockDoorAssignment_OtherLinks_PBL()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order2.PackageJob.Packages.AddNew();
			var package3 = order1.PackageJob.Packages.AddNew();
			var package4 = order2.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);

			var userCode = GlbStaff.CurrentUser.GS_Code;
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package3.PK);
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				data.Whs1.WW_DefaultOutboundDockDoor, userCode, package4.PK);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(package1.PK, Factory);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("DDA exists", 1, dockDoorAssignments.Length);

			AssertEquals("Pick1 DDA cleared", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL set", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA cleared", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL set", ZGuid.Empty, pick2.WP_WL_DockDoor);
		}

		public void TestService_RemovePickDockDoorAssignment_OtherLinks_HU()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order2.PackageJob.Packages.AddNew();
			var package3 = order1.PackageJob.Packages.AddNew();
			var package4 = order2.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package3, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package4, handlingUnitPackage);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePickDockDoorAssignment(package1.PK, Factory);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("DDA exists", 1, dockDoorAssignments.Length);

			AssertEquals("Pick1 DDA cleared", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL set", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA cleared", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL set", ZGuid.Empty, pick2.WP_WL_DockDoor);
		}

		#endregion

		#region TestService_RemovePackageFromHandlingUnit

		public void TestService_RemovePackageFromHandlingUnit_PKNotPackage()
		{
			var pkgPK = ZGuid.NewZGuid();
			var errorMessage = new WhsPickDockDoorAssignmentService().RemovePackageFromHandlingUnit(pkgPK);
			AssertEquals("Correct message reported.", $"Package could NOT be loaded using '{pkgPK}'.", errorMessage);
		}

		public void TestService_RemovePackageFromHandlingUnit()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order2.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			var preTestHUDivotPkg2 = packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

			AssertEquals("Precondition: PreTestHUDivotPkg2 packed time is set", false, preTestHUDivotPkg2.KPD_PackedTime.IsEmpty);
			AssertEquals("Precondition: PreTestHUDivotPkg2 packed user is set", false, preTestHUDivotPkg2.KPD_GS_NKPackedUser.IsEmpty);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePackageFromHandlingUnit(package2.PK);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("DDA exists", 1, dockDoorAssignments.Length);

			AssertEquals("Pick1 DDA set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA cleared", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick2.WP_WL_DockDoor);

			var postTestHUDivotPkg2 = Factory.Load<PkgPackageHandlingUnitDivot>(preTestHUDivotPkg2.PK);
			AssertEquals("PostTestHUDivotPkg2 unpacked time is set", false, postTestHUDivotPkg2.KPD_UnpackedTime.IsEmpty);
			AssertEquals("PostTestHUDivotPkg2 unpacked user is set", false, postTestHUDivotPkg2.KPD_GS_NKUnpackedUser.IsEmpty);
		}

		public void TestService_RemovePackageFromHandlingUnit_LastOnDockDoorAssignment()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			var preTestHUDivot = packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			AssertEquals("Precondition: PreTestHUDivot packed time is set", false, preTestHUDivot.KPD_PackedTime.IsEmpty);
			AssertEquals("Precondition: PreTestHUDivot packed user is set", false, preTestHUDivot.KPD_GS_NKPackedUser.IsEmpty);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Factory.Save();

			AssertEquals("Precondition: Pick DDA", dda.PK, pick.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePackageFromHandlingUnit(package.PK);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("DDA gone", 0, dockDoorAssignments.Length);

			AssertEquals("Pick DDA set", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick DDL cleared", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);

			var postTestHUDivot = Factory.Load<PkgPackageHandlingUnitDivot>(preTestHUDivot.PK);
			AssertEquals("PostTestHUDivot unpacked time is set", false, postTestHUDivot.KPD_UnpackedTime.IsEmpty);
			AssertEquals("PostTestHUDivot unpacked user is set", false, postTestHUDivot.KPD_GS_NKUnpackedUser.IsEmpty);
		}

		public void TestService_RemovePackageFromHandlingUnit_LastOnDockDoorAssignment_HasLoad()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			var preTestHUDivot = packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);

			AssertEquals("Precondition: PreTestHUDivot packed time is set", false, preTestHUDivot.KPD_PackedTime.IsEmpty);
			AssertEquals("Precondition: PreTestHUDivot packed user is set", false, preTestHUDivot.KPD_GS_NKPackedUser.IsEmpty);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Factory.Save();

			AssertEquals("Precondition: Pick DDA", dda.PK, pick.WP_WDA_DockDoorAssignment);

			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L1", transportUnit: truck, startTime: DateTimeOffset.Now);
			Helper.CreateLoadPkgPackagePivot(handlingUnitPackage.PK, load);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			new WhsPickDockDoorAssignmentService().RemovePackageFromHandlingUnit(package.PK);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("DDA gone", 1, dockDoorAssignments.Length);

			AssertEquals("Pick1 DDA set", dda.PK, pick.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick.WP_WL_DockDoor);

			var postTestHUDivot = Factory.Load<PkgPackageHandlingUnitDivot>(preTestHUDivot.PK);
			AssertEquals("PostTestHUDivot unpacked time is set", false, postTestHUDivot.KPD_UnpackedTime.IsEmpty);
			AssertEquals("PostTestHUDivot unpacked user is set", false, postTestHUDivot.KPD_GS_NKUnpackedUser.IsEmpty);
		}

		public void TestService_RemovePackageFromHandlingUnit_OtherLink()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);

			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order2.PackageJob.Packages.AddNew();
			var package3 = order1.PackageJob.Packages.AddNew();
			var package4 = order2.PackageJob.Packages.AddNew();

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			var preTestHUDivotPkg3 = packingHelper.PackHandlingUnit(handlingUnitPackage, package3, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package4, handlingUnitPackage);

			AssertEquals("Precondition: PreTestHUDivotPkg3 packed time is set", false, preTestHUDivotPkg3.KPD_PackedTime.IsEmpty);
			AssertEquals("Precondition: PreTestHUDivotPkg3 packed user is set", false, preTestHUDivotPkg3.KPD_GS_NKPackedUser.IsEmpty);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			AssertEquals("Precondition: Pick1 DDA", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Precondition: Pick2 DDA", dda.PK, pick2.WP_WDA_DockDoorAssignment);

			new WhsPickDockDoorAssignmentService().RemovePackageFromHandlingUnit(package3.PK);

			var dockDoorAssignments = Factory.Load<WhsDockDoorAssignment>(new ZQuery());
			AssertEquals("DDA exists", 1, dockDoorAssignments.Length);

			AssertEquals("Pick1 DDA set", dda.PK, pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick1 DDL cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 DDA set", dda.PK, pick2.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick2 DDL cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			var postTestHUDivotPkg3 = Factory.Load<PkgPackageHandlingUnitDivot>(preTestHUDivotPkg3.PK);
			AssertEquals("PostTestHUDivotPkg3 unpacked time is set", false, postTestHUDivotPkg3.KPD_UnpackedTime.IsEmpty);
			AssertEquals("PostTestHUDivotPkg3 unpacked user is set", false, postTestHUDivotPkg3.KPD_GS_NKUnpackedUser.IsEmpty);
		}

		#endregion

		#region TestService_IsDockDoorOverrideAllowedForHandlingUnit

		public void TestService_IsDockDoorOverrideAllowedForHandlingUnit_Factory()
		{
			TestService_IsDockDoorOverrideAllowedForHandlingUnitCore(inputFactory: true);
		}

		public void TestService_IsDockDoorOverrideAllowedForHandlingUnit_NoFactory()
		{
			TestService_IsDockDoorOverrideAllowedForHandlingUnitCore(inputFactory: false);
		}

		void TestService_IsDockDoorOverrideAllowedForHandlingUnitCore(bool inputFactory)
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

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			var service = new WhsPickDockDoorAssignmentService();
			AssertEquals(
				"GetIsDockDoorOverrideAllowedForHandlingUnit should be false",
				$"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides",
				service.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackage.PK, inputFactory ? Factory : null));

			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForHandlingUnit should be true",
				string.Empty,
				service.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackage.PK, inputFactory ? Factory : null));
		}

		public void TestService_IsDockDoorOverrideAllowedForHandlingUnit_Linked()
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

			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", data.Part2, 10m);

			var client3 = Helper.CreateClient("CL3");
			Helper.CreateProductClientRelationShip(client3, data.Part2);
			var pickPackParam3 = Helper.CreatePickPackParameter(client3, data.Whs1);
			pickPackParam3.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(client3, data.Whs1, "R3", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			var order3 = Helper.CreateWhsOrderWithOrderLine(client3, data.Whs1, data.Part2, 10m);
			var pick3 = Helper.CreatePickNew(order3);

			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P01";
			package1.Pack(order1.Lines[0].ReleaseLines[0], 10m);

			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "P02";
			package2.Pack(order2.Lines[0].ReleaseLines[0], 10m);

			var package3 = order1.PackageJob.Packages.AddNew();
			package3.KP_PackageID = "P03";
			package3.Pack(order1.Lines[0].ReleaseLines[0], 10m);

			var package4 = order3.PackageJob.Packages.AddNew();
			package4.KP_PackageID = "P04";
			package4.Pack(order3.Lines[0].ReleaseLines[0], 10m);

			var handlingUnit = packingHelper.CreatePkgHandlingUnit(data.Whs1.WW_GB_RelatedCompanyBranch, "3PL");
			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
			packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package3.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package4.PK, 2);
			Factory.Save();

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2, pick3);
			Factory.Save();

			var service = new WhsPickDockDoorAssignmentService();
			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks1 should be returning error",
				true,
				service.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackage.PK).Contains("Cannot override Dock Door Location as Client"));

			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks2 should be returning error",
				true,
				service.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackage.PK).Contains("Cannot override Dock Door Location as Client"));

			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks3 should be returning error",
				true,
				service.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackage.PK).Contains("Cannot override Dock Door Location as Client"));

			pickPackParam3.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks4 should be returning no error",
				string.Empty,
				service.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackage.PK));

			dda.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks5 should be false, when WDA_FirstPutawayToDockDoorUtc is set",
				$"Cannot override Dock Door Location as Stock is already putaway to '{data.Whs1.DefaultInboundDockDoorLocation.WLV_LocationString_UserFriendly}'",
				service.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackage.PK));
		}

		#endregion

		#region TestService_IsDockDoorOverrideAllowedForPicks

		public void TestService_IsDockDoorOverrideAllowedForPicks()
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

			var service = new WhsPickDockDoorAssignmentService();
			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks should be false",
				$"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides",
				service.GetIsDockDoorOverrideAllowedForPicks([pick], Factory));

			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks should be true",
				string.Empty,
				service.GetIsDockDoorOverrideAllowedForPicks([pick], Factory));
		}

		public void TestService_IsDockDoorOverrideAllowedForPicks_PlannedLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = false;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultInboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;
			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package, pickLines[0]);
			Factory.Save();

			AssertEquals("Precondition: WD_WLO_PlannedLoad", data.Whs1.WW_DefaultOutboundDockDoor, order.PlannedLoad.WLO_WL_PlannedDockDoor);

			var service = new WhsPickDockDoorAssignmentService();
			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks should be returning error",
				$"Cannot override Dock Door Location as Client '{data.Org1.OH_FullNameTruncated}' does not allow overrides",
				service.GetIsDockDoorOverrideAllowedForPicks([pick], Factory));

			pickPackParam.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals("GetIsDockDoorOverrideAllowedForPicks should be returning no error",
				string.Empty,
				service.GetIsDockDoorOverrideAllowedForPicks([pick], Factory));
		}

		public void TestService_IsDockDoorOverrideAllowedForPicks_Linked_Trolley()
		{
			TestService_IsDockDoorOverrideAllowedForPicksLinkedCore(
				(_, package1, package2) =>
				{
					var trolley = Helper.CreateTrolley("T1");
					var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley.PK, "BLD");
					Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package1.PK, 1);
					Helper.CreateWhsPickTrolleySlot(trolleyJob.PK, package2.PK, 2);
				});
		}

		public void TestService_IsDockDoorOverrideAllowedForPicks_Linked_PickByLabel()
		{
			TestService_IsDockDoorOverrideAllowedForPicksLinkedCore(
				(whs, package1, package2) =>
				{
					var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Factory, whs.PK, "AAA", whs.WW_DefaultOutboundDockDoor);
					WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
					WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);
				});
		}

		public void TestService_IsDockDoorOverrideAllowedForPicks_Linked_HandlingUnit()
		{
			TestService_IsDockDoorOverrideAllowedForPicksLinkedCore(
				(whs, package1, package2) =>
				{
					var packingHelper = new PackingTestHelper(whs.Factory);
					var handlingUnit = packingHelper.CreatePkgHandlingUnit(whs.WW_GB_RelatedCompanyBranch, "3PL");
					var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
					var handlingUnitPackage = packingHelper.CreatePackage(handlingUnitPackageJob, "HU1", 1, PkgUnit.Package);
					packingHelper.PackHandlingUnit(handlingUnitPackage, package1, handlingUnitPackage);
					packingHelper.PackHandlingUnit(handlingUnitPackage, package2, handlingUnitPackage);
				});
		}

		public void TestService_IsDockDoorOverrideAllowedForPicksLinkedCore(Action<WhsWarehouse, PkgPackage, PkgPackage> linkPicks)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
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

			linkPicks(data.Whs1, package1, package2);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Factory.Save();

			var service = new WhsPickDockDoorAssignmentService();
			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks should be returning error",
				true,
				service.GetIsDockDoorOverrideAllowedForPicks([pick1, pick2], Factory).Contains("Cannot override Dock Door Location as Client"));

			// Enable 1 param
			pickPackParam1.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals("GetIsDockDoorOverrideAllowedForPicks should be returning error",
				true,
				service.GetIsDockDoorOverrideAllowedForPicks([pick1, pick2], Factory).Contains("Cannot override Dock Door Location as Client"));

			// Enable both params
			pickPackParam2.WPP_AllowPickDockDoorLocationOverride = true;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks should be returning not error",
				string.Empty,
				service.GetIsDockDoorOverrideAllowedForPicks([pick1, pick2], Factory));

			dda.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(
				"GetIsDockDoorOverrideAllowedForPicks should be false, when WDA_FirstPutawayToDockDoorUtc is set",
				$"Cannot override Dock Door Location as Stock is already putaway to '{data.Whs1.DefaultInboundDockDoorLocation.WLV_LocationString_UserFriendly}'",
				service.GetIsDockDoorOverrideAllowedForPicks([pick1, pick2], Factory));
		}

		#endregion

		#region TestCreatePutawayDockDoorAssignmentForPick

		public void TestService_CreatePutawayDockDoorAssignmentForPicks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick1 = (IWhsPick)Helper.CreatePickNew(order1);
			var pick2 = (IWhsPick)Helper.CreatePickNew(order2);

			var errorMessage = new WhsPickDockDoorAssignmentService().TryCreateAndPutawayDockDoorAssignmentForPicks([pick1, pick2], Factory);
			AssertEquals("No error message", string.Empty, errorMessage);

			AssertEquals("Pick1 WP_WDA_DockDoorAssignment set", true, pick1.WP_WDA_DockDoorAssignment.IsValid);
			AssertEquals("Pick1 WP_WL_DockDoor cleared", ZGuid.Empty, pick1.WP_WL_DockDoor);

			AssertEquals("Pick2 WP_WDA_DockDoorAssignment set", true, pick2.WP_WDA_DockDoorAssignment.IsValid);
			AssertEquals("Pick2 WP_WL_DockDoor cleared", ZGuid.Empty, pick2.WP_WL_DockDoor);

			AssertEquals("Pick1 & Pick2 on same DockDoorAssignment", pick2.WP_WDA_DockDoorAssignment, pick2.WP_WDA_DockDoorAssignment);
			var dockDoorAssignment = Factory.Load<WhsDockDoorAssignment>(pick1.WP_WDA_DockDoorAssignment);
			AssertEquals("DockDoorAssignment putaway", true, dockDoorAssignment.WDA_FirstPutawayToDockDoorUtc.IsValid);
			AssertEquals("DockDoorAssignment DDL", data.Whs1.WW_DefaultOutboundDockDoor, dockDoorAssignment.WDA_WL_AssignedDockDoor);
		}

		public void TestService_CreatePutawayDockDoorAssignmentForPicks_FinalisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var errorMessage = new WhsPickDockDoorAssignmentService().TryCreateAndPutawayDockDoorAssignmentForPicks([pick], Factory);
			AssertEquals("Error message", "Cannot complete dock door putaway for a finalized, canceled or already putaway Pick.", errorMessage);

			AssertEquals("Pick WP_WDA_DockDoorAssignment empty", ZGuid.Empty, pick.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick WP_WL_DockDoor set", data.Whs1.WW_DefaultOutboundDockDoor, pick.WP_WL_DockDoor);
		}

		public void TestService_CreatePutawayDockDoorAssignmentForPicks_PickAlreadyHas_DockDoorAssignment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var dockDoorAssignment = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Factory.Save();

			var errorMessage = new WhsPickDockDoorAssignmentService().TryCreateAndPutawayDockDoorAssignmentForPicks([pick], Factory);
			AssertEquals("Error message", string.Empty, errorMessage);

			AssertEquals("Pick WP_WDA_DockDoorAssignment set", dockDoorAssignment.PK, pick.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick WP_WL_DockDoor empty", ZGuid.Empty, pick.WP_WL_DockDoor);

			AssertEquals("DockDoorAssignment putaway", true, dockDoorAssignment.WDA_FirstPutawayToDockDoorUtc.IsValid);
			AssertEquals("DockDoorAssignment DDL", data.Whs1.WW_DefaultOutboundDockDoor, dockDoorAssignment.WDA_WL_AssignedDockDoor);
		}

		public void TestService_CreatePutawayDockDoorAssignmentForPicks_PickAlreadyHas_PutawayDockDoorAssignment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var dockDoorAssignment = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick);
			Factory.Save();

			var time = ZDateTime.UtcNow;
			dockDoorAssignment.WDA_FirstPutawayToDockDoorUtc = time;
			Factory.Save();

			var errorMessage = new WhsPickDockDoorAssignmentService().TryCreateAndPutawayDockDoorAssignmentForPicks([pick], Factory);
			AssertEquals("Error message", string.Empty, errorMessage);

			AssertEquals("Pick WP_WDA_DockDoorAssignment set", dockDoorAssignment.PK, pick.WP_WDA_DockDoorAssignment);
			AssertEquals("Pick WP_WL_DockDoor empty", ZGuid.Empty, pick.WP_WL_DockDoor);

			AssertEquals("DockDoorAssignment putaway time", time, dockDoorAssignment.WDA_FirstPutawayToDockDoorUtc);
			AssertEquals("DockDoorAssignment DDL", data.Whs1.WW_DefaultOutboundDockDoor, dockDoorAssignment.WDA_WL_AssignedDockDoor);
		}

		#endregion
	}
}

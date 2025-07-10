using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	[TestedType(typeof(WhsPickByLabelLabel))]
	public class WhsPickByLabelLabelTest : WhsBusinessObjectTestCase
	{
		#region Related Business Objects

		#region WhsPickByLabelLabel

		public void TestWhsPickByLabelJob()
		{
			var label = Factory.New<WhsPickByLabelLabel>();
			AssertNull(label.PickByLabelJob);

			var job = Factory.New<WhsPickByLabelJob>();
			label.WTL_WTK_PickByLabelJob = job.PK;
			AssertEquals(job, label.PickByLabelJob);
		}

		#endregion

		#region Package

		public void TestPackage()
		{
			var label = Factory.New<WhsPickByLabelLabel>();
			AssertNull(label.Package);

			var package = Factory.New<PkgPackage>();
			label.WTL_KP_Package = package.PK;
			AssertEquals(package, label.Package);
		}

		#endregion

		#region TestDelete

		public void TestDelete_IfJobOnlyHasOneLabel()
		{
			var label = Factory.New<WhsPickByLabelLabel>();
			var job = Factory.New<WhsPickByLabelJob>();
			label.WTL_WTK_PickByLabelJob = job.PK;

			label.Delete();
			AssertEquals(false, job.IsDeleted);
			AssertEquals(true, label.IsDeleted);
		}

		public void TestDelete_IfJobHasMultipleLabels()
		{
			var label1 = Factory.New<WhsPickByLabelLabel>();
			var label2 = Factory.New<WhsPickByLabelLabel>();
			var job = Factory.New<WhsPickByLabelJob>();
			label1.WTL_WTK_PickByLabelJob = job.PK;
			label2.WTL_WTK_PickByLabelJob = job.PK;

			label1.Delete();
			AssertEquals(false, job.IsDeleted);
			AssertEquals(true, label1.IsDeleted);
			AssertEquals(false, label2.IsDeleted);
		}

		#endregion

		#region TestDockDoorLocationPK

		public void TestDockDoorLocationPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var label = Factory.New<WhsPickByLabelLabel>();
			AssertNull(label.DockDoorLocationPK);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			AssertNull("label does not have package with order and pick.", label.DockDoorLocationPK);

			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_DockDoor = ZGuid.NewZGuid();
			AssertNull("label does not have package with order and pick.", label.DockDoorLocationPK);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			label.WTL_KP_Package = package.PK;
			AssertEquals("Should get dock door location from pick.", pick.WP_WL_DockDoor, label.DockDoorLocationPK);

			pick.WP_WL_DockDoor = ZGuid.Empty;
			pick.WP_WW_Whs = data.Whs1.PK;
			AssertEquals("Pick has no WP_WL_DockDoor value.", ZGuid.Empty, label.DockDoorLocationPK);

			var dda = Factory.New<WhsDockDoorAssignment>();
			dda.WDA_WL_AssignedDockDoor = ZGuid.NewZGuid();
			pick.WP_WDA_DockDoorAssignment = dda.PK;
			AssertEquals("Should get dock door location from dock door assignment.", dda.WDA_WL_AssignedDockDoor, label.DockDoorLocationPK);
		}

		#endregion

		#region TestPickForWhsPickByLabel

		public void TestPickForWhsPickByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var label = Factory.New<WhsPickByLabelLabel>();
			AssertNull(label.Pick);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			AssertNull("label does not have package with order and pick.", label.Pick);

			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_DockDoor = ZGuid.NewZGuid();
			AssertNull("label does not have package with order and pick.", label.Pick);

			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			label.WTL_KP_Package = package.PK;
			AssertEquals("Should be able to get the related Pick.", pick.PK, label.Pick.PK);
		}

		#endregion

		#endregion

		#region Properties

		public void TestIsPickedFromPutawayLocation()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "123");
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			var label = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertEquals("Not started label should not be picked for putaway.", false, label.IsPickedFromPutawayLocation);

			var pickLine = package.GetPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Picked label should be picked for putaway.", true, label.IsPickedFromPutawayLocation);
		}

		public void TestIsPickedFromPutawayLocation_OutboundTransfer()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "123");
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			var label = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			Factory.Save();
			AssertEquals("Not started label should not be picked for putaway.", false, label.IsPickedFromPutawayLocation);

			var pickLine = package.GetPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			var pickLineFromOutboundTransfer = pickLine.InventoryLine.PickLines.Single();
			AssertNotEquals("New pick line should be created for DDL.", pickLineFromOutboundTransfer, pickLine);
			AssertEquals("Pick line attached to package and label should not be picked.", false, pickLine.IsPicked);
			AssertEquals("Pick line from outbound trasnfer should be picked.", true, pickLineFromOutboundTransfer.IsPicked);
			AssertEquals("Label should be picked for putaway.", true, label.IsPickedFromPutawayLocation);
		}

		public void TestIsPutawayIntoOutboundDDL()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "123");
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			var label = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertEquals("Not started label should not be putaway into outbound DDL.", false, label.IsPutawayIntoOutboundDDL);

			var pickLine = package.GetPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Picked label should not be putaway into outbound DDL.", false, label.IsPutawayIntoOutboundDDL);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Pick should be finalised", true, pick.IsFinalised);
			AssertEquals("Labels linked to finalised jobs should be putaway into outbound DDL.", true, label.IsPutawayIntoOutboundDDL);
		}

		public void TestIsPutawayIntoOutboundDDL_WithOutboundTransfer()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			Helper.CreatePickNew(order);

			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "123");
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			var label = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			Factory.Save();
			AssertEquals("Not started label should not be putaway into outbound DDL.", false, label.IsPutawayIntoOutboundDDL);

			var pickLine = package.GetPickLines().Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			var pickLineFromOutboundTransfer = pickLine.InventoryLine.PickLines.Single();
			AssertNotEquals("New pick line should be created for DDL.", pickLineFromOutboundTransfer, pickLine);
			AssertEquals("Pick line attached to package and label should not be picked.", false, pickLine.IsPicked);
			AssertEquals("Pick line from outbound trasnfer should be picked.", true, pickLineFromOutboundTransfer.IsPicked);
			AssertEquals("Picked label should not be putaway into outbound DDL.", false, label.IsPutawayIntoOutboundDDL);

			transferLine.FinaliseDocketLine();
			AssertEquals("Outbound transfer line should be finalised", true, transferLine.IsFinalised);
			AssertEquals("Pick line attached to package and label should not be picked.", false, pickLine.IsPicked);
			AssertEquals("Pick line from outbound trasnfer should be picked.", true, pickLineFromOutboundTransfer.IsPicked);
			AssertEquals("Labels linked to finalised outbound transfer line should be putaway into outbound DDL.", true, label.IsPutawayIntoOutboundDDL);
		}

		#endregion

		#region TestIsUsingDirectedPackingConsolidation

		public void TestIsUsingDirectedPackingConsolidation_True()
		{
			TestIsUsingDirectedPackingConsolidationCore(usingPCO: true);
		}

		public void TestIsUsingDirectedPackingConsolidation_False()
		{
			TestIsUsingDirectedPackingConsolidationCore(usingPCO: false);
		}

		void TestIsUsingDirectedPackingConsolidationCore(bool usingPCO)
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			order.WD_UseDirectedPackingConsolidation = usingPCO;
			Helper.CreatePickNew(order);

			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "123");
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			var label = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);
			AssertEquals("IsUsingDirectedPackingConsolidation correct.", usingPCO, label.IsUsingDirectedPackingConsolidation);
		}

		#endregion

		#region TestAddPackgeToListOfPickByLabelForUser

		public void TestAddPackgeToListOfPickByLabelForUser()
		{
			var user = "me";
			var warehousePK = ZGuid.NewZGuid();
			var ddlPK = ZGuid.NewZGuid();
			var package = Factory.New<PkgPackage>();

			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, warehousePK, ddlPK, user, package.PK);

			var pickByLabelJob = Factory.Load<WhsPickByLabelJob>(new ZQuery()).Single();
			AssertEquals(user, pickByLabelJob.WTK_GS_NKAssignedTo);
			AssertEquals(warehousePK, pickByLabelJob.WTK_WW_Warehouse);
			AssertEquals(ddlPK, pickByLabelJob.WTK_WL_DockDoor);
			AssertEquals(package.PK, pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.PK);
		}

		#endregion

		#region IWhsPickByLabelLabel Members

		public void TestIWhsPickByLabelLabel_PK()
		{
			var pickByLabelLabel = Factory.New<WhsPickByLabelLabel>();
			AssertEquals("PK is correct", pickByLabelLabel.PK, ((IWhsPickByLabelLabel)pickByLabelLabel).PK);
		}

		public void TestIWhsPickByLabelLabel_WTL_KP_Package()
		{
			var packagePK = ZGuid.NewZGuid();
			var pickByLabelLabel = Factory.New<WhsPickByLabelLabel>();
			pickByLabelLabel.WTL_KP_Package = packagePK;
			AssertEquals("Package is correct", pickByLabelLabel.WTL_KP_Package, ((IWhsPickByLabelLabel)pickByLabelLabel).WTL_KP_Package);
		}

		public void TestIWhsPickByLabelLabel_WTK_WW_Warehouse()
		{
			var labelJobPK = ZGuid.NewZGuid();
			var pickByLabelLabel = Factory.New<WhsPickByLabelLabel>();
			pickByLabelLabel.WTL_WTK_PickByLabelJob = labelJobPK;
			AssertEquals("PickByLabelJob is correct", pickByLabelLabel.WTL_WTK_PickByLabelJob, ((IWhsPickByLabelLabel)pickByLabelLabel).WTL_WTK_PickByLabelJob);
		}

		#endregion

		#region Implementations

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetWhsPickByLabelLabelWithValidDataForTesting(factory);

		static WhsPickByLabelLabel GetWhsPickByLabelLabelWithValidDataForTesting(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory);
			var helper = new WhsTestHelperFunctions(factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			factory.Save();

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, "Bob");
			var pickByLabelLabel = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);

			return pickByLabelLabel;
		}

		#endregion
	}
}


using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.PickByLabel.Testing
{
	public class WhsPickByLabelHelperTest : WhsTestCaseWithFactory
	{
		#region TestArgumentException
		public void TestArgumentNullException_AddPackageToListOfPickByLabelForUser()
		{
			AssertExceptionThrown("Factory cannot be empty.", typeof(ArgumentNullException), () => WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(null, ZGuid.NewZGuid(), ZGuid.NewZGuid(), GlbStaff.CurrentUser.GS_Code, ZGuid.NewZGuid()));
			AssertExceptionThrown("Warehouse cannot be empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(new BusinessObjectFactory(), ZGuid.Empty, ZGuid.NewZGuid(), GlbStaff.CurrentUser.GS_Code, ZGuid.NewZGuid()));
			AssertExceptionThrown("Dock door location cannot be empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(new BusinessObjectFactory(), ZGuid.NewZGuid(), ZGuid.Empty, GlbStaff.CurrentUser.GS_Code, ZGuid.NewZGuid()));
			AssertExceptionThrown("Staff code cannot be null or empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(new BusinessObjectFactory(), ZGuid.NewZGuid(), ZGuid.NewZGuid(), null, ZGuid.NewZGuid()));
			AssertExceptionThrown("Staff code cannot be null or empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(new BusinessObjectFactory(), ZGuid.NewZGuid(), ZGuid.NewZGuid(), "", ZGuid.NewZGuid()));
			AssertExceptionThrown("Package cannot be empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(new BusinessObjectFactory(), ZGuid.NewZGuid(), ZGuid.NewZGuid(), GlbStaff.CurrentUser.GS_Code, ZGuid.Empty));
		}

		public void TestArgumentException_PickByLabelPackageFinder()
		{
			AssertExceptionThrown("Factory cannot be empty.", typeof(ArgumentNullException), () => WhsPickByLabelHelper.PickByLabelPackageFinder(null, "PACKAGE-1", ZGuid.NewZGuid(), GlbStaff.CurrentUser));
			AssertExceptionThrown("Staff code cannot be null.", typeof(ArgumentNullException), () => WhsPickByLabelHelper.PickByLabelPackageFinder(new BusinessObjectFactory(), "PACKAGE-1", ZGuid.NewZGuid(), null));
			AssertExceptionThrown("Package cannot be null or empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.PickByLabelPackageFinder(new BusinessObjectFactory(), null, ZGuid.NewZGuid(), GlbStaff.CurrentUser));
			AssertExceptionThrown("Package cannot be null or empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.PickByLabelPackageFinder(new BusinessObjectFactory(), "", ZGuid.NewZGuid(), GlbStaff.CurrentUser));
		}

		public void TestArgumentNullException_GetOrCreatePickByLabelJob()
		{
			AssertExceptionThrown("Factory cannot be empty.", typeof(ArgumentNullException), () => WhsPickByLabelHelper.GetOrCreatePickByLabelJob(null, ZGuid.NewZGuid(), GlbStaff.CurrentUser.GS_Code, ZGuid.NewZGuid()));
			AssertExceptionThrown("Warehouse cannot be empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.GetOrCreatePickByLabelJob(new BusinessObjectFactory(), ZGuid.Empty, GlbStaff.CurrentUser.GS_Code, ZGuid.NewZGuid()));
			AssertExceptionThrown("Staff code cannot be null or empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.GetOrCreatePickByLabelJob(new BusinessObjectFactory(), ZGuid.NewZGuid(), null, ZGuid.NewZGuid()));
			AssertExceptionThrown("Staff code cannot be null or empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.GetOrCreatePickByLabelJob(new BusinessObjectFactory(), ZGuid.NewZGuid(), "", ZGuid.NewZGuid()));
			AssertExceptionThrown("Dock door location cannot be empty.", typeof(ArgumentException), () => WhsPickByLabelHelper.GetOrCreatePickByLabelJob(new BusinessObjectFactory(), ZGuid.NewZGuid(), GlbStaff.CurrentUser.GS_Code, ZGuid.Empty));
		}

		#endregion

		#region AddPackageToListOfPickByLabelForUser

		public void TestAddPackageToListOfPickByLabelForUser()
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

		#region TestPickByLabelPackageFinder

		public void TestPickByLabelPackageFinder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var packingHelper = new PackingTestHelper(Factory);
			packingHelper.SetRefPackTypeUOM("PLT", "PLT");
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var package = order.PackageJob.Packages.AddNew("PLT", "PACKAGE-1");
			var releaseLine = order.Lines[0].ReleaseLines[0];
			package.Pack(releaseLine, 5m);
			Factory.Save();

			var packageFinder = WhsPickByLabelHelper.PickByLabelPackageFinder(Factory, "PACKAGE-1", data.Whs1.PK, GlbStaff.CurrentUser);
			AssertEquals("Should be able to return package finder.", package, packageFinder.Package);
			AssertEquals("Should be able to return package finder.", pick.WP_WL_DockDoor, packageFinder.PickDockDoorLocationPK);
		}

		#endregion

		#region GetOrCreatePickByLabelJob

		public void GetOrCreatePickByLabelJob()
		{
			var user = "me";
			var warehousePK = ZGuid.NewZGuid();
			var ddlPK = ZGuid.NewZGuid();
			var package = Factory.New<PkgPackage>();

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, warehousePK, ddlPK, user, package.PK);

			AssertEquals(user, pickByLabelJob.WTK_GS_NKAssignedTo);
			AssertEquals(warehousePK, pickByLabelJob.WTK_WW_Warehouse);
			AssertEquals(ddlPK, pickByLabelJob.WTK_WL_DockDoor);
			AssertEquals(package.PK, pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.PK);

			var pickByLabelJob2 = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, warehousePK, ddlPK, user, package.PK);
			AssertEquals("Should not create new pick by label job.", pickByLabelJob, pickByLabelJob2);
			AssertEquals("Should not create new pick by label job.", user, pickByLabelJob2.WTK_GS_NKAssignedTo);
			AssertEquals("Should not create new pick by label job.", warehousePK, pickByLabelJob2.WTK_WW_Warehouse);
			AssertEquals("Should not create new pick by label job.", ddlPK, pickByLabelJob2.WTK_WL_DockDoor);
			AssertEquals("Should not create new pick by label job.", package.PK, pickByLabelJob2.Labels.Cast<WhsPickByLabelLabel>().Single().Package.PK);
			AssertEquals("Should not create new pick by label job.", pickByLabelJob, Factory.Load<WhsPickByLabelJob>(new ZQuery()).Single());
		}

		#endregion

		#region TestGetOrCreatePickByLabelLabel

		public void TestGetOrCreatePickByLabelLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var package = Factory.New<PkgPackage>();

			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_WW_Warehouse = data.Whs1.PK;
			pickByLabelJob.WTK_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			pickByLabelJob.WTK_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;

			var label = WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package.PK);
			AssertEquals("Package has correct task", package.PK, label.Package.PK);
		}

		#endregion

		#region TestPutawayPickedLabelsAndSplitJobIfNecessary

		public void TestPutawayPickedLabelsAndSplitJobIfNecessary()
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
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);

			var pickLine = package.GetPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			Factory.Save();
			AssertEquals("Precondition", false, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
			AssertEquals("Should have finalised PBL job.", true, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			var splitPickByLabelJob = Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.PK, SQLComparisonOperator.NotEqual, pickByLabelJob.PK)).SingleOrDefault();
			AssertNull("No new PBL jobs should be created.", splitPickByLabelJob);
		}

		public void TestPutawayPickedLabelsAndSplitJobIfNecessary_WithSplit()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var packageJob = order.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "123");
			var package2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "456");
			package1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package2.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			var label1 = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package1.PK);
			var label2 = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package2.PK);

			var pickLine2 = package2.GetPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			Factory.Save();
			AssertEquals("Precondition", false, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
			AssertEquals("Should have finalised PBL job.", true, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);
			AssertEquals("Should have kept Picked Label attached to finalised job.", label2, pickByLabelJob.Labels.Single());

			var splitPickByLabelJob = Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.PK, SQLComparisonOperator.NotEqual, pickByLabelJob.PK)).SingleOrDefault();
			AssertEquals("New PBL job should NOT be finalised.", false, !splitPickByLabelJob.WTK_FinalisedDate.IsEmpty);
			AssertEquals("Should have not picked label attached to split job.", label1, splitPickByLabelJob.Labels.Single());
		}

		#endregion

		#region TestCloseAndSplitNotPutawayLabelsIfNecessary

		public void TestCloseAndSplitNotPutawayLabelsIfNecessary()
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
			WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package.PK);

			var pickLine = package.GetPickLines().Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();
			AssertIsFinalisedPrecondition(transferLine);
			AssertEquals("Precondition", false, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			WhsPickByLabelHelper.CloseAndSplitNotPutawayLabelsIfNecessary(pickByLabelJob);
			AssertEquals("Should have finalised PBL job.", true, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			var splitPickByLabelJob = Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.PK, SQLComparisonOperator.NotEqual, pickByLabelJob.PK)).SingleOrDefault();
			AssertNull("No new PBL jobs should be created.", splitPickByLabelJob);
		}

		public void TestCloseAndSplitNotPutawayLabelsIfNecessary_WithSplit()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("A", "A");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var packageJob = order.PackageJob;
			var package1 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "123");
			var package2 = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet, "456");
			package1.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package2.Pack(order.Lines[0].ReleaseLines[0], 5m);

			var pickByLabelJob = WhsPickByLabelHelperForTesting.CreateWhsPickByLabelJob(Helper.Factory, data.Whs1.PK, data.Whs1.DefaultOutboundDockDoorLocation.PK, staff.GS_Code);
			var label1 = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package1.PK);
			var label2 = WhsPickByLabelHelperForTesting.AddNewPackageForJob(pickByLabelJob, package2.PK);

			var pickLine1 = package1.GetPickLines().Single();
			var pickLine2 = package2.GetPickLines().Single();
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.FinaliseDocketLine();
			Factory.Save();
			AssertIsFinalisedPrecondition(transferLine2);
			AssertEquals("Precondition", false, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);

			WhsPickByLabelHelper.CloseAndSplitNotPutawayLabelsIfNecessary(pickByLabelJob);
			AssertEquals("Should have finalised PBL job.", true, !pickByLabelJob.WTK_FinalisedDate.IsEmpty);
			AssertEquals("Should have kept Putaway Label attached to finalised job.", label2, pickByLabelJob.Labels.Single());

			var splitPickByLabelJob = Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.PK, SQLComparisonOperator.NotEqual, pickByLabelJob.PK)).SingleOrDefault();
			AssertEquals("New PBL job should NOT be finalised.", false, !splitPickByLabelJob.WTK_FinalisedDate.IsEmpty);
			AssertEquals("Should have Picked but not Putaway Label attached to split job.", label1, splitPickByLabelJob.Labels.Single());
		}

		#endregion

		#region TestHasAnyPackageAssignedToAPickByLabelJob

		public void TestHasAnyPackageAssignedToAPickByLabelJob_JobChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("HasAnyPackageAssignedToAPickByLabelJob should be false for no job", false, order.HasAnyPackageAssignedToAPickByLabelJob());

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			AssertEquals("HasAnyPackageAssignedToAPickByLabelJob should be true for active job of packed package", true, order.HasAnyPackageAssignedToAPickByLabelJob());

			pickByLabelJob.WTK_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Should have finalised the pick by label job.", false, pickByLabelJob.WTK_FinalisedDate.IsEmpty);
			Factory.Save();
			AssertEquals("HasAnyPackageAssignedToAPickByLabelJob should be true for closed job", true, order.HasAnyPackageAssignedToAPickByLabelJob());

			pickByLabelJob.Labels[0].Delete();
			Factory.Save();
			AssertEquals("HasAnyPackageAssignedToAPickByLabelJob should be false for deleted joblabel, label of packed package.", false, order.HasAnyPackageAssignedToAPickByLabelJob());
		}

		public void TestHasAnyPackageAssignedToAPickByLabelJob_EmptyPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			AssertEquals("HasAnyPackageAssignedToAPickByLabelJob should be true for active job of empty package", true, order.HasAnyPackageAssignedToAPickByLabelJob());

			WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(pickByLabelJob);
			Factory.Save();
			AssertEquals("HasAnyPackageAssignedToAPickByLabelJob should be true for closed job of empty package", true, order.HasAnyPackageAssignedToAPickByLabelJob());

			pickByLabelJob.Labels[0].Delete();
			Factory.Save();
			AssertEquals("HasAnyPackageAssignedToAPickByLabelJob should be false for deleted joblabel, label of empty package.", false, order.HasAnyPackageAssignedToAPickByLabelJob());
		}

		public void TestHasAnyPackageAssignedToAPickByLabelJob_NoPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			AssertNoExceptionThrown(() => { _ = order.HasAnyPackageAssignedToAPickByLabelJob(); });
		}

		#endregion

		#region TestIsPackageAssignedToPickByLabelJob

		public void TestIsPackageAssignedToPickByLabelJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			AssertEquals("IsPackageAssignedToAPickByLabelJob should be true for active job of packed package", true, order.IsPackageAssignedToPickByLabelJob(package));

			pickByLabelJob.WTK_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Should have finalised the pick by label job.", false, pickByLabelJob.WTK_FinalisedDate.IsEmpty);
			Factory.Save();
			AssertEquals("IsPackageAssignedToAPickByLabelJob should be true for closed job", true, order.IsPackageAssignedToPickByLabelJob(package));

			pickByLabelJob.Labels[0].Delete();
			Factory.Save();
			AssertEquals("IsPackageAssignedToAPickByLabelJob should be false for deleted joblabel, label of packed package.", false, order.IsPackageAssignedToPickByLabelJob(package));
		}

		#endregion

		#region TestGetActivePickByLabelByPackage

		public void TestGetActivePickByLabelByPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK, order.Pick.DockDoorLocation.PK, "BP", package.PK);
			Factory.Save();

			AssertEquals("Should return active label.", pickByLabelJob.Labels.Single(), WhsPickByLabelHelper.GetActivePickByLabelByPackage(package));
		}

		#endregion

		#region TestCancelPickByLabel

		public void TestCancelPickByLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var package = Factory.New<PkgPackage>();

			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_WW_Warehouse = data.Whs1.PK;
			pickByLabelJob.WTK_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			pickByLabelJob.WTK_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Precondition: job has no labels.", false, pickByLabelJob.Labels.Count > 0);

			var label = WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package.PK);
			AssertEquals("Precondition: job has label.", true, pickByLabelJob.Labels.Count > 0);

			WhsPickByLabelHelper.CancelPickByLabel(label);
			AssertEquals("Label is deleted.", true, label.IsDeleted);
			AssertEquals("PickByLabel job is also deleted.", true, pickByLabelJob.IsDeleted);
		}

		public void TestCancelPickByLabel_MultipleLabelsOnJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();

			var pickByLabelJob = Factory.New<WhsPickByLabelJob>();
			pickByLabelJob.WTK_WW_Warehouse = data.Whs1.PK;
			pickByLabelJob.WTK_WL_DockDoor = data.Whs1.WW_DefaultOutboundDockDoor;
			pickByLabelJob.WTK_GS_NKAssignedTo = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Precondition: job has no labels.", false, pickByLabelJob.Labels.Count > 0);

			var label1 = WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);
			AssertEquals("Precondition: job has more than 1 label.", 2, pickByLabelJob.Labels.Count);

			WhsPickByLabelHelper.CancelPickByLabel(label1);
			AssertEquals("Label is deleted.", true, label1.IsDeleted);
			AssertEquals("PickByLabel job is not deleted.", false, pickByLabelJob.IsDeleted);
		}

		public void TestCancelPickByLabel_NullLabel()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception thrown for null label.", () => WhsPickByLabelHelper.CancelPickByLabel(null));
		}

		#endregion
	}
}

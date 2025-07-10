using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class CancelPickByLabelTest : WhsSecureServiceTestCase
	{
		#region TestCancelPickByLabel

		public void TestCancelPickByLabel()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			AssertEquals(true, pickLine1.WZ_IsPicking);
			AssertEquals(true, pickLine2.WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Should have unassigned picker.", "", pickLine1.WZ_GS_NKAssignedTo);
				AssertEquals("Should have unassigned picker.", "", pickLine2.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking should be false.", false, pickLine1.WZ_IsPicking);
				AssertEquals("Is picking should be false.", false, pickLine2.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine1.HasChanges);
				AssertEquals("Should be saved.", false, pickLine2.HasChanges);

				AssertEquals("WhsPickByLabelLabel is deleted.", true, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is deleted.", true, pickByLabelJob.IsDeleted);
			});
		}

		public void TestCancelPickByLabel_DockDoorAssignment()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_PickPalletsByLabel = true;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_PickPalletsByLabel = true;

			var pickLine1 = pick1.GetAllPickLines().Single();
			pickLine1.WZ_GS_NKAssignedTo = "A";

			var pickLine2 = pick2.GetAllPickLines().Single();
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var package2 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package2.PK);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1, pick2);
			Helper.Factory.Save();

			// ensure jobs label and task were created
			var pickByLabelJobTest = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var labs = pickByLabelJobTest.Labels.Select(l => l.Package.KP_PackageID).OrderBy(g => g).ToArray();
			AssertEquals("PACKAGE1", labs[0]);
			AssertEquals("PACKAGE2", labs[1]);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.CancelPickByLabel("PACKAGE1");
			AssertSuccessfulResponse(response1, webService1);

			CombineAssertions(() =>
			{
				AssertEquals("Should have unassigned picker.", "", pickLine1.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking should be false.", false, pickLine1.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine1.HasChanges);

				var dockDoorAssignments = webService1.Factory.Load<WhsDockDoorAssignment>(new ZQuery());
				AssertEquals("DDA exists", 1, dockDoorAssignments.Length);

				AssertEquals("Pick1 DDA cleared", ZGuid.Empty, pick1.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick1 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick1.WP_WL_DockDoor);

				AssertEquals("Pick2 DDA not cleared", dda.PK, pick2.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick2 DDL not set", ZGuid.Empty, pick2.WP_WL_DockDoor);
			});

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.CancelPickByLabel("PACKAGE2");
			AssertSuccessfulResponse(response2, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Should have unassigned picker.", "", pickLine2.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking should be false.", false, pickLine2.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine2.HasChanges);

				var dockDoorAssignments = webService1.Factory.Load<WhsDockDoorAssignment>(new ZQuery());
				AssertEquals("No DDA exists", 0, dockDoorAssignments.Length);

				AssertEquals("Pick1 DDA cleared", ZGuid.Empty, pick1.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick1 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick1.WP_WL_DockDoor);

				AssertEquals("Pick2 DDA cleared", ZGuid.Empty, pick2.WP_WDA_DockDoorAssignment);
				AssertEquals("Pick2 DDL set", data.Whs1.WW_DefaultOutboundDockDoor, pick2.WP_WL_DockDoor);
			});
		}

		public void TestCancelPickByLabel_DockDoorAssignment_Error()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_PickPalletsByLabel = true;

			var pickLine1 = pick1.GetAllPickLines().Single();
			pickLine1.WZ_GS_NKAssignedTo = "A";

			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE1");
			packingHelper.CreatePackageDivot(package1, pickLine1);

			Helper.Factory.Save();

			var pickByLabelJob = WhsPickByLabelHelper.GetOrCreatePickByLabelJob(Helper.Factory, data.Whs1.PK, staff.GS_Code, data.Whs1.WW_DefaultOutboundDockDoor);
			WhsPickByLabelHelper.GetOrCreatePickByLabelLabel(pickByLabelJob, package1.PK);

			var dda = Helper.CreateWhsDockDoorAssignment(data.Whs1.DefaultOutboundDockDoorLocation, pick1);
			Helper.Factory.Save();

			// ensure jobs label and task were created
			var pickByLabelJobTest = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var labs = pickByLabelJobTest.Labels.Select(l => l.Package.KP_PackageID).OrderBy(g => g).ToArray();
			AssertEquals("PACKAGE1", labs[0]);

			var ddaService = new Mock<IWhsPickDockDoorAssignmentService>();
			ddaService.Setup(m =>
				m.RemovePickDockDoorAssignment(
					It.IsAny<ZGuid>(),
					It.IsAny<BusinessObjectFactory>()))
				.Returns("Error");

			using (ObjectFactory.Substitute(ddaService.Object))
			{
				var webService1 = GetNewWebService(data.Whs1, staff);
				var response1 = webService1.CancelPickByLabel("PACKAGE1");
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Error", response1.ErrorMessage);
			}
		}

		#endregion

		#region TestCancelPickByLabel_SetIsPicking

		public void TestCancelPickByLabel_SetIsPicking()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			AssertEquals(true, pickLine.WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Should have unassigned picker.", "", pickLine.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking should be false.", false, pickLine.WZ_IsPicking);
				AssertEquals("Ensure saved.", false, pickLine.HasChanges);

				AssertEquals("WhsPickByLabelLabel is deleted.", true, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is deleted.", true, pickByLabelJob.IsDeleted);
			});
		}

		#endregion

		#region TestCancelPickByLabel_PickedLabel

		public void TestCancelPickByLabel_PickedLabel()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Today;
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition: pick line1 is picked.", true, pickLine1.IsPicked);
			AssertEquals("Precondition: pick line2 is picked.", true, pickLine2.IsPicked);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Label 'PACKAGE-1' is already picked and cannot be canceled.", response.ErrorMessage);
				AssertEquals("Label cannot be cancelled as it's already picked.", ErrorTypes.BusinessValidationError, response.Error);

				AssertEquals("WhsPickByLabelLabel is not deleted.", false, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		public void TestCancelPickByLabel_PartiallyPickedLabel()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			AssertEquals("Precondition: pick line is picking.", true, pickLine1.WZ_IsPicking);

			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Today;
			AssertEquals("Precondition: pick line is picked.", true, pickLine2.IsPicked);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Label 'PACKAGE-1' is already picked and cannot be canceled.", response.ErrorMessage);
				AssertEquals("Label cannot be cancelled as it's already picked.", ErrorTypes.BusinessValidationError, response.Error);

				AssertEquals("WhsPickByLabelLabel is not deleted.", false, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		public void TestCancelPickByLabel_PickedAndAlreadyPutaway()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);
			AssertEquals(true, pickLine.WZ_IsPicking);

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			AssertEquals("Should be finalised.", true, transferLine.IsFinalised);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Label 'PACKAGE-1' is already picked and putaway.", response.ErrorMessage);
				AssertEquals("Label 'PACKAGE-1' is already picked and putaway.", ErrorTypes.BusinessValidationError, response.Error);

				AssertEquals("WhsPickByLabelLabel is not deleted.", false, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		#endregion

		#region TestCancelPickByLabel_MultipleLabelsOnJob

		public void TestCancelPickByLabel_MultipleLabelsOnJob()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package1, pickLine1);
			var package2 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-2");
			packingHelper.CreatePackageDivot(package2, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);
			webService.GetPickByLabelPackage("PACKAGE-2", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			AssertEquals("Pick by label job has 2 labels.", 2, pickByLabelJob.Labels.Count);
			var label1 = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-1");
			var label2 = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single(label => label.Package.KP_PackageID == "PACKAGE-2");

			AssertEquals("Precondition: pick line1 is picking.", true, pickLine1.WZ_IsPicking);
			AssertEquals("Precondition: pick line2 is picking.", true, pickLine2.WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Only 1 label is left on the job and it's 'PACKAGE-2'.", "PACKAGE-2", pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>().Single().Package.KP_PackageID);
				AssertEquals("Should have unassigned picker.", "", pickLine1.WZ_GS_NKAssignedTo);
				AssertEquals("Should have not unassigned picker.", "A", pickLine2.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking is reverted to not picking.", false, pickLine1.WZ_IsPicking);
				AssertEquals("Is picking is not reverted to not picking.", true, pickLine2.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine1.HasChanges);
				AssertEquals("Should be saved.", false, pickLine2.HasChanges);

				AssertEquals("WhsPickByLabelLabel1 is deleted.", true, label1.IsDeleted);
				AssertEquals("WhsPickByLabelLabel2 is not deleted.", false, label2.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		#endregion

		#region TestCancelPickByLabel_AssignedToAnotherUser

		public void TestCancelPickByLabel_AssignedToAnotherUser()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff1 = Helper.CreateGlbStaff("A", "A");
			var staff2 = Helper.CreateGlbStaff("B", "B");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff1.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			AssertEquals(true, pickLine1.WZ_IsPicking);
			AssertEquals(true, pickLine2.WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff2);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Label 'PACKAGE-1' cannot be found or assigned to someone else.", response.ErrorMessage);
				AssertEquals("Label cannot be cancelled as it's assigned to another user.", ErrorTypes.BusinessValidationError, response.Error);

				AssertEquals("Should have not unassigned picker.", "A", pickLine1.WZ_GS_NKAssignedTo);
				AssertEquals("Should have not unassigned picker.", "A", pickLine2.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking is not reverted to not picking.", true, pickLine1.WZ_IsPicking);
				AssertEquals("Is picking is not reverted to not picking.", true, pickLine2.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine1.HasChanges);
				AssertEquals("Should be saved.", false, pickLine2.HasChanges);

				AssertEquals("WhsPickByLabelLabel is not deleted.", false, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		#endregion

		#region TestCancelPickByLabel_LabelDoesNotExist

		public void TestCancelPickByLabel_PackageDoesNotExist()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff1 = Helper.CreateGlbStaff("A", "A");
			var staff2 = Helper.CreateGlbStaff("B", "B");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff1.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			AssertEquals(true, pickLine1.WZ_IsPicking);
			AssertEquals(true, pickLine2.WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff2);
			var response = webService2.CancelPickByLabel("PACKAGE-2");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Label 'PACKAGE-2' cannot be found or assigned to someone else.", response.ErrorMessage);
				AssertEquals("Label cannot be cancelled as it does not exist.", ErrorTypes.BusinessValidationError, response.Error);

				AssertEquals("Should have not unassigned picker.", "A", pickLine1.WZ_GS_NKAssignedTo);
				AssertEquals("Should have not unassigned picker.", "A", pickLine2.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking is not reverted to not picking.", true, pickLine1.WZ_IsPicking);
				AssertEquals("Is picking is not reverted to not picking.", true, pickLine2.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine1.HasChanges);
				AssertEquals("Should be saved.", false, pickLine2.HasChanges);

				AssertEquals("WhsPickByLabelLabel is not deleted.", false, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		public void TestCancelPickByLabel_LabelDoesNotExist()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals("Pick by Label task for label 'PACKAGE-1' does not exist.", response.ErrorMessage);
				AssertEquals("Label 'PACKAGE-1' does not exist.", ErrorTypes.BusinessValidationError, response.Error);
			});
		}

		#endregion

		#region TestCancelPickByLabel_FinalisedJob

		public void TestCancelPickByLabel_FinalisedJob()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			pickByLabelJob.WTK_FinalisedDate = ZDateTimeOffset.Today;
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response = webService2.CancelPickByLabel("PACKAGE-1");
			AssertSuccessfulResponse(response, webService2);

			CombineAssertions(() =>
			{
				AssertEquals("Label 'PACKAGE-1' cannot be canceled as the job is already finalized.", response.ErrorMessage);
				AssertEquals("Job is already finalised.", ErrorTypes.BusinessValidationError, response.Error);
			});
		}

		#endregion

		#region TestCancelPickByLabel_SaveException

		public void TestCancelPickByLabel_HandleZCannotSaveException()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			AssertEquals(true, pickLine1.WZ_IsPicking);
			AssertEquals(true, pickLine2.WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff);
			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService2.Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			}
			webService2.Factory.Saving += action;

			var response2 = webService2.CancelPickByLabel("PACKAGE-1");
			AssertEquals("Should Error as validation prevents save.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Test - Cannot Save", response2.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

			CombineAssertions(() =>
			{
				AssertEquals("Should have not unassigned picker.", "A", pickLine1.WZ_GS_NKAssignedTo);
				AssertEquals("Should have not unassigned picker.", "A", pickLine2.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking should not be reverted to false.", true, pickLine1.WZ_IsPicking);
				AssertEquals("Is picking should not be reverted to false.", true, pickLine2.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine1.HasChanges);
				AssertEquals("Should be saved.", false, pickLine2.HasChanges);

				AssertEquals("WhsPickByLabelLabel is not deleted.", false, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		public void TestCancelPickByLabel_HandleZSaveException()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var staff = Helper.CreateGlbStaff("A", "A");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var pickLines = pick.GetAllPickLines().ToArray();
			var pickLine1 = pickLines[0];
			var pickLine2 = pickLines[1];
			pickLine1.WZ_GS_NKAssignedTo = "A";
			pickLine2.WZ_GS_NKAssignedTo = "A";

			var package = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, "PACKAGE-1");
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			// start the pick, creating the task
			webService.GetPickByLabelPackage("PACKAGE-1", false);

			// ensure jobs label and task were created
			var pickByLabelJob = Helper.Factory.Load<WhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_GS_NKAssignedTo, staff.GS_Code)).Single();
			var label = (WhsPickByLabelLabel)pickByLabelJob.Labels.Single();
			AssertEquals("Precondition: Label is created for the correct package", "PACKAGE-1", label.Package.KP_PackageID);

			AssertEquals(true, pickLine1.WZ_IsPicking);
			AssertEquals(true, pickLine2.WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff);
			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService2.Factory.Saving -= action;
				var row = ((INeedRow)label).Row;
				throw new ZSaveException(new DummyDataException(row, TestConnection), label.Factory);
			}
			webService2.Factory.Saving += action;

			var response2 = webService2.CancelPickByLabel("PACKAGE-1");
			AssertEquals("Should Error as validation prevents save.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Blah", response2.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

			CombineAssertions(() =>
			{
				AssertEquals("Should have not unassigned picker.", "A", pickLine1.WZ_GS_NKAssignedTo);
				AssertEquals("Should have not unassigned picker.", "A", pickLine2.WZ_GS_NKAssignedTo);
				AssertEquals("Is picking should not be reverted to false.", true, pickLine1.WZ_IsPicking);
				AssertEquals("Is picking should not be reverted to false.", true, pickLine2.WZ_IsPicking);
				AssertEquals("Should be saved.", false, pickLine1.HasChanges);
				AssertEquals("Should be saved.", false, pickLine2.HasChanges);

				AssertEquals("WhsPickByLabelLabel is not deleted.", false, label.IsDeleted);
				AssertEquals("WhsPickByLabelJob is not deleted.", false, pickByLabelJob.IsDeleted);
			});
		}

		#endregion
	}
}

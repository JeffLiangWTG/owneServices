using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferController))]
	class TransferControllerBasherTest : WhsControllerBaseBasherTest
	{
		#region TestShowFormCreatesInventoryFilterStrip

		public void TestShowFormCreatesInventoryFilterStrip()
		{
			var docket = (WhsDocket)GetBusinessObjectThatIsInTheDatabase();
			using (var form = (TransferEntryForm)Controller.ShowViewForm(docket))
			{
				AssertNull("filterstrip should not show for view form", form.inventoryFilterStripUserControl);
			}

			using (var form = (TransferEntryForm)Controller.ShowDeleteForm(docket))
			{
				AssertNull("filterstrip should not show for delete form", form.inventoryFilterStripUserControl);
			}

			using (var form = (TransferEntryForm)Controller.ShowNewForm())
			{
				AssertNotNull("filterstrip should show for new dockets", form.inventoryFilterStripUserControl);
			}

			using (var form = (TransferEntryForm)Controller.ShowEditForm(docket))
			{
				AssertNotNull("filterstrip should show for edit dockets that are not finalised", form.inventoryFilterStripUserControl);
			}

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);
			using (var form = (TransferEntryForm)Controller.ShowEditForm(docket))
			{
				AssertNull("filterstrip should not show for edit dockets that are finalised", form.inventoryFilterStripUserControl);
			}

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_IsPutawayTransfer = true;
			AssertEquals("Precondition", false, docket.IsFinalised);
			AssertEquals("Precondition", true, docket.WD_IsPutawayTransfer);
			using (var form = (TransferEntryForm)Controller.ShowEditForm(docket))
			{
				AssertNull("filterstrip should not show for Putaway Transfers.", form.inventoryFilterStripUserControl);
			}
		}

		#endregion

		#region TestShowNewFormForUserNotHavingPermissions

		public void TestShowNewFormForUserNotHavingPermissions()
		{
			// setup test user and deny create new transfer permissions
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";

			var createNewTransferCheckPoint = Env.Security.WhsTransferNew;
			var securityRecord = staff.GroupSecurityPermissionsCollectionForBinding.AddNew();
			securityRecord.GU_SecurityRight = createNewTransferCheckPoint.Code;
			securityRecord.GU_ItemGUID = createNewTransferCheckPoint.ItemGuid;
			securityRecord.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			// test user without security rights to create new transfers
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var controller = new TransferController();
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
				{
					controller.ShowNewForm();
					AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\n" +
						"If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n" +
						"Operate -> Warehouse -> Transfers -> New", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		#endregion

		#region TestShowEditFormForUserNotHavingPermissions

		public void TestShowEditFormForUserNotHavingPermissions()
		{
			//Setup test user and deny edit transfer permissions
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";

			var editTransferCheckPoint = Env.Security.WhsTransferEdit;
			var securityRecord = staff.GroupSecurityPermissionsCollectionForBinding.AddNew();
			securityRecord.GU_SecurityRight = editTransferCheckPoint.Code;
			securityRecord.GU_ItemGUID = editTransferCheckPoint.ItemGuid;
			securityRecord.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			// test user without security rights to edit transfers
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var docket = GetBusinessObjectThatIsInTheDatabase();
				var controller = new TransferController();
				AssertNoExceptionThrown(() => controller.ShowEditForm(docket));
				AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\n" +
					"If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n" +
					"Operate -> Product Warehouse -> Transfers -> View", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsTransfer;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var transfer = helper.CreateWhsTransfer(helper.CreateClient().PK, helper.CreateWarehouse("WHS1").PK);
			Factory.Save();
			return transfer;
		}

		#endregion
	}
}

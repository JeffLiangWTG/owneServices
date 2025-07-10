using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AdjustmentController))]
	class AdjustmentControllerBasherTest : WhsControllerBaseBasherTest
	{
		public void TestShowFormCreatesInventoryFilterStrip()
		{
			var docket = (WhsDocket)GetBusinessObjectThatIsInTheDatabase();
			using (var form = (AdjustmentEntryForm)Controller.ShowViewForm(docket))
			{
				AssertNull("filterstrip should not show for view form", form.inventoryFilterStripUserControl);
			}

			using (var form = (AdjustmentEntryForm)Controller.ShowDeleteForm(docket))
			{
				AssertNull("filterstrip should not show for delete form", form.inventoryFilterStripUserControl);
			}

			using (var form = (AdjustmentEntryForm)Controller.ShowNewForm())
			{
				AssertNotNull("filterstrip should show for new dockets", form.inventoryFilterStripUserControl);
			}

			using (var form = (AdjustmentEntryForm)Controller.ShowEditForm(docket))
			{
				AssertNotNull("filterstrip should show for edit dockets that are not finalised", form.inventoryFilterStripUserControl);
			}

			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(docket);
			using (var form = (AdjustmentEntryForm)Controller.ShowEditForm(docket))
			{
				AssertNull("filterstrip should not show for edit dockets that are finalised", form.inventoryFilterStripUserControl);
			}
		}

		public void TestShowNewFormForUserNotHavingPermissions()
		{
			// setup test user and deny create new adjustment permissions
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";

			var createNewAdjustmentCheckPoint = Env.Security.WhsAdjustmentNew;
			var securityRecord = staff.GroupSecurityPermissionsCollectionForBinding.AddNew();
			securityRecord.GU_SecurityRight = createNewAdjustmentCheckPoint.Code;
			securityRecord.GU_ItemGUID = createNewAdjustmentCheckPoint.ItemGuid;
			securityRecord.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			// test user without security rights to create new adjustments
			using (Env.SetTemporaryUserContext(staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var controller = new AdjustmentController();
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
				{
					controller.ShowNewForm();
					AssertEquals("You do not have the appropriate security rights to run this function.\r\n\r\n" +
						"If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n" +
						"Operate -> Warehouse -> Adjustments -> New", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			var controllerForNewAdjustment = new AdjustmentControllerForTest(AdjustmentType.Codes.Adjustment);
			var newAdjustment = (WhsAdjustment)controllerForNewAdjustment.GetNewBusinessEntityInLocalFactoryForTest();
			AssertNull("Child adjustment should not be created.", newAdjustment.ChildAdjustment);
			AssertEquals(AdjustmentType.Codes.Adjustment, newAdjustment.WD_DocketSubType);
			AssertNoErrors("There should not be any errors on client when we first generate the BizO", newAdjustment.WD_OH_ClientInfo);
			AssertNoErrors("There should not be any errors on warehouse when we first generate the BizO", newAdjustment.WD_WW_WhsInfo);

			var controllerForNewOwnershipChangeAdjustment = new AdjustmentControllerForTest(AdjustmentType.Codes.OwnershipAdjustment);
			var newOwnershipChangeAdjustment = (WhsAdjustment)controllerForNewOwnershipChangeAdjustment.GetNewBusinessEntityInLocalFactoryForTest();
			AssertEquals(AdjustmentType.Codes.OwnershipAdjustment, newOwnershipChangeAdjustment.WD_DocketSubType);
			AssertNotNull("Child adjustment should be created.", newOwnershipChangeAdjustment.ChildAdjustment);
			AssertNoErrors("There should not be any errors on client when we first generate the BizO", newOwnershipChangeAdjustment.WD_OH_ClientInfo);
			AssertNoErrors("There should not be any errors on warehouse when we first generate the BizO", newOwnershipChangeAdjustment.WD_WW_WhsInfo);
		}

		class AdjustmentControllerForTest : AdjustmentController
		{
			public AdjustmentControllerForTest(string adjustmentType)
				: base(adjustmentType)
			{ }

			public IBusiness GetNewBusinessEntityInLocalFactoryForTest()
			{
				return base.GetNewBusinessEntityInLocalFactory();
			}
		}

		public void TestShowEditFormForChildAdjustmentsNotInDB()
		{
			var controller = new AdjustmentController();
			AssertNoExceptionThrown(() => controller.ShowEditForm(Factory.New<WhsAdjustment>()));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsAdjustment;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var adjustment = helper.CreateWhsAdjustment(helper.CreateClient().PK, helper.CreateWarehouse("WHS1").PK);
			Factory.Save();
			return adjustment;
		}
	}
}

using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffController))]
	sealed class GlbStaffControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var staff = Factory.New(typeof(GlbStaff));
			Factory.Save();
			return staff;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbStaff;
		}

		public void TestLocalAdministratorStaffMemberRights()
		{
			var controller = new GlbStaffController();
			AssertEquals(Env.Security.StaffEdit, controller.GetCheckPointForEdit(null));

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			Assert(staff.IsCurrentUserLocalAdminForThisStaff);
			AssertEquals(Env.Security.StaffLocalAdministratorPlaceholder, controller.GetCheckPointForEdit(staff));
		}

		#region Staff Permissions

		GlbStaff GetStaffWithoutPermissions()
		{
			GlbStaff staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPermissions.GS_IsController = false;

			foreach (SecurityCheckpoint checkPoint in Env.Security.AllLoadedCheckPoints)
			{
				GlbSecurity securityRecord = Factory.New<GlbSecurity>();
				securityRecord.GU_SecurityRight = checkPoint.Code;
				securityRecord.GU_ItemGUID = checkPoint.ItemGuid;
				securityRecord.GU_SecurityItemIsAllowed = false;
				securityRecord.GU_GS = staffWithoutPermissions.PK;
				staffWithoutPermissions.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);
			}

			return staffWithoutPermissions;
		}

		GlbStaff GetStaffWithPermissions()
		{
			GlbStaff staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPermissions.GS_IsController = true;

			return staffWithPermissions;
		}

		#endregion

		[StressTest()]
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestPermissions()
		{
			var initialUserContext = Env.CurrentUserContext;
			ZBool previousCachingSetting = Env.Security.CachingEnabled;

			GlbStaffController controller = new GlbStaffController();

			try
			{
				Env.Security.CachingEnabled = false;

				GlbStaff staffWithPermissions = GetStaffWithPermissions();
				GlbStaff staffWithoutPermissions = GetStaffWithoutPermissions();
				var localAdminForGroup = Factory.NewWithValidTestData<GlbStaff>();
				var group = Factory.NewWithValidTestData<GlbGroup>();

				localAdminForGroup.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);

				GlbStaff viewStaff1 = Factory.NewWithValidTestData<GlbStaff>();
				GlbStaff viewStaff2 = Factory.NewWithValidTestData<GlbStaff>();
				GlbStaff viewStaff3 = Factory.NewWithValidTestData<GlbStaff>();

				Factory.Save();

				Env.SetUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				AssertEquals("Precondition: Current user has changed", staffWithoutPermissions.PK, Env.CurrentUser.PK);

				controller.ShowEditForm(staffWithoutPermissions);
				AssertNull("Staff member with no permissions should not be able to edit their own record", controller.LastShownForm);

				controller.ShowViewForm(staffWithoutPermissions);
				AssertNull("Staff member with no permissions should not be able to view their own record", controller.LastShownForm);

				controller.ShowDeleteForm(staffWithoutPermissions);
				AssertNull("Staff member with no permissions should not be able to delete their own record", controller.LastShownForm);

				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () => controller.ShowNewForm());
				AssertNull("Staff member with no permissions should not be able create a new form", controller.LastShownForm);

				Env.SetUserContext(new UserContext(staffWithPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				controller.ShowDeleteForm(staffWithPermissions);
				AssertNull("Staff member has all permissions, but should not be permitted to delete own form", controller.LastShownForm);

				controller.ShowEditForm(viewStaff1);
				AssertEquals("Staff member has all permissions, Form should be displayed for Browse", ZArchitecture.Core.ODisplayMode.Browse, ((GlbStaffForm)controller.LastShownForm).DisplayMode);
				((GlbStaffForm)controller.LastShownForm).Close();

				controller.ShowViewForm(viewStaff2);
				AssertEquals("Staff member has all permissions, Form should be displayed for Readonly", ZArchitecture.Core.ODisplayMode.ReadOnly, ((GlbStaffForm)controller.LastShownForm).DisplayMode);
				((GlbStaffForm)controller.LastShownForm).Close();

				controller.ShowDeleteForm(viewStaff3);
				AssertEquals("Staff member has all permissions, Form should be displayed for Delete", ZArchitecture.Core.ODisplayMode.Delete, ((GlbStaffForm)controller.LastShownForm).DisplayMode);
				((GlbStaffForm)controller.LastShownForm).Close();

				controller.ShowNewForm();
				AssertEquals("Staff member with all permissions should be able create a new form", ZArchitecture.Core.ODisplayMode.New, ((GlbStaffForm)controller.LastShownForm).DisplayMode);
				((GlbStaffForm)controller.LastShownForm).Close();

				Env.SetUserContext(new UserContext(localAdminForGroup.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				controller.ShowEditForm(staffWithoutPermissions);
				AssertNull("Local admin with no permissions should not be able to edit their own record", controller.LastShownForm);

				controller.ShowViewForm(staffWithoutPermissions);
				AssertNull("Local admin with no permissions should not be able to view their own record", controller.LastShownForm);

				controller.ShowDeleteForm(staffWithoutPermissions);
				AssertNull("Local admin with no permissions should not be able to delete their own record", controller.LastShownForm);

				controller.ShowNewForm();
				AssertEquals("Local admin with no permissions should be able to create a new form", ZArchitecture.Core.ODisplayMode.New, ((GlbStaffForm)controller.LastShownForm).DisplayMode); // Local admin for should be able to create new staff, but he must assign it to the group where he is admin, otherwise he will not be able to save it.
				((GlbStaffForm)controller.LastShownForm).Close();
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					((GlbStaffForm)controller.LastShownForm).Close();
				}

				Env.SetUserContext(initialUserContext);
				Env.Security.CachingEnabled = previousCachingSetting;
			}
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new GlbStaffController();
			AssertEquals("For New", Env.Security.StaffModifyAll, controller.TestCheckPointForNewInternal());
			AssertEquals("For View", Env.Security.StaffView, controller.TestCheckPointForViewInternal());
			AssertEquals("For Edit", Env.Security.StaffEdit, controller.GetCheckPointForEdit(null));
		}

		#region TestGetCheckPointForDelete

		public void TestGetCheckPointForDelete()
		{
			GlbStaffController controller = new GlbStaffController();

			AssertEquals(Env.Security.StaffModifyAll, controller.GetCheckPointForDelete(null));

			SecurityCheckpoint checkpoint = controller.GetCheckPointForDelete(GlbStaff.CurrentUser);
			Assert(!checkpoint.IsAllowed);
			AssertEquals("You cannot delete a user currently logged in.", checkpoint.DisplayText);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaffForDeleteCheckpointTest>();
			Factory.Save();
			checkpoint = controller.GetCheckPointForDelete(staff);
			Assert(!checkpoint.IsAllowed);
			AssertEquals("Just because.", checkpoint.DisplayText);

			staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			checkpoint = controller.GetCheckPointForDelete(staff);
			Assert(checkpoint.IsAllowed);
		}

		class GlbStaffForDeleteCheckpointTest : GlbStaff
		{
			public GlbStaffForDeleteCheckpointTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override bool CanDelete
			{
				get { return false; }
			}

			public override MultilingualString ReasonForNotAbleToDelete
			{
				get { return (NoResString)"Just because."; }
			}
		}

		#endregion
	}
}

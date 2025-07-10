using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbGroupController))]
	public class GlbGroupControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new GlbGroupControllerForTest();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var databaseDeveloperGroup = Factory.Load<GlbGroup>(GlbGroup.DbDeveloperGroupPK);
			var databaseReaderGroup = Factory.Load<GlbGroup>(GlbGroup.DbReaderGroupPK);
			var backupOperatorGroup = Factory.Load<GlbGroup>(GlbGroup.BackupOperatorGroupPK);

			var hrmstaffGroup = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			hrmstaffGroup.GG_Code = "HRM_Test_1";
			var hrmstaffGroupRole = Factory.New<GlbGroupRole>();
			hrmstaffGroupRole.GGR_RoleName = "cwHRMStaffRole";
			hrmstaffGroup.Roles.Add(hrmstaffGroupRole);

			var groupOwner = Factory.NewWithValidTestData<GlbStaff>();
			var nonGroupOwner = Factory.NewWithValidTestData<GlbStaff>();

			groupOwner.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			groupOwner.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);
			groupOwner.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(databaseDeveloperGroup.GG_Code);
			groupOwner.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(databaseReaderGroup.GG_Code);
			groupOwner.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(backupOperatorGroup.GG_Code);
			groupOwner.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(hrmstaffGroup.GG_Code);
			groupOwner.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(groupOwner.GS_LoginName))
			{
				AssertEquals("For Edit", Env.Security.FindOrCreateGroupOwnerSecurityCheckpoint(group.PK.ToGuid()), controller.GetCheckPointForEdit(group));
				AssertEquals("For Edit", false, controller.GetCheckPointForEdit(databaseDeveloperGroup).IsAllowed);
				AssertEquals("For Edit", false, controller.GetCheckPointForEdit(databaseReaderGroup).IsAllowed);
				AssertEquals("For Edit", false, controller.GetCheckPointForEdit(backupOperatorGroup).IsAllowed);
				AssertEquals("For Edit", Env.Security.FindOrCreateGroupOwnerSecurityCheckpoint(hrmstaffGroup.PK.ToGuid()), controller.GetCheckPointForEdit(hrmstaffGroup));
			}

			using (CurrentUserChanger.SwitchToNewUserTemporarily(nonGroupOwner.GS_LoginName))
			{
				AssertEquals("For Edit", Env.Security.GroupsModify, controller.GetCheckPointForEdit(group));
				AssertEquals("For Edit", false, controller.GetCheckPointForEdit(databaseDeveloperGroup).IsAllowed);
				AssertEquals("For Edit", false, controller.GetCheckPointForEdit(databaseReaderGroup).IsAllowed);
				AssertEquals("For Edit", false, controller.GetCheckPointForEdit(backupOperatorGroup).IsAllowed);
				AssertEquals("For Edit", Env.Security.GroupsModify, controller.GetCheckPointForEdit(hrmstaffGroup));
			}

			AssertEquals("For Delete", Env.Security.GroupsModify, controller.CheckPointForDelete);
			AssertEquals("For New", Env.Security.GroupsModify, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.GroupsView, controller.CheckPointForView);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GlbGroup;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var group = Factory.New<GlbGroup>();
			Factory.Save();
			return group;
		}

		class GlbGroupControllerForTest : GlbGroupController
		{
			internal new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;

			internal new SecurityCheckpoint CheckPointForView => base.CheckPointForView;

			internal new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
		}
	}
}

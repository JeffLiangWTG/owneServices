using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupManyToManyCollection))]
	sealed class GlbGroupManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestScimReadOnly()
		{
			var staff1 = Factory.New<GlbStaff>();
			var collection1 = new GlbGroupManyToManyCollection(staff1);

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_ExternalId = "123";
			var collection2 = new GlbGroupManyToManyCollection(staff2);

			AssertEquals(false, collection1.ReadOnly);
			AssertEquals(true, collection2.ReadOnly);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			return new GlbGroupManyToManyCollection(staff);
		}

		public void TestRemoveAllGroup()
		{
			var otherGroup = Factory.New<GlbGroup>();
			var staff = Factory.New<GlbStaff>();
			staff.Groups.Add(otherGroup);
			AssertEquals(2, staff.Groups.Count);

			staff.Groups.RemoveAll();
			AssertEquals(1, staff.Groups.Count);

			staff.GS_IsActive = false;

			staff.Groups.RemoveAll();
			AssertEquals(0, staff.Groups.Count);
		}

		public void TestRemoveFixedDatabaseAccessGroup()
		{
			var databaseDeveloperGroup = Factory.Load<GlbGroup>(GlbGroup.DbDeveloperGroupPK);
			var databaseReaderGroup = Factory.Load<GlbGroup>(GlbGroup.DbReaderGroupPK);
			var backupOperatorGroup = Factory.Load<GlbGroup>(GlbGroup.BackupOperatorGroupPK);
			var staff = Factory.New<GlbStaff>();
			staff.Groups.Add(databaseDeveloperGroup);
			staff.Groups.Add(databaseReaderGroup);
			staff.Groups.Add(backupOperatorGroup);

			staff.Groups.RemoveAll();

			AssertEquals(4, staff.Groups.Count);

			staff.GS_IsActive = false;

			staff.Groups.RemoveAll();
			AssertEquals(0, staff.Groups.Count);
		}

		public void TestRemoveFlexibleDatabaseAccessGroup()
		{
			var hrmstaffGroup = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			var hrmstaffGroupRole = Factory.New<GlbGroupRole>();
			hrmstaffGroupRole.GGR_RoleName = "cwHRMStaffRole";
			hrmstaffGroup.Roles.Add(hrmstaffGroupRole);

			var staff = Factory.New<GlbStaff>();
			staff.Groups.Add(hrmstaffGroup);
			AssertEquals(2, staff.Groups.Count);

			staff.Groups.RemoveAll();
			AssertEquals(1, staff.Groups.Count);
		}

		public void TestRemoveScimMappedGroup()
		{
			var staff = Factory.New<GlbStaff>();

			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var collection = new StaffColumnToGroupDescriptionScimMappingCollection();
			int index = 1;
			foreach (CodeDescriptionPair item in new StaffColumnToGroupDescriptionScimMappingLookups(null).StaffList)
			{
				var desc = (index++).ToString();
				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Desc = desc;
				staff.Groups.Add(group);

				collection.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = item.Code, GroupDescriptionMapping = desc });
			}

			Factory.Save();

			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			staff.Groups.RemoveAll();

			AssertEquals(index, staff.Groups.Count);

			staff.GS_IsActive = false;

			staff.Groups.RemoveAll();
			AssertEquals(0, staff.Groups.Count);
		}

		public void TestAddAndRemoveGroupWithStaffLink()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			AssertNull("Group has no link to staff, current group link should be null", group.CurrentGroupLink);

			TestCollection.Add(group);
			AssertNotNull("Group has a link to staff, current group link should not be null", group.CurrentGroupLink);

			group.CurrentGroupLink.GK_MembershipType = "PPP";
			TestCollection.Remove(group);
			Assert("Group has no link to staff, current group link should be deleted", group.CurrentGroupLink.IsDeleted);

			TestCollection.Add(group);
			AssertNotNull("Group has a link to staff, current group link should not be null", group.CurrentGroupLink);
			Assert("Group has lost the original membership type of the staff member", group.CurrentGroupLink.GK_MembershipType != "PPP");
		}

		public void TestGroupChangedEvent()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			GlbGroup group = Factory.New<GlbGroup>();
			staff.Groups.Add(group);
			using (StaffFormForTestCollection mockForm = new StaffFormForTestCollection(staff))
			{
				Assert("Event should not have been fired", !mockForm.GroupChangedEventFired);
				staff.Groups.Remove(group);
				Assert("Event should have been fired", mockForm.GroupChangedEventFired);
				mockForm.GroupChangedEventFired = false;
				staff.Groups.Add(group);
				Assert("Event should have been fired", mockForm.GroupChangedEventFired);
			}
		}

		public void TestRemoveGroups_StaffIsActive()
		{
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mappings = new StaffColumnToGroupDescriptionScimMappingCollection();
			mappings.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsController", GroupDescriptionMapping = "is controller" });
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var staff = Factory.New<GlbStaff>();
			var group = Factory.New<GlbGroup>();

			var databaseDeveloperGroup = Factory.Load<GlbGroup>(GlbGroup.DbDeveloperGroupPK);

			var hrmStaffGroup = Factory.NewWithValidTestData<GlbGroup>();
			var hrmStaffGroupRole = Factory.NewWithValidTestData<GlbGroupRole>();
			hrmStaffGroupRole.GGR_GG_Group = hrmStaffGroup.PK;
			hrmStaffGroupRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

			var groupMapped = Factory.New<GlbGroup>();
			groupMapped.GG_Desc = "is controller";

			staff.Groups.Add(group);
			staff.Groups.Add(databaseDeveloperGroup);
			staff.Groups.Add(hrmStaffGroup);
			staff.Groups.Add(groupMapped);

			var collection = staff.Groups;

			using (var mockForm = new StaffFormForTestCollection(staff))
			{
				collection.Remove(group);
				AssertEquals("Group should be removed successfully", 4, staff.Groups.Count);

				Assert("Event should not have been fired", !mockForm.AllUsersEventFired);
				Assert("Event should not have been fired", !mockForm.DatabaseAccessEventFired);
				Assert("Event should not have been fired", !mockForm.SCIMFired);

				collection.Remove(hrmStaffGroup);
				Assert("Event should not have been fired", !mockForm.AllUsersEventFired);
				Assert("Event should not have been fired", !mockForm.DatabaseAccessEventFired);
				Assert("Event should not have been fired", !mockForm.SCIMFired);
				AssertEquals("hrm staff group should be removed successfully", 3, staff.Groups.Count);

				collection.Remove(databaseDeveloperGroup);
				Assert("Event should have been fired", mockForm.DatabaseAccessEventFired);
				Assert("Event should not have been fired", !mockForm.AllUsersEventFired);
				Assert("Event should not have been fired", !mockForm.SCIMFired);
				AssertEquals("Attempt to delete staff from database developer group should be unsuccessful", 3, staff.Groups.Count);

				collection.Remove(groupMapped);
				Assert("Event should have been fired", mockForm.DatabaseAccessEventFired);
				Assert("Event should not have been fired", !mockForm.AllUsersEventFired);
				Assert("Event should have been fired", mockForm.SCIMFired);
				AssertEquals("Attempt to delete staff from database developer group should be unsuccessful", 3, staff.Groups.Count);

				collection.RemoveAll();
				Assert("Event should have been fired", mockForm.DatabaseAccessEventFired);
				Assert("Event should have been fired", mockForm.AllUsersEventFired);
				Assert("Event should have been fired", mockForm.SCIMFired);
				AssertEquals("Attempt to delete staff from All Users group should be unsuccessful", 3, staff.Groups.Count);
			}
		}

		public void TestRemoveGroups_StaffIsNotActive()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = false;

			var group = Factory.New<GlbGroup>();

			var databaseDeveloperGroup = Factory.Load<GlbGroup>(GlbGroup.DbDeveloperGroupPK);

			var hrmStaffGroup = Factory.NewWithValidTestData<GlbGroup>();
			var hrmStaffGroupRole = Factory.NewWithValidTestData<GlbGroupRole>();
			hrmStaffGroupRole.GGR_GG_Group = hrmStaffGroup.PK;
			hrmStaffGroupRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

			staff.Groups.Add(group);
			staff.Groups.Add(databaseDeveloperGroup);
			staff.Groups.Add(hrmStaffGroup);

			var collection = staff.Groups;

			using (var mockForm = new StaffFormForTestCollection(staff))
			{
				collection.Remove(group);
				AssertEquals("Group should be removed successfully", 2, staff.Groups.Count);

				Assert("Event should not have been fired", !mockForm.AllUsersEventFired);
				Assert("Event should not have been fired", !mockForm.DatabaseAccessEventFired);

				collection.Remove(hrmStaffGroup);
				Assert("Event should not have been fired", !mockForm.AllUsersEventFired);
				Assert("Event should not have been fired", !mockForm.DatabaseAccessEventFired);
				AssertEquals("hrm staff group should be removed successfully", 1, staff.Groups.Count);

				collection.Remove(databaseDeveloperGroup);
				Assert("Event should not have been fired", !mockForm.DatabaseAccessEventFired);
				Assert("Event should not have been fired", !mockForm.AllUsersEventFired);
				AssertEquals("database developer group should be removed successfully", 0, staff.Groups.Count);
			}
		}

		public void TestContainsAnyCode()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			bool inAllStaffGroup = staff.Groups.ContainsAnyCode(new string[] { GlbGroup.AllStaffGroupCode });
			AssertEquals("Current login in the 'all staff' group", true, inAllStaffGroup);

			bool inSplatGroup = staff.Groups.ContainsAnyCode(new string[] { "splat" });
			AssertEquals("Current login in NOT in the a nonexistant group", false, inSplatGroup);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, TestCollection.AllowNew);
		}

		GlbGroupManyToManyCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			GlbStaff testStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			TestCollection = new GlbGroupManyToManyCollection(testStaff);
		}
	}
}

using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffManyToManyCollection))]
	sealed class GlbStaffManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestScimReadOnly()
		{
			var group1 = Factory.New<GlbGroup>();
			var collection1 = new GlbStaffManyToManyCollection(group1);

			var group2 = Factory.New<GlbGroup>();
			group2.GG_ExternalId = "123";
			var collection2 = new GlbStaffManyToManyCollection(group2);

			AssertEquals(false, collection1.ReadOnly);
			AssertEquals(true, collection2.ReadOnly);
		}

		public void TestOnlyCurrentGroupLinkValidated()
		{
			GlbStaff staffWithInvalidBirthdate = Factory.New<GlbStaff>();
			using (staffWithInvalidBirthdate.SuspendValidationTesting())
			{
				staffWithInvalidBirthdate.GS_Birthdate = ZDate.Invalid;
				TestCollection.Add(staffWithInvalidBirthdate);
				staffWithInvalidBirthdate.ClearAllNotifications();

				staffWithInvalidBirthdate.RunPreSaveValidation();
				AssertNoErrors("Staff is in collection, invalid birth date should not cause errors", staffWithInvalidBirthdate.GS_BirthdateInfo);

				GlbStaff staffWithInvalidMembershipType = Factory.New<GlbStaff>();
				using (staffWithInvalidMembershipType.SuspendValidationTesting())
				{
					TestCollection.Add(staffWithInvalidMembershipType);
					using (staffWithInvalidMembershipType.CurrentGroupLink.SuspendValidationTesting())
					{
						string invalidCode = "///";
						staffWithInvalidMembershipType.CurrentGroupLink.GK_MembershipType = invalidCode;
						staffWithInvalidMembershipType.CurrentGroupLink.ClearAllNotifications();

						staffWithInvalidMembershipType.CurrentGroupLink.RunPreSaveValidation();
						AssertHasErrors("Staff is in collection, invalid membership type should still cause errors", staffWithInvalidMembershipType.CurrentGroupLink.GK_MembershipTypeInfo);
					}
				}
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			return new GlbStaffManyToManyCollection(group);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, TestCollection.AllowNew);
		}

		GlbStaffManyToManyCollection TestCollection;

		public void TestAddAndRemoveStaffWithGroupLink()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			AssertNull("Staff has no link to group, current group link should be null", staff.CurrentGroupLink);

			TestCollection.Add(staff);
			Assert("Staff has a link to group, current group link should be not be null", staff.CurrentGroupLink != null);

			staff.CurrentGroupLink.GK_MembershipType = "PPP";
			TestCollection.Remove(staff);
			Assert("Staff has no link to group, current group link should be deleted", staff.CurrentGroupLink.IsDeleted);

			TestCollection.Add(staff);
			Assert("Staff has a link to group, current group link should be not be null", staff.CurrentGroupLink != null);
			Assert("Staff member has lost their original membership type", staff.CurrentGroupLink.GK_MembershipType != "PPP");
		}

		public void TestRemoveFromAllUsersGroup()
		{
			var staff = Factory.New<GlbStaff>();
			var allUsersGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode));
			var mockForm = new GroupFormForTestCollection(allUsersGroup);
			Assert("AllUserEvent should not have been fired", !mockForm.AllUserEventFired);
			allUsersGroup.Staff.Remove(staff);
			Assert("AllUserEvent should have been fired", mockForm.AllUserEventFired);
		}

		public void TestRemoveFromScimMappedGroup()
		{
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mappings = new StaffColumnToGroupDescriptionScimMappingCollection();
			mappings.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsController", GroupDescriptionMapping = "is controller" });
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var staff = Factory.New<GlbStaff>();
			var groupMapped = Factory.New<GlbGroup>();
			groupMapped.GG_Desc = "is controller";
			groupMapped.Staff.Add(staff);

			var mockForm = new GroupFormForTestCollection(groupMapped);
			Assert("SCIMEvent should not have been fired", !mockForm.SCIMEventFired);
			groupMapped.Staff.Remove(staff);
			Assert("SCIMEvent should have been fired", mockForm.SCIMEventFired);
		}

		public void TestRemoveFromFixDatabaesAccessGroups_StaffIsActive()
		{
			var staff = Factory.New<GlbStaff>();
			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.IsBackupOperator = true;

			var databaseDeveloperGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.DbDeveloperGroupPK));
			var databaseReaderGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.DbReaderGroupPK));
			var backupOperatorGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.BackupOperatorGroupPK));

			var mockForm_dbdeveloper = new GroupFormForTestCollection(databaseDeveloperGroup);
			var mockForm_dbreader = new GroupFormForTestCollection(databaseReaderGroup);
			var mockForm_bkoperator = new GroupFormForTestCollection(backupOperatorGroup);

			Assert("DbDeveloper form DatabaseAccessEvent should not have been fired", !mockForm_dbdeveloper.DatabaseAccessEventFired);
			Assert("DbReader form DatabaseAccessEvent should not have been fired", !mockForm_dbreader.DatabaseAccessEventFired);
			Assert("BkOperator form DatabaseAccessEvent should not have been fired", !mockForm_bkoperator.DatabaseAccessEventFired);

			databaseDeveloperGroup.Staff.Remove(staff);
			databaseReaderGroup.Staff.Remove(staff);
			backupOperatorGroup.Staff.Remove(staff);

			Assert("DbDeveloper form DatabaseAccessEvent should have been fired", mockForm_dbdeveloper.DatabaseAccessEventFired);
			Assert("DbReader form DatabaseAccessEvent should have been fired", mockForm_dbreader.DatabaseAccessEventFired);
			Assert("BkOperator form DatabaseAccessEvent should have been fired", mockForm_bkoperator.DatabaseAccessEventFired);
		}

		public void TestRemoveFromFixDatabaesAccessGroups_StaffIsNotActive()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = false;

			staff.IsDatabaseDeveloper = true;
			staff.IsReadOnlyDBUser = true;
			staff.IsBackupOperator = true;

			var databaseDeveloperGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.DbDeveloperGroupPK));
			var databaseReaderGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.DbReaderGroupPK));
			var backupOperatorGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.BackupOperatorGroupPK));

			var mockForm_dbdeveloper = new GroupFormForTestCollection(databaseDeveloperGroup);
			var mockForm_dbreader = new GroupFormForTestCollection(databaseReaderGroup);
			var mockForm_bkoperator = new GroupFormForTestCollection(backupOperatorGroup);

			Assert("DbDeveloper form DatabaseAccessEvent should not have been fired", !mockForm_dbdeveloper.DatabaseAccessEventFired);
			Assert("DbReader form DatabaseAccessEvent should not have been fired", !mockForm_dbreader.DatabaseAccessEventFired);
			Assert("BkOperator form DatabaseAccessEvent should not have been fired", !mockForm_bkoperator.DatabaseAccessEventFired);

			databaseDeveloperGroup.Staff.Remove(staff);
			databaseReaderGroup.Staff.Remove(staff);
			backupOperatorGroup.Staff.Remove(staff);

			Assert("DbDeveloper form DatabaseAccessEvent should not have been fired", !mockForm_dbdeveloper.DatabaseAccessEventFired);
			Assert("DbReader form DatabaseAccessEvent should not have been fired", !mockForm_dbreader.DatabaseAccessEventFired);
			Assert("BkOperator form DatabaseAccessEvent should not have been fired", !mockForm_bkoperator.DatabaseAccessEventFired);
		}

		public void TestRemoveFromFlexibleDatabaesAccessGroup_StaffIsActive()
		{
			var staff = Factory.New<GlbStaff>();

			var hrmStaffGroup = Factory.NewWithValidTestData<GlbGroup>();
			var hrmStaffGroupRole = Factory.NewWithValidTestData<GlbGroupRole>();
			hrmStaffGroupRole.GGR_GG_Group = hrmStaffGroup.PK;
			hrmStaffGroupRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

			staff.Groups.Add(hrmStaffGroup);

			var mockForm = new GroupFormForTestCollection(hrmStaffGroup);

			Assert("DatabaseAccessEvent should not have been fired", !mockForm.DatabaseAccessEventFired);

			hrmStaffGroup.Staff.Remove(staff);

			Assert("DatabaseAccessEvent should not have been fired", !mockForm.DatabaseAccessEventFired);
		}

		public void TestRemoveFromFlexibleDatabaesAccessGroup_StaffIsNotActive()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = false;

			var hrmStaffGroup = Factory.NewWithValidTestData<GlbGroup>();
			var hrmStaffGroupRole = Factory.NewWithValidTestData<GlbGroupRole>();
			hrmStaffGroupRole.GGR_GG_Group = hrmStaffGroup.PK;
			hrmStaffGroupRole.GGR_RoleName = DbRoleTypes.CwHRMStaffRole;

			staff.Groups.Add(hrmStaffGroup);

			var mockForm = new GroupFormForTestCollection(hrmStaffGroup);

			Assert("DatabaseAccessEvent should not have been fired", !mockForm.DatabaseAccessEventFired);

			hrmStaffGroup.Staff.Remove(staff);

			Assert("DatabaseAccessEvent should not have been fired", !mockForm.DatabaseAccessEventFired);
		}

		public void TestRemoveFromFixedDatabaseAccessGroup()
		{
			var staff = Factory.New<GlbStaff>();
			var databasedeveloperGroup = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.PK, GlbGroup.DbDeveloperGroupPK));
			var mockForm = new GroupFormForTestCollection(databasedeveloperGroup);

			databasedeveloperGroup.Staff.Remove(staff);
			Assert("DatabaseAccessEvent should have been fired", mockForm.DatabaseAccessEventFired);
		}

		public void TestRemoveFromFlexibleDatabaseAccessGroup()
		{
			var hrmstaffGroup = Factory.NewWithPrimaryKey<GlbGroup>(Guid.NewGuid());
			var hrmstaffGroupRole = Factory.New<GlbGroupRole>();
			hrmstaffGroupRole.GGR_RoleName = "cwHRMStaffRole";
			hrmstaffGroup.Roles.Add(hrmstaffGroupRole);

			var staff = Factory.New<GlbStaff>();
			staff.Groups.Add(hrmstaffGroup);

			var mockForm = new GroupFormForTestCollection(hrmstaffGroup);

			hrmstaffGroup.Staff.Remove(staff);
			Assert("DatabaseAccessEvent should not have been fired", !mockForm.DatabaseAccessEventFired);
		}

		[ExpectNoExceptions]
		public void TestRemoveFromAllUsersGroup_WhenEventNotSubscribed_ShouldNotThrow()
		{
			var group = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
			var staff = group.Staff.AddNew();
			var collection = new GlbStaffManyToManyCollection(group);
			collection.Remove(staff);
		}

		public void TestOnStaffAddedEventRaisedOnAdd()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Title = "Peasant";

			TestCollection.OnStaffAdded += (sender, args) =>
			{
				var senderAsStaff = sender as GlbStaff;
				if (senderAsStaff != null)
				{
					senderAsStaff.GS_Title = "King";
				}
			};

			TestCollection.Add(staff);
			AssertEquals("King", staff.GS_Title);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup testGroup = Factory.New<GlbGroup>();
			TestCollection = new GlbStaffManyToManyCollection(testGroup);
		}
	}
}

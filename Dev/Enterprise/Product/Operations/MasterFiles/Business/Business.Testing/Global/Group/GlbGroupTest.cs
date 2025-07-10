using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	partial class GlbGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSaveScimGroupWithExistingNonScimUser()
		{
			SetupScim(true);

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "scim test";
			group.GG_Code = "SCIMTESTCODE";

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "nonscimlogin1";
			staff.GS_FullName = "regular dude";
			group.Staff.Add(staff);

			Factory.Save();

			using (var cmd = Db.Connection.Command("update GlbGroup set GG_ExternalId = 'ext', GG_SystemLastEditTimeUtc = getdate(), GG_SystemLastEditUser = 'Z' where GG_Code = 'SCIMTESTCODE'"))
			{
				cmd.ExecuteNonQuery();
			}

			var newFactory = new BusinessObjectFactory();
			group = newFactory.Load<GlbGroup>(group.PK);

			AssertNotNull(group);
			AssertEquals("ext", group.GG_ExternalId);

			foreach (GlbSecurity securityPermission in group.SecurityPermissions)
			{
				securityPermission.GU_SecurityItemIsAllowed = true;
			}

			group.RunPreSaveValidation();

			AssertNoErrors(group);
		}

		public void TestChangeExistingGroupToScim()
		{
			SetupScim(true);

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "scim test";
			group.GG_Code = "SCIMTESTCODE";

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "nonscimlogin1";
			staff.GS_FullName = "regular dude";
			group.Staff.Add(staff);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			group = newFactory.Load<GlbGroup>(group.PK);

			AssertNotNull(group);

			group.GG_ExternalId = "zzz";

			group.RunPreSaveValidation();

			AssertNoErrors(group);
		}

		public void TestStaffs_NoExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.Staff.ReadOnly);
		}

		public void TestStaffs_NoExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.Staff.ReadOnly);
		}

		public void TestStaffs_HasExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.Staff.ReadOnly);
		}

		public void TestStaffs_HasExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.Staff.ReadOnly);
		}

		public void TestGroupDescription_NoExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.GG_DescInfo.ReadOnly);
		}

		public void TestGroupDescription_NoExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.GG_DescInfo.ReadOnly);
		}

		public void TestGroupDescription_HasExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.GG_DescInfo.ReadOnly);
		}

		public void TestGroupDescription_HasExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.GG_DescInfo.ReadOnly);
		}

		public void TestAddRemoveMembers_NoExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.Staff.ReadOnly);
		}

		public void TestAddRemoveMembers_NoExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.Staff.ReadOnly);
		}

		public void TestAddRemoveMembers_HasExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.Staff.ReadOnly);
		}

		public void TestAddRemoveMembers_HasExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.Staff.ReadOnly);
		}

		public void TestParentGroup_NoExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.GG_GG_ParentGroupInfo.ReadOnly);
		}

		public void TestParentGroup_NoExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", ZString.Empty);

			AssertEquals(false, group.GG_GG_ParentGroupInfo.ReadOnly);
		}

		public void TestParentGroup_HasExternalId_ScimEnabled()
		{
			SetupScim(true);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.GG_GG_ParentGroupInfo.ReadOnly);
		}

		public void TestParentGroup_HasExternalId_ScimDisabled()
		{
			SetupScim(false);
			var group = GetGroup("aaa", "id");

			AssertEquals(true, group.GG_GG_ParentGroupInfo.ReadOnly);
		}

		public void TestMultipleGroups_ExternalId()
		{
			var group1 = GetGroup("aaa", "");
			var group2 = GetGroup("bbb", "id");

			AssertEquals(false, group1.GG_DescInfo.ReadOnly);
			AssertEquals(true, group2.GG_DescInfo.ReadOnly);
			AssertEquals(false, group1.Staff.ReadOnly);
			AssertEquals(true, group2.Staff.ReadOnly);

			SetupScim(true);

			AssertEquals(false, group1.GG_DescInfo.ReadOnly);
			AssertEquals(true, group2.GG_DescInfo.ReadOnly);
			AssertEquals(false, group1.Staff.ReadOnly);
			AssertEquals(true, group2.Staff.ReadOnly);
		}

		void SetupScim(bool enabled)
		{
			SystemDataRegistry.Instance.EnableScimService.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);
		}

		GlbGroup GetGroup(ZString name, ZString externalId)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = name;
			group.GG_ExternalId = externalId;
			group.GG_Code = name;

			Factory.Save();
			return group;
		}

		public void TestCategory()
		{
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("WER", "Things that were"),
				new CodeDescriptionPair("ARE", "Things that are"),
				new CodeDescriptionPair("NOT", "And some things that have not yet come to pass"),
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, validCategories);

			var group = Factory.NewWithValidTestData<GlbGroup>();

			group.Validation.ValidateAll();
			AssertNoErrors(group);

			group.GG_Category = "WER";
			AssertNoErrors(group);

			group.GG_Category = "PAS";
			AssertHasError(group.GG_CategoryInfo, "Enter a valid Category.");

			group.GG_Category = "";
			AssertNoErrors(group);
		}

		public void TestParentGroupValidation()
		{
			var grandparentGroup = Factory.NewWithValidTestData<GlbGroup>();
			var parentGroup = Factory.NewWithValidTestData<GlbGroup>();
			var childGroup = Factory.NewWithValidTestData<GlbGroup>();

			childGroup.GG_GG_ParentGroup = ZGuid.NewZGuid();
			AssertHasError(childGroup.GG_GG_ParentGroupInfo, "Enter a valid Parent Group.");

			childGroup.GG_GG_ParentGroup = parentGroup.PK;
			parentGroup.GG_GG_ParentGroup = grandparentGroup.PK;

			AssertNoErrors(grandparentGroup);
			AssertHasError(parentGroup.GG_IsSecurityEnabledInfo, "Security groups must not have a Parent Group specified.");
			AssertHasError(parentGroup.GG_GG_ParentGroupInfo, "Security groups must not have a Parent Group specified.");
			AssertHasError(childGroup.GG_IsSecurityEnabledInfo, "Security groups must not have a Parent Group specified.");
			AssertHasError(childGroup.GG_GG_ParentGroupInfo, "Security groups must not have a Parent Group specified.");

			grandparentGroup.GG_IsSecurityEnabled = false;
			parentGroup.GG_IsSecurityEnabled = false;
			childGroup.GG_IsSecurityEnabled = false;

			AssertNoErrors(grandparentGroup);
			AssertNoErrors(parentGroup);
			AssertNoErrors(childGroup);
		}
	}
}

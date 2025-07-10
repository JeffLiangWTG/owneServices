using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class UserGroupTest : TestCaseWithFactory
	{
		public void TestUserGroup()
		{
			var parentGroup = Factory.New<GlbGroup>();
			parentGroup.GG_Code = "PAR";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "XYZ";
			group.GG_Desc = "Group XYZ";
			group.GG_DomainName = "domain1";
			group.GG_Category = "ABC";
			group.GG_GG_ParentGroup = parentGroup.PK;
			var userGroup = new UserGroup(group);

			CombineAssertions(() =>
			{
				AssertEquals("Code", "XYZ", userGroup.Code);
				AssertEquals("Description", "Group XYZ", userGroup.Description);
				AssertEquals("DomainName", "domain1", userGroup.DomainName);
				AssertEquals("Category", "ABC", userGroup.Category);
				AssertEquals("ParentGroup", "PAR", userGroup.ParentGroup.Code);
				AssertEquals("IsActive", ZBool.True, userGroup.IsActive);
				AssertEquals("IsNonSecurity", ZBool.False, userGroup.IsNonSecurity);
			});
		}

		public void TestNullUserGroup()
		{
			var userGroup = new UserGroup(null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Code", userGroup.Code);
				AssertNullOrEmpty("Description", userGroup.Description);
				AssertNullOrEmpty("DomainName", userGroup.DomainName);
				AssertNullOrEmpty("Category", userGroup.Category);
				AssertNull("ParentGroup", userGroup.ParentGroup);
				AssertEquals("IsActive", ZBool.False, userGroup.IsActive);
				AssertEquals("IsNonSecurity", ZBool.False, userGroup.IsNonSecurity);
			});
		}
	}
}

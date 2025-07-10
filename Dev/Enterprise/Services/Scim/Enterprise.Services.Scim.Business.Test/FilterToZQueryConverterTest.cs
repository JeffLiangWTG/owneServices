using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Domains.Builders;
using SimpleIdServer.Scim.Parser;
using SimpleIdServer.Scim.Persistence;
#if NET48
using System.Collections.Generic;
using System.Linq;
#endif

namespace Enterprise.Services.Scim.Business.Test
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Testing")]
	public class FilterToZQueryConverterTest : TestCaseWithFactory
	{
		public void TestUsers()
		{
			var staff1 = CreateStaff(true, "john smith", "john", "login1", "user1@email.com", "1", "", "");
			var staff2 = CreateStaff(true, "john haire", "john", "login2", "user2@email.com", "2", "SYD", "BRN");
			var staff3 = CreateStaff(true, "sam wang", "sam", "login3", "user3@email.com", "3", "SYD", "TLA");
			var staff4 = CreateStaff(true, "ted zheng", "ted", "login4", "user4@email.com", "4", "BNE", "TLA");

			Factory.Save();

			var converter = new FilterToZQueryConverter();

			var loaded = LoadStaffFromFilter(converter, "externalId eq \"1\"");
			AssertEquals(1, loaded.Length);
			AssertEquals(staff1.PK, loaded[0].PK);

			loaded = LoadStaffFromFilter(converter, "givenName eq \"john\"");
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { staff1.PK, staff2.PK }, loaded.Select(s => s.PK));

			loaded = LoadStaffFromFilter(converter, "givenName eq \"john\" and userName eq \"login2\"");
			AssertEquals(1, loaded.Length);
			AssertEquals(staff2.PK, loaded[0].PK);

			loaded = LoadStaffFromFilter(converter, "givenName eq \"john\" and userName eq \"login2\" and externalId eq \"2\"");
			AssertEquals(1, loaded.Length);
			AssertEquals(staff2.PK, loaded[0].PK);

			loaded = LoadStaffFromFilter(converter, "givenName eq \"john\" and userName eq \"login2\" and externalId eq \"1\"");
			AssertEquals(0, loaded.Length);

			loaded = LoadStaffFromFilter(converter, "givenName eq \"john\" and userName eq \"login2\" and (externalId eq \"1\" or externalId eq \"2\")");
			AssertEquals(1, loaded.Length);
			AssertEquals(staff2.PK, loaded[0].PK);

			loaded = LoadStaffFromFilter(converter, "homeBranch eq \"SYD\"");
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { staff2.PK, staff3.PK }, loaded.Select(s => s.PK));

			loaded = LoadStaffFromFilter(converter, "homeBranch ne \"SYD\"");
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { staff1.PK, staff4.PK }, loaded.Select(s => s.PK));

			loaded = LoadStaffFromFilter(converter, "homeDepartment eq \"TLA\"");
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { staff3.PK, staff4.PK }, loaded.Select(s => s.PK));

			loaded = LoadStaffFromFilter(converter, "homeBranch eq \"SYD\" and homeDepartment ne \"TLA\"");
			AssertEquals(1, loaded.Length);
			AssertEquals(staff2.PK, loaded[0].PK);
		}

		GlbStaff[] LoadStaffFromFilter(FilterToZQueryConverter converter, string literalFilter)
		{
			var userSchema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:User", "User", "User Account")
				 .AddStringAttribute("userName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var filter = SCIMFilterParser.Parse(literalFilter, new List<SCIMSchema> { userSchema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.User, 0, 100, null, filter: filter);
			var query = converter.ConvertToZQuery<GlbStaff>(param.Filter);
			var loaded = Factory.Load<GlbStaff>(query);

			AssertNotNull(loaded);
			return loaded;
		}

		GlbStaff CreateStaff(bool active, string fullname, string firstname, string loginName, string email, string externalId, string homeBranch, string homeDepartment)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_IsActive = active;
			staff.GS_FullName = fullname;
			staff.GS_GivenName = firstname;
			staff.GS_LoginName = loginName;
			staff.GS_EmailAddress = email;
			staff.GS_ExternalId = externalId;

			if (!string.IsNullOrEmpty(homeBranch) )
			{
				var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, homeBranch));
				staff.GS_GB_HomeBranch = branch.PK;
			}

			if (!string.IsNullOrEmpty(homeDepartment))
			{
				var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, homeDepartment));
				staff.GS_GE_HomeDepartment = department.PK;
			}

			return staff;
		}

		public void TestGroups()
		{
			var staff1 = CreateStaff(true, "sam wang", "sam", "login1", "user1@email.com", "1", "", "");
			var staff2 = CreateStaff(true, "ted zheng", "ted", "login2", "user2@email.com", "2", "", "");

			var group1 = CreateGroup(true, "group desc 1", "1", "");
			var group2 = CreateGroup(true, "group desc 2", "2", "C1", staff1);
			var group3 = CreateGroup(true, "other desc 3", "3", "C2", staff1, staff2);

			Factory.Save();

			var converter = new FilterToZQueryConverter();

			var loaded = LoadGroupsFromFilter(converter, "externalId eq \"1\"");
			AssertEquals(1, loaded.Length);
			AssertEquals(group1.PK, loaded[0].PK);

			loaded = LoadGroupsFromFilter(converter, "displayName co \"group desc\"");
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { group1.PK, group2.PK }, loaded.Select(g => g.PK));

			loaded = LoadGroupsFromFilter(converter, "displayName co \"group desc\" and externalId ne \"1\"");
			AssertEquals(1, loaded.Length);
			AssertEquals(group2.PK, loaded[0].PK);

			loaded = LoadGroupsFromFilter(converter, "externalId ne \"\" and (displayName eq \"other desc 3\" or displayName eq \"group desc 1\")");
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { group1.PK, group3.PK }, loaded.Select(g => g.PK));

			loaded = LoadGroupsFromFilter(converter, $"members[value eq \"{staff1.PK}\"]").Where(g => g.GG_Code != "ALL").ToArray();
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { group2.PK, group3.PK }, loaded.Select(g => g.PK));

			loaded = LoadGroupsFromFilter(converter, "category eq \"C1\"");
			AssertEquals(1, loaded.Length);
			AssertEquals(group2.PK, loaded[0].PK);

			loaded = LoadGroupsFromFilter(converter, "category ne \"\"");
			AssertEquals(2, loaded.Length);
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { group2.PK, group3.PK }, loaded.Select(g => g.PK));
		}

		GlbGroup[] LoadGroupsFromFilter(FilterToZQueryConverter converter, string literalFilter)
		{
			var schema = SCIMSchemaBuilder.Create("urn:ietf:params:scim:schemas:core:2.0:Group", "Group", "Group")
				 .AddStringAttribute("displayName", caseExact: true, uniqueness: SCIMSchemaAttributeUniqueness.SERVER)
				 .Build();

			var filter = SCIMFilterParser.Parse(literalFilter, new List<SCIMSchema> { schema });
			var param = new SearchSCIMRepresentationsParameter(SCIMResourceTypes.Group, 0, 100, null, filter: filter);
			var query = converter.ConvertToZQuery<GlbGroup>(param.Filter);
			var loaded = Factory.Load<GlbGroup>(query);

			AssertNotNull(loaded);
			return loaded;
		}

		GlbGroup CreateGroup(bool active, string desc, string externalId, string category, params GlbStaff[] members)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_IsActive = active;
			group.GG_Desc = desc;
			group.GG_ExternalId = externalId;
			group.GG_Code = "group" + externalId;
			group.GG_Category = category;

			foreach (var member in members)
			{
				group.Staff.Add(member);
			}

			return group;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var validCategories = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("C1", "Category 1"),
				new CodeDescriptionPair("C2", "Category 2"),
			};
			SystemDataRegistry.Instance.GroupCategoryList.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, validCategories);
		}
	}
}

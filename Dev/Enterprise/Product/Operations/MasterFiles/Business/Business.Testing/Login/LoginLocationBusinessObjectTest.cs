using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(LoginLocationBusinessObject))]
	public class LoginLocationBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBranchFetchHints()
		{
			var glbstaff = Factory.NewWithValidTestData<GlbStaff>();
			glbstaff.GS_Code = "GSJ";
			glbstaff.GS_IsController = false;

			for (var i = 0; i < 10; i++)
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.GC_Code = $"GC{i}";
				company1.GC_Name = $"test Company {i}";
				company1.GC_IsActive = true;

				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_Code = $"GB{i}";
				branch1.GB_BranchName = $"test Branch {i}";
				branch1.GB_GC = company1.PK;
				branch1.GB_IsActive = true;
			}
			Factory.Save();

			var loginLocationBizo = new LoginLocationBusinessObject(new BusinessObjectFactory());
			loginLocationBizo.CurrentUserForTesting = glbstaff;

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbSecuritySchema.Constants.TableName, 2 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, loginLocationBizo.Factory))
			{
				_ = loginLocationBizo.Companies;
			}
		}

		[ExpectNoExceptions]
		public void TestLoginSecuritiesWithSameCompanyAndBranch()
		{
			PrepareTestData();

			var gb1 = Factory.LoadFromUniqueKey<GlbBranch>(GlbBranchSchema.GB_Code, (ZString)"B1");

			var glbdepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			glbdepartment1.GE_Code = "DE1";
			var glbdepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			glbdepartment2.GE_Code = "DE2";

			var glbstaff = Factory.NewWithValidTestData<GlbStaff>();
			glbstaff.GS_Code = "GS1";

			AddSecurity(glbstaff.PK, ZGuid.Empty, ZGuid.Empty, gb1.PK, glbdepartment1.PK, true);
			AddSecurity(glbstaff.PK, ZGuid.Empty, ZGuid.Empty, gb1.PK, glbdepartment2.PK, true);
			AddSecurity(glbstaff.PK, ZGuid.Empty, ZGuid.Empty, gb1.PK, ZGuid.Empty, false);

			Factory.Save();

			var loginLocationBizo = CreateNewLoginObject();
			loginLocationBizo.CompanyCode = "C1";
			SetupUser(glbstaff, loginLocationBizo);

			_ = loginLocationBizo.Branches;
		}

		[StressTest]
		public virtual void TestAllowedBranchList()
		{
			PrepareTestData();
			SetAllStaffDefaultPermissionsTo(false);
			AssertBranchesForUser("User 1 (granted by default)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 2 (default group)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 3 (granted group)", "Branch 1", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 4 (denied group)");
			AssertBranchesForUser("User 5 (1Gra 2Den 3Def group)", "Branch 1", "Branch 3");
			AssertBranchesForUser("User 6 (granted group extended)", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 7 (denied group extended)", "Branch 1");
			AssertBranchesForUser("User 8 (1Gra 2Den 3Def group overriden)", "Branch 2", "Branch 3");
			AssertBranchesForUser("User 9 (1Gra 2Def 3Den)", "Branch 1", "Branch 2");
			AssertBranchesForUser("User 10 (denied company)", "Branch 3");
			AssertBranchesForUser("User 11 (all denied)");
			AssertBranchesForUser("User 12 (1Gra)", "Branch 1");
			AssertBranchesForUser("adminXXX", "Branch 1", "Branch 2", "Branch 3");
		}

		[StressTest]
		public void TestAllowedBranchListWithImplicitAllStaff()
		{
			PrepareTestData();
			AssertBranchesForUser("User 13 (all granted because implicit)", "Branch 1", "Branch 2", "Branch 3"); // because belongs only to All Staff
			AssertBranchesForUser("User 14 (all granted because 1 is implicitly allowed)", "Branch 1", "Branch 2", "Branch 3"); // anni.skeete
		}

		[StressTest]
		public void TestAllowDepartmentsList()
		{
			PrepareTestData();
			LoginObj.CompanyCode = "";
			LoginObj.BranchCode = "";
			LoginObj.DepartmentCode = "";
			AssertDepartmentsForUser("User 1 (granted by default)", "D1", "D2", "D3");
			AssertDepartmentsForUser("User 2 (default group)", "D1", "D2", "D3");

			AssertDepartmentsForUser("User 3 (granted group)", "D1", "D2", "D3");
			LoginObj.BranchCode = "B1";
			AssertDepartmentsForUser("User 3 (granted group)", "D2", "D3");
			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "C1";
			AssertDepartmentsForUser("User 3 (granted group)", "D1", "D2", "D3");

			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "";
			AssertDepartmentsForUser("User 7 (denied group extended)");
			LoginObj.BranchCode = "B1";
			AssertDepartmentsForUser("User 7 (denied group extended)", "D1", "D3");
			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "C2";
			AssertDepartmentsForUser("User 7 (denied group extended)");
			LoginObj.CompanyCode = "";
			LoginObj.BranchCode = "B3";
			AssertDepartmentsForUser("User 7 (denied group extended)");

			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "";
			AssertDepartmentsForUser("User 17 (Two groups)", "D1", "D2", "D3");
			LoginObj.BranchCode = "B1";
			AssertDepartmentsForUser("User 17 (Two groups)", "D2", "D3");
			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "C2";
			AssertDepartmentsForUser("User 17 (Two groups)", "D1", "D2", "D3");
			LoginObj.CompanyCode = "";
			LoginObj.BranchCode = "B3";
			AssertDepartmentsForUser("User 17 (Two groups)", "D1", "D2", "D3");

			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "";
			AssertDepartmentsForUser("User 18 (B1 denied, C1 granted)", "D1", "D2", "D3");
			LoginObj.BranchCode = "B1";
			AssertDepartmentsForUser("User 18 (B1 denied, C1 granted)", "D2", "D3");
			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "C1";
			AssertDepartmentsForUser("User 18 (B1 denied, C1 granted)", "D1", "D2", "D3");
			LoginObj.BranchCode = "B1";
			AssertDepartmentsForUser("User 18 (B1 denied, C1 granted)", "D2", "D3");

			LoginObj.BranchCode = "";
			LoginObj.CompanyCode = "C1";
			AssertDepartmentsForUser("User 19 (Two groups 2)", "D1", "D2", "D3");
			LoginObj.BranchCode = "B1";
			AssertDepartmentsForUser("User 19 (Two groups 2)", "D1", "D2", "D3");
		}

		protected void PrepareTestData()
		{
			#region Companies and branches

			foreach (var existingCompany in Factory.Load<GlbCompany>(new ZQuery()))
			{
				existingCompany.GC_IsActive = false;
			}

			foreach (var existingDepartment in Factory.Load<GlbDepartment>(new ZQuery()))
			{
				existingDepartment.GE_IsActive = false;
			}

			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "C1";
			company1.GC_Name = "C1";
			company1.SetCountry("AU");
			company1.SetCurrency("AUD");

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "C2";
			company2.GC_Name = "C2";
			company1.SetCountry("AU");
			company1.SetCurrency("AUD");

			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "B1";
			branch1.GB_BranchName = "Branch 1";
			branch1.GB_GC = company1.PK;
			branch1.GB_IsActive = true;

			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "B2";
			branch2.GB_BranchName = "Branch 2";
			branch2.GB_GC = company1.PK;
			branch2.GB_IsActive = true;

			GlbBranch branch3 = Factory.New<GlbBranch>();
			branch3.GB_Code = "B3";
			branch3.GB_BranchName = "Branch 3";
			branch3.GB_GC = company2.PK;
			branch3.GB_IsActive = true;

			GlbBranch branch4 = Factory.New<GlbBranch>(); //This branch should never show up because it is inactive
			branch4.GB_Code = "INA";
			branch4.GB_BranchName = "Inactive";
			branch4.GB_GC = company2.PK;
			branch4.GB_IsActive = false;

			GlbDepartment department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "D1";
			department1.GE_Desc = "Department 1";

			GlbDepartment department2 = Factory.New<GlbDepartment>();
			department2.GE_Code = "D2";
			department2.GE_Desc = "department 2";

			GlbDepartment department3 = Factory.New<GlbDepartment>();
			department3.GE_Code = "D3";
			department3.GE_Desc = "Department 3";
			#endregion

			#region Groups

			GlbGroup group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "G1";
			group1.SecurityPermissions.RemoveAndDeleteAll();
			group1.GG_Desc = "Group 1 (default access)";
			AddSecurity(ZGuid.Empty, group1.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true);

			GlbGroup group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_Desc = "Group 2 (all granted)";
			group2.SecurityPermissions.RemoveAndDeleteAll();
			AddSecurity(ZGuid.Empty, group2.PK, ZGuid.Empty, branch1.PK, department1.PK, false); // Non-empty department should not deny access rights to a branch
			AddSecurity(ZGuid.Empty, group2.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true);

			GlbGroup group3 = Factory.New<GlbGroup>();
			group3.GG_Code = "G3";
			group3.GG_Desc = "Group 3 (all denied)";
			group3.SecurityPermissions.RemoveAndDeleteAll();
			AddSecurity(ZGuid.Empty, group3.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false);

			GlbGroup group4 = Factory.New<GlbGroup>();
			group4.GG_Code = "G4";
			group4.GG_Desc = "Group 4 (1Gra 2Den 3Def)";
			group4.SecurityPermissions.RemoveAndDeleteAll();
			AddSecurity(ZGuid.Empty, group4.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AddSecurity(ZGuid.Empty, group4.PK, ZGuid.Empty, branch1.PK, department1.PK, true); // Department Pk should not be checked in branches filter
			AddSecurity(ZGuid.Empty, group4.PK, ZGuid.Empty, branch2.PK, ZGuid.Empty, false);

			GlbGroup group5 = Factory.New<GlbGroup>();
			group5.GG_Code = "G5";
			group5.GG_Desc = "Group 5";
			group5.SecurityPermissions.RemoveAndDeleteAll();
			AddSecurity(ZGuid.Empty, group5.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false);
			AddSecurity(ZGuid.Empty, group5.PK, company1.PK, ZGuid.Empty, ZGuid.Empty, true);

			GlbGroup group6 = Factory.New<GlbGroup>();
			group6.GG_Code = "G6";
			group6.GG_Desc = "Group 6";
			group6.SecurityPermissions.RemoveAndDeleteAll();
			AddSecurity(ZGuid.Empty, group6.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false);
			AddSecurity(ZGuid.Empty, group6.PK, company1.PK, ZGuid.Empty, ZGuid.Empty, true);
			AddSecurity(ZGuid.Empty, group6.PK, ZGuid.Empty, branch1.PK, ZGuid.Empty, false);

			#endregion

			#region Staff

			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "U1";
			staff1.GS_LoginName = "User 1 (granted by default)";
			AddSecurity(staff1.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true);

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "U2";
			staff2.GS_LoginName = "User 2 (default group)";
			staff2.Groups.Add(group1);

			GlbStaff staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "U3";
			staff3.GS_LoginName = "User 3 (granted group)";
			staff3.Groups.Add(group2);

			GlbStaff staff4 = Factory.New<GlbStaff>();
			staff4.GS_Code = "U4";
			staff4.GS_LoginName = "User 4 (denied group)";
			staff4.Groups.Add(group3);

			GlbStaff staff5 = Factory.New<GlbStaff>();
			staff5.GS_Code = "U5";
			staff5.GS_LoginName = "User 5 (1Gra 2Den 3Def group)";
			staff5.Groups.Add(group4);

			GlbStaff staff6 = Factory.New<GlbStaff>();
			staff6.GS_Code = "U6";
			staff6.GS_LoginName = "User 6 (granted group extended)";
			staff6.Groups.Add(group2);
			AddSecurity(staff6.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, ZGuid.Empty, false);

			GlbStaff staff7 = Factory.New<GlbStaff>();
			staff7.GS_Code = "U7";
			staff7.GS_LoginName = "User 7 (denied group extended)";
			staff7.Groups.Add(group3);
			AddSecurity(staff7.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, ZGuid.Empty, true);
			AddSecurity(staff7.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, department2.PK, false);
			AddSecurity(staff7.PK, ZGuid.Empty, company2.PK, ZGuid.Empty, department3.PK, false);

			GlbStaff staff8 = Factory.New<GlbStaff>();
			staff8.GS_Code = "U8";
			staff8.GS_LoginName = "User 8 (1Gra 2Den 3Def group overriden)";
			staff8.Groups.Add(group4);
			AddSecurity(staff8.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, ZGuid.Empty, false);
			AddSecurity(staff8.PK, ZGuid.Empty, ZGuid.Empty, branch2.PK, ZGuid.Empty, true);

			GlbStaff staff9 = Factory.New<GlbStaff>();
			staff9.GS_Code = "U9";
			staff9.GS_LoginName = "User 9 (1Gra 2Def 3Den)";
			AddSecurity(staff9.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AddSecurity(staff9.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, ZGuid.Empty, true);
			AddSecurity(staff9.PK, ZGuid.Empty, ZGuid.Empty, branch3.PK, ZGuid.Empty, false);

			GlbStaff staff10 = Factory.New<GlbStaff>();
			staff10.GS_Code = "U10";
			staff10.GS_LoginName = "User 10 (denied company)";
			AddSecurity(staff10.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AddSecurity(staff10.PK, ZGuid.Empty, company1.PK, ZGuid.Empty, ZGuid.Empty, false);

			GlbStaff staff11 = Factory.New<GlbStaff>();
			staff11.GS_Code = "U11";
			staff11.GS_LoginName = "User 11 (all denied)";
			AddSecurity(staff11.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false);

			GlbStaff staff12 = Factory.New<GlbStaff>();
			staff12.GS_Code = "U12";
			staff12.GS_LoginName = "User 12 (1Gra)";
			AddSecurity(staff12.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, department1.PK, true);
			AddSecurity(staff12.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false);

			GlbStaff admin = Factory.New<GlbStaff>();
			admin.GS_Code = "XXX";
			admin.GS_LoginName = "adminXXX";
			admin.GS_IsController = true;
			AddSecurity(admin.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, false);

			GlbStaff staff13 = Factory.New<GlbStaff>();
			staff13.GS_Code = "U13";
			staff13.GS_LoginName = "User 13 (all granted because implicit)";

			GlbStaff staff14 = Factory.New<GlbStaff>();
			staff14.GS_Code = "U14";
			staff14.GS_LoginName = "User 14 (all granted because 1 is implicitly allowed)";
			staff14.Groups.Add(group3);

			var staff15 = Factory.New<GlbStaff>();
			staff15.GS_Code = "U15";
			staff15.GS_LoginName = "User 15 (D1 denied)";
			AddSecurity(staff15.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, department1.PK, false);

			var staff16 = Factory.New<GlbStaff>();
			staff16.GS_Code = "U16";
			staff16.GS_LoginName = "User 16 (D1 denied, but I'm a controller)";
			staff16.GS_IsController = true;
			AddSecurity(staff16.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, department1.PK, false);

			var staff17 = Factory.New<GlbStaff>();
			staff17.GS_Code = "U17";
			staff17.GS_LoginName = "User 17 (Two groups)";
			staff17.Groups.Add(group3);
			staff17.Groups.Add(group2);

			var staff18 = Factory.New<GlbStaff>();
			staff18.GS_Code = "U18";
			staff18.GS_LoginName = "User 18 (B1 denied, C1 granted)";
			AddSecurity(staff18.PK, ZGuid.Empty, company1.PK, ZGuid.Empty, department1.PK, true);
			AddSecurity(staff18.PK, ZGuid.Empty, ZGuid.Empty, branch1.PK, department1.PK, false);

			var staff19 = Factory.New<GlbStaff>();
			staff19.GS_Code = "U19";
			staff19.GS_LoginName = "User 19 (Two groups 2)";
			staff19.Groups.Add(group5);
			staff19.Groups.Add(group6);
			#endregion

			Factory.Save();
		}

		void SetAllStaffDefaultPermissionsTo(bool value)
		{
			GlbStaff[] users = Factory.Load<GlbStaff>(new ZQuery());
			GlbBranch[] branches = Factory.Load<GlbBranch>(new ZQuery());
			foreach (var user in users)
			{
				if (user.ActiveGroups.Any() && user.ActiveGroups[0].GG_Desc == "ALL STAFF")
				{
					foreach (var branch in branches)
					{
						AddSecurity(ZGuid.Empty, user.ActiveGroups[0].PK, ZGuid.Empty, branch.PK, ZGuid.Empty, value); // explicitly disallow All Staff for each Branch so we can have some denials
					}
					break;
				}
			}
			Factory.Save();
		}

		void AddSecurity(ZGuid staffPk, ZGuid groupPk, ZGuid companyPk, ZGuid branchPk, ZGuid deptPk, ZBool granted)
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_GS = staffPk;
			security.GU_GG = groupPk;
			security.GU_GC = companyPk;
			security.GU_GB = branchPk;
			security.GU_GE = deptPk;
			security.GU_SecurityRight = "Login";
			security.GU_SecurityItemIsAllowed = granted;
		}

		protected void AssertBranchesForUser(string userLogin, params string[] branchesExpected)
		{
			ResetLoginUser();
			var loginLocationBizo = CreateNewLoginObject();

			var currentUser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, userLogin));
			SetupUser(currentUser, loginLocationBizo);

			var branches = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_BranchName, branchesExpected));
			var branchesGroupedByCompany = branches.GroupBy(branch => branch.GB_GC);

			var expectedCompanies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, branchesGroupedByCompany.Select(g => g.Key))).Select(company => company.GC_Name);
			var actualCompanies = loginLocationBizo.Companies.Select(company => company.GC_Name);
			AssertContainsExactElementsInAnyOrder(userLogin + " companies", expectedCompanies, actualCompanies);

			foreach (var group in branchesGroupedByCompany)
			{
				loginLocationBizo.CompanyCode = Factory.Load<GlbCompany>(group.Key).GC_Code;
				var expectedBranches = group.Select(branch => branch.GB_BranchName);
				var actualBranches = loginLocationBizo.Branches.Select(branch => branch.GB_BranchName);
				AssertContainsExactElementsInAnyOrder(userLogin + " " + loginLocationBizo.CompanyCode + " branches", expectedBranches, actualBranches);
			}
		}

		protected virtual void AssertDepartmentsForUser(string userLogin, params string[] expectedDepartments)
		{
			SetupUser(Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, userLogin)), LoginObj);
			var departments = Factory.Load<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, expectedDepartments));
			AssertContainsExactElementsInAnyOrder(userLogin + " departments",
				departments.Select(d => d.GE_Code),
				LoginObj.Departments.Select(d => d.GE_Code));
		}

		protected virtual void SetupUser(GlbStaff user, LoginLocationBusinessObject loginObj)
		{
			if (loginObj.CurrentUserForTesting == null || loginObj.CurrentUserForTesting.GS_Code != user.GS_Code)
			{
				loginObj.CurrentUserForTesting = user;
				loginObj.ClearSecurityCache();
			}
		}

		protected LoginLocationBusinessObject LoginObj => loginObj ?? (loginObj = CreateNewLoginObject());

		LoginLocationBusinessObject loginObj;

		protected virtual LoginLocationBusinessObject CreateNewLoginObject()
		{
			return new LoginLocationBusinessObject(Factory);
		}

		protected virtual void ResetLoginUser()
		{
		}
	}
}

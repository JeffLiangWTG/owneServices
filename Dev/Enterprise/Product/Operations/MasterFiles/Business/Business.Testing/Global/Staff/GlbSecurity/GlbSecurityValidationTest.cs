using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbSecurityValidationTest : BusinessObjectValidationTestCase
	{
		#region Validation

		public void TestCompanyAndBranchCannotBothBeSpecified()
		{
			Security.CompanyCode = Security.Lookups.Companies[0].GC_Code;
			Security.BranchCode = Security.Lookups.Branches[0].GB_Code;
			AssertEquals("Company Code should be cleared", "*", Security.CompanyCode);
			AssertEquals("Branch Code should not be cleared", Security.Lookups.Branches[0].GB_Code, Security.BranchCode);
			Security.CompanyCode = Security.Lookups.Companies[0].GC_Code;
			AssertEquals("Branch Code should be cleared", "*", Security.BranchCode);
			AssertEquals("Company Code should not be cleared", Security.Lookups.Companies[0].GC_Code, Security.CompanyCode);
		}

		[ExpectNoExceptions()]
		public void TestDuplicateRecordIssue()
		{
			GlbStaff savedStaff = Factory.New<GlbStaff>();
			savedStaff.GS_Code = "ZAC";

			GlbBranch branchOfCompany = Factory.New<GlbBranch>();
			branchOfCompany.GB_GC = Security.Lookups.Companies[0].PK;

			Factory.Save();

			GlbSecurity security1 = Factory.New<GlbSecurity>();
			security1.GU_SecurityRight = "test";
			security1.GU_GS = savedStaff.PK;
			security1.GU_GC = Security.Lookups.Companies[0].PK;
			savedStaff.StaffSecurityPermissionsCollection.Add(security1);

			GlbSecurity security2 = Factory.New<GlbSecurity>();
			security2.GU_SecurityRight = "test";
			security2.GU_GS = savedStaff.PK;
			savedStaff.StaffSecurityPermissionsCollection.Add(security2);

			Factory.Save();

			security1.GU_GC = ZGuid.Empty;
			security1.GU_GB = branchOfCompany.PK;

			security2.GU_GC = Security.Lookups.Companies[0].PK;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			GlbSecurityCollection loadedSecurityCollection = new GlbSecurityCollection(newFactory);
			loadedSecurityCollection.Load(new ZQuery(GlbSecuritySchema.GU_GS, savedStaff.PK));

			savedStaff.StaffSecurityPermissionsCollection.Load(new ZQuery(GlbSecuritySchema.GU_GS, savedStaff.PK));

			SortInfo sortDirection = new SortInfo(GlbSecuritySchema.GU_GB.Name, System.ComponentModel.ListSortDirection.Ascending);
			savedStaff.StaffSecurityPermissionsCollection.Sort(sortDirection);
			loadedSecurityCollection.Sort(sortDirection);

			//Check that Security permissions saved correctly
			AssertEquals("Security permissions should match", loadedSecurityCollection[0].PK, savedStaff.StaffSecurityPermissionsCollection[0].PK);
			AssertEquals("Security permissions should match", loadedSecurityCollection[0].GU_GC, savedStaff.StaffSecurityPermissionsCollection[0].GU_GC);
			AssertEquals("Security permissions should match", loadedSecurityCollection[0].GU_GB, savedStaff.StaffSecurityPermissionsCollection[0].GU_GB);

			AssertEquals("Security permissions should match", loadedSecurityCollection[1].PK, savedStaff.StaffSecurityPermissionsCollection[1].PK);
			AssertEquals("Security permissions should match", loadedSecurityCollection[1].GU_GC, savedStaff.StaffSecurityPermissionsCollection[1].GU_GC);
			AssertEquals("Security permissions should match", loadedSecurityCollection[1].GU_GB, savedStaff.StaffSecurityPermissionsCollection[1].GU_GB);
		}

		public void TestValidateBranch()
		{
			Security.BranchCode = "!!!";
			Assert("Invalid code, BranchCode should have errors", Security.BranchCodeInfo.HasErrors());
			Security.BranchCode = Security.Lookups.Branches[0].GB_Code;
			Assert("Valid code, BranchCode should not have errors", !Security.BranchCodeInfo.HasErrors());
			Security.BranchCode = "*";
			Assert("Empty code, BranchCode should not have errors", !Security.BranchCodeInfo.HasErrors());
		}

		public void TestValidateCompany()
		{
			Security.CompanyCode = "!!!";
			Assert("Invalid code, CompanyCode should have errors", Security.CompanyCodeInfo.HasErrors());
			Security.CompanyCode = Security.Lookups.Companies[0].GC_Code;
			Assert("Valid code, CompanyCode should not have errors", !Security.CompanyCodeInfo.HasErrors());
			Security.CompanyCode = "*";
			Assert("Empty code, CompanyCode should not have errors", !Security.CompanyCodeInfo.HasErrors());
		}

		public void TestValidateDepartment()
		{
			Security.DepartmentCode = "!!!";
			Assert("Invalid code, DepartmentCode should have errors", Security.DepartmentCodeInfo.HasErrors());
			Security.BranchCode = "*";
			Security.CompanyCode = "*";
			Security.DepartmentCode = Security.Lookups.Departments[0].GE_Code;
			Assert("CompanyCode or BranchCode must be specified, DepartmentCode should have errors", Security.DepartmentCodeInfo.HasErrors());

			Security.CompanyCode = Security.Lookups.Companies[0].GC_Code;
			Security.DepartmentCode = Security.Lookups.Departments[0].GE_Code;
			Assert("Valid code, Department should not have errors", !Security.DepartmentCodeInfo.HasErrors());
		}

		public void TestRowIsDuplicate()
		{
			Group.SecurityPermissions.Add(Security);
			GlbSecurity security2 = Factory.New<GlbSecurity>();
			Group.SecurityPermissions.Add(security2);

			security2.GU_SecurityRight = Security.GU_SecurityRight;
			security2.GU_GG = Security.GU_GG;
			security2.GU_GS = Security.GU_GS;
			security2.GU_GE = Security.GU_GE;
			security2.GU_GB = Security.GU_GB;
			security2.GU_GC = Security.GU_GC;
			security2.RunPreSaveValidation();
			AssertHasErrors("The Security Permission is not unique, Group should have errors", security2.CompanyCodeInfo);

			security2.GU_ItemGUID = ZGuid.NewZGuid();
			security2.RunPreSaveValidation();
			AssertNoErrors("The Security Permission is unique, should have no errors", security2.CompanyCodeInfo);
		}

		public void TestRowIsDuplicate_WhenChangedLater()
		{
			GlbCompany company1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();

			GlbBranch branch1 = company1.Branches.AddNew();
			company1.Branches.AdditionalFilter = new ZQuery(GlbBranchSchema.PK, branch1.PK);
			GlbBranch branch2 = company2.Branches.AddNew();
			company1.Branches.AdditionalFilter.AddToFilter(JoinCondition.And, GlbBranchSchema.PK, branch2.PK);

			branch1.FillWithValidTestData();
			branch2.FillWithValidTestData();

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			GlbSecurity security = Factory.New<GlbSecurity>();
			Group.SecurityPermissions.Add(security);
			GlbSecurity security2 = Factory.New<GlbSecurity>();
			Group.SecurityPermissions.Add(security2);

			security.GU_GC = company1.PK;
			security.GU_GB = branch1.PK;
			security.GU_GE = department1.PK;

			security2.GU_SecurityRight = security.GU_SecurityRight;
			security2.GU_GG = security.GU_GG;
			security2.GU_GS = security.GU_GS;
			security2.GU_GC = security.GU_GC;
			security2.GU_GB = security.GU_GB;

			security2.GU_GE = department2.PK;

			security.RunPreSaveValidation();
			AssertNoErrors("The Security Permission is unique, should have no errors", security.CompanyCodeInfo);
			security.Factory.Save();

			security2.GU_GE = department1.PK;
			security2.RunPreSaveValidation();
			AssertHasErrors("The Security Permission is not unique, should have errors", security2.DepartmentCodeInfo);

			security.GU_GE = department2.PK;
			security.RunPreSaveValidation();
			AssertNoErrors("The Security Permission is unique, should have no errors", security.DepartmentCodeInfo);
		}

		public void TestValidateAllWhenNoUserLoggedIn_ShouldNotThrow()
		{
			var staff = Factory.New<GlbStaff>();
			var security = Factory.New<GlbSecurity>();
			security.GU_GS = staff.PK;

			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var validation = new GlbSecurityValidation(security);
				AssertNoExceptionThrown(() => validation.ValidateAll());
			}
		}

		#endregion

		#region Implementation

		GlbGroup Group;
		GlbSecurity Security;

		protected override void SetUp()
		{
			base.SetUp();
			Group = Factory.New<GlbGroup>();

			GlbCompany companyToAdd = Factory.NewWithValidTestData<GlbCompany>();
			GlbDepartment departmentToAdd = Factory.NewWithValidTestData<GlbDepartment>();
			GlbBranch branchToAdd = Factory.NewWithValidTestData<GlbBranch>();

			Security = Factory.NewWithValidTestData<GlbSecurity>();

			Security.Lookups.Companies.AdditionalFilter = new ZQuery(GlbCompanySchema.PK, companyToAdd.PK);
			Security.Lookups.Departments.AdditionalFilter = new ZQuery(GlbDepartmentSchema.PK, departmentToAdd.PK);
			Security.Lookups.Branches.Add(branchToAdd);
		}

		#endregion
	}
}

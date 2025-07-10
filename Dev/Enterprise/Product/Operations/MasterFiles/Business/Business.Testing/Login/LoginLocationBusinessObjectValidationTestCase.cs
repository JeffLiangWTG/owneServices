using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class LoginLocationBusinessObjectValidationTestCase : TestCaseWithFactory
	{
		public void TestCodeValidation()
		{
			var bizo = CreateNewLoginObject();
			AssertNoErrors(bizo.CompanyCodeInfo);
			AssertNoErrors(bizo.BranchCodeInfo);
			AssertNoErrors(bizo.DepartmentCodeInfo);

			bizo.CompanyCode = "XXX";
			AssertHasErrors(bizo.CompanyCodeInfo);
			bizo.CompanyCode = Env.CurrentCompany.Code;
			AssertNoErrors(bizo.CompanyCodeInfo);

			bizo.BranchCode = "XXX";
			AssertHasErrors(bizo.BranchCodeInfo);

			bizo.DepartmentCode = "XXX";
			AssertHasErrors(bizo.DepartmentCodeInfo);
		}

		public void TestValidateDepartmentCode()
		{
			GlbCompany company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "C1";
			company1.GC_Name = "C1";

			GlbCompany company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "C2";
			company2.GC_Name = "C2";

			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "B1";
			branch1.GB_BranchName = "Branch 1";
			branch1.GB_GC = company1.PK;
			branch1.GB_IsActive = true;

			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "B2";
			branch2.GB_BranchName = "Branch 2";
			branch2.GB_GC = company2.PK;
			branch2.GB_IsActive = true;

			GlbDepartment department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "D1";
			department1.GE_Desc = "Department 1";

			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "U1";
			staff1.GS_LoginName = "User 1 (default access)";

			AddSecurity(staff1.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, true);
			AddSecurity(staff1.PK, company1.PK, ZGuid.Empty, department1.PK, true);
			AddSecurity(staff1.PK, company2.PK, ZGuid.Empty, department1.PK, false);

			Factory.Save();

			var loginObject = CreateNewLoginObject();
			loginObject.CurrentUserForTesting = staff1;
			AssertNoErrors(loginObject.CompanyCodeInfo);
			AssertNoErrors(loginObject.BranchCodeInfo);
			AssertNoErrors(loginObject.DepartmentCodeInfo);

			loginObject.CompanyCode = company1.GC_Code;
			loginObject.BranchCode = branch1.GB_Code;
			loginObject.DepartmentCode = department1.GE_Code;
			AssertNoErrors(loginObject.CompanyCodeInfo);
			AssertNoErrors(loginObject.BranchCodeInfo);
			AssertNoErrors(loginObject.DepartmentCodeInfo);

			loginObject.CompanyCode = company2.GC_Code;
			loginObject.BranchCode = branch2.GB_Code;
			AssertNoErrors(loginObject.CompanyCodeInfo);
			AssertNoErrors(loginObject.BranchCodeInfo);
			AssertDepartmentCode(loginObject);
		}

		protected virtual void AssertDepartmentCode(LoginLocationBusinessObject loginObj)
		{
			AssertHasError(loginObj.DepartmentCodeInfo, "Enter a valid Department.");
		}

		void AddSecurity(ZGuid staffPk, ZGuid companyPk, ZGuid branchPk, ZGuid deptPk, ZBool granted)
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_GS = staffPk;
			security.GU_GC = companyPk;
			security.GU_GB = branchPk;
			security.GU_GE = deptPk;
			security.GU_SecurityRight = "Login";
			security.GU_SecurityItemIsAllowed = granted;
		}

		protected virtual LoginLocationBusinessObject CreateNewLoginObject()
		{
			return new LoginLocationBusinessObject(Factory);
		}
	}
}

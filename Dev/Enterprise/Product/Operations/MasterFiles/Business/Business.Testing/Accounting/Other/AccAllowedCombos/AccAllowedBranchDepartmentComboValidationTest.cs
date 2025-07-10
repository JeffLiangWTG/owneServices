using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccAllowedBranchDepartmentComboValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAAB_GE_Department()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var combo1 = branch.AllowedDepartments.AddNew();
			var combo2 = branch.AllowedDepartments.AddNew();
			combo1.AAB_GE_Department = combo2.AAB_GE_Department = department.PK;

			AssertNoErrors(combo2);
			combo2.RunPreSaveValidation();
			AssertHasError(combo2.AAB_GE_DepartmentInfo, "This department is already added. Please enter another department.");
			combo2.AAB_GE_Department = Factory.NewWithValidTestData<GlbDepartment>().PK;
			AssertNoErrors(combo2);

			var combo3 = branch.AllowedDepartments.AddNew();
			combo3.AAB_GE_Department = ZGuid.NewZGuid(); //not a valid department
			AssertHasError(combo3.AAB_GE_DepartmentInfo, "Enter a valid selection.");
			combo3.AAB_GE_Department = Factory.NewWithValidTestData<GlbDepartment>().PK;
			AssertNoErrors(combo3);
		}
	}
}

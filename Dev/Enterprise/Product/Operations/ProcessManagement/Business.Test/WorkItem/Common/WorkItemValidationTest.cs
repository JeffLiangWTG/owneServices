using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business.Test
{
	public abstract class WorkItemValidationTest<T> : BusinessObjectValidationTestCase
		where T : WorkItemCommon
	{
		public void TestAssignedDepartment()
		{
			var workItem = Factory.NewWithValidTestData<T>();
			GlbDepartment department1 = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			GlbDepartment department2 = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FIA");

			workItem.WKI_GE_AssignedDepartment = department1.PK;
			AssertNoErrors(workItem.WKI_GE_AssignedDepartmentInfo);

			department1.GE_IsActive = false;
			workItem.Validation.ValidateWKI_GE_AssignedDepartment();
			AssertHasErrors(workItem.WKI_GE_AssignedDepartmentInfo);

			workItem.Factory.Save();

			department2.GE_IsActive = false;

			workItem.Validation.ValidateWKI_GE_AssignedDepartment();
			AssertNoErrors(workItem.WKI_GE_AssignedDepartmentInfo);

			workItem.WKI_GE_AssignedDepartment = department2.PK;
			workItem.Validation.ValidateWKI_GE_AssignedDepartment();
			AssertHasErrors("can't set new inactive department", workItem.WKI_GE_AssignedDepartmentInfo);
		}

		public void TestCreatedBy()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_SystemCreateUser = "XXX";
			AssertHasError(workItem.WKI_SystemCreateUserInfo, "Enter a valid selection.");

			workItem.WKI_SystemCreateUser = string.Empty;
			AssertNoErrors(workItem.WKI_SystemCreateUserInfo);

			Factory.Save();
			AssertEquals("Saving the workitem should cause the Created By field to autopopulate", "E", workItem.WKI_SystemCreateUser);

			workItem.WKI_SystemCreateUser = string.Empty;
			AssertHasError(workItem.WKI_SystemCreateUserInfo, "Please enter a value.");
		}
	}
}

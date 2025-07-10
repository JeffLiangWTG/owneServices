using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class DepartmentTest : TestCaseWithFactory
	{
		public void TestDepartment()
		{
			var departmentBizObj = Factory.New<GlbDepartment>();
			departmentBizObj.GE_Code = "FES";
			departmentBizObj.GE_Desc = "Forwarding Export Sea";

			var department = new Department(departmentBizObj);

			AssertEquals("Code", "FES", department.Code);
			AssertEquals("PK", departmentBizObj.PK, department.PK);
			AssertEquals("Description", "Forwarding Export Sea", department.Description);
		}

		public void TestNullCompany()
		{
			var department = new Department(null);

			AssertEquals("Code", ZString.Empty, department.Code);
			AssertEquals("PK", ZGuid.Empty, department.PK);
			AssertEquals("Description", ZString.Empty, department.Description);
		}
	}
}

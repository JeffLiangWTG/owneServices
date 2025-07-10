using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgStaffAssignmentsLookupsImplementer))]
	public class OrgStaffAssignmentsLookupsImplementerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDepartmentList()
		{
			OrgStaffAssignmentsLookupsImplementer helper = OrgStaffAssignmentsLookupsImplementer.Get(Factory);
			Assert("Contains ALL", helper.DepartmentCodes.ContainsCode("ALL"));
			Assert("Contains FRT", helper.DepartmentCodes.ContainsCode("FRT"));
			Assert("Contains SEA", helper.DepartmentCodes.ContainsCode("SEA"));
			Assert("Contains AIR", helper.DepartmentCodes.ContainsCode("AIR"));
			Assert("Contains CFS", helper.DepartmentCodes.ContainsCode("CFS"));
			Assert("Contains LOC", helper.DepartmentCodes.ContainsCode("LOC"));
			Assert("Contains FIS", helper.DepartmentCodes.ContainsCode("FIS"));
			Assert("Contains FEA", helper.DepartmentCodes.ContainsCode("FEA"));

			AssertEquals("ALL - All Services", "All Services", helper.DepartmentCodes.GetDescriptionFromCode("ALL"));
			AssertEquals("FRT - Freight", "Freight Services", helper.DepartmentCodes.GetDescriptionFromCode("FRT"));
			AssertEquals("LOC - Port Transport", "Port Transport Services", helper.DepartmentCodes.GetDescriptionFromCode("LOC"));

			Assert("Doesn't contain XXX", !helper.DepartmentCodes.ContainsCode("XXX"));
		}

		public void TestDepartmentDescTranslatable()
		{
			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TDT";
			department.GE_Desc = "Test Department";
			Factory.Save();

			OrgStaffAssignmentsLookupsImplementer helper = OrgStaffAssignmentsLookupsImplementer.Get(Factory);
			using (var mockData = Res.UseMockData())
			{
				var key = department.GE_DescInfo.CustomizableDataResourceStrings.Source.GetKey(null, "Test Department");
				mockData.Put(key, new ResourceStringData(key, "测试部门"));
				AssertEquals("测试部门", helper.DepartmentCodes.GetMultilingualDescriptionFromCode("TDT"));
			}
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DepartmentMapping))]
	sealed class DepartmentMappingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DepartmentMappingCollection collection = new DepartmentMappingCollection(Factory);
			DepartmentMapping bizO = new DepartmentMapping("FEA", "CEA", collection);
			return bizO;
		}

		public void TestMapping()
		{
			DepartmentMapping elem = (DepartmentMapping)GetNewBusinessObject();
			elem.Dept1Code = "FEA";
			elem.Dept2Code = "CEA";
			AssertEquals("Dept1", "FEA", elem.Dept1Code);
			AssertEquals("Dept2", "CEA", elem.Dept2Code);

			DepartmentMapping elem2 = DepartmentMapping.FromString(elem.ToString(), null);
			AssertEquals("Dept1", "FEA", elem2.Dept1Code);
			AssertEquals("Dept2", "CEA", elem2.Dept2Code);
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DepartmentMappingCollection))]
	sealed class DepartmentMappingTestCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DepartmentMappingCollection>
	{
		protected override DepartmentMappingCollection GetCollectionToTest()
		{
			return new DepartmentMappingCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DepartmentMapping("Dept1", "Dept2", new DepartmentMappingCollection(Factory));
		}

		public void TestGetMapping()
		{
			DepartmentMappingCollection collection = new DepartmentMappingCollection("");
			AssertEquals(ZString.Empty, collection.GetMapping("ZUB"));

			collection = new DepartmentMappingCollection("ZUB|RAK");
			AssertEquals("RAK", collection.GetMapping("ZUB"));
			AssertEquals(ZString.Empty, collection.GetMapping("BOB"));

			collection = new DepartmentMappingCollection("ZUB|RAK;BOB|JON");
			AssertEquals("RAK", collection.GetMapping("ZUB"));
			AssertEquals("JON", collection.GetMapping("BOB"));
		}
	}
}

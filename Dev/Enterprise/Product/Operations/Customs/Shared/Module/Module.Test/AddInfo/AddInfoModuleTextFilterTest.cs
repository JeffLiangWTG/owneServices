using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(AddInfoModuleTextFilter))]
	sealed class AddInfoModuleTextFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AddInfoModuleTextFilter("TestDescription", DummyBizoSchema.Z0_Description, "TestAddInfo");
		}

		public void TestConstructor()
		{
			GetTextQueryWithOperator testQuery = (c, v) => new ZQuery();
			var tester = new AddInfoModuleTextFilter("DESC", testQuery);
			AssertNotNull(tester);
			AssertEquals(testQuery, tester.QueryDelegate);
		}
	}
}

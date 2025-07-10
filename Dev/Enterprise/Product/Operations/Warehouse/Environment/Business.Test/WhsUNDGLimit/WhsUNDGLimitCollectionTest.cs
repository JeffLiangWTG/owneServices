using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsUNDGLimitCollection))]
	class WhsUNDGLimitCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsUNDGLimitCollection>
	{
		public void TestWarehouse()
		{
			var collection = GetCollectionToTest();
			Assert("Warehouse == Master in base", object.ReferenceEquals(collection.Relationship.Master, collection.Warehouse));
		}

		#region Implementation

		protected override WhsUNDGLimitCollection GetCollectionToTest()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateTRWWarehouse("AAA");
			return new WhsUNDGLimitCollection(warehouse);
		}

		#endregion
	}
}

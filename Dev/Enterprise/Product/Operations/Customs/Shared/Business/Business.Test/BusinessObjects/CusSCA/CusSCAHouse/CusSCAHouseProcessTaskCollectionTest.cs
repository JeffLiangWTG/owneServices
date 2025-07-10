using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSCAHouseProcessTaskCollection))]
	sealed class CusSCAHouseProcessTaskCollectionTest : ProcessTaskCollectionTest<CusSCAHouseProcessTaskCollection>
	{
		protected override CusSCAHouseProcessTaskCollection GetCollectionToTestCore()
		{
			return new CusSCAHouseProcessTaskCollection(Factory.New<TestCusSCAHouse>());
		}

		public void TestParent()
		{
			var parent = Factory.New<TestCusSCAHouse>();
			AssertEquals(parent, ((IWorkflowProvider)parent).WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			var parent = Factory.New<TestCusSCAHouse>();
			var collection = (CusSCAHouseProcessTaskCollection)((IWorkflowProvider)parent).WorkflowItems;
			var task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}
	}
}

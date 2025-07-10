using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(RefContainerStockProcessTaskCollection))]
	internal class RefContainerStockProcessTaskCollectionTest : ProcessTaskCollectionTest<RefContainerStockProcessTaskCollection>
	{
		public void TestParent()
		{
			RefContainerStock parent = Factory.New<RefContainerStock>();
			AssertEquals(parent, ((IWorkflowProvider)parent).WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			RefContainerStock parent = Factory.New<RefContainerStock>();
			RefContainerStockProcessTaskCollection collection = (RefContainerStockProcessTaskCollection)((IWorkflowProvider)parent).WorkflowItems;
			RefContainerStockProcessTask task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}

		protected override RefContainerStockProcessTaskCollection GetCollectionToTestCore()
		{
			RefContainerStock parent = Factory.New<RefContainerStock>();
			return new RefContainerStockProcessTaskCollection(parent);
		}
	}
}

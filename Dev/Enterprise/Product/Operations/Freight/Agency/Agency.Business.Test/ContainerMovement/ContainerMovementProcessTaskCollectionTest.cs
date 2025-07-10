using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovementProcessTaskCollection))]
	internal class ContainerMovementProcessTaskCollectionTest : ProcessTaskCollectionTest<ContainerMovementProcessTaskCollection>
	{
		public void TestParent()
		{
			var parent = Factory.New<ContainerMovement>();
			AssertEquals(parent, ((IWorkflowProvider)parent).WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			var parent = Factory.New<ContainerMovement>();
			var collection = (ContainerMovementProcessTaskCollection)((IWorkflowProvider)parent).WorkflowItems;
			var task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}

		protected override ContainerMovementProcessTaskCollection GetCollectionToTestCore()
		{
			var parent = Factory.New<ContainerMovement>();
			return new ContainerMovementProcessTaskCollection(parent);
		}
	}
}

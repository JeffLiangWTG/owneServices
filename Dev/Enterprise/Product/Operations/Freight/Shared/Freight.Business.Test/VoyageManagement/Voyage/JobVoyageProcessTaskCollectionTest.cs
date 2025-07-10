using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobVoyageProcessTaskCollection))]
	sealed class JobVoyageProcessTaskCollectionTest : ProcessTaskCollectionTest<JobVoyageProcessTaskCollection>
	{
		public void TestParent()
		{
			IWorkflowProvider voyage = Factory.New<JobVoyage>();
			AssertEquals(voyage, voyage.WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			IWorkflowProvider voyage = Factory.New<JobVoyage>();
			var collection = (JobVoyageProcessTaskCollection)voyage.WorkflowItems;
			var task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}

		protected override JobVoyageProcessTaskCollection GetCollectionToTestCore()
		{
			var voyage = Factory.New<JobVoyage>();
			return new JobVoyageProcessTaskCollection(voyage);
		}
	}
}

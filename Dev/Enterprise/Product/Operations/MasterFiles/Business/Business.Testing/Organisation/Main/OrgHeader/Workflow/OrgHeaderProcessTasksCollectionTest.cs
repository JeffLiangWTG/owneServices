using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderProcessTasksCollection))]
	sealed class OrgHeaderProcessTasksCollectionTest : ProcessTaskCollectionTest<OrgHeaderProcessTasksCollection>
	{
		public void TestAddNewProcessTask()
		{
			OrgHeaderProcessTasksCollection collection = GetCollectionToTestCore();
			AssertEquals(typeof(OrgHeaderProcessTask), collection.AddNew().GetType());
		}

		#region Implementation

		protected override OrgHeaderProcessTasksCollection GetCollectionToTestCore()
		{
			return (OrgHeaderProcessTasksCollection)Factory.NewWithValidTestData<OrgHeader>().WorkflowItems;
		}

		#endregion
	}
}

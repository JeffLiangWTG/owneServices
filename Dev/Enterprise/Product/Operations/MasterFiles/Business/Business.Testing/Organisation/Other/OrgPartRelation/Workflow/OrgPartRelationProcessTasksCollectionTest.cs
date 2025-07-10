using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartRelationProcessTasksCollection))]
	sealed class OrgPartRelationProcessTasksCollectionTest : ProcessTaskCollectionTest<OrgPartRelationProcessTasksCollection>
	{
		#region TestAddNewProcessTask

		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(OrgPartRelationProcessTask), collection.AddNew().GetType());
		}

		#endregion

		#region Implementation

		protected override OrgPartRelationProcessTasksCollection GetCollectionToTestCore()
		{
			return (OrgPartRelationProcessTasksCollection)Factory.NewWithValidTestData<OrgPartRelation>().WorkflowItems;
		}

		#endregion
	}
}

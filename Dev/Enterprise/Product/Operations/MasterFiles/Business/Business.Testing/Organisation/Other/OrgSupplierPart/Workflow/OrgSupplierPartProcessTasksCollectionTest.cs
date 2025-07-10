using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartProcessTasksCollection))]
	sealed class OrgSupplierPartProcessTasksCollectionTest : ProcessTaskCollectionTest<OrgSupplierPartProcessTasksCollection>
	{
		#region TestAddNewProcessTask

		public void TestAddNewProcessTask()
		{
			var collection = GetCollectionToTestCore();
			AssertEquals(typeof(OrgSupplierPartProcessTask), collection.AddNew().GetType());
		}

		#endregion

		#region Implementation

		protected override OrgSupplierPartProcessTasksCollection GetCollectionToTestCore()
		{
			return (OrgSupplierPartProcessTasksCollection)Factory.NewWithValidTestData<OrgSupplierPart>().WorkflowItems;
		}

		#endregion
	}
}

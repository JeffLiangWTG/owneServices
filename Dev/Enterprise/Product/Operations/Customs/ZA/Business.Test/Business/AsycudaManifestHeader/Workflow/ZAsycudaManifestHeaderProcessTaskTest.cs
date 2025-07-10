using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ZAsycudaManifestHeaderProcessTask))]
	sealed class ZAsycudaManifestHeaderProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			var task = (ZAsycudaManifestHeaderProcessTask)GetNewBusinessObject();
			AssertNotNull("GetNewBusinessObject() should return valid ProcessTask", task);
			AssertNotNull("ParentControllerID should be overriden to return correct ID for none standalone task, or override this test to assert true", task.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return header.WorkflowItems.AddNew();
		}
	}
}

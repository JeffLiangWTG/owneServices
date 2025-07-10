using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTaskExtraResource))]
	sealed class ProcessTaskExtraResourceTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			DummyWithWorkflow dummy = factory.New<DummyWithWorkflow>();
			ProcessTask processTask = dummy.WorkflowItems.AddNew();
			ProcessTaskExtraResource extraResource = processTask.ExtraResources.AddNew();

			return extraResource;
		}

		#endregion

		public void TestReadOnlyMatchesProcessTaskTrue()
		{
			AssertReadOnlyMatchesProcessTask(true);
		}

		public void TestReadOnlyMatchesProcessTaskFalse()
		{
			AssertReadOnlyMatchesProcessTask(false);
		}

		void AssertReadOnlyMatchesProcessTask(bool readOnly)
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var processTask = dummy.WorkflowItems.AddNew();
			var extraResource = processTask.ExtraResources.AddNew();

			processTask.ReadOnly = readOnly;
			AssertEquals(readOnly, extraResource.ReadOnly);
		}
	}
}

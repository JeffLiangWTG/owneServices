using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequenceProcessTask))]
	sealed class AccComplianceSequenceProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			AccComplianceSequenceProcessTask processTask = ((AccComplianceSequenceProcessTaskCollection)sequence.WorkflowItems).AddNew();

			AssertEquals("Parent", sequence, processTask.Parent);
			AssertEquals("ParentControllerID", ControllerIDs.AccComplianceSequence, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<AccComplianceSequence>().WorkflowItems.AddNew();
		}

		#endregion
	}
}

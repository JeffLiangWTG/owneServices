using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class JobVoyageWorkflowUserControlTest : TestCaseWithFactory
	{
		public void TestGetStmALogFilterStripBusinessObjectOverriddenCorrectly()
		{
			using (var jobVoyageWorkflowUserControl = new TestJobVoyageWorkflowUserControl())
			{
				AssertNotNull("Precondition", jobVoyageWorkflowUserControl.GetStmALogFilterStripBusinessObjectExposed);

				var jobVoyage = Factory.NewWithValidTestData<JobVoyage>();
				var jobVoyageLogFilterBusinessObject = jobVoyageWorkflowUserControl.GetStmALogFilterStripBusinessObjectExposed.Invoke(jobVoyage);

				Assert(
					"JobVoyageWorkflowUserControl should have GetStmALogFilterStripBusinessObject overridden so that it returns a JobVoyageLogFilterBusinessObject.",
					jobVoyageWorkflowUserControl.GetStmALogFilterStripBusinessObjectExposed.Invoke(jobVoyage) is JobVoyageLogFilterBusinessObject
				);
			}
		}
	}
}

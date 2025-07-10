using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class JobVoyageWorkflowTabPageTest : TestCaseWithFactory
	{
		public void TestTrackingUserControlIsCorrectlyOverridden()
		{
			using (var jobVoyageWorkflowTabPage = new TestJobVoyageWorkflowTabPage())
			{
				Assert(jobVoyageWorkflowTabPage.TrackingUserControlExposed is JobVoyageWorkflowUserControl);
			}
		}
	}
}

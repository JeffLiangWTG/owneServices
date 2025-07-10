using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.GUI.Consol.UserControls;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingConsolTabPageTest : TestCaseWithFactory
	{
		public void TestTrackingUserControlIsCorrectlyOverridden()
		{
			using (var workflowTabPage = new TestForwardingConsolWorkflowTabPage())
			{
				AssertType(typeof(ForwardingConsolWorkflowUserControl), workflowTabPage.TrackingUserControlExposed);
			}
		}
	}

	#region Implementation

	class TestForwardingConsolWorkflowTabPage : ForwardingConsolWorkflowTabPage
	{
		public ZWorkflowUserControl TrackingUserControlExposed => base.TrackingUserControl;
	}

	#endregion
}

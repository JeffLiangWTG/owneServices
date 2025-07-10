using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingShipmentTabPageTest : TestCaseWithFactory
	{
		public void TestTrackingUserControlIsCorrectlyOverridden()
		{
			using (var workflowTabPage = new TestForwardingShipmentWorkflowTabPage())
			{
				AssertType(typeof(ForwardingShipmentWorkflowUserControl), workflowTabPage.TrackingUserControlExposed);
			}
		}
	}

	#region Implementation

	class TestForwardingShipmentWorkflowTabPage : ForwardingShipmentWorkflowTabPage
	{
		public ZWorkflowUserControl TrackingUserControlExposed => base.TrackingUserControl;
	}

	#endregion
}

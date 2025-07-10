using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class TestJobVoyageWorkflowTabPage : JobVoyageWorkflowTabPage
	{
		public ZWorkflowUserControl TrackingUserControlExposed => base.TrackingUserControl;
	}
}

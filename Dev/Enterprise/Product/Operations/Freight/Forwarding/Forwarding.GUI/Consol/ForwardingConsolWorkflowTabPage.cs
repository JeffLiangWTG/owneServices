using Enterprise.Freight.Forwarding.GUI.Consol.UserControls;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.GUI
{
	public class ForwardingConsolWorkflowTabPage : ZWorkflowTabPage
	{
		protected override ZWorkflowUserControl TrackingUserControl
		{
			get
			{
				if (trackingUserControl == null)
				{
					trackingUserControl = new ForwardingConsolWorkflowUserControl();
				}

				return trackingUserControl;
			}
		}

		ForwardingConsolWorkflowUserControl trackingUserControl;
	}
}

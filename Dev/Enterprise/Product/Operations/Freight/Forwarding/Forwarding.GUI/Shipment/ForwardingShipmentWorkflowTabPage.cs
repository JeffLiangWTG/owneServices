using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.GUI
{
	public class ForwardingShipmentWorkflowTabPage : ZWorkflowTabPage
	{
		protected override ZWorkflowUserControl TrackingUserControl
		{
			get
			{
				if (trackingUserControl == null)
				{
					trackingUserControl = new ForwardingShipmentWorkflowUserControl();
				}

				return trackingUserControl;
			}
		}

		ForwardingShipmentWorkflowUserControl trackingUserControl;
	}
}

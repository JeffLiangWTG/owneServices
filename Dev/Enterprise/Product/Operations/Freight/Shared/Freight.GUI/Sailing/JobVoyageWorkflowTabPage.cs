using Enterprise.MasterFiles.GUI;

namespace Enterprise.Freight.GUI
{
	public class JobVoyageWorkflowTabPage : ZWorkflowTabPage
	{
		protected override ZWorkflowUserControl TrackingUserControl
		{
			get
			{
				if (trackingUserControl == null)
				{
					trackingUserControl = new JobVoyageWorkflowUserControl();
				}

				return trackingUserControl;
			}
		}

		JobVoyageWorkflowUserControl trackingUserControl;
	}
}

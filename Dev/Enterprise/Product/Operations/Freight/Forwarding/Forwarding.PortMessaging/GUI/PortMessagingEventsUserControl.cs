using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	public class PortMessagingEventsUserControl : FilteredLogsViewUserControl
	{
		public PortMessagingEventsUserControl()
			: base()
		{ }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var manager = (PortMessagingManager)dataSource;

			var logs = manager?.Data?.DakosyLogs;
			base.SetDataBinding(logs, "");
			sourceInfoUserControl.SetDataBinding(logs, "");
		}
	}
}

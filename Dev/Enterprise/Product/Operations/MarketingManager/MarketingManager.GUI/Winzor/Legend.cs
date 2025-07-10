using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class Legend : ZUserControl
	{
		public event EventHandler<TrackingStatusDescriptionEventArgs> FilterByDeliveryStatus;
		public event EventHandler<UnsubscribedStatusDescriptionEventArgs> FilterByUnsubscribeStatus;

		public ZTextBox EmailsTotal { get; }
		public ZTextBox ClientsTotal { get; }
		public bool ClientCountVisible { get; set; }
	}
}

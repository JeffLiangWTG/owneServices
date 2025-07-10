using System;

namespace Enterprise.Services.ServiceHost
{
	public class DeliveryDueDateQuery
	{
		public DateTime PickupDate { get; set; }
		public string ServiceLevel { get; set; }
		public string HBLDlvMode { get; set; }
		public string PickupOrg { get; set; }
		public string PickupAddr { get; set; }
		public string PickupCFSOrg { get; set; }
		public string PickupCFSAddr { get; set; }
		public string DeliveryOrg { get; set; }
		public string DeliveryAddr { get; set; }
		public string DeliveryCFSOrg { get; set; }
		public string DeliveryCFSAddr { get; set; }
		public string TransportMode { get; set; }
		public string DeliveryType { get; set; }
	}
}

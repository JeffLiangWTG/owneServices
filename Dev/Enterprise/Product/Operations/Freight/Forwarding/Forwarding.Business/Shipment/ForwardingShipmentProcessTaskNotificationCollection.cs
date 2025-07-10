using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentProcessTaskNotificationCollection : ProcessTaskNotificationCollection
	{
		public ForwardingShipmentProcessTaskNotificationCollection(ProcessTask master)
			: base(master)
		{
		}

		public new ForwardingShipmentProcessTaskNotification this[int index]
		{
			get { return (ForwardingShipmentProcessTaskNotification)base[index]; }
		}

		public new ForwardingShipmentProcessTaskNotification AddNew()
		{
			return (ForwardingShipmentProcessTaskNotification)base.AddNew();
		}
	}
}

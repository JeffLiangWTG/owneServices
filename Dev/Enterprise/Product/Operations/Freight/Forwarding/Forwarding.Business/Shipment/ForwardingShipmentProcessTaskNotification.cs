using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentProcessTaskNotification : ProcessTaskNotification
	{
		public ForwardingShipmentProcessTaskNotification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}

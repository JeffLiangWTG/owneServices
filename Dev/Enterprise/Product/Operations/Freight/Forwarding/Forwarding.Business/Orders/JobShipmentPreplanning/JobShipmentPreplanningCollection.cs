using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[ModuleID(ModuleId.JobShipmentPreplanning)]
	public class JobShipmentPreplanningCollection : BusinessObjectCollection<JobShipmentPreplanning>
	{
		public JobShipmentPreplanningCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

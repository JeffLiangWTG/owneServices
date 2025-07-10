using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business.GPS;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class GPSEventAllocationCollection : NonPersistentBusinessObjectCollection<GPSEventAllocation>
	{
		public GPSEventAllocationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GPSEventAllocationCollection(IEnumerable<GPSEvent> gpsEvents, CartageLegAllocationManager parent)
		{
			foreach (GPSEvent gpsEvent in gpsEvents)
			{
				this.Add(new GPSEventAllocation(gpsEvent, parent));
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GPSEventAllocation(Factory);
		}
	}
}

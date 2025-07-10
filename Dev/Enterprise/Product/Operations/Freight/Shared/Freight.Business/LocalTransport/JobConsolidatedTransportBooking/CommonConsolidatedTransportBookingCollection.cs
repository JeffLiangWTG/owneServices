using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	[ModuleID(ModuleId.ConsolidatedTransportBooking)]
	public class CommonConsolidatedTransportBookingCollection : ActiveBusinessObjectCollection<CommonConsolidatedTransportBooking>
	{
		public CommonConsolidatedTransportBookingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

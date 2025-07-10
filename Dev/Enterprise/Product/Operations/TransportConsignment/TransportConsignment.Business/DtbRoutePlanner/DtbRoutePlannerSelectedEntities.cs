using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.TransportConsignment.Business
{
	[ImmutableObject(true)]
	public class DtbRoutePlannerSelectedEntities
	{
		public DtbRoutePlannerSelectedEntities()
		{
		}

		public DtbRoutePlannerSelectedEntities(ZGuid runSheetPK, ZGuid carrierPK, ZGuid driverPK, ZGuid vehiclePK)
		{
			RunSheetPK = runSheetPK;
			CarrierPK = carrierPK;
			DriverPK = driverPK;
			VehiclePK = vehiclePK;
		}

		public readonly ZGuid RunSheetPK;
		public readonly ZGuid CarrierPK;
		public readonly ZGuid DriverPK;
		public readonly ZGuid VehiclePK;
	}
}

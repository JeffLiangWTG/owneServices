using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan
{
	public interface IStowPlanSailingData : IEDIMessageCollectionProvider
	{
		ZString VoyageNumber { get; }
		IStowPlanVesselData Vessel { get; }
		ZString Arrival { get; }
		ZDateTime ArrivalTime { get; }
		ZBool IsArrivalTimeEstimated { get; }
		ZString Departure { get; }
		ZDateTime DepartureTime { get; }
		ZBool IsDepartureTimeEstimated { get; }
		IEnumerable<IStowPlanShipmentData> Shipments { get; }
	}
}

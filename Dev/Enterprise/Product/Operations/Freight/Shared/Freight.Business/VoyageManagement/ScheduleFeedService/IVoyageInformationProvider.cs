using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public interface IVoyageInformationProvider
	{
		ZPropertyInfo VoyageNumber { get; }
		ZPropertyInfo CarrierPK { get; }
		ZPropertyInfo VesselPK { get; }
		ZPropertyInfo TransportMode { get; }
		IEnumerable<ITrackableVoyagePort> Origins { get; }
		IEnumerable<ITrackableVoyagePort> Destinations { get; }
	}
}

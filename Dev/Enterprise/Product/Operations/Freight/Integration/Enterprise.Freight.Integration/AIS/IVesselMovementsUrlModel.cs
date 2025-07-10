using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IVesselMovementsUrlModel
	{
		ZString LloydsNumber { get; set; }
		ZDateTime DepartureTime { get; set; }
		ZDateTime ArrivalTime { get; set; }
		ZString CarrierCode { get; set; }
		ZString VoyageNumber { get; set; }
		ZString DeparturePortUnloco { get; set; }
		ZString ArrivalPortUnloco { get; set; }
	}
}

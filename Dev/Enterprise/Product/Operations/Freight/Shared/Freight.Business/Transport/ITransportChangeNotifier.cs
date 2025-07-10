using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface ITransportChangeNotifier
	{
		void NotifyChanged(TransportChangeNotifyType notifyType, Transport transport, IZType previousValue);
	}

	public enum TransportChangeNotifyType
	{
		TransportType,
		Sailing,
		Load,
		Discharge,
		Vessel,
		VoyageFlight,
		ETD,
		ETA,
		ATD,
		ATA,
		TerminalAvailabilityDate,
		TerminalStorageDate,
		DepotAvailabilityDate,
		DepotStorageDate,
		CarrierAddress,
		CarrierBookingRef,
		LegOrder,
		IsCharter,
		TransportMode,
		ArrivalLocation,
		DepartureLocation,
		AircraftType
	}

	public delegate void TransportChangeNotifyHandler(Transport transport, IZType previousValue);
}

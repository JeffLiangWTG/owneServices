using CargoWise.Types;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.Freight.CarbonEmissions.Integration
{
	public interface ICO2ePrePostCarriage
	{
		bool RequiresPrePostCarriageLegs { get; }
		IPrePostCarriageLocation[] GetPreCarriageLocations(ZString hblDeliveryMode);
		IPrePostCarriageLocation[] GetPostCarriageLocations(ZString hblDeliveryMode);
		void OnTransportBookingCalculated(IDtbBooking booking);
		void OnTransportBookingCO2eStatusChanged(IDtbBooking booking);
		void OnTransportBookingActiveStatusChanged(IDtbBooking booking);
	}
}

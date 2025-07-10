using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class TransportLegDataObjectReaderForDomesticTransportTest : TransportLegDataObjectReaderTest
	{
		protected override ITransportParentCommon GetNewShipmentBO()
		{
			return shipmentBO ?? (shipmentBO = (ITransportParentCommon)Factory.BOFactory.New<IDtbBookingConsolidation>());
		}
		ITransportParentCommon shipmentBO;
	}
}

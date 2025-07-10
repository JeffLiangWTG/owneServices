using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class AgencyShipmentTest
	{
		public void TestTransports()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AssertType(typeof(AgencyShipmentTransportCollection), shipment.Transports);
		}

		public void TestTransportSupporter()
		{
			ITransportParent parent = Factory.New<AgencyShipment>();

			AssertType(typeof(AgencyShipmentTransportSupporter<AgencyShipment>), parent.TransportSupporter);
		}

		public void TestTypeCode()
		{
			ITransportParent parent = Factory.New<AgencyShipment>();
			AssertEquals(Constants.TransportParentTypes.AgencyShipment, parent.TypeCode);
		}
	}
}

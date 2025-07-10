using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentTransportSupporterTest : TransportSupporterTestCase<AgencyShipmentTransportSupporter<AgencyShipment>>
	{
		public void TestShippingLine()
		{
			OrgHeader principal1 = Factory.New<OrgHeader>();
			OrgHeader principal2 = Factory.New<OrgHeader>();
			OrgHeader prinicpal3 = Factory.New<OrgHeader>();
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = principal1.PK;
			TransportSupporter supporter = ((ITransportParent)shipment).TransportSupporter;
			AssertEquals(principal1.PK, supporter.ShippingLine);
			supporter.ShippingLine = principal2.PK;
			AssertEquals(principal2.PK, shipment.JS_OH_DeliveryAgent);
			shipment.JS_OH_DeliveryAgent = prinicpal3.PK;
			AssertEquals(prinicpal3.PK, supporter.ShippingLine);
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get
			{
				return Env.Security.RoadDistanceCalculationServiceShipping;
			}
		}

		protected override TransportSupporter GetNewTransportSupporter()
		{
			ITransportParent parent = Factory.New<AgencyShipment>();
			return parent.TransportSupporter;
		}

		protected override ZString TestingCountry
		{
			get
			{
				return Core.Constants.CountryCodes.Australia;
			}
		}
	}
}

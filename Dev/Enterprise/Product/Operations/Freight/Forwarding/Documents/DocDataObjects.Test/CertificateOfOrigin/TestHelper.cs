using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.CertificateOfOrigin
{
	public static class TestHelper
	{
		public static IDisposable RetainShipmentDatesDuringDestinationUpdate(this ForwardingShipment shipment)
		{
			_ = shipment ?? throw new ArgumentNullException(nameof(shipment));
			return new RetainShipmentDatesDisposable(shipment);
		}

		public class RetainShipmentDatesDisposable : IDisposable
		{
			readonly ForwardingShipment shipment;
			readonly ZDateTime arrival;
			readonly ZDateTime departure;

			public RetainShipmentDatesDisposable(ForwardingShipment shipment)
			{
				this.shipment = shipment;
				arrival = shipment.JS_E_ARV;
				departure = shipment.JS_E_DEP;
			}

			public void Dispose()
			{
				shipment.JS_E_ARV = arrival;
				shipment.JS_E_DEP = departure;
			}
		}
	}
}

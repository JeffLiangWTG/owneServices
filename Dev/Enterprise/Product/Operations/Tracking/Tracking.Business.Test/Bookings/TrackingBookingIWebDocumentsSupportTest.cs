using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBooking))]
	public class TrackingBookingIWebDocumentsSupportTest : IWebDocumentsSupportBaseTest
	{
		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_IsBooking = true;
			TrackingBooking result = new TrackingBooking(shipment.PK, Factory, null);

			fExpectedDocRelatedPKs = new ZGuid[] { shipment.PK };

			return result;
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return fExpectedDocRelatedPKs; }
		}
		ZGuid[] fExpectedDocRelatedPKs = System.Array.Empty<ZGuid>();
	}
}

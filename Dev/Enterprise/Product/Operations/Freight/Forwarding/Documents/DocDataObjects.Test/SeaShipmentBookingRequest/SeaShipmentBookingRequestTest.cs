using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(SeaShipmentBookingRequest))]
	sealed class SeaShipmentBookingRequestTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsNVO()
		{
			var request = GetNewBusinessObject() as SeaShipmentBookingRequest;
			Assert("SeaShipmentBookingRequest should be always NVO.", request.IsNVO);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SeaShipmentBookingRequest("ForwardingShipment", "S001")
			{
				Numbers = new List<ReferenceNumber>()
			};
		}
	}
}

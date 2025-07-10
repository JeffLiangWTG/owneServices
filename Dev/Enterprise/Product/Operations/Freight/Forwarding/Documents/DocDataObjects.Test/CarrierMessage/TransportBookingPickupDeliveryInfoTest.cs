using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(TransportBookingPickupDeliveryInfo))]
	sealed class TransportBookingPickupDeliveryInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new TransportBookingPickupDeliveryInfo();
	}
}

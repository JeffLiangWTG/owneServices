using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(EManifest))]
	sealed class EManifestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var eManifest = new EManifest(
				"ForwardingShipment",
				"S0001000",
				"ZZZ");

			eManifest.Transports = new Transports();
			eManifest.Containers = System.Array.Empty<Container>();
			eManifest.Bookings = System.Array.Empty<Booking>();

			return eManifest;
		}
	}
}

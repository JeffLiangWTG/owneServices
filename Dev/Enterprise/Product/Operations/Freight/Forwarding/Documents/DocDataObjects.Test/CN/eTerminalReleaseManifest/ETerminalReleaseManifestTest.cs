using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ETerminalReleaseManifest))]
	sealed class ETerminalReleaseManifestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var context = new CommonContext(Factory.GetCachedReadOnlyFactory());

			var eTerminalReleaseManifest = new ETerminalReleaseManifest(
				"ForwardingConsol",
				"C0001000",
				"ZZZ");

			eTerminalReleaseManifest.Transports = Transports.Create(context, System.Array.Empty<Freight.Business.Transport>());
			eTerminalReleaseManifest.Containers = System.Array.Empty<Container>();
			eTerminalReleaseManifest.Bookings = System.Array.Empty<Booking>();

			return eTerminalReleaseManifest;
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	[TestedType(typeof(CertifiedPickup))]
	sealed class CertifiedPickupTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var certifiedPickup = new CertifiedPickup("ForwardingConsol", "C20210512");

			return certifiedPickup;
		}
	}
}

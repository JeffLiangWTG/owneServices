using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(Cresa))]
	sealed class CresaTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cresa = new Cresa(
				"ForwardingShipment",
				"S0001000");

			cresa.PackingLines = System.Array.Empty<BookingPackingLine>();

			return cresa;
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CPT;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.CPT
{
	[TestedType(typeof(CartaDePorte))]
	sealed class CartaDePorteTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cartaDePorte = new CartaDePorte(
				"ForwardingShipment",
				"S0001000");

			return cartaDePorte;
		}
	}
}

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(ShippingOrder))]
	sealed class ShippingOrderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var shippingOrder = new ShippingOrder(
				"ForwardingConsol",
				"C0001000",
				"ZZZ");

			shippingOrder.Transports = Transports.Create(new CommonContext(Factory.GetCachedReadOnlyFactory()), new List<Freight.Business.Transport>());
			shippingOrder.Containers = System.Array.Empty<Container>();

			return shippingOrder;
		}
	}
}

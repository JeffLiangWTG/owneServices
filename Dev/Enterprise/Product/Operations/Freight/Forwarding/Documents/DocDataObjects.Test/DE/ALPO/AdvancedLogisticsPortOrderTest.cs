using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DE.Testing
{
	[TestedType(typeof(AdvancedLogisticsPortOrder))]
	sealed class AdvancedLogisticsPortOrderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var advancedLogisticsPortOrder = new AdvancedLogisticsPortOrder(
					"ForwardingConsol",
					"C00001015");

			advancedLogisticsPortOrder.Containers = System.Array.Empty<Container>();

			return advancedLogisticsPortOrder;
		}
	}
}

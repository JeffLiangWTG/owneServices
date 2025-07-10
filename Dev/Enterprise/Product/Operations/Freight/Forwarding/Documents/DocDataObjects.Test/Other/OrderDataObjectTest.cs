using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(OrderDataObject))]
	sealed class OrderDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrderDataObject(ZGuid.NewZGuid())
			{
				OrderLines = System.Array.Empty<OrderLineDataObject>()
			};
		}
	}
}

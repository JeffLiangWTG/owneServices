using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	[TestedType(typeof(DeliveryOrderDocDataObject))]
	sealed class DeliveryOrderDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new DeliveryOrderDocDataObject("ForwardingShipment", "S0001000", Factory);
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	[TestedType(typeof(GatePassMovementDocDataObject))]
	sealed class GatePassMovementDocDataObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new GatePassMovementDocDataObject("ForwardingShipment", "S0001001", Factory);
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	[TestedType(typeof(PortbaseShipment))]
	sealed class PortbaseShipmentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new PortbaseShipment();
	}
}

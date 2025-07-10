using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	[TestedType(typeof(ShippersSecurityEndorsement))]
	sealed class ShippersSecurityEndorsementTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShippersSecurityEndorsement("ForwardingShipment", "S001")
			{
			};
		}
	}
}

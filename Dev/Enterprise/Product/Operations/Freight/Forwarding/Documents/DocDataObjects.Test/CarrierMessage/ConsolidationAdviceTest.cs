using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(ConsolidationAdvice))]
	sealed class ConsolidationAdviceTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var consolidationAdvice = new ConsolidationAdvice(
				"ForwardingShipment",
				"SHP000001");

			consolidationAdvice.Transports = new Transports();
			consolidationAdvice.SubShipments = new[] { new SubShipment(ZGuid.NewZGuid()) };

			return consolidationAdvice;
		}
	}
}

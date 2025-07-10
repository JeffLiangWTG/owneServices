using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class GatePassMovementProviderFactoryTest : TestCaseWithFactory
	{
		public void TestCreateGatePassMovementProvider()
		{
			AssertType<ForwardingShipmentGatePassMovementProvider>(Factory.New<ForwardingShipment>().GatePassMovementProvider);
			AssertType<ForwardingConsolGatePassMovementProvider>(Factory.New<ForwardingConsol>().GatePassMovementProvider);
		}
	}
}

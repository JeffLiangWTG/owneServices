using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TransitReceiveConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.TransitReceive;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceTransitWarehouse; }
		}

		public override void TestExcludeFromClientVisibleOption()
		{
			AssertEquals("Receives should not have ExcludeFromClientVisibleOption available", true, ConsumerType.ExcludeFromClientVisibleOption);
		}
	}
}

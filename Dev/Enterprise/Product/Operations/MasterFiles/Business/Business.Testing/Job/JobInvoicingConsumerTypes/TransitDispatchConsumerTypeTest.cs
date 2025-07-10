using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TransitDispatchConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.TransitDispatch;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceTransitWarehouse; }
		}

		public override void TestExcludeFromClientVisibleOption()
		{
			AssertEquals("Dispatch should not have ExcludeFromClientVisibleOption available", true, ConsumerType.ExcludeFromClientVisibleOption);
		}
	}
}

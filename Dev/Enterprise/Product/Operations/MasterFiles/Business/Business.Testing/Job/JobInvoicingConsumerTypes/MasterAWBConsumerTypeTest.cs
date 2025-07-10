using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MasterAWBConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.MasterAWB;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}
		public override void TestSupportsWiseRates()
		{
			AssertEquals(true, ConsumerType.SupportsWiseRates);
		}
	}
}

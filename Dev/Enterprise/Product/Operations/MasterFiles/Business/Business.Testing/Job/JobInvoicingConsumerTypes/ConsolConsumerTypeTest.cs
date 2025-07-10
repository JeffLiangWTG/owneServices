using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ConsolConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.Consol;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		public override void TestIsTransportModeSupported()
		{
			AssertEquals(true, ConsumerType.IsTransportModeSupported);
		}

		public override void TestIsDirectionSupported()
		{
			AssertEquals(true, ConsumerType.IsDirectionSupported);
		}
		public override void TestSupportsWiseRates()
		{
			AssertEquals(true, ConsumerType.SupportsWiseRates);
		}
	}
}

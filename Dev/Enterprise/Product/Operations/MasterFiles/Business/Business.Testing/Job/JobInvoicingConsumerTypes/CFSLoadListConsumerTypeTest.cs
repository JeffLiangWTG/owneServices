using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CFSLoadListConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public override void TestIsTransportModeSupported()
		{
			AssertEquals(true, ConsumerType.IsTransportModeSupported);
		}

		public override void TestIsDirectionSupported()
		{
			AssertEquals(true, ConsumerType.IsDirectionSupported);
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.CFSLoadList;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCFS; }
		}

		public override void TestSupportsWiseRates()
		{
			AssertEquals(true, ConsumerType.SupportsWiseRates);
		}
	}
}

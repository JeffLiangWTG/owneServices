using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CusMAWBConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.CusMAWB;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}

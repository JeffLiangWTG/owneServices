using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WarehouseStocktakeConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.WarehouseStocktake;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceWarehouse; }
		}
	}
}

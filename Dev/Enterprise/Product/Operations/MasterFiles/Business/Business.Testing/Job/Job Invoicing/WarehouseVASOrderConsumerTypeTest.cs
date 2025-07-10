using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class WarehouseVASOrderConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public void TestBizoType()
		{
			var warehouseVASOrderConsumeType = JobInvoicingConsumerTypes.WarehouseVASOrder;
			AssertEquals(ObjectFactory.GetType<IWhsVASOrder>(), warehouseVASOrderConsumeType.BizoType);
		}

		#region Implementation

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceWarehouse;

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType() => JobInvoicingConsumerTypes.WarehouseVASOrder;

		#endregion
	}
}

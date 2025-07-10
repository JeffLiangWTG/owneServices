using CargoWise.Application;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WarehouseAdHocServiceJobConsumerTypeTest : JobInvoicingConsumerTypesTest
	{
		public void TestBizoType()
		{
			var warehouseAdHocServiceJobConsumeType = JobInvoicingConsumerTypes.WarehouseAdHocServiceJob;
			AssertEquals(ObjectFactory.GetType<IWhsAdHocServiceJob>(), warehouseAdHocServiceJobConsumeType.BizoType);
		}
	}
}

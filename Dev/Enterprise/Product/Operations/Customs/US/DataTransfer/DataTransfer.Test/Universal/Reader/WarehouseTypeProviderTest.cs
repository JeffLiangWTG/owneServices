using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class WarehouseTypeProviderTest : TestCaseWithFactory
	{
		public void TestGetWarehouseType()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			IWarehouseTypeProvider provider = new WarehouseTypeProvider(shipment);
			AssertEquals(WarehouseType.Default, provider.GetWarehouseType());
			shipment.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
			AssertEquals(WarehouseType.Default, provider.GetWarehouseType());
			shipment.MessageType.Code = JobMessageTypeList.Codes.FTZ;
			AssertEquals(WarehouseType.FreeTradeZone, provider.GetWarehouseType());
		}
	}
}

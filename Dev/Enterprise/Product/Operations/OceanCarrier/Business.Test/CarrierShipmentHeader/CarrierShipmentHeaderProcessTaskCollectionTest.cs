using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderProcessTaskCollection))]
	sealed class CarrierShipmentHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<CarrierShipmentHeaderProcessTaskCollection>
	{
		public void TestParent() => AssertType<CarrierShipmentHeader>("Collection Parent Type", Collection.Parent);

		protected override CarrierShipmentHeaderProcessTaskCollection GetCollectionToTestCore() =>
			new CarrierShipmentHeaderProcessTaskCollection(CarrierShipmentHeader);

		CarrierShipmentHeader CarrierShipmentHeader => carrierShipmentHeader ?? (carrierShipmentHeader = Factory.NewWithValidTestData<CarrierShipmentHeader>());
		CarrierShipmentHeader carrierShipmentHeader;
	}
}

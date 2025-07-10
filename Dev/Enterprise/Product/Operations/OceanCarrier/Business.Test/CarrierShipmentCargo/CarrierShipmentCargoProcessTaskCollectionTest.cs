using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargoProcessTaskCollection))]
	sealed class CarrierShipmentProcessCargoTaskCollectionTest : ProcessTaskCollectionTest<CarrierShipmentCargoProcessTaskCollection>
	{
		public void TestParent() => AssertType<CarrierShipmentCargo>("Collection Parent Type", Collection.Parent);

		protected override CarrierShipmentCargoProcessTaskCollection GetCollectionToTestCore() =>
			new CarrierShipmentCargoProcessTaskCollection(CarrierShipmentCargo);

		CarrierShipmentCargo CarrierShipmentCargo => carrierShipmentCargo ?? (carrierShipmentCargo = Factory.New<CarrierShipmentCargo>());
		CarrierShipmentCargo carrierShipmentCargo;
	}
}

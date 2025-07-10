using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargoCollection))]
	sealed class CarrierShipmentCargoCollectionTest : ActiveBusinessObjectCollectionTestCase<CarrierShipmentCargoCollection>
	{
		protected override CarrierShipmentCargoCollection GetCollectionToTest() => new CarrierShipmentCargoCollection(Factory.New<CarrierShipmentHeader>());
	}
}

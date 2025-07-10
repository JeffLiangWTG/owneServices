using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderCollection))]
	sealed class CarrierShipmentHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CarrierShipmentHeaderCollection>
	{
	}
}

using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyageCollection))]
	sealed class CarrierVoyageCollectionTest : ActiveBusinessObjectCollectionTestCase<CarrierVoyageCollection>
	{
	}
}

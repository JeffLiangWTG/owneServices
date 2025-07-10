using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderHazmatCollection))]
	sealed class DeliveryOrderHazmatCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new DeliveryOrderHazmatCollection(Factory.New<DeliveryOrderHeader>());
	}
}

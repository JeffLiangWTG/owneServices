using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderLineCollection))]
	sealed class DeliveryOrderLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new DeliveryOrderLineCollection(Factory.New<DeliveryOrderHeader>());
	}
}

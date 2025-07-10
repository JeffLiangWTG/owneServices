using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ParentShipmentOuterPackLineCollection))]
	sealed class ParentShipmentOuterPackLineCollectionTest : ActiveBusinessObjectCollectionTestCase<ParentShipmentOuterPackLineCollection>
	{
		protected override ParentShipmentOuterPackLineCollection GetCollectionToTest()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			return new ParentShipmentOuterPackLineCollection(Factory, shipment);
		}
	}
}

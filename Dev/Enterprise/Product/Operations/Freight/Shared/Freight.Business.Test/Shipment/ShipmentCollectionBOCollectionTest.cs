using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentCollection))]
	sealed class ShipmentCollectionBOCollectionTest : BusinessObjectCollectionTestCase //ShipmentCollectionBOCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ShipmentCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return CommonShipment.New(Factory);
		}
	}
}

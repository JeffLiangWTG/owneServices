using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentRoutingCollection))]
	sealed class ShipmentRoutingCollectionBOTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ShipmentRoutingCollection(Factory.New<CommonShipment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Transport transport = Factory.New<Transport>();
			transport.ParentType = typeof(CommonShipment);
			return transport;
		}

		#endregion
	}
}

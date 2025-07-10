using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(TopLevelShipmentCollection))]
	sealed class TopLevelShipmentCollectionBOTest : TopLevelShipmentCollectionBOTest<TopLevelShipmentCollection>
	{
		protected override TopLevelShipmentCollection GetCollectionToTest()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			return new TopLevelShipmentCollection(consol.Shipments, consol);
		}
	}
}

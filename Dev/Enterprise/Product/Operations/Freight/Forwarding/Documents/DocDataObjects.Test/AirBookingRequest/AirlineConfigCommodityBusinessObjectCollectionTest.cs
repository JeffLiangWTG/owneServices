using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(AirlineConfigCommodityBusinessObjectCollection))]
	sealed class AirlineConfigCommodityBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AirlineConfigCommodityBusinessObjectCollection>
	{
		protected override AirlineConfigCommodityBusinessObjectCollection GetCollectionToTest()
		{
			return new AirlineConfigCommodityBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AirlineConfigCommodityBusinessObject(new AirlineConfigCommodity
			{
				Code = "AAA",
				Description = "AAA Desc"
			});
		}
	}
}

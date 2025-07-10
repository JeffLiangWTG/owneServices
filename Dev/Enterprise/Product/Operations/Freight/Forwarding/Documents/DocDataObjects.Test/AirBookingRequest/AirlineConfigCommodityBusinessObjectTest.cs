using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(AirlineConfigCommodityBusinessObject))]
	sealed class AirlineConfigCommodityBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AirlineConfigCommodityBusinessObject(new AirlineConfigCommodity
			{
				Code = "AAA",
				Description = "AAA Desc"
			});
		}
	}
}

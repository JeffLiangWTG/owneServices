using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyContainerCommodityCodeCollection))]
	internal class AgencyContainerCommodityCodeCollectionTest : ContainerCommodityCodeCollectionTest<AgencyContainerCommodityCodeCollection>
	{
		public override void TestSetDefaultsForNewElementCore()
		{
			var collection = GetCollectionToTest();
			var element = collection.AddNew();
			Assert(element.RH_IsShipping);
			Assert(!element.RH_IsForwarding);
		}

		public override void TestCreateRelationshipFilter()
		{
			var commodityType1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityType2 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityType3 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityType1.RH_IsForwarding = true;
			commodityType2.RH_IsShipping = true;
			commodityType3.RH_IsForwarding = true;
			commodityType3.RH_IsShipping = true;
			Factory.Save();
			var collection = GetCollectionToTest();
			AssertCollectionContains(commodityType2, collection);
			AssertCollectionContains(commodityType3, collection);
			AssertCollectionNotContains(commodityType1, collection);
		}

		protected override AgencyContainerCommodityCodeCollection GetNewCollectionCore()
		{
			return new AgencyContainerCommodityCodeCollection(Factory);
		}
	}
}

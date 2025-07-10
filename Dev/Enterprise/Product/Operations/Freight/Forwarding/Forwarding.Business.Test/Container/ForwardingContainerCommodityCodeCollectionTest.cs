using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingContainerCommodityCodeCollection))]
	sealed class ForwardingContainerCommodityCodeCollectionTest : ContainerCommodityCodeCollectionTest<ForwardingContainerCommodityCodeCollection>
	{
		public override void TestSetDefaultsForNewElementCore()
		{
			var collection = GetCollectionToTest();
			var element = collection.AddNew();
			Assert(element.RH_IsForwarding);
			Assert(!element.RH_IsShipping);
		}

		public override void TestCreateRelationshipFilter()
		{
			var commodityType1 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityType2 = Factory.NewWithValidTestData<RefCommodityCode>();
			var commodityType3 = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityType1.RH_IsForwarding = true;
			commodityType2.RH_IsShipping = true;
			commodityType2.RH_IsForwarding = false;
			commodityType3.RH_IsForwarding = true;
			commodityType3.RH_IsShipping = true;

			Factory.Save();

			var collection = GetCollectionToTest();
			AssertCollectionContains(commodityType1, collection);
			AssertCollectionContains(commodityType3, collection);
			AssertCollectionNotContains(commodityType2, collection);
		}

		protected override ForwardingContainerCommodityCodeCollection GetNewCollectionCore()
		{
			return new ForwardingContainerCommodityCodeCollection(Factory);
		}
	}
}

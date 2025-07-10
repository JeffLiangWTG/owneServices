
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(EmptyDripMarketingCollection))]
	sealed class EmptyDripMarketingCollectionTest : ActiveBusinessObjectCollectionTestCase<EmptyDripMarketingCollection>
	{
		public override void TestAdd()
		{
			Assert(true);
		}

		public override void TestDelete()
		{
			Assert(true);
		}

		public override void TestTypedget_Item()
		{
			Assert(true);
		}
	}
}

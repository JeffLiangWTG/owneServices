using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BasePackingGroupCollection))]
	sealed class BasePackingGroupCollectionTest : BasePackingGroupCollectionTest<BasePackingGroupCollection>
	{
		protected override BasePackingGroupCollection GetCollectionToTest()
		{
			return new BasePackingGroupCollection(HouseBill);
		}
	}
}

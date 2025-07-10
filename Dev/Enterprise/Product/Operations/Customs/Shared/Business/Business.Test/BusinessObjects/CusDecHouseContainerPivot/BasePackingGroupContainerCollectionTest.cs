using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BasePackingGroupContainerCollection))]
	sealed class BasePackingGroupContainerCollectionTest : BasePackingGroupContainerCollectionTest<BasePackingGroupContainerCollection>
	{
		protected override BasePackingGroupContainerCollection GetCollectionToTest()
		{
			return new BasePackingGroupContainerCollection(Container);
		}
	}
}

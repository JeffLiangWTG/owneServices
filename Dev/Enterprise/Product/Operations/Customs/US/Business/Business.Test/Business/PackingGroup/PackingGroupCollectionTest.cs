using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PackingGroupCollection))]
	sealed class PackingGroupCollectionTest : Customs.Business.Testing.BasePackingGroupCollectionTest<PackingGroupCollection>
	{
		protected override PackingGroupCollection GetCollectionToTest() => new PackingGroupCollection((Bill)base.HouseBill);
	}
}

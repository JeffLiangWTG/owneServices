using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestsSubclassesOf(typeof(IPackingParentWithPackableItems))]
	public abstract class PackingParentWithPackableItemsTestCase<T> : PackingParentTestCase<T> where T : IPackingParentWithPackableItems
	{
		public void TestPackableItemParentsIsNotNull()
		{
			var parent = GetNewParent();
			AssertNotNull("Packable Items should not be Null.", parent.PackableItemParents);
		}
	}
}

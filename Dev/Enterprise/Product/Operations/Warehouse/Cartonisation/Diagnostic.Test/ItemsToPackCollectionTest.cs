using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	[TestedType(typeof(ItemsToPackCollection))]
	class ItemsToPackCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ItemsToPackCollection>
	{
		protected override ItemsToPackCollection GetCollectionToTest()
		{
			return new ItemsToPackCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyCartonisableItem();
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsDocketFlattenedCollection))]
	internal class WhsDocketFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsDocketFlattenedCollection>
	{
		protected override WhsDocketFlattenedCollection GetCollectionToTest()
		{
			return new WhsDocketFlattenedCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WhsDocketFlattened();
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MultipleNonPersistentItemManager))]
	sealed class MultipleNonPersistentItemManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			return new MultipleNonPersistentItemManager(collection, "Z0_Code");
		}
	}
}

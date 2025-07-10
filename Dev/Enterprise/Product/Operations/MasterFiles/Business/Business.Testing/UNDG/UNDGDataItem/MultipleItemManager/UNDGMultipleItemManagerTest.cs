using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGMultipleItemManager<UNDGDataItem>))]
	sealed class UNDGMultipleItemManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyBizoWithUNDGs>();
			return new UNDGMultipleItemManager<UNDGDataItem>(new UNDGDataItemCollection(dummy), Factory);
		}
	}
}

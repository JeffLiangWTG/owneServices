using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGDataItemCollection.UNDGDataItemStandAloneCollection))]
	sealed class UNDGDataItemStandAloneCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGDataItemCollection.UNDGDataItemStandAloneCollection>
	{
		protected override UNDGDataItemCollection<UNDGDataItem>.UNDGDataItemStandAloneCollection GetCollectionToTest()
		{
			return new UNDGDataItemCollection<UNDGDataItem>.UNDGDataItemStandAloneCollection(Factory, typeof(UNDGDataItem));
		}
	}
}

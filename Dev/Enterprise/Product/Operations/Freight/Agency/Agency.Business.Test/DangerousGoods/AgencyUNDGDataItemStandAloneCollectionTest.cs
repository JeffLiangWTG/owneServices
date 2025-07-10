using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyUNDGDataItemCollection.AgencyUNDGDataItemStandAloneCollection))]
	sealed class AgencyUNDGDataItemStandAloneCollectionTest : ActiveBusinessObjectCollectionTestCase<AgencyUNDGDataItemCollection.AgencyUNDGDataItemStandAloneCollection>
	{
		protected override AgencyUNDGDataItemCollection.AgencyUNDGDataItemStandAloneCollection GetCollectionToTest()
		{
			return new AgencyUNDGDataItemCollection.AgencyUNDGDataItemStandAloneCollection(Factory, typeof(AgencyUNDGDataItem));
		}

		#region

		public new void TestReintroducedAddNewRemovedForGenericCollection()
		{
			Assert(true);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection()
		{
			Assert(true);
		}

		#endregion
	}
}

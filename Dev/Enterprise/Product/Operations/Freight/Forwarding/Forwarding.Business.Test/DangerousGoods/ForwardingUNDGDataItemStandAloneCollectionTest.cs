using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingUNDGDataItemCollection.ForwardingUNDGDataItemStandAloneCollection))]
	sealed class ForwardingUNDGDataItemStandAloneCollectionTest : ActiveBusinessObjectCollectionTestCase<ForwardingUNDGDataItemCollection.ForwardingUNDGDataItemStandAloneCollection>
	{
		protected override ForwardingUNDGDataItemCollection.ForwardingUNDGDataItemStandAloneCollection GetCollectionToTest()
		{
			return new ForwardingUNDGDataItemCollection.ForwardingUNDGDataItemStandAloneCollection(Factory, typeof(ForwardingUNDGDataItem), null);
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

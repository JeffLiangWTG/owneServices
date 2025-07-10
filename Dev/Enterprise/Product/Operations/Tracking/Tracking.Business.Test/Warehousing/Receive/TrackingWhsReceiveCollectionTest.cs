using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReceiveCollection))]
	[HttpContextEnabledTest]
	sealed class TrackingWhsReceiveCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingWhsReceiveCollection>
	{
		protected override TrackingWhsReceiveCollection GetCollectionToTest()
		{
			return new TrackingWhsReceiveCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return TrackingHelper.Get(Factory.New<WhsReceive>());
		}

		public void TestConstructorWithFiltering()
		{
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var receive1 = TrackingHelper.Get(Factory.New<WhsReceive>());
			receive1.WhsReceive.WD_OH_Client = client1.PK;
			var receive2 = TrackingHelper.Get(Factory.New<WhsReceive>());
			receive2.WhsReceive.WD_OH_Client = client1.PK;
			var receive3 = TrackingHelper.Get(Factory.New<WhsReceive>());
			receive3.WhsReceive.WD_OH_Client = client2.PK;

			var filter = new ZQuery(WhsDocketSchema.WD_OH_Client, client1.PK);
			var collection = new TrackingWhsReceiveCollection(Factory);
			collection.Load(filter);
			AssertEquals("Collection should contain 2 Receive", 2, collection.Count);
			AssertCollectionContains("Receive1 should be in Collection", receive1, collection);
			AssertCollectionContains("Receive2 should be in Collection", receive2, collection);
		}
	}
}

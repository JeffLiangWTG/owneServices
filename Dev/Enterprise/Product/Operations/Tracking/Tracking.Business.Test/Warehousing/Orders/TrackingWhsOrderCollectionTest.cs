using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrderCollection))]
	sealed class TrackingWhsOrderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingWhsOrderCollection>
	{
		protected override TrackingWhsOrderCollection GetCollectionToTest()
		{
			return new TrackingWhsOrderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return TrackingHelper.Get(Factory.New<WhsOrder>());
		}

		public void TestLoad()
		{
			var collection = GetCollectionToTest();
			collection.Load();

			AssertEquals(0, collection.Count);

			var order1 = collection.AddNew();
			order1.WhsOrder.FillWithValidTestData();
			var order2 = collection.AddNew();
			order2.WhsOrder.FillWithValidTestData();

			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order3 = TrackingHelper.Get(helper.CreateWhsOrder(data.Org1, data.Whs1, "O1"));

			AssertEquals(2, collection.Count);
			AssertCollectionContains(order1, collection);
			AssertCollectionContains(order2, collection);

			Factory.Save();

			var collectionReload = new TrackingWhsOrderCollection(new BusinessObjectFactory());
			collectionReload.Load();

			AssertEquals(3, collectionReload.Count);

			var rawCollectionReload = collectionReload.ToArray<TrackingWhsOrder>().Select(x => x.WhsOrder.PK).ToArray();
			Assert(rawCollectionReload.Contains(order1.WhsOrder.PK));
			Assert(rawCollectionReload.Contains(order2.WhsOrder.PK));
			Assert(rawCollectionReload.Contains(order3.WhsOrder.PK));
		}

		public void TestConstructorWithFiltering()
		{
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			TrackingWhsOrder order1 = TrackingHelper.Get(Factory.New<WhsOrder>());
			order1.WhsOrder.WD_OH_Client = client1.PK;
			TrackingWhsOrder order2 = TrackingHelper.Get(Factory.New<WhsOrder>());
			order2.WhsOrder.WD_OH_Client = client1.PK;
			TrackingWhsOrder order3 = TrackingHelper.Get(Factory.New<WhsOrder>());
			order3.WhsOrder.WD_OH_Client = client2.PK;

			ZQuery filter = new ZQuery(WhsDocketSchema.WD_OH_Client, client1.PK);
			TrackingWhsOrderCollection collection = new TrackingWhsOrderCollection(Factory);
			collection.Load(filter);
			AssertEquals("Collection should contain 2 orders", 2, collection.Count);
			AssertCollectionContains("Order1 should be in Collection", order1, collection);
			AssertCollectionContains("Order2 should be in Collection", order2, collection);
		}
	}
}

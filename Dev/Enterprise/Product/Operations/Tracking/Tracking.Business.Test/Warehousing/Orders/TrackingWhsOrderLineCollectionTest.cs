using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsOrderLineCollection))]
	sealed class TrackingWhsOrderLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TrackingWhsOrderLineCollection>
	{
		protected override TrackingWhsOrderLineCollection GetCollectionToTest()
		{
			TrackingWhsOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			return new TrackingWhsOrderLineCollection(TrackingWhsOrder);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return TrackingHelper.Get(Factory.New<WhsOrderLine>());
		}

		TrackingWhsOrder TrackingWhsOrder { get; set; }

		public void TestSynchonizedDelete()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = helper.CreateProduct(data.Org1, "MP1");
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var line1 = helper.CreateWhsOrderLine(order, mainProduct.PK, 10m);
			var line2 = helper.CreateWhsOrderLine(order, mainProduct.PK, 12m);
			var line3 = helper.CreateWhsOrderLine(order, mainProduct.PK, 15m);

			AssertEquals(3, order.Lines.Count);

			Factory.Save();

			var collection = CreateLineCollection(order);

			AssertEquals(3, collection.Count);

			collection.RemoveAndDelete(TrackingHelper.Get(line2));

			AssertEquals(2, collection.Count);
			AssertEquals(2, order.Lines.Count);

			Factory.Save();
		}

		public void TestSynchonizedDeleteNewLines()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = helper.CreateProduct(data.Org1, "MP1");
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var line1 = Factory.NewWithValidTestData<WhsOrderLine>();
			var line2 = Factory.NewWithValidTestData<WhsOrderLine>();
			var line3 = Factory.NewWithValidTestData<WhsOrderLine>();

			var trackingOrder = TrackingHelper.Get(order);
			trackingOrder.Lines.Add(TrackingHelper.Get(line1));
			trackingOrder.Lines.Add(TrackingHelper.Get(line2));
			trackingOrder.Lines.Add(TrackingHelper.Get(line3));

			AssertEquals(3, order.Lines.Count);
			AssertEquals(3, trackingOrder.Lines.Count);

			var trackingLine2 = TrackingHelper.Get(line2);
			trackingOrder.Lines.RemoveAndDelete(trackingLine2);

			AssertEquals(2, trackingOrder.Lines.Count);
			AssertEquals(2, order.Lines.Count);

			Factory.Save();

			trackingOrder.Lines.Load();

			AssertEquals(2, trackingOrder.Lines.Count);
			AssertEquals(2, order.Lines.Count);
		}

		TrackingWhsOrderLineCollection CreateLineCollection(WhsOrder order)
		{
			var collection = new TrackingWhsOrderLineCollection(TrackingHelper.Get(LoadOrderFromDb(order.PK)));
			collection.Load();
			return collection;
		}

		WhsOrder LoadOrderFromDb(ZGuid pk)
		{
			var query = new ZDBOnlyQuery(typeof(WhsOrder));
			query.AddToFilter(WhsDocketSchema.PK, pk);
			var order = Factory.LoadTop1<WhsOrder>(query);
			return order;
		}

		public void TestLoad()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = helper.CreateProduct(data.Org1, "MP1");
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var collection = new TrackingWhsOrderLineCollection(TrackingHelper.Get(order));
			collection.Load();

			AssertEquals(0, collection.Count);

			var line1 = collection.AddNew();
			FillLineWithValidTestDate(line1, mainProduct.PK, 10m);
			var line2 = collection.AddNew();
			FillLineWithValidTestDate(line2, mainProduct.PK, 12m);
			var line3 = TrackingHelper.Get(helper.CreateWhsOrderLine(order, mainProduct, 15m));

			AssertEquals(2, collection.Count);
			AssertCollectionContains(line1, collection);
			AssertCollectionContains(line2, collection);

			Factory.Save();

			var collectionReload = new TrackingWhsOrderLineCollection(TrackingHelper.Get(order));
			collectionReload.Load();

			AssertEquals(3, collectionReload.Count);

			var rawCollectionReload = collectionReload.ToArray<TrackingWhsOrderLine>().Select(x => x.WhsOrderLine.PK).ToArray();
			Assert(rawCollectionReload.Contains(line1.WhsOrderLine.PK));
			Assert(rawCollectionReload.Contains(line2.WhsOrderLine.PK));
			Assert(rawCollectionReload.Contains(line3.WhsOrderLine.PK));
		}

		void FillLineWithValidTestDate(TrackingWhsOrderLine orderLine, ZGuid partPK, ZDecimal units)
		{
			orderLine.WhsOrderLine.WE_OP = partPK;
			orderLine.WhsOrderLine.WE_TransactionQuantity = units;
		}

		#region Implementation

		#region Test Setup

		bool oldIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			oldIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = oldIsWeb;
			base.TearDown();
		}

		#endregion

		#endregion
	}
}

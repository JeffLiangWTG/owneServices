using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public abstract class WhsInventoryLineBaseInfoCollectionTestCase<T> : TestCaseWithFactory
	{
		#region TestAdd

		public void TestAdd()
		{
			var collection = GetCollection();
			AssertEquals(0, collection.InventoryLineInfos.Count);
			var info = GetInventoryLineInfo();
			collection.InventoryLineInfos.Add(info);
			AssertEquals(1, collection.InventoryLineInfos.Count);
			AssertEquals(info, collection.InventoryLineInfos[0]);
			AssertEquals(typeof(T), collection.InventoryLineInfos[0].GetType());
		}

		#endregion

		#region TestProductInfos

		public void TestProductInfos()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			var line3 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 3m);

			var lineCollection = GetCollection();
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line1));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line2));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line3));

			AssertEquals(2, lineCollection.ProductInfos.Count);
			AssertContainsExactElementsInAnyOrder(new[] { data.Part1.PK, data.Part2.PK }, lineCollection.ProductInfos.Select(p => p.PK));
		}

		#endregion

		#region TestProductPartAttributesInfos

		public void TestProductPartAttributesInfos()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var line2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			var line3 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var line4 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			var line5 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m);

			var lineCollection = GetCollection();
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line1));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line2));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line3));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line4));
			lineCollection.InventoryLineInfos.Add(GetInventoryLineInfo(lineCollection, line5));

			AssertEquals(2, lineCollection.ProductPartAttributesInfos.Count);
			AssertEquals(1, lineCollection.ProductPartAttributesInfos.Count(pp => pp.ClientPK == data.Org1.PK && pp.ProductPK == data.Part1.PK));
			AssertEquals(1, lineCollection.ProductPartAttributesInfos.Count(pp => pp.ClientPK == data.Org1.PK && pp.ProductPK == data.Part2.PK));
		}

		#endregion

		protected abstract WhsInventoryLineBaseInfoCollection<T> GetCollection();

		protected abstract T GetInventoryLineInfo(WhsInventoryLineBaseInfoCollection<T> collection, WhsInventoryView inventory);

		protected abstract T GetInventoryLineInfo();
	}
}

using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocketGenericCollection))]
	class WhsDocketGenericCollectionTest : WhsDocketCollectionTestCase<WhsDocketGenericCollection>
	{
		#region Test Cases

		public void TestCollectionSupportsMixedDocketTypes()
		{
			var whs1 = Helper.CreateWarehouse("1", "A");
			var whs2 = Helper.CreateWarehouse("2", "A");
			var whs3 = Helper.CreateWarehouse("3", "A");
			var org1 = Helper.CreateClient("1");
			var org2 = Helper.CreateClient("2");
			var org3 = Helper.CreateClient("3");

			var collection = new WhsDocketGenericCollection(Factory);

			collection.Add(Helper.CreateWhsOrder(org1, whs1, "1"));
			collection.Add(Helper.CreateWhsReceive(org2, whs2, "2"));
			collection.Add(Helper.CreateWhsAdjustment(org3.PK, whs3.PK, "3", Notify));

			AssertEquals(3, collection.Count);
			collection.ApplySort("WD_ExternalReference", System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals(typeof(WhsOrder), collection[0].GetType());
			AssertEquals(typeof(WhsReceive), collection[1].GetType());
			AssertEquals(typeof(WhsAdjustment), collection[2].GetType());
		}

		[ExpectException(typeof(System.NotSupportedException))]
		public void TestAddNewNotAllowed()
		{
			var collection = (WhsDocketCollection)GetCollectionToTest();
			collection.AddNew();
		}

		#endregion

		#region Overrides

		protected override WhsDocketGenericCollection GetCollectionToTest()
		{
			return new WhsDocketGenericCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<WhsOrder>();
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override string[] DocketTypesForTest
		{
			get
			{
				return new[]
				{
					DocketType.Codes.Adjustment,
					DocketType.Codes.Order,
					DocketType.Codes.Receive,
					DocketType.Codes.Transfer,
					DocketType.Codes.WorkOrder,
				};
			}
		}

		#endregion
	}
}

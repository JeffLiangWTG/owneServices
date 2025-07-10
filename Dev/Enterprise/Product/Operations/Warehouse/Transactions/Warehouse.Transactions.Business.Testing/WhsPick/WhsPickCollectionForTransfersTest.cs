using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickCollectionForTransfers))]
	public class WhsPickCollectionForTransfersTest : ActiveBusinessObjectCollectionTestCase<WhsPickCollectionForTransfers>
	{
		public void TestPickCollectionForTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var helper = new WhsTestHelperFunctions(Factory);
			var client2 = helper.CreateClient("Tst");
			helper.CreateProductClientRelationShip(client2, data.Part1);
			Factory.Save();

			var receive1 = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 915m);
			Factory.Save();

			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var pick1 = helper.CreatePickNew(order1);
			pick1.FinaliseAllOrders();
			pick1.FinalisePick();

			var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			helper.CreateWhsOrderLine(order2, data.Part2, 10m);
			var pick2 = helper.CreatePickNew(order2);

			var order7 = helper.CreateWhsOrder(client2, data.Whs1, "O7");
			helper.CreateWhsOrderLine(order7, data.Part1, 10m);
			var pick7 = Factory.New<WhsPick>();
			pick7.AddOrders(new[] { order7 });
			Factory.Save();

			var collection = new WhsPickCollectionForTransfers(Factory, data.Whs1.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Collection count correct", 3, collection.Count);
				AssertContainsExactElementsInAnyOrder("Pick collections correct",
					new[] { pick1.PK, pick2.PK, pick7.PK }, collection.Select(p => p.PK));
				AssertContainsExactElementsInAnyOrder("Orders from picks correct", new[] { "O1", "O2", "O7" },
					collection.Select(p => p.Orders[0].WD_ExternalReference));
			});
		}

		public void TestPickCollectionForTransfers_MustBeInSameWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse2 = helper.CreateWarehouse("Whs2", "Row1", 2, 2);
			Factory.Save();

			var order2 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pick2 = helper.CreatePickNew(order2);

			var order4 = helper.CreateWhsOrder(data.Org1, warehouse2, "O4");
			helper.CreateWhsOrderLine(order4, data.Part1, 10m);
			var pick4 = Factory.New<WhsPick>();
			Factory.Save();

			var collection = new WhsPickCollectionForTransfers(Factory, data.Whs1.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Collection count correct", 1, collection.Count);
				AssertContainsExactElementsInAnyOrder("Pick collections correct", new[] { pick2.PK },
					collection.Select(p => p.PK));
				AssertContainsExactElementsInAnyOrder("Orders from picks correct", new[] { "O2" },
					collection.Select(p => p.Orders[0].WD_ExternalReference));
			});
		}

		protected override WhsPickCollectionForTransfers GetCollectionToTest()
		{
			var data = TestDataSimpleEnvironment;
			return new WhsPickCollectionForTransfers(Factory, data.Whs1.PK);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = TestDataSimpleEnvironment;
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1,
				data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), data.Part1, 10m);
			return helper.CreatePickNew(false, false, order);
		}

		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup TestDataSimpleEnvironment
		{
			get
			{
				return testDataSimpleEnvironment ?? (testDataSimpleEnvironment =
					new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory));
			}
		}

		TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup testDataSimpleEnvironment;
	}
}

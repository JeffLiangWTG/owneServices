using System;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsDocketInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsDocketInfo>
	{
		#region TestAdditionalConstructors

		public void TestAdditionalConstructors()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var client = helper.CreateClient("CLIENT");
			var whs = helper.CreateWarehouse("WHS");
			var part = helper.CreateProduct(client, "P1");

			var order1 = helper.CreateWhsOrder(client, whs, "O1");
			helper.CreateWhsOrderLine(order1, part.PK, 5m);

			var order2 = helper.CreateWhsOrder(client, whs, "O2");
			helper.CreateWhsOrderLine(order2, part.PK, 5m);

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(new WhsPickableDocket[] { order1, order2 });
			Factory.Save();

			var docketInfoCollection = new WhsDocketInfoCollection(pick.Orders.Cast<WhsPickableDocket>());
			AssertNotNull(docketInfoCollection);
			AssertEquals(2, docketInfoCollection.Count);
			AssertEquals("O1", docketInfoCollection[0].ExternalReference);
			AssertEquals("O2", docketInfoCollection[1].ExternalReference);
			AssertEquals(1, docketInfoCollection[0].Lines.Count);
			AssertEquals(1, docketInfoCollection[1].Lines.Count);

			docketInfoCollection = new WhsDocketInfoCollection(pick.Orders.Cast<WhsPickableDocket>(), shouldCreateDocketLines: false);
			AssertNotNull(docketInfoCollection);
			AssertEquals(2, docketInfoCollection.Count);
			AssertEquals("O1", docketInfoCollection[0].ExternalReference);
			AssertEquals("O2", docketInfoCollection[1].ExternalReference);
			AssertEquals("Lines should not be loaded", 0, docketInfoCollection[0].Lines.Count);
			AssertEquals("Lines should not be loaded", 0, docketInfoCollection[1].Lines.Count);
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsDocketInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsDocketInfo);
		}

		protected override WhsDocketInfo GetNewObjectInfo()
		{
			return new WhsDocketInfo();
		}

		protected override DataObjectInfoCollection<WhsDocketInfo> GetNewObjectInfoCollection()
		{
			return new WhsDocketInfoCollection();
		}

		#endregion
	}
}

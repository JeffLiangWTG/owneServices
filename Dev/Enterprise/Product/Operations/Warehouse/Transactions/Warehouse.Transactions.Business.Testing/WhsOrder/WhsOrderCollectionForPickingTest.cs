using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderCollectionForPicking))]
	class WhsOrderCollectionForPickingTest : WhsBusinessObjectCollectionTestCase
	{
		#region OrdersForTest

		public class OrdersForTest : WhsOrderCollectionForPicking
		{
			public OrdersForTest(BusinessObjectFactory factory, WhsPick parent)
				: base(factory, parent)
			{
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.CreateAdditionalFilter(); }
			}
		}

		#endregion

		#region Additional Filter

		public void TestAdditionalFilter()
		{
			var parent = Factory.New<WhsPick>();
			var parent1 = Factory.New<WhsPick>();
			parent1.WP_WW_Whs = Helper.CreateWarehouse("Warehouse").PK;
			var orders = new OrdersForTest(Factory, parent);
			var orders1 = new OrdersForTest(Factory, parent1);
			var ord = new WhsOrderCollectionForPicking(Factory, parent);

			AssertEquals("Additional Filter",
				"WD_DocketStatus = 'ENT' and WD_PK IN (SELECT WE_WD FROM dbo.WhsDocketLine WHERE WE_WD = WD_PK)",
				orders.AdditionalFilter.LiteralTextADO);
			AssertEquals("Additional Filter",
				"WD_DocketStatus = 'ENT' and WD_WW_Whs = CONVERT('" + parent1.WP_WW_Whs + "', 'System.Guid') and WD_PK IN (SELECT WE_WD FROM dbo.WhsDocketLine WHERE WE_WD = WD_PK)",
				orders1.AdditionalFilter.LiteralTextADO);
		}

		public void TestAdditionalFilter_WhenPickHasDynamicPickAreaOverride_ThenExcludeOrdersWithPalletIDOnOrderLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true);

			var orderWithPalletID = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			orderWithPalletID.WD_ExternalReference = "1";
			var orderLine1 = Helper.CreateWhsOrderLine(orderWithPalletID, data.Part1, 10m, "ABC");
			var orderLine2 = Helper.CreateWhsOrderLine(orderWithPalletID, data.Part1, 10m);
			var orderWithNoOrderedPalletID = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			orderWithNoOrderedPalletID.WD_ExternalReference = "2";
			var orderLine3 = Helper.CreateWhsOrderLine(orderWithNoOrderedPalletID, data.Part1, 10m);
			var anotherOrderWithNoOrderedPalletID = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			anotherOrderWithNoOrderedPalletID.WD_ExternalReference = "3";
			var orderLine4 = Helper.CreateWhsOrderLine(anotherOrderWithNoOrderedPalletID, data.Part1, 10m);

			var parent = Factory.New<WhsPick>();
			parent.WP_WW_Whs = data.Whs1.PK;
			var dynamicPickFaceArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			parent.WP_WA_DynamicPickAreaOverride = dynamicPickFaceArea.PK;
			Factory.Save();

			var orders = new OrdersForTest(Factory, parent);
			var fetchedOrders = Factory.Load<WhsOrder>(orders.AdditionalFilter);
			AssertContainsExactElementsInAnyOrder(new[] { orderWithNoOrderedPalletID, anotherOrderWithNoOrderedPalletID }, fetchedOrders);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var pick = Factory.New<WhsPick>();
			var whs1 = Helper.CreateWarehouse("1");
			var whs2 = Helper.CreateWarehouse("2");
			var org1 = Helper.CreateClient();
			var org2 = Helper.CreateClient();
			var order11 = Helper.CreateWhsOrder(org1, whs1);
			var order12 = Helper.CreateWhsOrder(org2, whs1);
			var order13 = Helper.CreateWhsOrder(org2, whs1);
			var order21 = Helper.CreateWhsOrder(org1, whs2);
			var order31 = Helper.CreateWhsOrder(org1, whs1);
			var part = Helper.CreateProduct(org1, "P1");
			Helper.CreateProductClientRelationShip(org2, part);
			var orderLine11 = Helper.CreateWhsOrderLine(order11, part, 10m);
			var orderLine12 = Helper.CreateWhsOrderLine(order12, part, 10m);
			var orderLine21 = Helper.CreateWhsOrderLine(order21, part, 10m);
			var orderLine31 = Helper.CreateWhsOrderLine(order31, part, 10m, "123");

			var parent1 = Factory.New<WhsPick>();
			var parent2 = Factory.New<WhsPick>();
			parent2.WP_WW_Whs = whs1.PK;
			var parent3 = Factory.New<WhsPick>();
			parent3.WP_WW_Whs = whs1.PK;
			var dynamicPickFaceArea = Helper.CreateArea(whs1, "DYNAMIC");
			parent3.WP_WA_DynamicPickAreaOverride = dynamicPickFaceArea.PK;
			var ord1 = new OrdersForTest(Factory, parent1);
			var ord2 = new OrdersForTest(Factory, parent2);

			order11.WD_DocketStatus = DocketStatus.Codes.Held;
			order13.WD_DocketStatus = DocketStatus.Codes.Entered;
			order21.WD_DocketStatus = DocketStatus.Codes.Entered;
			order31.WD_DocketStatus = DocketStatus.Codes.Entered;

			Factory.New<WhsPick>().PickOrdersWithAllocationMock(new[] { order12 });

			AssertEquals("\nThis Order has no Lines.", ord1.GetAllNotificationsWhenAdditionalFilterNotMet(order13));
			AssertEquals("\nThis Order is for a different Warehouse.", ord2.GetAllNotificationsWhenAdditionalFilterNotMet(order21));
			AssertEquals("\nThis Order is attached to another Pick.", ord1.GetAllNotificationsWhenAdditionalFilterNotMet(order12).ToString());
			AssertEquals("\nThis Order is Held. Only Entered (Saved) Orders can be attached to a Pick.", ord1.GetAllNotificationsWhenAdditionalFilterNotMet(order11));

			var ord3 = new OrdersForTest(Factory, parent3);
			AssertEquals("\nPick is using Dynamic Pick Face Replenishment and cannot attach Order with Ordered Pallet ID.", ord3.GetAllNotificationsWhenAdditionalFilterNotMet(order31));
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsOrderCollectionForPicking(Factory, null);
		}

		#endregion
	}
}

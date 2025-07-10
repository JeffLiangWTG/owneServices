using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrdersBuyerCollection))]
	sealed class OrdersBuyerCollectionTest : ConsigneeCollectionTest
	{
		public void TestJD_OH_Buyer_ListCollectionType()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_IsConsignee = true;

			var order = Factory.New<Order>();
			AssertType(typeof(OrdersBuyerCollection), order.BuyerList);
		}

		public void TesGetExtraNotification_AllowOrder()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_IsConsignee = true;
			buyer.MiscServ.OM_IMAllowOrders = true;

			var order = Factory.New<Order>();
			order.BuyerList.Load();
			AssertCollectionContains("Precondition - Collection loads element", buyer, order.BuyerList);

			var notificationProvider = order.BuyerList as IFilterModuleExtraNotificationProvider;
			AssertNull(notificationProvider.GetExtraNotification(order));
		}

		public void TestGetExtraNotification_NotAllowOrder()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_IsConsignee = true;
			buyer.MiscServ.OM_IMAllowOrders = false;

			var order = Factory.New<Order>();
			order.BuyerList.Load();
			AssertCollectionContains("Precondition - Collection loads element", buyer, order.BuyerList);

			var notificationProvider = order.BuyerList as IFilterModuleExtraNotificationProvider;
			AssertEquals("This buyer is restricted from using Order Manager (Organization > Consignee > Disallow Order Manager flag is on)", notificationProvider.GetExtraNotification(buyer).Message);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrdersBuyerCollection(Factory);
		}
	}
}

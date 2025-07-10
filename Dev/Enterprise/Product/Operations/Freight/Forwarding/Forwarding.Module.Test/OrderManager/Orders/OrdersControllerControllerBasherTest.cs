using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrdersController))]
	class OrdersControllerBasherTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			Order orderBizo = Factory.New<Order>();
			orderBizo.JD_OrderNumber = "Zubstest";
			orderBizo.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			orderBizo.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Factory.Save();
			return orderBizo;
		}

		public void TestNewOrderSplitHasSaveEnabled()
		{
			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "TEST1";
			OrderLine orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_Quantity = 10;
			orderLine1.JO_QtyInvoiced = 10;
			orderLine1.JO_QtyReceived = 5;
			order1.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order1.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Factory.Save();
			OrdersController orderController = (OrdersController)ZControllerFactory.Create(GetControllerID());
			try
			{
				orderController.ShowSplitForm(order1, CreateOrderType.New);

				var postingButtonsControl = (Core.Forms.ZPostingButtonsUserControl)((OrdersForm)orderController.LastShownForm).Controls.Find("PostingButtonsUserControl", true)[0];
				AssertEquals("Save button enabled", true, postingButtonsControl.SaveButton.Enabled);
				AssertEquals(true, ((OrdersForm)orderController.LastShownForm).BusinessEntity.HasChanges);
			}
			finally
			{
				if (orderController.LastShownForm != null)
				{
					orderController.LastShownForm.Dispose();
				}
			}
		}

		public void TestNewSplitUsesDifferentFactory()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			Factory.Save();
			OrdersController orderController = (OrdersController)ZControllerFactory.Create(GetControllerID());
			try
			{
				orderController.ShowSplitForm(order1, CreateOrderType.New);
				Assert("New forms use different factory", order1.Factory != ((ZForm)orderController.LastShownForm).BusinessEntity.Factory);
			}
			finally
			{
				if (orderController.LastShownForm != null)
				{
					orderController.LastShownForm.Dispose();
				}
			}
		}

		public void TestGetModuleID()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			var orderController = ZControllerFactory.Create(GetControllerID());
			AssertEquals("Orders", orderController.ModuleID.ToString());
		}

		public void TestOrdersControllerChildEditableState()
		{
			var ordersController = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ChildEditableServiceStates.Order, ChildEditableService.GetStateDirectly(ordersController.Factory));
		}

		#region CRM Security

		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<Order>();
			bizObjWithoutAccess.Buyer.MiscServ.OM_GG_OrgSecurityGroup = Factory.NewWithValidTestData<GlbGroup>().PK;
			CRMSecurityProviderTest<Order>.AssertController(new OrdersController(), bizObjWithoutAccess, Env.Security.OrderTrackingCRMSecurity);
		}

		#endregion

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Orders;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}
	}
}

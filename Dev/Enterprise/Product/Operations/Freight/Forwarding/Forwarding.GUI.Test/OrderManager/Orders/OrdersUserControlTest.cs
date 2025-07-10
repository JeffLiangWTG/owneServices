using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	sealed class OrdersUserControlTest : TestCaseWithFactory
	{
		public void TestCreatePackLinesFromOrderLines()
		{
			var shipment = Factory.New<CommonShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_ActualWeight = 1;
			order.JD_ActualVolume = 1;
			order.JD_Packs = 1;
			order.BuyerPK = org.PK;
			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_ActualWeight = 1;
			orderLine.JO_ActualVolume = 1;
			orderLine.JO_OuterPacks = 1;
			order.OrderLines.Add(orderLine);
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Orders.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(order))
			{
				var ordersControl = new TestOrdersUserControl();
				form.Controls.Add(ordersControl);
				form.Show();
				ordersControl.SetDataBinding(order, "");
				Application.DoEvents();

				order.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;

				AssertEquals(1, order.Shipment.OuterPackLines.Count);
				AssertEquals(1m, order.Shipment.JS_ActualWeight);
				AssertEquals(1m, order.Shipment.JS_ActualVolume);
				AssertEquals(1, order.Shipment.JS_OuterPacks);
				AssertEquals(null, UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Do you want to link the order lines from the recently attached orders to pack-lines on this shipment?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var order2 = Factory.NewWithValidTestData<Order>();
			order2.BuyerPK = org2.PK;
			order2.JD_ActualWeight = 2;
			order2.JD_ActualVolume = 2;
			order2.JD_Packs = 2;
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Orders.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(order2))
			{
				var ordersControl = new TestOrdersUserControl();
				form.Controls.Add(ordersControl);
				form.Show();
				ordersControl.SetDataBinding(order2, "");
				Application.DoEvents();

				order2.JD_JS_ThatAutoUpdatesShipmentDetailsFromOrder = shipment.PK;

				AssertEquals(1, order2.Shipment.OuterPackLines.Count);
				AssertEquals(3m, order2.Shipment.JS_ActualWeight);
				AssertEquals(3m, order2.Shipment.JS_ActualVolume);
				AssertEquals(3, order2.Shipment.JS_OuterPacks);
				AssertEquals("Do you want to link the order lines from the recently attached orders to pack-lines on this shipment?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Do you want to update the Packs, Weight, Volume on the Shipment from the Order?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowShipmentWithChildEditableServiceStates()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (TestOrdersUserControl ordersControl = new TestOrdersUserControl())
			{
				Form.Controls.Add(ordersControl);
				ordersControl.SetDataBinding(order, "");

				Application.DoEvents();

				ordersControl.Order.JD_JS = shipment.PK;
				ordersControl.Order.BuyerPK = org.PK;
				ordersControl.OpenShipment();
				AssertEquals("Should set Shipment State", ChildEditableServiceStates.Shipment, ChildEditableService.GetState(Order.Shipment.Factory));
			}
		}

		//public void TestBuyerCaption()
		//{
		//	AssertEquals("Ordered By", OrdersControl.JD_OH_BuyerBoundOrgControl.Text);
		//}

		//public void TestSupplierCaption()
		//{
		//	AssertEquals("Supplier", OrdersControl.JD_OH_SupplierBoundOrgControl.Text);
		//}

		public void TestIncoTermsExplainButton()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Order bO = factory.New<Order>();
			using (ZForm form = new ZForm(bO))
			{
				TestOrdersUserControl ordersControl = new TestOrdersUserControl();
				form.Controls.Add(ordersControl);
				ordersControl.SetDataBinding(bO, "");
				form.Show();
				Application.DoEvents();

				bO.JD_IncoTerm = "EXW";
				ordersControl.IncoTermsExplainButton.PerformClick();
				AssertEquals(typeof(IncoTermDescriptionForm), ZFormModaliser.ActiveForm.GetType());
				AssertEquals("EXW", ((ZForm)ZFormModaliser.ActiveForm).Text);
				((ZForm)ZFormModaliser.ActiveForm).Dispose();

				bO.JD_IncoTerm = "ZUB";
				ordersControl.IncoTermsExplainButton.PerformClick();
				AssertNull(ZFormModaliser.ActiveForm);
			}
		}

		public void TestOrderLineDetailRecordsNotValidated()
		{
			// save some bodgey data to start with
			Order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			Order.JD_OrderNumber = "z";

			OrderLine line = Order.OrderLines.AddNew();
			line.JO_LineNo = 1;

			OrderLineDelivery delivery = line.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "x";
			delivery.J4_OA_NKDeliveryPoint = ZString.Empty;
			AssertEquals("Must have errors for test", true, delivery.J4_OA_NKDeliveryPointInfo.HasErrors());

			OrderLineDeliverContainer container = delivery.Containers.AddNew();
			container.J5_QuantityInStore = -1;
			AssertEquals("Must have errors for test", true, container.J5_QuantityInStoreInfo.HasWarnings());
			Factory.Save();

			// now see if the order has errors when bound to the form and before saving
			ZController controller = ZControllerFactory.Create(ControllerIDs.Orders);
			controller.ShowEditForm(Order);
			using (OrdersForm form = (OrdersForm)controller.LastShownForm)
			{
				form.Order.RunPreSaveValidation();
				AssertEquals("Validation should be suspended", false, form.Order.OrderLines[0].Deliveries[0].HasErrors);
				AssertEquals("Validation should be suspended", false, form.Order.OrderLines[0].Deliveries[0].Containers[0].HasWarnings);
			}
		}

		public void TestChangeChargesTabVisibility()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Order order = factory.New<Order>();
			using (ZForm form = new ZForm(order))
			{
				TestOrdersUserControl ordersControl = new TestOrdersUserControl();
				form.Controls.Add(ordersControl);
				ordersControl.SetDataBinding(order, "");
				form.Show();
				Application.DoEvents();
				Assert(!ordersControl.ChargesTabPage.TabVisible);

				order.JD_RL_NKPortOfLoading = "NZAKL";
				Assert(ordersControl.ChargesTabPage.TabVisible);
			}
		}

		public void TestBuyerIsSelectedControlWhenFormOpened()
		{
			Form.Controls.Add(OrdersControl);
			Form.Show();
			OrdersControl.SetDataBinding(Order, "");
			Application.DoEvents();

			AssertEquals(0, OrdersControl.BottomTabControl.SelectedIndex);
			//	AssertNotNull("Buyer field should contain focus", OrdersControl.JD_OH_BuyerBoundOrgControl.ActiveControl);
		}

		public void TestPlannedContainersGrid_NotReadOnly()
		{
			Form.Controls.Add(OrdersControl);
			Form.Show();
			OrdersControl.SetDataBinding(Order, "");
			Application.DoEvents();

			Assert("Grid Shouldn't be read only", !OrdersControl.PlannedContainersGrid.ReadOnly);
		}

		public void TestPlannedContainersGrid_ReadOnly()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			Order.JD_EF_ShipmentPrePlanning = preAdvice.PK;

			Form.Controls.Add(OrdersControl);
			Form.Show();
			OrdersControl.SetDataBinding(Order, "");
			Application.DoEvents();

			Assert("Grid should be read only", OrdersControl.PlannedContainersGrid.ReadOnly);
		}

		public void TestConsolsGrid_ReadOnly_EditButtonEnabled()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Consols.AddNew();
			Order.JD_JS = shipment.PK;

			Form.Controls.Add(OrdersControl);
			Form.Show();
			OrdersControl.SetDataBinding(Order, "");
			Application.DoEvents();
			((TabControl)OrdersControl.ShipmentTab.Parent).SelectedTab = OrdersControl.ShipmentTab;

			AssertEquals("Grid should be read only", true, OrdersControl.ConsolsBoundButtonGrid.InnerGrid.ReadOnly);

			var toolStrip = OrdersControl.ConsolsBoundButtonGrid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
			var editButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
			AssertEquals("Edit button enabled", true, editButton.Enabled);
		}

		public void TestMilestonesTab()
		{
			OrdersDataRegistry.Instance.ShowOrderMilestonesOnMainScreen.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (ordersControl = new TestOrdersUserControl())
			{
				AssertEquals(true, OrdersControl.MilestonesTabPage.TabVisible);
			}

			OrdersDataRegistry.Instance.ShowOrderMilestonesOnMainScreen.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (ordersControl = new TestOrdersUserControl())
			{
				AssertEquals(false, OrdersControl.MilestonesTabPage.TabVisible);
			}
		}

		public void TestShipmentDetachButton()
		{
			var factory = new BusinessObjectFactory();
			var shipment1 = factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = "SEA";

			var order = factory.NewWithValidTestData<Order>();
			shipment1.AttachedOrders.Add(order);
			order.JD_JS = shipment1.PK;
			order.OnDetachingPreAdviceAskToDetachShipmentOrDeclaration += (s, e) => { e.Cancel = false; };
			AssertEquals("Precondition", 0, order.ConsolsForBinding.Count);

			var consol1 = factory.NewWithValidTestData<ForwardingConsol>();
			shipment1.Consols.Add(consol1);
			AssertContainsExactElementsInAnyOrder(new[] { consol1 }, order.ConsolsForBinding);
			Assert(!order.IsAllowedToDetachShipment);

			using (var form = new ZForm(order))
			{
				var ordersControl = new TestOrdersUserControl();
				form.Controls.Add(ordersControl);
				ordersControl.SetDataBinding(order, "");
				form.Show();
				Application.DoEvents();

				((TabControl)ordersControl.ShipmentTab.Parent).SelectedTab = ordersControl.ShipmentTab;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ordersControl.ShipmentDetachButton.PerformClick();
				AssertEquals("This shipment contains important changes and should be saved before it can be detached.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("JD_JS should not be cleared", shipment1.PK, order.JD_JS);
				AssertEquals("JD_JE should not be cleared", ZGuid.Empty, order.JD_JE);

				factory.Save();
				Assert(order.IsAllowedToDetachShipment);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ordersControl.ShipmentDetachButton.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("JD_JS should be cleared", ZGuid.Empty, order.JD_JS);
				AssertEquals("JD_JE should be cleared", ZGuid.Empty, order.JD_JE);
			}
		}

		public void TestRefreshPlanningTabText_ShouldSetPlanningTabTextToPlanning_WhenOrderIsNull()
		{
			using (var zForm = new ZForm(null))
			{
				var ordersUserControl = new TestOrdersUserControl();
				zForm.Controls.Add(ordersUserControl);
				ordersUserControl.PlanningTab.Text = string.Empty;
				zForm.Show();
				Application.DoEvents();
				AssertNoExceptionThrown(ordersUserControl.RefreshPlanningTabText);
				AssertEquals("Planning", ordersUserControl.PlanningTab.Text);
			}
		}

		#region Test Classes

		class TestOrdersUserControl : OrdersUserControl
		{
			public new ZModuleButtonGrid OrderLinesBoundButtonGrid
			{
				get { return base.OrderLinesBoundButtonGrid; }
			}

			public new ZGrid PlannedContainersGrid
			{
				get { return base.PlannedContainersGrid; }
			}

			public new ZModuleButtonGrid ConsolsBoundButtonGrid
			{
				get { return base.ConsolsBoundButtonGrid; }
			}

			public new ZTabPage MilestonesTabPage
			{
				get { return base.MilestonesTabPage; }
			}

			public new ZTabPage OrdersTab
			{
				get { return base.OrdersTab; }
			}

			public new ZGroupBox OrderDetailsGroupBox
			{
				get { return base.OrderDetailsGroupBox; }
			}

			public new ZTemplateTabControl BottomTabControl
			{
				get { return base.BottomTabControl; }
			}

			public new ZTabPage ShipmentTab
			{
				get { return base.ShipmentTab; }
			}

			public new ZButton IncoTermsExplainButton
			{
				get { return base.IncoTermsExplainButton; }
			}

			public new ZGuidFindBox JD_JSBoundFindBox
			{
				get { return base.JD_JSBoundFindBox; }
			}

			public new ZGuidFindBox JD_VBBoundFindBox
			{
				get { return base.JD_VBBoundFindBox; }
			}

			public new ZGuidFindBox JD_JEBoundFindBox
			{
				get { return base.JD_JEBoundFindBox; }
			}

			public new ZButton ShipmentDetachButton
			{
				get { return base.ShipmentDetachButton; }
			}

			public new ZTabPage PlanningTab
			{
				get { return base.PlanningTab; }
			}
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (ordersControl != null)
			{
				ordersControl.Dispose();
			}
		}

		TestOrdersUserControl OrdersControl
		{
			get
			{
				if (ordersControl == null)
				{
					ordersControl = new TestOrdersUserControl();
				}
				return ordersControl;
			}
		}
		TestOrdersUserControl ordersControl;

		ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(Order);
				}
				return form;
			}
		}
		ZForm form;

		Order Order
		{
			get { return order ?? (order = Factory.New<Order>()); }
		}
		Order order;

		#endregion
	}
}

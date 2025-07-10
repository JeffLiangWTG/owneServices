using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	class OrdersAttachUserControlTest : TestCaseWithFactory
	{
		public void TestJP_OrderItemsAsString_AddNew()
		{
			ShipmentForm.Show();
			Application.DoEvents();
			OrdersAttachUserControl.OrdersButtonGrid.Parent.Parent.Parent.Visible = true;

			Shipment.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "123";
			Shipment.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "456";

			OrdersAttachUserControl.OrderItemsEditButton.PerformClick();
			AssertEquals("123,456", OrdersAttachUserControl.JP_OrderItemsAsStringTextBox.Text);

			Shipment.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "987";

			OrdersAttachUserControl.OrderItemsEditButton.PerformClick();
			AssertEquals("123,456,987", OrdersAttachUserControl.JP_OrderItemsAsStringTextBox.Text);
		}

		public void TestOrdersGridNewButton_WhenShipmentNotCommitted()
		{
			ShipmentForm.Show();
			Application.DoEvents();
			OrdersAttachUserControl.OrdersButtonGrid.Parent.Parent.Parent.Visible = true;
			ModuleButtonGridNewButton.PerformClick();
			OrdersForm orderForm = (OrdersForm)OrdersAttachUserControl.OrdersButtonGrid.LastShownZForm;

			Order order = (Order)orderForm.BusinessEntity;
			order.FillWithValidTestData();

			int rowCountBeforeSave = OrdersAttachUserControl.OrdersButtonGrid.InnerGrid.VisibleRowCount;
			order.Factory.Save();
			typeof(ZForm).InvokeMember("FireSaved", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, orderForm, null); // expect no foreign key violation exception
			AssertEquals("1 new row should appear in the grid after save of the order form", 1, OrdersAttachUserControl.OrdersButtonGrid.InnerGrid.VisibleRowCount - rowCountBeforeSave);
			orderForm.Dispose();
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (shipmentForm != null)
			{
				shipmentForm.Dispose();
			}
		}

		OrdersAttachUserControl OrdersAttachUserControl
		{
			get
			{
				if (ordersAttachUserControl == null)
				{
					ordersAttachUserControl = new OrdersAttachUserControl();
					ShipmentForm.Controls.Add(ordersAttachUserControl);
				}
				return ordersAttachUserControl;
			}
		}

		OrdersAttachUserControl ordersAttachUserControl;

		ShipmentForm ShipmentForm
		{
			get
			{
				if (shipmentForm == null)
				{
					ChildEditableService.SetState(Shipment.Factory, ChildEditableServiceStates.Shipment);
					shipmentForm = new ShipmentForm(Shipment);
				}
				return shipmentForm;
			}
		}
		ShipmentForm shipmentForm;

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "USLAX";
				}
				return shipment;
			}
		}
		ForwardingShipment shipment;

		ZToolStripButton ModuleButtonGridNewButton
		{
			get
			{
				var toolStrip = OrdersAttachUserControl.OrdersButtonGrid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();
			}
		}

		#endregion
	}
}

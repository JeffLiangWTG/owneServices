using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	class AttachGenericOrdersUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestJP_OrderItemsAsString_AddNew()
		{
			ShipmentForm.Show();
			Application.DoEvents();
			AttachGenericOrdersUserControl.GenericOrdersButtonGrid.Parent.Parent.Parent.Visible = true;

			Shipment.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "123";
			Shipment.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "456";

			AttachGenericOrdersUserControl.OrderItemsEditButton.PerformClick();
			AssertEquals("123,456", AttachGenericOrdersUserControl.JP_OrderItemsAsStringTextBox.Text);

			Shipment.DocsAndCartage.OrderItems.AddNew().JT_OrderReference = "987";

			AttachGenericOrdersUserControl.OrderItemsEditButton.PerformClick();
			AssertEquals("123,456,987", AttachGenericOrdersUserControl.JP_OrderItemsAsStringTextBox.Text);
		}

		[RequiresSTA]
		public void TestOrdersGridNewButton_WhenShipmentNotCommitted()
		{
			ShipmentForm.Show();
			Application.DoEvents();
			AttachGenericOrdersUserControl.GenericOrdersButtonGrid.Parent.Parent.Parent.Visible = true;
			ModuleButtonGridNewButton.PerformButtonClick();
			var orderForm = (OrdersForm)AttachGenericOrdersUserControl.GenericOrdersButtonGrid.LastShownZForm;

			var order = (Order)orderForm.BusinessEntity;
			order.FillWithValidTestData();

			int rowCountBeforeSave = AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.VisibleRowCount;
			order.Factory.Save();
			typeof(ZForm).InvokeMember("FireSaved", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, orderForm, null); // expect no foreign key violation exception
			AssertEquals("1 new row should appear in the grid after save of the order form", 1, AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.VisibleRowCount - rowCountBeforeSave);
			orderForm.Dispose();
		}

		public void TestTotalWeightAndUnitofWeightGrouping()
		{
			ShipmentForm.Show();

			AssertEquals("Total Weight", AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.GetColumnStyle("TotalWeight").GroupName.Caption);
			AssertEquals("Total Weight", AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.GetColumnStyle("WeightUnit").GroupName.Caption);
		}

		public void TestTotalVolumeAndUnitofVolumeGrouping()
		{
			ShipmentForm.Show();

			AssertEquals("Total Volume", AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.GetColumnStyle("TotalVolume").GroupName.Caption);
			AssertEquals("Total Volume", AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.GetColumnStyle("VolumeUnit").GroupName.Caption);
		}

		public void TestPacksAndPacksTypeGrouping()
		{
			ShipmentForm.Show();

			AssertEquals("Total Packs", AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.GetColumnStyle("TotalPacks").GroupName.Caption);
			AssertEquals("Total Packs", AttachGenericOrdersUserControl.GenericOrdersButtonGrid.InnerGrid.GetColumnStyle("PacksType").GroupName.Caption);
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

		AttachGenericOrdersUserControl AttachGenericOrdersUserControl
		{
			get { return FindOrdersAttachUserControl(ShipmentForm); }
		}

		AttachGenericOrdersUserControl FindOrdersAttachUserControl(Control control)
		{
			foreach (Control child in control.Controls)
			{
				AttachGenericOrdersUserControl userControl = child as AttachGenericOrdersUserControl;
				if (userControl != null)
				{
					return userControl;
				}
				userControl = FindOrdersAttachUserControl(child);
				if (userControl != null)
				{
					return userControl;
				}
			}
			return null;
		}

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

		ToolStripSplitButton ModuleButtonGridNewButton
		{
			get
			{
				return (ToolStripSplitButton)AttachGenericOrdersUserControl.GenericOrdersButtonGrid.GetType()
					.GetField("NewOrderSplitButton", BindingFlags.NonPublic | BindingFlags.Instance)
					.GetValue(AttachGenericOrdersUserControl.GenericOrdersButtonGrid);
			}
		}

		#endregion
	}
}

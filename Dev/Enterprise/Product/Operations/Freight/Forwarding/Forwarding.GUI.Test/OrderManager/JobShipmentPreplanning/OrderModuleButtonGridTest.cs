using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	public class OrderModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestDetachButton_Click_DetachShipments()
		{
			JobShipmentPreplanning preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();

			using (var form = new MockJobShipmentPreplanningForm(preadvice))
			{
				form.Show();
				var toolStrip = form.OrdersGrid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				// Case 1: orders not attached to shipment
				Order order1 = preadvice.Orders.AddNew();
				Order order2 = preadvice.Orders.AddNew();

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // Default confirmation question

				form.OrdersGrid.InnerGrid.SelectAllElements();
				detachButton.PerformClick();

				AssertNull("No orders with shipments, dialog wasn't raised", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Orders were detached from preadvice", 0, preadvice.Orders.Count);

				// Case 2: orders are attached to shipment, but user selected to not detach them from shipment
				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				order1 = preadvice.Orders.AddNew();
				order1.JD_JS = shipment.PK;

				order2 = preadvice.Orders.AddNew();
				order2.JD_JS = shipment.PK;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // Default confirmation question
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);   // Detach shipment question

				form.OrdersGrid.InnerGrid.SelectAllElements();
				detachButton.PerformClick();

				Assert("Detach from shipment question was raised", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Do you want to detach order(s) from the shipment as well?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Orders were detached from preadvice", 0, preadvice.Orders.Count);
				AssertNotNull("Order wasn't detached from shipment", order1.Shipment);
				AssertNotNull("Order wasn't detached from shipment", order2.Shipment);

				// Case 3: orders are attached to shipment, user selected to detach them from shipment as well
				order1 = preadvice.Orders.AddNew();
				order1.JD_JS = shipment.PK;

				order2 = preadvice.Orders.AddNew();
				order2.JD_JS = shipment.PK;

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // Default confirmation question
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);  // Detach shipment question

				form.OrdersGrid.InnerGrid.SelectAllElements();
				detachButton.PerformClick();

				Assert("Detach from shipment question was raised", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Do you want to detach order(s) from the shipment as well?", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("Orders were detached from preadvice", 0, preadvice.Orders.Count);
				AssertNull("Order was detached from shipment", order1.Shipment);
				AssertNull("Order was detached from shipment", order2.Shipment);
			}
		}

		public void TestDetachButton_Click_DataExportEvents()
		{
			var shipmentPreplanning = Factory.NewWithValidTestData<JobShipmentPreplanning>();

			var order1 = shipmentPreplanning.Orders.AddNew();
			order1.JD_OrderNumber = "1";

			Factory.Save();

			using (MockJobShipmentPreplanningForm shipmentPreplanningForm = new MockJobShipmentPreplanningForm(shipmentPreplanning))
			{
				shipmentPreplanningForm.Show();

				var toolStrip = shipmentPreplanningForm.OrdersGrid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				shipmentPreplanningForm.OrdersGrid.InnerGrid.Select(0);
				detachButton.PerformClick();
				AssertNotEquals("No messages", "This shipment pre-advice has been exported and you should not detach orders from it as this could cause data consistency problems for your customers. Instead, the order line quantities should be set to 0. Do you wish to set all order lines to 0 on the order(s) you were trying to detach?",
					UnitTestUserNotification.Instance.PreviousMessages[1].Text);

				Factory.Save();

				AssertEquals("Shipment Preplanning must have 0 Orders", 0, shipmentPreplanning.Orders.Count);

				shipmentPreplanning.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.DataExport);
				Order order2 = shipmentPreplanning.Orders.AddNew();
				order2.JD_OrderNumber = "2";
				OrderLine orderLine1 = order2.OrderLines.AddNew();
				OrderLine orderLine2 = order2.OrderLines.AddNew();
				orderLine1.JO_Quantity = 1;
				orderLine1.JO_QtyInvoiced = 1;
				orderLine1.JO_QtyReceived = 1;
				orderLine2.JO_Quantity = 1;
				orderLine2.JO_QtyInvoiced = 1;
				orderLine2.JO_QtyReceived = 1;

				Factory.Save();

				AssertEquals("OrderLines Count", 2, shipmentPreplanning.Orders[0].OrderLines.Count);
				AssertEquals("JO_Quantity", 1m, shipmentPreplanning.Orders[0].OrderLines[0].JO_Quantity);
				AssertEquals("JO_QtyInvoiced", 1m, shipmentPreplanning.Orders[0].OrderLines[0].JO_QtyInvoiced);
				AssertEquals("JO_QtyReceived", 1m, shipmentPreplanning.Orders[0].OrderLines[0].JO_QtyReceived);
				AssertEquals("JO_Quantity", 1m, shipmentPreplanning.Orders[0].OrderLines[1].JO_Quantity);
				AssertEquals("JO_QtyInvoiced", 1m, shipmentPreplanning.Orders[0].OrderLines[1].JO_QtyInvoiced);
				AssertEquals("JO_QtyReceived", 1m, shipmentPreplanning.Orders[0].OrderLines[1].JO_QtyReceived);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				shipmentPreplanningForm.OrdersGrid.InnerGrid.Select(0);
				detachButton.PerformClick();
				Assert("Shipment Preplanning question should pop up", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Shipment Preplanning question should pop up", "This shipment pre-advice has been exported and you should not detach orders from it as this could cause data consistency problems for your customers. Instead, the order line quantities should be set to 0. Do you wish to set all order lines to 0 on the order(s) you were trying to detach?",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Shipment Preplanning must have 1 Orders", 1, shipmentPreplanning.Orders.Count);
				AssertEquals("Shipment Preplanning must have 2 OrderLines", 2, shipmentPreplanning.Orders[0].OrderLines.Count);
				AssertEquals("JO_Quantity", 0m, shipmentPreplanning.Orders[0].OrderLines[0].JO_Quantity);
				AssertEquals("JO_QtyInvoiced", 0m, shipmentPreplanning.Orders[0].OrderLines[0].JO_QtyInvoiced);
				AssertEquals("JO_QtyReceived", 0m, shipmentPreplanning.Orders[0].OrderLines[0].JO_QtyReceived);
				AssertEquals("JO_Quantity", 0m, shipmentPreplanning.Orders[0].OrderLines[1].JO_Quantity);
				AssertEquals("JO_QtyInvoiced", 0m, shipmentPreplanning.Orders[0].OrderLines[1].JO_QtyInvoiced);
				AssertEquals("JO_QtyReceived", 0m, shipmentPreplanning.Orders[0].OrderLines[1].JO_QtyReceived);
			}
		}

		#region Implementation

		class MockJobShipmentPreplanningForm : JobShipmentPreplanningForm
		{
			public MockJobShipmentPreplanningForm(JobShipmentPreplanning preAdvice)
				: base(preAdvice)
			{
			}

			public OrderModuleButtonGrid OrdersGrid;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				OrdersGrid = new OrderModuleButtonGrid();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.Caption = "Order Number";
				zTextBoxColumnStyleInfo1.ColumnName = "JD_OrderNumberAndSplit";

				OrdersGrid.BindToGridList = "Orders";
				OrdersGrid.BindToFindBoxList = "Lookups+Orders";
				OrdersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				Controls.Add(OrdersGrid);
			}
		}

		#endregion
	}
}

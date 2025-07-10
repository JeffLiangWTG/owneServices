using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(ProductDeliveryForm))]
	public class ProductDeliveryFormTest : ZFormBasherTest
	{
		public void TestAllOrdersAreEnabled()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();
			var order = preAdvice.Orders.AddNew();
			var order2 = preAdvice.Orders.AddNew();

			using (var form = new ProductDeliveryForm(preAdvice))
			{
				form.Show();

				var checkBoxList = form.FindSingle<ZCheckedListBox>(item => item.Name == "OrdersCheckboxList");

				AssertEquals(preAdvice.Orders.Count, checkBoxList.Items.Count);
				for (int i = 0; i < preAdvice.Orders.Count; i++)
				{
					Assert(checkBoxList.GetItemChecked(i));
				}
			}
		}

		public void TestReceiveAllButton()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();
			var order = preAdvice.Orders.AddNew();

			var line = order.OrderLines.AddNew();
			line.JO_Quantity = 100m;
			line.JO_QtyReceived = 10m;

			var line2 = order.OrderLines.AddNew();
			line2.JO_Quantity = 75m;
			line2.JO_QtyReceived = 30m;

			using (var form = new ProductDeliveryForm(preAdvice))
			{
				form.Show();
				preAdvice.OrderNumberList[0].Value = true;

				AssertEquals("Precondition: 2 lines", 2, preAdvice.OrderLines.Count);
				form.ReceiveAllButton.PerformClick();

				CombineAssertions(delegate
				{
					AssertEquals(100m, line.JO_QtyReceived);
					AssertEquals(75m, line2.JO_QtyReceived);
				});
			}
		}

		#region Close

		public void TestClose()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();
			var order = preAdvice.Orders.AddNew();
			order.JD_OrderNumber = "123";
			var line = order.OrderLines.AddNew();
			line.JO_QtyReceived = 10m;

			using (var form = new ProductDeliveryForm(preAdvice))
			{
				form.Show();
				preAdvice.OrderNumberList[0].Value = true;

				CombineAssertions(delegate
				{
					AssertEquals("Precondition: 1 line", 1, preAdvice.OrderLines.Count);
					AssertEquals("Precondition: 1 order shown", true, preAdvice.OrderNumberList[0].Value);
				});

				form.Close();

				CombineAssertions(delegate
				{
					AssertEquals("Cleared on form close", 0, preAdvice.OrderLines.Count);
					AssertEquals("Reset on form close", false, preAdvice.OrderNumberList[0].Value);
				});
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();
			return new ProductDeliveryForm(preAdvice);
		}

		#endregion
	}
}

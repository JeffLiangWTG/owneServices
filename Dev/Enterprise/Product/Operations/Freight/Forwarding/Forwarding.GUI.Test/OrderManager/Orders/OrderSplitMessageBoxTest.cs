using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(OrderSplitMessageBox))]
	public class OrderSplitMessageBoxTest : ZFormBasherTest
	{
		public void TestMessageLabel()
		{
			using (var messageBox = new OrderSplitMessageBox())
			{
				var messageLabel = messageBox.Controls.Find("MessageLabel", true)[0] as ZLabel;
				AssertEquals("The order(s) has some incomplete order lines. Would you like to: ", messageLabel.Text);
			}

			using (var messageBox = new OrderSplitMessageBox(null))
			{
				var messageLabel = messageBox.Controls.Find("MessageLabel", true)[0] as ZLabel;
				AssertEquals("The order(s) has some incomplete order lines. Would you like to: ", messageLabel.Text);
			}

			using (var messageBox = new OrderSplitMessageBox("ORDER1", "ORDER2"))
			{
				var messageLabel = messageBox.Controls.Find("MessageLabel", true)[0] as ZLabel;
				AssertEquals("The order(s) 'ORDER1, ORDER2' has some incomplete order lines. Would you like to: ", messageLabel.Text);
			}

			using (var messageBox = new OrderSplitMessageBox("ORDER1", "ORDER2", "ORDER3", "ORDER4"))
			{
				var messageLabel = messageBox.Controls.Find("MessageLabel", true)[0] as ZLabel;
				AssertEquals("Multiple orders have some incomplete order lines. Would you like to: ", messageLabel.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new OrderSplitMessageBox();
		}
	}
}

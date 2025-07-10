using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderSplitMessageBox : KForm
	{
		public OrderSplitMessageBox(params ZString[] orders)
		{
			InitializeComponent();
			this.Text = Res.GetString("49013b8b-d319-4765-a2c1-924702d7f93d", "Incomplete Order.");
			this.MessageLabel.Text = Res.GetString("f6037819-4b36-47b0-a74e-541bf8764763", "{0} some incomplete order lines. Would you like to:", GetOrderNames(orders)) + " ";
		}

		ZString GetOrderNames(ZString[] orders)
		{
			var orderNames = ZString.Empty;

			var count = orders?.Length ?? 0;
			if (count > 3)
			{
				orderNames = Res.GetString("206b40ec-26ae-4d0c-b6cf-268e80b40a4b", "Multiple orders have");
			}
			else if (count > 0)
			{
				orderNames = Res.GetString("783fd33e-06fe-4011-b6a2-1090e04421f0", "The order(s) '{0}' has", string.Join(", ", orders));
			}
			else
			{
				orderNames = Res.GetString("a33fd828-779f-421a-aea0-dac03a8d53da", "The order(s) has");
			}

			return orderNames;
		}

		public OrderSplitDialogResult OrderSplitDialogResult
		{
			get { return fOrderSplitDialogResult; }
		}

		#region Implementation

		OrderSplitDialogResult fOrderSplitDialogResult = OrderSplitDialogResult.DoNothing;

		void SplitButton_Click(object sender, System.EventArgs e)
		{
			fOrderSplitDialogResult = OrderSplitDialogResult.SplitOrder;
			this.Close();
		}

		void NoButton_Click(object sender, System.EventArgs e)
		{
			fOrderSplitDialogResult = OrderSplitDialogResult.DoNothing;
			this.Close();
		}

		void NewButton_Click(object sender, System.EventArgs e)
		{
			fOrderSplitDialogResult = OrderSplitDialogResult.CreateNewOrder;
			this.Close();
		}

		#endregion
	}
}

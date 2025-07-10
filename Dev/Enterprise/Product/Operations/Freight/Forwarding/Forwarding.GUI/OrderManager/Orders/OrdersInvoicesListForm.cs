using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrdersInvoicesListForm : ZChildForm
	{
		public OrdersInvoicesListForm(QueryUserFindboxEventArgs findBoxArgs)
			: base(findBoxArgs)
		{
			FindBox.ModuleID = findBoxArgs.ModuleID;
			this.zLabel1.Text = Res.GetString("PreAdviceInvoicesListForm|Description1", "Please select an invoice to add the orders line to. If an invoice with a matching");
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}

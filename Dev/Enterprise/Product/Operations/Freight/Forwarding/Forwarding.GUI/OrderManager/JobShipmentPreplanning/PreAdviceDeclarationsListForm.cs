using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class PreAdviceDeclarationsListForm : ZChildForm
	{
		public PreAdviceDeclarationsListForm(QueryUserFindboxEventArgs findBoxArgs)
			: base(findBoxArgs)
		{
			FindBox.ModuleID = findBoxArgs.ModuleID;
			this.zLabel1.Text = Res.GetString("PreAdviceDeclarationsListForm|Description1", "Please select a declaration to add the commercial invoice to. If a declaration with a matching");
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

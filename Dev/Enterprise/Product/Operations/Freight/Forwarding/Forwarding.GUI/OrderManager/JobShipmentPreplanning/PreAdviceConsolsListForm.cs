using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class PreAdviceConsolsListForm : ZChildForm
	{
		public PreAdviceConsolsListForm(QueryUserFindboxEventArgs findBoxArgs)
			: base(findBoxArgs)
		{
			FindBox.ModuleID = findBoxArgs.ModuleID;
			DescriptionLabel.Text = Res.GetString("fcbfead9-84b3-4afd-901b-8d9b90a9e475", "Please select a Consolidation to attach the Shipment to.");
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

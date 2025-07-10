using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class CancelRequestActionForm : ZChildForm
	{
		public CancelRequestActionForm()
		{
			InitializeComponent();
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 115, true);
		}

		public string Reason { get; set; }

		#region Implementation

		void zButtonAccept_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			Close();
		}

		void zButtonReject_Click(object sender, EventArgs e)
		{
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 165, true);
			zLabelRejectionReason.Visible = true;
			zTextBoxRejectionReason.Visible = true;
			zButtonSubmitRejectionReason.Visible = true;
			zTextBoxRejectionReason.Focus();
		}

		void zButtonSubmitRejectionReason_Click(object sender, EventArgs e)
		{
			Reason = zTextBoxRejectionReason.Text;
			if (string.IsNullOrEmpty(Reason))
			{
				Globals.Message.Show(Res.GetString("94248b0f-10e4-4037-b63b-776c23d8198c", "Please fill in a reason of rejection."), Res.GetString("8f510fb8-959f-46c1-bb98-02fa5fc38853", "Reason of rejection is mandatory."), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				DialogResult = DialogResult.No;
				Close();
			}
		}

		void zButtonCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}

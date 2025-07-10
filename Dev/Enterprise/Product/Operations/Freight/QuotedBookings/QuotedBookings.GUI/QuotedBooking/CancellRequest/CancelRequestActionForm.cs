using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class CancelRequestActionForm : ZChildForm
	{
		public string Reason { get; set; }

		public CancelRequestActionForm()
		{
			InitializeComponent();
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 115, true);
		}

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
				Globals.Message.Show(Res.GetString("2D8894BF-5D20-4E84-AAAF-C6E49790B99F", "Please fill in a reason of rejection."), Res.GetString("E7572A38-706F-4075-B64C-EABFB8C33058", "Reason of rejection is mandatory."), MessageBoxButtons.OK, MessageBoxIcon.Error);
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

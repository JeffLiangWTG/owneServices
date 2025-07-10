using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ExceptionReasonDialog : ZChildForm
	{
		public ExceptionReasonDialog(ForwardingShipmentProcessTask milestoneException)
			: base(milestoneException)
		{
			this.milestoneException = milestoneException;
		}

		#region Buttons

		void OKButtonX_Click(object sender, System.EventArgs e)
		{
			((CargoReportAcceptedExceptionValidation)(milestoneException.Validation)).ValidateLateCargoReport();
			if (!milestoneException.HasErrors)
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

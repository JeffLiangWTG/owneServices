using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class AllocateEntryInstructionsPopupForm : ZChildForm
	{
		public AllocateEntryInstructionsPopupForm(bool showOverwriteOrIgnoreChoices)
		{
			this.showOverwriteOrIgnoreChoices = showOverwriteOrIgnoreChoices;
			base.CancelButton = CancelButton;

			if (!showOverwriteOrIgnoreChoices)
			{
				var hiddenHeight = CancelButton.Top - AlreadyLinkedConfirmationCaptionLabel.Top;
				Height -= ControlDpiScalingHelper.MarkAsScaled(hiddenHeight);
				AlreadyLinkedConfirmationCaptionLabel.Visible = false;
				OverwriteRadioButton.Visible = false;
				IgnoreRadioButton.Visible = false;
			}

			CheckedChanged(this, System.EventArgs.Empty);
		}

		public bool? AllowOverwrite => IgnoreRadioButton.Checked ? false : (OverwriteRadioButton.Checked ? true : null);

		void CheckedChanged(object sender, System.EventArgs e)
		{
			OKButton.Enabled = !showOverwriteOrIgnoreChoices || AllowOverwrite != null;
		}

		readonly bool showOverwriteOrIgnoreChoices;
	}
}

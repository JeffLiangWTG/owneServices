using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public enum UpdateChoice
	{
		ThisRecord = 0,
		AllRecords = 1,
		Yes = 2,
		Cancel = 3
	}

	public partial class UpdateMultipleJobDocAddressesForm : ZChildForm
	{
		public UpdateMultipleJobDocAddressesForm(string title, bool showYesButton = true)
		{
			InitializeComponent();
			TitleLabel.Text = title;
			UpdateAllRecordsButton.Visible = !showYesButton;
			UpdateThisRecordButton.Visible = !showYesButton;
			YesButton.Visible = showYesButton;
			CancelSelectButton.Visible = true;
			CancelSelectButton.Location = showYesButton ? CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 15) : CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 15);
		}

		public UpdateChoice Choice { get; set; } = UpdateChoice.Cancel;

		void CancelButton_Click(object sender, EventArgs e)
		{
			Choice = UpdateChoice.Cancel;
			Close();
		}

		void UpdateAllRecordsButton_Click(object sender, EventArgs e)
		{
			Choice = UpdateChoice.AllRecords;
			Close();
		}

		void UpdateThisRecordButton_Click(object sender, EventArgs e)
		{
			Choice = UpdateChoice.ThisRecord;
			Close();
		}

		void YesButton_Click(object sender, EventArgs e)
		{
			Choice = UpdateChoice.Yes;
			Close();
		}
	}
}

using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class DpsMarkJobClearConfirmationForm : ZChildForm
	{
		readonly DpsMarkJobClearConfirmationModel confirmationModel;

		public DpsMarkJobClearConfirmationForm(DpsMarkJobClearConfirmationModel confirmationModel) : base(confirmationModel)
		{
			this.confirmationModel = confirmationModel;
			InitializeComponent();
			InitialiseImagesAndLabelText(confirmationModel.RelatedJobsIDNotJCLOrCLR?.Any() ?? false);
		}

		void InitialiseImagesAndLabelText(bool jobHasRelatedJobsNotJCLOrCLR)
		{
			WarningPictureBox.Image = Bitmap.FromHicon(SystemIcons.Warning.Handle);
			ReasonLabel.Text = Res.GetString("29A7875C-1B73-4648-B282-E2102081B7DB", "Please provide the clearing reason which will be stored in the audit logs");
			var relatedJobsNotJCLOrCLRMessage = jobHasRelatedJobsNotJCLOrCLR ? Res.GetString("4C9EF978-6D51-485D-B318-F5E1218D3C81", "The screening status of its related Job(s) {0} will also be marked as Job Clear.", string.Join(", ", confirmationModel.RelatedJobsIDNotJCLOrCLR)) : string.Empty;
			var spaceIfNeeded = jobHasRelatedJobsNotJCLOrCLR ? " " : string.Empty;
			WarningMessageLabel.Text = Res.GetString("043935DD-3DDB-4CAE-9C1E-6A1CEEF4CB38", "Marking Job {0} as JCL - Job Clear will allow the job to be progressed and override any document restrictions.{1}{2} This action should only be undertaken by a compliance officer who understands your companies compliance processes and your action will be recorded for auditing purposes.", confirmationModel.JobID, spaceIfNeeded, relatedJobsNotJCLOrCLRMessage);
		}

		protected override void OnLoad(EventArgs e)
		{
			this.WarningMessagePanel.Size = ControlDpiScalingHelper.NewScaledSize(600, 20 + ControlDpiScalingHelper.UnscaleFromCurrentDpiY(this.WarningMessageLabel.Height));
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(684, 190 + ControlDpiScalingHelper.UnscaleFromCurrentDpiY(this.WarningMessagePanel.Height));

			this.MaximumSize = ControlDpiScalingHelper.NewScaledSize(684, 500);
			if (this.Size == MaximumSize)
			{
				this.WarningMessagePanel.AutoSize = false;
				this.WarningMessagePanel.Size = ControlDpiScalingHelper.NewScaledSize(628, 311);
			}

			base.OnLoad(e);
		}

		public override string FormVerb => string.Empty;

		#region Check Confirm Input

		void ConfirmMergeText_TextChanged(object sender, EventArgs e)
		{
			OKButton.Enabled = ExpectedStringLabel.Text == ConfirmClearText.Text.ToUpper(CultureInfo.InvariantCulture);
			ExpectedStringLabel.UpdateInput(ConfirmClearText.Text);
		}

		void ConfirmMergeText_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (ConfirmClearText.SelectionStart < ExpectedStringLabel.Text.Length)
			{
				var expectedChar = ExpectedStringLabel.Text[ConfirmClearText.SelectionStart];
				var startPosition = ConfirmClearText.SelectionStart;
				if (char.IsLetter(e.KeyChar) && char.IsLetter(expectedChar))
				{
					if (ConfirmClearText.SelectionLength > 0)
					{
						ConfirmClearText.Text = ConfirmClearText.Text.Remove(ConfirmClearText.SelectionStart, ConfirmClearText.SelectionLength);
					}

					var charToInsert = char.IsUpper(expectedChar) ? char.ToUpper(e.KeyChar, CultureInfo.InvariantCulture) : char.ToLower(e.KeyChar, CultureInfo.InvariantCulture);
					ConfirmClearText.Text = ConfirmClearText.Text.Insert(startPosition, charToInsert.ToString());
					ConfirmClearText.SelectionStart = startPosition + 1;
					e.Handled = true;
				}
			}
		}

		#endregion

		void OKButton_Click(object sender, EventArgs e)
		{
			confirmationModel.RunPreSaveValidation();

			if (confirmationModel.HasErrors)
			{
				Globals.Message.ShowError(confirmationModel.GetErrors().ToMessageListString());
			}
			else
			{
				SetDialogResult(DialogResult.OK);
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			SetDialogResult(DialogResult.Cancel);
		}

		void SetDialogResult(DialogResult result)
		{
			DialogResult = result;
			Close();
		}
	}
}

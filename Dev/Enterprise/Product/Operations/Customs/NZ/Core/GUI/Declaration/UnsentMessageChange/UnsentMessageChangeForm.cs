namespace Enterprise.Customs.NZ.GUI.Declaration
{
	using System;
	using System.Windows.Forms;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.ZArchitecture.GUI;

	public partial class UnsentMessageChangeForm : ZChildForm
	{
		/// <summary>
		/// For the form designer only.
		/// </summary>
		UnsentMessageChangeForm()
		{
		}

		public UnsentMessageChangeForm(HeldMessageSyncInfo declaration)
			: base(declaration)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			ControlEnableState();
		}

		void ControlEnableState()
		{
			RemarksTextBox.Enabled = (LeaveMessageUnchangedRadioButton.Checked);
			ProceedButton.Enabled = (!RemarksTextBox.Enabled || !String.IsNullOrEmpty(RemarksTextBox.Text));
		}

		public virtual void ProceedButton_Click(object sender, EventArgs e)
		{
			DialogResult =
				(RecreateMessageRadioButton.Checked || LeaveMessageUnchangedRadioButton.Checked) ?
				DialogResult.Yes : DialogResult.Cancel;

			if (!(this.LeaveMessageUnchangedRadioButton.Checked && String.IsNullOrEmpty(RemarksTextBox.Text)))
			{
				Close();
			}
		}

		void MessageRadioButtonGroup_CheckedChanged(object sender, EventArgs e)
		{
			ControlEnableState();
		}

		void RemarksTextBox_TextChanged(object sender, EventArgs e)
		{
			ControlEnableState();
		}
	}
}

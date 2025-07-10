using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ReSequencerForm : ZChildForm
	{
		public ReSequencerForm(ReSequencer reSequencer)
			: base(reSequencer)
		{
			InitializeComponent();
		}

		public override string FormCaption => Res.GetString("{C7BAC9BA-7BEE-4565-B725-25BFF5A7D3B5}", "Re-Sequencer");
		public override string FormVerb => string.Empty;

		public new ReSequencer DataSource => (ReSequencer)base.DataSource;

		void ProceedButton_Click(object sender, System.EventArgs e)
		{
			DataSource.RunPreSaveValidation();
			if (DataSource.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("{F3E5776A-6A74-43B0-92C9-01F1AEE01B30}", "Please fix all errors before proceeding."));
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}

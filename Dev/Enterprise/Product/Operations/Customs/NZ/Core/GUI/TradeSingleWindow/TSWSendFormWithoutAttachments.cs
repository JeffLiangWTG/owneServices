using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class TSWSendFormWithoutAttachments : ZChildForm
	{
		public TSWSendFormWithoutAttachments(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		public TSWSendFormWithoutAttachments()
			: base()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return Res.GetString("C740924D-B8F1-4FEC-831C-C29488C3A38E", "Trade Single Window -"); }
		}

		public override string FormCaption
		{
			get { return Res.GetString("A0DB0FF4-D355-4816-836B-FFCA15D6A466", "Send"); }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (!BusinessEntity.HasMessageErrors() || Globals.Message.Show("There are message errors. Are you sure you wish to continue?", "Continue to send", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.OK)
			{
				DialogResult = DialogResult.OK;
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}

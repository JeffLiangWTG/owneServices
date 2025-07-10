using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow
{
	public partial class TSWSendFormWithAttachments : ZChildForm
	{
		public TSWSendFormWithAttachments(AdditionalMessageInformation additionalMessageInformation)
			: base(additionalMessageInformation)
		{
		}

		protected TSWSendFormWithAttachments()
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
			get { return Res.GetString("5D3480ED-A9D2-4390-B8CE-39136DD55A9F", "Trade Single Window -"); }
		}

		public override string FormCaption
		{
			get { return Res.GetString("C69C0081-F41C-479C-A1ED-79A2B314F729", "Send"); }
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

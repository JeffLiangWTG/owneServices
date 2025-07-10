using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class TaskClosedForm : ZChildForm
	{
		public TaskClosedForm(ProcessTask task)
			: base(task)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("DB48420C-08A7-4AC9-92AD-4FE694E2E17D", "Close?"); }
		}

		#region Buttons

		void OKButton_Click(object sender, EventArgs e)
		{
			if (!BusinessEntity.IsValidationSuspended)
			{
				BusinessEntity.RunPreSaveValidation();
			}

			if (!BusinessEntity.HasErrors())
			{
				DialogResult = DialogResult.Yes;
				Close();
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		void NoButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
		}

		#endregion
	}
}

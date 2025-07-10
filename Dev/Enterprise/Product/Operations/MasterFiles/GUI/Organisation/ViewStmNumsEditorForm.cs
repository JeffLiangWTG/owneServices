using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ViewStmNumsEditorForm : ZChildForm
	{
		public ViewStmNumsEditorForm(ViewStmNums stmNums)
			: base(stmNums)
		{
			InitializeComponent();
			DialogResult = DialogResult.Cancel;
		}

		#region Events

		void ValidateAndSaveButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#region Implementation

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion
	}
}

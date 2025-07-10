using System;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI
{
	public partial class TWCustomsNumberViewStmNumsEditorForm : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyEditorForm
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public TWCustomsNumberViewStmNumsEditorForm()
		{
			InitializeComponent();
		}

		public TWCustomsNumberViewStmNumsEditorForm(TWCustomsNumberViewStmNumsWrapper stmNums)
			: base(stmNums)
		{
			InitializeComponent();
			FountainNameTextBox.Visible = false;
			TopPanel.Visible = stmNums.IsBranchLevel;
		}
	}
}

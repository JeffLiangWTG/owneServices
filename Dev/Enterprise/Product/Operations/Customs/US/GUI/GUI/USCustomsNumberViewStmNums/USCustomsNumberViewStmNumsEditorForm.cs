using System;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI
{
	public partial class USCustomsNumberViewStmNumsEditorForm : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyEditorForm
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public USCustomsNumberViewStmNumsEditorForm()
		{
			InitializeComponent();
		}

		public USCustomsNumberViewStmNumsEditorForm(USCustomsNumberViewStmNumsWrapper stmNums)
			: base(stmNums)
		{
			InitializeComponent();
			FountainNameTextBox.Visible = false;
			TopPanel.Visible = stmNums.IsBranchLevel;
		}
	}
}

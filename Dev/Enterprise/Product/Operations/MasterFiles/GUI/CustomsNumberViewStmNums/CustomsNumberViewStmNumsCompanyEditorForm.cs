using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomsNumberViewStmNumsCompanyEditorForm : CustomsNumberViewStmNumsEditorForm
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public CustomsNumberViewStmNumsCompanyEditorForm()
		{
			InitializeComponent();
		}

		public CustomsNumberViewStmNumsCompanyEditorForm(CustomsNumberViewStmNumsCompanyWrapper wrapper)
			: base(wrapper)
		{
			InitializeComponent();
			TopPanel.Visible = wrapper.IsBranchLevel;
		}
	}
}

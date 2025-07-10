using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class WorkItemCustomFieldsControl : ZUserControl
	{
		public WorkItemCustomFieldsControl()
		{
			InitializeComponent();
			WorkItemProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("E910E276-9C15-4BAE-809F-87438854FE23", "To make use of this tab, please setup work item custom fields in Workflow Manager.");
		}
	}
}

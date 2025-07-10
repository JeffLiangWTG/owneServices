using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI;

sealed partial class ValidationToolTabPageUserControl : ZUserControl
{
	public ValidationToolTabPageUserControl()
	{
		InitializeComponent();
		ValidationToolUserControl.SetReadOnlyIncludingChildren();
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		if (dataSource is not IWorkflowProvider workflowProvider)
		{
			return;
		}
		var validationToolParent = new NonPersistentValidationToolParent(workflowProvider);
		ValidationToolUserControl.SetDataBinding(validationToolParent, "");
	}
}

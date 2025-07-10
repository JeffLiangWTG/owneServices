using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class AddProjectLogPopupForm : BaseProjectPopupForm
	{
		public AddProjectLogPopupForm(ProjectAction action)
			: base(action)
		{
			InitializeComponent();
			action.ActionType = ActionType.Log;
		}
	}
}

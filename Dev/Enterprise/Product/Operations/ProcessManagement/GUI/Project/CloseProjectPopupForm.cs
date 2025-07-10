using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class CloseProjectPopupForm : BaseProjectPopupForm
	{
		public CloseProjectPopupForm(ProjectAction action)
			: base(action)
		{
			InitializeComponent();
			action.ActionType = ActionType.Close;
		}
	}
}

using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ReOpenProjectPopupForm : BaseProjectPopupForm
	{
		public ReOpenProjectPopupForm(ProjectAction action)
			: base(action)
		{
			InitializeComponent();
			action.ActionType = ActionType.ReOpen;
		}
	}
}

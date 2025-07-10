using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MatchingTemplateForm : ZChildForm
	{
		public MatchingTemplateForm(IWorkflowItemCollection workflowProvider)
			: base(new MatchingTemplateView(workflowProvider))
		{
			InitializeComponent();
			this.HeadingLabel.Text = Res.GetString("MatchingTemplateForm|HeadingLabel", "The following Workflow Templates apply to [{0}]", workflowProvider);
		}

		public override string FormVerb => string.Empty;

#if DEBUG
		public void TasksGrid_MouseDoubleClick_ForTest() => TasksGrid_MouseDoubleClick(this, new MouseEventArgs(MouseButtons.Left, 2, 20, 20, 0));
#endif
		void TasksGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GridEntityFormOpener.OpenForm(TasksGrid, e, GetSelectedTemplate, ControllerIDs.ProcessTemplates);
		}

		BusinessObject GetSelectedTemplate()
		{
			var selectedBizo = (MatchingTemplateViewLine)TasksGrid.ListManager.GetCurrent();

			return selectedBizo?.Template;
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GridPopupWorkflowForm : ZTemplateForm
	{
		public GridPopupWorkflowForm(IWorkflowProvider workflowProvider, ZString workflowProviderReference)
			: base((IBusiness)workflowProvider)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(workflowProvider);
			MainTabControl.TabPages.Remove(MainTabPage);
			this.workflowProviderReference = workflowProviderReference;
		}
		readonly ZString workflowProviderReference;

		public override string FormCaption
		{
			get { return string.Format("{0} {1}", workflowProviderReference, Res.GetString("f3f9a0cd-7eb8-4ac9-be37-9b987da894d7", "Workflow / eDocs")); }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}
	}
}

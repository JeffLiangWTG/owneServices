namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed partial class FormWithMultipleWorkflowTabs
	{
		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			workflowTabPage1 = new ZWorkflowTabPage();
			workflowTabPage2 = new ZWorkflowTabPage();
			Controls.Add(TabControl);
			TabControl.TabPages.Add(workflowTabPage1);
			TabControl.TabPages.Add(workflowTabPage2);
		}

		public Enterprise.ZArchitecture.GUI.ZTabControl TabControl;
		ZWorkflowTabPage workflowTabPage1;
		ZWorkflowTabPage workflowTabPage2;
	}
}

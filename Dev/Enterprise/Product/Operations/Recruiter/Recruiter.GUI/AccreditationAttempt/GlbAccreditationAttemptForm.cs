using Enterprise.Integration.Recruiter;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public partial class GlbAccreditationAttemptForm : ZTemplateForm, IGlbAccreditationAttemptForm
	{
		public GlbAccreditationAttemptForm(GlbAccreditationAttempt attempt)
			: base(attempt)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(BusinessEntity as GlbAccreditationAttempt);
			MainTabPage.TabVisible = false;
			ControllerID = ControllerIDs.GlbAccreditationAttempt;
		}

		protected override bool SupportsEDocs => false;

		protected override bool AllowNew => false;

		protected override bool ShowNotesTab => false;

		void WorkflowTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.WorkflowTabPage.SuspendLayout();
			this.WorkflowTabPage.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(true);
		}
	}
}

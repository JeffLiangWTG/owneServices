using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefComplianceListForm : ZTemplateForm
	{
		public RefComplianceListForm(RefComplianceList refComplianceList) : base(refComplianceList)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(refComplianceList);
		}

		protected override bool AllowNew => false;

		protected override bool ShowAuditTab => true;
	}
}

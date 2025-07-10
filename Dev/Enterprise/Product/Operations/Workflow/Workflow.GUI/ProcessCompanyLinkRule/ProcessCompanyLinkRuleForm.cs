using CargoWise.Integration;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class ProcessCompanyLinkRuleForm : ZTemplateForm
	{
		ProcessCompanyLinkRuleForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("bb038993-6899-4115-ba3a-d9e020a0ea2d", "Template Company Rule"); }
		}

		public ProcessCompanyLinkRuleForm(ProcessCompanyLinkRule rule)
			: base(rule)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			((ProcessCompanyLinkRule)DataSource).CloneForOtherCompanies();
			base.Save(factories);
		}

		protected override bool ShowAuditTab => true;
	}
}

using Enterprise.ComplianceRisk.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class ComplianceRuleBulkUpdateConfirmForm : ZChildForm
	{
		public ComplianceRuleBulkUpdateConfirmForm(ComplianceRuleWrapper complianceRuleWrapper)
			: base(complianceRuleWrapper)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;
	}
}

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Organisation - Agent Referral Report")]
	public class TestAgentReferralTemplate : TemplateTestCase
	{
	}

	public class TestAgentReferralReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Agent Referral"; }
		}

		public override string Hint
		{
			get { return "The Organization – Agent Referral Report generates a list of organizations with contacts information. The contacts shown are those without a job category and those with a job category of CEO/Managing Director, CFO, CIO or IT Manager."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAgentReferralTemplate();
		}
	}
}

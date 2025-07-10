namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Job Profit - Global Forwarding and Customs Summary")]
	class TestGlobalJobProfitForwardingAndCustomsSummary : TemplateTestCase
	{
	}

	public class TestGlobalJobProfitForwardingAndCustomsSummarySetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - Global Forwarding and Customs Summary"; }
		}

		public override string Hint
		{
			get
			{
				return string.Empty;
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGlobalJobProfitForwardingAndCustomsSummary();
		}
	}
}

namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Job Status Summary Report")]
	public class TestJobStatusSummaryTemplate : TemplateTestCase
	{
	}

	public class TestJobStatusSummaryReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Job Status Summary"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Job Status Summary Report shows a summary of the status of each job invoicing tab.
Amongst other filters, this report can be filtered by Status Type E.G. Held, Working and Closed.

NB:  The Status field is available on the Job Invoicing Tab and can be used to manage and track jobs.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobStatusSummaryTemplate();
		}
	}
}

namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	public class TestJobStatusSummaryReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Status Summary"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Job Status Summary Report lets you review the Accounting Job Header status of each job.
Amongst other filters, this report can be filtered by Status Type E.G. Held, Working and Closed.

NB:  The Status field is available on the Job Invoicing Tab and can be used to manage and track jobs.
NB: This report includes both FORWARDING SHIPMENT and STANDALONE DECLARATION records";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new Freight.Forwarding.TestJobStatusSummaryTemplate();
		}
	}
}

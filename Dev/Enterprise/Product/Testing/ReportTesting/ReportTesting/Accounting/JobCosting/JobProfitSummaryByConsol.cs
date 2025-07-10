namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Job Profit Summary By Consol")]
	public class TestJobProfitSummaryByConsolReportTest : TemplateTestCase
	{
	}

	public class TestJobProfitSummaryByConsolReportTestMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Profit - Summary by Consol"; }
		}

		public override string Hint
		{
			get
			{
				return
@"The Job Profit - Summary by Consol Report displays a list of “Most Relevant” Forwarding Consols and can be used for analyzing the profitability of individual Consols.
The system decides the “Most Relevant Consol” based on the following rules:
When a shipment is attached to more than one Consol, the Consol showing on the report is dependent on the country of the currently logged in branch that is generating the report.
If the report is generated from the Export Branch, the system will pick the relevant Consol by the first departure.
If the report is generated from the Import Branch, the system will pick the relevant Consol by the last arrival.
For Transshipment or Third-Party branches, the Ports of Consols attached to the shipment will be compared to the country of the currently logged in company/branch in the following order:
- Arrival Consol.
- Departure Consol.
- First Consol.
The first matching Consol will be selected for the report.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobProfitSummaryByConsolReportTest();
		}
	}
}

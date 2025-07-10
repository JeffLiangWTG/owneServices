namespace Enterprise.ReportTesting.Freight.Agency
{
	using Enterprise.Freight.Agency.Module;
	using Enterprise.Freight.Business;

	[TemplateName("Job Profit - Shipping")]
	public class TestJobProfitReport : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			Factory.Save();
		}
	}

	public class JobProfitTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AgencyReports(); }
		}

		public override string Hint
		{
			get
			{
				return @"This report specifically targets the analysis of Job Profits for Shipping jobs. It supports an extensive range of shipping and invoicing job filters as well as four layout options.

The layout style selected when running the report determines the type of analysis and level of detail included in the report. Using this report you can now produce reports that analyze Revenue, WIP, Costs, Accruals and Job Profit by:
· Charge Code for each Job
· Totals for each Job
· Transaction Details by Job
· Summary by Client";
			}
		}

		public override string MenuName
		{
			get { return "Job Profit - Liner & Agency"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobProfitReport();
		}
	}
}

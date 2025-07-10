namespace Enterprise.ReportTesting.Rating
{
	using Enterprise.ReportTesting;

	[TemplateName("Communications Analysis - 12 Period Communication Count by Client and Method")]
	public class TestAnalysis12PeriodCallCountByClientAndTypeReport : TemplateTestCase
	{
	}

	public class TestAnalysis12PeriodCallCountByClientAndTypeReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Rating.Module.SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Communications Analysis - 12 Period Communication Count by Client and Method"; }
		}

		public override string Hint
		{
			get
			{
				return
					@"Use this report to review Communications recorded for your clients across a 12 month period.
For the 12 periods up to and including the nominated period, the report counts Communications by client and communication methods.
The report uses the actual date recorded against each communication when selecting records to include in the report.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAnalysis12PeriodCallCountByClientAndTypeReport();
		}
	}
}

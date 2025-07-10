namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Profit Loss Period Analysis For Reporting Book")]
	public class TestProfitLossPeriodAnalysisForReportingBook : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	public class TestProfitLossPeriodAnalysisForReportingBookMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.GLReportingBooksReport(); }
		}

		public override string MenuName
		{
			get { return "Reporting Book - Profit and Loss Period Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Reporting Book - Profit and Loss Periods Analysis is a multi-periods report.

For each Alternate Account, this report displays: 
  - Net Movement in each reporting period in the selected reporting year up to & including the selected reporting period. 
  - the YTD closing balance (i.e cumulative value)
  - DR balances and net DR movements are displayed as NEGATIVE values 
  - CR balances and net CR movements are displayed as POSITIVE values 

Note: This report can only be generated for Reporting Books in Local Reporting Currency only. ";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestProfitLossPeriodAnalysisForReportingBook();
		}
	}
}

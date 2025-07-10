namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("GL Trial Balance Period Analysis For Reportingbook")]
	public class TestTrialBalancePeriodAnalysisForReportingbook : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	public class TestTrialBalancePeriodAnalysisForReportingbookMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.GLReportingBooksReport(); }
		}

		public override string MenuName
		{
			get { return "Reporting Book - Trial Balance Period Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Reporting Book - Trial Balance Periods Analysis is a multi-periods report.

It will list the Opening Balance at the Start of the Reporting Year, Net Movement in each Period, then Year-to Date Closing Balance for each Alternate Account.

Note: This report can only be generated for Reporting Books in Local Reporting Currency only.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestTrialBalancePeriodAnalysisForReportingbook();
		}
	}
}

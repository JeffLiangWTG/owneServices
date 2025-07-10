namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("GL Profit And Loss Report")]
	public class TestGLProfitandLossReportTemplate : TemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			SelectAllOptionalTemplates();
			RunReport();
		}
	}

	public class TestGLProfitandLossReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.GLReports(); }
		}

		public override string MenuName
		{
			get { return "Profit And Loss Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Profit and Loss Report is a local currency report that identifies actual results for the selected accounting period and it's associated year to date balance. 
The report includes comparative Budget and Last Year values. Variance analysis columns can also be included in the analysis.

The report can be filtered by Accounting Period, Department and Branch.  
By default the report only lists those accounts with values to be displayed.
When running this report, you can also choose to include all general ledger accounts, including those with no transactions.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGLProfitandLossReportTemplate();
		}
	}
}

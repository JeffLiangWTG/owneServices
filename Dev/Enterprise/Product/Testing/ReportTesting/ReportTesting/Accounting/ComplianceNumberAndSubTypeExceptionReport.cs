namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Compliance Number and Sub Type Exception Report")]
	public class TestComplianceNumberAndSubTypeExceptionReport : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return true;
			}
		}
	}

	public class TestComplianceNumberAndSubTypeExceptionReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.GLReports(); }
		}

		public override string MenuName
		{
			get { return "Compliance Number and Sub Type Exception Report"; }
		}

		public override string Hint
		{
			get
			{
				return "This report lists Compliance Number and Compliance Sub Type allocation errors for Receivables and Payables INV, ADJ and CRD Transactions with Post Dates in a nominated date range. Use this report to find transactions with empty Compliance Numbers or Compliance Sub Types, as well as transactions with Compliance Numbers outside the accepted range configured for the Compliance Sub Type. Additionally, in countries where Compliance Numbers must be allocated to transactions in Post Date order, you can find all transactions where the numbers were not allocated correctly.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestComplianceNumberAndSubTypeExceptionReport();
		}
	}
}

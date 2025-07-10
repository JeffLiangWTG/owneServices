namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using NUnit.Framework;

	[TemplateName("Italy Receivables Transactions – Stamp Duty Analysis (Detail)")]
	public class TestItalyReceivablesReportDetail : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Transactions with NO SDL Event", "Transactions with an SDL Event" };
		}

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			SelectAllOptionalTemplates();
			RunReport();
		}
	}

	[TemplateName("Italy Receivables Transactions – Stamp Duty Working Paper (Summary)")]
	public class TestItalyReceivablesReportSummary : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Stamp Duty Working Paper" };
		}

		[ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();
			RunReport();
		}
	}
}

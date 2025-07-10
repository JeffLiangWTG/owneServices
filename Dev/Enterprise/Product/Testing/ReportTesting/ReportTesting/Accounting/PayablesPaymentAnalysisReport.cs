using System.Collections.Generic;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Payables Payment Analysis Report")]
	public class TestPayablesPaymentAnalysisReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Payment Analysis Report" };
		}
	}

	public class TestPayablesPaymentAnalysisReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Payables Payment Analysis Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report analyses AP Payment (PAY) transactions.
For a selected Post Date Range, this report lists Payments posted to your Payables ledger.
For each Payment listed, the report will also identify the transactions that paid (matched) that payment.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestPayablesPaymentAnalysisReport();
		}
	}
}

namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;

	[TemplateName("AR Outstanding Balances with Average Days to Payment Analysis")]
	public class TestAROutstandingBalancesWithAverageDaysToPaymentAnalysisReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "AR Outstanding & Avg Payment" };
		}
	}

	public class AROutstandingBalancesWithAverageDaysToPaymentAnalysisReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Outstanding Balances with Average Days to Payment Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"By Receivable account, this report calculates summary outstanding balances (in local currency) as at end of a nominated Accounting Period.
An analysis of disbursement and standard transactions including totals due, totals past due, age of oldest outstanding transaction and average days to payment can be optionally included in the report.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAROutstandingBalancesWithAverageDaysToPaymentAnalysisReport();
		}
	}
}

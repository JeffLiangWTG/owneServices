namespace Enterprise.ReportTesting.Accounting.Receivables
{
	using Enterprise.Accounting.Module;

	[TemplateName("AR Charge Code Analysis - Summary by Charge Code")]
	public class ARChargeCodeAnalysisSummarybyChargeCodeTemplateTest : TemplateTestCase
	{
	}

	public class ARChargeCodeAnalysisSummarybyChargeCodeReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Charge Code Analysis - Summary by Charge Code"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Receivables Charge Code Analysis Summary by Charge Code report is used to analyze total turnover by charge code. For each charge code it identifies totals by receivable account. The report analyses AR  Invoice, Credit Note and Adjustment Note transactions at the Transaction Line / Charge Code level.
Use this report to identify the contribution of each receivable account to each charge code.
This report also supports SIX Charge Group Analysis options (Customs, Forwarding, Warehouse, CFS, Transport and Other).  When you select any of the Charge Group Analysis filter options the report will include extra columns that dissect and total charges across charge groups relevant to each charge code.
This optional dissection supports a deeper analysis of you key revenue streams.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ARChargeCodeAnalysisSummarybyChargeCodeTemplateTest();
		}
	}
}

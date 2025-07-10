namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.Accounting.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("VAT Analysis - Summary Report for EU Companies")]
	public class TestVATAnalysisSummaryReportForEUCompanies : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			Factory.NewWithValidTestData<AccInvMsg>().A9_Code = "SMTH";
			Factory.Save();
		}
	}

	public class TestVATAnalysisSummaryReportForEUCompaniesMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new GLReports(); }
		}

		public override string MenuName
		{
			get { return "VAT Analysis - Summary Report for EU Companies"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report is a summary analysis of Input and Output VAT recorded against Receivables, Payables and Cashbook ledger transactions posted in a nominated period or date range.  Reverse VAT charges are also appropriately included and calculated in the report.  

ONLY AR and AP  INV, CRD and ADJ transactions; and Cashbook DPY and DRC transactions are summarized in this report as they are the only transactions against which VAT can be recorded.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestVATAnalysisSummaryReportForEUCompanies();
		}
	}
}

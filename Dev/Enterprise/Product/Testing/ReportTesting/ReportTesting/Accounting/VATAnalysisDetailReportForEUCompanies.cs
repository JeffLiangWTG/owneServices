namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.Accounting.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("VAT Analysis - Detail Report for EU Companies")]
	public class TestVATAnalysisDetailReportForEUCompanies : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			Factory.NewWithValidTestData<AccInvMsg>().A9_Code = "SMTH";
			Factory.Save();
		}
	}

	public class TestVATAnalysisDetailReportForEUCompaniesMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new GLReports(); }
		}

		public override string MenuName
		{
			get { return "VAT Analysis - Detail Report for EU Companies"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report is an analysis of Input and Output VAT recorded against Receivables, Payables and Cashbook ledger transactions posted in a nominated period or date range.  Reverse VAT charges are also appropriately included and calculated in the report.

ONLY transaction lines on INV, CRD and ADJ transactions in the AR & AP ledger are included.
ONLY transaction lines on DPY and DRC transactions in the CB Ledger are included.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestVATAnalysisDetailReportForEUCompanies();
		}
	}
}

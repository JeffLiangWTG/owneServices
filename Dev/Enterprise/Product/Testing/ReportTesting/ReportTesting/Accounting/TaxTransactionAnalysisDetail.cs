namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.Data;

	[TemplateName("Tax Transaction Analysis - Detail Report")]
	public class TestTaxTransactionAnalysisDetail : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			string insertAccInvMsg = "INSERT INTO dbo.AccInvMsg (A9_PK, A9_LocalMsg, A9_EnglishMsg, A9_Code, A9_Description, A9_RN_NKCountryCode, A9_SystemCreateTimeUtc, A9_SystemCreateUser, A9_SystemLastEditTimeUtc, A9_SystemLastEditUser)" +
							"VALUES(newid(), 'Test', 'Test', 'TST', 'Test', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(insertAccInvMsg);
			base.FillReportWithDefaultValues();
		}
	}

	public class TestTaxTransactionAnalysisDetailMenuSetupReceivables : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new Enterprise.Accounting.Module.ReceivReports();
			}
		}

		public override string MenuName
		{
			get { return "Tax Transaction Analysis - Detail Report"; }
		}

		public override string Hint
		{
			get
			{
				return
@"This report details the VAT/GST Type tax treatment of Revenue and Cost Charge Lines.
For a nominated period / date range, this report provides a detailed analysis of AR, AP and CB ledger Revenue and Cost Charge Lines.
Transaction Charge Line information can be filtered by Input / Output tax treatment, Transaction Ledger, Charge Line Tax ID, Tax Message and Transaction Branch.
Results can be returned grouped in different ways. ‘Group By’ options, include combinations of Input/Output, Charge Line Tax ID, Tax Message and AR/AP Location.
Use the related 'Tax Transaction - Analysis - Summary Report' to generate a summary version.
PLEASE NOTE: This report evaluates the revenue and cost Charge Lines recorded against Receivables and Payables INV, CRD and ADJ transactions; and Cash Book DPY and DRC transactions.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestTaxTransactionAnalysisDetail();
		}
	}

	public class TestTaxTransactionAnalysisDetailMenuSetupPayables : TestTaxTransactionAnalysisDetailMenuSetupReceivables
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new Enterprise.Accounting.Module.PayablesReports();
			}
		}
	}

	public class TestTaxTransactionAnalysisDetailMenuSetupGeneralLedger : TestTaxTransactionAnalysisDetailMenuSetupReceivables
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new Enterprise.Accounting.Module.GLReports();
			}
		}
	}
}

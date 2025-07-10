namespace Enterprise.ReportTesting.Accounting
{
	using CargoWise.Data;

	[TemplateName("Tax Transaction Analysis - Summary Report")]
	public class TestTaxTransactionAnalysisSummary : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			string insertAccInvMsg = "INSERT INTO dbo.AccInvMsg (A9_PK, A9_LocalMsg, A9_EnglishMsg, A9_Code, A9_Description, A9_RN_NKCountryCode, A9_SystemCreateTimeUtc, A9_SystemCreateUser, A9_SystemLastEditTimeUtc, A9_SystemLastEditUser)" +
							"VALUES(newid(), 'Test', 'Test', 'TST', 'Test', 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(insertAccInvMsg);
			base.FillReportWithDefaultValues();
		}
	}

	public class TestTaxTransactionAnalysisSummaryMenuSetupReceivables : ReportTestCase
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
			get { return "Tax Transaction Analysis - Summary Report"; }
		}

		public override string Hint
		{
			get
			{
				return
@"This report summarizes VAT/GST Type tax treatments recorded against Revenue and Cost Charge Lines.
The report supports three optional templates.
For a nominated period / date range, each template summarizes AR, AP and CB ledger transaction Charge Lines by either Charge Line Tax ID, and / or Tax Message and / or AR/AP Location.
Use the report filter options to limit results to Input / Output tax treatment, Transaction Ledger, Charge Line Tax ID, Tax Message and Transaction Branch.
Use the related ‘Tax Transaction Analysis – Detail Report’ to generate a report itemizing each Charge Line.
PLEASE NOTE: This report evaluates the revenue and cost Charge Lines recorded against Receivables and Payables INV, CRD and ADJ transactions; and Cash Book DPY and DRC transactions.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestTaxTransactionAnalysisSummary();
		}
	}

	public class TestTaxTransactionAnalysisSummaryMenuSetupPayables : TestTaxTransactionAnalysisSummaryMenuSetupReceivables
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get
			{
				return new Enterprise.Accounting.Module.PayablesReports();
			}
		}
	}

	public class TestTaxTransactionAnalysisSummaryMenuSetupGeneralLedger : TestTaxTransactionAnalysisSummaryMenuSetupReceivables
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

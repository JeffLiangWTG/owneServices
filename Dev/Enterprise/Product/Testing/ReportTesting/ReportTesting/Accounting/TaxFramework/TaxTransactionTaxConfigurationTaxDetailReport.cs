using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ReportTesting.Accounting.TaxFramework
{
	[TemplateName("Tax Transaction - Tax Configuration Tax Detail")]
	public class TaxTransactionTaxConfigurationTaxDetailReport : TemplateTestCase
	{
	}

	public class TaxTransactionTaxConfigurationTaxDetailReport_Receivables : ReportTestCase
	{
		public override string MenuName => "Tax Transaction - Tax Configuration Tax Detail";

		public override ZEmbeddedModule ModuleToTest => new Enterprise.Accounting.Module.ReceivReports();

		public override string Hint => @"This report details Tax Configuration Tax Transaction records in the current Login Company.
For a nominated period / date range, the report lists each Tax Transaction record recorded against Invoice and Credit Note Transactions.
The report identifies the Tax System, Tax Authority, Posted Date, Realized Date, value of each Tax Transaction and the related Invoice or Credit Note transaction.
Report filter options include Tax Configuration, Transaction Ledger, Tax Super Type, Transaction Branch, Tax Record Post Date and / or Realized Date.
This report is relevant when Tax Configurations are enabled on the Company and / or Branch records.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TaxTransactionTaxConfigurationTaxDetailReport();
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "TF=Y");
	}

	public class TaxTransactionTaxConfigurationTaxDetailReport_Payables : ReportTestCase
	{
		public override string MenuName => "Tax Transaction - Tax Configuration Tax Detail";

		public override ZEmbeddedModule ModuleToTest => new Enterprise.Accounting.Module.PayablesReports();

		public override string Hint => @"This report details Tax Configuration Tax Transaction records in the current Login Company.
For a nominated period / date range, the report lists each Tax Transaction record recorded against Invoice and Credit Note Transactions.
The report identifies the Tax System, Tax Authority, Posted Date, Realized Date, value of each Tax Transaction and the related Invoice or Credit Note transaction.
Report filter options include Tax Configuration, Transaction Ledger, Tax Super Type, Transaction Branch, Tax Record Post Date and / or Realized Date.
This report is relevant when Tax Configurations are enabled on the Company and / or Branch records.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TaxTransactionTaxConfigurationTaxDetailReport();
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "TF=Y");
	}

	public class TaxTransactionTaxConfigurationTaxDetailReport_GL : ReportTestCase
	{
		public override string MenuName => "Tax Transaction - Tax Configuration Tax Detail";

		public override ZEmbeddedModule ModuleToTest => new Enterprise.Accounting.Module.GLReports();

		public override string Hint => @"This report details Tax Configuration Tax Transaction records in the current Login Company.
For a nominated period / date range, the report lists each Tax Transaction record recorded against Invoice and Credit Note Transactions.
The report identifies the Tax System, Tax Authority, Posted Date, Realized Date, value of each Tax Transaction and the related Invoice or Credit Note transaction.
Report filter options include Tax Configuration, Transaction Ledger, Tax Super Type, Transaction Branch, Tax Record Post Date and / or Realized Date.
This report is relevant when Tax Configurations are enabled on the Company and / or Branch records.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TaxTransactionTaxConfigurationTaxDetailReport();
		}

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "TF=Y");
	}
}

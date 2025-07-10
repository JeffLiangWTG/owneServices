namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Module;

	[TemplateName("Stock Balance By Movement Reference Number Report")]
	public class StockBalanceByMovementReferenceNumberReportTest : WhsTemplateTestCase
	{
	}

	public class StockBalanceByMovementReferenceNumberReport_ReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new ReportModule();

		public override string MenuName => "Stock Balance Reports by MRN";

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase() => new StockBalanceByMovementReferenceNumberReportTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("DE");
		}
	}
}

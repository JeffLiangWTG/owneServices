using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("Warehouse Operator Transaction Recon Report")]
	public class TestWarehouseOperatorTransactionReconTemplate : TemplateTestCase
	{
	}

	public class WarehouseOperatorTransactionReconReportMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Warehouse Operator Transaction Recon Report";

		public override string Hint => "This report shows all outbound transactions processed on the weekly run versus the available qty versus the allocation versus the declaration";

		protected override TemplateTestCase GetTemplateTestCase() => new TestWarehouseOperatorTransactionReconTemplate();
	}
}

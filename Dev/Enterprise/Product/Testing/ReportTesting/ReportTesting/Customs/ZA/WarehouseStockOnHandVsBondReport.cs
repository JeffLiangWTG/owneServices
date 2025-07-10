using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("Warehouse Stock on Hand vs Bond Store (without Duty and VAT)")]
	public class TestWarehouseStockOnHandVsBondStoreWithoutDutyVATTemplate : TemplateTestCase
	{
	}

	public class TestWarehouseStockOnHandVsBondStoreWithoutDutyVATMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Warehouse Stock on Hand vs Bond Store (without Duty and VAT)";

		public override string Hint => "This report shows stock on hand versus bonded stock from products without duties or VAT in the warehouse.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestWarehouseStockOnHandVsBondStoreWithoutDutyVATTemplate();
	}

	[TemplateName("Warehouse Stock on Hand vs Bond Store (with Duty and VAT)")]
	public class TestWarehouseStockOnHandVsBondStoreWithDutyVATTemplate : TemplateTestCase
	{
	}

	public class TestWarehouseStockOnHandVsBondStoreWithDutyVATMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Warehouse Stock on Hand vs Bond Store (with Duty and VAT)";

		public override string Hint => "This report shows stock on hand versus bonded stock from products with duties or VAT in the warehouse.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestWarehouseStockOnHandVsBondStoreWithDutyVATTemplate();
	}
}

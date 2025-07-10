using Enterprise.Warehouse.Transactions.Module;

namespace Enterprise.ReportTesting.Warehouse
{
	[TemplateName("Whs Inventory Accuracy By Location Report")]
	public class TestWhsInventoryAccuracyByLocationReport : TestWhsInventoryAccuracyReport
	{
	}

	public class TestWhsInventoryAccuracyByLocationReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new ReportModule();

		public override string MenuName => "Inventory Accuracy By Location Report";

		public override string Hint => @"This report shows the inventory accuracy (by location) of cycle counting at the time of printing the report. When you need the accuracy of cycle counting on the stock you currently have in your warehouses and where it is located, this is the report to use.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestWhsInventoryAccuracyByLocationReport();
	}
}

namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.ReportTesting;

	[TemplateName("Whs Stock Replenishment")]
	public class TestStockReplenishmentReportTemplate : WhsTemplateTestCase
	{
	}

	public class TestStockReplenishmentReportReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Warehouse.Transactions.Module.ReportModule(); }
		}

		public override string MenuName
		{
			get { return @"Stock Replenishment Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to identify Stock Replenishment requirements by Client and Warehouse.
The report shows stock lines where the quantity available is less than the warehouse Replenishment Minimum required by each product for a client.
Only stock with a Replenishment Minimum defined on the Product Profile will be listed in this report.
Run this report regularly to see what stock needs to be re ordered for your clients.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestStockReplenishmentReportTemplate();
		}
	}
}

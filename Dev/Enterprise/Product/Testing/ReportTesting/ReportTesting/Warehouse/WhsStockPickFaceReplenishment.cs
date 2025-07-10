namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.ReportTesting;

	[TemplateName("Whs Stock Pick Face Replenishment")]
	public class TestStockPickFaceReplenishmentReportTemplate : WhsTemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	public class TestStockPickFaceReplenishmentReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Warehouse.Transactions.Module.ReportModule(); }
		}

		public override string MenuName
		{
			get { return @"Stock Pick Face Replenishment Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report is used to identify Pick Faces that are below their minimum inventory levels.
Only Products defined with a Pick Face in the product master are included in this report. Run this report regularly to monitor Pick Faces requiring replenishment.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestStockPickFaceReplenishmentReportTemplate();
		}
	}
}

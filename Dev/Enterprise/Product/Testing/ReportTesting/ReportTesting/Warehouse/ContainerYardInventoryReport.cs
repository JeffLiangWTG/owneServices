namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.Warehouse.Yard.Module;

	[TemplateName("Inventory Report")]
	public class TestContainerYardInventoryReportTemplate : TemplateTestCase
	{
	}

	public class TestContainerYardInventoryReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new YardReportModule(); }
		}

		public override string MenuName
		{
			get { return "Inventory Report"; }
		}

		public override string Hint => "";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestContainerYardInventoryReportTemplate();
		}
	}
}

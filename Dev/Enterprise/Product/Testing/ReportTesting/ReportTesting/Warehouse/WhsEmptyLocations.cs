namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.Warehouse.Transactions.Module;
	using Enterprise.ZArchitecture.Modules;

	[TemplateName("Whs Empty Locations")]
	public class TestWhsEmptyLocationsReport : WhsTemplateTestCase
	{
	}

	public class TestWhsEmptyLocationsReportMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Warehouse Empty Locations Report"; }
		}

		public override string Hint
		{
			get { return @"Use the Warehouse Empty Locations Report to determine the number of empty Locations in a warehouse and to determine warehouse utilization."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsEmptyLocationsReport();
		}
	}
}

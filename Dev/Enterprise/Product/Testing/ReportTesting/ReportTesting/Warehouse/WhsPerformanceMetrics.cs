namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.ReportTesting;

	[TemplateName("Whs Job Performance Report")]
	public class TestWhsPerformanceMetricsTemplate : WhsTemplateTestCase
	{
		protected override string TemplateFileType => ".xlsx";
	}

	public class TestWhsPerformanceMetricsReportReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Warehouse.Transactions.Module.ReportModule(); }
		}

		public override string MenuName
		{
			get { return @"Job Performance Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report displays performance metrics to measure key performance indicators in your warehouses and compare your business to industry standards.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsPerformanceMetricsTemplate();
		}
	}
}

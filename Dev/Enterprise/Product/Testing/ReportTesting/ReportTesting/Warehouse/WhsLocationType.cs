namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Module;

	[TemplateName("Whs Location Type")]
	public class TestWhsLocationTypeReport : WhsTemplateTestCase
	{
	}

	public class TestWhsLocationTypeReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Location Type Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Location Type report allows comprehensive reporting on locations by Location Type. It also has multiple run time options for the purpose of including or excluding Pick Face configurations and or Inventory data. The report can also include the product weight and volume and the location weight and volume capacity.

In addition, the Inventory search screen has had fields from the Location master added, allowing search by Location Type, Status, Pick Method and more.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsLocationTypeReport();
		}
	}
}

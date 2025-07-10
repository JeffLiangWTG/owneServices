namespace Enterprise.ReportTesting.SystemTest
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Listing of CargoWise Reports")]
	public class TestListingOfReportsTemplate : TemplateTestCase
	{
	}

	public class TestListingOfReportsReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new SystemReports(); }
		}

		public override string MenuName
		{
			get { return "Listing of CargoWise Reports"; }
		}

		public override string Hint
		{
			get { return @"This report will itemize all reports currently available from any 'Reports' menu in your CargoWise system."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestListingOfReportsTemplate();
		}
	}
}

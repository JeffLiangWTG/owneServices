namespace Enterprise.ReportTesting.SystemTest
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Listing of CargoWise System DocBuilder Documents")]
	public class TestListingOfCargoWiseSystemDocBuilderDocumentsTemplate : TemplateTestCase
	{
	}

	public class TestListingOfCargoWiseSystemDocBuilderDocumentsReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new SystemReports(); }
		}

		public override string MenuName
		{
			get { return "Listing of CargoWise System DocBuilder Documents"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report will itemize all system documents using DocBuilder that are currently available from any business context in your CargoWise system.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestListingOfCargoWiseSystemDocBuilderDocumentsTemplate();
		}
	}
}

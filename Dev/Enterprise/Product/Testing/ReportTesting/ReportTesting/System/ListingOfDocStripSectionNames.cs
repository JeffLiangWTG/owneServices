namespace Enterprise.ReportTesting.SystemTest
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Listing of DocStrip Section Names")]
	public class ListingOfDocStripSectionNamesTemplate : TemplateTestCase
	{
	}

	public class ListingOfDocStripSectionNamesReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new SystemReports(); }
		}

		public override string MenuName
		{
			get { return "Listing of Doc Strip Section Names"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to identify both CargoWise System and Customized Doc Strip Sections in your CargoWise system. 
The report identifies all Doc Strip Sections in your system whether defined in the CargoWise System Document Elements template or a Customized Document Elements Template.
Use the report to understand the full scope of Doc Strip Section customization in your system.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ListingOfDocStripSectionNamesTemplate();
		}
	}
}

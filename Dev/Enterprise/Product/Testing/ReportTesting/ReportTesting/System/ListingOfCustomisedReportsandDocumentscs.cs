namespace Enterprise.ReportTesting.SystemTest
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Listing of Customised Reports and Documents")]
	public class TestListingofCustomisedReportsandDocumentsTemplate : TemplateTestCase
	{
	}

	public class TestListingofCustomisedReportsandDocumentsReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new SystemReports(); }
		}

		public override string MenuName
		{
			get { return "Listing of Customized Reports and Documents"; }
		}

		public override string Hint
		{
			get { return @"Use this report to identify Customized Reports and Documents in your CargoWise system.  There are TWO types of customization:

1. Customization added and maintained by WiseTech Global. 
These 'Client Specific' customizations are developed and maintained for your CargoWise system by WiseTech Global.
This report will identify these 'Client Specific' reports and documents.

2. Customization added and maintained by your own staff. 
These 'User Site Created' customizations are developed and maintained by your own staff and are not covered by any CargoWise maintenance arrangements.
This report will identify these 'User Site Created' reports and documents."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestListingofCustomisedReportsandDocumentsTemplate();
		}
	}
}

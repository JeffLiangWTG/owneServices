namespace Enterprise.ReportTesting.MasterFiles
{
	using Enterprise.ReportTesting;

	[TemplateName("Organisation By Added Date Report")]
	public class TestOrganisationByAddedDateReport : TemplateTestCase
	{
	}

	public class TestOrganisationByAddedDateReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - By Added Date"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"The Organization - This report lists all Organizations added within a selected data range.
It prints Organization Code, Name, Address, Phone, Temporary Account flag, Receivables flag, Payables flag, Added Date and the User who added the Organization";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestOrganisationByAddedDateReport();
		}
	}
}

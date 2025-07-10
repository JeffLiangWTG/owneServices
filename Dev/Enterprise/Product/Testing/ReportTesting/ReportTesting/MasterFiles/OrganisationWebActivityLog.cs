namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Web Activity Log")]
	public class TestOrganisationWebActivityLogTemplate : TemplateTestCase
	{
	}

	public class TestOrganisationWebActivityLogReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Web Activity Log"; }
		}

		public override string Hint
		{
			get
			{
				return "The Organization – Web Activity Log Profile Report tracks the usage of WebTracker for a given organization in a time frame.\r\n\r\nThis report requires the Web > Performance and Appearance > Web Activity Logging settings in the Registry to be turned on.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestOrganisationWebActivityLogTemplate();
		}
	}
}

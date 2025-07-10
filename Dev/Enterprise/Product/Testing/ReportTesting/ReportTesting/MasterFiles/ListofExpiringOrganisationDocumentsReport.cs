namespace Enterprise.ReportTesting.MasterFiles
{
	using Enterprise.ReportTesting;

	[TemplateName("List of Expiring Organisation Documents")]
	public class TestListofExpiringOrganisationDocumentsReportTemplate : TemplateTestCase
	{
	}

	public class TestListofExpiringOrganisationDocumentsReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "List of Expiring Organization Documents"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to generate a list of Organization documents with expiry dates in a nominated date range.
The report identifies all documents recorded on the eDocs tab of your Organization records that have 'Valid to Dates' in the selected date range. The report supports your ensuring that your client documents are current and up to date at all times.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestListofExpiringOrganisationDocumentsReportTemplate();
		}
	}
}

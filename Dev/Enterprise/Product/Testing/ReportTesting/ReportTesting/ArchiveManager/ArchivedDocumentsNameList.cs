using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Archived Documents Name List Report")]
	class ArchivedReportsNameListTemplateTest : TemplateTestCase
	{
	}

	class ArchivedReportsNameListReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return "This report shows all the documents marked for generation during the archiving process."; }
		}

		public override string MenuName
		{
			get { return "Archived Documents Name List Report"; }
		}

		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ArchiveReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ArchivedReportsNameListTemplateTest();
		}
	}
}

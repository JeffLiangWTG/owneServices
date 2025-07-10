namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("Import Declaration Hold Report")]
	public class TestImportDeclarationHoldReport : TemplateTestCase
	{
		public void TestReportSql()
		{
			PrepareReportForRender();
			AssertEquals("ReportData: SELECT <ReportData.SelectList> FROM USEntriesOnHoldStats(<CurrentCompany>, <Job Registered On.FromDateUtc>, <Job Registered On.ToNextDateUtc>, <Importer>) ORDER BY Year, Month",
				Report.Analyser.ReportSQLSources[0].TableNameAndSelectStatement);
		}
	}

	public class TestImportDeclarationHoldReportMenuSetup : ReportTestCase
	{
		public override string MenuName => "Import Declaration Hold Report";

		public override string Hint => "This report displays Hold Summary Information for import declarations in addition to listing the related declarations.";

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "1=0");

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		protected override TemplateTestCase GetTemplateTestCase() => new TestImportDeclarationHoldReport();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
		}
	}
}

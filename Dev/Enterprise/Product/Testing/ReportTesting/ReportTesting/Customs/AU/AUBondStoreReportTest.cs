namespace Enterprise.ReportTesting.Customs.AU
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("AU Bond Store Report")]
	public class AUBondStoreReportTemplateTest : TemplateTestCase
	{
	}

	public class AUBondStoreReportTest : ReportTestCase
	{
		public override string MenuName => "Bond Store Report";

		public override string Hint => @"This report lists air and sea cargo details that are outturned. Zero landed cargos are not included.";

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, "CTY=AU");

		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AUBondStoreReportTemplateTest();
		}
	}
}

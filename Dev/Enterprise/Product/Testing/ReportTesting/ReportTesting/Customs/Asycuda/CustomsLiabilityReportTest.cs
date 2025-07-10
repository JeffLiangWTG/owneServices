namespace Enterprise.ReportTesting.Customs.Asycuda
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	[TemplateName("Risk Report")]
	public class CustomsLiabilityReportTemplateTest : TemplateTestCase
	{
	}

	public class CustomsLiabilityReportTest : ReportTestCase
	{
		public override string MenuName => "Customs Liability Report";

		public override string Hint => @"";

		protected override ZQuery AdditionalCandidateMenuItemsFilter => new DocumentZQuery(StmMenuItemSchema.SU_FilterList, @"""<IsFunctionalityValid(RISK)>""==""Y""");

		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new CustomsLiabilityReportTemplateTest();
		}
	}
}

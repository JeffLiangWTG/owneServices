namespace Enterprise.ReportTesting.Customs.IT
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("IT Export Exit Status Report")]
	class ITExportExitStatusReportTemplateTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
		}
	}

	class ITExportExitStatusReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Export exit status report";

		public override string Hint => "This report is a list of registered export customs declarations, sorted by MRN. It is possible to include/exclude declarations with the IVISTO message issued on exit.";

		protected override TemplateTestCase GetTemplateTestCase() => new ITExportExitStatusReportTemplateTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
		}
	}
}

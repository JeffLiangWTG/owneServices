namespace Enterprise.ReportTesting.Customs.IT
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("IT Entry Pay Info Report")]
	class ITEntryPayInfoReportTemplateTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
		}
	}

	class ITEntryPayInfoReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "A93 Control Report";

		public override string Hint => "A report that can be run to include Customs Duty, VAT & other taxes, for Customs and NCTS Declarations, ordered by A93 number on a specified Deferred Account.";

		protected override TemplateTestCase GetTemplateTestCase() => new ITEntryPayInfoReportTemplateTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
		}
	}
}

namespace Enterprise.ReportTesting.Customs.IT
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("IT Customs Taxes Control Report")]
	class ITCustomsTaxesControlReportTemplateTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
		}
	}

	class ITCustomsTaxesControlReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "Customs Taxes Control Report";

		public override string Hint => "A report that can be run to include Customs Duty, VAT & other taxes, with reference to taxes paid and taxes charged to customers.";

		protected override TemplateTestCase GetTemplateTestCase() => new ITCustomsTaxesControlReportTemplateTest();

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);
		}
	}
}

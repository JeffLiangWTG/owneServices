namespace Enterprise.ReportTesting.Customs.ImporterSecurityFiling
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Importer Security Filing Header Report")]
	public class ISFHeadersReportTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}

	public class ISFHeadersReportMenuSetupTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsGlobalReportModule(); }
		}

		public override string MenuName
		{
			get { return "Importer Security Filing Header Report"; }
		}

		public override string Hint
		{
			get { return "This report makes Importer Security Filing Header information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ISFHeadersReportTest();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}

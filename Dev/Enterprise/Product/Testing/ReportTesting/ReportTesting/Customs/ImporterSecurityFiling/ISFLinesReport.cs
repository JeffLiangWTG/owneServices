namespace Enterprise.ReportTesting.Customs.ImporterSecurityFiling
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Importer Security Filing Line Report")]
	public class ISFLinesReportTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}

	public class ISFLinesReportMenuSetupTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsGlobalReportModule(); }
		}

		public override string MenuName
		{
			get { return "Importer Security Filing Line Report"; }
		}

		public override string Hint
		{
			get { return "This report makes Importer Security Filing Line information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ISFLinesReportTest();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}

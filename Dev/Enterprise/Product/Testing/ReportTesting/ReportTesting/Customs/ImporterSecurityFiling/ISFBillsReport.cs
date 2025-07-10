namespace Enterprise.ReportTesting.Customs.ImporterSecurityFiling
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Importer Security Filing Bill Report")]
	public class ISFBillsReportTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}

	public class ISFBillsReportMenuSetupTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsGlobalReportModule(); }
		}

		public override string MenuName
		{
			get { return "Importer Security Filing Bill Report"; }
		}

		public override string Hint
		{
			get { return "This report makes Importer Security Filing Bill information available."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ISFBillsReportTest();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}

namespace Enterprise.ReportTesting.Customs.CA
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("CA RNS Report")]
	public class TestCARNSReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}

	public class TestCARNSReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "RNS Report"; }
		}

		public override string Hint
		{
			get { return ""; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCARNSReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}
}

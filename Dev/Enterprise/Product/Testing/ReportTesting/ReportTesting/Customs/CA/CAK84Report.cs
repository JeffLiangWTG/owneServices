namespace Enterprise.ReportTesting.Customs.CA
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("CA K84 Report")]
	public class TestCAK84Report : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}

	public class TestCAK84ReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "ARL Report"; }
		}

		public override string Hint
		{
			get { return "This report may be used to list transactions reported on DNs (Daily Notices) received from Customs. It may also be used to list legacy Daily K84 data."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCAK84Report();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}
}

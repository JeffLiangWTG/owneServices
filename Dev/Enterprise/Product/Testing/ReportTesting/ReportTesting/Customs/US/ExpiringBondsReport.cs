namespace Enterprise.ReportTesting.Customs.US
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Expiring Bonds Report")]
	public class TestExpiringBondsReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}

	public class TestExpiringBondsReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Expiring Bonds Report"; }
		}

		public override string Hint
		{
			get { return ""; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestExpiringBondsReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}

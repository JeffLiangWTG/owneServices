namespace Enterprise.ReportTesting.Customs.FR
{
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("New FR Delta G Regularization Report")]
	public class TestFRDeltaGRegularizationReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("FR");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<BaseJobDeclaration>();
			Factory.Save();
		}
	}

	public class TestFRDeltaGRegularizationReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "New FR Delta G Regularization Report"; }
		}

		public override string Hint
		{
			get { return string.Empty; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestFRDeltaGRegularizationReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("FR");
		}
	}
}

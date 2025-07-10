namespace Enterprise.ReportTesting.Customs.US
{
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Import Invoice Line PGA Report - APHIS")]
	public class TestUSInvoiceLinePGAAPHISReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			var declaration = Factory.New<Integration.Customs.US.IJobDeclaration>();
			((BusinessObject)declaration).FillWithValidTestData();
			Factory.Save();
		}
	}

	public class TestUSInvoiceLinePGAAPHISReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();
		public override string MenuName => "Import Invoice Line PGA Report - APHIS";
		public override string Hint => "Import Invoice Line PGA Report lists detailed PGA data related with Import Invoice Lines";
		protected override TemplateTestCase GetTemplateTestCase() => new TestUSInvoiceLinePGAAPHISReport();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override bool ExpectedIsPublished => true;
	}
}

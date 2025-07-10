namespace Enterprise.ReportTesting.Customs.CA
{
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("CA Export Invoice Line Report")]
	public class TestCAExportInvoiceLinesReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}

		protected override string TemplateFileType => @".xlsx";
	}

	public class TestCAExportInvoiceLinesReportMenuSetup : ReportTestCase
	{
		public override string MenuName => "CA Export Invoice Line Report";

		public override string Hint => "This report makes export declaration invoice line information available.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCAExportInvoiceLinesReport();
		}

		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}
	}
}

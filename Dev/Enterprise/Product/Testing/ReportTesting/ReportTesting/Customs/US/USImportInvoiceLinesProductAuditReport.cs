namespace Enterprise.ReportTesting.Customs.US
{
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;

	[TemplateName("Import Invoice Lines Product Audit Report (US)")]
	public class TestUSImportInvoiceLinesProductAuditReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}

		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<BaseJobDeclaration>();
			Factory.Save();
		}
	}

	public class TestUSImportInvoiceLinesProductCodeAuditReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Import Invoice Lines Product Audit Report (US)"; }
		}

		public override string Hint
		{
			get { return "Report that compares invoice lines created to current product data."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestUSImportInvoiceLinesProductAuditReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("US");
		}
	}
}

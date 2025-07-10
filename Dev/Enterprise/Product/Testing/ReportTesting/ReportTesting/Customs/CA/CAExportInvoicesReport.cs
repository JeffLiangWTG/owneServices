namespace Enterprise.ReportTesting.Customs.CA
{
	using CargoWise.Types;
	using Enterprise.Customs.Module;
	using Enterprise.MasterFiles.Business;

	[TemplateName("CA Export Invoice Report")]
	public class TestCAExportInvoicesReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("CA");
		}

		protected override string TemplateFileType => @".xlsx";
	}

	public class TestCAExportInvoicesReportMenuSetup : ReportTestCase
	{
		public override string MenuName => "CA Export Invoice Report";

		public override string Hint => ZString.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCAExportInvoicesReport();
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

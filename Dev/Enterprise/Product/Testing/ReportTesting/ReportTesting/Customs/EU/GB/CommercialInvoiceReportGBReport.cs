using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("Commercial Invoice Report (GB)")]
	public class TestCommercialInvoiceReportGBReportTemplate : TemplateTestCase
	{
	}

	public class TestCommercialInvoiceReportGBReportReport : ReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.CustomsReport;

		public override string MenuName
		{
			get { return "Commercial Invoice Report (GB)"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCommercialInvoiceReportGBReportTemplate();
		}
	}
}

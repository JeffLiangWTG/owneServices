using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("Customs Declarations GB Export")]
	public class TestCustomsDeclarationsGBExportReportTemplate : TemplateTestCase
	{
	}

	public class TestCustomsDeclarationsGBExportReportReport : ReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.CustomsReport;

		public override string MenuName
		{
			get { return "Customs Declarations GB Export"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCustomsDeclarationsGBExportReportTemplate();
		}
	}
}

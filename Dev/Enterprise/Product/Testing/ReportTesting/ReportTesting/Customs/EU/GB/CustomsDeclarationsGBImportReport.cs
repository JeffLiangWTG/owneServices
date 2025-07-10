using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("Customs Declarations GB Import")]
	public class TestCustomsDeclarationsGBImportReportTemplate : TemplateTestCase
	{
	}

	public class TestCustomsDeclarationsGBImportReportReport : ReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.CustomsReport;

		public override string MenuName
		{
			get { return "Customs Declarations GB Import"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCustomsDeclarationsGBImportReportTemplate();
		}
	}
}

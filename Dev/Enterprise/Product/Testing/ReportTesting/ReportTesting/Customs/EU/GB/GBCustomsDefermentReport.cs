using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("GB Customs Deferment Report")]
	public class TestGBCustomsDefermentReportTemplate : TemplateTestCase
	{
	}

	public class TestGBCustomsDefermentReportReport : ReportTestCase
	{
		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.CustomsReport;

		public override string MenuName
		{
			get { return "GB Customs Deferment Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"Shows details and summary of taxes deferred to a deferment account. 
Imports only. Uses box 47 and 48 details. 
Search/group by declaration#, importer code, DAN, tax type.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGBCustomsDefermentReportTemplate();
		}
	}
}

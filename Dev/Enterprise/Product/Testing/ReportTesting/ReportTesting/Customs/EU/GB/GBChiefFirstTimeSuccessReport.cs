using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("GB CHIEF First Time Success Rate")]
	public class TestGBChiefFirstTimeSuccessReportTemplate : TemplateTestCase
	{
	}

	public class GBChiefFirstTimeSuccessReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "GB CHIEF first time success rate"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report will show, for a given optional branch and mandatory date range, the number of customs entries whose first business response from CHIEF was positive and the number whose first response was negative. A total number of customs entries and a calculated percentage indicating the first time success rate is included.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGBChiefFirstTimeSuccessReportTemplate();
		}
	}
}

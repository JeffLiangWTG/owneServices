using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Declaration Report")]
	public class TestZADeclarationsReportTemplate : TemplateTestCase
	{
	}

	public class TestZADeclarationsReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Declaration Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report makes ZA declaration information available.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestZADeclarationsReportTemplate();
		}
	}
}

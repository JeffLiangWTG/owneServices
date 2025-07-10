using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.DE
{
	[TemplateName("DE Open Simplified Declarations Report")]
	public class TestDEOpenSimplifiedDeclarationsReportTemplate : TemplateTestCase
	{
	}

	class TestDEOpenSimplifiedDeclarationsReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "DE Open Simplified Declarations Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"DE Imports Monthly Closing - New Report Open Simplified Declarations";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestDEOpenSimplifiedDeclarationsReportTemplate();
		}
	}
}

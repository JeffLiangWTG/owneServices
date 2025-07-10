using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Invoice Report")]
	public class TestZAInvoicesReportTemplate : TemplateTestCase
	{
	}

	public class TestZAInvoicesReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Invoice Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report makes ZA declaration and invoice information available.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestZAInvoicesReportTemplate();
		}
	}
}

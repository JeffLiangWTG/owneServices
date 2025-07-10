using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Product Valuation Report")]
	public class TestZAProductValuationReportTemplate : TemplateTestCase
	{
	}

	public class TestZAProductValuationReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Product Valuation Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This Report lists the Valuation of goods stored in the Warehouse";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestZAProductValuationReportTemplate();
		}
	}
}

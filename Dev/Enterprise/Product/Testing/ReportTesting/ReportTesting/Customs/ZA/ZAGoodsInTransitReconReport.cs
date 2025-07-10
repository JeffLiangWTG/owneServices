using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("ZA Goods In Transit Recon")]
	public class TestGoodsInTransitReconReportTemplate : TemplateTestCase
	{
	}

	public class TestGoodsInTransitReconReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Goods In Transit Recon Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This Report lists the Goods In Transit";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGoodsInTransitReconReportTemplate();
		}
	}
}

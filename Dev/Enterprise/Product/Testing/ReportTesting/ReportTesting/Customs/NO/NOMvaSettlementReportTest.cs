using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.NO
{
	[TemplateName("NO MVA settlement report")]
	public class NOMvaSettlementReportTemplateTest : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Norway);
		}
	}

	class NOMvaSettlementReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "NO MVA settlement report";

		public override string Hint => "NO MVA settlement report - New Report MVA Settlement Report";

		protected override TemplateTestCase GetTemplateTestCase() => new NOMvaSettlementReportTemplateTest();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Norway);
		}
	}
}

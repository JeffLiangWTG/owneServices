using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.NO
{
	[TemplateName("NO Temporary Storage Register Report")]
	sealed class NOTemporaryStorageRegisterReportTemplateTest : TemplateTestCase
	{
	}

	sealed class NOTemporaryStorageRegisterReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "NO Temporary Storage Register Report";

		public override string Hint => "This report lists the temporary storage register with goodsnumber taken into warehouse and number of goodsitems taken out of the temporary storage.";

		protected override TemplateTestCase GetTemplateTestCase() => new NOTemporaryStorageRegisterReportTemplateTest();
	}
}

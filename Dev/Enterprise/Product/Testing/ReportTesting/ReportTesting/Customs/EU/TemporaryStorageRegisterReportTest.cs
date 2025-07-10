using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.EU
{
	[TemplateName("Temporary Storage - Register Report")]
	sealed class TemporaryStorageRegisterReportTemplateTest : TemplateTestCase
	{
	}

	sealed class TemporaryStorageRegisterReportTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName => "Temporary Storage - Register Report";

		public override string Hint => "Temporary Storage - Register Report";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TemporaryStorageRegisterReportTemplateTest();
		}
	}
}

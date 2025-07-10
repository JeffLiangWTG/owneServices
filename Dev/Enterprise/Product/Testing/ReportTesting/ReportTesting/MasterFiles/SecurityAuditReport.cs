using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Security Audit Report")]
	class SecurityAuditTemplateTest : TemplateTestCase
	{
	}

	class SecurityAuditReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return "The Security Audit Report shows changes made to Staff or Group record in a specific period of time."; }
		}

		public override string MenuName
		{
			get { return "Security Audit Report"; }
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new SecurityAuditTemplateTest();
		}
	}
}

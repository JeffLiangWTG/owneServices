using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.UserAdmin
{
	[TemplateName("Staff Certificate Report")]
	class StaffCertificatesTemplateTest : TemplateTestCase
	{
	}

	class StaffCertificatesReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return "The Staff Certificate Report shows a listing of staff members and their certificates and can be used to find the certificates of a certain type that are close to expiry."; }
		}

		public override string MenuName
		{
			get { return "Staff Certificates Report"; }
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new StaffCertificatesTemplateTest();
		}
	}
}

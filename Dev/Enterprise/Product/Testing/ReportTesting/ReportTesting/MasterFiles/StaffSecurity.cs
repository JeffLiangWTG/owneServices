using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Staff Security Report")]
	class StaffSecurityTemplateTest : TemplateTestCase
	{
	}

	class StaffSecurityReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return "The Staff Security Report shows a listing of all security rights in the system and summarizes the effective permissions of each of these security rights for all active staff members, branches and departments."; }
		}

		public override string MenuName
		{
			get { return "Staff Security Report"; }
		}

		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new StaffSecurityTemplateTest();
		}
	}
}

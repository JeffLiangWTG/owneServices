using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Staff By Group Membership Report")]
	class StaffByGroupMembershipReportTemplateTest : TemplateTestCase
	{
	}

	class StaffByGroupMembershipReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return "Staff By Group Membership Report lists what groups staff members belong to. It can be optionally filtered to show only staff members who have rights to login to a specific company and branch."; }
		}

		public override string MenuName
		{
			get { return "Staff By Group Membership Report"; }
		}

		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new StaffByGroupMembershipReportTemplateTest();
		}
	}
}

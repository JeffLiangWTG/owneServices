using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.UserAdmin
{
	[TemplateName("Staff Profile Report")]
	class StaffProfileTemplateTest : TemplateTestCase
	{
	}

	class StaffProfileReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return @"The Staff Profile Report shows a listing of users set up in CargoWise. It shows Lists Login Name, full name, User ID, Work Phone, Mobile, Fax number and Email address.

This report does not list personal details of staff members."; }
		}

		public override string MenuName
		{
			get { return "Staff Profile Report"; }
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new StaffProfileTemplateTest();
		}
	}
}

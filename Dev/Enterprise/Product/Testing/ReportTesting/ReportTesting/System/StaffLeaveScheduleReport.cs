namespace Enterprise.ReportTesting.SystemTest
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Staff Leave Schedule Report")]
	public class TestStaffLeaveScheduleReportTemplate : TemplateTestCase
	{
	}

	public class TestStaffLeaveScheduleReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		public override string MenuName
		{
			get { return "Staff Leave Schedule Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Staff Leave Schedule details leave type, dates, status and number of days by staff member.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestStaffLeaveScheduleReportTemplate();
		}
	}
}

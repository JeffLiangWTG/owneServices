namespace Enterprise.ReportTesting.SystemTest
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Staff Retention and Turnover Report")]
	public class TestStaffRetentionandTurnoverReportTemplate : TemplateTestCase
	{
	}

	public class TestStaffRetentionandTurnoverReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		public override string MenuName
		{
			get { return "Staff Retention and Turnover Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"For a nominated Analysis Date Range this report provides an analysis of staff turnover.  
The detail report will identify all New, Continuing and Departing Staff. It provides a count of staff movement in each category.
The summary report omits the detail and identifies total New, Continuing and Departing staff movements by Home Branch and Department";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestStaffRetentionandTurnoverReportTemplate();
		}
	}
}

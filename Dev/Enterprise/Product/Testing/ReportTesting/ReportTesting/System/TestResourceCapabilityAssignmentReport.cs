namespace Enterprise.ReportTesting.SystemTest
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Resource Capability Assignment Report")]
	public class TestResourceCapabilityAssignmentReportTemplate : TemplateTestCase
	{
	}

	public class TestResourceCapabilityAssignmentReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		public override string MenuName
		{
			get { return "Resource Capability Assignment Report"; }
		}

		public override string Hint
		{
			get { return ""; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestResourceCapabilityAssignmentReportTemplate();
		}
	}
}

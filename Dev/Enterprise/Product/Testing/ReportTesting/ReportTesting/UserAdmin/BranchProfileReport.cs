using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.UserAdmin
{
	[TemplateName("Company Branch Profile Report")]
	class BranchProfileTemplateTest : TemplateTestCase
	{
	}

	class BranchProfileReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return "The Branch Profile Report lists the details set up in CargoWise for each Branch.  It shows the branch code, name, location, address, phone, fax, email and Organization proxy."; }
		}

		public override string MenuName
		{
			get { return "Branch Profile Report"; }
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new BranchProfileTemplateTest();
		}
	}
}

using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.UserAdmin
{
	[TemplateName("Company Profile Report")]
	class CompanyProfileTemplateTest : TemplateTestCase
	{
	}

	class CompanyProfileReportTest : ReportTestCase
	{
		public override string Hint
		{
			get { return @"The Company Profile Report lists the details set up in CargoWise for each Company.  It shows the company code, name, location, address, phone, fax, email, Organization proxy, business registration numbers and all other details set for each company through Config > System > Companies.  

This report is designed for use in Excel and can be used to preview or email an XLS file to yourself."; }
		}

		public override string MenuName
		{
			get { return "Company Profile Report"; }
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new UserAdminReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new CompanyProfileTemplateTest();
		}
	}
}

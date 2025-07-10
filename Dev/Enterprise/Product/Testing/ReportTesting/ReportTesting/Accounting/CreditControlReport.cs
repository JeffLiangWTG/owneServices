namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ZArchitecture.Modules;

	[TemplateName("Credit Control Report")]
	public class TestCreditControlReport : TemplateTestCase
	{
	}

	public class TestCreditReportMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.MasterFiles.Module.MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Credit Control Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The report will only show organizations that have a 'Credit Controls Edited' or 'Credit Approved' event within the specified period.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCreditControlReport();
		}
	}
}

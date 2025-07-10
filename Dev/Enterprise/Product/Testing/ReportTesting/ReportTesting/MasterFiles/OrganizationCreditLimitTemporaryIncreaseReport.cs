namespace Enterprise.ReportTesting.MasterFiles
{
	using Enterprise.MasterFiles.Module;

	[TemplateName("Organization - Credit Limit Temporary Increase Report")]
	public class OrganizationCreditLimitTemporaryIncreaseTemplateTest : TemplateTestCase
	{
	}

	public class OrganizationCreditLimitTemporaryIncreaseReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new MasterDataReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Credit Limit Temporary Increase Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report displays a list of AR Organizations for which the credit limit has been temporarily increased, including the expiry date.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OrganizationCreditLimitTemporaryIncreaseTemplateTest();
		}
	}
}

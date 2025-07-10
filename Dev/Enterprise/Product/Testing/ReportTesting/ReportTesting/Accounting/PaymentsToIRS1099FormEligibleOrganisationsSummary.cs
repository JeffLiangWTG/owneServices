namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Payments to IRS 1099 Form Eligible Organisations Summary Report")]
	public class TestPaymentsToIRS1099FormEligibleOrganisationsSummaryTemplate : TemplateTestCase
	{
	}

	public class TestPaymentsToIRS1099FormEligibleOrganisationsSummaryMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Payments to IRS 1099 Form Eligible Organizations Summary Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to assist in deciding which AP Organizations need an IRS Form 1099-MISC issued against them.
For a nominated calendar year and by Payable Organization, this report identifies the total value of all AP payments (PAY transactions) posted against each Creditor in the year.  Additionally, the report identifies when an AP Organization is flagged as an ""Eligible IRS 1099 form Organization"" and when the payments posted in the year against those Organizations reach the USD600 reportable threshold.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestPaymentsToIRS1099FormEligibleOrganisationsSummaryTemplate();
		}
	}
}

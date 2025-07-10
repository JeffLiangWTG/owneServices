namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Exception Reporting - Charges Prepared Not Yet Posted as WIP, ACR, REV or CST")]
	public class TestExceptionReportingChargesPreparedNotYetPosted : TemplateTestCase
	{
	}

	public class TestExceptionReportingChargesPreparedNotYetPostedMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Exception Reporting - Charges Prepared Not Yet Posted as WIP, ACR, REV or CST"; }
		}

		public override string Hint
		{
			get
			{
				return @"Use this report to identify jobs where charges have been prepared  in the Billing screen, but not yet recognized in the financial accounting system as a WIP, CST, REV or ACR transaction.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestExceptionReportingChargesPreparedNotYetPosted();
		}
	}
}

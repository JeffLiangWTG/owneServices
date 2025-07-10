namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Job Invoicing - Charges Summary for Selected Debtor")]
	class JobInvoicingChargesSummaryForSelectedDebtor : TemplateTestCase
	{
	}

	public class JobInvoicingChargesSummaryForSelectedDebtorTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string Hint
		{
			get { return @"This report provides a summary of posted revenue charges based on the revenue recognized date for a selected Debtor within a specified date range. It includes AR Invoice and AR Credit Note transactions.

You can summarize by four optional templates - Summary by Charge Code & Group / By Charge code with Job Summary / By Job with Charge Summary / By Job with Transaction Detail.
Filter options include: Transaction Branch, Transaction Department, Transaction Recognized Revenue date range, and Transaction Debtor.

Note: The report identifies job related REV transactions recorded in your login company and all values are reported in your local currency equivalent."; }
		}

		public override string MenuName
		{
			get { return "Job Invoicing - Charges Summary for Selected Debtor"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new JobInvoicingChargesSummaryForSelectedDebtor();
		}
	}
}

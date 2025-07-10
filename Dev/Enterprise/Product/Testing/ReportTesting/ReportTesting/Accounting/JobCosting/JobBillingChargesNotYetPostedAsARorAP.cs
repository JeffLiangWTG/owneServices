namespace Enterprise.ReportTesting.Accounting
{
	using Enterprise.ReportTesting;

	[TemplateName("Job Billing - Charges Not Yet Posted as AR or AP")]
	public class TestJobBillingChargesNotYetPostedAsARorAP : TemplateTestCase
	{
	}

	public class TestJobBillingChargesNotYetPostedAsARorAPMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Job Billing – Charges Not Yet Posted as REV or CST"; }
		}

		public override string Hint
		{
			get
			{
				return
@"This detail report supports monitoring SELL and COST job charges before they are posted on an invoice.  
It itemizes each job related charge that has been prepared and not yet posted as a Revenue (AR) or Cost (AP) item.

The financial accounting system concepts of ‘Revenue Recognition’ and ‘Accounting Period’ are ignored.  Results will therefore not reconcile to the General Ledger control accounts.

The report identifies WIP Sell and Accrued Cost charges prepared against jobs as they are right now, whether recognized or not.
By including both recognized (charges with an associated WIP or ACR) and not recognized (charges prepared but not yet recognized as WIP or ACR) the report identifies charges that will effect a movement in the AR and AP subsidiary ledgers once posted. 

Note:  Posted charges are ignored.  Once a charge has been posted on an Invoice or Credit Note, the management of the receivable or payable transactions can be monitored via the Receivables and Payables subsidiary ledgers.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobBillingChargesNotYetPostedAsARorAP();
		}
	}
}

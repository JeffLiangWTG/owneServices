using System;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests.DataModel.eHubTransactions
{
	class eHubTransformationSet
	{
		public eHubTransformationSet()
		{
			TS_PK = Guid.NewGuid();
		}

		public Guid TS_PK { get; set; }
		public string TS_Name { get; set; }
		public Guid? TS_CC_Sender { get; set; }
		public Guid? TS_CC_Recipient { get; set; }
		public Guid? TS_DTSource { get; set; }
		public string TS_XPathPredicate { get; set; }
		public string TS_BillingInterfaceName { get; set; }
		public string TS_BillingElement { get; set; }
		public string TS_BillingXPathSource { get; set; }
		public string TS_BillingXPathTarget { get; set; }
		public bool? TS_BillSender { get; set; }
		public bool? TS_BillRecipient { get; set; }
		public Guid? TS_CC_BillOther { get; set; }
		public int? TS_BillingNumMessagesIncluded { get; set; }
		public int? TS_BillingFee { get; set; }
	}
}
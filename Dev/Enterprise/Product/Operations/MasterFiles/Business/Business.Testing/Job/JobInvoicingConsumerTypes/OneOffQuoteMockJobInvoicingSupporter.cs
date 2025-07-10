namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OneOffQuoteMockJobInvoicingSupporter : DummyJobHeaderParentJobInvoicingSupporter
	{
		public OneOffQuoteMockJobInvoicingSupporter(JobInvoicingConsumerType consumerType)
		{
			ConsumerType = consumerType;
		}
	}
}

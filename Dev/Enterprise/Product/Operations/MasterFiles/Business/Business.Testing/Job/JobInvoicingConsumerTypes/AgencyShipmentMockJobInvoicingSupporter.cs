namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AgencyShipmentMockJobInvoicingSupporter : DummyJobHeaderParentJobInvoicingSupporter
	{
		public AgencyShipmentMockJobInvoicingSupporter(JobInvoicingConsumerType consumerType)
		{
			ConsumerType = consumerType;
		}
	}
}

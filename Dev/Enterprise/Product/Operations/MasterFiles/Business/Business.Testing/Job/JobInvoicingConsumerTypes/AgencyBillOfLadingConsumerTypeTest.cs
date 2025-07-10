namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AgencyBillOfLadingConsumerTypeTest : AgencyConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.AgencyBillOfLading;
		}
	}
}

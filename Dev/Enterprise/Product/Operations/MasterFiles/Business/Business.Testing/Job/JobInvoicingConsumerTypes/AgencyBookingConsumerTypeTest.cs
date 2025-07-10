namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AgencyBookingConsumerTypeTest : AgencyConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.AgencyBooking;
		}
	}
}

namespace Enterprise.MasterFiles.Business.Testing
{
	public class QuotedBookingConsumerTypeTest : BaseShipmentConsumerTypeTest
	{
		#region Test Supporting Classes and Methods

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.QuotedBooking;
		}

		#endregion
	}
}

using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyBookingRatingAdapterTest : AgencyShipmentRatingAdapterTest<AgencyBookingJobDatesProvider>
	{
		public override AgencyShipment CreateShipment()
		{
			return Factory.NewWithValidTestData<AgencyBooking>();
		}

		public override string ConsumerType
		{
			get
			{
				return JobInvoicingConsumerTypes.AgencyBooking.Code;
			}
		}

		public override AgencyShipmentContainer AddNewContainer(AgencyShipment shipment)
		{
			return shipment.BookedContainers.AddNew();
		}

		protected override string GetExpectedSellAutoratingMode()
		{
			return Core.Constants.FreightRateAutoratingModes.Code.StandardRate;
		}

		protected override string GetExpectedCostAutoratingMode()
		{
			return Core.Constants.FreightRateAutoratingModes.Code.StandardRate;
		}
	}
}

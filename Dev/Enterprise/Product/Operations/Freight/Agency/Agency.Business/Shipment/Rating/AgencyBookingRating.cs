using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyBookingRatingAdapter : AgencyShipmentRatingAdapter
	{
		protected internal AgencyBookingRatingAdapter(AgencyBooking parent)
			: base(parent) { }

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new AgencyBookingJobDatesProvider(parent); }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.AgencyBooking; }
		}

		protected override SpotRateInfo GetSellSpotRateInfoCore()
		{
			return new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.ClientRate);
		}

		protected override SpotRateInfo GetCostSpotRateInfoCore()
		{
			return new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
		}
	}
}

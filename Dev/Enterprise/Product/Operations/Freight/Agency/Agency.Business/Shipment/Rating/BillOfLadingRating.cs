using Enterprise.Freight.Agency.Business.Shipment.Rating;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class BillOfLadingRatingAdapter : AgencyShipmentRatingAdapter
	{
		protected internal BillOfLadingRatingAdapter(BillOfLading parent)
			: base(parent) { }

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new BillOfLadingJobDatesProvider(parent); }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.AgencyBillOfLading; }
		}

		protected override SpotRateInfo GetSellSpotRateInfoCore()
		{
			return new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.ClientRate);
		}

		protected override SpotRateInfo GetCostSpotRateInfoCore()
		{
			return new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
		}

		protected override ShipmentRateLineConditionsSupporter GetConditionsSupporterCore() => new BillOfLadingRateLineConditionsSupporter(parent as BillOfLading);
	}
}

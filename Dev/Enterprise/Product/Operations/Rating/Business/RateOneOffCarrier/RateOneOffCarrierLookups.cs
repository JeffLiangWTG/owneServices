using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateOneOffCarrierLookups : AutoRateOneOffCarrierLookups
	{
		public RateOneOffCarrierLookups(AutoRateOneOffCarrier parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TransitTimesList
		{
			get
			{
				var rateOneOffCarrier = (RateOneOffCarrier)Parent;
				return rateOneOffCarrier.OneOffShipment.IsAir
					? RateEntryLookups.AirTransitTimes
					: RateEntryLookups.SeaTransitTimes;
			}
		}

		RateEntryLookups RateEntryLookups => new RateEntryLookups(Factory.GetNull<RateEntry>());

		public FrequencyList FrequencyUnits => Factory.GetCachedValue<FrequencyList>();
	}
}

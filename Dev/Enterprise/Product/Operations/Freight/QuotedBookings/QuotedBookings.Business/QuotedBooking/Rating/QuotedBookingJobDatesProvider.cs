using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingJobDatesProvider : JobDatesProvider<QuotedBooking>, IJobDatesProviderForCarriers
	{
		public QuotedBookingJobDatesProvider(QuotedBooking quotedBooking)
			: base(quotedBooking) { }

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.GetFromBooking ? Parent.Booking.JS_E_DEP : Parent.StartDate;
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.GetFromBooking ? Parent.Booking.JS_E_ARV : Parent.EndDate;
		}

		protected override ZDateTime GetPickupDateCore()
		{
			return Parent.GetFromBooking ? Parent.PickupReady : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryDateCore()
		{
			return Parent.GetFromBooking ? Parent.DeliveryOpen : ZDateTime.Empty;
		}

		public override string TransitTime => Parent?.TransitTime ?? string.Empty;

		Dictionary<ZGuid, string> IJobDatesProviderForCarriers.CarrierTransitTimes
		{
			get
			{
				if (transitTimes == null)
				{
					transitTimes = new Dictionary<ZGuid, string>();

					if (Parent?.Quote?.CurrentOneOffQuote?.PossibleCarriers != null)
					{
						foreach (RateOneOffCarrier potentialCarrier in Parent.Quote.CurrentOneOffQuote.PossibleCarriers)
						{
							if (potentialCarrier.Carrier != null)
							{
								transitTimes[potentialCarrier.Carrier.PK] = string.IsNullOrEmpty(potentialCarrier.TTC_TransitTime)
									? Parent.TransitTime
									: potentialCarrier.TTC_TransitTime;
							}
						}
					}

					if (Parent?.Carrier != null && !string.IsNullOrEmpty(Parent.TransitTime))
					{
						transitTimes[Parent.Carrier.PK] = Parent.TransitTime;
					}
				}

				return transitTimes;
			}
		}

		Dictionary<ZGuid, string> transitTimes;
	}
}

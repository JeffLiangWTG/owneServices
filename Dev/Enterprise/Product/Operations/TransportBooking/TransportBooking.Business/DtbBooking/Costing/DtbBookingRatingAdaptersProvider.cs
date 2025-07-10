using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingRatingAdaptersProvider : RatingAdaptersProvider<DtbBooking>
	{
		public DtbBookingRatingAdaptersProvider(DtbBooking parent)
			: base(parent)
		{
		}

		protected override List<IAutoRating> GetAdapters(DtbBooking parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = new List<IAutoRating>();

			if (parent.KM_RatingFreightMode == RatingFreightModes.Codes.Containerised || parent.KM_RatingFreightMode == RatingFreightModes.Codes.Both)
			{
				if (parent.KM_TransportMode == Constants.TransportModes.Rail)
				{
					result.AddRange(GetRatingPairs(parent, FreightMode.FRA, i => i.KN_IsContainerRateable));
				}
				else
				{
					result.AddRange(GetRatingPairs(parent, FreightMode.FRO, i => i.KN_IsContainerRateable));
				}
			}

			if (parent.KM_RatingFreightMode == RatingFreightModes.Codes.Loose || parent.KM_RatingFreightMode == RatingFreightModes.Codes.Both)
			{
				if (parent.KM_TransportMode == Constants.TransportModes.Rail)
				{
					result.AddRange(GetRatingPairs(parent, FreightMode.LRA, i => i.KN_IsLooseRateable));
				}
				else
				{
					result.AddRange(GetRatingPairs(parent, FreightMode.LRO, i => i.KN_IsLooseRateable));
				}
			}

			if (result.Count == 0)  // tested in DtbBookingRating
			{
				result.Add(new DtbBookingRatingAdapter(parent, FreightMode.UKN, parent.Instructions.FirstOrDefault(i => i.IsPickUp), parent.Instructions.FirstOrDefault(i => i.IsDelivery)));
			}

			return result;
		}

		List<IAutoRating> GetRatingPairs(DtbBooking booking, FreightMode freightMode, Func<DtbBookingInstruction, bool> isRateable)
		{
			var result = new List<IAutoRating>();
			var ratedInstructions = booking.Instructions.OrderBy(i => i.KN_Sequence).Where(isRateable).ToArray();
			if (ratedInstructions.Length > 0)
			{
				var first = ratedInstructions.First();
				var last = ratedInstructions.Last();

				var pickups = ratedInstructions.Where(i => i == first || ((i.IsPickUp || i.IsMulti) && i != last));
				var deliveries = ratedInstructions.Except(pickups);

				foreach (var pickup in pickups)
				{
					foreach (var delivery in deliveries)
					{
						var rp = new DtbBookingRatingAdapter(booking, freightMode, pickup, delivery);
						if ((freightMode == FreightMode.FRO && rp.CommonContainers.Any())
							|| (freightMode == FreightMode.LRO && rp.CommonPackages.Any())
							|| (freightMode == FreightMode.FRA && rp.CommonContainers.Any())
							|| (freightMode == FreightMode.LRA && rp.CommonPackages.Any()))
						{
							result.Add(rp);
						}
					}
				}
			}
			return result;
		}
	}
}

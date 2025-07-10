using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class TransportZoneComparer : BaseRateLineComparer
	{
		public TransportZoneComparer(bool isCheckingOrigin)
		{
			this.isCheckingOrigin = isCheckingOrigin;
		}

		readonly bool isCheckingOrigin;

		public override int Compare(FastLine line1, FastLine line2)
		{
			return GetZoneRank(line1).CompareTo(GetZoneRank(line2));
		}

		int GetZoneRank(FastLine line)
		{
			if (line != null && line.ParentRateEntry != null)
			{
				var zone = isCheckingOrigin ? line.ParentRateEntry.OriginZone : line.ParentRateEntry.DestinationZone;

				RateTransportProvider zoneSet;
				if (zone != null && (zoneSet = zone.TransportProvider) != null)
				{
					return zoneSet.TP_ZoneType == RatingConstants.RatingZoneTypes.Rating
						? 2
						: zoneSet.TP_ZoneType == RatingConstants.RatingZoneTypes.All
							? 1
							: -1;
				}
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"Transport Zone";  // log message, subject to change, more for support people as of now
		}
	}
}

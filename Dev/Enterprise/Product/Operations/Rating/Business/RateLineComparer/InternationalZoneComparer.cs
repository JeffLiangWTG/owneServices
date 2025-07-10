using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class InternationalZoneComparer : BaseRateLineComparer
	{
		public InternationalZoneComparer(bool isCheckingOrigin, RatingCriteria criteria)
		{
			this.isCheckingOrigin = isCheckingOrigin;
			this.criteria = criteria;
		}

		readonly bool isCheckingOrigin;
		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var rank1 = GetZoneRank(line1);
			var rank2 = GetZoneRank(line2);

			if ((rank1 == 0 || rank2 == 0) && (rank1 >= 0 && rank2 >= 0))
			{
				return 0;
			}

			return rank1.CompareTo(rank2);
		}

		int GetZoneRank(FastLine line)
		{
			if (line != null && line.ParentRateEntry != null)
			{
				var location = isCheckingOrigin ? line.ParentRateEntry.TI_OriginLRC : line.ParentRateEntry.TI_DestinationLRC;

				if (LocationHelper.GetLocationType(location) == LocationHelper.LocationType.Zone)
				{
					var zoneHeader = line.Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, location));
					if (zoneHeader != null)
					{
						var rank = GetZoneRank(zoneHeader);
						if (rank > 0 && !zoneHeader.FZ_OH_RelatedParty.IsEmpty)
						{
							return rank + 128;
						}

						return rank;
					}

					return -1;
				}
			}

			return 0;
		}

		int GetZoneRank(RefZoneHeader zone)
		{
			var rank = 0;
			switch (zone.FZ_ZoneType)
			{
				case RefZoneHeaderLookups.ZoneTypeCodes.RatingImport:
					rank = (criteria != null && criteria.IsImport()) ? 128 : -128;
					break;

				case RefZoneHeaderLookups.ZoneTypeCodes.RatingExport:
					rank = (criteria != null && criteria.IsImport()) ? -128 : -128;
					break;

				case RefZoneHeaderLookups.ZoneTypeCodes.Rating:
					rank = 64;
					break;

				case RefZoneHeaderLookups.ZoneTypeCodes.All:
					rank = 32;
					break;
			}

			if (rank > 0)
			{
				rank += GetZoneRankForZoneMode(zone);
			}

			return rank;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		int GetZoneRankForZoneMode(RefZoneHeader zone)
		{
			var rank = 0;

			if (criteria == null)
			{
				return rank;
			}

			if (criteria.FreightMode.ToString() == zone.FZ_ZoneMode)
			{
				return rank += 16;
			}

			switch (zone.FZ_ZoneMode)
			{
				case Core.Constants.RateMode.ALL:
					rank += 2;
					break;

				case Core.Constants.RateMode.AIR:
					rank += criteria.FreightMode == FreightMode.LSE || criteria.FreightMode == FreightMode.ULD ? 8 : -8;
					break;

				case Core.Constants.RateMode.SEA:
					rank += criteria.FreightMode == FreightMode.FCL || criteria.FreightMode == FreightMode.LCL ? 8 : -8;
					break;

				case Core.Constants.RateMode.ROA:
					rank += criteria.FreightMode == FreightMode.FTL || criteria.FreightMode == FreightMode.FRO || criteria.FreightMode == FreightMode.LRO ? 8 : -8;
					break;

				case Core.Constants.RateMode.RAI:
					rank += criteria.FreightMode == FreightMode.FWL || criteria.FreightMode == FreightMode.FRA || criteria.FreightMode == FreightMode.LRA ? 8 : -8;
					break;

				default:
					rank += 4;
					break;
			}

			return rank;
		}

		protected override string GetName()
		{
			return (NoResString)"International Zone"; // log message, subject to change, more for support people as of now
		}
	}
}

using System;
using CargoWise.Schema;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	class MatchingLocationComparer : BaseRateLineComparer
	{
		public MatchingLocationComparer(SchemaStringColumn locationColumn, RatingCriteria criteria)
		{
			LocationColumnName = locationColumn.Name;
			Criteria = criteria;
		}

		string LocationColumnName { get; }
		RatingCriteria Criteria { get; }

		protected override string GetName() => LocationColumnName;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var rateEntry1 = line1.ParentRateEntry;
			var location1 = GetRateLocation(rateEntry1, LocationColumnName);
			var location2 = GetRateLocation(line2.ParentRateEntry, LocationColumnName);
			var criteriaLocation = GetCriteriaLocation(Criteria, LocationColumnName, rateEntry1.IsCostRate() ? CostSell.Cost : CostSell.Revenue);

			var isLine2LessSpecific = location2.IsLessSpecificThan(location1, criteriaLocation);
			var isLine1LessSpecific = location1.IsLessSpecificThan(location2, criteriaLocation);

			return Convert.ToInt32(isLine2LessSpecific) - Convert.ToInt32(isLine1LessSpecific);
		}

		static ILocation GetRateLocation(IRateEntry rateEntry, string locationColumnName)
		{
			switch (locationColumnName)
			{
				case nameof(IRateEntry.TI_FirstLoadLRC):
					return rateEntry.FirstLoad();
				case nameof(IRateEntry.TI_LastDischargeLRC):
					return rateEntry.LastDischarge();
				case nameof(IRateEntry.TI_FirstRouteSetLoadPortLRC):
					return rateEntry.FirstRouteSetLoad();
				case nameof(IRateEntry.TI_LastRouteSetDischargePortLRC):
					return rateEntry.LastRouteSetDischarge();
			}

			throw new NotSupportedException(FormattableString.Invariant($"{locationColumnName} is not a matching location field!"));
		}

		static ILocation GetCriteriaLocation(RatingCriteria criteria, string locationColumnName, CostSell costOrSell)
		{
			switch (locationColumnName)
			{
				case nameof(IRateEntry.TI_FirstLoadLRC):
					return criteria?.GetFirstLoad(costOrSell);
				case nameof(IRateEntry.TI_LastDischargeLRC):
					return criteria?.GetLastDischarge(costOrSell);
				case nameof(IRateEntry.TI_FirstRouteSetLoadPortLRC):
					return criteria?.GetFirstRouteSetLoad(costOrSell);
				case nameof(IRateEntry.TI_LastRouteSetDischargePortLRC):
					return criteria?.GetLastRouteSetDischarge(costOrSell);
			}

			throw new NotSupportedException(FormattableString.Invariant($"{locationColumnName} is not a matching location field!"));
		}
	}
}

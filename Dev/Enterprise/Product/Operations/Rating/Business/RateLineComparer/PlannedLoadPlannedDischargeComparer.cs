using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class PlannedLoadPlannedDischargeComparer : BaseRateLineComparer
	{
		public PlannedLoadPlannedDischargeComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			return IsPlannedLoadPlannedDischargeOverriden(line2.Line, line1.Line, criteria) - IsPlannedLoadPlannedDischargeOverriden(line1.Line, line2.Line, criteria);
		}

		public static int IsPlannedLoadPlannedDischargeOverriden(IRateLine rateLine, IRateLine overrideLine, RatingCriteria criteria)
		{
			var costSell = _Rating.Sell ? CostSell.Revenue : CostSell.Cost;

			var currentEntry = rateLine.ParentRateEntry;
			var overrideEntry = overrideLine.ParentRateEntry;

			var currentEntryPlannedLoad = currentEntry.PlannedLoad();
			var currentEntryPlannedDischarge = currentEntry.PlannedDischarge();
			var overrideEntryPlannedLoad = overrideEntry.PlannedLoad();
			var overrideEntryPlannedDischarge = overrideEntry.PlannedDischarge();

			var isPlannedLoadOverridden = currentEntryPlannedLoad.IsLessSpecificThan(overrideEntryPlannedLoad, criteria?.PlannedLoad(costSell));
			var isPlannedDischargeOverridden = currentEntryPlannedDischarge.IsLessSpecificThan(overrideEntryPlannedDischarge, criteria?.PlannedDischarge(costSell));

			if (currentEntry.IsOriginEntry() || currentEntry.IsFreightEntry())
			{
				if (isPlannedLoadOverridden)
				{
					return 2;
				}
				if (isPlannedDischargeOverridden)
				{
					return 1;
				}
			}
			else if (currentEntry.IsDestinationEntry())
			{
				if (isPlannedDischargeOverridden)
				{
					return 2;
				}
				if (isPlannedLoadOverridden)
				{
					return 1;
				}
			}

			return 0;
		}

		protected override string GetName()
		{
			return (NoResString)"Planned Load Planned Discharge"; // log message, subject to change, more for support people as of now
		}
	}
}

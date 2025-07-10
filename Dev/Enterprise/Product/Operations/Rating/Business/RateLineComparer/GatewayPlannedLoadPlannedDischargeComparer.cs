using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class GatewayPlannedLoadPlannedDischargeComparer : BaseRateLineComparer
	{
		public GatewayPlannedLoadPlannedDischargeComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;
			var costSell = _Rating.Sell ? CostSell.Revenue : CostSell.Cost;

			if (entry1.IsIntercompanyTariff() && entry2.IsIntercompanyTariff())
			{
				var (entry1PlannedLoadAgentOrder, entry1PlannedLoad) = GetGatewayAgentOrderAndLocation(entry1.PlannedLoad(), criteria?.SortedOverridenPlannedLoad);
				var (entry1PlannedDischargeAgentOrder, entry1PlannedDischarge) = GetGatewayAgentOrderAndLocation(entry1.PlannedDischarge(), criteria?.SortedOverridenPlannedDischarge);

				var (entry2PlannedLoadAgentOrder, entry2PlannedLoad) = GetGatewayAgentOrderAndLocation(entry2.PlannedLoad(), criteria?.SortedOverridenPlannedLoad);
				var (entry2PlannedDischargeAgentOrder, entry2PlannedDischarge) = GetGatewayAgentOrderAndLocation(entry2.PlannedDischarge(), criteria?.SortedOverridenPlannedDischarge);

				var entry1Rank = entry1PlannedLoadAgentOrder + entry1PlannedDischargeAgentOrder;
				var entry2Rank = entry2PlannedLoadAgentOrder + entry2PlannedDischargeAgentOrder;

				if (entry1PlannedLoadAgentOrder > 0 && entry1PlannedLoadAgentOrder == entry2PlannedLoadAgentOrder)
				{
					var isEntry1Overriden = entry1.PlannedLoad().IsLessSpecificThan(entry2.PlannedLoad(), entry1PlannedLoad);
					if (isEntry1Overriden)
					{
						entry2Rank = entry2Rank * 40;
					}
				}

				if (entry1PlannedDischargeAgentOrder > 0 && entry1PlannedDischargeAgentOrder == entry2PlannedDischargeAgentOrder)
				{
					var isEntry1Overriden = entry1.PlannedDischarge().IsLessSpecificThan(entry2.PlannedDischarge(), entry1PlannedDischarge);
					if (isEntry1Overriden)
					{
						entry2Rank = entry2Rank * 20;
					}
				}

				if (entry1Rank > entry2Rank)
				{
					return -1;
				}
				if (entry1Rank < entry2Rank)
				{
					return 1;
				}
			}

			return 0;
		}

		(int order, ILocation location) GetGatewayAgentOrderAndLocation(ILocation location, List<ILocation> collection)
		{
			var locationWithSource = collection?.FirstOrDefault(x => location?.CompletelyCovers(x) ?? false);
			if (locationWithSource == null)
			{
				return (0, null);
			}

			return (collection.IndexOf(locationWithSource) + 1, locationWithSource);
		}

		protected override string GetName()
		{
			return (NoResString)"Gateway Planned Load Planned Discharge"; // log message, subject to change, more for support people as of now
		}
	}
}

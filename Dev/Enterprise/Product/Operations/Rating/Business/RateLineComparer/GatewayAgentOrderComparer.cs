using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	class GatewayAgentOrderComparer : BaseRateLineComparer
	{
		public GatewayAgentOrderComparer(RatingCriteria criteria)
		{
			this.criteria = criteria;
		}

		readonly RatingCriteria criteria;

		public override int Compare(FastLine line1, FastLine line2)
		{
			if (line1.IsIntercompanyTariff() && line2.IsIntercompanyTariff())
			{
				if (_Rating.Cost)
				{
					var currentEntryGatewayOrder = GatewayAgentOrder(line1.ParentRateEntry);
					var overridenEntryGatewayOrder = GatewayAgentOrder(line2.ParentRateEntry);

					if (currentEntryGatewayOrder > overridenEntryGatewayOrder)
					{
						return -1;
					}
					else if (currentEntryGatewayOrder < overridenEntryGatewayOrder)
					{
						return 1;
					}
				}
			}

			return 0;
		}

		int GatewayAgentOrder(IRateEntry entry)
		{
			return criteria.SortedGatewayAgentPKs.IndexOf(entry.ParentRatingHeader.TH_OH);
		}

		protected override string GetName()
		{
			return (NoResString)"Gateway Agent Order"; // log message, subject to change, more for support people as of now
		}
	}
}

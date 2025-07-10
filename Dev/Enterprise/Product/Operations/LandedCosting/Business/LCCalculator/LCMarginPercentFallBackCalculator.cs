using Enterprise.MasterFiles.Business;

namespace Enterprise.LandedCosting.Business
{
	public class LCMarginPercentFallBackCalculator
	{
		public LCMarginPercentFallBackCalculator(LandedCostHistory lcHistory)
		{
			this.lcHistory = lcHistory;
			lcHeader = lcHistory.LCHeader;
			if (lcHeader != null)
			{
				lcHost = lcHeader.Parent;
			}
		}

		public LCMarginPercentages GetLCMarginPercentagesForFallBack()
		{
			LCMarginPercentages result = lcHistory.GetLCMarginPercentagesForFallBack();
			if (!result.HasValues && lcHost != null && lcHost.Consignee != null)
			{
				if (lcHistory.UltimateDistributee != null)
				{
					result = lcHistory.UltimateDistributee.GetProductSpecificLCMarginPercentagesForFallBack(lcHost.Consignee);
				}
				if (!result.HasValues)
				{
					result = lcHost.Consignee.GetLCMarginPercentagesForFallBack();
				}
			}
			return result;
		}

		readonly LandedCostHistory lcHistory;
		readonly LandedCostHeader lcHeader;
		readonly ILandedCostHeader lcHost;
	}
}

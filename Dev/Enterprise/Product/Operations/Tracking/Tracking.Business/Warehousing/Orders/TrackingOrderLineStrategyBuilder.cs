using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business
{
	public class TrackingOrderLineStrategyBuilder : ITrackingOrderLineStrategyBuilder
	{
		public IWhsOrderLineWrapperStrategy Build(WhsOrderLine line)
		{
			if (Globals.IsWeb)
			{
				return TrackingHelper.Get(line);
			}
			else
			{
				return line;
			}
		}
	}
}

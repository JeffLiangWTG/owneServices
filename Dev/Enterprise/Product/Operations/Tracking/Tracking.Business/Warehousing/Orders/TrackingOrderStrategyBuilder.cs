using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business
{
	public class TrackingOrderStrategyBuilder : ITrackingOrderStrategyBuilder
	{
		public IWhsOrderWrapperStrategy Build(WhsOrder order)
		{
			if (Globals.IsWeb)
			{
				return TrackingHelper.Get(order);
			}
			else
			{
				return order;
			}
		}
	}
}

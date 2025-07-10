using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business
{
	public class WhsReceiveStrategyBuilder : IWhsReceiveStrategyBuilder
	{
		public IWhsReceiveWrapperStrategy Build(WhsReceive receive)
		{
			if (Globals.IsWeb)
			{
				return TrackingHelper.Get(receive);
			}
			else
			{
				return receive;
			}
		}
	}
}

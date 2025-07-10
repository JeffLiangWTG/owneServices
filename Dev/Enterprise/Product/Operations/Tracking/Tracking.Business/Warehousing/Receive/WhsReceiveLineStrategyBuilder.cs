using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business
{
	public class WhsReceiveLineStrategyBuilder : IWhsReceiveLineStrategyBuilder
	{
		public IWhsReceiveLineWrapperStrategy Build(WhsReceiveLine line)
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

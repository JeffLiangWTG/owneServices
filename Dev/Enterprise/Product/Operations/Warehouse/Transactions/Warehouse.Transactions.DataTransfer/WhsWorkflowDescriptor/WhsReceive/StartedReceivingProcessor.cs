using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class StartedReceivingProcessor : ReceiveActionProcessor
	{
		public StartedReceivingProcessor(WhsReceive receive)
			: base(receive)
		{
		}

		protected override ZString ActionName => Res.GetString("FB4AB9B5-96F7-48E0-8388-1745573AF6A9", "started receiving");

		protected override bool PerformAction(WhsReceive receive, INotifications notifications)
		{
			receive.PopulateASNLines();
			if (receive.WD_ArrivalDate.IsEmpty)
			{
				receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			}

			return true;
		}
	}
}

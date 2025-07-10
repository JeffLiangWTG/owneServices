using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class ReceiveAutoPalletiseProcessor : ReceiveActionProcessor
	{
		public ReceiveAutoPalletiseProcessor(WhsReceive receive)
			: base(receive)
		{
		}

		protected override ZString ActionName => Res.GetString("BA44A5BD-6168-448F-A7B5-0CA0CA41E62B", "auto palletize");

		protected override bool PerformAction(WhsReceive receive, INotifications notifications)
		{
			var result = true;
			receive.PalletizeLines(receive.Lines.ToArray<WhsReceiveLine>());

			if (receive.Lines.Any(l => l.HasRowWarnings))
			{
				notifications.AddWarning(Res.GetString("8EC67162-7C22-4199-8DD8-3EFF97E60AE7", "Unable to {0} for all receive lines of receive {1} because at least one of its lines does not have a product or the product does not have a pallet definition.", ActionName, receive.WD_DocketID));
				result = false;
			}

			return result;
		}
	}
}

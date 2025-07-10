using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class ReceivePalletIDGenerator : ReceiveActionProcessor
	{
		public ReceivePalletIDGenerator(WhsReceive receive)
			: base(receive)
		{
		}

		protected override ZString ActionName => Res.GetString("43620B46-23E7-4E4E-B1BF-C382F7D1BED4", "generate pallet IDs");

		protected override bool PerformAction(WhsReceive receive, INotifications notifications)
		{
			receive.GenerateSequentialPalletIDs(receive.Lines.ToArray<WhsReceiveLine>());

			return true;
		}
	}
}

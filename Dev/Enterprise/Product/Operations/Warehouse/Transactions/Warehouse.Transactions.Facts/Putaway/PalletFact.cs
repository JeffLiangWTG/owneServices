using CargoWise.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class PalletFact : IPalletFact
	{
		public PalletFact(string palletID)
		{
			Argument.NotNullOrEmpty(palletID, nameof(palletID));
			PalletID = palletID;
		}

		public string PalletID { get; }
	}
}

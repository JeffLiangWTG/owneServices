using CargoWise.Common;
using CargoWise.ComponentModel;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveLineValidationStrategy
	{
		public WhsReceiveLineValidationStrategy(WhsReceiveLine parent)
		{
			Parent = Argument.NotNull(parent, "parent");
		}

		protected readonly WhsReceiveLine Parent;

		public void CheckPalletIDAssignedToPutawayTransfer(WhsReceiveLine receiveLine)
		{
			if (receiveLine.WE_StockOnHand > 0 && !receiveLine.WE_PalletIDInfo.HasErrors() && !receiveLine.WE_PalletID.IsEmpty && receiveLine.Docket != null && !receiveLine.HasPutawayTransfer)
			{
				if (IsPutawayTransferCreatedForThisPalletID(receiveLine.Docket, receiveLine.WE_PalletID))
				{
					receiveLine.WE_PalletIDInfo.AddError(Res.GetString("0192a649-7c6e-40b5-b5a1-ef71c308765e", "Pallet ID is assigned to a putaway transfer. Use a different Pallet ID."));
				}
			}
		}

		protected virtual bool IsPutawayTransferCreatedForThisPalletID(WhsReceive receive, string palletID)
		{
			return PalletIDPutawayTransferCacheManager.HasPutawayTransferForPallet(receive, palletID);
		}
	}
}

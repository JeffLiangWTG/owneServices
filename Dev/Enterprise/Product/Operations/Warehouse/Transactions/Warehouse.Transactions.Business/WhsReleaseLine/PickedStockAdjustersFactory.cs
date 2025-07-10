using System;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public enum ReduceStockReason
	{
		Lost,
		Returned
	}

	public interface IPickedStockAdjustersFactory
	{
		IPickedStockAdjuster GetNewAdjuster(WhsWarehouse warehouse, OrgHeader client, ReduceStockReason reduceStockReason);

		SecurityCheckpoint GetAdjusterSecurityCheckpoint(ReduceStockReason reduceStockReason);

		IMultilingualString GetDescription(ReduceStockReason reduceStockReason);
	}

	public class PickedStockAdjustersFactory : IPickedStockAdjustersFactory
	{
		public IPickedStockAdjuster GetNewAdjuster(WhsWarehouse warehouse, OrgHeader client, ReduceStockReason reduceStockReason)
		{
			Argument.NotNull(warehouse, nameof(warehouse));
			Argument.NotNull(client, nameof(client));

			switch (reduceStockReason)
			{
				case ReduceStockReason.Lost:
					return GetNewAdjuster<WhsAdjustment>(warehouse, client, AdjustmentType.Codes.Adjustment);
				case ReduceStockReason.Returned:
					return GetNewAdjuster<WhsTransfer>(warehouse, client, TransferType.Codes.Internal);
				default:
					throw new NotSupportedException(string.Format(Culture.Invariant, "ReduceStockReasons {0} is not a valid reason to reduce stock.", reduceStockReason));
			}
		}

		IPickedStockAdjuster GetNewAdjuster<T>(WhsWarehouse warehouse, OrgHeader client, ZString subType) where T : WhsDocket, IPickedStockAdjuster
		{
			var result = warehouse.Factory.New<T>();
			result.WD_WW_Whs = warehouse.PK;
			result.WD_OH_Client = client.PK;
			result.WD_DocketSubType = subType;
			return result;
		}

		public SecurityCheckpoint GetAdjusterSecurityCheckpoint(ReduceStockReason reduceStockReason)
		{
			switch (reduceStockReason)
			{
				case ReduceStockReason.Lost:
					return Env.Security.WhsReleaseAdjustOutQtyMet;
				case ReduceStockReason.Returned:
					return Env.Security.WhsReleaseReturnItemsToStock;
				default:
					throw new NotSupportedException(string.Format(Culture.Invariant, "ReduceStockReasons {0} is not a valid reason to reduce stock.", reduceStockReason));
			}
		}

		public IMultilingualString GetDescription(ReduceStockReason reduceStockReason)
		{
			switch (reduceStockReason)
			{
				case ReduceStockReason.Lost:
					return ResString.GetMultilingualString("PickedStockAdjustersFactory|GetDescription|Lost", "Adjust Out Quantity Met");
				case ReduceStockReason.Returned:
					return ResString.GetMultilingualString("PickedStockAdjustersFactory|GetDescription|Returned", "Return Items to Stock");
				default:
					throw new NotSupportedException(string.Format(Culture.Invariant, "ReduceStockReasons {0} is not a valid reason to reduce stock.", reduceStockReason));
			}
		}
	}
}

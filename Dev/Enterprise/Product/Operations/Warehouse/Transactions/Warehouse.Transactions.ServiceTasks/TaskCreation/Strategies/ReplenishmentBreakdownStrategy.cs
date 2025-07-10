using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class ReplenishmentBreakdownStrategy : TransferBreakdownStrategy
	{
		protected override bool GetHasAwaitingPicks(WhsTransfer transfer, WhsTransferLine transferLine)
		{
			return transfer.WD_WP_PickBeingReplenished.IsValid || GetProductHasAwaitingPicks();

			bool GetProductHasAwaitingPicks()
			{
				var cacheKey = $"{nameof(ReplenishmentBreakdownStrategy)}_{nameof(GetHasAwaitingPicks)}|{transfer.PK}";
				var hasAwaitingPicksCache = transfer.Factory.GetCachedValue(
					cacheKey,
					() =>
					{
						var hasAwaitingPickProductsCollection = new DynamicBusinessObjectCollection(transfer.Factory);
						var sql = @"
SELECT DISTINCT
	WWP_OP
FROM
	dbo.WhsDocket
	JOIN dbo.WhsDocketLine on WE_WD = WD_PK
	JOIN dbo.WhsPickFaceAwaitingReplenishmentView on WWP_WW_Whs = WD_WW_Whs AND WWP_OH_Client = WD_OH_Client AND WWP_OP = WE_OP
WHERE
	WD_PK = @TransferPK";

						var sqlParam = ZSqlParameter.New("@TransferPK", transfer.PK, WhsDocketSchema.PK);
						hasAwaitingPickProductsCollection.Load(sql, new[] { sqlParam });

						var hasAwaitingPickProducts = new HashSet<ZGuid>();
						foreach (var hasAwaitingPickProduct in hasAwaitingPickProductsCollection)
						{
							hasAwaitingPickProducts.Add((ZGuid)hasAwaitingPickProduct[WhsPickFaceAwaitingReplenishmentViewSchema.Constants.WWP_OP]);
						}

						return hasAwaitingPickProducts;
					});

				return hasAwaitingPicksCache.Contains(transferLine.WE_OP);
			}
		}

		protected override ZString FormflowType => WarehouseTaskFormFlowTypes.ReplenishmentJob;

		protected override ZString TaskAndWorkflowName => Res.GetString("548b8baf-d38d-4a8e-8ccf-383e2526ba41", "Replenishment");

		protected override short RawNudge => 200;
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsPickLineInPickFaceAfterPickingService : IAfterCommittedService
	{
		public WhsPickLineInPickFaceAfterPickingService(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		BusinessObjectFactory Factory { get; }
		HashSet<ZGuid> InventoryPKCache { get; } = new HashSet<ZGuid>();
		IServiceTaskNudger ServiceTaskNudger { get; } = ObjectFactory.Get<IServiceTaskNudger>();

		public void RegisterInventory(ZGuid inventoryPK) => InventoryPKCache.Add(inventoryPK);

		public void DoAfterCommitted(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			if (InventoryPKCache.Count > 0)
			{
				try
				{
					if (CheckPickFacesRequireNudging())
					{
						ServiceTaskNudger.NudgeServiceTask("PFR");
					}
				}
				finally
				{
					InventoryPKCache.Clear();
				}
			}

			bool CheckPickFacesRequireNudging()
			{
				var sql = $@"
SELECT TOP 1
	WF_PK
FROM
	dbo.WhsPickFace
	JOIN dbo.WhsDocketLine ON WF_OP = WE_OP AND WF_WL = WE_WL
	JOIN dbo.WhsDocket ON WE_WD = WD_PK AND WF_OH_Client = WD_OH_Client
WHERE
	WE_CurrentInventoryStatus = '{InventoryStatus.Codes.Available}'
	AND WE_DocketLineStatus = '{DocketLineStatus.Codes.Finalised}'
	AND EXISTS
	(
		SELECT
			null
		FROM
			dbo.WhsDocketLine
			JOIN dbo.WhsDocket ON WE_WD = WD_PK
		WHERE
			WE_PK IN (SELECT VALUE FROM @InventoryPKs) AND
			WF_OH_Client = WD_OH_Client AND WF_OP = WE_OP AND WF_WL = WE_WL
	)
	AND NOT EXISTS
	(
		SELECT
			null
		FROM
			dbo.WhsDocketLine
			JOIN dbo.WhsDocket ON WE_WD = WD_PK
		WHERE
			WE_DocketLineType = '{DocketType.Codes.Transfer}' AND
			WE_DocketLineStatus != '{DocketLineStatus.Codes.Finalised}' AND
			WF_OH_Client = WD_OH_Client AND WF_OP = WE_OP AND WF_WL = WE_WL
	)
GROUP BY
	WF_PK, WF_ReplenishMinimum
HAVING
	SUM(WE_StockOnHand) <= WF_ReplenishMinimum";

				var sqlParameters = new ZSqlParameterCollection();
				sqlParameters.Add(ZSqlParameter.New("@InventoryPKs", value: InventoryPKCache.ToArray(), WhsInventoryViewSchema.PK, isTableValued: true));

				var pickFacesMayNeedReplenishment = new DynamicBusinessObjectCollection(Factory);
				pickFacesMayNeedReplenishment.Load(sql, sqlParameters);

				return pickFacesMayNeedReplenishment.Count > 0;
			}
		}
	}
}

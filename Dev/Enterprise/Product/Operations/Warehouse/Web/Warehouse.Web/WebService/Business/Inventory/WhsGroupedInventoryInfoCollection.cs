using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public class WhsGroupedInventoryInfoCollection : DataObjectInfoCollection<WhsGroupedInventoryInfo>
	{
		#region Constructor

		public WhsGroupedInventoryInfoCollection()
		{
			TotalCount = 0;
		}

		public WhsGroupedInventoryInfoCollection(WhsInventoryLevel level, WhsInventoryView[] inventory, WhsWarehouse whs, int firstInventoriesCount)
			: this()
		{
			if (inventory == null)
			{
				throw new ArgumentNullException(nameof(inventory), "inventory is required.");
			}

			if (firstInventoriesCount <= 0)
			{
				throw new ArgumentException("firstInventoriesCount must greater than 0.", nameof(firstInventoriesCount));
			}

			WhsGroupedInventoryInfo[] inventoryInfos = null;
			switch (level)
			{
				case WhsInventoryLevel.Level1:
					inventoryInfos = CreateInventoryInfosWithInventoryDynamicBizOCollection(SQL_GroupInventoryByProductAndClient, inventory, whs);
					break;
				case WhsInventoryLevel.Level2:
					inventoryInfos = CreateInventoryInfosWithInventoryDynamicBizOCollection(SQL_GroupInventoryByAttributes, inventory, whs);
					break;
				case WhsInventoryLevel.Level3:
					inventoryInfos = CreateInventoryInfos(inventory.Take(firstInventoriesCount));
					break;
			}

			if (inventoryInfos != null)
			{
				AddRange(inventoryInfos.Take(firstInventoriesCount));
				InventoryLineInfoCollection.InventoryLineInfos = this;
				TotalCount = level == WhsInventoryLevel.Level3 ? inventory.Length : inventoryInfos.Length;
			}
		}

		#endregion

		#region InventoryLineInfoCollection

		public WhsGroupedInventoryLineInfoCollection InventoryLineInfoCollection => inventoryLineInfoCollection ?? (inventoryLineInfoCollection = new WhsGroupedInventoryLineInfoCollection());
		WhsGroupedInventoryLineInfoCollection inventoryLineInfoCollection;

		#endregion

		#region CreateInventoryInfosWithInventoryDynamicBizOCollection

		WhsGroupedInventoryInfo[] CreateInventoryInfosWithInventoryDynamicBizOCollection(string sqlQuery, WhsInventoryView[] inventories, WhsWarehouse whs)
		{
			var factory = inventories[0].Factory;
			var inventorySummaries = new DynamicBusinessObjectCollection(factory);

			inventorySummaries.Load(sqlQuery, new[] { ZSqlParameter.New("@Inventories", inventories.Select(i => i.WI_WE_InDocketLine).ToArray(), WhsDocketLineSchema.PK, isTableValued: true) });
			var groupedInventories = inventorySummaries.Select(
				inv =>
				new WhsGroupedInventoryInfo
				(
					InventoryLineInfoCollection,
					new Guid(inv[WhsInventoryViewSchema.Constants.PK].ToString()),
					factory.Load<OrgHeader>((ZGuid)inv[WhsInventoryViewSchema.Constants.WI_OH_Client]),
					factory.Load<OrgSupplierPart>((ZGuid)inv[WhsInventoryViewSchema.Constants.WI_OP]),
					whs,
					(ZString)inv["PartAttrib1"],
					(ZString)inv["PartAttrib2"],
					(ZString)inv["PartAttrib3"],
					(ZString)inv["SerialNumber"],
					((ZDateTime)inv["ExpiryDate"]).IsValid ? ((ZDateTime)inv["ExpiryDate"]).ToDateTime() : new DateTime(),
					((ZDateTime)inv["PackingDate"]).IsValid ? ((ZDateTime)inv["PackingDate"]).ToDateTime() : new DateTime(),
					(ZDecimal)inv["TotalUnits"],
					(ZDecimal)inv["AvailableToPick"],
					(ZInt)inv["TotalPallets"]
				));

			return groupedInventories.ToArray();
		}

		#region SQL Scripts Constants

		const string SQL_GroupInventoryByProductAndClient = @"
SELECT
	WI_PK = CAST(CAST(0 AS Binary) AS Uniqueidentifier),
	WI_OH_Client,
	WI_OP,
	PartAttrib1 = '',
	PartAttrib2 = '',
	PartAttrib3 = '',
	SerialNumber = '',
	ExpiryDate = CONVERT(DATE, null, 121),
	PackingDate = CONVERT(DATE, null, 121),
	TotalUnits = SUM(WI_TotalUnits),
	TotalPallets = COUNT(DISTINCT CASE WHEN WI_PalletID <> '' THEN WI_PalletID END),
	AvailableToPick = SUM(CASE WHEN WI_InventoryStatus = 'AVL' AND WL_LocationStatus = 'NOR' THEN WI_TotalUnits - ISNULL(CommittedUnits, 0) ELSE 0 END)
FROM
	dbo.WhsInventoryView
	LEFT JOIN dbo.WhsLocation ON WI_WL = WL_PK
	JOIN dbo.OrgSupplierPart ON WI_OP = OP_PK
	CROSS APPLY
	(
		SELECT SUM(WZ_Units) as CommittedUnits
		FROM dbo.WhsCommittedStock
		WHERE WZ_WE_InventoryLine = WI_PK
	) AS CommittedUnits
WHERE
	WI_PK IN (SELECT Value FROM @Inventories)
GROUP BY
	WI_OH_Client,
	WI_OP,
	OP_PartNum
ORDER BY OP_PartNum
";

		const string SQL_GroupInventoryByAttributes = @"
SELECT
	WI_PK = CAST(CAST(0 AS Binary) AS Uniqueidentifier),
	WI_OH_Client,
	WI_OP,
	PartAttrib1 = WI_PartAttrib1,
	PartAttrib2 = WI_PartAttrib2,
	PartAttrib3 = WI_PartAttrib3,
	SerialNumber = WI_SerialNumber,
	ExpiryDate = WI_ExpiryDate,
	PackingDate = WI_PackingDate,
	TotalUnits = SUM(WI_TotalUnits),
	TotalPallets = COUNT(DISTINCT CASE WHEN WI_PalletID <> '' THEN WI_PalletID END),
	AvailableToPick = SUM(CASE WHEN WI_InventoryStatus = 'AVL' AND WL_LocationStatus = 'NOR' THEN WI_TotalUnits - ISNULL(CommittedUnits, 0) ELSE 0 END)
FROM
	dbo.WhsInventoryView
	LEFT JOIN dbo.WhsLocation ON WI_WL = WL_PK
	CROSS APPLY
	(
		SELECT SUM(WZ_Units) as CommittedUnits
		FROM dbo.WhsCommittedStock
		WHERE WZ_WE_InventoryLine = WI_PK
	) AS CommittedUnits
WHERE
	WI_PK IN (SELECT Value FROM @Inventories)
GROUP BY
	WI_OH_Client,
	WI_OP,
	WI_PartAttrib1,
	WI_PartAttrib2,
	WI_PartAttrib3,
	WI_SerialNumber,
	WI_ExpiryDate,
	WI_PackingDate
";

		#endregion

		#endregion

		#region CreateInventoryInfos

		WhsGroupedInventoryInfo[] CreateInventoryInfos(IEnumerable<WhsInventoryView> inventory)
		{
			return inventory.Select(i => new WhsGroupedInventoryInfo(InventoryLineInfoCollection, i)).ToArray();
		}

		#endregion

		#region TotalCount

		public int TotalCount { get; private set; }

		#endregion
	}
}

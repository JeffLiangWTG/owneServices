using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public class WhsGroupedInventoryInfo : WhsInventoryLineBaseInfo<WhsGroupedInventoryInfo>
	{
		#region Constructors

		public WhsGroupedInventoryInfo()
			: base()
		{
			TotalUnitsOnHand = 0;
			TotalUnitsOnAvailable = 0;
			TotalPalletIDs = 0;
		}

		public WhsGroupedInventoryInfo(WhsInventoryLineBaseInfoCollection<WhsGroupedInventoryInfo> lineInfoCollection, Guid pk, OrgHeader client, OrgSupplierPart part, WhsWarehouse whs,
			string attribute1, string attribute2, string attribute3, string serialNumber, DateTime expiryDate, DateTime packingDate,
			decimal totalUnitsOnHand, decimal totalUnitsOnAvailable, int totalPalletIDs)
			: base(lineInfoCollection, client, part, whs)
		{
			PK = pk;
			ProductPK = part.PK.ToGuid();
			ClientCode = client.OH_Code;
			ClientPK = client.PK.ToGuid();
			QtyUQ = part.OP_StockKeepingUnit;
			Attribute1 = attribute1;
			Attribute2 = attribute2;
			Attribute3 = attribute3;
			SerialNumber = serialNumber;
			ExpiryDate = expiryDate;
			PackingDate = packingDate;
			TotalUnitsOnHand = totalUnitsOnHand;
			TotalUnitsOnAvailable = totalUnitsOnAvailable;
			TotalPalletIDs = totalPalletIDs;
		}

		public WhsGroupedInventoryInfo(WhsInventoryLineBaseInfoCollection<WhsGroupedInventoryInfo> lineInfoCollection, WhsInventoryView inventory)
			: base(lineInfoCollection, inventory)
		{
		}

		#endregion

		#region Properties

		#region TotalUnitsOnHand

		public decimal TotalUnitsOnHand { get; set; }

		#endregion

		#region TotalUnitsOnAvailable

		public decimal TotalUnitsOnAvailable { get; set; }

		#endregion

		#region TotalPalletIDs

		public int TotalPalletIDs { get; set; }

		#endregion

		#endregion
	}
}

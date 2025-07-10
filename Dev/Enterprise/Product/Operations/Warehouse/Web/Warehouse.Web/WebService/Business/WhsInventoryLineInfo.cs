using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public abstract class WhsInventoryLineBaseInfo : DataObjectInfo
	{
		#region Constructors

		protected WhsInventoryLineBaseInfo()
		{
			Location = "";
			Location_UserFriendly = "";
			LocationFormattedCheckDigit = "";
			DestinationLocation = "";
			DestinationLocation_UserFriendly = "";
			DestinationLocationFormattedCheckDigit = "";
			DestinationPalletId = "";
			HasPutawayTransfer = false;
			ClientCode = "";
			Attribute1 = "";
			Attribute2 = "";
			Attribute3 = "";
			SerialNumber = "";
			ExpiryDate = new DateTime();
			PackingDate = new DateTime();
			Packs = 0m;
			Qty = 0m;
			QtyUQ = "";
			PalletID = "";
			InventoryStatus = "";
			HeldCode = "";
			HeldCodeDesc = "";
			HoldReason = "";
			PK = Guid.Empty;
			DocketPK = Guid.Empty;
			ClientCode = "";
			ProductPK = Guid.Empty;
			ClientPK = Guid.Empty;
			AvailableQty = 0m;
			AvailableToTransferQty = 0m;
			LocationPK = Guid.Empty;
			DestinationLocationPK = Guid.Empty;
		}

		protected WhsInventoryLineBaseInfo(WhsInventoryView inventory)
		{
			var part = Argument.NotNull(inventory.SupplierPart, "inventory.SupplierPart");
			var client = Argument.NotNull(inventory.Client, "inventory.Client");

			ProductPK = part.PK.ToGuid();
			ClientPK = client.PK.ToGuid();

			QtyUQ = part.OP_StockKeepingUnit;
			DocketPK = inventory.Docket != null ? inventory.Docket.PK.ToGuid() : Guid.Empty;

			PK = inventory.PK.ToGuid();
			InventoryStatus = inventory.WI_InventoryStatus;
			HeldCode = inventory.WI_HeldCode;

			var currentHeldCode = inventory.CurrentHeldCode;
			HeldCodeDesc = currentHeldCode != null ? (string)currentHeldCode.WHC_DescriptionMultilingual : HeldCode;
			StatusDesc = inventory.StatusDesc;
			Attribute1 = inventory.WI_PartAttrib1;
			Attribute2 = inventory.WI_PartAttrib2;
			Attribute3 = inventory.WI_PartAttrib3;
			SerialNumber = inventory.WI_SerialNumber;

			var docketLine = inventory.InDocketLine;
			Packs = docketLine?.WE_PackQuantity ?? 0m;
			HoldReason = docketLine?.WE_CurrentHoldReason ?? "";

			Qty = inventory.WI_TotalUnits;
			AvailableQty = inventory.WI_AvailableToPickQuantity;
			AvailableToTransferQty = inventory.WI_AvailableToTransferQuantity;
			PalletID = inventory.WI_PalletID;
			var inventoryLocation = inventory.CurrentLocation;
			Location = inventoryLocation?.WLV_LocationString ?? "";
			Location_UserFriendly = inventoryLocation?.WLV_LocationString_UserFriendly ?? "";
			LocationFormattedCheckDigit = inventoryLocation?.FormattedCheckDigit ?? "";
			if (inventoryLocation != null && !inventoryLocation.PK.IsEmpty)
			{
				LocationPK = inventoryLocation.PK.ToGuid();
			}
			ClientCode = inventory.Client.OH_Code;

			var inventoryPutawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			var location = inventoryPutawayTransferLine?.Location;
			DestinationLocation = location?.WLV_LocationString ?? "";
			DestinationLocation_UserFriendly = location?.WLV_LocationString_UserFriendly ?? "";
			DestinationLocationFormattedCheckDigit = location?.FormattedCheckDigit ?? "";
			if (location != null && !location.PK.IsEmpty)
			{
				DestinationLocationPK = location.PK.ToGuid();
			}
			DestinationPalletId = inventoryPutawayTransferLine?.WE_PalletID ?? "";
			HasPutawayTransfer = inventory.HasPutawayTransfer;

			if (inventory.WI_ExpiryDate.IsValid)
			{
				ExpiryDate = inventory.WI_ExpiryDate.ToZDateTime().ToUnspecifiedDateTime();
			}
			if (inventory.WI_PackingDate.IsValid)
			{
				PackingDate = inventory.WI_PackingDate.ToZDateTime().ToUnspecifiedDateTime();
			}
			RfAttributeConfirm = RFAttributeHelper.GetRFAttributeConfirm(inventory.SupplierPart, inventory.Client);
		}

		#endregion

		#region Properties

		public int RfAttributeConfirm { get; set; }
		public string StatusDesc { get; set; }
		public Guid DocketPK { get; set; }
		public string ClientCode { get; set; }
		public Guid PK { get; set; }
		public string PalletID { get; set; }
		public decimal Packs { get; set; }
		public decimal Qty { get; set; }
		public string QtyUQ { get; set; }
		public string Attribute1 { get; set; }
		public string Attribute2 { get; set; }
		public string Attribute3 { get; set; }
		public string SerialNumber { get; set; }
		public string InventoryStatus { get; set; }
		public string HeldCode { get; set; }
		public string HeldCodeDesc { get; set; }
		public string HoldReason { get; set; }
		public DateTime ExpiryDate { get; set; }
		public DateTime PackingDate { get; set; }
		public string Location { get; set; }
		public string Location_UserFriendly { get; set; }
		public string LocationFormattedCheckDigit { get; set; }
		public string DestinationLocation { get; set; }
		public string DestinationLocation_UserFriendly { get; set; }
		public string DestinationLocationFormattedCheckDigit { get; set; }
		public string DestinationPalletId { get; set; }
		public bool HasPutawayTransfer { get; set; }
		public Guid ProductPK { get; set; }
		public Guid ClientPK { get; set; }
		public decimal AvailableQty { get; set; }
		public decimal AvailableToTransferQty { get; set; }
		public Guid LocationPK { get; set; }
		public Guid DestinationLocationPK { get; set; }

		#endregion
	}

	[Serializable]
	public abstract class WhsInventoryLineBaseInfo<T> : WhsInventoryLineBaseInfo
	{
		#region Constructors

		protected WhsInventoryLineBaseInfo()
			: base()
		{ }

		protected WhsInventoryLineBaseInfo(WhsInventoryLineBaseInfoCollection<T> lineInfoCollection, WhsInventoryView inventory)
			: base(inventory)
		{
			InitialiseLineInfoCollection(lineInfoCollection, inventory.Client, inventory.SupplierPart, inventory.Warehouse);
		}

		protected WhsInventoryLineBaseInfo(WhsInventoryLineBaseInfoCollection<T> lineInfoCollection, OrgHeader client, OrgSupplierPart part, WhsWarehouse whs)
		{
			InitialiseLineInfoCollection(lineInfoCollection, client, part, whs);
		}

		void InitialiseLineInfoCollection(WhsInventoryLineBaseInfoCollection<T> lineInfoCollection, OrgHeader client, OrgSupplierPart part, WhsWarehouse whs)
		{
			Argument.NotNull(client, "inventory.Client");
			Argument.NotNull(part, "inventory.SupplierPart");
			this.LineInfoCollection = Argument.NotNull(lineInfoCollection, "lineInfoCollection");

			FillProductAndAttributesLists(part, client, whs);
		}

		#endregion

		void FillProductAndAttributesLists(OrgSupplierPart part, OrgHeader client, WhsWarehouse whs)
		{
			if (!LineInfoCollection.ProductInfos.Any(p => p.PK == part.PK))
			{
				LineInfoCollection.ProductInfos.Add(WhsProductInfo.GetInfo(part, GoodsHandlingInstructionsType.None));
			}
			if (!LineInfoCollection.ProductPartAttributesInfos.Any(pp => pp.ProductPK == part.PK && pp.ClientPK == client.PK))
			{
				LineInfoCollection.ProductPartAttributesInfos.Add(WhsProductPartAttributesInfo.GetInfo(client, part, whs));
			}
		}

		WhsInventoryLineBaseInfoCollection<T> LineInfoCollection;
	}

	[Serializable]
	public class WhsInventoryLineInfo : WhsInventoryLineBaseInfo<WhsInventoryLineInfo>
	{
		public WhsInventoryLineInfo()
			: base()
		{ }

		public WhsInventoryLineInfo(WhsInventoryLineBaseInfoCollection<WhsInventoryLineInfo> lineInfoCollection, WhsInventoryView inventory)
			: base(lineInfoCollection, inventory)
		{
		}
	}
}

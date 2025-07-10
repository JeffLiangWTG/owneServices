using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsDocketLineInfo : DataObjectInfo
	{
		#region Constructors

		public WhsDocketLineInfo()
			: base()
		{
			Attribute1 = "";
			Attribute2 = "";
			Attribute3 = "";
			SerialNumber = "";
			ExpiryDate = new DateTime();
			PackingDate = new DateTime();
			Packs = 0m;
			PackUQ = "";
			Qty = 0m;
			QtyUQ = "";
			PalletID = "";
			InventoryStatus = "";
			InventoryHeldCode = "";
			PK = Guid.Empty;
			DocketPK = Guid.Empty;
			ClientCode = "";
			ProductShelfLife = 0;
			LocationFormattedCheckDigit = "";
			DestLocationFormattedCheckDigit = "";
		}

		public WhsDocketLineInfo(OrgHeader client, OrgSupplierPart part, string barcode, Guid docketPK, WhsWarehouse warehouse)
			: this()
		{
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(part, nameof(part));
			Argument.NotNull(warehouse, nameof(warehouse));

			PartAttributes = WhsProductPartAttributesInfo.GetInfo(client, part, warehouse);

			var noteType = GoodsHandlingInstructionsType.None;
			if (docketPK != Guid.Empty)
			{
				var docketType = part.Factory.Load<WhsDocket>(docketPK)?.WD_DocketType ?? string.Empty;
				noteType = GetNoteTypeForDocket(docketType);
				SetPackingDateLimits(docketType);
				SetExpiryDateLimits(docketType);
			}

			Product = WhsProductInfo.GetInfo(part, noteType);
			QtyUQ = part.OP_StockKeepingUnit;
			PackUQ = part.OP_StockKeepingUnit;
			DocketPK = docketPK;
			foreach (OrgSupplierPartBarcode partBarcode in part.PartBarcodes)
			{
				if (partBarcode.PH_Barcode.ToUpper() == barcode.ToUpper(CultureInfo.CurrentCulture))
				{
					PackUQ = partBarcode.PH_F3_NKPackType;
					break;
				}
			}

			InventoryHeldCode = part.GetProductDefaultHoldCode(client.PK);

			if (PartAttributes.PackingDateIsUsed && PartAttributes.ExpiryDateIsUsed)
			{
				var whsProductParamsByWhsAndClient = WhsProduct.GetWhsProduct(part).GetParamsByWhsAndClient(warehouse, client);
				if (whsProductParamsByWhsAndClient != null)
				{
					ProductShelfLife = whsProductParamsByWhsAndClient.W3_MaximumShelfLife;
				}
			}
		}

		public WhsDocketLineInfo(WhsTransferLine transferLine)
			: this((WhsDocketLine)transferLine)
		{
			Qty = transferLine.QtyToMoveIncludingMatchingLines;
			Packs = transferLine.PackQtyIncludingMatchingLines;

			if (!string.IsNullOrEmpty(PalletID))
			{
				var locationFromDatabase = transferLine.TransferFromLocation?.LocationType;
				PalletIDNeutral = locationFromDatabase?.WLT_IsPalletIDNeutral ?? false;
			}
		}

		public WhsDocketLineInfo(WhsDocketLine docketLine)
			: this()
		{
			if (docketLine == null)
			{
				throw new ArgumentNullException(nameof(docketLine), "DocketLine is required");
			}
			var part = docketLine.SupplierPart;
			var docket = docketLine.Docket;
			var client = docket.Client;

			var noteType = GetNoteTypeForDocket(docket.WD_DocketType);

			Product = WhsProductInfo.GetInfo(part, noteType);
			PartAttributes = WhsProductPartAttributesInfo.GetInfo(client, part, docket.Warehouse);
			Packs = docketLine.WE_PackQuantity;
			PackUQ = docketLine.WE_F3_NKPackType;
			Qty = docketLine.WE_TransactionQuantity;
			QtyUQ = docketLine.ProductUQ;
			Attribute1 = docketLine.WE_PartAttrib1;
			Attribute2 = docketLine.WE_PartAttrib2;
			Attribute3 = docketLine.WE_PartAttrib3;
			SerialNumber = docketLine.WE_SerialNumber;
			if (!docketLine.WE_ExpiryDate.IsEmpty)
			{
				ExpiryDate = docketLine.WE_ExpiryDate.ToDateTime();
			}
			if (!docketLine.WE_PackingDate.IsEmpty)
			{
				PackingDate = docketLine.WE_PackingDate.ToDateTime();
			}
			PalletID = docketLine.WE_TransferFromPalletId;
			InventoryStatus = docketLine.WE_OriginalInventoryStatus;
			InventoryHeldCode = docketLine.WE_WHC_NKOriginalInventoryHeldCode;
			PK = docketLine.PK.ToGuid();
			DocketPK = docket.PK.ToGuid();
			ClientCode = client.OH_Code;

			var transferLine = docketLine as WhsTransferLine;
			if (transferLine != null)
			{
				var transferFromLocation = transferLine.TransferFromLocation;
				Location = transferFromLocation?.WLV_LocationString ?? "";
				Location_UserFriendly = transferFromLocation?.WLV_LocationString_UserFriendly ?? "";
				LocationFormattedCheckDigit = transferFromLocation?.FormattedCheckDigit ?? "";

				var pickedBy = transferLine.PickedBy;
				if (pickedBy != null)
				{
					PickedBy = pickedBy.GS_LoginName;
				}

				var pickedTime = transferLine.PickedTime;
				if (!pickedTime.IsEmpty)
				{
					PickedByTime = pickedTime.ToDateTime();
				}
			}

			// Need to flip for Dest transfers when we allow to edit those on RF.
			var location = docketLine.Location;
			DestLocation = location?.WLV_LocationString ?? "";
			DestLocation_UserFriendly = location?.WLV_LocationString_UserFriendly ?? "";
			DestLocationFormattedCheckDigit = location?.FormattedCheckDigit ?? "";
			DestPalletID = docketLine.WE_PalletID;

			var putawayBy = docketLine.PutawayBy;
			if (putawayBy != null)
			{
				PutawayBy = putawayBy.GS_LoginName;
			}
			if (!docketLine.WE_PutawayTime.IsEmpty)
			{
				var utcPutawayTime = docketLine.WE_PutawayTime.ToUtcDateTime();
				var branchTimeZoneSet = docket.Warehouse.RelatedCompanyBranch.HomePort.TimeZoneSet;

				PutawayByTime = branchTimeZoneSet.GetCalculationTimeZone().ToLocalTime(utcPutawayTime);
			}
			DockDoorLocationPK = docketLine.WE_WL_TransferFrom.IsValid ? docketLine.WE_WL_TransferFrom.ToGuid() : Guid.Empty;
			DockDoorLocation = Location;

			if (PartAttributes.PackingDateIsUsed)
			{
				SetPackingDateLimits(docketLine.GetExpectedPackingDateValidationRange());
			}

			if (PartAttributes.ExpiryDateIsUsed)
			{
				SetExpiryDateLimits(docketLine.GetExpectedExpiryDateValidationRange());
			}
		}

		GoodsHandlingInstructionsType GetNoteTypeForDocket(ZString docketType)
		{
			var result = GoodsHandlingInstructionsType.None;
			if (docketType == DocketType.Codes.Receive)
			{
				result = GoodsHandlingInstructionsType.Unload;
			}
			else if (docketType == DocketType.Codes.Order
				|| docketType == DocketType.Codes.WorkOrder
				|| docketType == DocketType.Codes.DynamicWorkOrder)
			{
				result = GoodsHandlingInstructionsType.OrderPicking;
			}
			return result;
		}

		void SetPackingDateLimits(ZString docketType)
		{
			if (PartAttributes.PackingDateIsUsed)
			{
				var limits = new TypeValidationLimits();
				if (docketType == DocketType.Codes.Transfer)
				{
					limits.PastYearsBeforeError = DateRangeValidation.MaximumPastYears;
				}
				SetPackingDateLimits(limits);
			}
		}

		void SetPackingDateLimits(TypeValidationLimits limits)
		{
			PartAttributes.PackingDateMaximumPastYears = limits.PastYearsBeforeError;
			PartAttributes.PackingDateMaximumFutureYears = limits.FutureYearsBeforeError;
		}

		void SetExpiryDateLimits(ZString docketType)
		{
			if (PartAttributes.ExpiryDateIsUsed)
			{
				var limits = new TypeValidationLimits { FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears };

				if (docketType == DocketType.Codes.Transfer)
				{
					limits.PastYearsBeforeError = DateRangeValidation.MaximumPastYears;
				}

				SetExpiryDateLimits(limits);
			}
		}

		void SetExpiryDateLimits(TypeValidationLimits limits)
		{
			PartAttributes.ExpiryDateMaximumPastYears = limits.PastYearsBeforeError;
			PartAttributes.ExpiryDateMaximumFutureYears = limits.FutureYearsBeforeError;
		}

		#endregion

		#region Related Property Infos

		#region Product

		public WhsProductInfo Product
		{
			get
			{
				if (product == null)
				{
					product = new WhsProductInfo();
				}
				return product;
			}
			set => product = value;
		}
		WhsProductInfo product;

		#endregion

		#region PartAttributes

		public WhsProductPartAttributesInfo PartAttributes
		{
			get
			{
				if (partAttributes == null)
				{
					partAttributes = new WhsProductPartAttributesInfo();
				}
				return partAttributes;
			}
			set => partAttributes = value;
		}
		WhsProductPartAttributesInfo partAttributes;

		#endregion

		#endregion

		#region Properties

		public Guid DocketPK { get; set; }

		public string ClientCode { get; set; }

		public Guid PK { get; set; }

		public string PalletID { get; set; }

		public decimal Packs { get; set; }

		public string PackUQ { get; set; }

		public decimal Qty { get; set; }

		public string QtyUQ { get; set; }

		public string Attribute1 { get; set; }

		public string Attribute2 { get; set; }

		public string Attribute3 { get; set; }

		public string SerialNumber { get; set; }

		public string InventoryStatus { get; set; }

		public string InventoryHeldCode { get; set; }

		public DateTime ExpiryDate { get; set; }

		public DateTime PackingDate { get; set; }

		public string Location { get; set; }

		public string Location_UserFriendly { get; set; }

		public string LocationFormattedCheckDigit { get; set; }

		public string DockDoorLocation { get; set; }

		public Guid DockDoorLocationPK { get; set; }

		public string PickedBy { get; set; }

		public DateTime PickedByTime { get; set; }

		public string DestLocation { get; set; }

		public string DestLocation_UserFriendly { get; set; }

		public string DestLocationFormattedCheckDigit { get; set; }

		public string DestPalletID { get; set; }

		public bool PalletIDNeutral { get; set; }

		public string PutawayBy { get; set; }

		public DateTime PutawayByTime { get; set; } // Represents a local DateTime for Warehouse Branch

		public short ProductShelfLife { get; set; }

		#endregion
	}
}

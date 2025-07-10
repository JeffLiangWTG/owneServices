using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsStocktakeLineInfo : DataObjectInfo
	{
		public WhsStocktakeLineInfo()
		{
		}

		public WhsStocktakeLineInfo(WhsStocktakeInfo stocktakeInfo, WhsStocktakeLine stocktakeLine)
			: this()
		{
			PK = stocktakeLine.PK.ToGuid();

			var location = stocktakeLine.Location;
			LocationString = location?.WLV_LocationString ?? ZString.Empty;
			LocationString_UserFriendly = location?.WLV_LocationString_UserFriendly ?? ZString.Empty;

			PalletID = stocktakeLine.WU_PalletID;
			IsEmptyLocation = stocktakeLine.IsEmptyLocation;
			if (!stocktakeLine.IsEmptyLocation)
			{
				var part = stocktakeLine.SupplierPart;
				ProductPK = part.PK.ToGuid();
				if (stocktakeInfo.ProductInfos.All(p => p.PK != ProductPK))
				{
					stocktakeInfo.ProductInfos.Add(WhsProductInfo.GetInfo(part, GoodsHandlingInstructionsType.None));
				}
				var client = stocktakeLine.Client;
				ClientPK = client.PK.ToGuid();
				ClientCode = client.OH_Code;
				RfAttributeConfirm = RFAttributeHelper.GetRFAttributeConfirm(part, client);
				PartAttribOne = stocktakeLine.WU_PartAttrib1;
				PartAttribTwo = stocktakeLine.WU_PartAttrib2;
				PartAttribThree = stocktakeLine.WU_PartAttrib3;
				SerialNumber = stocktakeLine.WU_SerialNumber;
				if (!stocktakeInfo.ProductPartAttributesInfos.Any(pp => pp.ClientPK == ClientPK && pp.ProductPK == ProductPK))
				{
					stocktakeInfo.ProductPartAttributesInfos.Add(WhsProductPartAttributesInfo.GetInfo(client, part, location.Warehouse));
				}
				if (!stocktakeLine.WU_PackingDate.IsEmpty)
				{
					PackingDate = stocktakeLine.WU_PackingDate.ToDateTime();
				}
				if (!stocktakeLine.WU_ExpiryDate.IsEmpty)
				{
					ExpiryDate = stocktakeLine.WU_ExpiryDate.ToDateTime();
				}
				InventoryStatus = stocktakeLine.WU_InventoryStatus;
				InventoryStatusDesc = stocktakeLine.StatusDesc;
				PackType = stocktakeLine.WU_F3_NKPackType;
				CurrentCount = stocktakeLine.CurrentCount;
			}
			if (!stocktakeLine.CurrentCountVerifiedDate.IsEmpty)
			{
				DateVerified = stocktakeLine.CurrentCountVerifiedDate.ToDateTime();
			}
			VerifiedBy = stocktakeLine.CurrentCountVerifiedBy;
			SystemUnits = stocktakeLine.WU_SystemUnits;
		}

		#region Properties

		public Guid PK { get; set; }
		public string LocationString { get; set; }
		public string LocationString_UserFriendly { get; set; }
		public string PalletID { get; set; }
		public Guid ProductPK { get; set; }
		public Guid ClientPK { get; set; }
		public string ClientCode { get; set; }
		public int RfAttributeConfirm { get; set; }
		public string PartAttribOne { get; set; }
		public string PartAttribTwo { get; set; }
		public string PartAttribThree { get; set; }
		public string SerialNumber { get; set; }
		public DateTime? PackingDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public string InventoryStatus { get; set; }
		public string InventoryStatusDesc { get; set; }
		public string PackType { get; set; }
		public decimal CurrentCount { get; set; }
		public DateTime? DateVerified { get; set; }
		public string VerifiedBy { get; set; }
		public decimal? SystemUnits { get; set; }
		public bool IsEmptyLocation { get; set; }

		#endregion
	}
}

using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.AsnMapping;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	static class AsnMapperDummies
	{
		#region Dummy creation

		internal static ProductInfo CreateDummyProduct()
		{
			return new ProductInfo(Guid.NewGuid(), string.Empty, string.Empty, string.Empty, string.Empty, ZDate.Empty, ZDate.Empty, string.Empty);
		}

		internal static WhsInventoryView CreateDummyInventory(WhsReceive receive, ProductInfo productInfo, int lineNo, int subLineNo, int qty)
		{
			var inv = receive.Lines.AddNew().Inventory[0];
			inv.WI_OP = productInfo.ProductPK;
			inv.WI_PartAttrib1 = productInfo.PartAttrib1;
			inv.WI_PartAttrib2 = productInfo.PartAttrib2;
			inv.WI_PartAttrib3 = productInfo.PartAttrib3;
			inv.WI_SerialNumber = productInfo.SerialNumber;
			inv.WI_ExpiryDate = productInfo.ExpiryDate;
			inv.WI_PackingDate = productInfo.PackingDate;
			inv.WI_PalletID = productInfo.PalletId;
			inv.WI_LineNo = (ZShort)lineNo;
			inv.WI_SubLineNo = (ZShort)subLineNo;
			inv.WI_InDocketLineUnits = qty;
			inv.WI_UnitsUQ = "UNT";

			return inv;
		}

		internal static WhsAsnLine CreateDummyAsnLine(WhsReceive receive, ProductInfo productInfo, int lineNo, int subLineNo, int qty)
		{
			var asnLine = receive.AsnLines.AddNew();
			asnLine.WN_OP = productInfo.ProductPK;
			asnLine.WN_PartAttrib1 = productInfo.PartAttrib1;
			asnLine.WN_PartAttrib2 = productInfo.PartAttrib2;
			asnLine.WN_PartAttrib3 = productInfo.PartAttrib3;
			asnLine.WN_SerialNumber = productInfo.SerialNumber;
			asnLine.WN_ExpiryDate = productInfo.ExpiryDate;
			asnLine.WN_PackingDate = productInfo.PackingDate;
			asnLine.WN_PalletId = productInfo.PalletId;
			asnLine.WN_LineNo = lineNo;
			asnLine.WN_SubLineNo = subLineNo;
			asnLine.WN_QuantityUQ = "UNT";
			asnLine.WN_Quantity = qty;
			return asnLine;
		}

		#endregion
	}
}

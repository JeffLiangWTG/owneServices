using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public sealed class WhsGroupedUnloadLineInfoCollection : DataObjectInfoCollection<WhsGroupedUnloadLineInfo>
	{
		public WhsGroupedUnloadLineInfoCollection()
		{
		}

		public WhsGroupedUnloadLineInfoCollection(IEnumerable<WhsAsnLine> asnLines, OrgHeader client, WhsWarehouse whs)
			: this()
		{
			var groupsWithSerials =
				from asnLine in asnLines
				let part = asnLine.SupplierPart
				group asnLine by new
				{
					asnLine.WN_PalletId,
					SupplierPart = part,
					Product = WhsProduct.GetWhsProduct(part),
					asnLine.WN_PackingDate,
					asnLine.WN_ExpiryDate,
					asnLine.WN_PartAttrib1,
					asnLine.WN_PartAttrib2,
					asnLine.WN_PartAttrib3,
					asnLine.WN_SerialNumber,
					asnLine.WN_QuantityUQ,
				}
				into groupedAsnLines
				select new
				{
					groupedAsnLines.Key.SupplierPart,
					PalletID = groupedAsnLines.Key.WN_PalletId,
					PackingDate = groupedAsnLines.Key.WN_PackingDate.IsEmpty ? new DateTime() : groupedAsnLines.Key.WN_PackingDate.ToDateTime(),
					ExpiryDate = groupedAsnLines.Key.WN_ExpiryDate.IsEmpty ? new DateTime() : groupedAsnLines.Key.WN_ExpiryDate.ToDateTime(),
					RealAttribute1 = (string)groupedAsnLines.Key.WN_PartAttrib1,
					RealAttribute2 = (string)groupedAsnLines.Key.WN_PartAttrib2,
					RealAttribute3 = (string)groupedAsnLines.Key.WN_PartAttrib3,
					RealSerialNumber = (string)groupedAsnLines.Key.WN_SerialNumber,
					GroupedAttribute1 = (string)groupedAsnLines.Key.WN_PartAttrib1,
					GroupedAttribute2 = (string)groupedAsnLines.Key.WN_PartAttrib2,
					GroupedAttribute3 = (string)groupedAsnLines.Key.WN_PartAttrib3,
					GroupedSerialNumber = groupedAsnLines.Key.Product.IsSerialNumberUsedAndNotReleaseCaptured(client) ? Res.GetString("4814b820-4fbe-4d05-a482-056a096ebfec", "<Many>") : (string)groupedAsnLines.Key.WN_SerialNumber,
					QtyUQ = groupedAsnLines.Key.WN_QuantityUQ,
					Qty = groupedAsnLines.Sum(a => a.WN_Quantity)
				};
			AddRange(
				from partiallyGroupedAsn in groupsWithSerials
				group partiallyGroupedAsn by new
				{
					partiallyGroupedAsn.SupplierPart,
					partiallyGroupedAsn.PalletID,
					partiallyGroupedAsn.PackingDate,
					partiallyGroupedAsn.ExpiryDate,
					partiallyGroupedAsn.GroupedAttribute1,
					partiallyGroupedAsn.GroupedAttribute2,
					partiallyGroupedAsn.GroupedAttribute3,
					partiallyGroupedAsn.GroupedSerialNumber,
					partiallyGroupedAsn.QtyUQ
				}
				into groupedASNs
				select new WhsGroupedUnloadLineInfo(client, groupedASNs.Key.SupplierPart, whs)
				{
					PalletID = groupedASNs.Key.PalletID,
					PackingDate = groupedASNs.Key.PackingDate,
					ExpiryDate = groupedASNs.Key.ExpiryDate,
					Attribute1 = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedAttribute1 : groupedASNs.First().RealAttribute1,
					Attribute2 = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedAttribute2 : groupedASNs.First().RealAttribute2,
					Attribute3 = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedAttribute3 : groupedASNs.First().RealAttribute3,
					SerialNumber = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedSerialNumber : groupedASNs.First().RealSerialNumber,
					QtyUQ = groupedASNs.Key.QtyUQ,
					Qty = groupedASNs.Sum(g => g.Qty)
				}
			);
		}

		public WhsGroupedUnloadLineInfoCollection(IEnumerable<WhsInventoryView> inventory, OrgHeader client, WhsWarehouse whs)
			: this()
		{
			var groupsWithSerials =
				from inventoryLine in inventory
				let part = inventoryLine.SupplierPart
				group inventoryLine by new
				{
					inventoryLine.WI_PalletID,
					SupplierPart = part,
					Product = WhsProduct.GetWhsProduct(part),
					inventoryLine.WI_PackingDate,
					inventoryLine.WI_ExpiryDate,
					inventoryLine.WI_PartAttrib1,
					inventoryLine.WI_PartAttrib2,
					inventoryLine.WI_PartAttrib3,
					inventoryLine.WI_SerialNumber,
					inventoryLine.WI_UnitsUQ,
				}
				into groupedAsnLines
				select new
				{
					groupedAsnLines.Key.SupplierPart,
					PalletID = groupedAsnLines.Key.WI_PalletID,
					PackingDate = groupedAsnLines.Key.WI_PackingDate.IsEmpty ? new DateTime() : groupedAsnLines.Key.WI_PackingDate.ToDateTime(),
					ExpiryDate = groupedAsnLines.Key.WI_ExpiryDate.IsEmpty ? new DateTime() : groupedAsnLines.Key.WI_ExpiryDate.ToDateTime(),
					RealAttribute1 = (string)groupedAsnLines.Key.WI_PartAttrib1,
					RealAttribute2 = (string)groupedAsnLines.Key.WI_PartAttrib2,
					RealAttribute3 = (string)groupedAsnLines.Key.WI_PartAttrib3,
					RealSerialNumber = (string)groupedAsnLines.Key.WI_SerialNumber,
					GroupedAttribute1 = (string)groupedAsnLines.Key.WI_PartAttrib1,
					GroupedAttribute2 = (string)groupedAsnLines.Key.WI_PartAttrib2,
					GroupedAttribute3 = (string)groupedAsnLines.Key.WI_PartAttrib3,
					GroupedSerialNumber = groupedAsnLines.Key.Product.IsSerialNumberUsedAndNotReleaseCaptured(client) ? Res.GetString("4814b820-4fbe-4d05-a482-056a096ebfec", "<Many>") : (string)groupedAsnLines.Key.WI_SerialNumber,
					QtyUQ = groupedAsnLines.Key.WI_UnitsUQ,
					Qty = groupedAsnLines.Sum(a => a.WI_TotalUnits)
				};
			AddRange(
				from partiallyGroupedAsn in groupsWithSerials
				group partiallyGroupedAsn by new
				{
					partiallyGroupedAsn.SupplierPart,
					partiallyGroupedAsn.PalletID,
					partiallyGroupedAsn.PackingDate,
					partiallyGroupedAsn.ExpiryDate,
					partiallyGroupedAsn.GroupedAttribute1,
					partiallyGroupedAsn.GroupedAttribute2,
					partiallyGroupedAsn.GroupedAttribute3,
					partiallyGroupedAsn.GroupedSerialNumber,
					partiallyGroupedAsn.QtyUQ
				}
				into groupedASNs
				select new WhsGroupedUnloadLineInfo(client, groupedASNs.Key.SupplierPart, whs)
				{
					PalletID = groupedASNs.Key.PalletID,
					PackingDate = groupedASNs.Key.PackingDate,
					ExpiryDate = groupedASNs.Key.ExpiryDate,
					Attribute1 = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedAttribute1 : groupedASNs.First().RealAttribute1,
					Attribute2 = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedAttribute2 : groupedASNs.First().RealAttribute2,
					Attribute3 = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedAttribute3 : groupedASNs.First().RealAttribute3,
					SerialNumber = groupedASNs.Sum(g => g.Qty) > 1 ? groupedASNs.Key.GroupedSerialNumber : groupedASNs.First().RealSerialNumber,
					QtyUQ = groupedASNs.Key.QtyUQ,
					Qty = groupedASNs.Sum(g => g.Qty)
				}
			);
		}
	}
}

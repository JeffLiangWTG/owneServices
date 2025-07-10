using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class ProductWithAttributes
	{
		public ProductWithAttributes(
			ZGuid clientPK,
			ZGuid productPK,
			ZString pA1,
			ZString pA2,
			ZString pA3,
			ZString serialNumber,
			ZDate expiryDate,
			ZDate packingDate,
			ZString bondedEntryKey,
			ZString allocationKey,
			ZDecimal perPackageQty)
		{
			ClientPK = clientPK;
			ProductPK = productPK;
			PartAttrib1 = pA1;
			PartAttrib2 = pA2;
			PartAttrib3 = pA3;
			SerialNumber = serialNumber;
			ExpiryDate = expiryDate;
			PackingDate = packingDate;
			BondedEntryKey = bondedEntryKey;
			AllocationKey = allocationKey;
			PerPackageQty = perPackageQty;
		}

		public ProductWithAttributes()
		{
		}

		public ZGuid ClientPK { get; }
		public ZGuid ProductPK { get; }
		public ZString PartAttrib1 { get; }
		public ZString PartAttrib2 { get; }
		public ZString PartAttrib3 { get; }
		public ZString SerialNumber { get; }
		public ZDate ExpiryDate { get; }
		public ZDate PackingDate { get; }
		public ZString BondedEntryKey { get; }
		public ZString AllocationKey { get; }
		public ZDecimal PerPackageQty { get; }

		public override int GetHashCode()
		{
			return ClientPK.GetHashCode()
				^ ProductPK.GetHashCode()
				^ PartAttrib1.GetHashCode()
				^ PartAttrib2.GetHashCode()
				^ PartAttrib3.GetHashCode()
				^ SerialNumber.GetHashCode()
				^ ExpiryDate.GetHashCode()
				^ PackingDate.GetHashCode()
				^ BondedEntryKey.GetHashCode()
				^ AllocationKey.GetHashCode()
				^ PerPackageQty.GetHashCode();
		}

		public override bool Equals(object obj) => obj is ProductWithAttributes pwa && pwa == this;

		public static bool operator ==(ProductWithAttributes lhs, ProductWithAttributes rhs)
		{
			return ReferenceEquals(lhs, rhs) ||
				(!(lhs is null) && !(rhs is null)
				&& lhs.ClientPK == rhs.ClientPK
				&& lhs.ProductPK == rhs.ProductPK
				&& lhs.BondedEntryKey == rhs.BondedEntryKey
				&& lhs.AllocationKey == rhs.AllocationKey
				&& lhs.SerialNumber == rhs.SerialNumber
				&& lhs.PartAttrib1 == rhs.PartAttrib1
				&& lhs.PartAttrib2 == rhs.PartAttrib2
				&& lhs.PartAttrib3 == rhs.PartAttrib3
				&& lhs.ExpiryDate == rhs.ExpiryDate
				&& lhs.PackingDate == rhs.PackingDate
				&& lhs.PerPackageQty == rhs.PerPackageQty);
		}

		public static bool operator !=(ProductWithAttributes productWithAttributes1, ProductWithAttributes productWithAttributes2) => !(productWithAttributes1 == productWithAttributes2);

		#region GetProductWithAttributes

		public static ProductWithAttributes GetProductWithAttributes(WhsPickOrderedInventory orderedInventory)
		{
			return new ProductWithAttributes(
				orderedInventory.Client.PK,
				orderedInventory.SupplierPart.PK,
				orderedInventory.PartAttrib1,
				orderedInventory.PartAttrib2,
				orderedInventory.PartAttrib3,
				orderedInventory.SerialNumber,
				orderedInventory.ExpiryDate,
				orderedInventory.PackingDate,
				orderedInventory.BondedEntryKey,
				orderedInventory.AllocationKey,
				0m);
		}

		public static ProductWithAttributes GetProductWithAttributes(WhsInventoryView inventory, bool useBondedEntryKey)
		{
			return new ProductWithAttributes(
				inventory.WI_OH_Client,
				inventory.WI_OP,
				inventory.WI_PartAttrib1,
				inventory.WI_PartAttrib2,
				inventory.WI_PartAttrib3,
				inventory.WI_SerialNumber,
				inventory.WI_ExpiryDate,
				inventory.WI_PackingDate,
				useBondedEntryKey ? inventory.WI_BondedEntryKey : ZString.Empty,
				inventory.WI_AllocationKey,
				inventory.PerPackageQty);
		}

		public static ProductWithAttributes GetProductWithAttributes(WhsPickAvailableInventory availableInventory)
		{
			var clientPK = availableInventory.Inventory.Count > 0 ? availableInventory.Inventory[0].WI_OH_Client : ZGuid.Empty;
			return new ProductWithAttributes(
				clientPK,
				availableInventory.SupplierPart.PK,
				availableInventory.PartAttrib1,
				availableInventory.PartAttrib2,
				availableInventory.PartAttrib3,
				availableInventory.SerialNumber,
				availableInventory.ExpiryDate,
				availableInventory.PackingDate,
				availableInventory.BondedEntryKey,
				availableInventory.AllocationKey,
				availableInventory.PerPackageQty);
		}

		public static ProductWithAttributes GetProductWithAttributes(WhsDocketLine docketLine)
		{
			var clientPK = docketLine.Docket?.WD_OH_Client ?? ZGuid.Empty;
			return new ProductWithAttributes(
				clientPK,
				docketLine.WE_OP,
				docketLine.WE_PartAttrib1,
				docketLine.WE_PartAttrib2,
				docketLine.WE_PartAttrib3,
				docketLine.WE_SerialNumber,
				docketLine.WE_ExpiryDate,
				docketLine.WE_PackingDate,
				docketLine.WE_BondedEntryKey,
				docketLine.WE_AllocationKey,
				docketLine.WE_PerPackageQty);
		}

		#endregion

		#region IsMatchingWithEmptyCheck

		public static bool IsMatchingWithEmptyCheck(ProductWithAttributes orderedProductWithAttributes, ProductWithAttributes inventoryProductWithAttributes)
		{
			return
				orderedProductWithAttributes.ClientPK == inventoryProductWithAttributes.ClientPK &&
				orderedProductWithAttributes.ProductPK == inventoryProductWithAttributes.ProductPK &&
				(orderedProductWithAttributes.BondedEntryKey.IsEmpty || orderedProductWithAttributes.BondedEntryKey.EqualsIgnoringCase(inventoryProductWithAttributes.BondedEntryKey)) &&
				(orderedProductWithAttributes.AllocationKey.IsEmpty || orderedProductWithAttributes.AllocationKey.EqualsIgnoringCase(inventoryProductWithAttributes.AllocationKey)) &&
				(orderedProductWithAttributes.SerialNumber.IsEmpty || orderedProductWithAttributes.SerialNumber.EqualsIgnoringCase(inventoryProductWithAttributes.SerialNumber)) &&
				(orderedProductWithAttributes.PartAttrib1.IsEmpty || orderedProductWithAttributes.PartAttrib1.EqualsIgnoringCase(inventoryProductWithAttributes.PartAttrib1)) &&
				(orderedProductWithAttributes.PartAttrib2.IsEmpty || orderedProductWithAttributes.PartAttrib2.EqualsIgnoringCase(inventoryProductWithAttributes.PartAttrib2)) &&
				(orderedProductWithAttributes.PartAttrib3.IsEmpty || orderedProductWithAttributes.PartAttrib3.EqualsIgnoringCase(inventoryProductWithAttributes.PartAttrib3)) &&
				(orderedProductWithAttributes.ExpiryDate.IsEmpty || orderedProductWithAttributes.ExpiryDate == inventoryProductWithAttributes.ExpiryDate) &&
				(orderedProductWithAttributes.PackingDate.IsEmpty || orderedProductWithAttributes.PackingDate == inventoryProductWithAttributes.PackingDate);
		}

		#endregion
	}
}

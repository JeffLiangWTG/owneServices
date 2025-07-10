using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	class PickLineGroupingInfo
	{
		public PickLineGroupingInfo(WhsPickLine pickLine)
			: this(pickLine, "", 0)
		{
		}

		public PickLineGroupingInfo(WhsPickLine pickLine, string packageID, short slotNumber)
		{
			var inventoryLine = Argument.NotNull(pickLine.InventoryLine, "pickLine.InventoryLine");
			Product = Argument.NotNull(inventoryLine.SupplierPart, "inventoryLine.SupplierPart");
			Client = Argument.NotNull(inventoryLine.Docket.Client, "inventory.Docket.Client");
			Location = Argument.NotNull(inventoryLine.Location, "inventoryLine.Location");
			PalletID = inventoryLine.WE_PalletID;
			UnitsUQ = pickLine.WZ_UnitsUQ;
			var docketLine = Argument.NotNull((WhsPickableDocketLine)pickLine.DocketLine, "pickLine.DocketLine");

			BOMProduct = docketLine.WE_WE_ParentDocketLine.IsValid ? docketLine.ParentLine.SupplierPart : null;

			Attribute1 = inventoryLine.WE_PartAttrib1;
			Attribute2 = inventoryLine.WE_PartAttrib2;
			Attribute3 = inventoryLine.WE_PartAttrib3;
			SerialNumber = inventoryLine.WE_SerialNumber;
			ExpiryDate = inventoryLine.WE_ExpiryDate;
			PackingDate = inventoryLine.WE_PackingDate;

			IsLocationEmptyAfterFinalisingPick = ((IEmptyLocationAfterPickFinalisation)pickLine).IsLocationEmptyAfterFinalisingPick;

			// Only set OrderedPartAttribute if Product is Attribute Neutral or the location is Pallet ID Neutral.
			if (Client.PartAttributeManager.IsAttributeNeutralUsedByProduct(Product) || (Location.LocationType?.WLT_IsPalletIDNeutral ?? false))
			{
				OrderedPartAttribute1 = docketLine.WE_PartAttrib1;
				OrderedPartAttribute2 = docketLine.WE_PartAttrib2;
				OrderedPartAttribute3 = docketLine.WE_PartAttrib3;
				OrderedSerialNumber = docketLine.WE_SerialNumber;
				OrderedExpiryDate = docketLine.WE_ExpiryDate;
				OrderedPackingDate = docketLine.WE_PackingDate;
			}

			OrderedPalletID = docketLine.WE_PalletID;
			PackageID = packageID;
			SlotNumber = slotNumber;
		}

		internal readonly ZString OrderedPartAttribute1;
		internal readonly ZString OrderedPartAttribute2;
		internal readonly ZString OrderedPartAttribute3;
		internal readonly ZString OrderedSerialNumber;
		internal readonly ZDateTime OrderedExpiryDate;
		internal readonly ZDateTime OrderedPackingDate;
		internal readonly ZString OrderedPalletID;

		internal readonly OrgSupplierPart Product;
		internal readonly OrgSupplierPart BOMProduct;
		internal readonly OrgHeader Client;
		internal readonly WhsLocation Location;
		internal readonly string PalletID;
		internal readonly string UnitsUQ;
		internal readonly string Attribute1;
		internal readonly string Attribute2;
		internal readonly string Attribute3;
		internal readonly string SerialNumber;
		internal readonly ZDateTime ExpiryDate;
		internal readonly ZDateTime PackingDate;
		internal readonly bool IsLocationEmptyAfterFinalisingPick;
		internal readonly string PackageID;
		internal readonly short SlotNumber;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj.GetType() == this.GetType() && Equals((PickLineGroupingInfo)obj);
		}

		protected bool Equals(PickLineGroupingInfo other)
		{
			return Product.Equals(other.Product)
				&& (ReferenceEquals(BOMProduct, other.BOMProduct) || BOMProduct != null && BOMProduct.Equals(other.BOMProduct))
				&& Client.Equals(other.Client)
				&& Location.Equals(other.Location)
				&& string.Equals(PalletID, other.PalletID)
				&& string.Equals(UnitsUQ, other.UnitsUQ)
				&& string.Equals(Attribute1, other.Attribute1)
				&& string.Equals(Attribute2, other.Attribute2)
				&& string.Equals(Attribute3, other.Attribute3)
				&& string.Equals(SerialNumber, other.SerialNumber)
				&& ExpiryDate.Equals(other.ExpiryDate)
				&& PackingDate.Equals(other.PackingDate)
				&& string.Equals(OrderedPartAttribute1, other.OrderedPartAttribute1)
				&& string.Equals(OrderedPartAttribute2, other.OrderedPartAttribute2)
				&& string.Equals(OrderedPartAttribute3, other.OrderedPartAttribute3)
				&& string.Equals(OrderedSerialNumber, other.OrderedSerialNumber)
				&& string.Equals(OrderedPalletID, other.OrderedPalletID)
				&& OrderedExpiryDate.Equals(other.OrderedExpiryDate)
				&& OrderedPackingDate.Equals(other.OrderedPackingDate)
				&& IsLocationEmptyAfterFinalisingPick == other.IsLocationEmptyAfterFinalisingPick
				&& PackageID == other.PackageID
				&& SlotNumber == other.SlotNumber;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Product.GetHashCode();
				hashCode = (hashCode * 397) ^ Client.GetHashCode();
				hashCode = (hashCode * 397) ^ Location.GetHashCode();
				hashCode = (hashCode * 397) ^ PalletID.GetHashCode();
				hashCode = (hashCode * 397) ^ UnitsUQ.GetHashCode();
				hashCode = (hashCode * 397) ^ Attribute1.GetHashCode();
				hashCode = (hashCode * 397) ^ Attribute2.GetHashCode();
				hashCode = (hashCode * 397) ^ Attribute3.GetHashCode();
				hashCode = (hashCode * 397) ^ SerialNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ ExpiryDate.GetHashCode();
				hashCode = (hashCode * 397) ^ PackingDate.GetHashCode();
				hashCode = (hashCode * 397) ^ OrderedPartAttribute1.GetHashCode();
				hashCode = (hashCode * 397) ^ OrderedPartAttribute2.GetHashCode();
				hashCode = (hashCode * 397) ^ OrderedPartAttribute3.GetHashCode();
				hashCode = (hashCode * 397) ^ OrderedSerialNumber.GetHashCode();
				hashCode = (hashCode * 397) ^ OrderedPalletID.GetHashCode();
				hashCode = (hashCode * 397) ^ OrderedExpiryDate.GetHashCode();
				hashCode = (hashCode * 397) ^ OrderedPackingDate.GetHashCode();
				hashCode = (hashCode * 397) ^ IsLocationEmptyAfterFinalisingPick.GetHashCode();
				hashCode = (hashCode * 397) ^ PackageID.GetHashCode();
				hashCode = (hashCode * 397) ^ SlotNumber.GetHashCode();

				if (BOMProduct != null)
				{
					hashCode = (hashCode * 397) ^ BOMProduct.GetHashCode();
				}

				return hashCode;
			}
		}
	}
}

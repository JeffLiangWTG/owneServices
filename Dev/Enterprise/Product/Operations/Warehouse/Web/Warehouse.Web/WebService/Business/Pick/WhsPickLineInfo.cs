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
#if DEBUG
	[Serializable]
#endif
	public class WhsPickLineInfo : DataObjectInfo
	{
		#region Constructors

		internal WhsPickLineInfo(WhsPickLine pickLine, WhsPickJobInfo parent, string packageID = "", short slotNumber = 0)
			: this(new PickLineGroupingInfo(pickLine), new[] { pickLine.PK }, pickLine.WZ_Units, parent, packageID, slotNumber)
		{
		}

		internal WhsPickLineInfo(PickLineGroupingInfo pickLineGroupedInfo, ZGuid[] pickLinePKs, ZDecimal totalPickLineUnits, WhsPickJobInfo parent, string packageID = "", short slotNumber = 0)
		{
			PKs = pickLinePKs.Select(x => x.ToGuid()).ToArray();
			Units = totalPickLineUnits;

			var supplierPart = pickLineGroupedInfo.Product;
			ProductPK = supplierPart.PK.ToGuid();
			var bomSupplierPart = pickLineGroupedInfo.BOMProduct;
			BOMProductPK = bomSupplierPart?.PK.ToGuid() ?? Guid.Empty;

			var location = pickLineGroupedInfo.Location;
			Location = location.WLV_LocationString;
			Location_UserFriendly = location.WLV_LocationString_UserFriendly;
			LocationFormattedCheckDigit = location.FormattedCheckDigit;
			LocationBarcode = location.OldBarcode;
			PalletID = pickLineGroupedInfo.PalletID;
			Row = location.RowName;
			var spaceDelimitedLocation = location.ToSpaceDelimitedLocationString();
			LocationWithoutDelimiter = spaceDelimitedLocation;
			LocationWithoutRowAndDelimiter = spaceDelimitedLocation.SubstringSafe(Row.Length + 1);
			UnitsUQ = pickLineGroupedInfo.UnitsUQ;

			Attribute1 = pickLineGroupedInfo.Attribute1;
			Attribute2 = pickLineGroupedInfo.Attribute2;
			Attribute3 = pickLineGroupedInfo.Attribute3;
			SerialNumber = pickLineGroupedInfo.SerialNumber;
			if (!pickLineGroupedInfo.ExpiryDate.IsEmpty)
			{
				ExpiryDate = DateTime.SpecifyKind(pickLineGroupedInfo.ExpiryDate.ToDateTime(), DateTimeKind.Unspecified);
			}
			if (!pickLineGroupedInfo.PackingDate.IsEmpty)
			{
				PackingDate = DateTime.SpecifyKind(pickLineGroupedInfo.PackingDate.ToDateTime(), DateTimeKind.Unspecified);
			}

			OrderedPartAttribute1 = pickLineGroupedInfo.OrderedPartAttribute1;
			OrderedPartAttribute2 = pickLineGroupedInfo.OrderedPartAttribute2;
			OrderedPartAttribute3 = pickLineGroupedInfo.OrderedPartAttribute3;
			OrderedSerialNumber = pickLineGroupedInfo.OrderedSerialNumber;
			if (!pickLineGroupedInfo.OrderedExpiryDate.IsEmpty)
			{
				OrderedExpiryDate = DateTime.SpecifyKind(pickLineGroupedInfo.OrderedExpiryDate.ToDateTime(), DateTimeKind.Unspecified);
			}
			if (!pickLineGroupedInfo.OrderedPackingDate.IsEmpty)
			{
				OrderedPackingDate = DateTime.SpecifyKind(pickLineGroupedInfo.OrderedPackingDate.ToDateTime(), DateTimeKind.Unspecified);
			}

			if (!string.IsNullOrEmpty(PalletID))
			{
				var isOrderedPalletID = !string.IsNullOrEmpty(pickLineGroupedInfo.OrderedPalletID);
				var isPalletIDNeutral = location.LocationType?.WLT_IsPalletIDNeutral ?? false;
				PalletIDNeutral = isPalletIDNeutral && !isOrderedPalletID;
			}

			PackageID = packageID;
			SlotNumber = slotNumber;

			var client = pickLineGroupedInfo.Client;
			ClientPK = client.PK.ToGuid();
			ClientCode = client.OH_Code;

			IsLocationEmptyAfterPicking = pickLineGroupedInfo.IsLocationEmptyAfterFinalisingPick;
			IsSingleProductInLocation = GetIsSingleProductInLocation(location, supplierPart);

			Parent = parent;
			FillProductAndAttributesListsOnParent(supplierPart, bomSupplierPart, client, location.Warehouse);
		}

		public WhsPickLineInfo()
		{
			Location = "";
			Location_UserFriendly = "";
			LocationFormattedCheckDigit = "";
			LocationBarcode = "";
			LocationWithoutDelimiter = "";
			LocationWithoutRowAndDelimiter = "";
			PalletID = "";
			Row = "";
			Attribute1 = "";
			Attribute2 = "";
			Attribute3 = "";
			SerialNumber = "";
			Units = 0;
			UnitsUQ = "";
			PKs = Array.Empty<Guid>();
			ProductPK = Guid.Empty;
			ClientPK = Guid.Empty;
			ClientCode = "";
			OrderedPartAttribute1 = "";
			OrderedPartAttribute2 = "";
			OrderedPartAttribute3 = "";
			OrderedSerialNumber = "";
			OrderedExpiryDate = DateTime.MinValue;
			OrderedPackingDate = DateTime.MinValue;
			AttachedToOrder = "";
			AttachedToPick = "";
			PackageID = "";
			SlotNumber = 0;
		}

		WhsPickJobInfo Parent;

		#region LinkToPickInfo

		internal void LinkToPickInfo(WhsPickInfo pickInfo)
		{
			Parent = pickInfo;
		}

		#endregion

		#region UpdateLists

		internal void FillProductAndAttributesListsOnParent(OrgSupplierPart part, OrgSupplierPart bomProduct, OrgHeader client, WhsWarehouse whs)
		{
			var needToAddProduct = true;
			var needToAddBOMProduct = bomProduct != null;
			foreach (var productInfo in Parent.ProductInfos)
			{
				if (productInfo.PK == ProductPK)
				{
					needToAddProduct = false;
				}
				if (productInfo.PK == BOMProductPK)
				{
					needToAddBOMProduct = false;
				}
				if (!needToAddProduct && !needToAddBOMProduct)
				{
					break;
				}
			}
			if (needToAddProduct)
			{
				Parent.ProductInfos.Add(WhsProductInfo.GetInfo(part, GoodsHandlingInstructionsType.OrderPicking));
			}
			if (needToAddBOMProduct)
			{
				Parent.ProductInfos.Add(WhsProductInfo.GetInfo(bomProduct, GoodsHandlingInstructionsType.OrderPicking));
			}

			if (!Parent.ProductPartAttributesInfos.Any(pp => pp.ProductPK == ProductPK && pp.ClientPK == ClientPK))
			{
				Parent.ProductPartAttributesInfos.Add(WhsProductPartAttributesInfo.GetInfo(client, part, whs));
			}
		}

		#endregion

		#endregion

		#region GetIsSingleProductInLocation

		static bool GetIsSingleProductInLocation(WhsLocation location, OrgSupplierPart product)
		{
			bool result;

			var locationCache = location.Factory.GetCachedValue("WhsPickLineInfo|GetIsSingleProductInLocation", () => new Dictionary<WhsLocation, bool>());
			if (!locationCache.TryGetValue(location, out result))
			{
				locationCache[location] = result = GetIsSingleProductInLocationCore(location, product);
			}

			return result;
		}

		static bool GetIsSingleProductInLocationCore(WhsLocation location, OrgSupplierPart product)
		{
			var productsInLocationQuery = new ZQuery();
			productsInLocationQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, location.PK);
			productsInLocationQuery.AddToFilter(WhsInventoryViewSchema.WI_OP, SQLComparisonOperator.NotEqual, product.PK);
			productsInLocationQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			return location.Factory.LoadTop1<WhsInventoryView>(productsInLocationQuery) == null;
		}

		#endregion

		#region Properties

		#region PK

		public Guid[] PKs { get; set; }

		#endregion

		#region Client

		public Guid ClientPK { get; set; }

		public string ClientCode { get; set; }

		#endregion

		#region Location

		public string Location { get; set; }

		public string Location_UserFriendly { get; set; }

		public string LocationFormattedCheckDigit { get; set; }

		public string LocationBarcode { get; set; }

		public string LocationWithoutDelimiter { get; set; }

		public string LocationWithoutRowAndDelimiter { get; set; }

		public string PalletID { get; set; }

		public string Row { get; set; }

		#endregion

		#region ProductPK

		public Guid ProductPK { get; set; }

		#endregion

		#region BOMProductPK

		public Guid BOMProductPK { get; set; }

		#endregion

		#region Part Attributes

		public string Attribute1 { get; set; }

		public string Attribute2 { get; set; }

		public string Attribute3 { get; set; }

		public string SerialNumber { get; set; }

		public DateTime ExpiryDate { get; set; }

		public DateTime PackingDate { get; set; }

		#endregion

		#region PartAttributes Info

		public WhsProductPartAttributesInfo PartAttributes
		{
			get
			{
				WhsProductPartAttributesInfo result = null;
				if (Parent != null)
				{
					result = Parent.ProductPartAttributesInfos.FirstOrDefault(pp => pp.ProductPK == ProductPK && pp.ClientPK == ClientPK) ?? new WhsProductPartAttributesInfo();
				}
				return result;
			}
		}

		#endregion

		#region Units

		public decimal Units { get; set; }

		#endregion

		#region UnitsUQ

		public string UnitsUQ { get; set; }

		#endregion

		#region OrderedPartAttributes

		public string OrderedPartAttribute1 { get; set; }

		public string OrderedPartAttribute2 { get; set; }

		public string OrderedPartAttribute3 { get; set; }

		public string OrderedSerialNumber { get; set; }

		public DateTime OrderedExpiryDate { get; set; }

		public DateTime OrderedPackingDate { get; set; }

		#endregion

		#region AttachedToPick

		public string AttachedToPick { get; set; }

		#endregion

		#region AttachedToOrder

		public string AttachedToOrder { get; set; }

		#endregion

		#region PackageID

		public string PackageID { get; set; }

		#endregion

		#region SlotNumber

		public short SlotNumber { get; set; }

		#endregion

		#endregion

		#region Flags

		public bool IsLocationEmptyAfterPicking { get; set; }

		public bool IsSingleProductInLocation { get; set; }

		public bool IsVerifiedNonEmpty { get; set; }

		public bool PalletIDNeutral { get; set; }

		#endregion
	}
}

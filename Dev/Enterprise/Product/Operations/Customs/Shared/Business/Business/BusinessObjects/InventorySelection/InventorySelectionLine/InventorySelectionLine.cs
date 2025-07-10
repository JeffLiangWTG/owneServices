using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class InventorySelectionLine : AutoInventorySelectionLine
	{
		public InventorySelectionLine(InventorySelectionHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		readonly InventorySelectionHeader header;

		public void UpdateSelectionLinesDetails(params WhsInventoryWrapper[] inventoryWrappers)
		{
			var productList = new List<ZString>();
			var productDescriptionList = new List<ZString>();
			us_Attribute1 = ZString.Empty;
			us_Attribute2 = ZString.Empty;
			us_Attribute3 = ZString.Empty;
			us_SerialNumber = ZString.Empty;
			us_GroupingID = ZString.Empty;
			var customsEntryKey = ZString.Empty;
			us_CartonQtyOnHand = ZInt.Zero;
			us_OriginalBondedQty = ZDecimal.Zero;
			us_ProductQtyOnHand = ZDecimal.Zero;
			us_ProductQtyPerCarton = ZDecimal.Zero;
			us_Warehouse = ZString.Empty;
			us_ArrivalDate = ZDateTime.Empty;
			var isGroupByInventory = header.IsGroupByInventory;
			var isGroupByProduct = header.IsGroupByProduct;
			var isGroupByCarton = header.IsGroupByCarton;
			this.InventoryWrappers = inventoryWrappers.OrderBy(x => GetOrderByKeyArrivalDate(x)).ToArray();
			if (InventoryWrappers.Length > 0)
			{
				customsEntryKey = GetCustomsEntryKey(isGroupByInventory, isGroupByCarton);
				if (isGroupByCarton)
				{
					UpdateSelectionLinesDetailsWithCartonData(productList, productDescriptionList);
					us_Attribute1 = ZString.Empty;
					us_Attribute2 = ZString.Empty;
					us_Attribute3 = ZString.Empty;
					us_SerialNumber = ZString.Empty;
				}
				else
				{
					UpdateSelectionLinesDetailsWithProductData(productList, productDescriptionList);
					if (isGroupByInventory)
					{
						var inventoryWrapper = inventoryWrappers.FirstOrDefault();
						if (inventoryWrapper != null)
						{
							us_ProductQtyPerCarton = new ZDecimal(inventoryWrapper.PerPackageQty);
						}
					}
				}
				UpdateDataFromInventories(ref us_Warehouse, ref us_ArrivalDate, ref us_OriginalPackType, ref us_GroupingID);
				if (isGroupByProduct)
				{
					us_ArrivalDate = ZDateTime.Empty;
					us_GroupingID = ZString.Empty;
				}
			}
			Update(US_ProductInfo, ref us_Product, new ZStringBuilder(productList).ToStringWithDelimiterBetweenAppends(", "));
			Update(US_Attribute1Info, ref us_Attribute1, us_Attribute1);
			Update(US_Attribute2Info, ref us_Attribute2, us_Attribute2);
			Update(US_Attribute3Info, ref us_Attribute3, us_Attribute3);
			Update(US_SerialNumberInfo, ref us_SerialNumber, us_SerialNumber);
			Update(US_GroupingIDInfo, ref us_GroupingID, us_GroupingID);
			Update(US_DescriptionInfo, ref us_Description, new ZStringBuilder(productDescriptionList).ToStringWithDelimiterBetweenAppends(", "));
			Update(US_ArrivalDateInfo, ref us_ArrivalDate, us_ArrivalDate);
			Update(US_CustomsEntryKeyInfo, ref us_CustomsEntryKey, customsEntryKey);
			Update(US_CartonQtyOnHandInfo, ref us_CartonQtyOnHand, us_CartonQtyOnHand);
			Update(US_OriginalBondedQtyInfo, ref us_OriginalBondedQty, us_OriginalBondedQty);
			Update(US_ProductQtyOnHandInfo, ref us_ProductQtyOnHand, us_ProductQtyOnHand);
			Update(US_WarehouseInfo, ref us_Warehouse, us_Warehouse);
		}

		public override ZString US_Product => us_Product;
		ZString us_Product;

		public override ZString US_Attribute1 => us_Attribute1;
		ZString us_Attribute1;

		public override ZString US_Attribute2 => us_Attribute2;
		ZString us_Attribute2;

		public override ZString US_Attribute3 => us_Attribute3;
		ZString us_Attribute3;

		public override ZString US_SerialNumber => us_SerialNumber;
		ZString us_SerialNumber;

		public override ZString US_GroupingID => us_GroupingID;
		ZString us_GroupingID;

		public override ZString US_Description => us_Description;
		ZString us_Description;

		public override ZDateTime US_ArrivalDate => us_ArrivalDate;
		ZDateTime us_ArrivalDate;

		public override ZString US_CustomsEntryKey => us_CustomsEntryKey;
		ZString us_CustomsEntryKey;

		public override ZInt US_CartonQtyOnHand => us_CartonQtyOnHand;
		ZInt us_CartonQtyOnHand;

		public override ZDecimal US_OriginalBondedQty => us_OriginalBondedQty;
		ZDecimal us_OriginalBondedQty;

		public override ZDecimal US_ProductQtyOnHand => us_ProductQtyOnHand;
		ZDecimal us_ProductQtyOnHand;

		public override ZString US_DeclarantsReference => ZString.Empty;
		public override ZDateTime US_CustomsDeadline => ZDateTime.Empty;

		public override ZInt US_CartonQtytoDraw
		{
			get => base.US_CartonQtytoDraw;
			set
			{
				var oldValue = US_CartonQtytoDraw;
				validateUS_CartonQtytoDrawCalled = false;
				base.US_CartonQtytoDraw = value;
				if (!IsCopying && oldValue != US_CartonQtytoDraw)
				{
					if (!settingDrawQtyInProgress)
					{
						try
						{
							settingDrawQtyInProgress = true;
							US_ProductQtyToDraw = !header.IsGroupByProduct ? CalculateProductQtyFromCartonQty() : ZDecimal.Zero;
						}
						finally
						{
							settingDrawQtyInProgress = false;
						}
					}
				}
			}
		}

		public override ZDecimal US_ProductQtyToDraw
		{
			get => base.US_ProductQtyToDraw;
			set
			{
				validateUS_ProductQtyToDrawCalled = false;
				var oldValue = US_ProductQtyToDraw;
				using (GetValidationSuspender())
				{
					base.US_ProductQtyToDraw = value;
					if (!IsCopying && oldValue != US_ProductQtyToDraw)
					{
						if (!settingDrawQtyInProgress)
						{
							try
							{
								settingDrawQtyInProgress = true;
								US_CartonQtytoDraw = !header.IsGroupByProduct ? CalculateCartonQtyFromProductQty() : ZInt.Zero;
								if (US_CartonQtytoDraw > ZDecimal.Zero)
								{
									header.UpdateRelatedProductQty(this, US_CartonQtytoDraw);
								}
							}
							finally
							{
								settingDrawQtyInProgress = false;
							}
						}
						UpdateQuantityOnWrapper();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateUS_ProductQtyToDraw();
				}
			}
		}
		bool settingDrawQtyInProgress;

		public override ZDecimal US_ProductQtyPerCarton => us_ProductQtyPerCarton;
		ZDecimal us_ProductQtyPerCarton;

		public override ZString US_Warehouse => us_Warehouse;
		ZString us_Warehouse;

		public override ZString US_OriginalPackType => us_OriginalPackType;
		ZString us_OriginalPackType;

		public void ClearInventoryDetail()
		{
			InventoryWrappers = EmptyWhsInventoryWrapperArray;
			us_CustomsEntryKey = ZString.Empty;
			us_Product = ZString.Empty;
			us_Description = ZString.Empty;
			us_ArrivalDate = ZDateTime.Empty;
			us_CartonQtyOnHand = ZInt.Zero;
			US_CartonQtytoDraw = ZInt.Zero;
			us_OriginalBondedQty = ZDecimal.Zero;
			US_ProductQtyToDraw = ZDecimal.Zero;
			us_ProductQtyOnHand = ZDecimal.Zero;
			us_ProductQtyPerCarton = ZDecimal.Zero;
			us_Warehouse = ZString.Empty;
			us_Attribute1 = ZString.Empty;
			us_Attribute2 = ZString.Empty;
			us_Attribute3 = ZString.Empty;
			us_SerialNumber = ZString.Empty;
			us_GroupingID = ZString.Empty;
			US_CartonQtytoDrawInfo.ClearAllNotifications();
			US_ProductQtyOnHandInfo.ClearAllNotifications();
		}

		public override void ValidateUS_CartonQtytoDraw()
		{
			if (!validateUS_CartonQtytoDrawCalled)
			{
				validateUS_CartonQtytoDrawCalled = true;
				base.ValidateUS_CartonQtytoDraw();
				if (header.IsGroupByCarton)
				{
					CompareValidation.CheckNumberNotNegative(US_CartonQtytoDrawInfo);
					if (US_CartonQtytoDraw > US_CartonQtyOnHand)
					{
						US_CartonQtytoDrawInfo.AddError(CartonQtyToDrawIsGreaterThanOnHand);
					}
				}
				ValidateUS_ProductQtyToDraw();
			}
		}
		bool validateUS_CartonQtytoDrawCalled;

		public sealed override void ValidateUS_ProductQtyToDraw()
		{
			if (!validateUS_ProductQtyToDrawCalled)
			{
				validateUS_ProductQtyToDrawCalled = true;
				base.ValidateUS_ProductQtyToDraw();
				ValidateUS_ProductQtyToDrawCore();
				ValidateUS_CartonQtytoDraw();
			}
		}

		bool validateUS_ProductQtyToDrawCalled;

		protected virtual void ValidateUS_ProductQtyToDrawCore()
		{
			CompareValidation.CheckNumberNotNegative(US_ProductQtyToDrawInfo);
			if (US_ProductQtyToDraw > US_ProductQtyOnHand)
			{
				US_ProductQtyToDrawInfo.AddError(ProductQtyToDrawIsGreaterThanOnHand);
			}
			else if (US_ProductQtyToDraw > ZDecimal.Zero && US_ProductQtyPerCarton > ZDecimal.Zero)
			{
				long remainder;
				Math.DivRem((long)US_ProductQtyToDraw, (long)US_ProductQtyPerCarton, out remainder);
				if (remainder > 0)
				{
					US_ProductQtyToDrawInfo.AddError(ProductQtyToDrawIncorrectMultiple(US_ProductQtyPerCarton));
				}
			}
			if (US_ProductQtyToDraw > ZDecimal.Zero && !US_ProductQtyToDrawInfo.HasErrors() && !header.AllowWithdrawalOfMultipleEntryDetails)
			{
				CheckMultipleEntryDetails();
			}
		}

		public static string ProductQtyToDrawIncorrectMultiple(ZDecimal multiple)
		{
			return Res.GetString("9BB16C5C-7725-417B-A95B-A5223DCC0C91", "Product Qty to draw must be a multiple of {0}.", multiple.ToStringTrimZeros());
		}

		public static string CartonQtyToDrawIsGreaterThanOnHand
		{
			get { return Res.GetString("2A386581-4D22-47DD-9153-CF36191C0FEE", "Please enter a Carton Qty less or equal to what's available on hand."); }
		}

		public static string ProductQtyToDrawIsGreaterThanOnHand
		{
			get { return Res.GetString("4477E5FC-F0AB-412E-951B-BFA735097639", "Please enter a Product Qty less or equal to what's available on hand."); }
		}

		public static string CannotWithdrawProductHavingDifferentEntryNumber
		{
			get { return Res.GetString("F5EDB386-37EB-4B72-837B-A4206F9C4D2B", "Cannot withdraw Product having different Entry Number."); }
		}

		public bool HasDrawQty
		{
			get { return US_ProductQtyToDraw > ZInt.Zero; }
		}

		#region Implementation

		void UpdateQuantityOnWrapper()
		{
			var quantity = US_ProductQtyToDraw;
			if (quantity > US_ProductQtyOnHand)
			{
				quantity = ZDecimal.Zero;
			}
			else if (quantity > ZDecimal.Zero && US_ProductQtyPerCarton > ZDecimal.Zero)
			{
				long remainder;
				Math.DivRem((long)quantity, (long)US_ProductQtyPerCarton, out remainder);
				if (remainder > 0)
				{
					quantity = ZDecimal.Zero;
				}
			}
			if (header.IsGroupByProduct)
			{
				UpdateQuantityOnWrapperBasedOnProduct(quantity);
			}
			else
			{
				UpdateQuantityOnWrapperBasedOnGroupByInventoryOrCarton(quantity);
			}
		}

		void UpdateQuantityOnWrapperBasedOnProduct(ZDecimal quantity)
		{
			var inventoryWrappersInArrivalDateOrder = new List<WhsInventoryWrapper>(InventoryWrappers);
			foreach (var inventoryWrapper in inventoryWrappersInArrivalDateOrder.ToArray())
			{
				var packingGroupKey = inventoryWrapper.PackingGroupKey;
				if (packingGroupKey.IsEmpty)
				{
					quantity = UpdateQuantityOnWrapperBasedOnProduct_EmptyPackingGroup(quantity, inventoryWrappersInArrivalDateOrder, inventoryWrapper);
				}
				else if (inventoryWrappersInArrivalDateOrder.Contains(inventoryWrapper))
				{
					quantity = UpdateQuantityOnWrapperBasedOnProduct_WithPackingGroup(quantity, inventoryWrappersInArrivalDateOrder, inventoryWrapper, packingGroupKey);
				}
				if (quantity.IsEmpty)
				{
					ClearQuantityOnWrapper(inventoryWrappersInArrivalDateOrder);
					break;
				}
			}
		}

		ZDecimal UpdateQuantityOnWrapperBasedOnProduct_WithPackingGroup(ZDecimal quantity, List<WhsInventoryWrapper> inventoryWrappersInArrivalDateOrder, WhsInventoryWrapper inventoryWrapper, ZString packingGroupKey)
		{
			var inventoryWrappersWithSamePackingGroupKey = inventoryWrappersInArrivalDateOrder.Where(x => x.PackingGroupKey == packingGroupKey).ToList();
			var availableQuantity = (ZDecimal)inventoryWrappersWithSamePackingGroupKey.Sum(x => x.QuantityOnHand);
			if (availableQuantity > ZDecimal.Zero)
			{
				if (availableQuantity < quantity)
				{
					header.UpdateInventoriesPerCartonQuantityMatching(packingGroupKey, new ZDecimal(Math.Ceiling(inventoryWrapper.QuantityOnHand / inventoryWrapper.PerPackageQty)).ToZInt());
					quantity -= availableQuantity;
				}
				else
				{
					var cartonQtyToDraw = ZInt.Zero;
					if (quantity > ZDecimal.Zero)
					{
						cartonQtyToDraw = new ZDecimal(Math.Ceiling(quantity / inventoryWrapper.PerPackageQty)).ToZInt();
					}
					header.UpdateInventoriesPerCartonQuantityMatching(packingGroupKey, cartonQtyToDraw);
					quantity = ZDecimal.Zero;
				}
			}
			else
			{
				header.UpdateInventoriesPerCartonQuantityMatching(packingGroupKey, ZInt.Zero);
			}
			inventoryWrappersWithSamePackingGroupKey.ForEach(x => inventoryWrappersInArrivalDateOrder.Remove(x));
			return quantity;
		}

		ZDecimal UpdateQuantityOnWrapperBasedOnProduct_EmptyPackingGroup(ZDecimal quantity, List<WhsInventoryWrapper> inventoryWrappersInArrivalDateOrder, WhsInventoryWrapper inventoryWrapper)
		{
			var availableQuantity = inventoryWrapper.QuantityOnHand;
			if (availableQuantity > ZDecimal.Zero)
			{
				if (availableQuantity < quantity)
				{
					inventoryWrapper.QuantityToDraw = availableQuantity;
					quantity -= availableQuantity;
				}
				else
				{
					inventoryWrapper.QuantityToDraw = quantity;
					quantity = ZDecimal.Zero;
				}
			}
			else
			{
				inventoryWrapper.QuantityToDraw = ZDecimal.Zero;
			}
			inventoryWrappersInArrivalDateOrder.Remove(inventoryWrapper);
			return quantity;
		}

		void ClearQuantityOnWrapper(List<WhsInventoryWrapper> inventoryWrappers)
		{
			foreach (var inventoryWrapper in inventoryWrappers.ToArray())
			{
				var packingGroupKey = inventoryWrapper.PackingGroupKey;
				if (packingGroupKey.IsEmpty)
				{
					inventoryWrapper.QuantityToDraw = ZDecimal.Zero;
				}
				else
				{
					header.UpdateInventoriesPerCartonQuantityMatching(packingGroupKey, ZInt.Zero);
				}
			}
		}

		void UpdateQuantityOnWrapperBasedOnGroupByInventoryOrCarton(ZDecimal quantity)
		{
			var inventoryWrapper = InventoryWrappers.FirstOrDefault();
			if (inventoryWrapper != null)
			{
				var packingGroupKey = inventoryWrapper.PackingGroupKey;
				if (packingGroupKey.IsEmpty)
				{
					inventoryWrapper.QuantityToDraw = quantity;
				}
				else
				{
					header.UpdateInventoriesPerCartonQuantityMatching(packingGroupKey, US_CartonQtytoDraw);
				}
			}
		}

		ZString GetOrderByKeyArrivalDate(WhsInventoryWrapper inventoryWrapper)
		{
			var keyBuilder = new ZStringBuilder();
			keyBuilder.Append(inventoryWrapper.ArrivalDate.IsValid ? inventoryWrapper.ArrivalDate.ToString(DateFormat) : SpaceFillers12Length);
			keyBuilder.Append(inventoryWrapper.CustomsEntryKey);
			keyBuilder.Append(inventoryWrapper.ProductGroupKey);
			keyBuilder.Append(inventoryWrapper.PackingGroupKey.ToUpper());
			keyBuilder.Append(inventoryWrapper.LineNoKey);
			return keyBuilder.ToString();
		}
		const string SpaceFillers12Length = "            ";
		const string DateFormat = "yyyyMMddhhmmssnnnn";

		void CheckMultipleEntryDetails()
		{
			var isGroupByCarton = header.IsGroupByCarton;
			var firstInventoryWrapper = InventoryWrappers.FirstOrDefault();
			var currentEntryNumber = firstInventoryWrapper == null ? ZString.Empty : new EntryLineCodeParser(firstInventoryWrapper.CustomsEntryKey).EntryNumber;
			foreach (WhsInventoryWrapper otherLine in header.SelectedLines)
			{
				var otherLineEntryNumber = new EntryLineCodeParser(otherLine.CustomsEntryKey).EntryNumber;
				if (currentEntryNumber != otherLineEntryNumber)
				{
					US_ProductQtyToDrawInfo.AddError(header.GetCannotWithdrawProductHavingDifferentEntryNumber());
					break;
				}
			}
		}

		ZInt CalculateCartonQtyFromProductQty()
		{
			var result = ZInt.Zero;
			if (US_ProductQtyPerCarton != ZDecimal.Zero)
			{
				result = new ZDecimal(US_ProductQtyToDraw / US_ProductQtyPerCarton).ToZInt();
			}
			return result;
		}

		ZDecimal CalculateProductQtyFromCartonQty()
		{
			return US_CartonQtytoDraw * US_ProductQtyPerCarton;
		}

		void UpdateSelectionLinesDetailsWithProductData(List<ZString> productList, List<ZString> productDescriptionList)
		{
			foreach (var inventoryWrapper in InventoryWrappers)
			{
				var part = inventoryWrapper.Part;
				if (part != null)
				{
					us_ProductQtyOnHand += inventoryWrapper.QuantityOnHand;
					if (productList.Count == 0)
					{
						productList.Add(part.OP_PartNum);
						productDescriptionList.Add(part.OP_Desc);
						us_Attribute1 = inventoryWrapper.Inventory.WI_PartAttrib1;
						us_Attribute2 = inventoryWrapper.Inventory.WI_PartAttrib2;
						us_Attribute3 = inventoryWrapper.Inventory.WI_PartAttrib3;
						us_SerialNumber = inventoryWrapper.Inventory.WI_SerialNumber;
					}
				}
				us_OriginalBondedQty += inventoryWrapper.OriginalBondedQty;
			}
		}

		void UpdateSelectionLinesDetailsWithCartonData(List<ZString> productList, List<ZString> productDescriptionList)
		{
			foreach (var inventoryWrapper in InventoryWrappers)
			{
				var part = inventoryWrapper.Part;
				if (part != null)
				{
					us_ProductQtyOnHand += inventoryWrapper.QuantityOnHand;
					GatherGroupByCartonData(productList, productDescriptionList, inventoryWrapper, part, part.OP_PartNum);
				}
				us_OriginalBondedQty += inventoryWrapper.OriginalBondedQty;
			}
			var wrapper = InventoryWrappers.FirstOrDefault();
			if (wrapper != null)
			{
				us_CartonQtyOnHand = new ZDecimal(Math.Ceiling(wrapper.CalculatedQuantityOnHand / wrapper.PerPackageQty)).ToZInt();
				if (us_CartonQtyOnHand > 0)
				{
					us_ProductQtyPerCarton = us_OriginalBondedQty / us_CartonQtyOnHand;
				}
			}
		}

		void UpdateDataFromInventories(ref ZString warehouse, ref ZDateTime arrivalDate, ref ZString originalPackType, ref ZString groupingID)
		{
			warehouse = ZString.Empty;
			var inventoryWrapper = InventoryWrappers.FirstOrDefault();
			if (inventoryWrapper != null)
			{
				warehouse = inventoryWrapper.WarehouseName;
				arrivalDate = inventoryWrapper.ArrivalDate;
				originalPackType = inventoryWrapper.OriginalPackType;
				groupingID = inventoryWrapper.ReceiveLine?.WE_PackageGroupId ?? ZString.Empty;
			}
		}

		internal protected OrgAddress GetWarehouseAddress()
		{
			OrgAddress result = null;
			var inventoryWrapper = InventoryWrappers.FirstOrDefault();
			if (inventoryWrapper != null)
			{
				result = inventoryWrapper.WarehouseAddress;
			}
			return result;
		}

		void GatherGroupByCartonData(List<ZString> productList, List<ZString> productDescriptionList, WhsInventoryWrapper inventoryWrapper, OrgSupplierPart part, ZString partNo)
		{
			var perPkgQty = inventoryWrapper.PerPackageQty;
			productList.Add(string.Format((NoResString)"{0} ({1} Per)", partNo, perPkgQty));
			productDescriptionList.Add(string.Format((NoResString)"{0} ({1} Per)", part.OP_Desc, perPkgQty));
		}

		ZString GetCustomsEntryKey(bool isGroupByInventory, bool isGroupByCarton)
		{
			var customsEntryKey = ZString.Empty;
			if (isGroupByInventory)
			{
				var customsEntryKeys = InventoryWrappers.Select((WhsInventoryWrapper x) => x.Inventory.WI_BondedEntryKey).Distinct().Take(2).ToArray();
				if (customsEntryKeys.Length > 0)
				{
					customsEntryKey = customsEntryKeys[0];
				}
				if (customsEntryKeys.Length > 1)
				{
					ErrorReporter.ReportOnce("2B52AA39-9818-4764-899D-0B97616A0F25", "Multiple Inventories when group by Inventory");
				}
			}
			else if (isGroupByCarton)
			{
				var inventoryWrapper = InventoryWrappers.FirstOrDefault();
				if (inventoryWrapper != null)
				{
					customsEntryKey = new EntryLineCodeParser(inventoryWrapper.Inventory.WI_BondedEntryKey).EntryNumber;
				}
			}
			return customsEntryKey;
		}

		void Update<T>(ZPropertyInfo info, ref T variable, T newValue)
			where T : IZType
		{
			variable = newValue;
			info.RefreshBinding();
		}

		public WhsInventoryWrapper[] InventoryWrappers
		{
			get => inventoryWrappers ?? (inventoryWrappers = EmptyWhsInventoryWrapperArray);
			private set => inventoryWrappers = value ?? EmptyWhsInventoryWrapperArray;
		}
		WhsInventoryWrapper[] inventoryWrappers;

		WhsInventoryWrapper[] EmptyWhsInventoryWrapperArray => emptyWhsInventoryWrapperArray ?? (emptyWhsInventoryWrapperArray = Array.Empty<WhsInventoryWrapper>());
		WhsInventoryWrapper[] emptyWhsInventoryWrapperArray;

		#endregion
	}
}

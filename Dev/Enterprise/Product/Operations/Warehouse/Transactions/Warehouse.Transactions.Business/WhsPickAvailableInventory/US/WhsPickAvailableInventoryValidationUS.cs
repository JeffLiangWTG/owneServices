using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	public class WhsPickAvailableInventoryValidationUS : WhsPickAvailableInventoryValidation
	{
		public WhsPickAvailableInventoryValidationUS(WhsPickAvailableInventory parent)
			: base(parent)
		{
		}

		#region CheckPickLineQuantity

		protected override void CheckPickLineQuantity()
		{
			base.CheckPickLineQuantity();
			Helper.CheckUnitsIsDivisibleByPerPackageQty(Parent.PickLineQuantityInfo);
			CheckPickLineQuantity_PickedFullPackages();
		}

		void CheckPickLineQuantity_PickedFullPackages()
		{
			var packageGroupID = Parent.PackageGroupId;
			if (!Parent.PickLineQuantityInfo.HasErrors() && !packageGroupID.IsEmpty && Parent.PickLineQuantity > 0m)
			{
				var pick = Parent.OrderedInventory.Pick;
				var locationKey = GetLocationKey(Parent);
				var packageGroupContent = USBondedHelper.GetDistinctPackageGroupIDInventories(Parent.Factory, pick.Warehouse, packageGroupID);
				var pickedPackageGroupContent = GetPickedPackageGroupContent(pick, packageGroupID, locationKey);
				var firstPackageCount = pickedPackageGroupContent.Values.FirstOrDefault();

				if (packageGroupContent.Count != pickedPackageGroupContent.Count || pickedPackageGroupContent.Any(g => !packageGroupContent.ContainsKey(g.Key) || pickedPackageGroupContent[g.Key] != firstPackageCount))
				{
					Parent.PickLineQuantityInfo.AddError(Res.GetString("3720c37c-7aed-4fe7-b2ab-1281db9a5b6f",
						"Allocating this inventory would split products in Package Group ID '{0}'. You must allocate the same whole Package Count for all other products with this Package Group ID from the same location.", packageGroupID));
				}
			}
		}

		#region GetLocationKey

		ZString GetLocationKey(WhsPickAvailableInventory availableInventory)
		{
			var locationKeyBuilder = new ZStringBuilder();
			locationKeyBuilder.Append(availableInventory.Location?.WLV_LocationString ?? ZString.Empty);
			locationKeyBuilder.Append(availableInventory.PalletID);
			locationKeyBuilder.Append(availableInventory.InventoryStatus);

			return locationKeyBuilder.ToStringWithDelimiterBetweenAppends("~");
		}

		#endregion

		#region GetPickedPackageGroupContent

		Dictionary<ProductWithAttributes, ZInt> GetPickedPackageGroupContent(WhsPick pick, ZString packageGroupID, ZString locationKey)
		{
			var result = new Dictionary<ProductWithAttributes, ZInt>();
			foreach (WhsPickOrderedInventory orderedInventory in pick.OrderedInventories)
			{
				foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
				{
					if (availableInventory.PickLineQuantity > 0m && availableInventory.PackageGroupId == packageGroupID && locationKey == GetLocationKey(availableInventory) && availableInventory.PerPackageQty > 0)
					{
						var productKey = ProductWithAttributes.GetProductWithAttributes(availableInventory);
						if (!result.ContainsKey(productKey))
						{
							result.Add(productKey, 0);
						}
						result[productKey] += (ZInt)(availableInventory.PickLineQuantity / availableInventory.PerPackageQty); // should always be Int
					}
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region AutoValidationType

		public override Type AutoValidationType
		{
			get { return typeof(WhsPickAvailableInventoryValidationUS); }
		}

		#endregion

		#region Implementations

		WhsPickAvailableInventoryValidationHelper Helper
		{
			get { return helper ?? (helper = new WhsPickAvailableInventoryValidationHelper(Parent)); }
		}

		WhsPickAvailableInventoryValidationHelper helper;

		#endregion
	}
}

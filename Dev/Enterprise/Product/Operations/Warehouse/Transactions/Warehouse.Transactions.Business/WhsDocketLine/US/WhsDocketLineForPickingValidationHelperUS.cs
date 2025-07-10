using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.US
{
	// tested in US WhsTransferLineValidation && AdjustmentLineValidation && WhsOrderLineValidationUS
	public abstract class WhsDocketLineForPickingValidationHelperUS<T> : WhsDocketLineValidationHelperUS<T>
		where T : WhsDocketLine
	{
		protected WhsDocketLineForPickingValidationHelperUS(T docketLine)
			: base(docketLine)
		{
		}

		#region CheckOnlyFullPackagesAreOrderedToBePicked

		public void CheckOnlyFullPackagesAreOrderedToBePicked(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && Docket != null && !Docket.IsFinalisedOrCancelled && !PackageGroupID.IsEmpty && PackageGroupContent.Count > 0)
			{
				var lines = GetRelatedPickingLines();
				if (lines.Length > 0)
				{
					CheckPickedOrPutawayInFullPackages(propertyInfo, lines, OnlyFullPackagesCanBeOrderedToBePickedErrorMessage);
				}
			}
		}

		protected abstract T[] GetRelatedPickingLines();

		protected abstract bool IsMatching(T line, ProductWithAttributes packedItem);

		protected abstract string LineDoesNotMatchAnyInventoryPackedIntoPackageGroupErrorMessage { get; }

		protected abstract string OnlyFullPackagesCanBeOrderedToBePickedErrorMessage { get; }

		#endregion

		#region CheckOnlyFullPackagesArePutawayIntoDestLocation

		public void CheckOnlyFullPackagesArePutawayIntoDestLocation(ZPropertyInfo propertyInfo)
		{
			if (!propertyInfo.HasErrors() && Docket != null && !Docket.IsFinalisedOrCancelled && !PackageGroupID.IsEmpty)
			{
				var lines = GetRelatedPutawayLines();
				if (lines.Length > 0)
				{
					CheckPickedOrPutawayInFullPackages(propertyInfo, lines, OnlyFullPackagesCanBePutawayInDestinationLocationErrorMessage);
				}
			}
		}

		protected abstract T[] GetRelatedPutawayLines();

		protected abstract string OnlyFullPackagesCanBePutawayInDestinationLocationErrorMessage { get; }

		#endregion

		#region CheckPickedOrPutawayInFullPackages

		void CheckPickedOrPutawayInFullPackages(ZPropertyInfo propertyInfo, T[] similarLines, string errorMessage)
		{
			var itemsToPickDictionary = GetItemsToPick(propertyInfo, similarLines);
			var first = itemsToPickDictionary.FirstOrDefault();
			if ((PackageGroupContent.Count != 0 && itemsToPickDictionary.Count != PackageGroupContent.Count) || itemsToPickDictionary.Any(i => i.Value != first.Value))
			{
				propertyInfo.AddError(errorMessage);
			}
		}

		Dictionary<ProductWithAttributes, ZInt> GetItemsToPick(ZPropertyInfo propertyInfo, T[] similarLines)
		{
			var orderedItemsDictionary = new Dictionary<ProductWithAttributes, ZInt>();
			if (PackageGroupContent.Count > 0)
			{
				PopulateExistingPackagesContent(orderedItemsDictionary, similarLines, propertyInfo);
			}
			else
			{
				PopulateNewPackagesContent(orderedItemsDictionary, similarLines, propertyInfo);
			}
			return orderedItemsDictionary;
		}

		#region PopulateExistingPackagesContent

		void PopulateExistingPackagesContent(Dictionary<ProductWithAttributes, ZInt> orderedItemsDictionary, T[] similarLines, ZPropertyInfo propertyInfo)
		{
			foreach (var line in similarLines)
			{
				var matchingPackedItems = PackageGroupContent.Where(i => IsMatching(line, i.Key));
				var perPackageQty = matchingPackedItems.Sum(i => i.Value);
				if (perPackageQty != 0 && line.WE_TransactionQuantity % perPackageQty == 0)
				{
					var packages = (int)(line.WE_TransactionQuantity / perPackageQty); // should always be int value
					foreach (var packedItem in matchingPackedItems)
					{
						if (!orderedItemsDictionary.ContainsKey(packedItem.Key))
						{
							orderedItemsDictionary.Add(packedItem.Key, 0);
						}
						orderedItemsDictionary[packedItem.Key] += packages;
					}
				}
				else if (line == LineAttributes)
				{
					propertyInfo.AddError(LineDoesNotMatchAnyInventoryPackedIntoPackageGroupErrorMessage);
					break;
				}
			}
		}

		#endregion

		#region PopulateNewPackagesContent

		void PopulateNewPackagesContent(Dictionary<ProductWithAttributes, ZInt> orderedItemsDictionary, T[] similarLines, ZPropertyInfo propertyInfo)
		{
			foreach (var line in similarLines)
			{
				if (line.WE_PerPackageQty > 0m)
				{
					var packedItem = ProductWithAttributes.GetProductWithAttributes(line);
					var packages = (int)(line.WE_TransactionQuantity / line.WE_PerPackageQty); // should always be int value
					if (!orderedItemsDictionary.ContainsKey(packedItem))
					{
						orderedItemsDictionary.Add(packedItem, 0);
					}
					orderedItemsDictionary[packedItem] += packages;
				}
				else if (line == LineAttributes)
				{
					propertyInfo.AddError(PerPackageQtyMustBeSpecifiedIfPackageGroupIDIsSpecified);
					break;
				}
			}
		}

		#endregion

		#endregion

		#region PackageGroupContent

		protected Dictionary<ProductWithAttributes, ZDecimal> PackageGroupContent
		{
			get { return packageGroupContent ?? (packageGroupContent = USBondedHelper.GetPackageGroupIDContent(LineAttributes.Factory, Docket.Warehouse, PackageGroupID)); }
		}

		Dictionary<ProductWithAttributes, ZDecimal> packageGroupContent;

		#endregion
	}
}

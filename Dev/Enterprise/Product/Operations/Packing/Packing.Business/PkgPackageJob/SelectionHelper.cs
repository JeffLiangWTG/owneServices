using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business
{
	public class SelectionHelper
	{
		public SelectionHelper(PkgPackageJob packageJob)
		{
			PackageJob = packageJob;
		}

		readonly PkgPackageJob PackageJob;

		// flags

		#region IsNothingSelected

		public bool IsNothingSelected
		{
			get { return !SelectedPackagesNonReadOnly.Any() && !SelectedPackedItemsNonReadOnly.Any(); }
		}

		#endregion

		#region IsPackageSelected

		public bool IsPackageSelected
		{
			get { return SelectedPackagesNonReadOnly.Any(); }
		}

		#endregion

		#region IsPackedItemSelected

		public bool IsPackedItemSelected
		{
			get { return SelectedPackedItemsNonReadOnly.Any(); }
		}

		#endregion

		#region IsSinglePackageSelected

		public bool IsSinglePackageSelected
		{
			get { return SelectedPackagesNonReadOnly.Count == 1; }
		}

		#endregion

		#region IsSinglePackedItemSelected

		public bool IsSinglePackedItemSelected
		{
			get { return SelectedPackedItemsNonReadOnly.Count == 1; }
		}

		#endregion

		// selections

		#region SelectedPackage

		public PkgPackage SelectedPackage
		{
			get { return SelectedPackagesNonReadOnly.SingleOrDefault(); }
		}

		#endregion

		#region SelectedPackages

		/// <summary>
		/// Do not add packages that are not part of this PackageJob hierarchy.
		/// </summary>
		public void UpdateSelectedPackages(IEnumerable<PkgPackage> packages)
		{
			Argument.NotNull(packages, "packages");

			foreach (var package in packages)
			{
				if (!PackageJob.ContainsPackage(package))
				{
					throw new InvalidOperationException(
						string.Format(CultureInfo.InvariantCulture, "Attempting to Select a package that is not part of this package job. PackageJob PK is {0}, Package FK to PackageJob is {1}.",
						PackageJob.PK, package.KP_KJ_ParentPackageJob));
				}
			}

			SelectedPackagesNonReadOnly.Clear();
			SelectedPackagesNonReadOnly.AddRange(packages);
		}

		public ReadOnlyCollection<PkgPackage> SelectedPackages
		{
			get { return SelectedPackagesNonReadOnly.AsReadOnly(); }
		}

		List<PkgPackage> SelectedPackagesNonReadOnly
		{
			get { return selectedPackagesNonReadOnly ?? (selectedPackagesNonReadOnly = new List<PkgPackage>()); }
		}

		List<PkgPackage> selectedPackagesNonReadOnly;

		#endregion

		#region SelectionParentPackage

		public PkgPackage SelectionParentPackage
		{
			get
			{
				var anySelectedPackageOrItem = (IPackageParent)SelectedPackagesNonReadOnly.FirstOrDefault() ?? SelectedPackedItemsNonReadOnly.FirstOrDefault();
				return (anySelectedPackageOrItem != null) ? anySelectedPackageOrItem.ParentPackage : null;
			}
		}

		#endregion

		#region SelectedPackedItem

		public PkgPackageItemDivotsWrapper SelectedPackedItem
		{
			get { return SelectedPackedItemsNonReadOnly.SingleOrDefault(); }
		}

		#endregion

		#region SelectedPackedItems

		/// <summary>
		/// Do not add packed items that are not part of this PackageJob hierarchy.
		/// </summary>
		public void UpdateSelectedPackedItems(IEnumerable<PkgPackageItemDivotsWrapper> packedItems)
		{
			Argument.NotNull(packedItems, nameof(packedItems));
			var packedItemsWithParentPackage = packedItems.Where(pi => pi.ParentPackage != null);
			var parentPackages = packedItemsWithParentPackage.Select(pi => pi.ParentPackage);
			foreach (var selectedPackage in parentPackages)
			{
				if (!PackageJob.ContainsPackage(selectedPackage))
				{
					throw new InvalidOperationException(
						string.Format(Culture.Invariant, "Attempting to Select a packed item that is not part of this package job. PackageJob PK is {0}, PackedItem.ParentPackage FK to PackageJob is {1}.",
						PackageJob.PK, selectedPackage.KP_KJ_ParentPackageJob));
				}
			}

			SelectedPackedItemsNonReadOnly.Clear();
			SelectedPackedItemsNonReadOnly.AddRange(packedItemsWithParentPackage);
		}

		public ReadOnlyCollection<PkgPackageItemDivotsWrapper> SelectedPackedItems
		{
			get { return SelectedPackedItemsNonReadOnly.AsReadOnly(); }
		}

		List<PkgPackageItemDivotsWrapper> SelectedPackedItemsNonReadOnly
		{
			get { return selectedPackedItemsNonReadOnly ?? (selectedPackedItemsNonReadOnly = new List<PkgPackageItemDivotsWrapper>()); }
		}

		List<PkgPackageItemDivotsWrapper> selectedPackedItemsNonReadOnly;

		#endregion

		// items to unpack

		#region ItemsToUnpackBasedOnSelected

		public ItemsToUnpack ItemsToUnpackBasedOnSelected()
		{
			ItemsToUnpack result = null;
			ZString errorMessage = "";

			// nothing selected for unpack or something closed/released?
			if (EnsurePackagesOrItemsSelectedForUnpack(out errorMessage) &&
				EnsurePackagesAreOpen(SelectedPackedItemsNonReadOnly, out errorMessage, SelectedPackagesNonReadOnly))
			{
				var packedItemsAndBarcodes = SelectedPackedItemsNonReadOnly.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, BarcodeMatch.Yes));
				result = new ItemsToUnpack(SelectedPackagesNonReadOnly, packedItemsAndBarcodes, errorMessage);
			}

			return result ?? new ItemsToUnpack(Enumerable.Empty<PkgPackage>(), Enumerable.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), errorMessage);
		}

		public IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> ItemsToUnpackBasedOnSelected(ZString barcode, out ZString errorMessage, bool isUserScanningQty = false)
		{
			IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> result = null;
			errorMessage = "";

			IEnumerable<PkgPackageItemDivotsWrapper> matchingPackedItems = null;
			var matchingWrappersOnJob = GetWrappersFromPackageJobMatchingBarcode(barcode);
			var itemInPackageTUN = matchingWrappersOnJob.FirstOrDefault(w => w.CurrentBarcodeMatch.IsTUN);

			// is this product a TUN code scan?
			if (itemInPackageTUN != null)
			{
				EnsureItemsMatchingTUNCodeHaveTheSamePackTypeAndQty(barcode, matchingWrappersOnJob);
				matchingPackedItems = GetPackedItemsToScanUnPackForTUNScan(matchingWrappersOnJob, isUserScanningQty, out errorMessage);
			}
			// non-TUN product unpack
			else if (EnsurePackagesOrItemsSelectedForUnpack(out errorMessage))
			{
				matchingPackedItems = GetPackedItemsToScanUnPackForScan(matchingWrappersOnJob, out errorMessage);
			}

			// combine packed items with barcodes
			if (matchingPackedItems != null && matchingPackedItems.Any() && EnsurePackagesAreOpen(matchingPackedItems, out errorMessage))
			{
				result = matchingPackedItems.Select(d => new PkgPackageItemDivotsWrapperAndBarcode(d, matchingWrappersOnJob.First(w => w.PackableItemParent == d.PackableItemParent).CurrentBarcodeMatch));
			}

			return result ?? Enumerable.Empty<PkgPackageItemDivotsWrapperAndBarcode>();
		}

		PackableItemParentWrapper[] GetWrappersFromPackageJobMatchingBarcode(ZString barcode)
		{
			var newWrappers = PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(w => WrapPackableItemParentWithBarcode(w, barcode));
			return newWrappers.Where(w => w.CurrentBarcodeMatch.IsMatch).ToArray();
		}

		PackableItemParentWrapper WrapPackableItemParentWithBarcode(PackableItemParentWrapper wrapperFromJob, ZString barcode)
		{
			var wrapper = new PackableItemParentWrapper(PackageJob.Factory, wrapperFromJob);
			wrapper.CurrentBarcode = barcode;

			return wrapper;
		}

		#region Non-TUN

		PkgPackageItemDivotsWrapper[] GetPackedItemsToScanUnPackForScan(IEnumerable<PackableItemParentWrapper> matchingWrappersOnJob, out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			var result = Array.Empty<PkgPackageItemDivotsWrapper>();
			bool resultIsValid = false;

			var matchingItems = new HashSet<IPackableItemParent>(matchingWrappersOnJob.Select(w => w.PackableItemParent));
			if (matchingItems.Any())
			{
				// first try to locate in the selected package
				if (IsSinglePackageSelected)
				{
					result = SelectedPackage.PackedItems.Typed.Where(d => matchingItems.Contains(d.PackableItemParent)).ToArray();
					resultIsValid = (result.Any()); // multiples are ok as they are all in the same package which means they have different attribs
				}
				else
				{
					// next try to locate the selected packable item
					result = SelectedPackedItems.Where(d => matchingItems.Contains(d.PackableItemParent)).ToArray();
					resultIsValid = result.IsOnePackedItem(); // if > 1 item is selected we do not know what to unpack
				}

				// next try to locate in the parent of the current selection
				if (!resultIsValid)
				{
					var package = SelectionParentPackage;
					if (package != null)
					{
						result = package.PackedItems.Typed.Where(d => matchingItems.Contains(d.PackableItemParent)).ToArray();
						resultIsValid = (result.Any()); // multiples are ok as they are all in the same package which means they have different attribs
					}

					// failed to find the item
					if (!resultIsValid)
					{
						errorMessage = GetAttemptingToScanUnpackNonPackedItemMessage(matchingWrappersOnJob.First());
					}
				}
			}

			return result;
		}

		ZString GetAttemptingToScanUnpackNonPackedItemMessage(PackableItemParentWrapper nonPackedItemParent)
		{
			return IsSinglePackageSelected
				? Res.GetString("7136002a-9bfa-47e2-8380-3bd75ede062a", "There is no {0} packed into the Selected {1}.", nonPackedItemParent.Description, SelectedPackage.KP_F3_NKPackType)
				: Res.GetString("5bbf973a-7763-46df-a606-e6e0730e7c65", "There is no single Package Selected from which to unpack any {0}.", nonPackedItemParent.Description);
		}

		#endregion

		#region TUN

		IEnumerable<PkgPackageItemDivotsWrapper> GetPackedItemsToScanUnPackForTUNScan(IEnumerable<PackableItemParentWrapper> matchingWrappersOnJob, bool isUserScanningQty, out ZString errorMessage)
		{
			#region Rules

			//  Valid configurations for TUN unpack. '[]' represents the tree selection.
			//
			//		Item: Bike
			//       TUN: BOX
			//       Qty: 5x
			//
			//  1. Selected Item *only* is in the TUN Package with matching TUN qty:
			//     BOX
			//       [5x Bike]
			//
			//  2. Selected Package is the TUN, and it contains the scanned item *only* with matching TUN qty:
			//     [BOX]
			//        5x Bike
			//
			//  3. Selected Package *CONTAINS* the TUN, and it contains the scanned item only with matching TUN qty:
			//     [PLT]
			//        BOX
			//          5x Bike

			#endregion

			errorMessage = ZString.Empty;
			var result = new List<PkgPackageItemDivotsWrapper>();

			if (IsSinglePackedItemSelected)
			{
				// single selection, packable items are identical with only attribute variation, as long as one matches we can unpack
				var selectedPackedItem = SelectedPackedItem;
				if (matchingWrappersOnJob.Any(w => IsSelectedPackedItemValidForUnpackTUN(w, selectedPackedItem)))
				{
					result.AddRange(selectedPackedItem.ParentPackage.PackedItems.Typed);
				}
				else
				{
					errorMessage = GetTUNDefinitionMismatchMessage(matchingWrappersOnJob.MaxBy(w => w.Description));
				}
			}
			else if (IsSinglePackageSelected || IsNothingSelected) // Nothing selected means PackageJob is selected most likely
			{
				var selectedPackage = SelectedPackage;
				var barcodeMatch = matchingWrappersOnJob.First().CurrentBarcodeMatch;
				if (IsSinglePackageSelected && selectedPackage.KP_F3_NKPackType == barcodeMatch.PackTypeToPackInto)
				{
					if (IsSelectedPackedItemValidForUnpackTUN(selectedPackage, barcodeMatch)
						&& matchingWrappersOnJob.Any(w => w.PackableItemParent == selectedPackage.PackedItems[0].PackableItemParent))
					{
						result.AddRange(selectedPackage.PackedItems.Typed);
					}
					else
					{
						errorMessage = GetTUNDefinitionMismatchMessage(matchingWrappersOnJob.MaxBy(w => w.Description));
					}
				}
				else // check if we are the parent package of the scanned TUN
				{
					// get the matching TUN child packages that contain the scanned item with the correct qty (and nothing else)
					var matchingPackedItems = GetMatchingPackedItemsFromChildTUNs(matchingWrappersOnJob, barcodeMatch, isUserScanningQty, out errorMessage);
					result.AddRange(matchingPackedItems);
				}
			}
			else // multiple packages or items selected
			{
				errorMessage = Res.GetString("81919f7b-a620-45e8-8f85-c81211004923", "Select a Single TUN or its Parent Package to Unpack the TUN.");
			}

			return result;
		}

		IEnumerable<PkgPackageItemDivotsWrapper> GetMatchingPackedItemsFromChildTUNs(IEnumerable<PackableItemParentWrapper> matchingWrappersOnJob, BarcodeMatch barcodeMatch, bool isUserScanningQty, out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			IEnumerable<PkgPackageItemDivotsWrapper> result = Array.Empty<PkgPackageItemDivotsWrapper>();

			var childPackages = IsSinglePackageSelected ? SelectedPackage.Packages : PackageJob.Packages;

			var validPackagesIncludingClosed = childPackages.Where(p =>
				IsSelectedPackedItemValidForUnpackTUN(p, barcodeMatch)
			 && matchingWrappersOnJob.Any(w => w.PackableItemParent == p.PackedItems[0].PackableItemParent)).ToArray();

			if (!validPackagesIncludingClosed.Any()) // not suitable packages found, not even closed/released ones
			{
				var anyWrapper = matchingWrappersOnJob.MaxBy(r => r.Description);
				errorMessage = GetTUNDefinitionMismatchMessageForParentPackage(anyWrapper);
			}
			else
			{
				var validPackagesOpenOnly = validPackagesIncludingClosed.Where(p => !p.IsClosedOrReleased).ToArray();
				var allOpenOrAllClosedPackages = validPackagesOpenOnly.Any() ? validPackagesOpenOnly : validPackagesIncludingClosed; // if all are closed, return those so that the caller knows they were Closed or Released

				if (validPackagesOpenOnly.Any(p => !p.KP_PackageID.IsEmpty))
				{
					errorMessage = Res.GetString("4f3f702d-9cc4-4a09-bd38-5daec13988e9", "One or more TUNs have a Package ID. Scan the ID or select the TUN to unpack, then re-scan the TUN barcode.");
				}
				else if (!isUserScanningQty)
				{ // if more than one attribute, we just want one of each
					result = allOpenOrAllClosedPackages
						.Select(p => new { Key = p.PackedItems[0].Key, Items = p.PackedItems.Typed })
						.GroupBy(o => o.Key)
						.SelectMany(g => g.First().Items); // we only want one of each unique group.
				}
				else
				{   // the user will want to choose the qty across duplicates
					result = allOpenOrAllClosedPackages.SelectMany(p => p.PackedItems.Typed);
				}
			}

			return result;
		}

		static bool IsSelectedPackedItemValidForUnpackTUN(PackableItemParentWrapper wrapper, PkgPackageItemDivotsWrapper selectedPackedItem)
		{
			return wrapper.PackableItemParent == selectedPackedItem.PackableItemParent
				&& IsSelectedPackedItemValidForUnpackTUN(selectedPackedItem.ParentPackage, wrapper.CurrentBarcodeMatch);
		}

		static bool IsSelectedPackedItemValidForUnpackTUN(PkgPackage tunPackage, BarcodeMatch barcodeMatch)
		{
			return barcodeMatch.PackTypeToPackInto == tunPackage.KP_F3_NKPackType
				&& tunPackage.KP_PackageQty == 1
				&& tunPackage.Packages.Count == 0
				&& IsPackedItemsCorrect(tunPackage, barcodeMatch);
		}

		static bool IsPackedItemsCorrect(PkgPackage tunPackage, BarcodeMatch barcodeMatch)
		{
			var packedItemsGrouped = tunPackage.PackedItems.Typed.GroupedPackedItems();
			return packedItemsGrouped.Count == 1 // Only one Type of Packed Item
				&& packedItemsGrouped.Single().Sum(p => p.PackedQty) == barcodeMatch.QtyToPack;
		}

		static ZString GetTUNDefinitionMismatchMessage(PackableItemParentWrapper anyWrapper)
		{
			return Res.GetString("078facb1-654d-4fde-ae80-2b9693ad9093", "The scanned TUN Code '{0}' defines a {1} with {2}x {3}. This does not match the current selection.",
				anyWrapper.CurrentBarcode, anyWrapper.CurrentBarcodeMatch.PackTypeToPackInto, anyWrapper.CurrentBarcodeMatch.QtyToPack.ToStringTrimZeros(), anyWrapper.PackableItemParent.Description);
		}

		ZString GetTUNDefinitionMismatchMessageForParentPackage(PackableItemParentWrapper anyWrapper)
		{
			return IsSinglePackageSelected

				? Res.GetString("e3629a2c-fca6-476d-b3e5-1e9285b13479", "The scanned TUN Code '{0}' defines a {1} with {2}x {3}. The Selected {4} does not contain a {5} that matches this definition.",
					anyWrapper.CurrentBarcode, anyWrapper.CurrentBarcodeMatch.PackTypeToPackInto, anyWrapper.CurrentBarcodeMatch.QtyToPack.ToStringTrimZeros(), anyWrapper.PackableItemParent.Description,
					SelectedPackage.KP_F3_NKPackType, anyWrapper.CurrentBarcodeMatch.PackTypeToPackInto)

				: Res.GetString("9f3fe2a5-a460-477b-9335-75544d77e30c", "The scanned TUN Code '{0}' defines a {1} with {2}x {3}. No Outer matches this definition.",
					anyWrapper.CurrentBarcode, anyWrapper.CurrentBarcodeMatch.PackTypeToPackInto, anyWrapper.CurrentBarcodeMatch.QtyToPack.ToStringTrimZeros(), anyWrapper.PackableItemParent.Description);
		}

		#endregion

		#region Ensure Checks

		static void EnsureItemsMatchingTUNCodeHaveTheSamePackTypeAndQty(ZString barcode, IEnumerable<PackableItemParentWrapper> matchingItems)
		{
			if (matchingItems.Any(w => !w.CurrentBarcodeMatch.IsTUN))
			{
				throw new InvalidOperationException(string.Format(Culture.Invariant,
					"Some but not all items matching barcode '{0}' are TUN codes. This is not valid -- either all or no matching items can be TUN codes.", barcode));
			}

			var firstItem = matchingItems.First();
			if (!matchingItems.All(i =>
					 i.CurrentBarcodeMatch.PackTypeToPackInto == firstItem.CurrentBarcodeMatch.PackTypeToPackInto
				&& i.CurrentBarcodeMatch.QtyToPack == firstItem.CurrentBarcodeMatch.QtyToPack))
			{
				throw new InvalidOperationException(string.Format(Culture.Invariant,
					"All items matching barcode '{0}' are TUN codes, but the PackType and/or Qty is not the same on all matching items.", barcode));
			}
		}

		bool EnsurePackagesOrItemsSelectedForUnpack(out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			bool isValidSelection = SelectedPackagesNonReadOnly.Any() || SelectedPackedItemsNonReadOnly.Any();
			if (!isValidSelection)
			{
				errorMessage = Res.GetString("40952165-a78f-4402-af67-f5a83b40d80a", "Select one or more Items or Packages to Unpack.");
			}
			return isValidSelection;
		}

		static bool EnsurePackagesAreOpen(IEnumerable<PkgPackageItemDivotsWrapper> packedItemsToUnpack, out ZString errorMessage, IEnumerable<PkgPackage> packagesToUnpack = null)
		{
			errorMessage = "";

			// union packages with the parent package of the packed items
			var distinctPackages = packedItemsToUnpack.Select(d => d.ParentPackage).Distinct();
			var packages = (packagesToUnpack != null) ? packagesToUnpack.Union(distinctPackages) : distinctPackages;
			return EnsurePackagesAreOpenIncludingChildren(packages, out errorMessage);
		}

		static bool EnsurePackagesAreOpenIncludingChildren(IEnumerable<PkgPackage> packages, out ZString errorMessage, bool isEnumeratingChildren = false)
		{
			errorMessage = ZString.Empty;
			foreach (var package in packages)
			{
				if (!package.IsAvailableForUnpacking(out errorMessage))
				{
					if (isEnumeratingChildren) // give a message that mentions child packages (instead of the selected pack msg)
					{
						errorMessage = Res.GetString("696408fa-2532-42bd-95a1-b15f94709972", "Cannot Unpack because one or more Child Packages are Closed.");
					}
					else if (packages.Count() > 1) // then give a more generic message (instead of the selected pack msg)
					{
						errorMessage = Res.GetString("693a83ec-8cf7-4191-aae0-4eb829a26ccf", "Cannot Unpack because one or more Packages are Closed or Released.");
					}
					break;
				}

				if (!EnsurePackagesAreOpenIncludingChildren(package.Packages, out errorMessage, isEnumeratingChildren: true))
				{
					break;
				}
			}

			return errorMessage.IsEmpty;
		}

		#endregion

		#endregion
	}

	#region class ItemsToUnpack

	public class ItemsToUnpack
	{
		public ItemsToUnpack(IEnumerable<PkgPackage> packages, IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> packedItemsAndBarcodes, ZString errorMessage)
		{
			Packages = packages;
			PackedItemsAndBarcodes = packedItemsAndBarcodes;
			ErrorMessage = errorMessage;
		}

		public IEnumerable<PkgPackage> Packages { get; }
		public IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> PackedItemsAndBarcodes { get; }
		public ZString ErrorMessage { get; }
	}

	#endregion
}

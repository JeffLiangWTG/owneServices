using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class PackItemsBusinessObject : ItemsBusinessObject
	{
		public PackItemsBusinessObject(PkgPackageJob packageJob, IEnumerable<IPackableItemParent> packableItemParentsToPack, IReadOnlyList<PkgPackage> existingPackages)
			: base(packageJob)
		{
			PackableItemParents = Argument.NotNull(packableItemParentsToPack, nameof(packableItemParentsToPack));
			ExistingPackages = existingPackages ?? System.Array.Empty<PkgPackage>();
		}

		#region Related Entities

		protected readonly IEnumerable<IPackableItemParent> PackableItemParents;
		protected readonly IReadOnlyList<PkgPackage> ExistingPackages;

		#region PackableItemParentsForBinding

		protected override IEnumerable<PackableItemParentWrapper> GetNewPackableItemParentsForBinding()
		{
			foreach (var wrapper in PackableItemParentWrappers)
			{
				using (wrapper.GetValidationSuspender())
				using (wrapper.SuspendSettingHasChanges())
				{
					wrapper.ProposedPackQty = wrapper.UnpackedQty;
				}
			}

			return PackableItemParentWrappers;
		}

		protected IReadOnlyCollection<PackableItemParentWrapper> PackableItemParentWrappers
		{
			get
			{
				if (packableItemParentWrappers == null)
				{
					// ToArray() to force evaluation otherwise re-evaluation on the IEnumerable will re-create wrappers (side-effects..)
					packableItemParentWrappers = PackableItemParents.Select(WrapPackableItemParent).Where(IsValidForPack).ToArray();
				}
				return packableItemParentWrappers;
			}
		}

		protected PackableItemParentWrapper WrapPackableItemParent(IPackableItemParent packableItemParent)
		{
			var wrapper = new PackableItemParentWrapper(Factory, packableItemParent);
			OnWrapPackableItemParent(wrapper);

			return wrapper;
		}

		protected virtual void OnWrapPackableItemParent(PackableItemParentWrapper wrapper)
		{
		}

		protected bool IsValidForPack(PackableItemParentWrapper wrapper)
		{
			return !wrapper.IsFullyPacked && IsValidForPackCore(wrapper);
		}

		protected virtual bool IsValidForPackCore(PackableItemParentWrapper wrapper)
		{
			return true;
		}

		PackableItemParentWrapper[] packableItemParentWrappers;

		#endregion

		#region NewPackages

		public IEnumerable<PkgPackage> NewPackages
		{
			get { return newPackages ?? Enumerable.Empty<PkgPackage>(); }
			protected set { newPackages = value; }
		}

		IEnumerable<PkgPackage> newPackages;

		#endregion

		#endregion

		#region Properties

		#region PackageTypeToCreate

		protected override ZString DefaultPackageTypeToCreate
		{
			get { return PackageJob.LastUsedOuterPackType; } // we always add new packages as outers.
		}

		#endregion

		#endregion

		#region Flags

		#region IsPackable

		public bool IsPackable
		{
			get { return Packability.IsPackable; }
		}

		public ZString IsPackableErrorMessage
		{
			get { return Packability.ErrorMessage; }
		}

		ItemPackability Packability
		{
			get { return packability ?? (packability = GetPackability()); }
		}

		ItemPackability GetPackability()
		{
			bool isPackable = false;
			bool isPackableIntoExisting = false;
			ZString result = "";

			// nothing to pack?
			if (!PackableItemParents.Any()) // don't check wrappers because if all items are fully packed, wrappers won't contain them (and we want the fully packed msg below instead)
			{
				result = IsScanPacking
						? Res.GetString("d12f60f3-8a82-4706-8781-395bf615d794", "Nothing to Pack.") // should not really get here.
						: Res.GetString("ba13ddea-0d8b-48c8-9f97-5de0ad313d31", "Select one or more items to Pack.");
			}
			// packable item parents have no packable items
			else if (!PackableItemParents.Any(packableItemParent => packableItemParent.PackableItems.Any()))
			{
				result = Res.GetString("37AE5352-8430-4F82-97E1-CDC99C4F5242", "There are no items to pack. Someone has made changes to the items to pack. Please close and re-open the form.");
			}
			// items already fully packed?
			else if (!PackableItemParentWrappers.Any())
			{
				result = GetPackabilityWhenItemsAreFullyPacked();
				if (result.IsEmpty)
				{
					result = IsScanPacking || PackableItemParents.Count() == 1
						? Res.GetString("b983e453-5a99-4e8d-99eb-0d1112f344b0", "All {0} items are fully packed.", PackableItemParentForBarcodeInformation.Description) // if scanPacking: 1 product, multiple attribs
						: Res.GetString("2693b1be-6194-4290-a0c9-969e93f3482b", "The selected items are fully packed."); // could be dragging/packing multiple items
				}
			}
			// packed Item selected?
			else if (IsScanPacking && PackageJob.Selected.IsPackedItemSelected)
			{
				result = Res.GetString("da45f725-d868-44e7-9603-009553c0579f", "Select a Single Package to pack into and then try again.");
			}
			// 0 or multiple packages selected?
			else if (!ExistingPackages.Any())
			{
				isPackable = true; // "pack into new..." menu item -or- scanning; both can pack into new instead.
			}
			// the selected package is closed or released?
			else if (ExistingPackages.Count == 1)
			{
				isPackableIntoExisting = ExistingPackages.First().IsAvailableForPacking(out result);
				isPackable = isPackableIntoExisting || IsScanPacking; // can pack into new if scanning, otherwise can pack into existing if available/open.

				if (IsScanPacking)
				{
					result = "";
				}
			}
			// multiple packages selected
			else
			{
				if (IsScanPacking)
				{
					result = Res.GetString("da45f725-d868-44e7-9603-009553c0579f", "Select a Single Package to pack into and then try again.");
				}
				// one or more packages are closed, released etc.
				else if (ExistingPackages.Any(p => p.IsClosedOrReleased)) // slight perf shortcut, rather than use .IsAvailableForPacking()
				{
					result = Res.GetString("c2972757-c588-46c2-8d92-6463749c3aae", "Cannot Pack into the selected Packages because one or more Packages are Closed or Released.");
				}
				// can pack into multiple existing packages if not scanning
				else
				{
					isPackable = true;
					isPackableIntoExisting = true;
				}
			}

			return new ItemPackability(isPackable, isPackableIntoExisting, result);
		}

		protected virtual IPackableItemParent PackableItemParentForBarcodeInformation
		{
			get { return PackableItemParents.FirstOrDefault(); }
		}

		protected virtual ZString GetPackabilityWhenItemsAreFullyPacked()
		{
			return "";
		}

		ItemPackability packability;

		class ItemPackability
		{
			public ItemPackability(bool isPackable, bool isPackableIntoExisting, ZString errorMessage)
			{
				IsPackable = isPackable;
				IsPackableIntoExisting = isPackableIntoExisting;
				ErrorMessage = errorMessage;
			}

			public bool IsPackable;
			public bool IsPackableIntoExisting;
			public ZString ErrorMessage;
		}

		#endregion

		#region IsPackableIntoExistingPackages

		public bool IsPackableIntoExistingPackages
		{
			get { return Packability.IsPackableIntoExisting; }
		}

		#endregion

		#region IsScanPacking

		public bool IsScanPacking
		{
			get { return IsScanPackingCore; }
		}

		protected virtual bool IsScanPackingCore
		{
			get { return false; }
		}

		#endregion

		#region IsPackingAlongsideExistingPackage

		public bool IsPackingAlongsideExistingPackage
		{
			get;
			protected set;
		}

		#endregion

		#endregion

		#region Validation

		public new PackItemsBusinessObjectValidation Validation
		{
			get { return (PackItemsBusinessObjectValidation)base.Validation; }
		}

		protected override ItemsBusinessObjectValidation GetNewValidation()
		{
			return new PackItemsBusinessObjectValidation(this);
		}

		#endregion

		#region RunValidationAndApplyChanges

		protected override sealed void ApplyChanges()
		{
			using (PackageJob.InitiateMassPackageProcess())
			{
				Pack();
			}
		}

		protected virtual void Pack()
		{
			if (IsPackableIntoExistingPackages)
			{
				PackageJob.MultiPack(ExistingPackages, PackableItemParentsForBinding);
			}
			else
			{
				NewPackages = PackageJob.MultiPack(PackageQtyToCreate, PackageTypeToCreate, PackableItemParentsForBinding);
			}
		}

		#endregion

		#region Pack Column/Desc

		protected override ZString ProposedPackUnpackColumnNameCore
		{
			get { return PackableItemParentWrapper.Schema.ProposedPackQty; }
		}

		protected override ZString ProposedPackUnpackDescriptionCore
		{
			get { return Res.GetString("3d6af3f2-8b1c-4b1c-b90f-a53cb2091dab", "Pack"); }
		}

		#endregion

		#region SetPackOrRemoveQuantity

		public override void SetPackOrRemoveQuantity(PackableItemParentWrapper wrapper, decimal quantity)
		{
			wrapper.ProposedPackQty = quantity;
		}

		#endregion
	}
}

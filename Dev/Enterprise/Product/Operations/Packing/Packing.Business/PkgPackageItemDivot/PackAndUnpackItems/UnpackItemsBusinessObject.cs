using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public class UnpackItemsBusinessObject : ItemsBusinessObject
	{
		public UnpackItemsBusinessObject(PkgPackageJob packageJob, IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> packedItemsToRemoveWithBarcodes, IEnumerable<PkgPackage> packagesToRemove)
			: base(packageJob)
		{
			PackedItemsToRemoveWithBarcodes = Argument.NotNull(packedItemsToRemoveWithBarcodes, nameof(packedItemsToRemoveWithBarcodes)).ToArray();
			PackagesToRemove = Argument.NotNull(packagesToRemove, nameof(packagesToRemove));
		}

		readonly IEnumerable<PkgPackage> PackagesToRemove;
		protected readonly IReadOnlyList<PkgPackageItemDivotsWrapperAndBarcode> PackedItemsToRemoveWithBarcodes;

		#region Related Entities

		#region PackableItemParentsForBinding

		protected override IEnumerable<PackableItemParentWrapper> GetNewPackableItemParentsForBinding()
		{
			var allPackedItems = GetAllPackedItems().ToArray(); // ToArray for performance to prevent re-evaluation
			var result = new List<PackableItemParentWrapper>(allPackedItems.Length);

			if (allPackedItems.Any())
			{
				//wrap packed items
				var parentJob = (IPackingParentWithPackableItems)PackageJob.ParentJob;
				foreach (var packedItemGroup in allPackedItems.GroupedPackedItems())
				{
					var wrapper = new PackableItemParentWrapper(Factory, packedItemGroup, parentJob);
					wrapper.ProposedRemoveQty = wrapper.PackedQtyFromWrapper;
					result.Add(wrapper);
				}
			}

			return result;
		}

		IEnumerable<PkgPackageItemDivotsWrapper> GetAllPackedItems()
		{
			var packedItemsFromPackages = GetAllPackedItemsFromPackages(PackagesToRemove);
			var packedItemsWithBarcodes = PackedItemsToRemoveWithBarcodes.Select(dAb => dAb.PackedItem);

			return packedItemsWithBarcodes.Union(packedItemsFromPackages);
		}

		IEnumerable<PkgPackageItemDivotsWrapper> GetAllPackedItemsFromPackages(IEnumerable<PkgPackage> packages)
		{
			var result = new List<PkgPackageItemDivotsWrapper>();

			foreach (var package in packages)
			{
				result.AddRange(GetAllPackedItemsFromPackages(package.Packages));
				result.AddRange(package.PackedItems.Typed);
			}

			return result;
		}

		#endregion

		#endregion

		#region Properties

		#region PackageTypeToCreate

		protected override ZString DefaultPackageTypeToCreate => "";

		#endregion

		#endregion

		#region Flags

		#region CanAutoApplyChanges

		protected override bool CanAutoApplyChangesCore => PackableItemParentsForBinding.Count == 0;

		#endregion

		#region IsAnythingSelectedToPackOrUnpack

		protected override bool IsAnythingSelectedToPackOrUnpackCore =>
			base.IsAnythingSelectedToPackOrUnpackCore || (PackableItemParentsForBinding.Count == 0 && PackagesToRemove.Any());

		#endregion

		#endregion

		#region Validation

		public new UnpackItemsBusinessObjectValidation Validation => (UnpackItemsBusinessObjectValidation)base.Validation;

		protected override ItemsBusinessObjectValidation GetNewValidation()
		{
			return new UnpackItemsBusinessObjectValidation(this);
		}

		#endregion

		#region RunValidationAndApplyChanges

		protected override void ApplyChanges()
		{
			var parentJob = PackageJob.ParentJob;
			parentJob?.BeforeUnpackingPackages(PackagesToRemove.ToList().AsReadOnly());

			using (PackageJob.InitiateMassPackageProcess())
			{
				UnpackPackedItems();
				DeleteEmptyPackages();
			}
		}

		void UnpackPackedItems()
		{
			var wrappers = GetWrappersToUnpack().ToArray();
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory)) // for performance
			{
				UnpackPackedItemsWithInfoOnException(wrappers);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error logging")]
		void UnpackPackedItemsWithInfoOnException(IEnumerable<PackableItemParentWrapper> wrappers)
		{
			try
			{
				foreach (var wrapper in wrappers)
				{
					UnpackPackedItemsCore(wrapper);
				}
			}
			catch (NullReferenceException ex)
			{
				var additionalErrorMessage = "";
				var wrapperWithNullGroupedPackedItems = wrappers.FirstOrDefault(wrapper => wrapper.GroupedPackedItems == null);
				if (wrapperWithNullGroupedPackedItems != null)
				{
					additionalErrorMessage = $@"GroupedPackedItems should not be null but is null.
PackableItemParent is {(wrapperWithNullGroupedPackedItems.PackableItemParent != null ? "not null" : "null")}.
ParentJob is {(wrapperWithNullGroupedPackedItems.ParentJob != null ? "not null" : "null")}.";
				}
				else
				{
					if (wrappers.SelectMany(wrapper => wrapper.GroupedPackedItems).Any(packedItem => packedItem.ParentPackage == null))
					{
						additionalErrorMessage = "Parent package on packedItem is null.";
					}
				}

				throw new NullReferenceException($@"{ex.Message}
{additionalErrorMessage}", ex);
			}
		}

		void UnpackPackedItemsCore(PackableItemParentWrapper wrapper)
		{
			var qtyToUnpack = wrapper.ProposedRemoveQty;
			foreach (var packedItem in wrapper.GroupedPackedItems)
			{
				var qtyCanUnpack = Math.Min(packedItem.PackedQty, qtyToUnpack);
				packedItem.ParentPackage.Unpack(packedItem, qtyCanUnpack, RemoveEmptyPackageEvenWithID);

				qtyToUnpack -= qtyCanUnpack;

				if (qtyToUnpack == 0m)
				{
					break;
				}
			}
		}

		protected virtual bool RemoveEmptyPackageEvenWithID => false;

		protected virtual IEnumerable<PackableItemParentWrapper> GetWrappersToUnpack()
		{
			return PackableItemParentsForBinding;
		}

		void DeleteEmptyPackages()
		{
			Array.ForEach(PackagesToRemove.ToArray(), pkg => AddFetchHintsIncludingChildren(pkg));
			Array.ForEach(PackagesToRemove.Reverse().ToArray(), pkg => pkg.DeleteEmptyPackagesIncludingChildren());
		}

		void AddFetchHintsIncludingChildren(PkgPackage package)
		{
			Factory.AddFetchHint(typeof(PkgPackageBookedDetail), PkgPackageBookedDetailSchema.KPB_KP_Package, package.PK);
			Factory.AddFetchHint(JobServiceLinkSchema.ESL_ParentID, package.PK);

			// tested in WhsOrderTest.TestDbHitsDeletePackagesWhenFinaliseOrder
			Factory.AddFetchHint(StmDefaultPrinterSchema.SDP_SubjectID, package.PK);

			Array.ForEach(package.Packages.ToArray(), pkg =>
			{
				AddFetchHintsIncludingChildren(pkg);
			});
		}

		#endregion

		#region Unpack Column/Desc

		protected override ZString ProposedPackUnpackColumnNameCore
		{
			get { return PackableItemParentWrapper.Schema.ProposedRemoveQty; }
		}

		protected override ZString ProposedPackUnpackDescriptionCore
		{
			get { return Res.GetString("37300cfd-ed2a-4b77-a689-06fadee89c58", "Unpack"); }
		}

		#endregion

		#region SetPackOrRemoveQuantity

		public override void SetPackOrRemoveQuantity(PackableItemParentWrapper wrapper, decimal quantity)
		{
			wrapper.ProposedRemoveQty = quantity;
		}

		#endregion
	}
}

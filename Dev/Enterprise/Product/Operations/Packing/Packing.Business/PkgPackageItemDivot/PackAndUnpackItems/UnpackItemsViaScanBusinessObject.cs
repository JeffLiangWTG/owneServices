using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class UnpackItemsViaScanBusinessObject : UnpackItemsBusinessObject, IScanningItemsBusinessObject
	{
		public UnpackItemsViaScanBusinessObject(PkgPackageJob packageJob, IEnumerable<PkgPackageItemDivotsWrapperAndBarcode> packedItemsToRemoveWithBarcodes, bool isUnpackingQty = false)
			: base(packageJob, packedItemsToRemoveWithBarcodes, System.Array.Empty<PkgPackage>())
		{
			isUserEnteringQty = isUnpackingQty;
		}

		#region Related Entities

		#region PackableItemParentsForBinding

		protected override IEnumerable<PackableItemParentWrapper> GetNewPackableItemParentsForBinding()
		{
			return ScanHelper.WrappersCacheCurrent.Distinct(new PackableItemParentWrapperEqualityComparer()).ToArray(); // distinct is for TUNs (it is always distinct when not TUN)
		}

		#endregion

		#endregion

		#region Properties

		#region PackageTypeToCreate

		protected override ZString DefaultPackageTypeToCreate
		{
			get { return TUNCode; }
		}

		#endregion

		#region TUNCodeAvailableCount

		public int TUNCodeAvailableCount
		{
			get
			{
				var cache = ScanHelper.WrappersCacheCurrent;
				if (cache == null)
				{
					BuildPackableItems();
					cache = ScanHelper.WrappersCacheCurrent;
				}

				return cache.Count();
			}
		}

		#endregion

		#endregion

		#region Flags

		#region CanAutoApplyChanges

		protected override bool CanAutoApplyChangesCore
		{
			get { return ScanHelper.CanAutoApplyChanges; }
		}

		#endregion

		#region CanScanPackQuanity

		protected override bool CanScanPackQuanityCore()
		{
			return ScanHelper.CanScanPackOrRemoveQuanity;
		}

		#endregion

		#region IsAnythingSelectedToPackOrUnpack

		protected override bool IsAnythingSelectedToPackOrUnpackCore
		{
			get { return ScanHelper.IsAnythingSelectedToPackOrUnpack; }
		}

		#endregion

		#endregion

		#region Mode

		protected override PackOrUnpackMode GetMode()
		{
			return ScanHelper.GetMode();
		}

		#endregion

		#region RunValidationAndApplyChanges

		protected override bool RemoveEmptyPackageEvenWithID
		{
			get { return IsTUN; }
		}

		protected override IEnumerable<PackableItemParentWrapper> GetWrappersToUnpack()
		{
			return IsTUN ? ScanHelper.WrappersCacheCurrent.Take(PackageQtyToCreate) : ScanHelper.WrappersCacheCurrent;
		}

		#endregion

		#region Validation

		public new UnpackItemsViaScanBusinessObjectValidation Validation
		{
			get { return (UnpackItemsViaScanBusinessObjectValidation)base.Validation; }
		}

		protected override ItemsBusinessObjectValidation GetNewValidation()
		{
			return new UnpackItemsViaScanBusinessObjectValidation(this);
		}

		#endregion

		#region IScanningItemsBusinessObject

		void IScanningItemsBusinessObject.SelectPackableItemParent(PackableItemParentWrapper wrapper)
		{
			ScanHelper.SelectPackableItemParent(wrapper);
		}

		public bool IsUserEnteringQty
		{
			get { return isUserEnteringQty; }
		}

		public bool IsTUN
		{
			get { return !TUNCode.IsEmpty; }
		}

		ZString IScanningItemsBusinessObject.TUNCode
		{
			get { return TUNCode; }
		}

		IEnumerable<PackableItemParentWrapper> IScanningItemsBusinessObject.GetAllWrappers()
		{
			var result = new List<PackableItemParentWrapper>(PackedItemsToRemoveWithBarcodes.Count);

			// wrap packed items with barcodes and set initial qty
			var parentJob = (IPackingParentWithPackableItems)PackageJob.ParentJob;
			var barcodeCache = new Dictionary<GroupingKey, BarcodeMatch>();
			var packedItems = new PkgPackageItemDivotsWrapper[PackedItemsToRemoveWithBarcodes.Count];

			for (int index = 0; index < PackedItemsToRemoveWithBarcodes.Count; index++)
			{
				var packedItemAndBarcode = PackedItemsToRemoveWithBarcodes[index];
				packedItems[index] = packedItemAndBarcode.PackedItem;
				barcodeCache[packedItemAndBarcode.PackedItem.Key] = packedItemAndBarcode.Barcode;
			}

			foreach (var packedItemGroup in packedItems.GroupedPackedItems())
			{
				var wrapper = new PackableItemParentWrapper(Factory, packedItemGroup, parentJob);
				// If user is entering the qty, default to full amount so that they can just click Unpack.
				// If not entering the qty, default to barcode amount (either 1 or TUN qty if unpacking a TUN).
				wrapper.ProposedRemoveQty = IsUserEnteringQty
					? wrapper.PackedQtyFromWrapper
					: SameItemExistsInList(result, wrapper)
						? 0
						: barcodeCache[wrapper.Key].QtyToPack;
				result.Add(wrapper);
			}

			return result;
		}

		static bool SameItemExistsInList(IEnumerable<PackableItemParentWrapper> result, PackableItemParentWrapper wrapper)
		{
			return result.Any(w => w.Key.IsSimilarItem_DoNotUse(wrapper.Key));
		}

		bool IScanningItemsBusinessObject.AddFilterIfValidAttribute(ZString value)
		{
			return ScanHelper.AddFilterIfValidAttribute(value);
		}

		ZString TUNCode
		{
			// note on First(): qty must be the same for all items matching a single barcode (attributes can be different of course)
			get { return tUNCode ?? (tUNCode = PackableItemParentsForBinding.Count > 0 ? PackedItemsToRemoveWithBarcodes.First().Barcode.PackTypeToPackInto : ZString.Empty); }
		}

		UnpackItemsViaScanBusinessObjectHelper ScanHelper
		{
			get { return scanHelper ?? (scanHelper = new UnpackItemsViaScanBusinessObjectHelper(this)); }
		}

		readonly bool isUserEnteringQty;
		string tUNCode;
		UnpackItemsViaScanBusinessObjectHelper scanHelper;

		#endregion

		#region class PackableItemParentWrapperEqualityComparer

		class PackableItemParentWrapperEqualityComparer : IEqualityComparer<PackableItemParentWrapper>
		{
			public bool Equals(PackableItemParentWrapper x, PackableItemParentWrapper y) { return x.PackableItemParent == y.PackableItemParent; }
			public int GetHashCode(PackableItemParentWrapper obj) { return obj.PackableItemParent.GetHashCode(); }
		}

		#endregion
	}
}

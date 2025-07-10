using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	abstract class ScanningItemsBusinessObjectHelper<T>
		where T : ItemsBusinessObject, IScanningItemsBusinessObject
	{
		public ScanningItemsBusinessObjectHelper(T bizO)
		{
			BizO = bizO;
		}

		protected readonly T BizO;

		#region CanAutoApplyChanges

		public bool CanAutoApplyChanges
		{
			get { return !BizO.IsUserEnteringQty && (WrappersCacheCurrent.Count() == 1 || AllWrappersHaveSameItem(WrappersCacheCurrent)); }
		}

		bool AllWrappersHaveSameItem(IEnumerable<PackableItemParentWrapper> wrappers)
		{
			return wrappers.Count() > 1 && IsDistinct(wrappers);
		}

		#endregion

		#region CanScanPackOrRemoveQuanity

		public bool CanScanPackOrRemoveQuanity
		{
			get { return BizO.IsUserEnteringQty && WrappersCacheCurrent.Count() == 1; } // Pack quantity can be scanned in scan mode and if there is only 1 line left
		}

		#endregion

		#region SelectPackableItemParent

		public void SelectPackableItemParent(PackableItemParentWrapper wrapper)
		{
			if (wrapper != null)
			{
				if (!BizO.PackableItemParentsForBinding.Contains(wrapper))
				{
					throw new ArgumentException("Wrapper cannot be selected because it does not exist in the PackableItemParents collection.");
				}

				AddFilter(wrapper);
				OnWrapperSelected();
			}
		}

		protected virtual void OnWrapperSelected()
		{
		}

		#endregion

		#region IsAnythingSelectedToPackOrUnpack

		public bool IsAnythingSelectedToPackOrUnpack
		{
			get { return (BizO.Mode == PackOrUnpackMode.Attribs || BizO.Mode == PackOrUnpackMode.TUN_Qty_Attribs) ? HasDistinctSelection : BizO.HasItemsWithQtyGreatherThanZero; }
		}

		bool HasDistinctSelection
		{
			get
			{
				return
					WrappersCacheForSelectedItem != null // if not null would contain all packed items matching a distinct packable item
				|| (WrappersCacheForScannedAttributes != null && IsDistinct(WrappersCacheForScannedAttributes));
			}
		}

		static bool IsDistinct(IEnumerable<PackableItemParentWrapper> wrappers)
		{
			// faster than distinct + count > 1
			var first = wrappers.FirstOrDefault();
			return first == null || wrappers.All(w => w.Key.IsSimilarItem_DoNotUse(first.Key));
		}

		#endregion

		#region Add Filter

		#region AddFilter (wrapper)

		void AddFilter(PackableItemParentWrapper wrapper)
		{
			Argument.NotNull(wrapper, nameof(wrapper));
			CreateWrappersCacheForSelectedItem(wrapper.PackableItemParent);
			EnableValidationOnSelectedOrDistinctWrapperOnly(wrapper);
		}

		void CreateWrappersCacheForSelectedItem(IPackableItemParent selectedItem)
		{
			if (selectedItem != null)
			{
				var matches = WrappersCacheAll.Where(w => w.PackableItemParent == selectedItem);
				WrappersCacheForSelectedItem = new List<PackableItemParentWrapper>(matches);
			}
		}

		#endregion

		#region AddFilterIfValidAttribute (attribute text)

		public bool AddFilterIfValidAttribute(ZString value)
		{
			Argument.NotNullOrEmpty(value, "value");

			var attribWasValid = CreateWrappersCacheForScannedAttributes(value);
			if (attribWasValid)
			{
				EnableValidationOnSelectedOrDistinctWrapperOnly();
				BizO.RefreshPackableItemParentsForBinding();
			}

			return attribWasValid;
		}

		bool CreateWrappersCacheForScannedAttributes(ZString attributeValue)
		{
			bool attributeIsValid = false;

			if (!attributeValue.IsEmpty) // scans override any prev selection that is not yet applied
			{
				var cacheToUse = WrappersCacheForScannedAttributes ?? WrappersCacheAll;
				var matches = cacheToUse.Where(w => w.PackableItemParent.AdditionalProperties.CustomProperties.Any(p => attributeValue.EqualsIgnoringCase(p.GetValue((BusinessObject)w.PackableItemParent).ToString())));

				attributeIsValid = matches.Any();
				if (attributeIsValid)
				{
					WrappersCacheForScannedAttributes = new List<PackableItemParentWrapper>(matches);
					WrappersCacheForSelectedItem = null;
				} // else no match, don't filter further
			}

			return attributeIsValid;
		}

		#endregion

		#region EnableValidationOnSelectedOrDistinctWrapperOnly

		void EnableValidationOnSelectedOrDistinctWrapperOnly(PackableItemParentWrapper selectedWrapper = null)
		{
			// NOTE: do not use PackableItemParentsForBinding because it is not yet updated.

			// clear errors on all items not matching current filter (after refresh they are no longer in the grid)
			foreach (var wrapper in WrappersCacheAll.Except(WrappersCacheCurrent))
			{
				wrapper.SetIsValidationDisabled(true);
				wrapper.Validation.ValidateAll();
			}

			// 1 item selected, validate it
			if (selectedWrapper != null)
			{
				selectedWrapper.SetIsValidationDisabled(false);
				selectedWrapper.Validation.ValidateAll();
			}
			// either 1 item left (packable), or items are non-distinct (not yet packable), re-enable validation on them..
			else
			{
				foreach (var wrapper in WrappersCacheCurrent)
				{
					wrapper.SetIsValidationDisabled(false);
				}
			}
		}

		#endregion

		#region WrappersCacheCurrent

		public IEnumerable<PackableItemParentWrapper> WrappersCacheCurrent
		{
			get { return WrappersCacheForSelectedItem ?? WrappersCacheForScannedAttributes ?? WrappersCacheAll; }
		}

		IEnumerable<PackableItemParentWrapper> WrappersCacheAll
		{
			get { return wrappersCacheAll ?? (wrappersCacheAll = BizO.GetAllWrappers()); }
		}

		IEnumerable<PackableItemParentWrapper> wrappersCacheAll;
		IEnumerable<PackableItemParentWrapper> WrappersCacheForScannedAttributes;
		IEnumerable<PackableItemParentWrapper> WrappersCacheForSelectedItem;

		#endregion

		#endregion

		#region GetMode

		public PackOrUnpackMode GetMode()
		{
			PackOrUnpackMode result;

			if (BizO.IsUserEnteringQty && BizO.IsTUN)
			{
				result = (IsUsingAttribs) ? PackOrUnpackMode.TUN_Qty_Attribs : PackOrUnpackMode.TUN_Qty;
			}
			else if (BizO.IsUserEnteringQty)
			{
				result = PackOrUnpackMode.Qty;
			}
			else if (IsUsingAttribs)
			{
				result = PackOrUnpackMode.Attribs; // scan attribs (we would not show this form for a single item)
			}
			else
			{
				result = PackOrUnpackMode.SingleItemNoQty;
			}

			return result;
		}

		bool IsUsingAttribs
		{
			get { return !IsDistinct(WrappersCacheAll); }
		}

		#endregion
	}

	#region Class PackItemsViaScanBusinessObjectHelper

	class PackItemsViaScanBusinessObjectHelper : ScanningItemsBusinessObjectHelper<PackItemsViaScanBusinessObject>
	{
		public PackItemsViaScanBusinessObjectHelper(PackItemsViaScanBusinessObject bizO)
			: base(bizO)
		{
		}
	}

	#endregion

	#region Class UnpackItemsViaScanBusinessObjectHelper

	class UnpackItemsViaScanBusinessObjectHelper : ScanningItemsBusinessObjectHelper<UnpackItemsViaScanBusinessObject>
	{
		public UnpackItemsViaScanBusinessObjectHelper(UnpackItemsViaScanBusinessObject bizO)
			: base(bizO)
		{
		}

		protected override void OnWrapperSelected()
		{
			base.OnWrapperSelected();
			BizO.Validation.ValidatePackageQtyToCreate();
		}
	}

	#endregion
}

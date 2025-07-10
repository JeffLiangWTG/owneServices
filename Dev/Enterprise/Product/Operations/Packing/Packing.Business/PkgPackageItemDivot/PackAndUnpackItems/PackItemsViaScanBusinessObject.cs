using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class PackItemsViaScanBusinessObject : PackItemsBusinessObject, IScanningItemsBusinessObject
	{
		public PackItemsViaScanBusinessObject(ZString barcode, PkgPackageJob packageJob, IEnumerable<IPackableItemParent> allPackableItemParents, IReadOnlyList<PkgPackage> existingPackages, bool isPackingQty = false)
			: base(packageJob, allPackableItemParents, existingPackages)
		{
			Barcode = Argument.NotNullOrEmpty(barcode, "barcode");
			isUserEnteringQty = isPackingQty;
		}

		readonly ZString Barcode;

		#region Related Entities

		#region PackableItemParentsForBinding

		protected override IEnumerable<PackableItemParentWrapper> GetNewPackableItemParentsForBinding()
		{
			return new List<PackableItemParentWrapper>(ScanHelper.WrappersCacheCurrent);
		}

		protected override void OnWrapPackableItemParent(PackableItemParentWrapper wrapper)
		{
			base.OnWrapPackableItemParent(wrapper);
			wrapper.CurrentBarcode = Barcode;
			StorePackableItemParentWrapperForBarcodeInformation(wrapper);
		}

		void StorePackableItemParentWrapperForBarcodeInformation(PackableItemParentWrapper wrapper)
		{
			if (!PackableItemParentWrapperForBarcodeInformationHadMultipleMatches && wrapper.CurrentBarcodeMatch.IsMatch)
			{
				if (packableItemParentWrapperForBarcodeInformation == null) // bit ugly because IsMatch will be checked later, but unless perf prob, code is neater like this.
				{
					packableItemParentWrapperForBarcodeInformation = wrapper;
				}
				else
				{
					PackableItemParentWrapperForBarcodeInformationHadMultipleMatches = true; // means there were multiple attribs that could not be packed due to lack of avail qty
				}
			}
		}

		protected override bool IsValidForPackCore(PackableItemParentWrapper wrapper)
		{
			// eg. 3 avail, but TUN defines BOX of 4.
			return wrapper.CurrentBarcodeMatch.IsMatch && wrapper.CurrentBarcodeMatch.QtyToPack <= wrapper.UnpackedQty;
		}

		/// <summary>
		/// PackableItemParentsForBinding will filter out any items that are either fully packed, or do not have enough qty to meet the TUN qty requirement.
		/// This can result in PackableItemParentsForBinding being empty (which is valid), however we need access to at least one wrapped item to check the TUN code,
		/// to test for a valid barcode etc.
		/// </summary>
		PackableItemParentWrapper PackableItemParentWrapperForBarcodeInformation
		{
			get
			{
				if (packableItemParentWrapperForBarcodeInformation == null)
				{
					BuildPackableItems();
				}
				return packableItemParentWrapperForBarcodeInformation;
			}
		}

		protected override IPackableItemParent PackableItemParentForBarcodeInformation
		{
			get { return PackableItemParentWrapperForBarcodeInformation.PackableItemParent; }
		}

		PackableItemParentWrapper packableItemParentWrapperForBarcodeInformation;
		bool PackableItemParentWrapperForBarcodeInformationHadMultipleMatches;

		#endregion

		#endregion

		#region Properties

		#region PackageTypeToCreate

		protected override ZString DefaultPackageTypeToCreate
		{
			get
			{
				var result = TUNCode;
				return result.IsEmpty ? base.DefaultPackageTypeToCreate : result;
			}
		}

		#endregion

		#region PackageQtyToCreate

		protected override void OnPackageQtyToCreateChanged()
		{
			base.OnPackageQtyToCreateChanged();
			UpdateProposedPackQtys();
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

		#region HasItemMatchingBarcode

		public bool HasItemMatchingBarcode
		{
			get { return PackableItemParentWrapperForBarcodeInformation != null; } // Wrappers will not include matching barcodes that are fully packed, so don't use them.
		}

		#endregion

		#region IsAnythingSelectedToPackOrUnpack

		protected override bool IsAnythingSelectedToPackOrUnpackCore
		{
			get { return ScanHelper.IsAnythingSelectedToPackOrUnpack; }
		}

		#endregion

		#region IsPackable

		protected override ZString GetPackabilityWhenItemsAreFullyPacked()
		{
			var result = base.GetPackabilityWhenItemsAreFullyPacked();

			if (result.IsEmpty && IsTUN)
			{
				var unpackedQty = PackableItemParentWrapperForBarcodeInformation.UnpackedQty;

				ZString availableUnpackedItemsText;
				if (PackableItemParentWrapperForBarcodeInformationHadMultipleMatches)
				{
					// don't say "none" -- with TUN + attribs we could have:
					// TUN definition of 5;
					//   attr1 has 3 avail
					//   attr2 has 1 avail
					//   attr3 has 0 available
					// -- cannot know which one to pick for the message (and obv don't want to ask the user :P)
					availableUnpackedItemsText = Res.GetString("2caa93c2-295d-476a-ae3a-916288ba9a44", "not enough are");
				}
				else if (unpackedQty == 0)
				{
					availableUnpackedItemsText = Res.GetString("02e183fd-11a1-4cef-9109-207a9a99eb21", "none are");
				}
				else if (unpackedQty == 1)
				{
					availableUnpackedItemsText = Res.GetString("7e6a5dd1-f8fd-4ff4-9709-b4cacc8f6f49", "only 1 is");
				}
				else
				{
					availableUnpackedItemsText = Res.GetString("7001046c-44fc-4e41-ae2a-3d50352e3f1c", "only {0} are", PackableItemParentWrapperForBarcodeInformation.UnpackedQty.ToStringTrimZeros());
				}

				result = Res.GetString("128cd4a8-4b41-454f-9bf5-d9877e5b8287", "The scanned TUN Code '{0}' defines a {1} with {2}x {3} but {4} available.",
						PackableItemParentWrapperForBarcodeInformation.CurrentBarcode,
						PackableItemParentWrapperForBarcodeInformation.CurrentBarcodeMatch.PackTypeToPackInto,
						PackableItemParentWrapperForBarcodeInformation.CurrentBarcodeMatch.QtyToPack.ToStringTrimZeros(),
						PackableItemParentWrapperForBarcodeInformation.PackableItemParent.Description, availableUnpackedItemsText);
			}

			return result;
		}

		#endregion

		#region IsScanPacking

		protected override bool IsScanPackingCore
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region UpdateProposedPackQtys

		void UpdateProposedPackQtys()
		{
			ClearProposedPackQty();

			// if entering qty for non-TUN, set proposed qty to full amount
			// otherwise, set it to the barcode qty (1 if not TUN) multiplied by package qty to create
			foreach (var wrapper in ScanHelper.WrappersCacheCurrent)
			{
				if (isUserEnteringQty && !IsTUN)
				{
					wrapper.ProposedPackQty = wrapper.UnpackedQty;
				}
				else
				{
					// in scan mode if we have multi match we pack one at a time
					wrapper.ProposedPackQty = GetProposedQty(wrapper);
					break;
				}
			}
		}

		void ClearProposedPackQty()
		{
			PackableItemParentWrappers.Where((w => w.ProposedPackQty != 0)).ForEach(w => w.ProposedPackQty = 0);
		}

		ZDecimal GetProposedQty(PackableItemParentWrapper wrapper)
		{
			return wrapper.CurrentBarcodeMatch.QtyToPack * Math.Max(0, PackageQtyToCreate);
		}

		#endregion

		#region RunValidationAndApplyChanges

		protected override void Pack()
		{
			IsPackingAlongsideExistingPackage = false;

			if (IsPackableIntoExistingPackages)
			{
				if (TUNCode.IsEmpty)
				{
					// pack items directly into the existing package
					PackageJob.MultiPack(ExistingPackages, ScanHelper.WrappersCacheCurrent);
				}
				else
				{
					var parentPackage = ExistingPackages.First();
					IsPackingAlongsideExistingPackage = TUNCode.EqualsIgnoringCase(parentPackage.KP_F3_NKPackType);

					if (IsPackingAlongsideExistingPackage)
					{
						// never scan pack a BOX into a BOX (they should always be BOX *alongside* BOX as opposed to BOX *contains* BOX)
						parentPackage = parentPackage.ParentPackage;
					}

					// create a new package in the existing package and pack items into that
					NewPackages = PackageJob.MultiPack(PackageQtyToCreate, TUNCode, ScanHelper.WrappersCacheCurrent, parentPackage);
				}
			}
			else
			{
				// create a new package and pack items into that
				NewPackages = PackageJob.MultiPack(PackageQtyToCreate, PackageTypeToCreate, ScanHelper.WrappersCacheCurrent);
			}
		}

		#endregion

		#region Mode

		protected override PackOrUnpackMode GetMode()
		{
			return ScanHelper.GetMode();
		}

		#endregion

		#region IScanningItemsBusinessObject

		void IScanningItemsBusinessObject.SelectPackableItemParent(PackableItemParentWrapper wrapper)
		{
			ScanHelper.SelectPackableItemParent(wrapper);
			UpdateProposedPackQtys();
		}

		bool IScanningItemsBusinessObject.IsUserEnteringQty
		{
			get { return isUserEnteringQty; }
		}

		bool IScanningItemsBusinessObject.IsTUN
		{
			get { return IsTUN; }
		}

		ZString IScanningItemsBusinessObject.TUNCode
		{
			get { return TUNCode; }
		}

		bool IScanningItemsBusinessObject.AddFilterIfValidAttribute(ZString value)
		{
			var result = ScanHelper.AddFilterIfValidAttribute(value);
			UpdateProposedPackQtys();
			return result;
		}

		IEnumerable<PackableItemParentWrapper> IScanningItemsBusinessObject.GetAllWrappers()
		{
			InitialisePackTypeAndProposedPackQty();
			return PackableItemParentWrappers;
		}

		// implementation

		bool IsTUN
		{
			get { return !TUNCode.IsEmpty; }
		}

		ZString TUNCode
		{
			get { return PackableItemParentWrapperForBarcodeInformation?.CurrentBarcodeMatch.PackTypeToPackInto ?? ZString.Empty; }
		}

		void InitialisePackTypeAndProposedPackQty()
		{
			if (!IsPackTypeAndProposedPackQtyInitialised)
			{
				IsPackTypeAndProposedPackQtyInitialised = true;
				UpdateProposedPackQtys();
				UpdatePackageTypeToCreate();
			}
		}

		void UpdatePackageTypeToCreate()
		{
			var tunCode = TUNCode;
			if (!tunCode.IsEmpty)
			{
				PackageTypeToCreate = tunCode;
			}
		}

		PackItemsViaScanBusinessObjectHelper ScanHelper
		{
			get { return scanHelper ?? (scanHelper = new PackItemsViaScanBusinessObjectHelper(this)); }
		}

		bool IsPackTypeAndProposedPackQtyInitialised;
		readonly bool isUserEnteringQty;
		PackItemsViaScanBusinessObjectHelper scanHelper;

		#endregion
	}
}

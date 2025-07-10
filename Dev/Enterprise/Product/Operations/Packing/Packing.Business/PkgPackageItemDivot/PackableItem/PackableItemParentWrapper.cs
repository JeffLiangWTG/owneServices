using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Packing.Business
{
	/// <summary>
	/// This class wraps an IPackableItemParent and adds aggregate properties -- useful for GUI binding.
	/// </summary>
	public class PackableItemParentWrapper : NonPersistentBusinessObject<PackableItemParentWrapperEmptyValidation>, ICustomPropertyContainer, IPackableItemParentWrapper
	{
		public static ZString ItemNoLongerExistsText
		{
			get { return Res.GetString("8806f61d-b620-4ab1-b8c6-e002a1e0ffca", "<item no longer exists>"); }
		}

		#region Schema

		public static class Schema
		{
			public const string ProposedPackQty = "ProposedPackQty";
			public const string ProposedRemoveQty = "ProposedRemoveQty";
		}

		#endregion

		#region Construction

		internal PackableItemParentWrapper(BusinessObjectFactory factory, PackableItemParentWrapper wrapper)
			: this(factory, null, Argument.NotNull(wrapper, nameof(wrapper)).PackableItemParent, wrapper.Key)
		{
		}

		public PackableItemParentWrapper(BusinessObjectFactory factory, IPackableItemParent packableItemParentToWrap, IPackingParentWithPackableItems parentJob = null)
			: this(factory, parentJob, packableItemParentToWrap, GetKeyFromPackableItemParent(packableItemParentToWrap))
		{
		}

		static GroupingKey GetKeyFromPackableItemParent(IPackableItemParent packableItemParentToWrap)
		{
			return packableItemParentToWrap?.PackableItems.First().Key ?? EmptyKey.Instance;
		}

		PackableItemParentWrapper(BusinessObjectFactory factory, IPackingParentWithPackableItems parentJob, IPackableItemParent packableItemParentToWrap, GroupingKey key)
			: this(factory, parentJob, key)
		{
			packableItemParent = Argument.NotNull(packableItemParentToWrap, nameof(packableItemParentToWrap));
		}

		PackableItemParentWrapper(BusinessObjectFactory factory, IPackingParentWithPackableItems parentJob, GroupingKey key)
			: base(factory)
		{
			ParentJob = parentJob;
			Key = Argument.NotNull(key, nameof(key));
		}

		/// <summary>
		/// This Constructor should only be used for Unpack, so that Unpacking can show Grouped Packed Items that are in the same Package.
		/// </summary>
		internal PackableItemParentWrapper(BusinessObjectFactory factory, IGrouping<object, PkgPackageItemDivotsWrapper> packedItemWrappers, IPackingParentWithPackableItems parentJob)
			: this(factory, GetWrappersWithNullCheck(packedItemWrappers), parentJob)
		{
		}

		PackableItemParentWrapper(BusinessObjectFactory factory, IReadOnlyList<PkgPackageItemDivotsWrapper> packedItemWrappers, IPackingParentWithPackableItems parentJob)
			: this(factory, parentJob, GetKeyFromPackedItemsWithEnsureCheck(packedItemWrappers))
		{
			GroupedPackedItems = packedItemWrappers;

			// packedItemWrapper can have a null PackableItemParent if the item no longer exists, so don't null check. the gui will show <item no longer exists> or similar.
			packableItemParent = packedItemWrappers[0].PackableItemParent;
		}

		static PkgPackageItemDivotsWrapper[] GetWrappersWithNullCheck(IGrouping<object, PkgPackageItemDivotsWrapper> packedItemWrappers)
		{
			return Argument.NotNull(packedItemWrappers, nameof(packedItemWrappers)).ToArray();
		}

		static GroupingKey GetKeyFromPackedItemsWithEnsureCheck(IReadOnlyList<PkgPackageItemDivotsWrapper> packedItemWrappers)
		{
			Argument.GreaterThanZero(packedItemWrappers.Count, nameof(packedItemWrappers.Count));
			return packedItemWrappers[0].Key;
		}

		#endregion

		#region Related Entities

		public GroupingKey Key { get; }
		public IPackableItemParent PackableItemParent => packableItemParent == null || packableItemParent.IsDeleted ? null : packableItemParent;
		public IPackingParentWithPackableItems ParentJob { get; }

		readonly IPackableItemParent packableItemParent;

		internal IReadOnlyList<PkgPackageItemDivotsWrapper> GroupedPackedItems { get; }

		#endregion

		#region Properties

		#region Description

		public ZString Description
		{
			get { return PackableItemParent?.Description ?? ItemNoLongerExistsText; }
		}

		#endregion

		#region PackedQty

		public ZDecimal PackedQty
		{
			get { return GetPackedQty(); }
		}

		public ZPropertyInfo PackedQtyInfo
		{
			get { return GetZPropertyInfo(nameof(PackedQty)); }
		}

		ZDecimal GetPackedQty() => PackableItemParent.GetPackedQty(Factory);

		internal ZInt PackageQtyToCreate;
		internal ZString PackageTypeToCreateDescription;

		#endregion

		#region PackedQtyFromWrapper

		public ZDecimal PackedQtyFromWrapper => GroupedPackedItems.Sum(w => w.PackedQty);

		#endregion

		#region UnpackedQty

		public ZDecimal UnpackedQty
		{
			get { return PackableItemParent.TotalQty - PackedQty; }
		}

		public ZPropertyInfo UnpackedQtyInfo
		{
			get { return GetZPropertyInfo(nameof(UnpackedQty)); }
		}

		#endregion

		#region UnpackedWeight

		public ZDecimal UnpackedWeight
		{
			get
			{
				if (!UnpackedWeightValidated)
				{
					UnpackedWeightValidated = true;
					Validation.ValidateUnpackedWeight();
				}
				return PackableItemParent.WeightPerUnit * UnpackedQty;
			}
		}

		public ZPropertyInfo UnpackedWeightInfo
		{
			get { return GetZPropertyInfo(nameof(UnpackedWeight)); }
		}

		bool UnpackedWeightValidated;

		#endregion

		// used when packing / removing

		#region ProposedPackQty

		public ZDecimal ProposedPackQty
		{
			get { return proposedPackQty; }
			set
			{
				SetNonPersistentPropertyValue(ProposedPackQtyInfo, ref proposedPackQty, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateProposedPackQty();
				}
			}
		}

		public ZPropertyInfoDecimal ProposedPackQtyInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(Schema.ProposedPackQty); }
		}

		ZDecimal proposedPackQty;

		#endregion

		#region ProposedRemoveQty

		public ZDecimal ProposedRemoveQty
		{
			get { return proposedRemoveQty; }
			set
			{
				if (GroupedPackedItems == null)
				{
					throw new NotSupportedException("ProposedRemoveQty cannot be used unless you are wrapping a Packed Item.");
				}

				SetNonPersistentPropertyValue(ProposedRemoveQtyInfo, ref proposedRemoveQty, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateProposedRemoveQty();
				}
			}
		}

		public ZPropertyInfoDecimal ProposedRemoveQtyInfo
		{
			get { return (ZPropertyInfoDecimal)GetZPropertyInfo(Schema.ProposedRemoveQty); }
		}

		ZDecimal proposedRemoveQty;

		#endregion

		#region IsFullyPacked

		public bool IsFullyPacked
		{
			get { return PackedQty >= PackableItemParent.TotalQty; }
		}

		#endregion

		#endregion

		#region CurrentBarcodeMatch

		public BarcodeMatch CurrentBarcodeMatch
		{
			get
			{
				if (currentBarcodeMatch == null)
				{
					currentBarcodeMatch = CurrentBarcode.IsEmpty ? BarcodeMatch.No : PackableItemParent.IsMatch(CurrentBarcode);
				}
				return currentBarcodeMatch ?? (currentBarcodeMatch = BarcodeMatch.No);
			}
		}

		public ZString CurrentBarcode
		{
			get { return currentBarcode; }
			set
			{
				bool barcodeChanged = !value.EqualsIgnoringCase(CurrentBarcode);
				if (barcodeChanged)
				{
					currentBarcodeMatch = null;
				}
				currentBarcode = value;
			}
		}

		BarcodeMatch currentBarcodeMatch;
		ZString currentBarcode;

		#endregion

		#region Validation

		public override PackableItemParentWrapperEmptyValidation GetNewValidation()
		{
			return IsValidationDisabled ? new PackableItemParentWrapperEmptyValidation(this) : new PackableItemParentWrapperValidation(this);
		}

		public void SetIsValidationDisabled(bool value)
		{
			IsValidationDisabled = value;
		}
		bool IsValidationDisabled;

		#endregion

		#region ICustomPropertyContainer

		/// <summary>
		/// We need to proxy GetValue because the grid type is PackableItemWrapper whereas the
		/// implementor of ICustomPropertyContainer is an IPackable Item (e.g. WhsOrderLine).
		/// 
		/// Using OrderLine as an example -- without this proxy, WhsOrderLine.GetValue(PackableItemWrapper)
		/// is called, however we need WhsOrderLine.GetValue(OrderLine).
		/// </summary>
		IEnumerable<ICustomProperty> ICustomPropertyContainer.CustomProperties
		{
			get { return customProperties ?? (customProperties = PkgPackageItemDivotCustomPropertiesHelper.GetCustomProperties<PackableItemParentWrapper>(PackableItemParent)); }
		}

		IEnumerable<ICustomProperty> customProperties;

		#endregion
	}
}

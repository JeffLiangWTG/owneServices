using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Packing.Business
{
	public abstract class ItemsBusinessObject : NonPersistentBusinessObject
	{
		protected ItemsBusinessObject(PkgPackageJob packageJob)
			: base(packageJob.Factory)
		{
			PackageJob = packageJob;
		}

		#region Related Entities

		protected readonly PkgPackageJob PackageJob;

		#region PackableItemParentsForBinding

		public PackableItemParentWrapperCollection PackableItemParentsForBinding
		{
			get
			{
				if (packableItemParentsForBinding == null)
				{
					packableItemParentsForBinding = new PackableItemParentWrapperCollection(Factory, GetNewPackableItemParentsForBinding());
					RegisterEditableChildObject(packableItemParentsForBinding);
				}
				else if (refreshCollectionOnNextAccess)
				{
					using (GetValidationSuspender())
					using (packableItemParentsForBinding.SuspendListChanged())
					{
						packableItemParentsForBinding.RemoveAll();
						packableItemParentsForBinding.AddRange(GetNewPackableItemParentsForBinding());
					}
				}
				refreshCollectionOnNextAccess = false;

				return packableItemParentsForBinding;
			}
		}

		public void RefreshPackableItemParentsForBinding()
		{
			refreshCollectionOnNextAccess = true;
			BuildPackableItems();
		}

		protected int BuildPackableItems() => PackableItemParentsForBinding.Count;

		bool refreshCollectionOnNextAccess;
		protected abstract IEnumerable<PackableItemParentWrapper> GetNewPackableItemParentsForBinding();
		PackableItemParentWrapperCollection packableItemParentsForBinding;

		#endregion

		#endregion

		#region Properties

		#region PackageQtyToCreate

		public ZInt PackageQtyToCreate
		{
			get { return packageQtyToCreate; }
			set
			{
				bool valueChanged = SetNonPersistentPropertyValue(PackageQtyToCreateInfo, ref packageQtyToCreate, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePackageQtyToCreate();
				}

				if (valueChanged)
				{
					OnPackageQtyToCreateChanged();
				}
			}
		}

		public ZPropertyInfoInt PackageQtyToCreateInfo
		{
			get { return (ZPropertyInfoInt)GetZPropertyInfo(nameof(PackageQtyToCreate)); }
		}

		protected virtual void OnPackageQtyToCreateChanged()
		{
		}

		public const int MaxPackagesToCreate = 100;
		ZInt packageQtyToCreate = 1;

		#endregion

		#region PackageTypeToCreate

		[List("PackageTypes")]
		[MaxLength(3)]
		public ZString PackageTypeToCreate
		{
			get
			{
				if (!IsPackageTypeDefaulted)
				{
					IsPackageTypeDefaulted = true;
					packageTypeToCreate = DefaultPackageTypeToCreate;
				}
				return packageTypeToCreate;
			}
			set
			{
				IsPackageTypeDefaulted = true;
				SetNonPersistentPropertyValue(PackageTypeToCreateInfo, ref packageTypeToCreate, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePackageTypeToCreate();
				}
			}
		}

		public ZPropertyInfoString PackageTypeToCreateInfo
		{
			get { return (ZPropertyInfoString)GetZPropertyInfo(nameof(PackageTypeToCreate)); }
		}

		ZString packageTypeToCreate;
		bool IsPackageTypeDefaulted;

		protected abstract ZString DefaultPackageTypeToCreate { get; }

		#endregion

		#endregion

		#region Flags

		#region CanAutoApplyChanges

		public bool CanAutoApplyChanges
		{
			get { return CanAutoApplyChangesCore; }
		}

		protected virtual bool CanAutoApplyChangesCore
		{
			get { return false; }
		}

		#endregion

		#region CanScanPackQuanity

		public bool CanScanPackQuanity
		{
			get { return CanScanPackQuanityCore(); }
		}

		protected virtual bool CanScanPackQuanityCore()
		{
			return false;
		}

		#endregion

		#region HasItemsWithQtyGreatherThanZero

		public bool HasItemsWithQtyGreatherThanZero
		{
			get { return PackableItemParentsForBinding.Any<PackableItemParentWrapper>(w => (ZDecimal)w[ProposedPackUnpackColumnName] > 0m); }
		}

		#endregion

		#region IsAnythingSelectedToPackOrUnpack

		public bool IsAnythingSelectedToPackOrUnpack
		{
			get { return IsAnythingSelectedToPackOrUnpackCore; }
		}

		protected virtual bool IsAnythingSelectedToPackOrUnpackCore
		{
			get { return HasItemsWithQtyGreatherThanZero; }
		}

		#endregion

		#endregion

		#region Lookups

		public RefPackTypeCollection PackageTypes => PkgPackageLookups.GetPackTypes(Factory, PackageJob);

		#endregion

		#region Validation

		public ItemsBusinessObjectValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual ItemsBusinessObjectValidation GetNewValidation()
		{
			return new ItemsBusinessObjectValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region RunValidationAndApplyChanges

		public bool RunValidationAndApplyChanges()
		{
			RunPreSaveValidation();

			if (!HasErrors)
			{
				ApplyChanges();
			}

			return !HasErrors;
		}

		protected abstract void ApplyChanges();

		#endregion

		#region SetPackOrRemoveQuantity

		public abstract void SetPackOrRemoveQuantity(PackableItemParentWrapper wrapper, decimal quantity);

		#endregion

		#region Column/Desc

		public ZString ProposedPackUnpackDescription
		{
			get { return ProposedPackUnpackDescriptionCore; }
		}

		public ZString ProposedPackUnpackColumnName
		{
			get { return ProposedPackUnpackColumnNameCore; }
		}

		protected abstract ZString ProposedPackUnpackDescriptionCore { get; }
		protected abstract ZString ProposedPackUnpackColumnNameCore { get; }

		#endregion

		#region Mode

		public PackOrUnpackMode Mode
		{
			get { return modeCore ?? (modeCore = GetMode()).Value; }
		}

		protected virtual PackOrUnpackMode GetMode()
		{
			return PackOrUnpackMode.SingleItemNoQty;
		}

		PackOrUnpackMode? modeCore;

		#endregion
	}

	public enum PackOrUnpackMode
	{
		SingleItemNoQty,
		TUN_Qty_Attribs,
		TUN_Qty,
		Qty,
		Attribs
	}
}

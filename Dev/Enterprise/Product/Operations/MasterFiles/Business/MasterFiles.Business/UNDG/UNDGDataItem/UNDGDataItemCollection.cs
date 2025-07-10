using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGDataItemCollection : UNDGDataItemCollection<UNDGDataItem>
	{
		public UNDGDataItemCollection(IUNDGDataItemProvider master, params OrgHeader[] contactSourceOrganisations)
			: base(master, contactSourceOrganisations)
		{
		}

		protected UNDGDataItemCollection(IUNDGDataItemProvider master, ZQuery itemFilter)
			: base(master, itemFilter)
		{
		}

		public ZString AsString
		{
			get => Factory.GetValue(ref asStringCached, () =>
			{
				return string.Join(",", this.Select(x => x.Substance?.DG_Code ?? ZString.Empty));
			});
		}

		CachedProperty<ZString> asStringCached;
	}

	public abstract class UNDGDataItemCollection<T> : ActiveBusinessObjectCollection<T>
		where T : UNDGDataItem
	{
		protected UNDGDataItemCollection(IUNDGDataItemProvider master, params OrgHeader[] contactSourceOrganisations)
			: base(master.Factory, new DependentRelationship((BusinessObject)master, typeof(T), new ZQuery(), UNDGDataItemSchema.DI_ParentID))
		{
			this.ContactSourceOrganisations = contactSourceOrganisations;
		}

		protected UNDGDataItemCollection(IUNDGDataItemProvider master)
			: this(master, contactSourceOrganisations: null)
		{
		}

		protected UNDGDataItemCollection(IUNDGDataItemProvider master, ZQuery itemFilter)
			: base(master.Factory, itemFilter)
		{
		}

		readonly OrgHeader[] ContactSourceOrganisations;

		protected override void SetReadOnlyIncludingChildrenCore(bool readOnly)
		{
			base.SetReadOnlyIncludingChildrenCore(readOnly);

			if (bindingItems != null)
			{
				bindingItems.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgSubstanceManager != null)
			{
				undgSubstanceManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgSubstanceManagerGuid != null)
			{
				undgSubstanceManagerGuid.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgIsCombustibleManager != null)
			{
				undgIsCombustibleManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgFlashPointManager != null)
			{
				undgFlashPointManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgContactManager != null)
			{
				undgContactManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgDGManager != null)
			{
				undgDGManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgClassManager != null)
			{
				undgClassManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgMarinePollutantManager != null)
			{
				undgMarinePollutantManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgVolumeManager != null)
			{
				undgVolumeManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgVolumeUnitManager != null)
			{
				undgVolumeUnitManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgWeightManager != null)
			{
				undgWeightManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgWeightUnitManager != null)
			{
				undgWeightUnitManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgTechnicalNameManager != null)
			{
				undgTechnicalNameManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgHasOverpackManager != null)
			{
				undgHasOverpackManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgOverpackIDManager != null)
			{
				undgOverpackIDManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgSubstanceExceptedQuantityManager != null)
			{
				undgSubstanceExceptedQuantityManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgPSAGroupsManager != null)
			{
				undgPSAGroupsManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (undgIsLimitedQuantityManager != null)
			{
				undgIsLimitedQuantityManager.SetReadOnlyIncludingChildren(readOnly);
			}
		}

		#region Binding Item

		/// <summary>
		/// This is used to allow binding to this item on a panel (not grid). 
		/// This item will automatically translate into a real item when data is entered.
		/// </summary>
		public UNDGDataItemStandAloneCollection FirstItemForBinding
		{
			get
			{
				if (Count > 0)
				{
					this[0].SubstanceOrClassUpdated -= UNDGDataItemCollection_SubstanceOrClassUpdated;
					BindingItems.AddIfNotExists(this[0]);
					BindingItems.RemoveAllExceptNewItem(this[0]);
				}
				else
				{
					var autoAddedItem = BindingItems.Count > 0 ? BindingItems[0] : null;

					if (BindingItems.Count > 0)
					{
						BindingItems[0].SubstanceOrClassUpdated -= UNDGDataItemCollection_SubstanceOrClassUpdated;
					}

					if (autoAddedItem == null || autoAddedItem.IsDeleted)
					{
						autoAddedItem = BindingItems.AddNew();
						BindingItems.RemoveAllExceptNewItem(autoAddedItem);
					}

					autoAddedItem.IsAutoAddedItem = true;
					autoAddedItem.AutoAddedItemNowValid += new EventHandler(autoAddedItem_AutoAddedItemNowValid);
				}

				BindingItems[0].SubstanceOrClassUpdated += UNDGDataItemCollection_SubstanceOrClassUpdated;

				return BindingItems;
			}
		}

		void UNDGDataItemCollection_SubstanceOrClassUpdated(object sender, EventArgs e)
		{
			var item = FirstItemForBinding[0];
			if (!item.IsSettingHasChangesSuspended)
			{
				if (Count == 1 && !item.IsAutoAddedItem && item.DI_DG.IsEmpty && item.DI_IMOClass.IsEmpty)
				{
					item.SubstanceOrClassUpdated -= UNDGDataItemCollection_SubstanceOrClassUpdated;
					DeleteAll();
				}

				FirstItemForBinding.RefreshBinding();
			}
		}

		void autoAddedItem_AutoAddedItemNowValid(object sender, EventArgs e)
		{
			var dgItem = (T)sender;

			dgItem.AutoAddedItemNowValid -= autoAddedItem_AutoAddedItemNowValid;

			if (dgItem.IsAutoAddedItem && !dgItem.IsDeleted)
			{
				dgItem.IsAutoAddedItem = false;
				Add(dgItem);
			}
		}

		UNDGDataItemStandAloneCollection BindingItems
		{
			get
			{
				if (bindingItems == null)
				{
					bindingItems = GetNewStandaloneCollection();
					CountChanged += delegate
					{
						((IActiveBusinessObjectCollection)FirstItemForBinding).FireListResetEvent();
						FirstItemForBinding.RefreshBinding();
					};

					if (ReadOnly)
					{
						bindingItems.SetReadOnlyIncludingChildren(true);
					}
				}
				return bindingItems;
			}
		}
		UNDGDataItemStandAloneCollection bindingItems;

		protected virtual UNDGDataItemStandAloneCollection GetNewStandaloneCollection()
		{
			return new UNDGDataItemStandAloneCollection(Factory, typeof(T));
		}

		public class UNDGDataItemStandAloneCollection : ActiveBusinessObjectCollection<T>
		{
			public UNDGDataItemStandAloneCollection(BusinessObjectFactory factory, Type type)
				: base(factory, new AdhocCollectionRelationship(type))
			{
			}

			public void AddIfNotExists(T newItem)
			{
				Argument.NotNull(newItem, nameof(newItem));

				if (!Contains(newItem))
				{
					Add(newItem);
				}
			}

			public void RemoveAllExceptNewItem(T newItem)
			{
				Argument.NotNull(newItem, nameof(newItem));

				foreach (var existingItem in this.Except(new[] { newItem }))
				{
					RemoveFromRelationship(existingItem);
				}
			}
		}

		#endregion

		#region Retrieve/Create

		public T TryGetOrCreate(ZString uNDGCode, ZString standard)
		{
			uNDGCode = uNDGCode.SubstringSafe(0, UNDGSubstanceSchema.DG_Code.MaxLength);
			standard = standard.IsEmpty ? (ZString)UNDGSubstanceStandardTypes.IMO : standard;
			var unno = uNDGCode.SubstringSafe(0, 4);
			var variant = uNDGCode.SubstringSafe(4, 2);

			foreach (T item in this)
			{
				if (item.Substance != null && item.Substance.DG_UNNO == unno && item.Substance.DG_Variant == variant && item.Substance.DG_Standard == standard)
				{
					return item;
				}
			}

			var undgs = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, standard);
			var subs = undgs != null && undgs.Count() == 1 ? undgs.FirstOrDefault() : null;
			if (subs == null)
			{
				return null;
			}
			T newItem = AddNew();

			var pivot = newItem.UNDGSubstancePivotCollection.AddNew();
			pivot.DP_UNNO = subs.DG_UNNO;
			pivot.DP_Variant = subs.DG_Variant;
			pivot.DP_Standard = subs.DG_Standard;
			pivot.DP_IsDefault = true;
			newItem.DI_DG = subs.PK;
			return newItem;
		}

		#endregion

		#region Multiple Item Managers

		#region UNDGSubstanceManager

		public UNDGMultipleItemManager<T> UNDGSubstanceManagerGuid
		{
			get
			{
				if (undgSubstanceManagerGuid == null)
				{
					undgSubstanceManagerGuid = new UNDGMultipleItemManager<T>(this, Factory);
					if (ReadOnly)
					{
						undgSubstanceManagerGuid.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgSubstanceManagerGuid;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		UNDGMultipleItemManager<T> undgSubstanceManagerGuid;

		public MultipleItemManager UNDGSubstanceManager
		{
			get
			{
				if (undgSubstanceManager == null)
				{
					undgSubstanceManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_DG_NKSubs, true);
					if (ReadOnly)
					{
						undgSubstanceManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgSubstanceManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgSubstanceManager;

		#endregion

		#region UNDGIsCombustibleManager

		public MultipleItemManager UNDGIsCombustibleManager
		{
			get
			{
				if (undgIsCombustibleManager == null)
				{
					undgIsCombustibleManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_IsCombustible, false);
					if (ReadOnly)
					{
						undgIsCombustibleManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgIsCombustibleManager;
			}
		}

		MultipleItemManager undgIsCombustibleManager;

		#endregion

		#region UNDGFlashPointManager

		public MultipleItemManager UNDGFlashPointManager
		{
			get
			{
				if (undgFlashPointManager == null)
				{
					undgFlashPointManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_DGFlashPoint, false);
					if (ReadOnly)
					{
						undgFlashPointManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgFlashPointManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgFlashPointManager;

		#endregion

		#region UNDGSubstanceExceptedQuantityManager

		public MultipleNonPersistentItemManager UNDGSubstanceExceptedQuantityManager
		{
			get
			{
				if (undgSubstanceExceptedQuantityManager == null)
				{
					undgSubstanceExceptedQuantityManager = new MultipleNonPersistentItemManager(this, UNDGDataItem.Schema.ExceptedQuantity);
					undgSubstanceExceptedQuantityManager.SetReadOnlyIncludingChildren(true);
				}

				return undgSubstanceExceptedQuantityManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleNonPersistentItemManager undgSubstanceExceptedQuantityManager;

		#endregion

		#region UNDGPSAGroupsManager

		public MultipleNonPersistentItemManager UNDGPSAGroupsManager
		{
			get
			{
				if (undgPSAGroupsManager == null)
				{
					undgPSAGroupsManager = new MultipleNonPersistentItemManager(this, UNDGDataItem.Schema.PSAGroup);
					undgPSAGroupsManager.SetReadOnlyIncludingChildren(true);
				}

				return undgPSAGroupsManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleNonPersistentItemManager undgPSAGroupsManager;

		#endregion

		#region UNDGContactManager

		public MultipleItemManagerGUID<OrgContact> UNDGContactManager
		{
			get
			{
				if (undgContactManager == null)
				{
					undgContactManager = new MultipleItemManagerGUID<OrgContact>(this, UNDGDataItemSchema.DI_OC_DGContact, Factory);
					if (ReadOnly)
					{
						undgContactManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgContactManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManagerGUID<OrgContact> undgContactManager;

		#endregion

		#region UNDGDGManager

		public MultipleItemManagerGUID<UNDGSubstance> UNDGDGManager
		{
			get
			{
				if (undgDGManager == null)
				{
					undgDGManager = new MultipleItemManagerGUID<UNDGSubstance>(this, UNDGDataItemSchema.DI_DG, Factory);
					if (ReadOnly)
					{
						undgDGManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgDGManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManagerGUID<UNDGSubstance> undgDGManager;

		#endregion

		#region UNDGClassManager

		public MultipleItemManager UNDGClassManager
		{
			get
			{
				if (undgClassManager == null)
				{
					undgClassManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_IMOClass, true, true);
					if (ReadOnly)
					{
						undgClassManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgClassManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgClassManager;

		#endregion

		#region UNDGWeightManager

		public MultipleItemManager UNDGWeightManager
		{
			get
			{
				if (undgWeightManager == null)
				{
					undgWeightManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_DGWeight, false);
					if (ReadOnly)
					{
						undgWeightManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgWeightManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgWeightManager;

		#endregion

		#region UNDGWeightUnitManager

		public MultipleItemManager UNDGWeightUnitManager
		{
			get
			{
				if (undgWeightUnitManager == null)
				{
					undgWeightUnitManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_UnitOfWeight, true, true);
					if (ReadOnly)
					{
						undgWeightUnitManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgWeightUnitManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgWeightUnitManager;

		#endregion

		#region UNDGVolumeManager

		public MultipleItemManager UNDGVolumeManager
		{
			get
			{
				if (undgVolumeManager == null)
				{
					undgVolumeManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_DGVolume, false);
					if (ReadOnly)
					{
						undgVolumeManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgVolumeManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgVolumeManager;

		#endregion

		#region UNDGVolumeUnitManager

		public MultipleItemManager UNDGVolumeUnitManager
		{
			get
			{
				if (undgVolumeUnitManager == null)
				{
					undgVolumeUnitManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_UnitOfVolume, true, true);
					if (ReadOnly)
					{
						undgVolumeUnitManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgVolumeUnitManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgVolumeUnitManager;

		#endregion

		#region UNDGMarinePollutantManager

		public MultipleItemManager UNDGMarinePollutantManager
		{
			get
			{
				if (undgMarinePollutantManager == null)
				{
					undgMarinePollutantManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_MPMarinePollutant, false);
					if (ReadOnly)
					{
						undgMarinePollutantManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgMarinePollutantManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgMarinePollutantManager;

		#endregion

		#region UNDGProperShippingNameManager

		public MultipleNonPersistentItemManager UNDGProperShippingNameManager
		{
			get
			{
				if (undgProperShippingNameManager == null)
				{
					undgProperShippingNameManager = new MultipleNonPersistentItemManager(this, "ProperShippingName");
					if (ReadOnly)
					{
						undgProperShippingNameManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgProperShippingNameManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleNonPersistentItemManager undgProperShippingNameManager;

		#endregion

		#region UNDGHazardousWasteCodeManager

		public MultipleItemManager UNDGHazardousWasteCodeManager
		{
			get
			{
				if (undgHazardousWasteCodeManager == null)
				{
					undgHazardousWasteCodeManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_HazardousWasteCode, false);
					if (ReadOnly)
					{
						undgHazardousWasteCodeManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgHazardousWasteCodeManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgHazardousWasteCodeManager;

		#endregion

		#region UNDGSpecialPermitIssueDateManager

		public MultipleItemManager UNDGSpecialPermitIssueDateManager
		{
			get
			{
				if (undgSpecialPermitIssueDateManager == null)
				{
					undgSpecialPermitIssueDateManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_SpecialPermitIssueDate, false);
					if (ReadOnly)
					{
						undgSpecialPermitIssueDateManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgSpecialPermitIssueDateManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgSpecialPermitIssueDateManager;

		#endregion

		#region UNDGSpecialPermitNumberManager

		public MultipleItemManager UNDGSpecialPermitNumberManager
		{
			get
			{
				if (undgSpecialPermitNumberManager == null)
				{
					undgSpecialPermitNumberManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_SpecialPermitNumber, false);
					if (ReadOnly)
					{
						undgSpecialPermitNumberManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgSpecialPermitNumberManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgSpecialPermitNumberManager;

		#endregion

		#region UNDGIsSalvagePackagingManager

		public MultipleItemManager UNDGIsSalvagePackagingManager
		{
			get
			{
				if (undgIsSalvagePackagingManager == null)
				{
					undgIsSalvagePackagingManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_IsSalvagePackaging, false);
					if (ReadOnly)
					{
						undgIsSalvagePackagingManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgIsSalvagePackagingManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgIsSalvagePackagingManager;

		#endregion

		#region UNDGIsResidueLastContainedManager

		public MultipleItemManager UNDGIsResidueLastContainedManager
		{
			get
			{
				if (undgIsResidueLastContainedManager == null)
				{
					undgIsResidueLastContainedManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_IsResidueLastContained, false);
					if (ReadOnly)
					{
						undgIsResidueLastContainedManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgIsResidueLastContainedManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgIsResidueLastContainedManager;

		#endregion

		#region UNDGTechnicalNameManager

		public MultipleItemManager UNDGTechnicalNameManager
		{
			get
			{
				if (undgTechnicalNameManager == null)
				{
					undgTechnicalNameManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_TechnicalName, false);
					if (ReadOnly)
					{
						undgTechnicalNameManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgTechnicalNameManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgTechnicalNameManager;

		#endregion

		#region UNDGHasOverpackManager

		public MultipleItemManager UNDGHasOverpackManager
		{
			get
			{
				if (undgHasOverpackManager == null)
				{
					undgHasOverpackManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_HasOverpack, false);
					if (ReadOnly)
					{
						undgHasOverpackManager.SetReadOnlyIncludingChildren(true);
					}

					undgHasOverpackManager.ValueInfo.ValueChanged += UNDGHasOverpackManagerValueInfo_ValueChanged;
				}

				return undgHasOverpackManager;
			}
		}

		void UNDGHasOverpackManagerValueInfo_ValueChanged(object sender, EventArgs e)
		{
			var value = ZBool.False;
			if (ZBool.TryParse(UNDGHasOverpackManager.Value, out value) && !value)
			{
				UNDGOverpackIDManager.Value = ZString.Empty;
			}

			UNDGOverpackIDManager.ValueInfo.RefreshBinding();
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgHasOverpackManager;

		#endregion

		#region UNDGOverpackIDManager

		public MultipleItemManager UNDGOverpackIDManager
		{
			get
			{
				if (undgOverpackIDManager == null)
				{
					undgOverpackIDManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_OverpackID, false);
					if (ReadOnly)
					{
						undgOverpackIDManager.SetReadOnlyIncludingChildren(true);
					}

					undgOverpackIDManager.ReadOnlyFunction = GetUNDGOverpackIDManager_ValueReadOnly;
				}

				return undgOverpackIDManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgOverpackIDManager;

		bool GetUNDGOverpackIDManager_ValueReadOnly()
		{
			var hasOverpack = ZBool.False;
			if (!ZBool.TryParse(UNDGHasOverpackManager.Value, out hasOverpack))
			{
				return true;
			}

			return !hasOverpack;
		}

		#endregion

		#region UNDGIsLimitedQuantityManager

		public MultipleItemManager UNDGIsLimitedQuantityManager
		{
			get
			{
				if (undgIsLimitedQuantityManager == null)
				{
					undgIsLimitedQuantityManager = new MultipleItemManager(this, UNDGDataItemSchema.DI_IsLimitedQuantity, false);
					if (ReadOnly)
					{
						undgIsLimitedQuantityManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return undgIsLimitedQuantityManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager undgIsLimitedQuantityManager;

		#endregion

		#endregion

		#region Lookups

		#region UNDGSubstances

		public virtual UNDGSubstanceCollection UNDGSubstances
		{
			get { return new UNDGSubstanceCollection(Factory); }
		}

		public virtual UNDGSubstanceCollection AllUNDGSubstances
		{
			get { return fAllUNDGSubstances ?? (fAllUNDGSubstances = new UNDGSubstanceCollection(Factory, new ZQuery())); }
		}
		UNDGSubstanceCollection fAllUNDGSubstances;

		#endregion

		#region Contacts

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public OrgContactCollection Contacts
		{
			get
			{
				ZQuery query = new ZQuery();

				if (ContactSourceOrganisations != null)
				{
					foreach (OrgHeader organisation in ContactSourceOrganisations)
					{
						if (organisation != null)
						{
							query.AddToFilter(JoinCondition.Or, OrgContactSchema.OC_OH, SQLComparisonOperator.Equal, organisation.PK);
						}
					}
				}

				return new OrgContactCollection(Factory, query);
			}
		}

		#endregion

		#region DGClassList

		public CodeDescriptionPairList DGClassList
		{
			get
			{
				if (dgClassList == null)
				{
					dgClassList = UNDGDataItemLookups.GetDGClassList(Factory);
				}

				return dgClassList;
			}
		}
		CodeDescriptionPairList dgClassList;

		#endregion

		#region VolumeUnits

		public CodeDescriptionPairList VolumeUnits
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region WeightUnits

		public CodeDescriptionPairList WeightUnits
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#endregion
	}
}

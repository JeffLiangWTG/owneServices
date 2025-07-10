using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.Packing.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Packing.Business
{
	[DebuggerDisplay("PackType={KP_F3_NKPackType}")]
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.PkgPackage)]
	[CodeProperty(nameof(KP_PackageID))]
	[DeferTriggerAndRunBeforeCommit("TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit", "PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit", PkgPackageSchema.Constants.PK, typeof(IPkgPackageEnsureChildPackagesHaveSameTopLevelHandlingUnitStrategy_PkgPackageInsertUpdate))]
	public class PkgPackage : AutoPkgPackage,
		IDocumentSupportable,
		IPackageParent,
		IPackageSummary,
		IPackingHasChanges,
		IPkgPackage,
		IPackageSequence,
		IHavePortReferences,
		IProcessHandlingInfoProvider,
		ITemperatureSettings,
		IUNDGDataItemProvider,
		ISupportDataImporting,
		IJobNumber,
		IEDocsProvider,
		ISupportPackageIDGenerationInternals,
		IHaveCusEntryNumReferences
	{
		public PkgPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public abstract new class Schema : AutoPkgPackage.Schema
		{
			public const string CartonGroupAndSize = "CartonGroupAndSize";
			public const string IsSentToRTUS = nameof(IsSentToRTUS);
			public const string RTUSBookedType = nameof(RTUSBookedType);
			public const string Container = "Container";
			public const string UNDGs = "UNDGs";
			public const string KP_PackageID = "KP_PackageID";
			public const string PackageStatus = "PackageStatus";
			public const string LabelPrinted = "LabelPrinted";
			public const string RTUSLabelPrinterPK = nameof(RTUSLabelPrinterPK);

			public const string ContainerDunnageWeight = "ContainerDunnageWeight";
			public const string ContainerTareWeight = "ContainerTareWeight";
			public const string ContainerGoodsWeight = "ContainerGoodsWeight";

			public const int RTUSBookedTypeMaxLength = 3;
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var defaultUnits = (IPackageDefaultUQs)new PkgDefaultUnits();
			var row = ((IBusinessObjectInternals)this).Row;

			// in case of bad data in registry 
			var defaultWeightUnit = defaultUnits.DefaultWeightUnit;
			var defaultVolumeUnit = defaultUnits.DefaultVolumeUnit;
			var defaultDimensionUnit = defaultUnits.DefaultDimensionUnit;

			CheckMaximumLength(KP_WeightUQInfo, defaultWeightUnit);
			CheckMaximumLength(KP_VolumeUQInfo, defaultVolumeUnit);
			CheckMaximumLength(KP_DimensionUQInfo, defaultDimensionUnit);

			row[PkgPackageSchema.Constants.KP_PackageQty] = 1;
			row[PkgPackageSchema.Constants.KP_WeightUQ] = defaultWeightUnit;
			row[PkgPackageSchema.Constants.KP_VolumeUQ] = defaultVolumeUnit;
			row[PkgPackageSchema.Constants.KP_DimensionUQ] = defaultDimensionUnit;
		}

		#endregion

		public static readonly PkgPackageTypeDecider TypeDecider = new PkgPackageTypeDecider();

		#region Related Entities

		#region Container

		public PkgPackageContainer Container
		{
			get
			{
				PkgPackageContainer result = null;

				if (IsContainer)
				{
					result = Factory.LoadFromUniqueKey<PkgPackageContainer>(PkgPackageContainerSchema.K0_KP_Package, PK);
					if (result == null && !IsDeletingContainerSemaphore.IsSuspended)
					{
						result = Factory.New<PkgPackageContainer>();
						result.K0_KP_Package = PK;
					}
					RegisterEditableChildObject(result);
				}

				return result;
			}
		}

		#endregion

		#region OuterPackage

		public PkgPackage OuterPackage
		{
			get { return IsOuter ? this : ParentPackage.OuterPackage; }
		}

		#endregion

		#region PackageJob

		public virtual PkgPackageJob PackageJob
		{
			get { return Factory.Load<PkgPackageJob>(KP_KJ_ParentPackageJob); }
		}

		#endregion

		#region Packages

		/// <summary>
		/// Returns a collection of PkgPackages that are packed into this PkgPackage.
		/// </summary>
		[ChildEditable(true)]
		public PkgPackageCollection Packages
		{
			get
			{
				if (packages == null)
				{
					packages = GetPackageCollectionCore();
					RegisterEditableChildObject(packages);
				}
				return packages;
			}
		}

		public ReadOnlyCollection<PkgPackage> GetAllPackages() // tested by PackageJob.GetAllPackages() test
		{
			return GetAllPackages(Packages).AsReadOnly();
		}

		List<PkgPackage> GetAllPackages(PkgPackageCollection packageCollection)
		{
			var result = new List<PkgPackage>();

			foreach (var package in packageCollection)
			{
				result.Add(package);
				result.AddRange(GetAllPackages(package.Packages));
			}

			return result;
		}

		PkgPackageCollection packages;

		protected virtual PkgPackageCollection GetPackageCollectionCore() => new PkgPackageCollection(this);

		#endregion

		#region KP_IsUnknownQty_Hidden

		[ActionField(ReadOnly = true, FieldType = ActionFieldType.Hidden)]
		[DocumentEngineIntegration.DocumentParsing.DocumentFieldExcludeFromMap]
		[WorkflowSetFieldReadonly]
		[ReadOnly(true)]
		public override ZBool KP_IsUnknownQty
		{
			get { return base.KP_IsUnknownQty; }
			set { base.KP_IsUnknownQty = value; }
		}

		#endregion KP_IsUnknownQty_Hidden

		#region PackageExtensions

		public PkgPackageExtensionCollection PackageExtensions
		{
			get
			{
				if (packageExtensions == null)
				{
					packageExtensions = new PkgPackageExtensionCollection(this);
				}
				return packageExtensions;
			}
		}

		PkgPackageExtensionCollection packageExtensions;

		#endregion

		#region PackedItemDivots

		/// <summary>
		/// Returns a collection of items that are packed into this PkgPackage.
		/// </summary>
		[ChildEditable(true)]
		public PkgPackageItemDivotCollection PackedItemDivots
		{
			get
			{
				if (packedItemDivots == null)
				{
					packedItemDivots = GetPackedItemDivotsCollection();
					RegisterEditableChildObject(packedItemDivots);
				}
				return packedItemDivots;
			}
		}

		PkgPackageItemDivotCollection packedItemDivots;

		protected virtual PkgPackageItemDivotCollection GetPackedItemDivotsCollection() => new PkgPackageItemDivotCollection(this);

		#endregion

		#region PackedItems

		public IPackedItemsCollection PackedItems
		{
			get
			{
				if (packedItems == null)
				{
					packedItems = new PkgPackageItemDivotsWrapperCollection(Factory);
					packedItems.AddRange(GetPackedItems());

					// Items may have been packed before this collection was built (stored in this cache).
					// GetPackedItems() above will have added these cached items to PackedItems, now clear out the cache.
					cacheForPackedItemsBeforeCollectionBound?.Clear();
					cacheForPackedItemsBeforeCollectionBound = null;

					PackedItemDivots.CollectionCountChange += PackedItemDivots_CollectionCountChange;
				}

				return packedItems;
			}
		}

		IEnumerable<PkgPackageItemDivotsWrapper> GetPackedItems()
		{
			if (IsDeleted)
			{
				yield break;
			}

			var wrappers = new Lazy<IEnumerable<PkgPackageItemDivotsWrapper>>(() => PackedItemDivots.GetDivotWrappers());

			foreach (var divot in PackedItemDivots)
			{
				yield return GetPackedItem(divot, wrappers);
			}
		}

		PkgPackageItemDivotsWrapper GetPackedItem(PkgPackageItemDivot divot, Lazy<IEnumerable<PkgPackageItemDivotsWrapper>> wrappers)
		{
			PkgPackageItemDivotsWrapper result;

			if (cacheForPackedItemsBeforeCollectionBound == null ||
				// packed items are cached if they are packed prior to the creation of the PackedItems collection (performant).
				!cacheForPackedItemsBeforeCollectionBound.TryGetValue(PackedItemKey.GetKey(divot), out result))
			{
				var packedItem = divot.PackedItem;
				if (packedItem != null)
				{
					var key = packedItem.Key;
					result = wrappers.Value.Single(w => w.Key == key);
				}
				else
				{
					result = PkgPackageItemDivotsWrapper.New(divot, EmptyPackableItemParent.Instance);
				}
			}

			return result;
		}

		void PackedItemDivots_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsDeleted && !PackedItemDivotsCountChangedSemaphore.IsSuspended && IsPackedItemsOutOfSync()) // only rebuild if we are out of sync
			{
				RemoveDuplicateDivots();

				PackedItems.Typed.ForEach(w => w.Delete_ThrowAwayWrapperButDontDeleteDivots());
				PackedItems.RemoveAll();
				PackedItems.AddRange(GetPackedItems());

				// we need to update the packing GUI if the packed items have changed and the GUI isn't being updated manually
				PackedItemDivotsCountChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		bool IsPackedItemsOutOfSync()
		{
			var packableItemsLookUp = PackedItems.Typed.SelectMany(d => d.PackedItems.Select(p => new { d.Key, Item = p })).ToLookup(p => p.Key, p => p.Item);

			// count check is for both a shortcircuit and to ensure there aren't duplicate divots
			var allItemsCount = packableItemsLookUp.Sum(o => o.Count());
			return allItemsCount != PackedItemDivots.Count
				|| PackedItemDivots.Select(d => d.PackedItem).Any(p => p == null || !packableItemsLookUp[p.Key].Contains(p));
		}

		void RemoveDuplicateDivots()
		{
			// if a user tries to pack the same item in two separate factories and saves one first, data refresh will propagate
			// the new divots saved in the DB to the other factory. In this case we will potentially have the same item packed
			// twice, in which case we have no choice but to simply remove those duplicates.

			using (SuspendPackedItemDivotsCountChanged())
			{
				var packedItemsByParent = new Dictionary<ZGuid, PkgPackageItemDivot>();
				for (int index = PackedItemDivots.Count - 1; index >= 0; index--)
				{
					PkgPackageItemDivot divotInCache;
					var packedItemDivot = PackedItemDivots[index];
					if (packedItemsByParent.TryGetValue(packedItemDivot.KI_ParentID, out divotInCache))
					{
						if (packedItemDivot.IsInDatabase)
						{
							// this means the divot in the cache is not in the database so it must be deleted and the cache overriden
							packedItemsByParent[packedItemDivot.KI_ParentID] = packedItemDivot;
							divotInCache.Delete();
						}
						else
						{
							packedItemDivot.Delete();
						}

						IBusinessObjectCollectionInternals collection = PackedItemDivots;
						if (!collection.HasChangesFromDelete)
						{
							collection.HasChangesFromDelete = true;
						}
					}
					else
					{
						packedItemsByParent[packedItemDivot.KI_ParentID] = packedItemDivot;
					}
				}
			}
		}

		public event EventHandler PackedItemDivotsCountChanged;

		IPackedItemsCollection packedItems;

		internal IDisposable SuspendPackedItemDivotsCountChanged() => new SemaphoreManager(PackedItemDivotsCountChangedSemaphore);

		Semaphore PackedItemDivotsCountChangedSemaphore => packedItemDivotsCountChangedSemaphore ?? (packedItemDivotsCountChangedSemaphore = new Semaphore());
		Semaphore packedItemDivotsCountChangedSemaphore;

		#region PkgPackageItemDivotsWrapperCollection

#if DEBUG
		public
#endif
		class PkgPackageItemDivotsWrapperCollection : NonPersistentBusinessObjectCollection<PkgPackageItemDivotsWrapper>, IPackedItemsCollection
		{
			public PkgPackageItemDivotsWrapperCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotSupportedException();
			}

			protected override bool AllowNewCore => false;

			#region IPackedItemsCollection Members

			IEnumerable<PkgPackageItemDivotsWrapper> IPackedItemsCollection.Typed
			{
				get
				{
					foreach (PkgPackageItemDivotsWrapper packedItem in this)
					{
						yield return packedItem;
					}
				}
			}

			#endregion
		}

		#endregion

		#endregion

		#region PackageHandlingUnitHandlingUnitDivots

		public PkgPackageHandlingUnitHandlingUnitDivotCollection PackageHandlingUnitHandlingUnitDivots
		{
			get
			{
				if (packageHandlingUnitHandlingUnitDivots == null)
				{
					packageHandlingUnitHandlingUnitDivots = GetPackageHandlingUnitHandlingUnitDivotCollection();
				}
				return packageHandlingUnitHandlingUnitDivots;
			}
		}

		PkgPackageHandlingUnitHandlingUnitDivotCollection packageHandlingUnitHandlingUnitDivots;

		protected virtual PkgPackageHandlingUnitHandlingUnitDivotCollection GetPackageHandlingUnitHandlingUnitDivotCollection() => new PkgPackageHandlingUnitHandlingUnitDivotCollection(this);

		#endregion

		#region PackageHandlingUnitPackageDivots

		public PkgPackageHandlingUnitPackageDivotCollection PackageHandlingUnitPackageDivots
		{
			get
			{
				if (packageHandlingUnitPackageDivots == null)
				{
					packageHandlingUnitPackageDivots = GetPackageHandlingUnitPackageDivotCollection();
				}
				return packageHandlingUnitPackageDivots;
			}
		}

		PkgPackageHandlingUnitPackageDivotCollection packageHandlingUnitPackageDivots;

		protected PkgPackageHandlingUnitPackageDivotCollection GetPackageHandlingUnitPackageDivotCollection() => new PkgPackageHandlingUnitPackageDivotCollection(this);

		#endregion

		#region HandlingUnitPackedPackages

		public IEnumerable<PkgPackage> HandlingUnitPackedPackages => PackageHandlingUnitHandlingUnitDivots.Where(d => d.KPD_UnpackedTime == ZDateTimeOffset.Empty).Select(d => d.Package);

		#endregion

		#region ParentPackage

		public PkgPackage ParentPackage
		{
			get { return KP_KP_ParentPackage.IsValid ? Factory.Load<PkgPackage>(KP_KP_ParentPackage) : null; }
		}

		#endregion

		#region TopHandlingUnitPackage

		public PkgPackage TopHandlingUnitPackage
		{
			get { return KP_KP_TopHandlingUnitPackage.IsValid ? Factory.Load<PkgPackage>(KP_KP_TopHandlingUnitPackage) : null; }
		}

		#endregion

		#region PackageID

		// This property/table generally aren't needed at the business layer outside of Packing.
		// I have made it internal to save developers time ("PackageID" intellisense), and decrease the risk of someone using it without a null check.
		internal PkgPackageHeader PackageID
		{
			get
			{
				var packageHeader = KP_KPH_PackageHeader.IsValid ? Factory.Load<PkgPackageHeader>(KP_KPH_PackageHeader) : null;
				if (currentPackageHeaderPK != KP_KPH_PackageHeader)
				{
					currentPackageHeaderPK = KP_KPH_PackageHeader;
					if (packageHeader != null)
					{
						RegisterEditableChildObject(packageHeader);
					}
				}

				return packageHeader;
			}
		}

		ZGuid currentPackageHeaderPK;

		public PkgPackageHeader GetPackageHeader()
		{
			return PackageID;
		}

		#endregion

		#region ScanEventsForBindingOnly

		[ChildEditable(false)]
		public StmALogDependentCollection ScanEventsForBindingOnly
		{
			get { return scanEventsForBindingOnly ?? (scanEventsForBindingOnly = Logs.GetAllLogs()); }
		}

		StmALogDependentCollection scanEventsForBindingOnly;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return KP_PackageID.IsEmpty
					? Res.GetString("5BFD881F-204F-43C5-B658-85B3CBBCEC37", "Package")
					: Res.GetString("EB9C879A-1EE8-4D6D-93CD-10BA4117BE89", "Package {0}", KP_PackageID);
			}
		}

		#endregion

		#region PackageSeals

		public PkgPackageSealCollection PackageSeals
		{
			get
			{
				if (packageSeals == null)
				{
					packageSeals = GetPkgPackageSealCollection();
				}

				return packageSeals;
			}
		}

		PkgPackageSealCollection packageSeals;

		protected PkgPackageSealCollection GetPkgPackageSealCollection() => new PkgPackageSealCollection(this);

		#endregion

		#region PackageOrderReference

		public PkgPackageOrderReference PackageOrderReference => Factory.LoadFromUniqueKey<PkgPackageOrderReference>(PkgPackageOrderReferenceSchema.KPO_KP_Package, PK);

		#endregion

		#region JobServiceLinks

		public JobServiceLinkCollection JobServiceLinks
		{
			get
			{
				if (jobServiceLinks == null)
				{
					jobServiceLinks = new JobServiceLinkCollection(this);
				}
				return jobServiceLinks;
			}
		}
		JobServiceLinkCollection jobServiceLinks;

		#endregion

		#endregion

		#region Properties

		// persistent

		#region KP_KJ_ParentPackageJob

		[RelatedBusinessObject("PackageJob")]
		public override ZGuid KP_KJ_ParentPackageJob
		{
			get { return base.KP_KJ_ParentPackageJob; }
			set
			{
				var previousValue = KP_KJ_ParentPackageJob;
				base.KP_KJ_ParentPackageJob = value;

				if (previousValue != KP_KJ_ParentPackageJob)
				{
					var packageJob = PackageJob;
					// Winzor relies on poking the "Packages" collection when adding an Outer so that Thread Sentry
					// does not blow up. This should be fixed in another work item as this looks like a bug in Winzor.
					// See PackingTreeViewTest.ShouldHighlightLatestWhenAddNewPackage()
					if (!previousValue.IsEmpty || Globals.IsWinzor)
					{
						ClearActionStrategyCacheIncludingChildren(packageJob);
					}

					ClearParentPackageLinkIfAssignedToAnotherPackageJob();
					FireOuterPackageAddedOnPackageJob(packageJob);

					KP_Sequence = 0;
					if (!KP_KJ_ParentPackageJob.IsEmpty)
					{
						GetPackageSequenceCalculator(packageJob)?.Sequence(this);
					}

					// recalculate ShouldPackTrackedPackagesViaDivot when KP_KJ_ParentPackageJob changes
					shouldPackTrackedPackagesViaDivot = null;
				}
			}
		}

		void ClearParentPackageLinkIfAssignedToAnotherPackageJob()
		{
			// if assigned to a *another* job, clear the KP_KP link (we must be making this a top level package)..
			var parentPackage = ParentPackage;
			if (parentPackage != null && parentPackage.KP_KJ_ParentPackageJob != KP_KJ_ParentPackageJob)
			{
				KP_KP_ParentPackage = ZGuid.Empty;
			}
		}

		void FireOuterPackageAddedOnPackageJob(PkgPackageJob packageJob)
		{
			if (IsOuter)
			{
				packageJob?.FireOuterPackageAdded(this);
			}
		}

		#endregion

		#region KP_KP_ParentPackage

		public override ZGuid KP_KP_ParentPackage
		{
			get { return base.KP_KP_ParentPackage; }
			set
			{
				if (value != base.KP_KP_ParentPackage)
				{
					var previousSequence = KP_Sequence;

					// deduct our weight from the previous parent unless parent is tracked via divot
					if (!ShouldPackTrackedPackagesViaDivot && KP_Weight > 0)
					{
						AddWeightToParentPackage(-KP_Weight);
					}

					bool wasPreviouslyAnOuter = IsOuter;
					base.KP_KP_ParentPackage = value;

					var packageJob = PackageJob;
					ClearActionStrategyCacheIncludingChildren(packageJob);

					FireParentPackageChangedOnOuterOnPackageJob(wasPreviouslyAnOuter, packageJob);
					AssignPackageJobLinkIfParentChanged(value);

					// add our weight to the new parent unless parent is tracked via divot
					if (!ShouldPackTrackedPackagesViaDivot && KP_Weight > 0)
					{
						AddWeightToParentPackage(KP_Weight);
					}

					// revalidate pack type (a Container might be moved into/out of a package)
					if (IsContainer)
					{
						Validation.ValidateKP_F3_NKPackType();
						KP_F3_NKPackTypeInfo.RefreshBinding();
					}

					var sequenceCalculator = PackageSequenceCalculator;
					sequenceCalculator?.Sequence(this);
					if (!KP_KP_ParentPackage.IsEmpty)
					{
						sequenceCalculator?.ShiftSequence(previousSequence);
						if (ShouldPackTrackedPackagesViaDivot)
						{
							PkgPackageHandlingUnitDivotHelper.ReplaceParentPackageWithDivotForLabelledPackages(Factory, this);
						}
					}

					// revalidate Released flags (a released package might be moved into/out of a package)
					Validation.ValidateKP_ReleasedTimeUtc();
					Validation.ValidateKP_IsReleasedViaJob();
				}
			}
		}

		void FireParentPackageChangedOnOuterOnPackageJob(bool wasPreviouslyAnOuter, PkgPackageJob packageJob)
		{
			if (wasPreviouslyAnOuter)
			{
				packageJob?.FireParentPackageChangedOnOuter(this);
			}
			else if (!IsDeletingPackage)
			{
				FireOuterPackageAddedOnPackageJob(packageJob);
			}
		}

		void AssignPackageJobLinkIfParentChanged(ZGuid value)
		{
			// if assigned to a package on *another* job, reassign the KP_KJ link also..
			if (!value.IsEmpty)
			{
				var parentPackage = ParentPackage;
				if (parentPackage != null && KP_KJ_ParentPackageJob != parentPackage.KP_KJ_ParentPackageJob)
				{
					KP_KJ_ParentPackageJob = parentPackage.KP_KJ_ParentPackageJob;
				}
			}
		}

		#endregion

		#region KP_PackageID

		[ReadOnlyMember(nameof(PackageIDReadOnly))]
		[MaxLength(PkgPackageHeader.Schema.KPH_PackageIDMaxLength)]
		public ZString KP_PackageID
		{
			get
			{
				var packageID = PackageID;
				return packageID != null ? packageID.KPH_PackageID : ZString.Empty;
			}
			set
			{
				if (!PackageIDHasChanges) // after saving, we want to reset original
				{
					originalPackageID = KP_PackageID;
				}

				SetPackageID(value);

				KP_PackageIDInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateKP_PackageID();
				}

				NotifyParentOfContainerIDChangeIfRequired();
			}
		}

		void SetPackageID(ZString packageIDValue)
		{
			if (!packageIDValue.IsEmpty)
			{
				var packageID = PackageID;
				if (packageID == null)
				{
					packageID = Factory.New<PkgPackageHeader>();
					currentPackageHeaderPK = packageID.PK;
					RegisterEditableChildObject(packageID);

					KP_KPH_PackageHeader = packageID.PK;
				}

				packageID.KPH_PackageID = packageIDValue;
			}
			else
			{
				var packageID = PackageID;
				if (packageID != null)
				{
					DeletePackageID(packageID);
				}
			}
		}

		public ZPropertyInfo KP_PackageIDInfo
		{
			get { return GetZPropertyInfo(Schema.KP_PackageID); }
		}

		void DeletePackageID(PkgPackageHeader packageID)
		{
			if (packageID != null)
			{
				packageID.Delete();
				KP_KPH_PackageHeader = ZGuid.Empty;
			}
		}

		void NotifyParentOfContainerIDChangeIfRequired()
		{
			var packageJob = PackageJob;
			if (packageJob != null && packageJob.IsInDatabase)
			{
				var parentJob = packageJob.ParentJob;
				if (parentJob != null && IsContainer)
				{
					if ((!IsInDatabase && !KP_PackageID.IsEmpty) || PackageIDHasChanges)
					{
						parentJob.OnContainerIDChanged(this);
					}
				}
			}
		}

		bool PackageIDReadOnly => IsSentToRTUS;

		#endregion

		#region KP_KPH_PackageHeader

		public override ZGuid KP_KPH_PackageHeader
		{
			get => base.KP_KPH_PackageHeader;
			set
			{
				var previousValue = KP_KPH_PackageHeader;
				base.KP_KPH_PackageHeader = value;

				if (value.IsEmpty || (previousValue != value && KP_Sequence == 0))
				{
					PackageSequenceCalculator?.Sequence(this);
				}

				// If a non labelled inner gets assigned an ID we should check whether to use divots
				if (previousValue.IsEmpty && ShouldPackTrackedPackagesViaDivot)
				{
					PkgPackageHandlingUnitDivotHelper.ReplaceParentPackageWithDivotForLabelledPackages(Factory, this, ignoreInnerPackageID: true);
				}
			}
		}

		#endregion

		#region OriginalPackageID

		public ZString OriginalPackageID
		{
			get { return originalPackageID ?? (originalPackageID = KP_PackageID).Value; }
		}

		ZString? originalPackageID;

		#endregion

		#region KP_PackageQty

		public override ZInt KP_PackageQty
		{
			get { return base.KP_PackageQty; }
			set
			{
				var originalValue = KP_PackageQty;

				if (IsCopying)
				{
					base.KP_PackageQty = value;
				}
				else
				{
					SetKP_PackageQtyCore(value);
				}

				UpdateIsUnknownQuantityIfNowKnown();

				if (originalValue != KP_PackageQty)
				{
					FirePackageDataChanged(PackageDataChangeType.PackageContent);
				}

				if (ParentPackage != null)
				{
					ParentPackage.Validation.ValidateKP_PackageQty();
				}
			}
		}

		protected virtual bool ForceUpdateWeightIfPackageQtyChanged => true;

		void UpdateIsUnknownQuantityIfNowKnown()
		{
			var newProposedIsUnknownQuantity = (KP_PackageQty == 0);
			if (!newProposedIsUnknownQuantity && KP_IsUnknownQty)
			{
				KP_IsUnknownQty = newProposedIsUnknownQuantity;
			}
		}

		void SetKP_PackageQtyCore(ZInt value)
		{
			ClearDescription();

			var previousPackageQty = KP_PackageQty;
			if (previousPackageQty != value)
			{
				if (previousPackageQty > 0)
				{
					PreviousNonZeroPackageQty = previousPackageQty;
				}

				base.KP_PackageQty = value;

				if (ForceUpdateWeightIfPackageQtyChanged)
				{
					CalculateWeight();
				}

				CalculateVolume();

				Validation.ValidateKP_PackageID();
			}

			KP_PackageIDInfo.RefreshBinding();
		}

		#endregion

		#region KP_ClosedTimeUtc

		[ReadOnly(true)]
		public override ZDateTime KP_ClosedTimeUtc
		{
			get => base.KP_ClosedTimeUtc;
			set
			{
				base.KP_ClosedTimeUtc = value;
				SetClosedByUserAndIsClosedFlag(value.IsEmpty);
				AutoOpenParentPackagesIfNecessary();
				if (!value.IsEmpty && IsOuter && !IsContainer)
				{
					Logs.AddNew(Events.PackingCompleted, "Packing Completed", GetPackingCompletedEventParams());
				}
			}
		}

		void SetClosedByUserAndIsClosedFlag(bool isEmptyClosedByDate)
		{
			KP_GS_NKClosedBy = isEmptyClosedByDate ? string.Empty : Env.CurrentUser.Initials;
			KP_IsClosed = !isEmptyClosedByDate;
		}

		[ReadOnly(true)]
		public override ZString KP_GS_NKClosedBy { get => base.KP_GS_NKClosedBy; set => base.KP_GS_NKClosedBy = value; }

		[ReadOnly(true)]
		public override ZBool KP_IsClosed { get => base.KP_IsClosed; set => base.KP_IsClosed = value; }

		KeyValuePair<string, string>[] GetPackingCompletedEventParams()
		{
			return PrinterUsedToPrintLabel.IsEmpty
				? Enumerable.Empty<KeyValuePair<string, string>>().ToArray()
				: new[] { new KeyValuePair<string, string>(EventReferenceParameters.Codes.EquipmentReferenceNumber, PrinterUsedToPrintLabel) };
		}

		public ZString PrinterUsedToPrintLabel { get; set; }

		void AutoOpenParentPackagesIfNecessary()
		{
			if (!IsClosed)
			{
				var parentPackage = ParentPackage;
				if (parentPackage != null)
				{
					parentPackage.KP_ClosedTimeUtc = ZDateTime.Empty;
				}
			}
		}

		#endregion

		#region KP_ReleasedTimeUtc

		[ReadOnly(true)]
		public override ZDateTime KP_ReleasedTimeUtc
		{
			get => base.KP_ReleasedTimeUtc;
			set
			{
				var isReleasing = !KP_ReleasedTimeUtc.IsValid && value.IsValid;
				base.KP_ReleasedTimeUtc = value;
				SetReleasedByUserAndIsReleasedFlag(value.IsEmpty);
				if (value.IsEmpty)
				{
					base.KP_IsReleasedViaJob = false;
				}

				if (isReleasing && IsOuter) // tested in PkgPackageJob -- TestFirePackageJobReleasedIfNecessary()
				{
					PackageJob.FirePackageJobReleasedIfNecessary();
				}
			}
		}

		void SetReleasedByUserAndIsReleasedFlag(bool isEmptyReleasedByDate)
		{
			KP_GS_NKReleasedBy = isEmptyReleasedByDate ? string.Empty : Env.CurrentUser.Initials;
			KP_IsReleased = !isEmptyReleasedByDate;
		}

		[ReadOnly(true)]
		public override ZString KP_GS_NKReleasedBy { get => base.KP_GS_NKReleasedBy; set => base.KP_GS_NKReleasedBy = value; }

		[ReadOnly(true)]
		public override ZBool KP_IsReleased { get => base.KP_IsReleased; set => base.KP_IsReleased = value; }

		#endregion

		#region KP_IsHeld

		public override ZBool KP_IsHeld
		{
			get { return base.KP_IsHeld; }
			set
			{
				if (KP_IsHeld != value)
				{
					var isRemovingHold = KP_IsHeld && !value;
					if (isRemovingHold)
					{
						if (PackageJob?.ParentJob is IStmALogParent parentJob)
						{
							parentJob.Logs.AddNew(Events.ClearedHold, "Package is no longer held", new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, KP_PackageID));
						}
					}
					else
					{
						//The package should not be released when it's held
						KP_ReleasedTimeUtc = ZDateTime.Empty;
					}

					base.KP_IsHeld = value;
				}
			}
		}

		#endregion

		#region KP_F3_NKPackType

		[List("Lookups.PackTypes")]
		public override ZString KP_F3_NKPackType
		{
			get { return base.KP_F3_NKPackType; }
			set
			{
				if (KP_F3_NKPackType != value)
				{
					if (IsCopying)
					{
						base.KP_F3_NKPackType = value;
					}
					else
					{
						SetKP_F3_NKPackTypeCore(value);
					}

					FirePackageDataChanged(PackageDataChangeType.PackageContent);

					NotifyParentOfContainerIDChangeIfRequired();
				}
			}
		}

		void SetKP_F3_NKPackTypeCore(ZString value)
		{
			if (CanChangePackType())
			{
				OnPackTypeChangingIfFromContainer();

				ClearDescription();
				DeleteContainer();

				base.KP_F3_NKPackType = value;
				OnPackTypeChangedIfToContainer();

				SetDIMsFromValidPackType();
				SetLastUsedPackType();

				Validation.ValidateKP_PackageID();
				KP_PackageIDInfo.RefreshBinding();
			}
			else
			{
				OnPackTypeChangingFromContainerCancelled();
			}
		}

		void OnPackTypeChangingIfFromContainer()
		{
			if (IsContainer && PackTypeChangingFromContainer != null)
			{
				// is currently a container but will change
				PackTypeChangingFromContainer(this, EventArgs.Empty);
			}
		}

		void OnPackTypeChangingFromContainerCancelled()
		{
			PackTypeChangingFromContainerCancelled?.Invoke(this, EventArgs.Empty);
		}

		void OnPackTypeChangedIfToContainer()
		{
			if (IsContainer && PackTypeChangedToContainer != null)
			{
				// has changed to a container
				PackTypeChangedToContainer(this, EventArgs.Empty);
			}
		}

		void FirePackageDataChanged(PackageDataChangeType dataChangeType)
		{
			var packageJob = PackageJob;
			if (packageJob != null) // cloning sets parent last
			{
				packageJob.FirePackageDataChanged(this, dataChangeType);
			}
		}

		public void PokePackageDataChangedEvent()
		{
			FirePackageDataChanged(PackageDataChangeType.PackageContent);
		}

		public event EventHandler PackTypeChangingFromContainer;
		public event EventHandler PackTypeChangingFromContainerCancelled;
		public event EventHandler PackTypeChangedToContainer;

		#region PackTypeChangingFromContainerWithData

		public event EventHandler<PackTypeChangingFromContainerWithDataEventArgs> PackTypeChangingFromContainerWithData;

		public class PackTypeChangingFromContainerWithDataEventArgs : EventArgs
		{
			public bool Continue;
		}

		bool CanChangePackType()
		{
			bool result = true;

			if (PackTypeChangingFromContainerWithData != null && IsContainerWithData)
			{
				var args = new PackTypeChangingFromContainerWithDataEventArgs();
				PackTypeChangingFromContainerWithData(this, args);
				result = args.Continue;
			}

			return result;
		}

		#endregion

		#endregion

		#region SetDIMsFromValidPackType

		void SetDIMsFromValidPackType()
		{
			if (!KP_F3_NKPackTypeInfo.HasErrors())
			{
				var refPackType = Lookups.PackTypes.FirstOrDefault(pack => pack.F3_Code == KP_F3_NKPackType);

				if (refPackType != null) // can be null if validation was suspended when setting KP_F3_NKPackType
				{
					SetValuesFromTemplate(new PackageTemplatePackTypeWrapper(refPackType));
				}
			}
		}

		#endregion

		#region SetLastUsedPackType

		void SetLastUsedPackType()
		{
			if (!KP_F3_NKPackType.IsEmpty && !KP_F3_NKPackTypeInfo.HasErrors())
			{
				var packageJob = PackageJob;
				if (packageJob != null)
				{
					if (IsOuter)
					{
						packageJob.LastUsedOuterPackType = KP_F3_NKPackType;
					}
					else
					{
						packageJob.LastUsedInnerPackType = KP_F3_NKPackType;
					}
				}
			}
		}

		#endregion

		#region Container Properties

		#region ContainerDunnageWeight

		[MeasureUnit(Schema.KP_WeightUQ, MeasureUnitType.Weight, "IsContainer")]
		public ZDecimal ContainerDunnageWeight
		{
			get { return Container != null ? KP_DunnageWeight : ZDecimal.Zero; }
			set
			{
				if (Container != null)
				{
					KP_DunnageWeight = value;
				}
			}
		}

		#endregion

		#region ContainerTareWeight

		[MeasureUnit(Schema.KP_WeightUQ, MeasureUnitType.Weight, "IsContainer")]
		public ZDecimal ContainerTareWeight
		{
			get { return Container != null ? KP_TareWeight : ZDecimal.Zero; }
			set
			{
				if (Container != null)
				{
					KP_TareWeight = value;
				}
			}
		}

		#endregion

		#region ContainerGoodsWeight

		[MeasureUnit(Schema.KP_WeightUQ, MeasureUnitType.Weight, "IsContainer")]
		public ZDecimal ContainerGoodsWeight
		{
			get { return Container != null ? Container.GoodsWeight : ZDecimal.Zero; }
			set
			{
				if (Container != null)
				{
					Container.GoodsWeight = value;
				}
			}
		}

		#endregion

		#endregion

		// persistent -- DIMs

		#region Goods Weight
		[MeasureUnit(Schema.KP_WeightUQ, MeasureUnitType.Weight)]
		public ZDecimal GoodsWeight
		{
			get { return KP_Weight - KP_TareWeight; }
			set
			{
				if (!IsCopying)
				{
					KP_Weight = KP_TareWeight + value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateGoodsWeight();
				}

				GoodsWeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfoDecimal GoodsWeightInfo => (ZPropertyInfoDecimal)GetZPropertyInfo(nameof(GoodsWeight));
		#endregion

		#region KP_Weight

		[MeasureUnit(Schema.KP_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal KP_Weight
		{
			get { return base.KP_Weight; }
			set
			{
				ZDecimal oldValue = Math.Max(0, KP_Weight); // in case field was negative (in error)
				base.KP_Weight = value;
				ZDecimal difference = KP_Weight - oldValue;
				AddWeightToParentPackage(difference);
				RefreshContainerGoodsWeight();

				if (!difference.IsEmpty)
				{
					FirePackageDataChanged(PackageDataChangeType.Weight);
				}
			}
		}

		void RefreshContainerGoodsWeight()
		{
			if (!IsCopying && IsContainer)
			{
				Container.GoodsWeightInfo.RefreshBinding();
			}
		}

		void AddWeightToParentPackage(ZDecimal weight)
		{
			var parentPackage = ParentPackage;
			if (parentPackage != null && !SuspendAddWeightToParentPackageSemaphore.IsSuspended)
			{
				var parentWeightUQ = (string)parentPackage.KP_WeightUQ;
				if (parentWeightUQ.Length == 0)
				{
					parentWeightUQ = PackingRegistry.Instance.WeightUnit.Value;
					parentPackage.KP_WeightUQ = parentWeightUQ;
				}

				var weightUQ = (string)KP_WeightUQ;
				if (Constants.Weight.ContainsCode(weightUQ) && Constants.Weight.ContainsCode(parentWeightUQ))
				{
					var weightInParentPackageUQ = Constants.Weight.Convert(weight, weightUQ, parentWeightUQ);
					parentPackage.KP_Weight = Math.Max(0, parentPackage.KP_Weight + weightInParentPackageUQ);
				}
			}
		}

		public Semaphore SuspendAddWeightToParentPackageSemaphore
		{
			get { return suspendAddWeightToParentPackageSemaphore ?? (suspendAddWeightToParentPackageSemaphore = new Semaphore()); }
		}

		Semaphore suspendAddWeightToParentPackageSemaphore;

		#endregion

		#region KP_WeightUQ

		[List("Lookups.WeightUQs")]
		public override ZString KP_WeightUQ
		{
			get { return base.KP_WeightUQ; }
			set
			{
				if (KP_WeightUQ != value)
				{
					if (KP_Weight > 0)
					{
						AddWeightToParentPackage(-KP_Weight);
					}

					base.KP_WeightUQ = value;

					if (KP_Weight > 0)
					{
						AddWeightToParentPackage(KP_Weight);
					}

					FirePackageDataChanged(PackageDataChangeType.Weight);
				}
			}
		}

		#endregion

		#region KP_VolumeUQ

		[List("Lookups.VolumeUQs")]
		public override ZString KP_VolumeUQ
		{
			get { return base.KP_VolumeUQ; }
			set
			{
				if (KP_VolumeUQ != value)
				{
					base.KP_VolumeUQ = value;
					FirePackageDataChanged(PackageDataChangeType.Volume);
				}
			}
		}

		#endregion

		#region KP_DimensionUQ

		[List("Lookups.DimensionUQs")]
		public override ZString KP_DimensionUQ
		{
			get { return base.KP_DimensionUQ; }
			set
			{
				base.KP_DimensionUQ = value;

				CalculateVolume();
				RefreshOverhangLength();
				RefreshOverhangWidth();
				RefreshOverhangHeight();
			}
		}

		#endregion

		#region KP_Length

		[MeasureUnit(Schema.KP_DimensionUQ, MeasureUnitType.Length)]
		public override ZDecimal KP_Length
		{
			get { return base.KP_Length; }
			set
			{
				base.KP_Length = value;

				CalculateVolume();
				RefreshOverhangLength();
			}
		}

		void RefreshOverhangLength()
		{
			if (!IsCopying && IsContainer)
			{
				Container.OverhangLengthInfo.RefreshBinding();
			}
		}

		#endregion

		#region KP_Width

		[MeasureUnit(Schema.KP_DimensionUQ, MeasureUnitType.Length)]
		public override ZDecimal KP_Width
		{
			get { return base.KP_Width; }
			set
			{
				base.KP_Width = value;

				CalculateVolume();
				RefreshOverhangWidth();
			}
		}

		void RefreshOverhangWidth()
		{
			if (!IsCopying && IsContainer)
			{
				Container.OverhangWidthInfo.RefreshBinding();
			}
		}

		#endregion

		#region KP_Height

		[MeasureUnit(Schema.KP_DimensionUQ, MeasureUnitType.Length)]
		public override ZDecimal KP_Height
		{
			get { return base.KP_Height; }
			set
			{
				base.KP_Height = value;

				CalculateVolume();
				RefreshOverhangHeight();
			}
		}

		void RefreshOverhangHeight()
		{
			if (!IsCopying && IsContainer)
			{
				Container.OverhangHeightInfo.RefreshBinding();
			}
		}

		#endregion

		#region KP_Volume

		[MeasureUnit(Schema.KP_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal KP_Volume
		{
			get { return base.KP_Volume; }
			set
			{
				var orignialValue = KP_Volume;

				base.KP_Volume = value;

				if (orignialValue != KP_Volume)
				{
					FirePackageDataChanged(PackageDataChangeType.Volume);
				}
			}
		}
		#endregion

		#region CalculateWeight()

		void CalculateWeight()
		{
			if (KP_PackageQty > 0 && PreviousNonZeroPackageQty > 0 && !IsCopying)
			{
				var difference = KP_PackageQty - PreviousNonZeroPackageQty;
				var singleTareWeight = KP_TareWeight / PreviousNonZeroPackageQty;
				KP_TareWeight += difference * singleTareWeight;

				var singleDunnageWeight = KP_DunnageWeight / PreviousNonZeroPackageQty;
				KP_DunnageWeight += difference * singleDunnageWeight;

				var singleGoodsWeight = GoodsWeight / PreviousNonZeroPackageQty;
				GoodsWeight += difference * singleGoodsWeight;
			}
		}

		int PreviousNonZeroPackageQty { get; set; }

		#endregion

		#region CalculateVolume()

		void CalculateVolume()
		{
			if (CanCalculateVolume)
			{
				KP_Volume = VolumeCalculator.Calculate() * KP_PackageQty;
			}
		}

		bool CanCalculateVolume
		{
			get { return !((ISupportDataImporting)this).IsImportingData && IsDimensionUQValid && IsVolumeUQValid && IsDimensionsValid && KP_PackageQty > 0; }
		}

		bool IsVolumeUQValid
		{
			get { return !KP_VolumeUQ.IsEmpty && Lookups.VolumeUQs.ContainsCode(KP_VolumeUQ); }
		}

		bool IsDimensionsValid
		{
			get { return KP_Length > 0 && KP_Width > 0 && KP_Height > 0; }
		}

		bool IsDimensionUQValid
		{
			get { return !KP_DimensionUQ.IsEmpty && Lookups.DimensionUQs.ContainsCode(KP_DimensionUQ); }
		}

		VolumeCalculator VolumeCalculator
		{
			get
			{
				return volumeCalculator ?? (volumeCalculator =
					new VolumeCalculator
					(
						(ZPropertyInfoDecimal)KP_LengthInfo,
						(ZPropertyInfoDecimal)KP_WidthInfo,
						(ZPropertyInfoDecimal)KP_HeightInfo,
						(ZPropertyInfoString)KP_DimensionUQInfo,
						(ZPropertyInfoString)KP_VolumeUQInfo
					));
			}
		}

		VolumeCalculator volumeCalculator;

		#endregion

		#region Booked Dimensions

		public PkgPackageBookedDetail BookedDimensions
		{
			get
			{
				var bookedDimensionsBO = Factory.LoadFromUniqueKey<PkgPackageBookedDetail>(PkgPackageBookedDetailSchema.KPB_KP_Package, PK);

				if (bookedDimensionsBO == null)
				{
					bookedDimensionsBO = Factory.New<PkgPackageBookedDetail>();
					bookedDimensionsBO.KPB_KP_Package = PK;
					RegisterEditableChildObject(bookedDimensionsBO);
				}

				return bookedDimensionsBO;
			}
		}

		void DeleteBookedDimensions()
		{
			var bookedDimensionsBO = Factory.LoadFromUniqueKey<PkgPackageBookedDetail>(PkgPackageBookedDetailSchema.KPB_KP_Package, PK);
			bookedDimensionsBO?.Delete();
		}

		#endregion

		#region Screenings

		public PkgPackageScreeningCollection Screenings
		{
			get
			{
				if (screenings == null)
				{
					screenings = new PkgPackageScreeningCollection(this);
				}
				return screenings;
			}
		}

		public PkgPackageScreening LatestScreening
		{
			get
			{
				return Screenings.OrderByDescending(s => s.KPS_Time).FirstOrDefault();
			}
		}

		PkgPackageScreeningCollection screenings;

		#endregion

		#region CusEntryNumReferences

		public ICusEntryNumReferenceCollection CusEntryNumReferences
		{
			get
			{
				if (cusEntryNumReferences == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumReferenceCollectionProvider>();
					cusEntryNumReferences = provider.GetCollection(this);

					var cusEntryNumReferencesBusinessObjectCollection = cusEntryNumReferences as BusinessObjectCollection;
					if (cusEntryNumReferencesBusinessObjectCollection != null)
					{
						cusEntryNumReferencesBusinessObjectCollection.Load();
					}

					cusEntryNumReferences.SuspendValidation();
				}

				return cusEntryNumReferences;
			}
		}

		ICusEntryNumReferenceCollection cusEntryNumReferences;

		#endregion

		#region CustomsReferenceNumbers

		public ICustomsReferenceCollection CustomsReferenceNumbers
		{
			get
			{
				if (customsReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICustomsReferenceCollectionProvider>();
					customsReferenceNumbers = provider.GetCollection(this);
				}

				return customsReferenceNumbers;
			}
		}

		ICustomsReferenceCollection customsReferenceNumbers;

		#endregion

		#region PortReferences

		public IPortReferenceCollection PortReferences
		{
			get
			{
				if (portReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<IPortReferenceCollectionProvider>();
					portReferenceNumbers = provider.GetCollection(this);
				}

				return portReferenceNumbers;
			}
		}

		IPortReferenceCollection portReferenceNumbers;

		#endregion

		// persistent -- temperature details

		#region KP_RequiredTemperatureMinimum

		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZDecimal KP_RequiredTemperatureMinimum
		{
			get { return base.KP_RequiredTemperatureMinimum; }
			set
			{
				base.KP_RequiredTemperatureMinimum = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateKP_RequiredTemperatureMaximum();
				}
			}
		}

		#endregion

		#region KP_RequiredTemperatureMaximum

		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZDecimal KP_RequiredTemperatureMaximum
		{
			get { return base.KP_RequiredTemperatureMaximum; }
			set
			{
				base.KP_RequiredTemperatureMaximum = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateKP_RequiredTemperatureMinimum();
				}
			}
		}

		#endregion

		#region KP_RequiredTemperatureUnit

		[List("Lookups.TemperatureUnits")]
		[ReadOnlyMember(nameof(TemperatureDetailsReadOnly))]
		public override ZString KP_RequiredTemperatureUnit
		{
			get { return base.KP_RequiredTemperatureUnit; }
			set { base.KP_RequiredTemperatureUnit = value; }
		}

		#endregion

		#region KP_TareWeight

		[MeasureUnit(Schema.KP_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal KP_TareWeight
		{
			get { return base.KP_TareWeight; }
			set
			{
				var oldGoodsWeight = GoodsWeight;
				base.KP_TareWeight = value;

				if (!IsCopying)
				{
					AfterKP_TareWeightSet(oldGoodsWeight);
				}
			}
		}

		protected virtual void AfterKP_TareWeightSet(ZDecimal oldValue)
		{
			KP_Weight = KP_TareWeight + oldValue;
		}

		#endregion

		#region KP_DunnageWeight

		[MeasureUnit(Schema.KP_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal KP_DunnageWeight
		{
			get { return base.KP_DunnageWeight; }
			set { base.KP_DunnageWeight = value; }
		}

		#endregion

		#region TemperatureDetailsReadOnly

		protected bool TemperatureDetailsReadOnly
		{
			get { return !KP_RequiresTemperatureControl; }
		}

		#endregion

		// strategy for haschanges

		#region SetStrategy

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return ValueSetStrategy;
		}

		PackingValueSetStrategy ValueSetStrategy => valueSetStrategy ?? (valueSetStrategy = new PackingValueSetStrategy(this, PkgPackageSchema.KP_IsReleased, PkgPackageSchema.KP_ReleasedTimeUtc, PkgPackageSchema.KP_GS_NKReleasedBy, PkgPackageSchema.KP_IsReleasedViaJob));
		PackingValueSetStrategy valueSetStrategy;

		#endregion

		// calculated

		#region Description

		public ZString Description
		{
			get
			{
				if (!description.HasValue) // caching this value is important for performance of the packing tree.
				{
					if (KP_F3_NKPackType.IsEmpty)
					{
						description = EmptyPackageCode;
					}
					else
					{
						description = Lookups.PackTypes.GetDescriptionFromCode(KP_F3_NKPackType);
						if (description.Value.IsEmpty)
						{
							description = KP_F3_NKPackType;
						}
						else if (KP_PackageQty > 1)
						{
							description = Grammar.Instance.Pluralize(description);
						}
					}
				}

				return description.Value;
			}
		}

		void ClearDescription()
		{
			description = null;
		}

		ZString? description;

		#endregion

		#region EmptyPackageCode

		public ZString EmptyPackageCode
		{
			get { return "???"; }
		}

		#endregion

		#region PackageIdDescription

		public ZString PackageIdDescription
		{
			get
			{
				return (IsContainer)
					? Res.GetString("3c536461-dc8d-4120-858e-8618844440f3", "Container ID:")
					: Res.GetString("1bda227f-731e-46e8-914a-ceb640b5308c", "Package ID:");
			}
		}

		#endregion

		#region PackageIDWithFallback

		public ZString PackageIDWithFallback
		{
			get
			{
				var packageID = KP_PackageID;
				if (packageID.IsEmpty)
				{
					var containerType = IsContainer ? Container.ContainerType : null;
					var containerTypeCode = containerType != null ? containerType.RC_Code : ZString.Empty;
					packageID = KP_PackageQty + KP_F3_NKPackType + containerTypeCode;
				}

				return packageID;
			}
		}

		#endregion

		#region PackageIDWithFallbackToExternalReference

		public ZString PackageIDWithFallbackToExternalReference
		{
			get
			{
				return !KP_PackageID.IsEmpty ? KP_PackageID : KP_ExternalReference;
			}
		}

		#endregion

		#region PackageSequenceString

		public ZString PackageSequenceString
		{
			get { return KP_Sequence.ToString("D4", CultureInfo.InvariantCulture); }
		}

		#endregion

		#region MostRecentLogDescription

		ZString MostRecentLogDescription
		{
			get
			{
				var log = Logs.MostRecentLog;
				return log != null && log.SL_SE_NKEvent != Events.PackingCompleted.Code && log.SL_SE_NKEvent != Events.DataExport.Code
							? log.SL_EventDescription
							: ZString.Empty;
			}
		}

		#endregion

		#region CartonGroupAndSize

		[ResourceStringData("PkgPackage|CartonGroupAndSize", Caption = "Carton Group And Size", ShortCaption = "Ctn Group & Size")]
		[ReadOnly(true)]
		[MaxLength(100)]
		public ZString CartonGroupAndSize
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.CartonGroupAndSize); }
			set
			{
				this.SetSystemDefinedValue(Schema.CartonGroupAndSize, value);
				CartonGroupAndSizeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CartonGroupAndSizeInfo
		{
			get { return GetZPropertyInfo(Schema.CartonGroupAndSize); }
		}

		#endregion

		#region IsSentToRTUS

		[ResourceStringData("PkgPackage|IsSentToRTUS", Caption = "Is sent to RTUS", ShortCaption = "Sent to RTUS")]
		[ReadOnly(true)]
		public ZBool IsSentToRTUS
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.IsSentToRTUS); }
			set
			{
				this.SetSystemDefinedValue(Schema.IsSentToRTUS, value);
				IsSentToRTUSInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsSentToRTUSInfo
		{
			get { return GetZPropertyInfo(Schema.IsSentToRTUS); }
		}

		#endregion

		#region RTUSBookedType

		[ResourceStringData("PkgPackage|RTUSBookedType", Caption = "RTUS Booked Type")]
		[ReadOnly(true)]
		[MaxLength(Schema.RTUSBookedTypeMaxLength)]
		public ZString RTUSBookedType
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.RTUSBookedType); }
			set
			{
				CheckMaximumLength(RTUSBookedTypeInfo, value);
				this.SetSystemDefinedValue(Schema.RTUSBookedType, value);
				RTUSBookedTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RTUSBookedTypeInfo => GetZPropertyInfo(Schema.RTUSBookedType);

		#endregion

		#region RTUSPrinterPK

		[List("Lookups.Printers")]
		[ResourceStringData("PkgPackage|RTUSLabelPrinterPK", Caption = "Carrier Label Printer", ShortCaption = "Label Printer")]
		public ZGuid RTUSLabelPrinterPK
		{
			get
			{
				var rtusLabelPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, this);
				return rtusLabelPrinter?.SDP_SQ_Printer ?? ZGuid.Empty;
			}
			set
			{
				StmDefaultPrinter.SetPrinterPKOrDeleteIfEmpty(Factory, this, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateRTUSLabelPrinterPK();
				}

				RTUSLabelPrinterPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RTUSLabelPrinterPKInfo => GetZPropertyInfo(Schema.RTUSLabelPrinterPK);

		#endregion

		#region PackageSequenceCalculator

		PackageSequenceCalculator GetPackageSequenceCalculator(PkgPackageJob packageJob) => !IsDeletingPackage ? packageJob?.PackageSequenceCalculator : null;
		PackageSequenceCalculator PackageSequenceCalculator => !IsDeletingPackage ? PackageJob?.PackageSequenceCalculator : null;

		#endregion

		public bool IsClosed => KP_IsClosed || KP_ClosedTimeUtc.IsValid;

		public bool IsReleased => KP_IsReleased || KP_ReleasedTimeUtc.IsValid;

		#endregion

		#region Flags

		#region ShouldPackTrackedPackagesViaDivot

		public bool ShouldPackTrackedPackagesViaDivot
		{
			set => shouldPackTrackedPackagesViaDivot = value;
			get
			{
				if (!shouldPackTrackedPackagesViaDivot.HasValue)
				{
					shouldPackTrackedPackagesViaDivot = PackageJob != null && (PkgPackageJob.LoadParent<IShouldPackTrackedPackagesViaDivot>(PackageJob)?.ShouldPackTrackedPackagesViaDivot ?? false);
				}

				return shouldPackTrackedPackagesViaDivot.Value;
			}
		}
		bool? shouldPackTrackedPackagesViaDivot;

		#endregion

		#region IsContainer

		public bool IsContainer => KP_F3_NKPackType.EqualsIgnoringCase(Constants.PkgUnit.Container);

		#endregion

		#region IsBookedViaCarrier

		public bool IsBookedViaCarrier => IsSentToRTUS;

		#endregion

		#region IsEmpty

		bool IsEmpty
		{
			get { return !PackedItemDivots.Any() && !Packages.Any(); }
		}

		#endregion

		#region IsPackageIdValid

		public bool IsPackageIdValid
		{
			get
			{
				bool result = KP_PackageID.IsEmpty;

				if (!result) // do IsEmpty check here for performance
				{
					var packageJob = PackageJob;
					result = packageJob == null || packageJob.IsPackageIdValid(this);
				}

				return result;
			}
		}

		#endregion

		#region IsPackageIdValidSSCCBarCode

		public bool IsPackageIdValidSSCCBarCode
		{
			get { return SSCCBarCodeChecker.IsSSCCBarCode(KP_PackageID); }
		}

		#endregion

		#region HasPackedItemsIncludingChildren

		bool HasPackedItemsIncludingChildren
		{
			get
			{
				if (PackedItemDivots.Count > 0)
				{
					return true;
				}

				foreach (var package in Packages)
				{
					if (package.HasPackedItemsIncludingChildren)
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		#region IsContainerWithData

		bool IsContainerWithData
		{
			get
			{
				var container = Container;
				return container != null && !container.HasAllDefaultValues;
			}
		}

		#endregion

		#region IsOuter

		public bool IsOuter
		{
			get { return ParentPackage == null; }
		}

		#endregion

		#region IsClosedOrReleased

		public bool IsClosedOrReleased
		{
			get { return IsClosed || KP_IsReleasedViaJob || IsReleased; }
		}

		#endregion

		#region IsValidCandidateForInner

		public bool IsValidCandidateForInner(out ZString errorMessage, bool useMessageAppropriateForMultiplePackages = false)
		{
			errorMessage = "";

			if (IsReleased || KP_IsReleasedViaJob)
			{
				errorMessage = useMessageAppropriateForMultiplePackages
					? Res.GetString("6ce4f2b0-49fe-4500-a859-1eeb56a629ea", "One or more Selected Packages are already Released and cannot be packed into another Package.")
					: Res.GetString("98cf8709-06eb-483e-9ab9-22ddfc1523d1", "The selected {0} is already Released and cannot be packed into another Package.", Description);
			}
			else if (IsContainer)
			{
				errorMessage = Res.GetString("9b5c73e8-cb59-4e59-a0a4-0cb36f428af4", "Cannot Pack a Container into another Package.");
			}

			return errorMessage.IsEmpty;
		}

		#endregion

		#region PackageIDHasChanges

		public bool PackageIDHasChanges
		{
			get
			{
				var result = KP_KPH_PackageHeaderInfo.HasChanges;
				if (!result)
				{
					var packageID = PackageID;
					result = packageID != null && packageID.KPH_PackageIDInfo.HasChanges;
				}

				return result;
			}
		}

		#endregion

		#region IsLabelPrinted

		public ZBool IsLabelPrinted
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.LabelPrinted); }
			set { this.SetSystemDefinedValue(Schema.LabelPrinted, value); }
		}

		#endregion

		#region IsActionAllowed

		bool IsActionAllowed(PackageAction action)
		{
			var result = ActionStrategy?.IsActionAllowed(action);
			return !result.HasValue || result.Value;
		}

		IPackageActionStrategy ActionStrategy => actionStrategy ?? (actionStrategy = GetActionStrategy(PackageJob));
		IPackageActionStrategy GetActionStrategy(PkgPackageJob packageJob) => packageJob?.ParentJob?.GetPackageActionStrategy(this);

		IPackageActionStrategy actionStrategy;

		public void ClearActionStrategyCacheIncludingChildren()
		{
			ClearActionStrategyCacheIncludingChildren(PackageJob);
		}

		void ClearActionStrategyCacheIncludingChildren(PkgPackageJob packageJob)
		{
			actionStrategy = null;
			if (packageJob != null)
			{
				Packages.ForEach(p => p.ClearActionStrategyCacheIncludingChildren(packageJob));
			}
		}

		#endregion

		#region CanReprintCarrierLabel

		public bool CanPrintCarrierLabel => !KP_PackageID.IsEmpty && IsValidParentJobForPrintingCarrierLabel();

		bool IsValidParentJobForPrintingCarrierLabel()
		{
			var parentJob = PackageJob?.ParentJob;
			return parentJob != null
				&& parentJob.ParentJobType != ParentJobType.None
				&& parentJob.CarrierBookingAgent != null;
		}

		#endregion

		#region CanCancelPackageLabel

		public bool CanCancelPackageLabel => IsSentToRTUS && CanPrintCarrierLabel;

		#endregion

		// temperature

		#region IsNotRequiringTemperatureControl

		public ZBool IsNotRequiringTemperatureControl
		{
			get { return !KP_RequiresTemperatureControl; }
			set { SetTemperatureFlagExclusive(value, SetIsNotRequiringTemperatureControlCore); }
		}

		void SetIsNotRequiringTemperatureControlCore(bool value)
		{
			KP_RequiresTemperatureControl = !value;
			if (value)
			{
				ResetTemperatureDetails();
			}
		}

		void ResetTemperatureDetails()
		{
			KP_RequiredTemperatureMinimum = 0m;
			KP_RequiredTemperatureMaximum = 0m;
			KP_RequiredTemperatureUnit = "";
		}

		#endregion

		#region IsChillerRequired

		public ZBool IsChillerRequired
		{
			get { return this.IsChiller(); }
			set { SetTemperatureFlagExclusive(value, (isChiller) => this.SetIsChiller(isChiller)); }
		}

		#endregion

		#region IsFreezerRequired

		public ZBool IsFreezerRequired
		{
			get { return this.IsFreezer(); }
			set { SetTemperatureFlagExclusive(value, (isFreezer) => this.SetIsFreezer(isFreezer)); }
		}

		#endregion

		#region SetTemperatureFlagExclusive

		void SetTemperatureFlagExclusive(bool value, Action<bool> setTemperatureFlag)
		{
			if (value || IsResettingTemperatureFlags.IsSuspended)
			{
				using (new SemaphoreManager(IsResettingTemperatureFlags))
				{
					setTemperatureFlag(value);
				}
			}
		}

		/// <summary>
		/// NoTemperatureControl/Chiller/Freezer all set each other to
		/// false, we must lock during this to prevent incorrect results.
		/// </summary>
		Semaphore IsResettingTemperatureFlags
		{
			get { return isResettingTemperatureFlags ?? (isResettingTemperatureFlags = new Semaphore()); }
		}

		Semaphore isResettingTemperatureFlags;

		#endregion

		#endregion

		#region DataRefresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			PreviousPackageJob = KP_KJ_ParentPackageJob;
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			if (PreviousPackageJob.HasValue && PreviousPackageJob.Value != KP_KJ_ParentPackageJob)
			{
				KP_KJ_ParentPackageJobInfo.RefreshBinding();
			}

			PreviousPackageJob = null;
		}

		ZGuid? PreviousPackageJob;

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PkgPackageFetchStrategy(this);
		}

		#endregion

		#region Packing

		#region IsAvailableForPacking / Unpacking

		public bool IsAvailableForPacking(out ZString errorMessage)
		{
			return IsAvailableCore(out errorMessage, isPacking: true);
		}

		public bool IsAvailableForUnpacking(out ZString errorMessage)
		{
			return IsAvailableCore(out errorMessage, isPacking: false);
		}

		bool IsAvailableCore(out ZString errorMessage, bool isPacking = true, PkgPackage childPackage = null)
		{
			errorMessage = "";

			if (IsClosedOrReleased)
			{
				var verb = isPacking ? Res.GetString("98af966b-11e6-4628-80e0-5f870f42582e", "Pack into")
									 : Res.GetString("14b2aee2-5de9-4de0-b6bc-82dc25215090", "Unpack");

				errorMessage = (childPackage == null)
					? Res.GetString("38c1136e-be90-47eb-ab1f-2b04864a7ab6", "Cannot {0} the selected {1} because it is already {2}.", verb, KP_F3_NKPackType, Status)
					: Res.GetString("0ccaeb1b-d5eb-4585-bc2b-1b2a65ad5c38", "Cannot {0} the selected {1} because it is packed onto a {2} that is already {3}.",
						verb, childPackage.KP_F3_NKPackType, KP_F3_NKPackType, Status);
			}
			else if (!IsActionAllowed(PackageAction.PackUnpack))
			{
				errorMessage = ActionStrategy.ReasonForNotAllowingAction;
			}
			else
			{
				var parentPackage = ParentPackage;
				if (parentPackage != null)
				{
					parentPackage.IsAvailableCore(out errorMessage, isPacking, childPackage ?? this);
				}
			}

			return errorMessage.IsEmpty;
		}

		#endregion

		#region GetPackedQty

		public ZDecimal GetPackedQty(IPackableItemParent packableItemParent)
		{
			return packableItemParent.GetPackedQty(this);
		}

		#endregion

		#region IsPacked

		public bool IsPacked(IPackableItemParent packableItemParent)
		{
			return packableItemParent.GetPackedDivots(this).Any();
		}

		public bool IsPacked(IPackableItem packableItem)
		{
			Argument.NotNull(packableItem, nameof(packableItem));

			var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, packableItem.PK);
			return PackedItemDivots.Find(query).FirstOrDefault() != null;
		}

		#endregion

		#region Pack

		public IEnumerable<PkgPackageItemDivotsWrapper> Pack(IPackableItemParent packableItemParentToPack, ZDecimal qtyToPack)
		{
			Argument.NotNull(packableItemParentToPack, nameof(packableItemParentToPack));

			IEnumerable<PkgPackageItemDivotsWrapper> result = null;
			if (qtyToPack > 0)
			{
				using (packedItems?.SuspendListChanged())
				{
					result = PackPackableItemParent(packableItemParentToPack, qtyToPack);
				}
			}

			return result ?? Enumerable.Empty<PkgPackageItemDivotsWrapper>();
		}

		internal void PackForAutoPack(IPackableItemParent packableItemParentToPack, Queue<IPackableItem> packableItems, decimal qtyToPack)
		{
			Argument.NotNull(packableItemParentToPack, nameof(packableItemParentToPack));
			Argument.NotNull(packableItems, nameof(packableItems));

			if (qtyToPack > 0m)
			{
				using (packedItems?.SuspendListChanged())
				{
					while (qtyToPack > 0m && packableItems.Count > 0)
					{
						var packableItem = packableItems.Peek();

						IPackableItem itemPacked;
						(itemPacked, _, qtyToPack) = PackCore(packableItemParentToPack, packableItem, qtyToPack);

						if (itemPacked == packableItem)
						{
							packableItems.Dequeue();
						}
					}
				}
			}
		}

		IEnumerable<PkgPackageItemDivotsWrapper> PackPackableItemParent(IPackableItemParent packableItemParent, ZDecimal qtyToPack)
		{
			var result = new List<PkgPackageItemDivotsWrapper>();
			var cachedKeys = new HashSet<GroupingKey>();
			var itemsToCheck = packableItemParent.PackableItems.OrderBy(i => i.Quantity).Where(i => i.IsUnpacked(Factory)); // pack smallest first

			foreach (var packableItem in itemsToCheck)
			{
				PkgPackageItemDivotsWrapper packedItem;
				(_, packedItem, qtyToPack) = PackCore(packableItemParent, packableItem, qtyToPack);

				if (cachedKeys.Add(packedItem.Key))
				{
					result.Add(packedItem);
				}

				if (qtyToPack == 0m)
				{
					break;
				}
			}

			return result;
		}

		(IPackableItem PackedItem, PkgPackageItemDivotsWrapper Wrapper, ZDecimal QtyLeftToPack) PackCore(IPackableItemParent packableItemParentToPack, IPackableItem packableItem, ZDecimal qtyToPack)
		{
			(IPackableItem, PkgPackageItemDivotsWrapper, ZDecimal) result;

			// pack smallest items first to avoid unnecessary splitting of larger items
			var qtyAvailable = packableItem.Quantity;
			if (qtyAvailable <= qtyToPack)
			{
				var wrapper = PackItem(packableItem, packableItemParentToPack);
				result = (packableItem, wrapper, qtyToPack - qtyAvailable);
			}
			else
			{
				var splitPackableItem = PackableItemSplitHelper.SplitItem(packableItem, qtyToPack);
				var wrapper = PackItem(splitPackableItem, packableItemParentToPack);

				result = (splitPackableItem, wrapper, ZDecimal.Zero);
			}

			return result;
		}

		public void Pack(IPackableItem itemToPack, IPackableItemParent packableItemParent)
		{
			Argument.NotNull(itemToPack, nameof(itemToPack));
			Argument.NotNull(packableItemParent, nameof(packableItemParent));

			if (!IsPacked(itemToPack))
			{
				PackItem(itemToPack, packableItemParent);
			}
			else
			{
				throw new InvalidOperationException("Attempting to pack IPackableItem that is already packed");
			}
		}

		#region PackItem

		PkgPackageItemDivotsWrapper PackItem(IPackableItem itemToPack, IPackableItemParent packableItemParent)
		{
			// Here we manually add the new Non-Persistent Packed Items, so
			// suspend the rebuild of the Non-Persistent Collection here.
			using (SuspendPackedItemDivotsCountChanged())
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				var divot = Factory.New<PkgPackageItemDivot>();
				divot.KI_KP_Package = PK;
				divot.KI_ParentID = itemToPack.PK;
				divot.KI_ParentTableCode = itemToPack.TablePrefix;

				var qtyToPack = itemToPack.Quantity;
				var packedItem = LoadOrCreateDivotWrapper(GetWrapperListFromPackedItemOrCache(), divot, packableItemParent);
				packedItem.SetDivotPackedQty(divot, qtyToPack); // Recalculate Weight of Package and update quantity.

				if (packedItems == null)
				{
					var key = new PackedItemKey(divot.PK, PK, itemToPack.PK, qtyToPack);
					CacheForPackedItemsBeforeCollectionBound.Add(key, packedItem);
				}
				else if (packedItems.Typed.All(w => w.Key != packedItem.Key))
				{
					packedItems.Add(packedItem);
				}

				return packedItem;
			}
		}

		#region GetWrapperListFromPackedItemOrCache

		IEnumerable<PkgPackageItemDivotsWrapper> GetWrapperListFromPackedItemOrCache()
		{
			return packedItems?.Typed.ToArray() ?? CacheForPackedItemsBeforeCollectionBound.Values.ToArray();
		}

		#endregion

		#region LoadOrCreateDivotWrapper

		PkgPackageItemDivotsWrapper LoadOrCreateDivotWrapper(IEnumerable<PkgPackageItemDivotsWrapper> loadedPackedItems, PkgPackageItemDivot divot, IPackableItemParent packableItemParent)
		{
			var result = loadedPackedItems.FirstOrDefault(w => w.PackableItemParent == packableItemParent && !w.IsDeleted); //Need to check !IsDeleted in case PackedItems not build yet, and wrapper deleted from CacheForPackedItemsBeforeCollectionBound

			if (result == null)
			{
				result = PkgPackageItemDivotsWrapper.New(divot, packableItemParent);
			}
			else
			{
				result.AddDivotToWrapper(divot);
			}

			return result;
		}

		#endregion

		Dictionary<PackedItemKey, PkgPackageItemDivotsWrapper> CacheForPackedItemsBeforeCollectionBound
		{
			get { return cacheForPackedItemsBeforeCollectionBound ?? (cacheForPackedItemsBeforeCollectionBound = new Dictionary<PackedItemKey, PkgPackageItemDivotsWrapper>()); }
		}

		Dictionary<PackedItemKey, PkgPackageItemDivotsWrapper> cacheForPackedItemsBeforeCollectionBound;

		#region PackedItemKey

		class PackedItemKey
		{
			public PackedItemKey(ZGuid divotPK, ZGuid packagePK, ZGuid packableItemPK, ZDecimal packedQty)
			{
				DivotPK = divotPK;
				PackagePK = packagePK;
				PackableItemPK = packableItemPK;
				PackedQty = packedQty;
			}

			public ZGuid DivotPK { get; }
			public ZGuid PackagePK { get; }
			public ZGuid PackableItemPK { get; }
			public ZDecimal PackedQty { get; }

			public static PackedItemKey GetKey(PkgPackageItemDivot divot)
			{
				return new PackedItemKey(divot.PK, divot.KI_KP_Package, divot.KI_ParentID, divot.KI_PackedQty);
			}

			public override bool Equals(object obj)
			{
				var other = obj as PackedItemKey;
				return !object.ReferenceEquals(other, null) && other == this;
			}

			public override int GetHashCode()
				=> DivotPK.GetHashCode() ^ PackagePK.GetHashCode() ^ PackableItemPK.GetHashCode() ^ PackedQty.GetHashCode();

			public static bool operator ==(PackedItemKey x, PackedItemKey y)
				=> x.DivotPK == y.DivotPK
				&& x.PackagePK == y.PackagePK
				&& x.PackableItemPK == y.PackableItemPK
				&& x.PackedQty == y.PackedQty;

			public static bool operator !=(PackedItemKey x, PackedItemKey y) => !(x == y);
		}

		#endregion

		#endregion

		#endregion

		#region Unpack

		public void Unpack(PkgPackageItemDivotsWrapper packedItemWrapper, ZDecimal qtyToUnpack, bool removeEmptyPackageEvenWithID = false)
		{
			packedItemWrapper.ThrowExceptionIfDeleted();

			if (qtyToUnpack > 0)
			{
				var packedQty = packedItemWrapper.PackedQty;
				if (packedQty < qtyToUnpack)
				{
					qtyToUnpack = packedQty;
				}

				// unpack
				if (packedQty - qtyToUnpack == 0)
				{
					var parentPackage = packedItemWrapper.ParentPackage;

					using (SuspendPackedItemDivotsCountChanged()) // we are manually removing the packed item
					{
						var key = packedItemWrapper.Key;
						var packedItemToDelete = key is EmptyKey
						? PackedItems.Typed.First(w => w.Key == key && w.PackedQty == qtyToUnpack)
						: PackedItems.Typed.Single(w => w.Key == key);

						PackedItems.Delete(packedItemToDelete); // no need to set KI_PackedQty to 0, avoids a change event
					}

					//parentPackage could be null and this code point could still be reached in rare case 
					//where the packedItemWrapper.Divots had been removed from relationship, e.g. RemoveAllFromRelationship was called improperly
					if (parentPackage != null && (removeEmptyPackageEvenWithID || parentPackage.KP_PackageID.IsEmpty) && parentPackage.IsEmpty)
					{
						parentPackage.Delete();
					}
				}
				// partially unpack
				else
				{
					var packableItemParent = packedItemWrapper.PackableItemParent;
					if (packableItemParent != null)
					{
						BuildPackedItems(); // need to build the collection if it's not built to allow the cache with packed items to be cleared.
						packedItemWrapper.ReducePackedQty(qtyToUnpack);
						packableItemParent.RefreshPackableItems();
					}
				}
			}
		}

		int BuildPackedItems() => PackedItems.Count;

		#endregion

		#endregion

		#region SetValuesFromTemplate

		public void SetValuesFromTemplate(IPackageTemplate template)
		{
			KP_Length = template.Length;
			KP_Width = template.Width;
			KP_Height = template.Height;
			SetWeightFromTemplate(template);

			if (!template.DimensionUQ.IsEmpty)
			{
				KP_DimensionUQ = template.DimensionUQ;
			}

			if (!template.WeightUQ.IsEmpty)
			{
				KP_WeightUQ = template.WeightUQ;
			}

			if (template.Volume.HasValue)
			{
				KP_Volume = template.Volume.Value;

				if (!template.VolumeUQ.IsEmpty)
				{
					KP_VolumeUQ = template.VolumeUQ;
				}
			}
		}

		protected virtual void SetWeightFromTemplate(IPackageTemplate template)
		{
			if (Constants.Weight.ContainsCode(KP_WeightUQ) && Constants.Weight.ContainsCode(template.WeightUQ))
			{
				GoodsWeight = Constants.Weight.Convert(GoodsWeight, KP_WeightUQ, template.WeightUQ);
			}

			KP_TareWeight = template.TareWeight;
		}

		#endregion

		#region BreakDownIntoIndividualPackages

		public bool IsBreakDownAllowed(out ZString errorMessage)
		{
			errorMessage = "";

			if (!IsActionAllowed(PackageAction.PackUnpack))
			{
				errorMessage = ActionStrategy.ReasonForNotAllowingAction;
			}

			return errorMessage.IsEmpty;
		}

		public IEnumerable<PkgPackage> BreakDownIntoIndividualPackages(int splitQty = 1)
		{
			PkgPackage[] result = null;

			if (KP_PackageQty > 1
				&& splitQty > 0 && splitQty < KP_PackageQty
				&& Packages.Count == 0 && PackedItemDivots.Count == 0)
			{
				var helper = new BreakDownHelper(this, splitQty);
				var numberOfSplits = KP_PackageQty / splitQty;

				if (helper.HasRemainder)
				{
					numberOfSplits++;
				}

				result = new PkgPackage[numberOfSplits];

				Factory.SuspendValidation();
				try
				{
					var parent = ParentPackage;
					var packageJob = PackageJob;
					var collection = (parent != null) ? parent.Packages : packageJob.Packages;
					using (((IBusinessObjectCollection)collection).SuspendListChanged())
					{
						// split packages
						for (int i = 1; i < numberOfSplits; i++)
						{
							var args = new BusinessObjectCloneArgs(Enumerable.Empty<string>(), performRowCopyWithoutTriggeringValidationAndSetter: true);
							var newPackage = (PkgPackage)Clone(args);
							var isLastPackage = (i == numberOfSplits - 1);
							var useRemainder = isLastPackage && helper.HasRemainder;

							helper.BreakDownPackage(newPackage, useRemainder);
							result[i] = newPackage;
						}

						// include this package and break it down
						result[0] = this;
						helper.BreakDownPackage(this);

						var parentJob = packageJob.ParentJob as IPackingParentWithAutoPackageBreakdown;
						using (parentJob?.SuspendWhileAddingAutoCreatedPackages())
						{
							collection.AddRange(result);
						}

						parentJob?.RunAfterAllAutoCreatedPackagesAreAdded(result);
					}
				}
				finally
				{
					Factory.ResumeValidation();
				}
			}

			return result ?? Array.Empty<PkgPackage>();
		}

		#region BreakDownHelper

		class BreakDownHelper
		{
			internal BreakDownHelper(PkgPackage packageToClone, int splitQty)
			{
				this.PackageToClone = packageToClone;
				this.SplitQty = splitQty;
			}

			readonly PkgPackage PackageToClone;
			readonly int SplitQty;

			#region Properties

			#region Remainder

			internal int Remainder
			{
				get
				{
					if (!remainder.HasValue)
					{
						remainder = PackageToClone.KP_PackageQty % SplitQty;
					}

					return remainder.Value;
				}
			}

			int? remainder;

			#endregion

			#region HasRemainder

			internal bool HasRemainder
			{
				get { return Remainder > 0; }
			}

			#endregion

			#region Volume

			BreakDownValuesCache Volume
			{
				get { return volume ?? (volume = new BreakDownValuesCache(PackageToClone.KP_Volume, this)); }
			}

			BreakDownValuesCache volume;

			#endregion

			#region Dunnage

			BreakDownValuesCache Dunnage
			{
				get { return dunnage ?? (dunnage = new BreakDownValuesCache(PackageToClone.KP_DunnageWeight, this)); }
			}

			BreakDownValuesCache dunnage;

			#endregion

			#region TareWeight

			BreakDownValuesCache TareWeight
			{
				get { return tareWeight ?? (tareWeight = new BreakDownValuesCache(PackageToClone.KP_TareWeight, this)); }
			}

			BreakDownValuesCache tareWeight;

			#endregion

			#region GoodsWeight

			BreakDownValuesCache GoodsWeight
			{
				get { return goodsWeight ?? (goodsWeight = new BreakDownValuesCache(PackageToClone.GoodsWeight, this)); }
			}

			BreakDownValuesCache goodsWeight;

			#endregion

			#endregion

			#region BreakDownPackage

			internal void BreakDownPackage(PkgPackage package, bool useRemainder = false)
			{
				// for performance, everything the qty setter does is not necessary as we are manually setting weight and volume here
				((IBusinessObjectInternals)package).Row[PkgPackageSchema.Constants.KP_PackageQty] = useRemainder ? Remainder : SplitQty;
				package.KP_PackageQtyInfo.RefreshBinding();
				package.KP_Volume = useRemainder ? Volume.PostBreakDownRemainderValue : Volume.PostBreakDownValue;
				package.KP_DunnageWeight = useRemainder ? Dunnage.PostBreakDownRemainderValue : Dunnage.PostBreakDownValue;
				package.KP_TareWeight = useRemainder ? TareWeight.PostBreakDownRemainderValue : TareWeight.PostBreakDownValue;
				package.GoodsWeight = useRemainder ? GoodsWeight.PostBreakDownRemainderValue : GoodsWeight.PostBreakDownValue;
			}

			#endregion

			#region BreakDownValuesCache

			class BreakDownValuesCache
			{
				/// <summary>
				/// This class is used to hold the caching of Weight Calculations for performance reasons.
				/// When breaking down Packages, the weight each split package will contain will be
				/// the same (except for the final package if the break down was not divisible), so
				/// this value is kept in PostBreakDownValue. PostBreakDownValueForRemainder is not
				/// cached because only one package will have this weight.
				/// </summary>
				internal BreakDownValuesCache(ZDecimal total, BreakDownHelper helper)
				{
					this.Total = total;
					this.Helper = helper;
				}

				readonly ZDecimal Total;
				readonly BreakDownHelper Helper;

				#region SinglePackageValue

				decimal SinglePackageValue
				{
					get
					{
						if (!singlePackageValue.HasValue)
						{
							singlePackageValue = Total / Helper.PackageToClone.KP_PackageQty;
						}

						return singlePackageValue.Value;
					}
				}

				decimal? singlePackageValue;

				#endregion

				#region PostBreakDownValue

				internal decimal PostBreakDownValue
				{
					get
					{
						if (!postBreakDownValue.HasValue)
						{
							postBreakDownValue = SinglePackageValue * Helper.SplitQty;
						}

						return postBreakDownValue.Value;
					}
				}

				decimal? postBreakDownValue;

				#endregion

				#region PostBreakDownRemainderValue

				internal decimal PostBreakDownRemainderValue
				{
					get { return SinglePackageValue * Helper.Remainder; }
				}

				#endregion
			}

			#endregion
		}

		#endregion

		#endregion

		#region Printing

		public void PrintLabel()
		{
			if (KP_KPH_PackageHeader.IsValid)
			{
				var packageJob = PackageJob;
				using (SubscribeToPackageJobAutoPrinted(packageJob, (sender, e) => { IsLabelPrinted = true; }))
				{
					packageJob.PrintLabel(this);
				}
			}
		}

		IDisposable SubscribeToPackageJobAutoPrinted(PkgPackageJob packageJob, EventHandler autoPrinted)
		{
			return new DisposableAction(
				() => packageJob.AutoPrinted += autoPrinted,
				() => packageJob.AutoPrinted -= autoPrinted);
		}

		public void PrintLabel(Guid printerPK, int numberOfLabelsToPrint) => PrintDocument(printerPK, numberOfLabelsToPrint, null);

		public void PrintDocument(Guid printerPK, int numberOfLabelsToPrint, IStmMenuItem documentToPrint)
		{
			if (KP_KPH_PackageHeader.IsValid)
			{
				var packageJob = PackageJob;
				packageJob.Selected.UpdateSelectedPackages(new[] { this }); // we only want to print the label for this particular package
				packageJob.PrintDocument(this, printerPK, numberOfLabelsToPrint, documentToPrint);
			}
		}

		public ReturnResult PrintCarrierLabel()
		{
			ReturnResult result;

			var packageJob = PackageJob;
			if (packageJob != null && packageJob.HasChanges)
			{
				result = new ReturnResult { Message = Res.GetString("FD9EF219-CF1C-4B99-A604-7F494DBC872F", "Save your changes first before printing carrier label.") };
			}
			else if (RTUSLabelPrinterPK.IsEmpty)
			{
				result = new ReturnResult { Message = Res.GetString("5687C57A-EEE3-49DB-99F7-182DD6A6E639", "Please select a Carrier Label Printer to print carrier label.") };
			}
			else
			{
				using (var carrierLabelPrintingProviderManager = new CarrierLabelPrintingProviderManager())
				{
					result = PrintCarrierLabel(RTUSLabelPrinterPK, carrierLabelPrintingProviderManager);
				}

				if (HasChanges)
				{
					// to make sure updates to package are persisted after request to print is successful
					Factory.Save();
				}
			}

			return result;
		}

		public ReturnResult PrintCarrierLabel(ZGuid printerPK, ICarrierLabelPrintingProviderManager carrierLabelPrintingProviderManager)
		{
			var errorMessage = "";
			var printer = (IStmPrintQueue)Lookups.Printers.FindByPK(printerPK);
			if (printer != null)
			{
				var parentJob = PackageJob?.ParentJob;
				if (carrierLabelPrintingProviderManager.IsParentJobValid(parentJob))
				{
					var carrierBookingAgent = parentJob.CarrierBookingAgent;
					if (carrierBookingAgent == null)
					{
						errorMessage = Res.GetString("1df3f1dd-c224-4cd3-9c27-78f010dad83f", "{0} '{1}' has no Carrier Booking Agent.", parentJob.JobDescription, parentJob.JobNo);
					}
					else
					{
						var rtusOption = ObjectFactory.Get<ITransportRegistry>().GetRTUSOption(carrierBookingAgent.PK.ToGuid());
						if (rtusOption == null)
						{
							errorMessage = Res.GetString("faa939b9-a0a9-48ab-be52-3056dc8a522c", "No RTUS URL provided for Organization '{0}'.", carrierBookingAgent.OH_Code);
						}
						else
						{
							errorMessage = PrintCarrierLabelCore(printer, rtusOption, carrierLabelPrintingProviderManager);
						}
					}
				}
				else
				{
					errorMessage = carrierLabelPrintingProviderManager.GetInvalidParentJobTypeErrorMessage(this);
				}
			}
			else
			{
				errorMessage = Res.GetString("f46483b4-5896-4c1b-a107-590729574435", "Could not find specified Printer.");
			}

			return new ReturnResult { Success = string.IsNullOrEmpty(errorMessage), Message = errorMessage };
		}

		string PrintCarrierLabelCore(IStmPrintQueue printer, IOrganisationRTUSOption rtusOption, ICarrierLabelPrintingProviderManager carrierLabelPrintingProviderManager)
		{
			var errorBuilder = new ZStringBuilder();

			var carrierLabelProvider = carrierLabelPrintingProviderManager.GetCarrierLabelPrintingProvider(ex => errorBuilder.Append(ex.Message));
			Argument.NotNull(carrierLabelProvider, nameof(carrierLabelProvider));

			var result = carrierLabelProvider.PrintCarrierLabel(this, rtusOption.RTUSCBA, rtusOption.WrappedUrl, printer);
			if (!result.Success)
			{
				if (!errorBuilder.IsEmpty)
				{
					errorBuilder.Prepend(Res.GetString("6CB98E7F-F822-4475-BC9D-FCD6ADEE1880", "Details below:"));
				}

				if (string.IsNullOrEmpty(result.Message))
				{
					errorBuilder.Prepend(Res.GetString("41AF338D-F92C-4DB4-B70B-699925543847", "Failed to send Print Request to '{0}'.", printer.SQ_DisplayName));
				}
				else
				{
					errorBuilder.Prepend(result.Message);
				}
			}

			return errorBuilder.ToStringWithNewLineBetweenAppends();
		}

		#region CarrierLabelPrintingProviderManager

		class CarrierLabelPrintingProviderManager : ICarrierLabelPrintingProviderManager
		{
			public CarrierLabelPrintingProviderManager()
			{
				ICarrierLabelPrintingProvider provider = null;
				GetProvider = action =>
				{
					var manager = ObjectFactory.New<ICarrierLabelManager>(action, null);
					provider = ObjectFactory.Get<ICarrierLabelPrintingProvider>(nameof(ICarrierLabelPrintingProvider), action, manager, null);
					return provider;
				};

				Provider = new Lazy<ICarrierLabelPrintingProvider>(() => provider);
			}

			readonly Lazy<ICarrierLabelPrintingProvider> Provider;
			readonly Func<Action<Exception>, ICarrierLabelPrintingProvider> GetProvider;

			public void Dispose()
			{
				if (Provider.IsValueCreated)
				{
					Provider.Value.Dispose();
				}
			}

			public ICarrierLabelPrintingProvider GetCarrierLabelPrintingProvider(Action<Exception> action)
			{
				if (!Provider.IsValueCreated)
				{
					GetProvider(action);
				}

				return Provider.Value;
			}

			public string GetInvalidParentJobTypeErrorMessage(PkgPackage package) => Res.GetString("11baaebf-0bb3-454c-89dc-a0a5b3fd725e", "Packing parent job is not configured to send its packages to RTUS.");

			public bool IsParentJobValid(IPackingParent packingParent) => packingParent != null && packingParent.ParentJobType != ParentJobType.None;
		}

		#endregion

		#endregion

		#region Save

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				// tested in PkgPackageJobTest.TestIsReleasedCanBeChangedWhenFinalised()
				ValueSetStrategy.HasChangesThatAreInvalidIfFinalised = false;
			}
		}

		protected sealed override void OnFactorySaving()
		{
			base.OnFactorySaving();
			// tested in PackageIDGenerationHelperTest.TestOnFactorySave_ShouldGenerateIDOnSaving_DeterminesPackageIDGeneration()
			PackageIDGenerationHelper.AddPackageForIDGenerationIfMarked(Factory, this);

			if (ShouldCancelCarrierLabelOnSaving)
			{
				var result = CancelCarrierLabel();
				if (!result.Success && !result.Message.IsNullOrEmpty())
				{
					AddRTUSSubscriptionFailNote(this, result.Message);
				}
			}
		}

		static void AddRTUSSubscriptionFailNote(PkgPackage package, string errorMessage)
		{
			var noteParent = (IStmNoteParent)package.PackageJob.ParentJob;
			var noteText = Res.GetString("71c5d1c1-04ca-48f6-a4b5-65eeaf199c40", "Error occurred during Cancellation Process of Carrier Label(s). Packages for label cancellation must be canceled manually. Error: {0}", errorMessage);
			noteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.RTUSRequestLog.Description, noteText);
		}

		#region CancelCarrierLabel

		internal bool ShouldCancelCarrierLabelOnSaving { get; set; }

		ReturnResult CancelCarrierLabel()
		{
			var result = PackageCarrierLabelSubscriber.SubscribePackageForCancellation(this);

			if (result.Success)
			{
				// to force the after on saving service to include the package's IsSentToRTUS row
				IsSentToRTUS = false;
			}

			return result;
		}

		#endregion

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (!saveSucceeded)
			{
				// tested in PackageIDGenerationHelperTest.TestOnFactorySave_ShouldGenerateIDOnSaving_ShouldClearOnSaveFailure()
				PackageIDGenerationHelper.Clear(Factory);
				PackageCarrierLabelSubscriber.GetService(Factory).ClearPackagesToCancel();
			}
		}

		#endregion

		#region Delete

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public sealed override void Delete()
		{
			DeleteCore();
		}

		void DeleteCore(bool skipCanDeleteCheck = false)
		{
			var packageJobCache = new Lazy<PkgPackageJob>(() => PackageJob);
			if (!IsDeleted && (skipCanDeleteCheck || CheckCanDelete(packageJobCache.Value)))
			{
				using (new SemaphoreManager(IsDeletingPackageSemaphore))
				{
					var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, this);
					if (defaultPrinter != null)
					{
						defaultPrinter.Delete(); // Tested in StmDefaultPrinterTest
					}

					var packageJob = packageJobCache.Value;
					if (packageJob != null)
					{
						var parentJob = packageJob.ParentJob;
						parentJob?.OnPackageDelete(this);

						// implementing ParentJobType means this module can send its packages to RTUS
						if (IsInDatabase && parentJob != null && parentJob.ParentJobType != ParentJobType.None && IsSentToRTUS)
						{
							var result = PackageCarrierLabelSubscriber.SubscribePackageForCancellation(this);
							if (!result.Success && !result.Message.IsNullOrEmpty())
							{
								AddRTUSSubscriptionFailNote(this, result.Message);
							}
						}
					}

					AddItemRemovedLogEventIfRequired();
					var currentSequence = KP_Sequence;
					this.KP_KP_ParentPackage = ZGuid.Empty; // to reduce the parent package weight

					DeletePackageHandlingUnitDivotsIfRequired(); // delete divots and update inners for deleted package

					PackedItemDivots.DeleteAll(); //
					Packages.DeleteAll();         // tested by SaveAndDeleteBusinessObject()
					DeletePackageID(PackageID);
					DeleteContainer();
					DeleteBookedDimensions();
					Screenings.DeleteAll();
					JobServiceLinks.DeleteAll();

					base.Delete();

					packageJob?.PackageSequenceCalculator.ShiftSequence(currentSequence);
				}
			}
		}

		public override bool CanDelete
		{
			get { return IsDeleted || CheckCanDelete(PackageJob); }
		}

		bool CheckCanDelete(PkgPackageJob packageJob)
		{
			var result = true;

			if (packageJob != null)
			{
				if (actionStrategy == null)
				{
					actionStrategy = GetActionStrategy(packageJob);
				}

				result = IsActionAllowed(PackageAction.Delete);
				if (!result)
				{
					packageJob.FireOnPackageDeleteCanceled(this, ActionStrategy.ReasonForNotAllowingAction);
				}
				else
				{
					result = Packages.All(p => p.CheckCanDelete(packageJob));
				}
			}

			return result;
		}

		public bool IsDeletingPackage
		{
			get { return IsDeletingPackageSemaphore.IsSuspended; }
		}

		Semaphore IsDeletingPackageSemaphore { get { return isDeletingPackageSemaphore ?? (isDeletingPackageSemaphore = new Semaphore()); } }
		Semaphore isDeletingPackageSemaphore;

		#region PackageCarrierLabelCancellation

		internal class PackageCarrierLabelCancellation : IAfterOnSavingBOProcessingService
		{
			public PackageCarrierLabelCancellation(BusinessObjectFactory factory)
			{
				Factory = Argument.NotNull(factory, nameof(factory));
			}

			BusinessObjectFactory Factory { get; }

			readonly HashSet<PkgPackage> PackagesToCancel = new HashSet<PkgPackage>();

			public ReturnResult SubscribePackage(PkgPackage package)
			{
				ReturnResult result;
				PackagesToCancel.Add(package);

				if (!package.IsCancellingAPackageSentToRTUS)
				{
					package.IsCancellingAPackageSentToRTUS = true;
					result = GetCancellation().SubscribePackageForCancellation(package);
				}
				else
				{
					result = new ReturnResult { Success = true };
				}

				return result;
			}

			internal void ClearPackagesToCancel()
			{
				foreach (var package in PackagesToCancel)
				{
					if (!package.IsDeleted)
					{
						package.IsSentToRTUS = true;
					}
				}

				PackagesToCancel.Clear();
			}

			ICarrierLabelCancellation GetCancellation()
			{
				return Factory.GetCachedValue("PackageCarrierLabelCancellation|ICarrierLabelCancellation",
					() => ObjectFactory.New<ICarrierLabelCancellation>(null, null), CacheStalenessPolicy.StaleOnFactorySave);
			}

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				var deletedPackages = businessObjectsInOnSavingOrder
					.Where(b => b.TablePrefix == PkgPackageSchema.Constants.Prefix && b.IsDeleted)
					.Cast<PkgPackage>()
					.Where(p => p.IsCancellingAPackageSentToRTUS);

				var packagesToCancelDictionary = GetPackagesToCancelDictionary(PackagesToCancel);
				var responses = GetCancellation().CancelPackages(deletedPackages.Union(PackagesToCancel));

				try
				{
					foreach (var response in responses)
					{
						var packingParent = response.PackingParent;
						if (packingParent != null)
						{
							var noteParent = (IStmNoteParent)packingParent;
							var noteText = response.IsSuccessful
								? Res.GetString("e238258d-fbbe-4007-ae14-4d5d64654c9b", "Successfully canceled Package '{0}'.", response.PackageID)
								: response.ErrorMessageForFailure;

							noteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.RTUSRequestLog.Description, noteText);
						}

						if (!response.IsSuccessful
							&& packagesToCancelDictionary.TryGetValue((response.PackageJobPK, response.PackageID), out PkgPackage package))
						{
							package.IsSentToRTUS = true;
						}
					}
				}
				finally
				{
					PackagesToCancel.Clear();
				}
			}

			static Dictionary<(ZGuid, ZString), PkgPackage> GetPackagesToCancelDictionary(IEnumerable<PkgPackage> packagesToCancel)
			{
				var packagesToCancelDictionary = new Dictionary<(ZGuid, ZString), PkgPackage>();
				foreach (var package in packagesToCancel.Where(pkg => !pkg.IsDeleted))
				{
					var key = (package.KP_KJ_ParentPackageJob, package.KP_PackageID);
					if (!packagesToCancelDictionary.ContainsKey(key))
					{
						packagesToCancelDictionary.Add(key, package);
					}
				}

				return packagesToCancelDictionary;
			}
		}

		bool IsCancellingAPackageSentToRTUS;

		#endregion

		#region DeleteContainer

		void DeleteContainer()
		{
			if (IsContainer)
			{
				using (new SemaphoreManager(IsDeletingContainerSemaphore))
				{
					var container = Container;
					if (container != null)
					{
						PackageExtensions.DeleteAll();
						container.Delete();
					}
				}
			}
		}

		Semaphore IsDeletingContainerSemaphore
		{
			get { return isDeletingContainerSemaphore ?? (isDeletingContainerSemaphore = new Semaphore()); }
		}

		Semaphore isDeletingContainerSemaphore;

		#endregion

		#region DeleteEmptyPackagesIncludingChildren

		public void DeleteEmptyPackagesIncludingChildren()
		{
			if (!IsDeleted)
			{
				DeleteEmptyPackagesIncludingChildrenCore(PackageJob);
			}
		}

		void DeleteEmptyPackagesIncludingChildrenCore(PkgPackageJob packageJob)
		{
			if (CheckCanDelete(packageJob))
			{
				DeleteEmptyPackagesIncludingChildren(Packages, packageJob);

				if (!HasPackedItemsIncludingChildren)
				{
					DeleteCore(true);
				}
			}
		}

		void DeleteEmptyPackagesIncludingChildren(PkgPackageCollection packageCollection, PkgPackageJob packageJob)
		{
			Array.ForEach(packageCollection.ToArray(), p => p.DeleteEmptyPackagesIncludingChildrenCore(packageJob));
		}

		#endregion

		#region AddItemRemovedLogEventIfRequired

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		void AddItemRemovedLogEventIfRequired()
		{
			if (!KP_ClosedTimeUtcInfo.OriginalValue.IsEmpty && IsOuter && PackageJob?.ParentJob is IStmALogParent parentJob)
			{
				parentJob.Logs.AddNew(Events.ItemRemoved, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, PackType.IsNull ? Res.GetString("627711b4-7f57-4c9e-ba5e-01252434d9d6", "Package") : PackType.F3_DescriptionMultilingual),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, KP_PackageID)
				});
			}
		}

		#endregion

		#region DeletePackageHandlingUnitDivots

		void DeletePackageHandlingUnitDivotsIfRequired()
		{
			if (ShouldPackTrackedPackagesViaDivot)
			{
				PackageHandlingUnitPackageDivots.DeleteAll();
			}

			UnpackInnersAndUpdateTopHandlingUnitPackage();
		}

		public void UnpackInnersAndUpdateTopHandlingUnitPackage()
		{
			UnpackInnersAndUpdateTopHandlingUnitPackage(PackageHandlingUnitHandlingUnitDivots);
		}

		public void UnpackInnersAndUpdateTopHandlingUnitPackage(IEnumerable<PkgPackageHandlingUnitDivot> packageHandlingUnitDivots)
		{
			if (!ShouldPackTrackedPackagesViaDivot || packageHandlingUnitDivots == null || !packageHandlingUnitDivots.Any())
			{
				return;
			}

			var packagesToBeUpdated = PkgPackageHandlingUnitDivotHelper.GetAllInnerPackagesViaDivots(packageHandlingUnitDivots);

			// Unpack all direct inners
			packageHandlingUnitDivots.DeleteAll();

			if (packagesToBeUpdated.Count > 0)
			{
				foreach (var package in packagesToBeUpdated)
				{
					var topLevelPackage = PkgPackageHandlingUnitDivotHelper.GetTopLevelPackageViaDivots(package);
					package.KP_KP_TopHandlingUnitPackage = topLevelPackage != package ? topLevelPackage.PK : ZGuid.Empty;
				}
			}
		}

		#endregion

		#endregion

		#region IsTransitPackage

		public bool IsTransitPackage => Factory.LoadFromUniqueKey<Warehouse.Integration.IWhsItemPackageState>(WhsItemPackageStateSchema.WPS_KP_Package, PK) != null;

		#endregion

		#region IsTransitOverpackPackage

		public bool IsTransitOverpackPackage
		{
			get
			{
				var query = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, PK);
				query.AddToFilter(new ZQuery(WhsItemPackageStateSchema.WPS_UnitType, "OVP"));
				return Factory.LoadTop1<Warehouse.Integration.IWhsItemPackageState>(query) != null;
			}
		}

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || !IsActionAllowed(PackageAction.Edit); }
			set { base.ReadOnly = value; }
		}

		#endregion

		#region ToString() Summaries

		public ZString ToStringPackageSummary()
		{
			return Res.GetString("15e7eceb-8c2c-4143-ae57-eb6452b142be", "{0}x {1}", KP_PackageQty, Description);
		}

		public ZString ToStringDimensionSummary()
		{
			return Res.GetString("5fa4dd86-b7e1-4b81-8541-2cb6c26d6019", "{0} x {1} x {2} {3}", FormatNumber(KP_Length), FormatNumber(KP_Width), FormatNumber(KP_Height), KP_DimensionUQ);
		}

		/// <summary>
		/// Converts a number to one or two decimals depending on whether or not there is precision beyond the first decimal.
		/// 
		/// For example:
		///		1.555 --> "1.55"
		///		1.55  --> "1.55"
		///		1.5   --> "1.5"
		///		1     --> "1.0";
		///		0.5   --> "0.5"
		///		
		/// </summary>
		ZString FormatNumber(ZDecimal number)
		{
			var result = number.ToString("N2", CultureInfo.InvariantCulture);

			if (result[result.Length - 1] == '0')
			{
				result = result.Remove(result.Length - 1, 1);
			}

			return result;
		}

		#endregion

		#region Clone

		public new PkgPackage Clone()
		{
			return (PkgPackage)base.Clone();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[]
			{
				PkgPackageSchema.Constants.KP_KP_ParentPackage,
				PkgPackageSchema.Constants.KP_KJ_ParentPackageJob,
				PkgPackageSchema.Constants.KP_KPH_PackageHeader
			});

			var clone = (PkgPackage)base.CloneInternal(args);

			if (!args.IsExcludedFromCloning(Schema.Container))
			{
				CloneContainer(clone);
			}
			if (!args.IsExcludedFromCloning(Schema.UNDGs))
			{
				CloneUNDGs(clone);
			}

			return clone;
		}

		void CloneContainer(PkgPackage packageClone)
		{
			if (IsContainer)
			{
				var propertiesToExclude = new string[] { PkgPackageContainerSchema.Constants.K0_KP_Package };
				var containerClone = (PkgPackageContainer)Container.Clone(new BusinessObjectCloneArgs(propertiesToExclude));
				containerClone.K0_KP_Package = packageClone.PK;
			}
		}

		void CloneUNDGs(PkgPackage packageClone)
		{
			foreach (UNDGDataItem undg in UNDGs)
			{
				var propertiesToExclude = new string[] { UNDGDataItemSchema.Constants.DI_ParentID };
				var clonedDg = (UNDGDataItem)undg.Clone(new BusinessObjectCloneArgs(propertiesToExclude));
				clonedDg.DI_ParentID = packageClone.PK;
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		//

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new PkgPackageDocumentSupporter(this); }
		}

		#endregion

		#region IPackageSummary Members

		ZString IPackageSummary.Commodity
		{
			get { return KP_RH_NKCommodityCode; }
		}

		ZString IPackageSummary.Contents
		{
			get { return Packages.ToStringSummary(); }
		}

		ZString IPackageSummary.Dimensions
		{
			get { return ToStringDimensionSummary(); }
		}

		bool IPackageSummary.IsStatusSignificant
		{
			get { return false; }
		}

		public bool IsTemperatureControlled
		{
			get
			{
				var container = Container;
				return (container != null) ? container.K0_IsControlledAtmosphere : KP_RequiresTemperatureControl;
			}
		}

		ZString IPackageSummary.PackageID
		{
			get { return KP_PackageID.IsEmpty ? (ZString)"---" : KP_PackageID; }
		}

		public ZString Status
		{
			get
			{
				ZString result = "";

				if (IsReleased)
				{
					result = PackageStatuses.Released;
				}
				else if (KP_IsReleasedViaJob)
				{
					result = PackageStatuses.ReleasedViaJob;
				}
				else if (IsClosed)
				{
					result = PackageStatuses.Closed;
				}

				result = string.Join(", ", new[] { result, MostRecentLogDescription }.Where(s => !s.IsEmpty));

				return result;
			}
		}

		public ZString LatestScreeningResult
		{
			get
			{
				if (LatestScreening != null && LatestScreening.KPS_Passed)
				{
					return PkgPackageScreeningResults.Descriptions.PAS;
				}
				else if (LatestScreening != null && !LatestScreening.KPS_Passed)
				{
					return PkgPackageScreeningResults.Descriptions.FAI;
				}

				return PkgPackageScreeningResults.Descriptions.NOT;
			}
		}

		public ZString LatestScreeningMethod
		{
			get
			{
				var screeningMethod = new ScreeningMethods();

				if (LatestScreening != null)
				{
					return screeningMethod.GetDescriptionFromCode(LatestScreening.KPS_Method);
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public virtual ZString ScreeningMethod => ZString.Empty;

		public virtual ZBool IsHighRisk => ZBool.False;

		public virtual ZString AdditionalScreeningMethod => ZString.Empty;

		ZString IPackageSummary.Temperature
		{
			get
			{
				ZString result;

				var container = Container;
				if (container != null)
				{
					result = container.TemperatureSummary;
				}
				else
				{
					result = KP_RequiredTemperatureMinimum == KP_RequiredTemperatureMaximum

						? Res.GetString("edef5116-f56b-4393-95ef-0d02a4cfa321", "{0} °{1}",
							KP_RequiredTemperatureMinimum.ToStringTrimZeros("N"), KP_RequiredTemperatureUnit)

						: Res.GetString("608cde23-e55a-4a65-b8d4-995c09efb686", "{0} to {1} °{2}",
							KP_RequiredTemperatureMinimum.ToStringTrimZeros("N"), KP_RequiredTemperatureMaximum.ToStringTrimZeros("N"), KP_RequiredTemperatureUnit);
				}

				return result;
			}
		}

		ZString IPackageSummary.PackageSequence
		{
			get { return KP_KP_ParentPackage.IsEmpty ? PackageSequenceString : ZString.Empty; }
		}

		ZString IPackageSummary.Volume
		{
			get { return ZString.Format("{0} {1}", KP_Volume.ToStringTrimZeros("N3"), KP_VolumeUQ); }
		}

		ZString IPackageSummary.Weight
		{
			get { return ZString.Format("{0} {1}", KP_Weight.ToStringTrimZeros("N"), KP_WeightUQ); }
		}

		ZString IPackageSummary.IsHeld
		{
			get { return KP_IsHeld ? Res.GetString("BBD044FB-30D9-4684-B853-747DE8B6F190", "Yes") : Res.GetString("8A9D4186-44AE-44BB-AAEE-A11ACDE0FAE2", "No"); }
		}

		ZString IPackageSummary.HandlingUnit
		{
			get
			{
				var handlingUnit = ZString.Empty;
				if (!KP_KP_ParentPackage.IsValid)
				{
					handlingUnit = TopHandlingUnitPackage?.KP_PackageID ?? ZString.Empty;
				}
				return handlingUnit;
			}
		}

		#region Captions

		ZString IPackageSummary.CommodityCaption
		{
			get { return KP_RH_NKCommodityCode.IsEmpty ? "" : Res.GetString("279c146d-cc70-4d47-97c6-9edada42c45b", "Comm"); }
		}

		ZString IPackageSummary.DimensionsCaption
		{
			get { return Res.GetString("3a961ba8-5bb5-48d3-8422-be5a13fa454f", "Dims"); }
		}

		ZString IPackageSummary.PackageIDCaption
		{
			get { return "ID"; }
		}

		ZString IPackageSummary.TemperatureCaption
		{
			get { return IsTemperatureControlled ? Res.GetString("3601e930-dd77-484d-b2fc-ed1bc733578b", "Temp.") : ""; }
		}

		ZString IPackageSummary.PackageSequenceCaption
		{
			get { return KP_KP_ParentPackage.IsEmpty ? Res.GetString("0A7FB45B-C6EA-4030-B728-7EFBE79F0234", "Sequence") : ""; }
		}

		ZString IPackageSummary.VolumeCaption
		{
			get { return Res.GetString("846a1a75-16ca-4131-83f0-4bea28b5346d", "Vol"); }
		}

		ZString IPackageSummary.WeightCaption
		{
			get { return Res.GetString("9200ce26-8896-4b4c-807b-b059d33bc619", "Wgt"); }
		}

		ZString IPackageSummary.IsHeldCaption
		{
			get { return Res.GetString("07F28FA6-03E1-4D07-8446-9328EA41EC0C", "Held"); }
		}

		ZString IPackageSummary.HandlingUnitCaption
		{
			get { return !KP_KP_ParentPackage.IsValid && KP_KP_TopHandlingUnitPackage.IsValid ? Res.GetString("5de06a75-0c58-454e-b0bd-72807ae4a96e", "HU") : ""; }
		}

		#endregion

		#endregion

		#region IPackingHasChanges Members

		bool IPackingHasChanges.HasChangesThatAreInvalidIfFinalised
		{
			// tested in IBusinessExtensionsTest.TestHasChangesOnChildrenNotValidIfFinalised()
			get { return ValueSetStrategy.HasChangesThatAreInvalidIfFinalised || this.HasChangesOnChildrenNotValidIfFinalised(); }
		}

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new PkgPackageProcessHandlingInfo(this); }
		}

		#endregion

		#region ITemperatureSettings Members

		bool ITemperatureSettings.IsTemperatureControlled
		{
			get { return KP_RequiresTemperatureControl; }
			set { KP_RequiresTemperatureControl = value; }
		}

		ZDecimal ITemperatureSettings.TemperatureMin
		{
			get { return KP_RequiredTemperatureMinimum; }
			set { KP_RequiredTemperatureMinimum = value; }
		}

		ZDecimal ITemperatureSettings.TemperatureMax
		{
			get { return KP_RequiredTemperatureMaximum; }
			set { KP_RequiredTemperatureMaximum = value; }
		}

		ZString ITemperatureSettings.TemperatureUnit
		{
			get { return KP_RequiredTemperatureUnit; }
			set { KP_RequiredTemperatureUnit = value; }
		}

		#endregion

		#region IUNDGDataItemProvider Members

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (uNDGs == null)
				{
					uNDGs = new UNDGDataItemCollection(this);

					if (AllowLoadingOverpackChildrenUndgs && IsTransitOverpackPackage)
					{
						var pkgState = Factory.LoadTop1<Warehouse.Integration.IWhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, PK)) as IUNDGDataItemProvider;
						uNDGs.AddRange(pkgState.UNDGs);
					}

					RegisterEditableChildObject(uNDGs);
				}

				return uNDGs;
			}
		}

		UNDGDataItemCollection uNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public IEnumerable<IUNDGDataItem> UNDGDataItems => UNDGs.ToArray();

		public bool AllowLoadingOverpackChildrenUndgs { get; set; }

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber => KP_PackageID;

		#endregion

		#region IEDocsProvider Members

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new PkgPackageDocManagerInfo(this));
		DocManagerInfo docManagerInfo;

		[BusinessObjectTestExclude]
		public Func<BusinessObject[]> GetRelatedBusinessObjects { get; set; }

		public EDocsProviderSupporter GetEDocsProviderSupporter() => new EDocsProviderSupporter(this);

		#endregion

		#region ISupportPackageID Members

		ZGuid ISupportPackageIDGeneration.PackageJobPK => KP_KJ_ParentPackageJob;

		IEnumerable<ISupportPackageIDGeneration> ISupportPackageIDGeneration.Packages => Packages;

		bool ISupportPackageIDGeneration.ShouldGenerateIDOnSaving { get; set; }

		void ISupportPackageIDGenerationInternals.CallAfterIDGenerated() => AfterIDGenerated?.Invoke(this, EventArgs.Empty);
		public event EventHandler AfterIDGenerated;

		#endregion

		#region IPackageIDSequence Members

		ZShort IPackageSequence.Sequence
		{
			get => KP_Sequence;
			set => KP_Sequence = value;
		}

		ZGuid IPackageSequence.PackageHeaderFK => KP_KPH_PackageHeader;

		ZGuid IPackageSequence.PackageJobFK => KP_KJ_ParentPackageJob;

		bool IPackageSequence.RequiresSequencing => KP_KP_ParentPackage.IsEmpty;

		#endregion

		#region IsImportingData

		bool ISupportDataImporting.IsImportingData { get; set; }

		#endregion

		#region Validation

		protected override PkgPackageValidation GetNewValidation()
		{
			var packageJob = PackageJob;
			return packageJob == null || !packageJob.KJ_IsFinalized
				? GetNewValidationCoreForUnfinalisedPackageJob()
				: GetNewValidationCore();
		}

		protected virtual PkgPackageValidation GetNewValidationCore() => new PkgPackageValidation(this);
		protected virtual PkgPackageValidation GetNewValidationCoreForUnfinalisedPackageJob() => new PkgPackageValidationForUnfinalisedPackageJob(this);
		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			KP_F3_NKPackType = "PKG";
			if (PackageJob == null)
			{
				KP_KJ_ParentPackageJob = Factory.NewWithValidTestData<PkgPackageJob>().PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
	}
}


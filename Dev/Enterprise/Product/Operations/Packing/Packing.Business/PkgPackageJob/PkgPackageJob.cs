using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business
{
	public interface IPackageParent
	{
		PkgPackage ParentPackage { get; }
	}

	[DebuggerDisplay("UniqueID={KJ_JobID}")]
	[UniversalCopyWithExtendedEntities]
	public class PkgPackageJob : AutoPkgPackageJob, IPkgPackageJob, IPackageSummary, IDocumentSupportable
	{
		public PkgPackageJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(KJ_IsFinalized), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(KJ_CriticalChangesVersionID), ConcurrencyPolicy.Ignore);
		}

		public static readonly PkgPackageJobTypeDecider TypeDecider = new PkgPackageJobTypeDecider();

		#region Static Loader

		#region LoadPackageJob

		/// <summary>
		/// Loads a PkgPackageJob for the parent entity. Returns null if the PkgPackageJob does not exist.
		/// </summary>
		public static PkgPackageJob LoadPackageJob(IPackingParent parent)
		{
			Argument.NotNull(parent, "parent");
			Argument.NotNull(parent.Factory, "parent.Factory");

			var result = parent.Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, parent.PK)); // there should only ever be one.

			if (result != null)
			{
				((BusinessObject)parent).RegisterEditableChildObject(result);
				result.UpdateReadOnly();
				parent.OnPackageJobCreatedOrLoaded(result);
			}

			return result;
		}

		public static PkgPackageJob LoadPackageJob(BusinessObjectFactory factory, ZGuid pk)
		{
			factory.AddFetchHint(PkgPackageSchema.Instance, new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, pk));
			factory.AddFetchHint(typeof(PkgPackageJobPackageHeaderPivot), PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, pk);

			var result = factory.Load<PkgPackageJob>(pk);
			if (result != null)
			{
				result.UpdateReadOnly();
			}

			return result;
		}

		#endregion

		#region LoadOrCreatePackageJob

		/// <summary>
		/// Loads or creates a PkgPackageJob for the parent entity.
		/// </summary>
		public static PkgPackageJob LoadOrCreatePackageJob(IPackingParent parent)
		{
			return LoadOrCreatePackageJobCore(parent);
		}

		/// <summary>
		/// Loads or creates a PkgPackageJob for the parent entity.
		/// If a new PackageJob is created, HasChanges will be set false *before* registering as editable on the parent.
		/// </summary>
		public static PkgPackageJob LoadOrCreatePackageJobWithNoChanges(IPackingParent parent)
		{
			return LoadOrCreatePackageJobCore(parent, noChangesOnNew: true);
		}

		static PkgPackageJob LoadOrCreatePackageJobCore(IPackingParent parent, bool noChangesOnNew = false)
		{
			Argument.NotNull(parent, "parent");
			Argument.NotNull(parent.Factory, "parent.Factory");

			PkgPackageJob result = LoadPackageJob(parent);

			if (result == null)
			{
				var pkgPackageJobType = PkgPackageJobTypeDecider.GetTypeByParentTableCode(parent.TablePrefix) ?? typeof(PkgPackageJob);
				result = (PkgPackageJob)parent.Factory.New(pkgPackageJobType);
				result.KJ_ParentID = parent.PK;
				result.KJ_ParentTableCode = parent.TablePrefix;
				result.KJ_IsFinalized = parent.IsParentJobFinalised;

				if (noChangesOnNew)
				{
					result.HasChanges = false;
				}
				((BusinessObject)parent).RegisterEditableChildObject(result);
				parent.OnPackageJobCreatedOrLoaded(result);
			}

			return result;
		}

		#endregion

		#region LoadParent

		/// <summary>
		/// Loads the parent entity for a PkgPackageJob.
		/// </summary>
		public static T LoadParent<T>(PkgPackageJob packageJob) where T : class
		{
			Argument.NotNull(packageJob, "packageJob");
			var job = packageJob.Factory.Load(packageJob.KJ_ParentTableCode, packageJob.KJ_ParentID);
			return job as T;
		}

		#endregion

		#endregion

		#region Related Entities

		#region ParentJob

		public IPackingParent ParentJob
		{
			get { return LoadParent<IPackingParent>(this); }
		}

		#endregion

		#region Packages

		/// <summary>
		/// Returns a collection of the top level PkgPackages on this PackageJob.
		/// </summary>
		[ChildEditable]
		[UniversalCopyCollectionEntity(PkgPackageSchema.Constants.TableName, PkgPackageSchema.Constants.KP_KJ_ParentPackageJob)]
		public PkgPackageCollection Packages
		{
			get
			{
				if (packages == null || ShouldReloadPackagesCollection)
				{
					ShouldReloadPackagesCollection = false;
					packages = GetPackageCollectionCore();
					RegisterEditableChildObject(packages);
				}
				return packages;
			}
		}

		PkgPackageCollection packages;

		public bool ShouldReloadPackagesCollection { get; set; }

		protected virtual PkgPackageCollection GetPackageCollectionCore() => new PkgPackageCollection(this);

		public bool ContainsPackage(PkgPackage package)
		{
			return ContainsPackageCore(package);
		}

		protected virtual bool ContainsPackageCore(PkgPackage package)
		{
			return package.KP_KJ_ParentPackageJob == this.PK;
		}

		#endregion

		#region AllPackages

		public PkgPackage[] GetAllPackagesOnJob() => GetAllPackagesOnJobCore();

		protected virtual PkgPackage[] GetAllPackagesOnJobCore() => Factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, PK));

		#endregion

		#region AllHeldPackages

		public ReadOnlyCollection<PkgPackage> AllHeldPackages
		{
			get
			{
				var query = new ZQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, PK);
				query.AddToFilter(new ZQuery(PkgPackageSchema.KP_IsHeld, true));
				return new List<PkgPackage>(Factory.Load<PkgPackage>(query)).AsReadOnly();
			}
		}

		#endregion

		#region GetAllNonContainerOutersAndFirstLevelPackagesOnContainers

		/// <summary>
		/// Returns all Outers that are *not* Containers, and the top-level inners that are *on* Containers.
		///
		/// For example, given the following hierarchy:
		///
		///		Container1
		///			Box1
		///		Container2
		///			Box2
		///		Pallet1
		///		Pallet2
		///
		///		...would return Box1, Box2, Pallet1, Pallet2.
		/// </summary>
		public ReadOnlyCollection<PkgPackage> GetAllNonContainerOutersAndFirstLevelPackagesOnContainers()
		{
			var result = new List<PkgPackage>();

			foreach (var package in Packages)
			{
				if (package.IsContainer)
				{
					result.AddRange(package.Packages);
				}
				else
				{
					result.Add(package);
				}
			}

			return result.AsReadOnly();
		}

		#endregion

		#region Containers

		[BusinessObjectTestExclude]
		public PkgPackageCollectionContainersOnly Containers
		{
			get { return containers ?? (containers = new PkgPackageCollectionContainersOnly(this)); }
		}

		PkgPackageCollectionContainersOnly containers;

		#endregion

		#region PackableItemParents

		public PackableItemParentWrapperCollection PackableItemParents
		{
			get
			{
				if (packableItemParents == null)
				{
					var parentJob = ParentJob as IPackingParentWithPackableItems;
					if (parentJob != null && parentJob.PackableItemParents != null)
					{
						packableItemParents = new PackableItemParentWrapperCollection(parentJob);
						packableItemParents.WrapPackableItemParents();
					}
					else
					{
						packableItemParents = new PackableItemParentWrapperCollection(Factory);
					}
				}
				return packableItemParents;
			}
		}

		PackableItemParentWrapperCollection packableItemParents;

		#endregion

		#region LoosePackageIDs

		[ChildEditable(true)]
		public PkgPackageHeaderCollection LoosePackageIDs
		{
			get
			{
				if (loosePackageIDs == null)
				{
					loosePackageIDs = new PkgPackageHeaderCollection(this);
					RegisterEditableChildObject(loosePackageIDs);
				}
				return loosePackageIDs;
			}
		}

		PkgPackageHeaderCollection loosePackageIDs;

		#endregion

		#region LoosePackagePivots

		[ChildEditable(true)]
		public PkgPackageJobPackageHeaderPivotCollection LoosePackagePivots
		{
			get
			{
				if (loosePackagePivots == null)
				{
					loosePackagePivots = new PkgPackageJobPackageHeaderPivotCollection(this);
					RegisterEditableChildObject(loosePackagePivots);
				}
				return loosePackagePivots;
			}
		}

		PkgPackageJobPackageHeaderPivotCollection loosePackagePivots;

		#endregion

		#region IsParentJobPackingParentWithPackableItems

		public ZBool IsParentJobPackingParentWithPackableItems => ParentJob is IPackingParentWithPackableItems;

		#endregion

		#endregion

		#region Properties

		// persistent

		#region KJ_ParentTableCode

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString KJ_ParentTableCode
		{
			get { return base.KJ_ParentTableCode; }
			set { base.KJ_ParentTableCode = value; }
		}

		#endregion

		#region KJ_IsFinalized

		public override ZBool KJ_IsFinalized
		{
			get { return base.KJ_IsFinalized; }
			set
			{
				var previousValue = KJ_IsFinalized;
				base.KJ_IsFinalized = value;

				if (KJ_IsFinalized != previousValue)
				{
					OnIsFinalisedChanged();
				}

				UpdateCriticalChangesVersion();
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(KJ_CriticalChangesVersionID), KJ_IsFinalized ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Ignore);
			}
		}

		void OnIsFinalisedChanged()
		{
			if (KJ_IsFinalized && !IsPackageClosureSuspended)
			{
				ClosePackages();
			}

			UpdateReadOnly();

			FireIsFinalisedChanged();
		}

		void FireIsFinalisedChanged()
		{
			if (IsFinalisedChanged != null)
			{
				IsFinalisedChanged(this, EventArgs.Empty);
			}
		}

		public void OnParentJobIsReadOnlyChanged()
		{
			UpdateReadOnly();

			if (ParentIsReadOnlyChanged != null)
			{
				ParentIsReadOnlyChanged(this, EventArgs.Empty);
			}
		}

		void UpdateReadOnly()
		{
			using (GetValidationSuspender())
			{
				SetReadOnlyIncludingChildren(KJ_IsFinalized || (ParentJob?.IsPackingJobReadOnly ?? false));
			}
		}

		void ClosePackages()
		{
			using (PropagationDeferrer.DeferEventPropagation(Factory))
			{
				foreach (var p in Packages)
				{
					p.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				}
			}
		}

		public event EventHandler IsFinalisedChanged;
		public event EventHandler ParentIsReadOnlyChanged;

		public IDisposable SuspendClosingPackagesOnFinalize() => new SemaphoreManager(SuspendPackageClosureOnJobFinalization);

		Semaphore SuspendPackageClosureOnJobFinalization => suspendPackageClosureOnJobFinalization ??= new Semaphore();
		Semaphore suspendPackageClosureOnJobFinalization;

		bool IsPackageClosureSuspended => suspendPackageClosureOnJobFinalization != null && suspendPackageClosureOnJobFinalization.IsSuspended;

		#endregion

		#region KJ_ReleasedTimeUtc

		[ReadOnly(true)]
		public override ZDateTime KJ_ReleasedTimeUtc
		{
			get => base.KJ_ReleasedTimeUtc;
			set
			{
				base.KJ_ReleasedTimeUtc = value;
				SetReleasedByUserAndIsReleasedFlag(value.IsEmpty);
				foreach (var package in Packages)
				{
					if (!value.IsEmpty)
					{
						package.KP_IsReleasedViaJob = true;
					}
					else
					{
						package.KP_IsReleasedViaJob = false;
						package.KP_ReleasedTimeUtc = ZDateTime.Empty;
					}
				}

				if (KJ_ReleasedTimeUtc.IsValid)
				{
					FirePackageJobReleasedIfNecessary();
				}
			}
		}

		void SetReleasedByUserAndIsReleasedFlag(bool isEmptyReleasedByDate)
		{
			KJ_GS_NKReleasedBy = isEmptyReleasedByDate ? string.Empty : Env.CurrentUser.Initials;
			KJ_IsReleased = !isEmptyReleasedByDate;
		}

		[ReadOnly(true)]
		public override ZString KJ_GS_NKReleasedBy { get => base.KJ_GS_NKReleasedBy; set => base.KJ_GS_NKReleasedBy = value; }

		[ReadOnly(true)]
		public override ZBool KJ_IsReleased { get => base.KJ_IsReleased; set => base.KJ_IsReleased = value; }

		internal void FirePackageJobReleasedIfNecessary()
		{
			if (IsReleased || (Packages.Any() && Packages.All(p => p.IsReleased || p.KP_IsReleasedViaJob)))
			{
				Logs.AddNew(AutoEvents.Released, "Released");
				var parentJob = ParentJob;
				if (parentJob != null)
				{
					ParentJob.OnPackageJobReleased();
				}
			}
		}

		#endregion

		#region Weight

		/// <summary>
		/// Returns the summed weight of **top level** packages, converted to WeightUQ.
		/// </summary>
		public ZDecimal Weight
		{
			get
			{
				ZDecimal result = 0m;
				ZString targetUQ = WeightUQ;

				if (Constants.Weight.ContainsCode(targetUQ))
				{
					foreach (var package in Packages.Where(p => p.KP_Weight > 0 && !p.KP_WeightUQ.IsEmpty))
					{
						if (Constants.Weight.ContainsCode(package.KP_WeightUQ))
						{
							result += Constants.Weight.Convert(package.KP_Weight, package.KP_WeightUQ, targetUQ);
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region WeightUQ

		/// <summary>
		/// Returns the default Packing Weight UQ (set via the Registry).
		/// </summary>
		public ZString WeightUQ
		{
			get { return PackingRegistry.Instance.WeightUnit.Value; }
		}

		#endregion

		#region Volume

		/// <summary>
		/// Returns the summed Volume of **top level** packages, converted to VolumeUQ.
		/// </summary>
		public ZDecimal Volume
		{
			get
			{
				ZDecimal result = 0m;
				ZString targetUQ = VolumeUQ;

				if (Constants.Volume.ContainsCode(targetUQ))
				{
					foreach (var package in Packages.Where(p => p.KP_Volume > 0 && !p.KP_VolumeUQ.IsEmpty))
					{
						if (Constants.Volume.ContainsCode(package.KP_VolumeUQ))
						{
							result += Constants.Volume.Convert(package.KP_Volume, package.KP_VolumeUQ, targetUQ);
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region VolumeUQ

		/// <summary>
		/// Returns the default Packing Volume UQ (set via the Registry).
		/// </summary>
		public ZString VolumeUQ
		{
			get { return PackingRegistry.Instance.VolumeUnit.Value; }
		}

		#endregion

		// strategy for haschanges

		#region SetStrategy

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return ValueSetStrategy;
		}

		PackingValueSetStrategy ValueSetStrategy => valueSetStrategy
			?? (valueSetStrategy = new PackingValueSetStrategy(this, PkgPackageJobSchema.KJ_IsReleased, PkgPackageJobSchema.KJ_ReleasedTimeUtc, PkgPackageJobSchema.KJ_GS_NKReleasedBy));
		PackingValueSetStrategy valueSetStrategy;

		#endregion

		// calculated

		#region Contents

		public ZString Contents
		{
			get { return Packages.ToStringSummary(); }
		}

		#endregion

		#region LastUsedOuterPackType

		public ZString LastUsedOuterPackType
		{
			get { return lastUsedOuterPackType.IsEmpty ? GetDefaultOuterPackType() : lastUsedOuterPackType; }
			internal set { lastUsedOuterPackType = value; }
		}

		ZString lastUsedOuterPackType;

		#endregion

		#region LastUsedInnerPackType

		public ZString LastUsedInnerPackType
		{
			get { return lastUsedInnerPackType.IsEmpty ? (ZString)PackingRegistry.Instance.InnerPackageUnit.Value : lastUsedInnerPackType; }
			internal set { lastUsedInnerPackType = value; }
		}

		ZString lastUsedInnerPackType;

		#endregion

		#region PackageIDSequenceCalculator

		public PackageSequenceCalculator PackageSequenceCalculator => packageSequenceCalculator ?? (packageSequenceCalculator = new PackageSequenceCalculator(this));

		PackageSequenceCalculator packageSequenceCalculator;

		#endregion

		#endregion

		#region Flags

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IsPackageIdValid

		public bool IsPackageIdValid(PkgPackage package)
		{
			return package.KP_PackageID.IsEmpty || IsPackageIdValid(package, Packages);
		}

		bool IsPackageIdValid(PkgPackage package, PkgPackageCollection packageCollection)
		{
			var isValid = true;

			foreach (var p in packageCollection)
			{
				if (p.KP_PackageID.EqualsIgnoringCase(package.KP_PackageID) && p.PK != package.PK)
				{
					isValid = false;
				}
				else
				{
					isValid = IsPackageIdValid(package, p.Packages);
				}

				if (!isValid)
				{
					break;
				}
			}

			return isValid;
		}

		#endregion

		#region ShowBasicLabelOnly

		/// <summary>
		/// Must be a ZBool, so that Document Customisation can see the Property.
		/// </summary>
		public ZBool ShowBasicLabelOnly
		{
			get
			{
				var parentJob = ParentJob;
				return parentJob != null && parentJob.DocumentOptions.HasFlag(DocumentOptions.ShowBasicLabelOnly);
			}
		}

		#endregion

		#endregion

		#region Events

		#region OnParentJobNumberChanged

		public void OnParentJobNumberChanged()
		{
			if (ParentJobNumberChanged != null)
			{
				ParentJobNumberChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler ParentJobNumberChanged;

		#endregion

		#region PackageDataChanged

		internal void FirePackageDataChanged(PkgPackage package, PackageDataChangeType packageDataChangeType)
		{
			if (package.IsOuter && PackageDataChanged != null)
			{
				PackageDataChanged(this, new PackageDataChangeEventArgs(package, packageDataChangeType));
			}
		}

		public event EventHandler<PackageDataChangeEventArgs> PackageDataChanged;

		#endregion

		#region OuterPackageAdded

		internal void FireOuterPackageAdded(PkgPackage package)
		{
			if (package.IsOuter && OuterPackageAdded != null)
			{
				OuterPackageAdded(this, new PackageEventArgs(package));
			}
		}

		public event EventHandler<PackageEventArgs> OuterPackageAdded;

		#endregion

		#region OuterParentPackageChanged

		internal void FireParentPackageChangedOnOuter(PkgPackage package)
		{
			if (!package.IsOuter && OuterParentPackageChanged != null)
			{
				OuterParentPackageChanged(this, new PackageEventArgs(package));
			}
		}

		public event EventHandler<PackageEventArgs> OuterParentPackageChanged;

		#endregion

		#endregion

		#region InitiateMassPackageProcess

		public IDisposable InitiateMassPackageProcess_WithNoDefer() => new SemaphoreManager(MassPackageProcessSemaphore);

		public IDisposable InitiateMassPackageProcess()
		{
			var manager = InitiateMassPackageProcess_WithNoDefer();
			return new DisposableAction(() =>
			{
				manager.Dispose();

				if (!IsMassPackageProcessRunning)
				{
					MassPackageProcessFinished?.Invoke(this, EventArgs.Empty);
				}
			});
		}

		public event EventHandler MassPackageProcessFinished;

		public bool IsMassPackageProcessRunning => massPackageProcessSemaphore != null && massPackageProcessSemaphore.IsSuspended;

		Semaphore MassPackageProcessSemaphore => massPackageProcessSemaphore ?? (massPackageProcessSemaphore = new Semaphore());
		Semaphore massPackageProcessSemaphore;

		#endregion

		#region CloseAndGenerateIDs

		public void CloseAndGenerateIDs(INotifications notify)
		{
			PackageIDGenerator.GenerateIDsForAllPackagesAndConsumeFountainImmediately_DoNotUse(this, Packages, notify, SSCCGenerationContext.AutoClosingPackage);
			foreach (var pkg in Packages)
			{
				pkg.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			}
		}

		#endregion

		#region RemoveHoldOfAllPackages

		public void RemoveHoldOfAllPackages()
		{
			AllHeldPackages.ForEach(p => p.KP_IsHeld = false);
		}

		#endregion

		#region GetAllPackageIDs

		public IEnumerable<ZString> GetAllPackageIDs()
		{
			return AllPackageIDs.Where(p => !p.KPH_PackageID.IsEmpty).Select(p => p.KPH_PackageID);
		}

		IEnumerable<PkgPackageHeader> AllPackageIDs
		{
			get { return LoosePackageIDs.Concat(GetAllPackagesOnJob().Select(p => p.PackageID).Where(i => i != null)); }
		}

		#endregion

		#region FindPackageByRawBarcodeOrAddNewIfSSCC

		public PkgPackage FindPackageByRawBarcodeOrAddNewIfSSCC(ZString rawBarcode)
		{
			// check if this is an SSCC
			var ssccPrefix = ParentJob.GetSSCCPrefix(new NotificationBuffer(), SSCCGenerationContext.CheckIfBarcodeIsSSCC);
			var barcodeMinus00Prefix = rawBarcode.SubstringSafe(2); // remove the '00' prefix
			var isValidSSCC = !string.IsNullOrEmpty(ssccPrefix) && SSCCBarCodeChecker.IsValidSSCC(barcodeMinus00Prefix, ssccPrefix);

			var actualBarcode = isValidSSCC ? barcodeMinus00Prefix : rawBarcode;
			var result = FindPackageByID(actualBarcode);

			if (result == null && isValidSSCC)
			{
				var selectedPackages = Selected.SelectedPackages;
				if (selectedPackages.Count == 1 && selectedPackages[0].KP_PackageID.IsEmpty)
				{
					// plug into existing package
					result = selectedPackages[0];
					result.KP_PackageID = actualBarcode;
				}
				else
				{
					// plug into new package
					result = Packages.AddNew();
					result.KP_PackageID = actualBarcode;
				}
				// if not an SSCC the barcode could be anything (ie a bad product), so don't do anything with it.
			}

			return result;
		}

		PkgPackage FindPackageByID(ZString packageID)
		{
			return GetAllPackagesOnJob().FirstOrDefault(p => p.KP_PackageID.EqualsIgnoringCase(packageID));
		}

		#endregion

		#region GetDefaultOuterPackType

		public ZString GetDefaultOuterPackType()
		{
			var parent = ParentJob as IPackingParentDefaultPackageType;
			var defaultPackTypeFromParent = parent != null ? parent.DefaultOuterPackType : ZString.Empty;
			return defaultPackTypeFromParent.IsEmpty ? (ZString)PackingRegistry.Instance.OuterPackageUnit.Value : defaultPackTypeFromParent;
		}

		#endregion

		#region AssignPackageID

		public void AssignPackageIDs(PkgPackage package, IReadOnlyCollection<PkgPackageHeader> selectedPackageHeaders)
		{
			Argument.NotNull(package, nameof(package));
			Argument.NotNull(selectedPackageHeaders, nameof(selectedPackageHeaders));

			if (package.KP_PackageID.IsEmpty)
			{
				foreach (var packageHeader in selectedPackageHeaders.Where(h => !IsPackageIDAlreadyAssigned(h)))
				{
					if (package.KP_PackageQty > 1)
					{
						var splitPackage = package.Clone();
						AssignPackageHeader(splitPackage, packageHeader);
						Packages.Add(splitPackage);

						package.KP_PackageQty = package.KP_PackageQty - 1;
					}
					else
					{
						AssignPackageHeader(package, packageHeader);
						break;
					}
				}
			}
		}

		void AssignPackageHeader(PkgPackage package, PkgPackageHeader header)
		{
			package.KP_PackageQty = 1;
			package.KP_KPH_PackageHeader = header.PK;
			LoosePackageIDs.RemoveFromRelationship(header);
		}

		#endregion

		#region IsPackageIDAlreadyAssigned

		public bool IsPackageIDAlreadyAssigned(PkgPackageHeader packageHeader)
		{
			return FindPackageByID(packageHeader.KPH_PackageID) != null;
		}

		#endregion

		#region UnassignPackageIDs

		public void UnassignPackageIDs(IReadOnlyCollection<PkgPackage> packagesToUnassignIDs)
		{
			Argument.NotNull(packagesToUnassignIDs, nameof(packagesToUnassignIDs));

			foreach (var package in packagesToUnassignIDs.Where(p => p.PackageID != null))
			{
				var packageHeader = package.PackageID;
				LoosePackageIDs.Add(packageHeader);
				package.KP_KPH_PackageHeader = ZGuid.Empty;
			}
		}

		#endregion

		#region Selected

		public SelectionHelper Selected
		{
			get { return selected ?? (selected = new SelectionHelper(this)); }
		}

		SelectionHelper selected;

		#endregion

		#region Packing

		#region GetPackedQty

		public ZDecimal GetPackedQty(IPackableItemParent packableItemParent) => packableItemParent.GetPackedQty(GetAllPackagesOnJob());

		#endregion

		#region IsPacked

		public bool IsPacked(IPackableItemParent packableItemParent) => packableItemParent.GetPackedDivots(GetAllPackagesOnJob()).Any();
		public bool IsPacked(IPackableItem packableItem) => GetAllPackagesOnJob().Any(p => p.IsPacked(packableItem));

		#endregion

		#region AutoPack

		public void AutoPack(INotifications notify)
		{
			var parentJobWithPackableItems = ParentJob as IPackingParentWithPackableItems;
			if (parentJobWithPackableItems == null)
			{
				notify.AddInformation(NothingToAutoPackMessage);
			}
			else if (Packages.Count > 0)
			{
				notify.AddInformation(Res.GetString("3ec9eafe-3329-49bd-990e-871d0a2839d9",
					"The Auto-Pack function requires that no packages exist in order to run.\r\nPlease remove your packages before running Auto-Pack."));
			}
			else
			{
				var isAutoPackAllowed = parentJobWithPackableItems.IsAutoPackAllowed;
				if (isAutoPackAllowed)
				{
					AutoPackCore(notify);
				}
				else
				{
					notify.AddInformation(isAutoPackAllowed.ReasonForNotAllowed);
				}
			}
		}

		public static string NothingToAutoPackMessage
		{
			get { return Res.GetString("673e4e1b-0156-4c65-a8b4-73b4f13092d2", "There is nothing to Auto-Pack."); }
		}

		void AutoPackCore(INotifications notify)
		{
			// update to group by KeyForAutoPack tested in WhsOrderTest for scenarios where KeyForAutoPack is different from Key
			var (packableItemParentsGroupedByKeyForAutoPack, itemParentsNotPacked) = GetPackableItemParentsGroupedForAutoPack(PackableItemParents);

			using (InitiateMassPackageProcess())
			{
				packableItemParentsGroupedByKeyForAutoPack.ForEach(packableItemParentsGroupedForAutoPack => AutoPackCore(packableItemParentsGroupedForAutoPack.Value));
			}

			var itemsWerePacked = itemParentsNotPacked.Count < PackableItemParents.Count;
			if (itemsWerePacked)
			{
				FireAutoPackEvent();
			}

			NotifyItemsNotPacked(itemParentsNotPacked, notify);
		}

		static (Dictionary<GroupingKey, List<IPackableItemParent>>, List<IPackableItemParent>) GetPackableItemParentsGroupedForAutoPack(IEnumerable<PackableItemParentWrapper> packableItemParentWrappers)
		{
			var itemParentsNotPacked = new List<IPackableItemParent>();
			var packableItemParentsGroupedForAutoPack = new Dictionary<GroupingKey, List<IPackableItemParent>>();
			foreach (var packableItemParentWrapper in packableItemParentWrappers)
			{
				var packableItemParent = packableItemParentWrapper.PackableItemParent;
				if (packableItemParent.AutoPackQtyPerPackage <= 0 || packableItemParent.AutoPackPackageType.IsEmpty)
				{
					itemParentsNotPacked.Add(packableItemParent);
				}
				else
				{
					var keyForAutoPack = packableItemParentWrapper.Key.KeyForAutoPack;
					AddToPackableItemParentsDictionaryForAutoPack(packableItemParentsGroupedForAutoPack, keyForAutoPack, packableItemParent);
				}
			}

			return (packableItemParentsGroupedForAutoPack, itemParentsNotPacked);
		}

		static void AddToPackableItemParentsDictionaryForAutoPack(Dictionary<GroupingKey, List<IPackableItemParent>> packableItemParentsGroupedForAutoPack, GroupingKey keyForAutoPack, IPackableItemParent packableItemParent)
		{
			if (!packableItemParentsGroupedForAutoPack.TryGetValue(keyForAutoPack, out var existingGroupedPackableItemParentsForAutoPack))
			{
				var groupedPackableItemParentsForAutoPack = new List<IPackableItemParent>(new[] { packableItemParent });
				packableItemParentsGroupedForAutoPack.Add(keyForAutoPack, groupedPackableItemParentsForAutoPack);
			}
			else
			{
				existingGroupedPackableItemParentsForAutoPack.Add(packableItemParent);
			}
		}

		void AutoPackCore(List<IPackableItemParent> groupedPackableItemParentsForAutoPack)
		{
			var packableItemParentReference = groupedPackableItemParentsForAutoPack[0];
			var packageType = packableItemParentReference.AutoPackPackageType;
			var qtyPerPackage = (decimal)packableItemParentReference.AutoPackQtyPerPackage;

			PkgPackage package = null;
			var packageQtyAvailableForPacking = 0m;

			foreach (var packableItemParent in groupedPackableItemParentsForAutoPack)
			{
				var packableItemParentQtyToPack = (decimal)packableItemParent.TotalQty;
				var itemsQueue = new Queue<IPackableItem>(packableItemParent.PackableItems.OrderBy(i => i.Quantity).Where(i => i.IsUnpacked(Factory)));

				while (packableItemParentQtyToPack > 0m)
				{
					if (packageQtyAvailableForPacking == 0m)
					{
						package = Packages.AddNew(packageType);
						packageQtyAvailableForPacking = qtyPerPackage;
					}

					var qtyToPack = Math.Min(packableItemParentQtyToPack, packageQtyAvailableForPacking);
					package.PackForAutoPack(packableItemParent, itemsQueue, qtyToPack);

					packableItemParentQtyToPack -= qtyToPack;
					packageQtyAvailableForPacking -= qtyToPack;
				}
			}
		}

		void FireAutoPackEvent()
		{
			var parent = ParentJob as IStmALogParent;
			if (parent != null)
			{
				parent.Logs.CreateOrRecreateEventLog(Events.ServiceCompleted, EstimateActual.Actual, ZDateTimeOffset.Now, "",
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.AutoPack));
			}
		}

		void NotifyItemsNotPacked(List<IPackableItemParent> itemParentsNotPacked, INotifications notify)
		{
			if (itemParentsNotPacked.Count > 0)
			{
				var message = "\r\n" + Res.GetString("70d198e9-aed8-4c41-bccb-29c47d88189c",
					"The following items could not be Auto-Packed because on the {0}, the Qty per Package is 0, or no Package Unit is defined:", ParentJob.JobDescription) + "\r\n";

				foreach (var itemParentNotPacked in itemParentsNotPacked)
				{
					message += "\r\n	" + Res.GetString("370fc988-2116-4f27-a0d2-8065ac6f23b6", "{0} x {1}", itemParentNotPacked.TotalQty.ToStringTrimZeros(), itemParentNotPacked.Description);
				}

				notify.AddWarning(message);
			}
		}

		#endregion

		#region MultiPack

		/// <summary>
		/// Creates n Packages and packs items according to the information proposed by the PackableItemWrappers.
		/// </summary>
		/// <param name="packageQtyToCreate">The number of Packages to create.</param>
		/// <param name="packageTypeToCreate">The Package type to create (eg. PLT, BOX etc.)</param>
		/// <param name="packableItemParentWrappers">The items to be packed. The ProposedQty of each item determines how many items to pack into each package.</param>
		/// <param name ="parentPackage">The Package to create new Packages in.</param>
		/// <returns>The packed packages.</returns>
		public PkgPackage[] MultiPack(ZInt packageQtyToCreate, ZString packageTypeToCreate, IEnumerable<PackableItemParentWrapper> packableItemParentWrappers, PkgPackage parentPackage = null)
		{
			PkgPackage[] result = null;

			if (packageQtyToCreate > 0 && !packageTypeToCreate.IsEmpty)
			{
				result = CreatePackages(packageQtyToCreate, packageTypeToCreate, parentPackage);
				MultiPack(result, packableItemParentWrappers);
			}

			return result;
		}

		/// <summary>
		/// Packs items according to the information proposed by the PackableItemWrappers.
		/// </summary>
		/// <param name="packagesToPackInto">The packages to pack items into.</param>
		/// <param name="packableItemParentWrappers">The items to be packed. The ProposedQty of each item determines how many items to pack into each package.</param>
		public void MultiPack(IReadOnlyList<PkgPackage> packagesToPackInto, IEnumerable<PackableItemParentWrapper> packableItemParentWrappers)
		{
			if (packagesToPackInto.Any())
			{
				var packageCount = packagesToPackInto.Count;

				foreach (var wrapper in packableItemParentWrappers.Where(w => w.ProposedPackQty > 0))
				{
					var isMorePackagesThanItems = packageCount > wrapper.ProposedPackQty;
					var qtyPerPackage = isMorePackagesThanItems ? 1 : Math.Floor(wrapper.ProposedPackQty / packageCount);

					var proposedPackQtyTruncated = Math.Floor(wrapper.ProposedPackQty);

					var noOfPackagesToEnumerate = (isMorePackagesThanItems
						? proposedPackQtyTruncated
						// we will enumerate one less than the package count so we can pack the last package with the qty for each package + any remainder in one go.
						: packageCount - 1);

					for (int i = 0; i < noOfPackagesToEnumerate; i++)
					{
						packagesToPackInto[i].Pack(wrapper.PackableItemParent, qtyPerPackage);
					}

					var remainingQty = isMorePackagesThanItems
						? wrapper.ProposedPackQty - proposedPackQtyTruncated // get the decimal part that we ignored earlier
						: wrapper.ProposedPackQty % packageCount + qtyPerPackage; // get the remainder of the division we did earlier + the amount for the last package

					if (remainingQty > 0)
					{
						packagesToPackInto[packagesToPackInto.Count - 1].Pack(wrapper.PackableItemParent, remainingQty);
					}
				}
			}
		}

		PkgPackage[] CreatePackages(ZInt packageQtyToCreate, ZString packageTypeToCreate, PkgPackage parentPackage = null)
		{
			var newPackages = new PkgPackage[packageQtyToCreate];
			var packagesToAddTo = parentPackage != null ? parentPackage.Packages : Packages;

			for (int i = 0; i < packageQtyToCreate; i++)
			{
				newPackages[i] = packagesToAddTo.AddNew(packageTypeToCreate);
			}

			return newPackages;
		}

		#endregion

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PkgPackageJobFetchStrategy(this);
		}

		#endregion

		#region Printing

		// tested in PkgPackageTest.TestPrintLabel

		public event EventHandler<AutoPrintFailedEventArgs> AutoPrintFailed
		{
			add { AutoPrinter.AutoPrintFailed += value; }
			remove { AutoPrinter.AutoPrintFailed -= value; }
		}

		public event EventHandler<AutoPrintingEventArgs> AutoPrinting
		{
			add { AutoPrinter.AutoPrinting += value; }
			remove { AutoPrinter.AutoPrinting -= value; }
		}

		public event EventHandler AutoPrinted
		{
			add { AutoPrinter.AutoPrinted += value; }
			remove { AutoPrinter.AutoPrinted -= value; }
		}

		public void PrintAllLabels()
		{
			// if no package is selected, PkgPackageJobDocumentSupport will get PackSelection.All as the labels to print.
			Selected.UpdateSelectedPackages(Enumerable.Empty<PkgPackage>());
			PrintLabel(Packages.First());
		}

		internal void PrintDocument(PkgPackage package, Guid printerPK, int numberOfLabelsToPrint, IStmMenuItem documentToPrint)
		{
			AutoPrinter.PrintDocument(package, printerPK, numberOfLabelsToPrint, documentToPrint);
		}

		internal void PrintLabel(PkgPackage package)
		{
			AutoPrinter.PrintDocument(package);
		}

		PackageLabelAutoPrinter AutoPrinter
		{
			get { return autoPrinter ?? (autoPrinter = new PackageLabelAutoPrinter(this)); }
		}

		PackageLabelAutoPrinter autoPrinter;

		#endregion

		#region Save

		#region IsSavedByFactory

		public override bool IsSavedByFactory
		{
			get { return IsInDatabase || HasChanges; }
		}

		#endregion

		#region OnSaving

		// sealed to prevent bypassing ID generation (eg. subclassing and not calling base)
		public sealed override void OnSaving()
		{
			base.OnSaving();
			if (KJ_IsFinalized && !Packages.Any())
			{
				Delete();
			}
			else
			{
				PopulateKJ_JobIDIfRequired();
			}
		}

		void PopulateKJ_JobIDIfRequired()
		{
			if (!IsInDatabase && !IsDeleted && KJ_JobID.IsEmpty)
			{
				KJ_JobID = NumberFountainForJobID.GetNextFormatted(Factory);
			}
		}

		protected virtual INumberFountainProxy NumberFountainForJobID => Env.NumberFountains.PackingID;

		#endregion

		#region OnSaved

		// sealed to prevent bypassing ID clearing (eg. subclassing and not calling base)
		public sealed override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				ValueSetStrategy.HasChangesThatAreInvalidIfFinalised = false;
			}
			else if (!IsInDatabase && !IsDeleted)
			{
				KJ_JobID = "";
			}
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			SplitAutoBreakdownPackagesAndSetForIDGeneration();
		}

		#region OnFactorySaving

		internal void MarkAsPackageIDGenerationFailed()
		{
			PackageIDGenerationFailed = true;
		}

		bool PackageIDGenerationFailed;

		protected sealed override void OnFactorySaving()
		{
			base.OnFactorySaving();
			PreventSaveIfPackageIDGenerationFailed();
			PreventSaveIfFinalised();
			RemoveDuplicatePackageIDs();
			CheckIfAnyOrphanScanMatchesPackageWithChangedPackageID();
		}

		#region SplitAutoBreakdownPackagesAndSetForIDGeneration

		void SplitAutoBreakdownPackagesAndSetForIDGeneration()
		{
			var parent = ParentWithAutoPackageBreakdown;
			if (parent != null && AreThereAnyPackagesWithChanges)
			{
				GetPackagesToGenerateIDsFor(parent).ForEach(p => ((ISupportPackageIDGeneration)p).ShouldGenerateIDOnSaving = true);
			}
		}

		IPackingParentWithAutoPackageBreakdown ParentWithAutoPackageBreakdown
		{
			get
			{
				// need to load parent this way to avoid invalid cast
				return Factory.Load(KJ_ParentTableCode, KJ_ParentID) as IPackingParentWithAutoPackageBreakdown;
			}
		}

		IEnumerable<PkgPackage> GetPackagesToGenerateIDsFor(IPackingParentWithAutoPackageBreakdown parent)
		{
			var packagesToGenerateIDFor = new List<PkgPackage>();

			using (parent.SuspendWhileAddingAutoCreatedPackages())
			{
				foreach (var package in Packages.ToArray())
				{
					if (!package.IsContainer)
					{
						if (package.KP_PackageQty > 1)
						{
							packagesToGenerateIDFor.AddRange(package.BreakDownIntoIndividualPackages());
						}
						else if (package.KP_PackageQty == 1 && (package.KP_PackageID.IsEmpty || package.Packages.Any(p => p.KP_PackageID.IsEmpty)))
						{
							packagesToGenerateIDFor.Add(package);
						}
					}
				}
			}

			parent.RunAfterAllAutoCreatedPackagesAreAdded(packagesToGenerateIDFor);

			return packagesToGenerateIDFor;
		}

		bool AreThereAnyPackagesWithChanges
		{
			get
			{
				IActiveBusinessObjectCollection iActiveCollection = Packages;
				return iActiveCollection.IsLoaded ? Packages.Any(p => p.HasChanges) : iActiveCollection.HasChanges;
			}
		}

		#endregion

		#region PreventSaveIfPackageIDGenerationFailed

		void PreventSaveIfPackageIDGenerationFailed()
		{
			if (PackageIDGenerationFailed)
			{
				throw new ZCannotSaveException(
					Res.GetString("167674a4-18e2-45fc-917a-61266f9e6fb0", "Package ID Generation failed and the Packages cannot be saved. Retry the operation again."),
					Res.GetString("08f5f78f-8f38-4c34-86ca-5483144f9619", "Package ID Generation failed."));
			}
		}

		#endregion

		#region PreventSaveIfFinalised

		void PreventSaveIfFinalised()
		{
			if (IsInDatabase && HasChangesThatAreInvalidIfFinalised) // don't use .HasChanges which is true when printing labels adds logs etc.
			{
				if (IsInDatabase && (ZBool)KJ_IsFinalizedInfo.OriginalValue)
				{
					throw new ZCannotSaveException("Packing is Finalized (perhaps by another user) and cannot be changed.", "Job is Finalized");
				}

				UpdateCriticalChangesVersion();
			}
		}

		void UpdateCriticalChangesVersion()
		{
			if (KJ_IsFinalized)
			{
				if (!KJ_CriticalChangesVersionID.IsEmpty)
				{
					KJ_CriticalChangesVersionID = ZGuid.Empty;
				}
			}
			else
			{
				KJ_CriticalChangesVersionID = ZGuid.NewZGuid();
			}
		}

		bool HasChangesThatAreInvalidIfFinalised
		{
			get { return ValueSetStrategy.HasChangesThatAreInvalidIfFinalised || this.HasChangesOnChildrenNotValidIfFinalised(); }
		}

		#endregion

		#region RemoveDuplicatePackageIDs

		void RemoveDuplicatePackageIDs()
		{
			if (HasChanges && Packages.Any(p => !p.IsInDatabase || p.PackageIDHasChanges))
			{
				var duplicatePackages = new List<PkgPackage>();

				var packageQuery = new ZQuery();
				packageQuery.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, PK);
				packageQuery.AddToFilter(PkgPackageSchema.KP_KPH_PackageHeader, SQLComparisonOperator.NotEqual, null);

				var existingPackageIds = new HashSet<ZString>();

				foreach (var package in Factory.Load<PkgPackage>(packageQuery).OrderByDescending(pkg => pkg.KP_PackageID))
				{
					var uppercasePackageID = package.KP_PackageID.ToUpperInvariant();
					if (existingPackageIds.Contains(uppercasePackageID))
					{
						duplicatePackages.Add(package);
					}
					else
					{
						existingPackageIds.Add(uppercasePackageID);
					}
				}

				if (duplicatePackages.Any())
				{
					Factory.SuspendValidation();
					try
					{
						PackageIDGenerator.GenerateNewIDsAndStoreOldIDsAsPreviousID(duplicatePackages, existingPackageIds);
					}
					finally
					{
						Factory.ResumeValidation();
					}
				}
			}
		}

		#endregion

		#region CheckIfAnyOrphanScanMatchesPackageWithChangedPackageID

		void CheckIfAnyOrphanScanMatchesPackageWithChangedPackageID()
		{
			if (PackingRegistry.Instance.AnonymousPackageCreationEnabled.Value)
			{
				var parent = ParentJob as IPackingParentOrphanScan;
				if (parent != null)
				{
					// only load packs that are already in the cache because we only care about packages whose ID has changed
					var packageQuery = new ZQuery();
					packageQuery.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, PK);
					packageQuery.AddToFilter(PkgPackageSchema.KP_KPH_PackageHeader, SQLComparisonOperator.NotEqual, null);
					packageQuery.FetchOnlyFromLocalCache = true;

					// create dictionary of relevant packages (package has an id *and* package is either new or its id changed)
					var packagesByID = Factory.Load<PkgPackage>(packageQuery)
						.Where(p => !p.IsInDatabase || p.PackageIDHasChanges)
						.ToDictionary(p => p.KP_PackageID.Trim().ToUpper());

					if (packagesByID.Any())
					{
						var orphanScanQuery = new ZQuery();
						orphanScanQuery.AddToFilter(JobOrphanScanSchema.JOS_JobType, parent.JobType.ToString());
						orphanScanQuery.AddToFilter(JobOrphanScanSchema.JOS_Barcode, packagesByID.Keys);
						orphanScanQuery.OrderBy = JobOrphanScanSchema.JOS_EventTimeUtc.Name;

						// Add logs from all orphan scans to the matching package and populate Weight and DIMs.
						// This means if two orphan scans share the same Package ID, logs from both will be added to the package.
						// In the case of Weights and DIMs, the latest event that has non-0 Weights *or* DIMS are used.
						foreach (var orphanScan in Factory.Load<JobOrphanScan>(orphanScanQuery))
						{
							var package = packagesByID[orphanScan.JOS_Barcode.ToUpper()];
							PopulateWeightsAndDimsFromOrphanScanRecord(package, orphanScan);
							AddScanLogAndDeleteOrphanScanRecord(package, orphanScan);
						}
					}
				}
			}
		}

		void PopulateWeightsAndDimsFromOrphanScanRecord(PkgPackage package, JobOrphanScan orphanScan)
		{
			if (orphanScan.JOS_Weight > 0)
			{
				package.KP_Weight = orphanScan.JOS_Weight;

				if (!orphanScan.JOS_WeightUQ.IsEmpty)
				{
					package.KP_WeightUQ = orphanScan.JOS_WeightUQ;
				}
			}

			if (orphanScan.JOS_Length > 0 || orphanScan.JOS_Width > 0 || orphanScan.JOS_Height > 0)
			{
				package.KP_Length = orphanScan.JOS_Length;
				package.KP_Width = orphanScan.JOS_Width;
				package.KP_Height = orphanScan.JOS_Height;

				if (!orphanScan.JOS_DimensionUQ.IsEmpty)
				{
					package.KP_DimensionUQ = orphanScan.JOS_DimensionUQ;
				}
			}
		}

		void AddScanLogAndDeleteOrphanScanRecord(PkgPackage package, JobOrphanScan orphanScan)
		{
			var log = package.Logs.AddNew((Event)orphanScan.Event, orphanScan.JOS_Reference, orphanScan.JOS_EventTimeUtc.ToDateTime().ToLocalTime());
			log.SL_GS_NKUser = orphanScan.JOS_GS_NKUser;
			orphanScan.Delete();
		}

		#endregion

		#endregion

		protected override void OnSavingForDelete()
		{
			base.OnSavingForDelete();
			DeletePackingFountain();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Deleting non-bizo record in DB")]
		void DeletePackingFountain()
		{
			var deleteQuery = string.Format(Culture.Invariant, @"
DELETE
	dbo.StmNums
WHERE
	SN_Owner = @Owner");

			using (var command = ((IDbConnected)Factory).Connection.Command(deleteQuery))
			{
				command.AddParameter("@Owner", SqlDbType.UniqueIdentifier, PK.ToGuid());
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new PkgPackageJobFountainUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		protected class PkgPackageJobFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public PkgPackageJobFountainUniqueIndexFailureHandler(PkgPackageJob job)
				: base(PkgPackageJobSchema.Constants.Indexes.NR_UX__KJ_JobID, job)
			{
				packageJob = job;
			}
			readonly PkgPackageJob packageJob;

			protected override INumberFountainProxy NumberFountainToFix => packageJob.NumberFountainForJobID;
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			using (new SemaphoreManager(IsDeletingPackageJobSemaphore))
			{
				Packages.DeleteAll(); // tested by SaveAndDeleteBusinessObject()
				LoosePackageIDs.DeleteAll(); // tested by SaveAndDeleteBusinessObject()
				base.Delete();
			}

			OnDeleted();
		}

		void OnDeleted()
		{
			Deleted?.Invoke(this, EventArgs.Empty);
		}

		protected override void DeleteForDataRefresh()
		{
			base.DeleteForDataRefresh();
			OnDeleted();
		}

		public event EventHandler Deleted;

		#region Package Delete

		internal void FireOnPackageDeleteCanceled(PkgPackage package, ZString reason)
		{
			PackageDeleteCanceled?.Invoke(this, new PackageCanceledActionEventArgs(package, reason));
		}

		public event EventHandler<PackageCanceledActionEventArgs> PackageDeleteCanceled;

		#endregion

		public bool IsDeletingPackageJob
		{
			get { return IsDeletingPackageJobSemaphore.IsSuspended; }
		}

		Semaphore IsDeletingPackageJobSemaphore { get { return isDeletingPackageJobSemaphore ?? (isDeletingPackageJobSemaphore = new Semaphore()); } }
		Semaphore isDeletingPackageJobSemaphore;

		#endregion

		#region DataRefresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			isFinalisedBeforeDataRefresh = KJ_IsFinalized;
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			if (KJ_IsFinalized != isFinalisedBeforeDataRefresh)
			{
				UpdateReadOnly();
				FireIsFinalisedChanged();
			}
		}

		bool isFinalisedBeforeDataRefresh;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return KJ_JobID.IsEmpty ?
					Res.GetString("af4dd6eb-d58a-4592-97d3-2f3fb1a09839", "Package Job") :
					Res.GetString("251792f7-00b5-4a64-861b-a771ceebe24b", "Package Job {0}", KJ_JobID);
			}
		}

		#endregion

		public bool IsReleased => KJ_IsReleased || KJ_ReleasedTimeUtc.IsValid;

		#region IPackageSummary

		ZString IPackageSummary.Commodity
		{
			get { return ""; }
		}

		ZString IPackageSummary.Contents
		{
			get { return Packages.ToStringSummary(); }
		}

		ZString IPackageSummary.Dimensions
		{
			get { return ""; }
		}

		public bool IsStatusSignificant
		{
			get { return Packages.Any() && Packages.All(p => p.IsReleased); }
		}

		bool IPackageSummary.IsTemperatureControlled
		{
			get { return false; }
		}

		ZString IPackageSummary.PackageID
		{
			get { return ""; }
		}

		ZString IPackageSummary.PackageSequence
		{
			get { return ""; }
		}

		ZString IPackageSummary.PackageSequenceCaption
		{
			get { return ""; }
		}

		ZString IPackageSummary.Status
		{
			get
			{
				var result = "";
				if (IsStatusSignificant)
				{
					result = PackageStatuses.Released;
				}
				else if (IsReleased)
				{
					result = PackageStatuses.ReleasedViaJob;
				}

				return result;
			}
		}

		ZString IPackageSummary.Temperature
		{
			get { return ""; }
		}

		ZString IPackageSummary.Volume
		{
			get { return ZString.Format("{0} {1}", Volume.ToStringTrimZeros("N3"), VolumeUQ); }
		}

		ZString IPackageSummary.Weight
		{
			get { return ZString.Format("{0} {1}", Weight.ToStringTrimZeros("N"), WeightUQ); }
		}

		ZString IPackageSummary.IsHeld
		{
			get { return ""; }
		}

		ZString IPackageSummary.HandlingUnit => "";

		#region Captions

		ZString IPackageSummary.CommodityCaption
		{
			get { return ""; }
		}

		ZString IPackageSummary.DimensionsCaption
		{
			get { return ""; }
		}

		ZString IPackageSummary.PackageIDCaption
		{
			get { return ""; }
		}

		ZString IPackageSummary.TemperatureCaption
		{
			get { return ""; }
		}

		ZString IPackageSummary.VolumeCaption
		{
			get { return Res.GetString("4d2c3cd6-7a96-492d-b310-cf7f1d2cf063", "Vol"); }
		}

		ZString IPackageSummary.WeightCaption
		{
			get { return Res.GetString("66878504-fe18-4720-81ae-33ef147f92b5", "Wgt"); }
		}

		ZString IPackageSummary.IsHeldCaption
		{
			get { return ""; }
		}

		ZString IPackageSummary.HandlingUnitCaption
		{
			get { return ""; }
		}

		#endregion

		#endregion

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new PkgPackageJobDocumentSupporter(this); }
		}

		/// <summary>
		/// Must be a ZString, so that Document Customisation can access the Property to filter Document Menus.
		/// </summary>
		public ZString PackageSupportedDataContexts
		{
			get
			{
				var parentDocumentSupportable = ParentJob as IDocumentSupportable;
				var documentSupporter = parentDocumentSupportable?.DocumentSupporter as IPackingParentDocumentSupporter;
				var moduleSpecificDataContexts = documentSupporter != null ? documentSupporter.GetModuleSpecificPackageSupportedDataContexts().Select(dc => dc.ToString()) : Array.Empty<string>();
				return new ZStringBuilder(moduleSpecificDataContexts).ToStringWithDelimiterBetweenAppends(",");
			}
		}

		#endregion

		#region CancelPackageCarrierLabel

		public ReturnResult CancelPackageCarrierLabel(IEnumerable<PkgPackage> packagesToCancel)
		{
			Argument.NotNull(packagesToCancel, nameof(packagesToCancel));

			var cancellingPackages = packagesToCancel.ToArray();
			if (cancellingPackages.Any(p => p == null))
			{
				throw new ArgumentException("Cannot have null Package elements in Collection.", nameof(packagesToCancel));
			}

			var error = "";
			if (!cancellingPackages.Any())
			{
				error = Res.GetString("da871657-fc84-4a41-83cd-0ce5ddc5f9c7", "No packages to cancel package label.");
			}
			else if (cancellingPackages.Any(package => package.KP_KJ_ParentPackageJob != PK))
			{
				error = Res.GetString("f7990e08-b1a2-4a1b-8933-a2b359d4ae1a", "One or more packages does not belong to the same package job.");
			}
			else if (cancellingPackages.Any(package => !package.CanCancelPackageLabel))
			{
				error = Res.GetString("50043cb5-03ec-4b0a-8b7d-0fd501c52efb", "One or more packages cannot cancel package label.");
			}
			else
			{
				foreach (var package in cancellingPackages)
				{
					package.ShouldCancelCarrierLabelOnSaving = true;
				}

				HasChanges = true;
			}

			return new ReturnResult { Success = error.IsNullOrEmpty(), Message = error };
		}

		#endregion

		#region CheckAndFixSequence

		internal ZString SequenceOverflowErrorMessage => $"Current Package Job {PK} has too many outer packages, cannot fix the sequence correctly."; // This is a Developer Error

		public void CheckAndFixSequence()
		{
			CheckAndFixSequence(ParentJob);
		}

		public static void CheckAndFixSequence(IPackingParent parent, IEnumerable<PkgPackage> outers = null)
		{
			if (parent != null)
			{
				switch (parent.PackageSequenceType)
				{
					case PackageSequenceType.OuterWithLooseID:
						{
							CheckAndFixSequenceCore(parent, outerPackagesOnly: true, includeLooseIDs: true, outers);
							break;
						}
					case PackageSequenceType.Outer:
						{
							CheckAndFixSequenceCore(parent, outerPackagesOnly: true, includeLooseIDs: false, outers);
							break;
						}
					case PackageSequenceType.Standard:
						{
							CheckAndFixSequenceCore(parent, outerPackagesOnly: false, includeLooseIDs: false, outers);
							break;
						}
				}
			}
		}

		static void CheckAndFixSequenceCore(IPackingParent parent, bool outerPackagesOnly, bool includeLooseIDs, IEnumerable<PkgPackage> outers = null)
		{
			var packageJob = LoadOrCreatePackageJob(parent);

			var packagesToRemoveSequence = (outers ?? packageJob.GetAllPackagesOnJob()).Where(p => (!p.KP_KP_ParentPackage.IsEmpty || p.KP_KPH_PackageHeader.IsEmpty) && p.KP_Sequence != 0);
			foreach (var packageToRemoveSequence in packagesToRemoveSequence)
			{
				packageToRemoveSequence.KP_Sequence = 0;
			}

			var outerPackages = outers;
			if (outerPackages == null || !outerPackages.Any())
			{
				outerPackages = packageJob.Packages;
			}
			outerPackages = outerPackages.Where(p => !p.KP_KPH_PackageHeader.IsEmpty);

			var packages = outerPackagesOnly ? outerPackages : packageJob.Packages;
			packages = packages.OrderBy(p => p.KP_PackageID);

			var loosePackages = includeLooseIDs ? packageJob.LoosePackagePivots.OrderBy(pivot => pivot.PackageHeader.KPH_PackageID) : null;
			var maxSequence = includeLooseIDs ? packages.Count() + loosePackages.Count() : packages.Count();

			if (maxSequence > short.MaxValue)
			{
				ErrorReporter.ReportOnce(packageJob.SequenceOverflowErrorMessage);
				maxSequence = short.MaxValue;
			}

			var existingSequenceSet = new HashSet<short>();
			var lostSequenceSet = new HashSet<short>(maxSequence);
			var packagesToFix = new HashSet<PkgPackage>();
			var loosePackagesToFix = new HashSet<AutoPkgPackageJobPackageHeaderPivot>();

			for (int i = 1; i <= maxSequence; i++)
			{
				lostSequenceSet.Add((short)i);
			}

			foreach (var package in packages)
			{
				if (existingSequenceSet.Contains(package.KP_Sequence) || package.KP_Sequence > maxSequence || package.KP_Sequence <= 0)
				{
					packagesToFix.Add(package);
				}
				else
				{
					existingSequenceSet.Add(package.KP_Sequence);
					lostSequenceSet.Remove(package.KP_Sequence);
				}
			}

			if (includeLooseIDs)
			{
				foreach (var loosePackage in loosePackages)
				{
					if (existingSequenceSet.Contains(loosePackage.KPJ_Sequence) || loosePackage.KPJ_Sequence > maxSequence || loosePackage.KPJ_Sequence <= 0)
					{
						loosePackagesToFix.Add(loosePackage);
					}
					else
					{
						existingSequenceSet.Add(loosePackage.KPJ_Sequence);
						lostSequenceSet.Remove(loosePackage.KPJ_Sequence);
					}
				}
			}

			if (lostSequenceSet.Count > 0)
			{
				foreach (var packageToFix in packagesToFix)
				{
					var sequenceToFix = lostSequenceSet.FirstOrDefault();
					if (sequenceToFix == default(short))
					{
						sequenceToFix = short.MaxValue;
					}
					packageToFix.KP_Sequence = sequenceToFix;
					lostSequenceSet.Remove(sequenceToFix);
				}
				if (includeLooseIDs)
				{
					foreach (var loosePackageToFix in loosePackagesToFix)
					{
						var sequenceToFix = lostSequenceSet.FirstOrDefault();
						if (sequenceToFix == default(short))
						{
							sequenceToFix = short.MaxValue;
						}
						loosePackageToFix.KPJ_Sequence = sequenceToFix;
						lostSequenceSet.Remove(sequenceToFix);
					}
				}
			}

			ZExceptionReporting.ProcessWithSaveExceptionHandling(packageJob.Factory.Save, null);
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (KJ_ParentID.IsEmpty)
			{
				KJ_ParentID = ZGuid.NewZGuid();
				KJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
	}

	#region PackingValueSetStrategy

	// tested via PackageJobTest.
	public class PackingValueSetStrategy : IValueSetStrategy
	{
		public PackingValueSetStrategy(PkgPackageJob bizO, params SchemaColumn[] columnsThatCanChangeWhenFinalised)
			: this((BusinessObject)bizO, columnsThatCanChangeWhenFinalised)
		{
			IsPackageJob = true;
		}

		public PackingValueSetStrategy(BusinessObject bizO, params SchemaColumn[] columnsThatCanChangeWhenFinalised)
		{
			BizO = bizO;
			ColumnsThatCanChangeWhenFinalised = columnsThatCanChangeWhenFinalised;
		}

		readonly bool IsPackageJob;
		readonly BusinessObject BizO;
		readonly SchemaColumn[] ColumnsThatCanChangeWhenFinalised;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			if (!ColumnsThatCanChangeWhenFinalised.Any(col => col.Name == valueThatHasChanged.Name))
			{
				HasChangesThatAreInvalidIfFinalised = true;
			}
		}

		public bool HasChangesThatAreInvalidIfFinalised
		{
			get { return hasChangesThatAreInvalidIfFinalised || !(IsPackageJob || BizO.IsInDatabase); } // !IsInDatabase for a new Packing object
			internal set { hasChangesThatAreInvalidIfFinalised = value; }
		}

		bool hasChangesThatAreInvalidIfFinalised;
	}

	#endregion

	#region PackageActionStrategy, PackageCanceledActionEventArgs, PackageAction

	public class PackageActionStrategy : PackageCanceledActionEventArgs, IPackageActionStrategy
	{
		public PackageActionStrategy(PkgPackage package, PackageAction notAllowedActions = PackageAction.None, string reasonForNotAllowingAction = "")
			: base(package, reasonForNotAllowingAction)
		{
			NotAllowedActions = notAllowedActions;
		}

		readonly PackageAction NotAllowedActions;

		public bool IsActionAllowed(PackageAction action)
		{
			return !((NotAllowedActions & action) == action);
		}
	}

	public class PackageCanceledActionEventArgs : EventArgs, IPackageCanceledActionEventArgs
	{
		public PackageCanceledActionEventArgs(PkgPackage package, string reasonForNotAllowingAction)
		{
			Package = package;
			ReasonForNotAllowingAction = reasonForNotAllowingAction;
		}

		public PkgPackage Package { get; private set; }
		public ZString ReasonForNotAllowingAction { get; }

		IPkgPackage IPackageCanceledActionEventArgs.Package => Package;
	}

	#endregion
}

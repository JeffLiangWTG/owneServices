#if DEBUG

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	#region DummyWithPacking

	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	public class DummyWithPacking : DummyBusinessObject,
		IDocumentSupportable,
		IPackingParentCustomPackTypes,
		IPackingParentDefaultPackageType,
		IPackingParentWithPackableItems,
		IPackingParentOrphanScan,
		IStmALogParent,
		IStmNoteParent,
		IWorkflowProvider
	{
		public DummyWithPacking(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			IsScanQtyAllowed = YesNoWithReasonForNo.No("Dummy says so.");
		}

		#region Lines

		public IDummyPackableItemParentCollection Lines
		{
			get { return lines ?? (lines = new DummyPackableItemParentCollection(Factory, this)); }
		}

		DummyPackableItemParentCollection lines;

		#endregion

		//

		#region IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new DummyDocumentSupporter(this); }
		}

		#region DummyDocumentSupporter

		class DummyDocumentSupporter : DocumentSupporter, IPackingParentDocumentSupporter
		{
			public DummyDocumentSupporter(DummyWithPacking dummy)
				: base(dummy)
			{
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { throw new NotImplementedException(); }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				throw new NotImplementedException();
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				throw new NotImplementedException();
			}

			public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
			{
				var parent = (DummyWithPacking)BusinessObject;
				var factory = parent.NewFactory ?? Factory;
				var orgHeader = factory.Load<OrgHeader>(parent.OrgForDocumentSupporter);
				if (orgHeader == null)
				{
					orgHeader = factory.NewWithValidTestData<OrgHeader>();
					parent.OrgForDocumentSupporter = orgHeader.PK;
					var contact = orgHeader.Contacts.AddNew();
					contact.OC_ContactName = "Fred";
					contact.OC_Mobile = "112233";
				}
				return new OrgHeaderContact(orgHeader, null);
			}

			#region IPackingParentDocumentSupporter

			public IEnumerable<Constants.DataContext> GetModuleSpecificPackageSupportedDataContexts()
			{
				return new[]
				{
					Constants.DataContext.GenericAuditVarianceLabel,
					Constants.DataContext.GenericAuditVarianceLabelAll
				};
			}

			public bool IsPrintablePackageForSpecificModuleDataContext(Constants.DataContext dataContext, PkgPackage package)
			{
				return !package.KP_PackageID.StartsWith("FilterMe");
			}

			public ZString GetModuleSpecificNotFoundMessage(Constants.DataContext dataContext)
			{
				return "Not Found!";
			}

			public bool IsAllPackLevelsEnabled(Constants.DataContext dataContext)
			{
				return dataContext == Constants.DataContext.GenericAuditVarianceLabelAll;
			}

			#endregion
		}

		public ZGuid OrgForDocumentSupporter { get; set; }
		public BusinessObjectFactory NewFactory { get; set; }

		#endregion

		#endregion

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot => false;

		#endregion

		#region IPackingParent

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			PackageActionStrategy result = new PackageActionStrategy(package);

			if (PackageActionResponses != null)
			{
				PackageActionResponses.TryGetValue(package, out result);
			}

			return result;
		}

		public void SetPackageActionResponses(Dictionary<PkgPackage, PackageActionStrategy> responses, bool clearExistingPackageActionStrategies = true)
		{
			PackageActionResponses = responses;
			if (clearExistingPackageActionStrategies)
			{
				PackageActionResponses.Keys.ForEach(p => p.ClearActionStrategyCacheIncludingChildren());
			}
		}
		Dictionary<PkgPackage, PackageActionStrategy> PackageActionResponses;

		ZString IPackingParent.JobDescription
		{
			get { return "Dummy"; }
		}

		public DocumentOptions DocumentOptions
		{
			get;
			set;
		}

		ZString IPackingParent.JobNo
		{
			get { return JobNoForPackingParent; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return ZString.Empty; }
		}

		public ZString JobNoForPackingParent;

		ControllerID IPackingParent.ControllerID
		{
			get { return DummyControllerIDs.Dummy; }
		}

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
		{
			if (context == SSCCGenerationContext.GeneratingIDsViaUser && ShouldShowMessagesInGetSSCCPrefix)
			{
				notify.AddInformation("Some message was shown!");
			}

			LastNotifications = notify;
			LastSSCCGenerationContext = context;

			return SSCCPrefix;
		}

		public INotifications LastNotifications { get; private set; }
		public SSCCGenerationContext? LastSSCCGenerationContext { get; private set; }

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
			foreach (var package in packages)
			{
				BeforeUnpackingPackagesWasCalledOnThisPackages.Add(package);
			}
		}

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => true;

		public HashSet<PkgPackage> BeforeUnpackingPackagesWasCalledOnThisPackages
		{
			get { return beforeUnpackingPackagesWasCalledOnThisPackages ?? (beforeUnpackingPackagesWasCalledOnThisPackages = new HashSet<PkgPackage>()); }
		}
		HashSet<PkgPackage> beforeUnpackingPackagesWasCalledOnThisPackages;

		public bool ShouldShowMessagesInGetSSCCPrefix { get; set; }

		public ZString SSCCPrefix
		{
			get;
			set;
		}

		public IEnumerable<IPackableItemParent> PackableItemParents
		{
			get { return Lines; }
		}

		public event EventHandler PackableItemParentsCountChanged
		{
			add { Lines.CountChanged += value; }
			remove { Lines.CountChanged -= value; }
		}

		void IPackingParentWithPackableItems.LoadAllPackableItemParentsInOneHit(PkgPackageJob packageJob)
		{
			LoadAllPackableItemsWasHit = true;
			PackageJobForLoadAllPackableItemsInOneHit = packageJob;
		}

		public bool LoadAllPackableItemsWasHit { get; private set; }
		public PkgPackageJob PackageJobForLoadAllPackableItemsInOneHit { get; private set; }

		public YesNoWithReasonForNo IsAutoPackAllowed
		{
			get;
			set;
		}

		public YesNoWithReasonForNo IsScanQtyAllowed
		{
			get;
			set;
		}

		public bool IsLoosePackageIDsSupported
		{
			get;
			set;
		}

		public bool IsAutoPrintAllowed
		{
			get { return false; }
		}

		public bool IsParentJobFinalised
		{
			get;
			set;
		}

		public void OnPackageJobReleased()
		{
			OnPackageJobReleasedFiredCount++;
		}

		public int OnPackageJobReleasedFiredCount
		{
			get;
			private set;
		}

		public void OnPackageDelete(PkgPackage package)
		{
			OnPackageDeleteFiredCount++;
		}

		public int OnPackageDeleteFiredCount
		{
			get;
			private set;
		}

		public void OnPackageJobCreatedOrLoaded(PkgPackageJob packageJob)
		{
			OnPackageJobCreatedOrLoadedCount++;
		}

		public int OnPackageJobCreatedOrLoadedCount
		{
			get;
			private set;
		}

		public void OnContainerIDChanged(PkgPackage container)
		{
			ContainersWithIDChange.Add(container);
		}

		public List<PkgPackage> ContainersWithIDChange
		{
			get { return containersWithIDChange ?? (containersWithIDChange = new List<PkgPackage>()); }
		}
		List<PkgPackage> containersWithIDChange;

		public bool IsPackingJobReadOnly
		{
			get;
			set;
		}

		public bool IsScanEventsVisible
		{
			get;
			set;
		}

		// Carrier Info

		public ZString CarrierServiceLevelCode(PkgPackage package) => carrierServiceLevelCode;

		public void SetCarrierServiceLevelCode(ZString code) => carrierServiceLevelCode = code;
		ZString carrierServiceLevelCode;

		public OrgHeader CarrierBookingAgent { get; set; }

		public OrgHeader CarrierForTest { get; set; }

		public OrgHeader GetCarrier(PkgPackage package) => this.CarrierForTest;

		public ZString TransportReference { get; set; }

		public IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetAdditionalEventContextValuesFromParent()
		{
			return GetAdditionalEventContextValuesFromParentCore();
		}

		protected virtual IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetAdditionalEventContextValuesFromParentCore() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		public ParentJobType ParentJobType { get; set; }

		public PackageSequenceType PackageSequenceType { get; set; }

		bool IPackingParent.CanReleasePackage(PkgPackage package) => CanReleasePackageForTest;

		public bool CanReleasePackageForTest { get; set; }

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => CannotReleasePackageMessageForTest;

		public ZString CannotReleasePackageMessageForTest { get; set; }

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime) => OnPackageBookedViaRTUS_LastDateTimeReceived = sentDateTime;

		public ZDateTime OnPackageBookedViaRTUS_LastDateTimeReceived;

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#endregion

		#region IPackingParentCustomPackTypes

		public IEnumerable<ZString> PackTypesToExclude
		{
			get { return packTypesToExclude ?? Enumerable.Empty<ZString>(); }
			set { packTypesToExclude = value; }
		}

		IEnumerable<ZString> packTypesToExclude;

		#endregion

		#region IPackingParentDefaultPackageType

		public ZString DefaultOuterPackType
		{
			get;
			set;
		}

		#endregion

		#region IPackingJobOrphanScanParent

		public OrphanScanJobType JobType
		{
			get { return OrphanScanJobType.DUM; }
		}

		#endregion

		#region IStmALogParent

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return Array.Empty<BusinessObject>(); }
		}

		Logs IStmALogProvider.Logs
		{
			get { return logs ?? (logs = new Logs(this)); }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return Factory; }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return TableName; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		Logs logs;

		#endregion

		#region IStmNoteParent

		public Notes Notes => notes ?? (notes = new Notes(this));

		ZGuid IStmNoteParent.NotesParentPK => PK;

		string IStmNoteParent.NotesParentTableName => TableName;

		BusinessObjectFactory IStmNoteParent.NotesFactory => Factory;

		public GetValueDelegate<NoteTypeCollection> CustomNoteTypesDelegate { get; set; }

		public bool SupportsNotes { get; set; }

		public BusinessObject[] BusinessObjectsWithRelatedNotes => Array.Empty<BusinessObject>();

		public StmNoteContexts NoteContextsForRelatedNotes => StmNoteContexts.Default;

		public NoteTypeCollection NoteTypes
		{
			get => noteTypes ?? new NoteTypeCollection();
			set => noteTypes = value;
		}

		Notes notes;
		NoteTypeCollection noteTypes;

		#endregion

		#region IWorkflowProvider

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					_ = DummyWorkflowDescriptor.Instance;

					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewWorkflowItems);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		DummyProcessTaskCollection GetNewWorkflowItems()
		{
			return new DummyProcessTaskCollection(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return "DUM"; }
		}

		DummyProcessTaskCollection workflowItems;

		#endregion
	}

	#endregion

	#region DummyWithPackingSupportConreteLoosePackage

	public class DummyWithPackingSupportConcreteLoosePackage : DummyWithPacking,
		IPackingParentSupportsImportingUnassignedPackageIdsAsPackages
	{
		public DummyWithPackingSupportConcreteLoosePackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	#endregion

	#region DummyWithPackingSupportsImportingBookedDimensions

	public class DummyWithPackingSupportsImportingBookedDimensions : DummyWithPacking,
		IPackingParentSupportsImportingBookedDimensions
	{
		public DummyWithPackingSupportsImportingBookedDimensions(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}

	#endregion

	#region DummyPackingParent

	public class DummyPackingParent : DummyBusinessObject,
		IPackingParent
	{
		public DummyPackingParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot { get; }

		#endregion

		#region IPackingParent

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package) => new PackageActionStrategy(package);

		ZString IPackingParent.JobDescription => "Dummy";

		public DocumentOptions DocumentOptions { get; set; }

		ZString IPackingParent.JobNo => ZString.Empty;

		ZString IPackingParent.ConnoteNo => ZString.Empty;

		ControllerID IPackingParent.ControllerID => DummyControllerIDs.Dummy;

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context) => throw new NotImplementedException();

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages) => throw new NotImplementedException();

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;

		public bool IsLoosePackageIDsSupported { get; set; }

		public bool IsAutoPrintAllowed { get; set; }

		public bool IsParentJobFinalised { get; set; }

		public void OnPackageJobReleased() => throw new NotImplementedException();

		public void OnPackageDelete(PkgPackage package) => throw new NotImplementedException();

		public void OnContainerIDChanged(PkgPackage container) => throw new NotImplementedException();

		public bool IsPackingJobReadOnly { get; set; }

		public bool IsScanEventsVisible { get; set; }

		public OrgHeader CarrierBookingAgent => throw new NotImplementedException();

		public OrgHeader GetCarrier(PkgPackage package) => throw new NotImplementedException();

		public ZString CarrierServiceLevelCode(PkgPackage package) => throw new NotImplementedException();

		public ZString TransportReference { get; set; }

		public IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetAdditionalEventContextValuesFromParent() => throw new NotImplementedException();

		public ParentJobType ParentJobType { get; set; }

		public PackageSequenceType PackageSequenceType { get; set; }

		public void OnPackageJobCreatedOrLoaded(PkgPackageJob packageJob)
		{
		}

		bool IPackingParent.CanReleasePackage(PkgPackage package) => true;
		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime) => OnPackageBookedViaRTUS_LastDateTimeReceived = sentDateTime;

		public ZDateTime OnPackageBookedViaRTUS_LastDateTimeReceived;

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#endregion

		#region IStmALogParent

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

		Logs IStmALogProvider.Logs => logs ?? (logs = new Logs(this));

		BusinessObjectFactory IStmALogProvider.LogsFactory => Factory;

		ZGuid IStmALogParent.LogsParentPK => PK;

		string IStmALogParent.LogsParentTableName => TableName;

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow => false;

		Logs logs;

		#endregion
	}

	#endregion

	#region DummyGroupingKey

	public class DummyGroupingKey : GroupingKey
	{
		public const string OptionalValueSeparator = "°"; // Its separator for Hash Code generation.
		public DummyGroupingKey(IDummyPackableItemParent packableItemParent)
		{
			PackableItemParentPK = ((DummyPackableItemParent)packableItemParent).PK;

			temporaryProperGroupingKeyForWhs = string.Join(OptionalValueSeparator,
				packableItemParent.Barcode,
				packableItemParent.ZD1_Code,
				packableItemParent.ZD1_Number.ToString());
		}

		readonly string temporaryProperGroupingKeyForWhs;

		public ZGuid PackableItemParentPK { get; }

		public override int GetHashCode() => PackableItemParentPK.GetHashCode();

		protected override bool Equals(GroupingKey other)
		{
			var key = other as DummyGroupingKey;
			return key != null && key.PackableItemParentPK == PackableItemParentPK;
		}

		// key to compare item in wrapper
		// Changing the attribute of parent is not change this value (Immutable object)
		protected override bool IsSimilarItem_DoNotUseCore(GroupingKey other)
		{
			var key = other as DummyGroupingKey;
			return key != null && temporaryProperGroupingKeyForWhs == key.temporaryProperGroupingKeyForWhs;
		}
	}

	#endregion

	#region DummyPackableItem

	public abstract class DummyPackableItem : DummyDependantBusinessObject, IPackableItem
	{
		protected DummyPackableItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DummyPackableItemParent PackableItemParent
		{
			get { return This.Factory.Load<DummyPackableItemParent>(ZD1_NumberUnit); }
		}

		public GroupingKey Key
		{
			get { return keyOverride ?? new DummyGroupingKey(PackableItemParent); }
			internal set { keyOverride = value; }
		}

		GroupingKey keyOverride;

		public ZDecimal Quantity
		{
			get { return ZDecimal.ParseSafe(This.ZD1_Code, 0m); }
			set { This.ZD1_Code = value.ToStringTrimZeros(); }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		IPackableItem IPackableItem.Split(ZDecimal qtyToSplit)
		{
			return SplitCore(qtyToSplit);
		}

		protected virtual IPackableItem SplitCore(ZDecimal qtyToSplit)
		{
			var remainingQty = Quantity - qtyToSplit;
			Quantity = remainingQty;

			using (SuspendSemaphoreForDefaultValues())
			{
				DummyPackableItem result = Factory.New<DummyPackableItemParent>();
				PackableItemParent.AddPackableItem(result);
				result.Quantity = qtyToSplit;

				return result;
			}
		}

		void IPackableItem.ReMerge()
		{
			ReMergeCore();
		}

		protected virtual void ReMergeCore()
		{
			var packableItemToMergeTo = (DummyPackableItem)PackableItemParent.PackableItems.FirstOrDefault(i => i != this && i.IsUnpacked(Factory));
			if (packableItemToMergeTo != null)
			{
				packableItemToMergeTo.Quantity += Quantity;
				PackableItemParent.RemoveItem(this);

				this.Delete();
			}
		}

		protected IDisposable SuspendSemaphoreForDefaultValues() => new SemaphoreManager(FactorySemaphoreForDefaultValues);
		protected bool IsSemaphoreForDefaultValuesSuspended => FactorySemaphoreForDefaultValues.IsSuspended;
		Semaphore FactorySemaphoreForDefaultValues => Factory.GetCachedValue("FactorySemaphoreForDefaultValues", () => new Semaphore());

		DummyPackableItem This
		{
			get
			{
				if (!IsPackableItem)
				{
					throw new InvalidOperationException("Do not use IPackableItem Properties/Methods when this Dummy is a PackableItemParent.");
				}

				return this;
			}
		}

		protected bool IsPackableItem => !ZD1_NumberUnit.IsEmpty;
	}

	#endregion

	#region DummyPackableItemParent

	// This interface is used to hide the IPackableItem implementations on DummyPackableItem
	public interface IDummyPackableItemParent : IPackableItemParent
	{
		DummyPackableItem AddNewPackableItem();

		void Delete();

		ZString ZD1_Code { get; set; }
		ZInt ZD1_Number { get; set; }

		new ZString Code { set; }

		new ZString Description { set; }
		new ZString DescriptionSupplement { set; }
		new ZString DescriptionSupplementSeparator { set; }

		new ZDecimal TotalQty { set; }
		new ZString TotalQtyUQ { set; }
		new Money UnitPrice { set; }

		new ZDecimal WeightPerUnit { set; }
		new ZString WeightUQ { set; }
		new ZDecimal AutoPackQtyPerPackage { set; }
		new ZString AutoPackPackageType { set; }

		new ICustomPropertyContainer AdditionalProperties { set; }

		ZString Barcode { get; set; }
		ZString BarcodeTUN { get; set; }
		ZString BarcodeTUNPackType { get; set; }
		ZDecimal BarcodeTUNPackQty { get; set; }

		IPackableItemParent Base { get; }
	}

	public class DummyPackableItem_DeleteSelfDuringSplit : DummyPackableItemParent
	{
		public DummyPackableItem_DeleteSelfDuringSplit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IPackableItem SplitCore(ZDecimal qtyToSplit)
		{
			var remainingQty = Quantity - qtyToSplit;
			Quantity = remainingQty;

			using (SuspendSemaphoreForDefaultValues())
			{
				var remainder = (DummyPackableItem_DeleteSelfDuringSplit)this.Clone();
				remainder.Quantity = remainingQty;

				var clone = (DummyPackableItem_DeleteSelfDuringSplit)this.Clone();
				clone.Quantity = qtyToSplit;

				Delete();

				return clone;
			}
		}

		protected override void ReMergeCore() { }
	}

	public class DummyPackableItemParent : DummyPackableItem, IDummyPackableItemParent
	{
		public DummyPackableItemParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (!IsSemaphoreForDefaultValuesSuspended && !IsPackableItem)
			{
				DescriptionSupplementSeparator = "-";

				TotalQty = 100m;
				TotalQtyUQ = "UNT";

				WeightPerUnit = 2m;
				WeightUQ = Constants.Weight.Kilograms;

				AutoPackQtyPerPackage = 4m;  //
				AutoPackPackageType = "BOX"; // 4 items pack into 1 box.

				// add custom properties to simulate attributes
				var additionalProperties = new CustomPropertyContainer<DummyPackableItemParent>();
				additionalProperties.AddCustomProperty("ZD1 Code", typeof(ZString), dummy => dummy.ZD1_Code);
				additionalProperties.AddCustomProperty("ZD1 Number", typeof(ZInt), dummy => dummy.ZD1_Number);
				additionalProperties.AddCustomProperty("ZD1 NumberUnitCode", typeof(ZString), dummy => dummy.ZD1_NumberUnitCode);
				AdditionalProperties = additionalProperties;
			}
		}

		IPackableItemParent IDummyPackableItemParent.Base => This;

		DummyPackableItemParent This
		{
			get
			{
				if (IsPackableItem)
				{
					throw new InvalidOperationException("Do not use IPackableItemParent Properties/Methods when this Dummy is a PackableItem.");
				}

				return this;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (!IsSemaphoreForDefaultValuesSuspended)
			{
				using (SuspendSemaphoreForDefaultValues())
				{
					AddPackableItem(Factory.New<DummyPackableItemParent>());
				}
			}
		}

		#region IPackableItemParent

		#region Code

		public ZString Code
		{
			get { return This.code; }
			set { This.code = value; }
		}

		ZString code;

		#endregion

		#region Description

		public ZString Description
		{
			get { return This.description; }
			set { This.description = value; }
		}

		ZString description;

		#endregion

		#region DescriptionSupplement

		public ZString DescriptionSupplement
		{
			get { return This.descriptionSupplement; }
			set { This.descriptionSupplement = value; }
		}

		ZString descriptionSupplement;

		#endregion

		#region DescriptionSupplementSeparator

		public ZString DescriptionSupplementSeparator
		{
			get { return This.descriptionSupplementSeparator; }
			set { This.descriptionSupplementSeparator = value; }
		}

		ZString descriptionSupplementSeparator;

		#endregion

		#region TotalQty

		public ZDecimal TotalQty
		{
			get { return This.totalQty; }
			set
			{
				This.totalQty = value;

				if (PackableItemsInternal.Count == 1 && !IsInDatabase)
				{
					PackableItemsInternal.Single().Quantity = value;
				}
			}
		}

		ZDecimal totalQty;

		#endregion

		#region TotalQtyUQ

		public ZString TotalQtyUQ
		{
			get { return This.totalQtyUQ; }
			set { This.totalQtyUQ = value; }
		}

		ZString totalQtyUQ;

		#endregion

		#region UnitPrice

		public Money UnitPrice
		{
			get { return unitPrice; }
			set { unitPrice = value; }
		}

		Money unitPrice;

		#endregion

		#region WeightPerUnit

		public ZDecimal WeightPerUnit
		{
			get { return This.weightPerUnit; }
			set { This.weightPerUnit = value; }
		}

		ZDecimal weightPerUnit;

		#endregion

		#region WeightUQ

		public ZString WeightUQ
		{
			get { return This.weightUQ; }
			set { This.weightUQ = value; }
		}

		ZString weightUQ;

		#endregion

		#region AutoPackQtyPerPackage

		public ZDecimal AutoPackQtyPerPackage
		{
			get { return This.autoPackQtyPerPackage; }
			set { This.autoPackQtyPerPackage = value; }
		}

		ZDecimal autoPackQtyPerPackage;

		#endregion

		#region AutoPackPackageType

		public ZString AutoPackPackageType
		{
			get { return This.autoPackPackageType; }
			set { This.autoPackPackageType = value; }
		}

		ZString autoPackPackageType;

		#endregion

		#region Barcodes

		public BarcodeMatch IsMatch(string barcode)
		{
			return Barcode.EqualsIgnoringCase(barcode) ? new BarcodeMatch(true)
														 : BarcodeTUN.EqualsIgnoringCase(barcode) ? new BarcodeMatch(true, BarcodeTUNPackType, BarcodeTUNPackQty)
																								: BarcodeMatch.No;
		}

		#region Barcode

		public ZString Barcode
		{
			get { return This.barcode; }
			set { This.barcode = value; }
		}

		ZString barcode;

		#endregion

		#region BarcodeTUN

		public ZString BarcodeTUN
		{
			get { return This.barcodeTUN; }
			set { This.barcodeTUN = value; }
		}

		ZString barcodeTUN;

		#endregion

		#region BarcodeTUNPackType

		public ZString BarcodeTUNPackType
		{
			get { return This.barcodeTUNPackType; }
			set { This.barcodeTUNPackType = value; }
		}

		ZString barcodeTUNPackType;

		#endregion

		#region BarcodeTUNPackQty

		public ZDecimal BarcodeTUNPackQty
		{
			get { return This.barcodeTUNPackQty; }
			set { This.barcodeTUNPackQty = value; }
		}

		ZDecimal barcodeTUNPackQty;

		#endregion

		#endregion

		#region AdditionalProperties

		public ICustomPropertyContainer AdditionalProperties
		{
			get { return This.additionalProperties; }
			set { This.additionalProperties = value; }
		}

		ICustomPropertyContainer additionalProperties;

		#endregion

		#region PackableItems

		void IPackableItemParent.RefreshPackableItems()
		{
			_ = PackableItemsInternal.Count;
		}

		DummyPackableItem IDummyPackableItemParent.AddNewPackableItem()
		{
			using (SuspendSemaphoreForDefaultValues())
			{
				var result = Factory.New<DummyPackableItemParent>();
				AddPackableItem(result);

				return result;
			}
		}

		public IEnumerable<IPackableItem> PackableItems => PackableItemsInternal;

		public void AddPackableItem(DummyPackableItem packableItem)
		{
			var itemsList = PackableItemsInternal; // build list first
			packableItem.ZD1_NumberUnit = PK;
			itemsList.Add(packableItem);
		}

		public void RemoveItem(DummyPackableItem packableItem)
		{
			var itemsList = PackableItemsInternal; // build list first
			packableItem.ZD1_NumberUnit = ZGuid.Empty;
			itemsList.Remove(packableItem);
		}

		#region PackableItemsInternal

		List<DummyPackableItem> PackableItemsInternal => This.packableItemsInternal ?? (packableItemsInternal = new List<DummyPackableItem>(GetPackableItems()));
		IEnumerable<DummyPackableItem> GetPackableItems() => Factory.Load<DummyPackableItemParent>(new ZQuery(DummyDependentBizoSchema.ZD1_NumberUnit, PK));

		List<DummyPackableItem> packableItemsInternal;

		#endregion

		#endregion

		#endregion
	}

	#endregion

	#region DummyPackableItemParentCollection

	public interface IDummyPackableItemParentCollection : IEnumerable<IDummyPackableItemParent>
	{
		IDummyPackableItemParent AddNew();
		event EventHandler CountChanged;
	}

	public class DummyPackableItemParentCollection : ActiveBusinessObjectCollection<DummyPackableItemParent>, IDummyPackableItemParentCollection
	{
		public DummyPackableItemParentCollection(BusinessObjectFactory factory, DummyWithPacking dummy)
			: base(factory, dummy, null, DummyDependentBizoSchema.ZD1_Z0)
		{
		}

		IDummyPackableItemParent IDummyPackableItemParentCollection.AddNew() => AddNew();
		IEnumerator<IDummyPackableItemParent> IEnumerable<IDummyPackableItemParent>.GetEnumerator() => GetEnumerator();
	}

	#endregion

	#region DummyWithPackingSupportsPackageExtensions

	public class DummyWithPackingSupportsPackageExtensions : DummyWithPacking,
		IPackingParentSupportsPackageExtensions
	{
		public DummyWithPackingSupportsPackageExtensions(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public PkgPackageExtension PackageExtension => Factory.LoadTop1<PkgPackageExtension>(new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, PK));
	}

	#endregion
}

#endif

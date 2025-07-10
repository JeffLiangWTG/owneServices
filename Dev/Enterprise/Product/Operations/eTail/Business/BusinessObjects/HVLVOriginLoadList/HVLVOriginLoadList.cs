using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.RefZoneHeaderLookups;

namespace Enterprise.eTail.Business
{
	[UniversalDataContext(DataContextType.HVLVOriginLoadList)]
	[CodeProperty(nameof(HVL_UniqueReference))]
	[DescriptionProperty(nameof(HVL_MasterBillNumber))]
	public class HVLVOriginLoadList : AutoHVLVOriginLoadList,
		IDocumentSupportable,
		IHVLVOriginLoadList,
		IJobNumber,
		IWorkflowProvider,
		IDtbBookingParent,
		IEDocsProvider
	{
		public HVLVOriginLoadList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (IsLodgedOrConsolidated)
			{
				SetReadOnlyIncludingChildren(true);
			}

			ResetIsNeutralMasterWhenMasterBillNumberAutoAllocationNotSupported();

			SubLoadLists = new List<SubLoadList>();
		}

		public static bool IsFunctionalTesting => HVLVDataRegistry.Instance.HVLVOriginLoadListTestingMode.Value;

		public IDisposable Split()
		{
			var groupings = HVLVOriginLoadListItemGrouping.GetGroupings(this);

			foreach (var grouping in groupings)
			{
				var subLoadList = new SubLoadList(this, grouping, grouping.Key.BillToPartyPK, grouping.Key.ServiceLevel);
				subLoadList.DestinationCountry = grouping.Key.DestinationCountry;
				subLoadList.DestinationPort = TryGetPortUNLOCOFromDestinationCountry(subLoadList.DestinationCountry);

				SubLoadLists.Add(subLoadList);
			}

			return new DisposableAction(() =>
			{
				SubLoadLists.Clear();
			});
		}

		public List<SubLoadList> SubLoadLists { get; }

		[List("Lookups.HVL_Status_List")]
		[OperationActionReadOnlyMember(nameof(HVL_Status_OperationActionReadOnly))]
		public override ZString HVL_Status
		{
			get => base.HVL_Status;
			set
			{
				var originalValue = base.HVL_Status;
				base.HVL_Status = value;
				if (value != originalValue)
				{
					var eventLogReferenceBuilder = EventLogReferenceBuilder.New()
						.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, originalValue)
						.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, value);

					if (value == HVLVOriginLoadListStatus.Codes.Failed)
					{
						eventLogReferenceBuilder.AddMandatory(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, FailureReason);
					}

					Logs.AddNew(AutoEvents.StatusUpdated, eventLogReferenceBuilder.Build());
				}
			}
		}

		public bool HVL_Status_OperationActionReadOnly => false;

		protected bool HVL_Status_ReadOnly => !IsFunctionalTesting;

		#region IsNeutralMaster

		public bool HVL_MasterBillNumber_ReadOnly => HVL_IsNeutralMaster;

		public override ZBool HVL_IsNeutralMaster
		{
			get => base.HVL_IsNeutralMaster;
			set
			{
				base.HVL_IsNeutralMaster = value;
				if (base.HVL_IsNeutralMaster && !base.HVL_MasterBillNumber.IsEmpty)
				{
					base.HVL_MasterBillNumber = string.Empty;
				}
			}
		}

		public bool HVL_IsNeutralMaster_ReadOnly => !IsMasterBillNumberAutoAllocationSupported;

		bool IsMasterBillNumberAutoAllocationSupported => Origin != null
			&& HasMAWBStockConfigured
			&& !IsLodgedOrConsolidated
			&& Origin.RL_RN_NKCountryCode == GlbBranch.CurrentBranch.HomePort.Country.Code
			&& HVL_TransportMode == TransportModes.Air;

		bool HasMAWBStockConfigured => Factory.GetCachedValue("HVLVOriginLoadList|HasMAWBStockConfigured", () =>
		{
			var query = new ZQuery(JobMawbSchema.JM_GB, GlbBranch.CurrentBranch.PK);
			query.AddToFilter(JobMawbSchema.JM_ParentTableCode, string.Empty);
			query.AddToFilter(JobMawbSchema.JM_ParentID, null);

			return Factory.Exists(typeof(JobMawb), query);
		});

		void ResetIsNeutralMasterWhenMasterBillNumberAutoAllocationNotSupported()
		{
			if (!IsLodgedOrConsolidated && !IsMasterBillNumberAutoAllocationSupported)
			{
				HVL_IsNeutralMaster = false;
			}
		}

		#endregion

		ZString FailureReason { get; set; }

		public void ReportProcessingErrorAndUpdateStatus(ZString failureReason)
		{
			FailureReason = failureReason;
			HVL_Status = HVLVOriginLoadListStatus.Codes.Failed;

			foreach (var item in Items.Cast<HVLVItem>())
			{
				item.HVI_Status = HVLVItemStatus.Codes.LoadListAllocated;
			}
		}

		public bool IsContainerInfoSpecified => !HVL_ContainerNumber.IsEmpty;

		[MaxLength(5)]
		[List("Lookups.Origins")]
		public override ZString HVL_RL_NKOrigin
		{
			get { return base.HVL_RL_NKOrigin; }
			set
			{
				if (base.HVL_RL_NKOrigin != value)
				{
					base.HVL_RL_NKOrigin = value;
					ResetIsNeutralMasterWhenMasterBillNumberAutoAllocationNotSupported();
				}
			}
		}

		public override ZGuid HVL_OA_OriginDepot
		{
			get => base.HVL_OA_OriginDepot;
			set
			{
				base.HVL_OA_OriginDepot = value;
				if (base.HVL_RL_NKOrigin.IsEmpty)
				{
					base.HVL_RL_NKOrigin = OriginDepot?.RelatedPortCodeWithFallback ?? ZString.Empty;
				}
			}
		}

		[MaxLength(5)]
		[List("Lookups.Destinations")]
		public override ZString HVL_RL_NKDestination { get => base.HVL_RL_NKDestination; set => base.HVL_RL_NKDestination = value; }

		public override ZGuid HVL_OA_DestinationDepot
		{
			get => base.HVL_OA_DestinationDepot;
			set
			{
				base.HVL_OA_DestinationDepot = value;
				if (base.HVL_RL_NKDestination.IsEmpty)
				{
					base.HVL_RL_NKDestination = DestinationDepot?.RelatedPortCodeWithFallback ?? ZString.Empty;
				}
			}
		}

		[List("Lookups.HVL_TransportMode_List")]
		public override ZString HVL_TransportMode
		{
			get => base.HVL_TransportMode;
			set
			{
				if (base.HVL_TransportMode != value)
				{
					base.HVL_TransportMode = value;
					ResetIsNeutralMasterWhenMasterBillNumberAutoAllocationNotSupported();
				}
			}
		}

		[List("Lookups.INCOTermsList")]
		public override ZString HVL_INCO
		{
			get { return base.HVL_INCO; }
			set { base.HVL_INCO = value; }
		}

		[List("Lookups.Vessels")]
		public override ZString HVL_VesselName
		{
			get { return base.HVL_VesselName; }
			set { base.HVL_VesselName = value; }
		}

		[BusinessObjectTestExclude]
		public HVLVOuterPackagesInOriginLoadListCollection OuterPackages
		{
			get
			{
				if (outerPackages == null)
				{
					outerPackages = new HVLVOuterPackagesInOriginLoadListCollection(this);
					outerPackages.Load();
				}

				return outerPackages;
			}
		}

		HVLVOuterPackagesInOriginLoadListCollection outerPackages;

		public HVLVItemInOriginLoadListCollection Items
		{
			get
			{
				if (items == null)
				{
					items = new HVLVItemInOriginLoadListCollection(this);
					items.Load();
				}

				return items;
			}
		}

		HVLVItemInOriginLoadListCollection items;

		public IEnumerable<HVLVItem> ActiveItems => Items.OfType<HVLVItem>().Where(item => item.HVI_IsActive);

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));
				return result.ToArray();
			}
		}

		public ZString TryGetPortUNLOCOFromDestinationCountry(ZString? destinationCountry)
		{
			var result = ZString.Empty;

			if (destinationCountry.HasValue)
			{
				var zoneHeaderQuery = new ZDBOnlySubQuery(typeof(RefZoneHeader), RefZoneHeaderSchema.PK);
				zoneHeaderQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, ZoneTypeCodes.HVLVGateway);
				zoneHeaderQuery.AddToFilter(RefZoneHeaderSchema.FZ_OH_RelatedParty, DestinationDepot?.Header?.PK);

				var zonePivotQuery = new ZDBOnlySubQuery(typeof(RefZonePivot), RefZonePivotSchema.F2_ParentID);
				zonePivotQuery.AddToFilter(RefZonePivotSchema.F2_ParentTableCode, RefUNLOCOSchema.Constants.Prefix);
				zonePivotQuery.AddSubQuery(RefZonePivotSchema.F2_FZ, RefZoneHeaderSchema.PK, zoneHeaderQuery, JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(RefUNLOCO));
				query.AddToFilter(RefUNLOCOSchema.RL_RN_NKCountryCode, destinationCountry);
				query.AddSubQuery(zonePivotQuery, JoinCondition.And);
				query.MaximumRows = 2;

				var unlocos = Factory.Load<RefUNLOCO>(query);

				if (unlocos.Length == 1)
				{
					result = unlocos[0].Code;
				}
			}

			return result;
		}

		public string CalculateContainerMode()
		{
			switch (HVL_TransportMode)
			{
				case TransportModes.Sea:
				case TransportModes.Rail:
					return ContainerModes.LCL;
				case TransportModes.Air:
					return HasContainerSpecified ? ContainerModes.ULD : ContainerModes.Loose;
				case TransportModes.Road:
					return ContainerModes.LTL;
			}

			return string.Empty;
		}

		bool HasContainerSpecified => !HVL_ContainerNumber.IsEmpty || !HVL_RC_ContainerType.IsEmpty;

		#region Payment Term Display

		public ZString PaymentTermDisplay
		{
			get
			{
				var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, HVL_INCO);
				switch (prepaidCollect)
				{
					case PaymentType.Prepaid:
						return HVLVConsignmentPaymentTermDisplay.FreightPrepaid;
					case PaymentType.Collect:
						return HVLVConsignmentPaymentTermDisplay.FreightCollect;
				}

				return string.Empty;
			}
		}

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => new HVLVOriginLoadListDocumentSupporter(this);

		#endregion

		#region OriginCTO

		[List("Lookups.OriginCTO_List")]
		[RelatedBusinessObject("OriginCTOAddress")]
		public ZGuid OriginCTO
		{
			get
			{
				var originCTO = ZGuid.Empty;

				if (Carrier != null)
				{
					var carrierParties = CTOFromCarrierDefaulter.GetCarrierParties(HVL_TransportMode, Carrier);
					if (carrierParties != null)
					{
						var address = carrierParties.FindAddress(HVL_RL_NKOrigin);
						originCTO = address?.PK ?? ZGuid.Empty;
					}
				}

				return originCTO;
			}
		}

		public ZPropertyInfo OriginCTOInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(OriginCTO)); }
		}

		public OrgAddress OriginCTOAddress
		{
			get { return Factory.Load<OrgAddress>(OriginCTO); }
		}

		public ZAddress OriginCTO_ZAddress
		{
			get
			{
				if (originCTO_ZAddress == null || OriginCTOChanged)
				{
					originCTO_ZAddress = new ZAddress(OriginCTOInfo);
				}

				return originCTO_ZAddress;
			}
		}

		ZAddress originCTO_ZAddress;

		bool OriginCTOChanged
		{
			get
			{
				if (HVL_TransportModeInfo.HasChanges || HVL_OH_CarrierInfo.HasChanges || HVL_OA_OriginDepotInfo.HasChanges || hadChanges)
				{
					hadChanges = true;
					return true;
				}

				return false;
			}
		}

		bool hadChanges;

		internal bool IsLodgedOrConsolidated => HVL_Status == HVLVOriginLoadListStatus.Codes.Lodged || HVL_Status == HVLVOriginLoadListStatus.Codes.Consolidated;

		#endregion

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get
			{
				string result = HumanReadableNameWithoutId;
				if (!HVL_UniqueReference.IsEmpty)
				{
					result += " " + HVL_UniqueReference;
				}

				return result;
			}
		}

		public ZString HumanReadableNameWithoutId => Res.GetString("6ab4a0af-7e3e-4755-9402-9d6b7e62680b", "HVLV Origin Load List");

		public void SetLoadedOnConsolForOuterPackages(ForwardingConsol consol)
		{
			foreach (HVLVOuterPackage outerPackage in OuterPackages)
			{
				if (outerPackage.HVO_JK_LoadedOnConsol.IsDefault)
				{
					outerPackage.HVO_JK_LoadedOnConsol = consol.PK;
					outerPackage.HVO_Status = HVLVOuterPackageStatus.Codes.Consolidated;
				}
			}
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber => HVL_UniqueReference;

		#endregion

		#region IWorkflowProvider

		public ZString WorkflowType => WorkflowDescriptors.HVLVOriginLoadListWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria() => new ColumnValueRanker();

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}

		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new HVLVOriginLoadListProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}

		HVLVOriginLoadListProcessTaskCollection workflowItems;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}

		#endregion

		#region IHVLVOriginLoadList

		IOrgHeader IHVLVOriginLoadList.Carrier => Carrier;

		IRefContainer IHVLVOriginLoadList.ContainerType => ContainerType;

		IRefUNLOCO IHVLVOriginLoadList.Destination => Destination;

		IOrgAddress IHVLVOriginLoadList.DestinationDepot => DestinationDepot;

		IRefUNLOCO IHVLVOriginLoadList.Origin => Origin;

		IOrgAddress IHVLVOriginLoadList.OriginCTOAddress => OriginCTOAddress;

		IOrgAddress IHVLVOriginLoadList.OriginDepot => OriginDepot;

		IOrgHeader IHVLVOriginLoadList.Owner => Owner;

		IRefServiceLevel IHVLVOriginLoadList.ServiceLevel => ServiceLevel;

		IHVLVOuterPackageCollection IHVLVOriginLoadList.OuterPackages => OuterPackages;

		IProcessTaskCollection IHVLVOriginLoadList.WorkflowItems => WorkflowItems;

		#endregion

		#region Load / Save / Delete

		public override void OnSaving()
		{
			PopulateHVL_UniqueReferenceIfNeeded();

			base.OnSaving();
		}

		public void PopulateHVL_UniqueReferenceIfNeeded()
		{
			if (!IsDeleted && HVL_UniqueReference.IsEmpty)
			{
				PopulateFormattedNumberPropertyIfRequired(HVL_UniqueReferenceInfo, Env.NumberFountains.HVLVOriginLoadListReference);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded && !IsInDatabase)
			{
				HVL_UniqueReference = ZString.Empty;
			}
		}

		public override void Delete()
		{
			WorkflowItems.DeleteAll();

			foreach (HVLVItem item in Items)
			{
				item.HVI_HVL_LoadList = ZGuid.Empty;
			}

			foreach (HVLVOuterPackage package in OuterPackages)
			{
				package.HVO_HVL_LoadList = ZGuid.Empty;
			}

			base.Delete();
		}

		#endregion

		#region IDtbParentBooking

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule => false;

		ZString IDtbBookingParent.JobType => WorkflowType;

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections() => new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob => null;

		ZQuery IDtbBookingParent.TransportBookingTemplateFilters => new ZQuery();

		ZString IDtbBookingParent.JobTypeDescription => HumanReadableNameWithoutId;

		bool IDtbBookingParent.RequiresMultiContainerBooking => false;

		bool IDtbBookingParent.CanCreateTransportBooking => true;

		ZGuid IDtbBookingParent.BookingParentPK => PK;

		string IDtbBookingParent.BookingParentTablePrefix => TablePrefix;

		(bool isShouldShow, string caption, string message, string confirmation) IDtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.HVLVOriginLoadList;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => HVL_UniqueReference;

		ZString IRelatedJob.JobDescription => HumanReadableNameWithoutId;

		ZString IRelatedJob.JobStatus => HVL_Status;

		#endregion

		#region IEDocsProvider

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new HVLVOriginLoadListDocManagerInfo(this));
		DocManagerInfo docManagerInfo;

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new EDocsProviderSupporter(this);
		}

		#endregion
	}
}

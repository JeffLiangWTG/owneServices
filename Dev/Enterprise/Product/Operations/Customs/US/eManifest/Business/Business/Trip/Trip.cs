using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using USBusiness = Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.USeManifestTrip)]
	[CodeProperty(Schema.BH_JobReference)]
	[DescriptionProperty(Schema.BH_JobReference)]
	public class Trip : BaseCusInBondHeader,
						IEDIMessageCollectionProvider,
						IShipmentActionsProvider,
						ITemplateCopyable,
						INoteSource,
						IEDocsProvider,
						IHaveRequiredDocuments,
						IJobInvoicingPlugIn,
						IWorkflowProvider,
						IBillTypeProvider,
						Integration.Customs.US.eManifest.ICusInBondHeader,
						IMultiSelectHandler,
						IRelatedJob
	{
		public Trip(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusInBondHeader.Schema
		{
			public const string BH_MessageStatusCodeDescription = "BH_MessageStatusCodeDescription";
			public const string BH_ReleaseStatusCodeDescription = "BH_ReleaseStatusCodeDescription";
			public const string ImporterName = "ImporterName";
			public const string ImporterCode = "ImporterCode";
		}

		#endregion

		#region Properties

		public bool IsFromHVLV
		{
			get
			{
				return Logs.HasLogWith(x =>
					x.SL_SE_NKEvent == AutoEvents.TransferredCode
					&& x.Parameters.Count == 2
					&& x.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var eventType)
					&& eventType == "HVL");
			}
		}

		public ZString DepartmentOfTransportationNumber
		{
			get { return Carrier?.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.DOTDepartmentOfTransportation, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty; }
		}

		public override ZDateTime BH_ETA
		{
			get => base.BH_ETA;
			set
			{
				var oldValue = base.BH_ETA;
				base.BH_ETA = value;
				if (!IsCopying && oldValue != BH_ETA && !IsValidationSuspended)
				{
					Shipments.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid BH_OH_Carrier
		{
			get { return base.BH_OH_Carrier; }
			set
			{
				base.BH_OH_Carrier = value;
				if (BH_CarrierSCAC.IsEmpty)
				{
					BH_CarrierSCAC = Carrier.USLocalCustomsCarrierCode(true);
				}
			}
		}

		#region BH_CarrierSCAC

		[List(nameof(Lookups) + "." + nameof(TripLookups.SCACCarrierCodes))]
		public override ZString BH_CarrierSCAC
		{
			get { return base.BH_CarrierSCAC; }
			set { base.BH_CarrierSCAC = value; }
		}

		public OrgHeader CarrierSCAC
		{
			get
			{
				if (carrierSCACCached == null)
				{
					carrierSCACCached = new CachedProperty<OrgHeader>(Factory, () =>
					{
						var orgCusCode = GetOrgCusCode(OrgCusCode.CodeTypes.TruckCarrierCode) ?? GetOrgCusCode(OrgCusCode.CodeTypes.CarrierCode);
						return orgCusCode?.Header;
					});
				}
				return carrierSCACCached.Value;
			}
		}
		CachedProperty<OrgHeader> carrierSCACCached;

		OrgCusCode GetOrgCusCode(ZString codeType)
		{
			var query = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, BH_CarrierSCAC);
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
			query.OrderBy = OrgCusCodeSchema.Constants.OK_OH;
			var orgCusCode = Factory.LoadTop1<OrgCusCode>(query);
			return orgCusCode;
		}

		#endregion

		#region BH_GB

		[LightValidationTestExempt]
		public override ZGuid BH_GB
		{
			get { return base.BH_GB; }
			set { base.BH_GB = value; }
		}

		#endregion

		#region BH_ImportTransportMode

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(TripLookups.TransportModes))]
		public override ZString BH_ImportTransportMode
		{
			get { return base.BH_ImportTransportMode; }
			set { base.BH_ImportTransportMode = value; }
		}

		#endregion

		#region BH_JobReference

		[ReadOnly(true)]
		public override ZString BH_JobReference
		{
			get { return base.BH_JobReference; }
			set
			{
				base.BH_JobReference = value;
				if (BH_VoyageNumber.IsEmpty)
				{
					BH_VoyageNumber = value.Left(Schema.BH_VoyageNumberMaxLength);
				}
			}
		}

		#endregion

		#region BH_MessageStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(TripLookups.MessageStatusList))]
		public override ZString BH_MessageStatus
		{
			get { return base.BH_MessageStatus; }
			set { base.BH_MessageStatus = value; }
		}

		public ZString BH_MessageStatusCodeDescription
		{
			get { return Lookups.MessageStatusList.GetCodeDescription(BH_MessageStatus); }
		}

		#endregion

		#region BH_PortUnladingDCode

		[List(nameof(Lookups) + "." + nameof(TripLookups.ScheduleDPortCodes))]
		public override ZString BH_PortUnladingDCode
		{
			get { return base.BH_PortUnladingDCode; }
			set
			{
				var hasChanged = BH_PortUnladingDCode != value;
				base.BH_PortUnladingDCode = value;
				if (!IsCopying && hasChanged && !value.IsEmpty && BH_RL_NKPortUnlading.IsEmpty)
				{
					PortUnladingDDefaulter.DefaultUNLOCO();
				}
			}
		}

		public ZBool PortUnladingDCodeIsDropEdit => PortUnladingDDefaulter.HasMultipleMappingPorts;

		public BusinessObjectCollection PortUnladingDRefLocoMappings => PortUnladingDDefaulter.MappingPorts;

		USBusiness.UNLOCO_USPortsDefaulter PortUnladingDDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.D, BH_RL_NKPortUnlading, USLocoMapSystemUsageList.Codes.All, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USBusiness.USPortLookupsHelper.GetRegionDistrictPorts(Factory, refLocoMapCodes);
				}

				return fPortUnladingDDefaulter ?? (fPortUnladingDDefaulter =
					new USBusiness.UNLOCO_USPortsDefaulter(Factory, BH_PortUnladingDCodeInfo, BH_RL_NKPortUnladingInfo, GetRefLocoMapping, GetEffectiveMappingPorts, true));
			}
		}
		USBusiness.UNLOCO_USPortsDefaulter fPortUnladingDDefaulter;

		#endregion

		#region BH_ReleaseStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(TripLookups.ReleaseStatusList))]
		public override ZString BH_ReleaseStatus
		{
			get { return base.BH_ReleaseStatus; }
			set
			{
				var hasChanges = base.BH_ReleaseStatus != value;
				base.BH_ReleaseStatus = value;
				if (hasChanges && !value.IsEmpty)
				{
					LogManager.AddStatusChangedLog(BH_ReleaseStatusCodeDescription);
				}
			}
		}

		public ZString BH_ReleaseStatusCodeDescription
		{
			get { return Lookups.ReleaseStatusList.GetCodeDescription(BH_ReleaseStatus); }
		}

		public bool IsFinalized
		{
			get { return IsLodged && BH_ReleaseStatus != TripEntryStatusList.Codes.AcceptedPreliminary; }
		}

		public bool IsLodged
		{
			get { return IsClearStatus(BH_ReleaseStatus) && BH_ReleaseStatus != TripEntryStatusList.Codes.Cancelled; }
		}

		TripLogManager LogManager
		{
			get { return logManager ?? (logManager = new TripLogManager(Logs)); }
		}

		TripLogManager logManager;

		#endregion

		#region BH_RL_NKPortUnlading

		public override ZString BH_RL_NKPortUnlading
		{
			get { return base.BH_RL_NKPortUnlading; }
			set
			{
				var oldValue = BH_RL_NKPortUnlading;
				base.BH_RL_NKPortUnlading = value;
				if (!IsCopying && oldValue != BH_RL_NKPortUnlading && BH_PortUnladingDCode.IsEmpty)
				{
					PortUnladingDDefaulter.DefaultPort();
				}
				foreach (var shipment in Shipments)
				{
					shipment.InBond.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region BH_TransitDirection

		[List(nameof(Lookups) + "." + nameof(TripLookups.TransitDirectionCodes))]
		public override ZString BH_TransitDirection
		{
			get { return base.BH_TransitDirection; }
			set { base.BH_TransitDirection = value; }
		}

		#endregion

		#region HasHazmatShipments

		internal bool HasHazmatShipments
		{
			get
			{
				if (hasHazmatShipmentsCached == null)
				{
					hasHazmatShipmentsCached = new CachedProperty<bool>(
						Factory,
						() => (from shipment in Shipments
							   from commodity in shipment.Commodities
							   where commodity.UNDGs.Count > 0
							   select commodity).Any());
				}
				return hasHazmatShipmentsCached.Value;
			}
		}

		CachedProperty<bool> hasHazmatShipmentsCached;

		#endregion

		public ZDateTime LatestSentMessageDateTime
		{
			get { return this.Messages.Cast<EDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && x.EM_Status == EDIMessage.Status.Sent).Select(x => x.EM_SystemCreateTimeUtc).OrderByDescending(y => y).FirstOrDefault(); }
		}

		#endregion

		#region Related Objects

		#region Conveyance

		public Equipment Conveyance
		{
			get
			{
				var conveyance = AllEquipmentIncludingMainConveyance.FirstOrDefault(e => e.BJ_IsConveyance && !e.IsDeleted);
				if (conveyance == null)
				{
					conveyance = Business.Equipment.LoadOrCreateConveyance(this);
					conveyance.BJ_IsConveyance = true;
					RegisterEditableChildObject(conveyance);
				}
				return conveyance;
			}
		}

		#endregion

		#endregion

		#region Collections

		#region Crew Members

		[ChildEditable]
		public CrewMemberCollection CrewMembers
		{
			get
			{
				if (crewMembers == null)
				{
					crewMembers = new CrewMemberCollection(this);
					RegisterEditableChildObject(crewMembers);
				}
				return crewMembers;
			}
		}

		CrewMemberCollection crewMembers;

		#endregion

		#region Equipment

		[ChildEditable]
		public EquipmentCollection AllEquipmentIncludingMainConveyance
		{
			get
			{
				if (allEquipment == null)
				{
					allEquipment = new EquipmentCollection(this, true);
					RegisterEditableChildObject(allEquipment);
				}
				return allEquipment;
			}
		}

		EquipmentCollection allEquipment;

		[ChildEditable]
		public EquipmentCollection Equipment
		{
			get
			{
				if (equipment == null)
				{
					equipment = new EquipmentCollection(this, false);
					RegisterEditableChildObject(equipment);
				}
				return equipment;
			}
		}

		EquipmentCollection equipment;

		#endregion

		#region Messages

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					messages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(messages);
				}
				return messages;
			}
		}

		EDIMessageCollection messages;

		#endregion

		#region Shipments

		[ChildEditable]
		public ShipmentCollection Shipments
		{
			get
			{
				if (shipments == null)
				{
					shipments = new ShipmentCollection(this);
					RegisterEditableChildObject(shipments);
				}
				return shipments;
			}
		}

		ShipmentCollection shipments;

		#endregion

		#region ShipmentsActions

		public void ResetShipmentsActionsIfMessageTypeChanged(string messageTypeToBeSubmitted)
		{
			if (messageTypeToBeSubmittedCached != messageTypeToBeSubmitted)
			{
				messageTypeToBeSubmittedCached = messageTypeToBeSubmitted;
				shipmentsActions = null;
			}
		}

		public ShipmentActionCollection ShipmentsActions
		{
			get
			{
				if (shipmentsActions == null)
				{
					shipmentsActions = new ShipmentActionCollection(this, messageTypeToBeSubmittedCached);
					Shipments.CountChanged += (s, e) => shipmentsActions = null;
				}
				return shipmentsActions;
			}
		}

		ShipmentActionCollection shipmentsActions;
		string messageTypeToBeSubmittedCached;

		#endregion

		#region Shipments Release Statuses

		public ShipmentCollection ShipmentsReleaseStatuses
		{
			get
			{
				if (shipmentsReleaseStatuses == null)
				{
					shipmentsReleaseStatuses = new ShipmentCollection(this);
					shipmentsReleaseStatuses.ApplySort(Shipment.Schema.B0_ReleaseStatusDate, ListSortDirection.Descending);
				}
				return shipmentsReleaseStatuses;
			}
		}

		ShipmentCollection shipmentsReleaseStatuses;

		#endregion

		#endregion

		public IEnumerable<Commodity> AllCommodities
		{
			get
			{
				var shipmentPKs = Shipments.Select(s => s.PK);
				var commodities = new List<Commodity>(Shipments.Count);
				shipmentPKs.Batch(1000).ForEach(shipmentPK => commodities.AddRange(Factory.Load<Commodity>(new ZQuery(CusInBondCargoDescSchema.BY_ParentID, shipmentPK))));

				return commodities;
			}
		}

		#region Overrides

		protected override bool CreateAutoLogIfOnlyChildrenHaveChanges => true;
		protected override bool UpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			BH_ImportTransportMode = TransportModes.Codes.Road;
			BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			BH_GB = GlbBranch.CurrentBranch.PK;
			if (BH_ETA.IsEmpty)
			{
				BH_ETA = ZDateTime.Now.AddHours(2);
			}
		}

		public override void Delete()
		{
			Conveyance.Delete();
			CrewMembers.DeleteAll();
			Equipment.DeleteAll();
			Shipments.DeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			RequiredDocuments.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore
		{
			get { return "Trip: " + BH_JobReference; }
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get { return Shipments.Cast<BusinessObject>().ToArray(); }
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var importerName = string.Empty;
				var tripReference = string.Empty;
				var separator = string.Empty;
				if (BH_OA_Importer_ZAddress != null && BH_OA_Importer_ZAddress.OrgHeader != null)
				{
					importerName = ((OrgHeader)BH_OA_Importer_ZAddress.OrgHeader).OH_FullName;
				}
				if (!BH_VoyageNumber.IsEmpty)
				{
					separator = string.IsNullOrEmpty(importerName) ? string.Empty : ", ";
					tripReference = string.Format(CultureInfo.CurrentCulture, "TRP: {0}", BH_VoyageNumber);
				}
				var optionalSuffix = string.Empty;
				if (!string.IsNullOrEmpty(tripReference) || !string.IsNullOrEmpty(importerName))
				{
					optionalSuffix = string.Format(CultureInfo.CurrentCulture, " - {0}{1}{2}", tripReference, separator, importerName);
				}
				return Res.GetString("B12C5D00-58D6-4C14-B2BC-2EFB8BA6CC22", "{0}{1}", BH_JobReference, optionalSuffix);
			}
		}

		protected override Type MovementHeaderTypeCore => null;

		#region OnSaving

		public override void OnSaving()
		{
			DeactivateJobHeaderWhenIsCancelled();
			base.OnSaving();
			PopulateJobReferenceIfNeeded();
			PopulateEquipmentAndCrewMembersForSiblingsIfNeeded();
		}

		void PopulateEquipmentAndCrewMembersForSiblingsIfNeeded()
		{
			if (!BH_ParentID.IsEmpty)
			{
				var query = new ZQuery(CusInBondHeaderSchema.BH_ParentID, BH_ParentID);
				query.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, BH_ParentTableCode);
				query.AddToFilter(CusInBondHeaderSchema.PK, SQLComparisonOperator.NotEqual, PK);
				query.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.eManifest);
				var siblings = Factory.Load<Trip>(query);
				if (siblings != null && siblings.Length > 0)
				{
					if (AllEquipmentIncludingMainConveyance.Count > 0)
					{
						var siblingsWithoutEquipment = siblings.Where(s => !(s.AllEquipmentIncludingMainConveyance.Count > 0)).ToArray();
						foreach (var sibling in siblingsWithoutEquipment)
						{
							foreach (var eqm in AllEquipmentIncludingMainConveyance)
							{
								var newEquipment = eqm.Clone() as Equipment;
								sibling.Equipment.Add(newEquipment);
							}

							sibling.Equipment.RefreshFromDb();
						}
					}

					if (CrewMembers.Count > 0)
					{
						var siblingsWithoutCrewMembers = siblings.Where(s => !(s.CrewMembers.Count > 0)).ToArray();
						foreach (var sibling in siblingsWithoutCrewMembers)
						{
							foreach (var cm in CrewMembers)
							{
								var newCrewMember = cm.Clone() as CrewMember;
								sibling.CrewMembers.Add(newCrewMember);
							}

							sibling.CrewMembers.RefreshFromDb();
						}
					}
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (IsInDatabase)
				{
					BH_MessageStatus = (ZString)BH_MessageStatusInfo.OriginalValue;
				}
				else
				{
					BH_MessageStatusInfo.ClearValue();
					BH_JobReferenceInfo.ClearValue();
				}

				if (messages != null)
				{
					foreach (Enterprise.Messaging.Business.EDIMessage message in Messages.ToArray())
					{
						if (message.IsTransmitMessage && !message.IsInDatabase && !message.IsDeleted)
						{
							message.Delete();
						}
					}
				}
			}
		}

		void DeactivateJobHeaderWhenIsCancelled()
		{
			if (!this.HasContext(BusinessContext.InvoicingPlugInGUI) && IsCancelled && IsCancelledHasChanged)
			{
				JobHeader.DeactivateAllJobs(this, true);
			}
		}

		internal void PopulateJobReferenceIfNeeded()
		{
			PopulateNumberPropertyIfRequired(BH_JobReferenceInfo, x => GetNewJobReference(x));
		}

		ZString GetNewJobReference(BusinessObjectFactory factory)
		{
			var target = new TripNumberGeneratorTarget();
			var generator = new NumberGenerator();
			generator.Factory = factory;
			generator.Context = new NumberGeneratorContext();
			generator.BaseFountain = Env.NumberFountains.USeManifestTripReference;
			generator.FountainGetter = Env.NumberFountains.GetUSeManifestTripReferenceGeneratorFountain;
			generator.PrimaryTarget = target;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();
			return target.Value.ToUpper();
		}

		public void PopulateCommodityWithEquipment(ZGuid equipmentPK, Predicate<Commodity> predicate = null)
		{
			AllCommodities.Where(commodity => predicate?.Invoke(commodity) ?? true)
						.ForEach(commodity => commodity.BY_BJ_Equipment = equipmentPK);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		static bool IsClearStatus(ZString status)
		{
			return !status.IsEmpty && status != MessageTypes.Codes.SyntaxError && status != EntryStatusList.Codes.Error;
		}

		#endregion

		#region Validation

		public new TripValidation Validation
		{
			get { return (TripValidation)base.Validation; }
		}

		protected override CusInBondHeaderValidation GetNewValidation()
		{
			return new TripValidation(this);
		}

		internal bool ShouldValidateChildren => shouldValidateChildren;
		bool shouldValidateChildren = true;

		public IDisposable SuspendValidationOnChildren()
		{
			shouldValidateChildren = false;
			return new DisposableAction(() => shouldValidateChildren = true);
		}

		internal bool ValidateAllHasBeenRun { get; set; }

		#endregion

		#region Lookups

		public new TripLookups Lookups
		{
			get { return (TripLookups)base.Lookups; }
		}

		protected override CusInBondHeaderLookups GetNewLookups()
		{
			return new TripLookups(this);
		}

		#endregion

		#endregion

		#region Interface Implementation

		#region Implementation of ITemplateCopyable

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			return new TripCloneStrategy(this).Clone();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Implementation of INoteSource

		ZString INoteSource.NoteSourceName
		{
			get { return HumanReadableName; }
		}

		#endregion

		#region Implementation of IDocumentSupportable

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new TripDocumentSupporter(this)); }
		}

		DocumentSupporter documentSupporter;

		#endregion

		#region Implementation of IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.USeManifest)); }
		}

		DocManagerInfo docManagerInfo;

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region Implementation of IHaveRequiredDocuments

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return null; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return ZString.Empty; }
		}

		Logs IHaveRequiredDocuments.Logs
		{
			get { return Logs; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return ZString.Empty; }
		}

		[ChildEditable]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}
				return requiredDocuments;
			}
		}

		JobRequiredDocumentDependentCollection requiredDocuments;

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return TablePrefix; }
		}

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return BH_JobReference; }
		}

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents()
		{
		}

		#endregion

		#region Implementation of IJobNumber

		string IJobNumber.JobNumber
		{
			get { return BH_JobReference; }
		}

		#endregion

		#region Implementation of IJobHeaderParent

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateJobReferenceIfNeeded();
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job) { }
		void IJobHeaderParent.OnJobCreated(JobHeader job) { }
		void IJobHeaderParent.OnJobDeleting(JobHeader job) { }
		void IJobHeaderParent.OnJobDeleted(JobHeader job) { }

		#endregion

		#region Implementation of IJobInvoicingPlugIn

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new eManifestJobInvoicingSupporter(this)); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region Implementation of IWorkflowProvider

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			var clientIds = new List<object> { ZGuid.Empty };
			if (Importer != null)
			{
				clientIds.Insert(0, Importer.OA_OH);
			}

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, clientIds.ToArray());
			result.Add(ProcessTaskTemplateSchema.P0_GB, BH_GB, ZGuid.Empty);
			return result;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.eManifest.Code; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<eManifestProcessTask, Trip>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		#endregion

		#region Implementation of IBillTypeProvider

		Type IBillTypeProvider.BillType
		{
			get { return typeof(Shipment); }
		}

		#endregion

		#region Implementation of IMultiSelectHandler

		ModuleIdentifier IMultiSelectHandler.FilterModuleId
		{
			get { return ModuleIDs.Customs.US.eManifestShipment; }
		}

		ZQuery IMultiSelectHandler.AdditionalFilter
		{
			get { return new ZQuery(CusInBondBillSchema.B0_BH, SQLComparisonOperator.NotEqual, PK); }
		}

		void IMultiSelectHandler.HandleSelectedObjects(BusinessObject[] selectedObjects)
		{
			foreach (var shipment in selectedObjects)
			{
				Shipments.Add(Factory.Load<Shipment>(shipment.PK));
			}
		}

		#endregion

		#endregion

		#region ICusInBondHeader Members

		Type Integration.Customs.ICusInBondHeader.MovementHeaderType
		{
			get { return typeof(InBond); }
		}

		#endregion

		#region IRelatedJob

		ZString IRelatedJob.JobNumber => BH_JobReference.IsEmpty ? new ZString("New eManifest") : BH_JobReference;
		ZString IRelatedJob.JobDescription => HumanReadableName;
		ZString IRelatedJob.JobStatus => BH_MessageStatus;

		#endregion

		#region IControllerIDProvider Members

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.US.eManifest; }
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		#endregion

		#region Duplicate Shipment Master Bill Number Cache

		public IDisposable CacheDuplicateMasterBillNumbers()
		{
			return new DisposableAction(
			  () => InitialiseDuplicateMasterBillNumberCache(),
			  () => DisposeDuplicateMasterBillNumberCache()
			);
		}

		void InitialiseDuplicateMasterBillNumberCache()
		{
			duplicateMasterBills = Shipments
						.GroupBy(b => b.B0_MasterBillNumber)
						.Where(p => p.Count() > 1)
						.Select(p => p.Key)
						.ToHashSet();
		}

		void DisposeDuplicateMasterBillNumberCache() => duplicateMasterBills = null;

#if DEBUG
		public bool duplicateMasterBillsCacheHit;
#endif

		HashSet<ZString> duplicateMasterBills;

		public bool ShipmentMasterBillExists(Shipment shipment, string shipmentMasterBill)
		{
			var result = false;
			if (duplicateMasterBills == null)
			{
				result = Shipments.Any(x => x.B0_MasterBillNumber == shipmentMasterBill && x.PK != shipment.PK);
			}
			else
			{
#if DEBUG
				duplicateMasterBillsCacheHit = true;
#endif
				result = duplicateMasterBills.Contains(shipmentMasterBill);
			}

			return result;
		}

		#endregion

		#region Master Bill Number To Shipment Lookup

		public IDisposable CreateTemporaryMasterBillToShipmentLookup()
		{
			return new DisposableAction(
			  () => BuildTemporaryMasterBillToShipmentLookup(),
			  () => DisposeTemporaryMasterBillToShipmentLookup()
			);
		}

		void BuildTemporaryMasterBillToShipmentLookup()
		{
			var shipmentQuery = new ZQuery(CusInBondBillSchema.B0_BH, PK);
			var shipments = Factory.Load<Shipment>(shipmentQuery);
			masterBillToShipmentLookup = shipments.GroupBy(s => s.B0_MasterBillNumber).ToDictionary(g => g.Key, g => g.First(), new CaseInsensitiveZStringEqualityComparer());
		}

		void DisposeTemporaryMasterBillToShipmentLookup() => masterBillToShipmentLookup = null;

		public Shipment GetShipmentFromLookup(ZString masterBillNumber)
		{
			if (masterBillToShipmentLookup.TryGetValue(masterBillNumber, out var shipment))
			{
				return shipment;
			}

			return null;
		}

		Dictionary<ZString, Shipment> masterBillToShipmentLookup;

		class CaseInsensitiveZStringEqualityComparer : IEqualityComparer<ZString>
		{
			bool IEqualityComparer<ZString>.Equals(ZString x, ZString y)
			{
				return string.Equals((string)x, (string)y, StringComparison.InvariantCultureIgnoreCase);
			}

			int IEqualityComparer<ZString>.GetHashCode(ZString obj)
			{
				return ((string)obj).ToLowerInvariant().GetHashCode();
			}
		}

		#endregion

		public ZString ImporterName
		{
			get { return Importer?.EffectiveCompanyName ?? ZString.Empty; }
		}

		public ZString ImporterCode
		{
			get { return Importer?.Header.OH_Code ?? ZString.Empty; }
		}

		#region Message Status Fields
		EDIMessage GetLastAcceptedMessage(ZString messageType, ZString[] messageSubTypes)
		{
			return Messages.OfType<EDIMessage>().Where(message =>
			{
				return message.EM_ApplicationCode.EqualsIgnoringCase(EDIMessage.ApplicationCodes.USeManifest)
					&& message.EM_ReceiveTransmit.EqualsIgnoringCase(EDIMessage.Direction.Receive)
					&& message.EM_MessageType.EqualsIgnoringCase(messageType)
					&& message.EM_Status.EqualsIgnoringCase(EDIMessageStatusList.Codes.Received)
					&& message.OriginalMessage is EDIMessage originalMessage
					&& originalMessage.EM_MessageType.EqualsIgnoringCase(messageType)
					&& originalMessage.EM_Status.EqualsIgnoringCase(EDIMessageStatusList.Codes.Sent)
					&& messageSubTypes.Any(subType => subType.EqualsIgnoringCase(originalMessage.EM_MessageSubType));
			}).OrderByDescending(y => y.EM_SystemCreateTimeUtc).FirstOrDefault();
		}

		ZDateTime LastAcceptedDate(EDIMessage message) => message?.EM_MessageDateTime ?? ZDateTime.Empty;
		ZString LastAcceptedStatus(EDIMessage message) => message?.EM_MessageSubTypeDescription ?? ZString.Empty;

		#region Complete eManifest

		public ZDateTime CompleteEManifestLastAcceptedDate => LastAcceptedDate(CompleteEManifestLastAcceptedMessage);
		public ZString CompleteEManifestLastAcceptedStatus => LastAcceptedStatus(CompleteEManifestLastAcceptedMessage);

		EDIMessage CompleteEManifestLastAcceptedMessage => Factory.GetValue(ref completeEManifestLastAcceptedMessageCached, () => GetLastAcceptedMessage(MessageTypes.Codes.eManifest, new ZString[] { MessageActionCodes.Codes.Original, MessageActionCodes.Codes.Change }));
		CachedProperty<EDIMessage> completeEManifestLastAcceptedMessageCached;
		#endregion

		public ZDateTime UnassociatedShipmentsLastAcceptedDate => LastAcceptedDate(UnassociatedShipmentsLastAcceptedMessage);
		public ZString UnassociatedShipmentsLastAcceptedStatus => LastAcceptedStatus(UnassociatedShipmentsLastAcceptedMessage);

		EDIMessage UnassociatedShipmentsLastAcceptedMessage => Factory.GetValue(ref unassociatedShipmentsLastAcceptedMessageCached, () => GetLastAcceptedMessage(MessageTypes.Codes.UnassociatedShipments, new ZString[] { MessageActionCodes.Codes.Original, MessageActionCodes.Codes.Change }));
		CachedProperty<EDIMessage> unassociatedShipmentsLastAcceptedMessageCached;

		public ZDateTime PreliminaryTripDetailsLastAcceptedDate => LastAcceptedDate(PreliminaryTripDetailsLastAcceptedMessage);
		public ZString PreliminaryTripDetailsLastAcceptedStatus => LastAcceptedStatus(PreliminaryTripDetailsLastAcceptedMessage);

		EDIMessage PreliminaryTripDetailsLastAcceptedMessage => Factory.GetValue(ref preliminaryTripDetailsLastAcceptedMessageCached, () => GetLastAcceptedMessage(MessageTypes.Codes.PreliminaryTrip, new ZString[] { MessageActionCodes.Codes.Original, MessageActionCodes.Codes.Change }));
		CachedProperty<EDIMessage> preliminaryTripDetailsLastAcceptedMessageCached;

		public ZDateTime CrewPassengersLastAcceptedDate => LastAcceptedDate(CrewPassengersLastAcceptedMessage);
		public ZString CrewPassengersLastAcceptedStatus => LastAcceptedStatus(CrewPassengersLastAcceptedMessage);

		EDIMessage CrewPassengersLastAcceptedMessage => Factory.GetValue(ref crewPassengersLastAcceptedMessageCached, () => GetLastAcceptedMessage(MessageTypes.Codes.CrewAndPassenger, new ZString[] { MessageActionCodes.Codes.Original, MessageActionCodes.Codes.Change }));
		CachedProperty<EDIMessage> crewPassengersLastAcceptedMessageCached;

		public ZDateTime CompleteTripDetailsLastAcceptedDate => LastAcceptedDate(CompleteTripDetailsLastAcceptedMessage);
		public ZString CompleteTripDetailsLastAcceptedStatus => LastAcceptedStatus(CompleteTripDetailsLastAcceptedMessage);

		EDIMessage CompleteTripDetailsLastAcceptedMessage => Factory.GetValue(ref completeTripDetailsLastAcceptedMessageCached, () => GetLastAcceptedMessage(MessageTypes.Codes.PreliminaryTrip, new ZString[] { MessageActionCodes.Codes.Confirmation }));
		CachedProperty<EDIMessage> completeTripDetailsLastAcceptedMessageCached;

		public ZDateTime CancelTripAndLinkedShipmentsLastAcceptedDate => LastAcceptedDate(CancelTripAndLinkedShipmentsLastAcceptedMessage);
		public ZString CancelTripAndLinkedShipmentsLastAcceptedStatus => LastAcceptedStatus(CancelTripAndLinkedShipmentsLastAcceptedMessage);

		EDIMessage CancelTripAndLinkedShipmentsLastAcceptedMessage => Factory.GetValue(ref cancelTripAndLinkedShipmentsLastAcceptedMessageCached, () => GetLastAcceptedMessage(MessageTypes.Codes.PreliminaryTrip, new ZString[] { MessageActionCodes.Codes.Cancellation }));
		CachedProperty<EDIMessage> cancelTripAndLinkedShipmentsLastAcceptedMessageCached;

		public ZDateTime RegisterCrewInformationLastAcceptedDate => LastAcceptedDate(RegisterCrewInformationLastAcceptedMessage);
		public ZString RegisterCrewInformationLastAcceptedStatus => LastAcceptedStatus(RegisterCrewInformationLastAcceptedMessage);

		EDIMessage RegisterCrewInformationLastAcceptedMessage => Factory.GetValue(ref registerCrewInformationLastAcceptedMessageCached, () => GetLastAcceptedMessage(MessageTypes.Codes.CrewOrEquipmentRegistration, new ZString[] { MessageActionCodes.Codes.Original, MessageActionCodes.Codes.Change }));
		CachedProperty<EDIMessage> registerCrewInformationLastAcceptedMessageCached;
		#endregion

		internal const string Truck = "30";
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UniversalDataContext(DataContextType.Service)]
	[UniversalCopyIgnoreElement(Schema.ES_ServiceId, Schema.ES_ExternalServiceId)]
	public class JobService : AutoJobService, IJobService, IDocumentSupportable, IProcessHandlingInfoProvider
	{
		public JobService(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public IHaveServices Parent
		{
			get { return parent == null && ES_ParentID.IsValid && ES_ParentTableCode.IsValid ? (IHaveServices)Factory.Load(GetTypeFromPrefix(ES_ParentTableCode), ES_ParentID) : parent; }
			set { parent = value; }
		}
		IHaveServices parent;

		public BusinessObject RequestForServiceParent
		{
			get { return Parent?.ServiceParent; }
		}

		#endregion

		#region BusinessObject Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override void OnSaving()
		{
			HandleServiceEventsOnSaving();
			base.OnSaving();
			PopulateServiceIdIfNeeded(this);
		}

		public static void PopulateServiceIdIfNeeded(JobService service)
		{
			if (!service.ES_ServiceId.IsEmpty || service.IsDeleted || service.IsInDatabase || !service.ShouldPopulateServiceId)
			{
				return;
			}

			if (service.Factory is IDbConnected)
			{
				service.ES_ServiceId = Environment.Env.NumberFountains.JobServiceId.GetNextFormatted(service.Factory);
			}
		}

		public bool HaveServiceId => !ES_ServiceId.IsEmpty || !IsDeleted && !IsInDatabase && ShouldPopulateServiceId;

		public bool ShouldPopulateServiceId { get; set; } = true;

		public override void Delete()
		{
			Parent?.JobServiceDeleted(this.PK);
			HandleServiceEventsOnDelete();
			base.Delete();
		}

		#endregion

		#region Service Events

		void HandleServiceEventsOnSaving()
		{
			var logsCollection = GetLogsCollection();

			if (!IsInDatabase || ES_ServiceCodeInfo.HasChanges || ES_BookedDateTimeOffsetInfo.HasChanges || ES_ReferencesInfo.HasChanges)
			{
				CreateOrUpdateServiceEventLog(logsCollection, AutoEvents.ServiceRequested, ES_Booked);
			}

			if (!IsInDatabase || ES_ServiceCodeInfo.HasChanges || ES_CompletedDateTimeOffsetInfo.HasChanges || ES_ReferencesInfo.HasChanges)
			{
				CreateOrUpdateServiceEventLog(logsCollection, AutoEvents.ServiceCompleted, ES_Completed);
			}
		}

		void HandleServiceEventsOnDelete()
		{
			var logsCollection = GetLogsCollection();
			CancelOrDeleteServiceEventLog(logsCollection, AutoEvents.ServiceRequestedCode);
			CancelOrDeleteServiceEventLog(logsCollection, AutoEvents.ServiceCompletedCode);
		}

		IEnumerable<Logs> GetLogsCollection()
		{
			var logsCollection = new List<Logs> { Logs };

			if (ShouldHandleServiceEventsOnParent)
			{
				var serviceParentLogs = GetServiceParentLogs();
				if (serviceParentLogs != null)
				{
					logsCollection.Add(serviceParentLogs);
				}
			}

			return logsCollection;
		}

		void CreateOrUpdateServiceEventLog(IEnumerable<Logs> logsCollection, Event addEvent, ZDateTime eventDate)
		{
			logsCollection?.ForEach(logs => CreateOrUpdateServiceEventLog(logs, addEvent, eventDate));
		}

		void CreateOrUpdateServiceEventLog(Logs logs, Event addEvent, ZDateTime eventDate)
		{
			if (logs == null)
			{
				return;
			}

			var needsReferenceNumber = (parent?.ServiceParent as IServicesParent)?.NeedsReferenceNumber ?? false;

			CancelOrDeleteServiceEventLog(logs, addEvent.Code, needsReferenceNumber);

			if (eventDate.IsValid)
			{
				var eventParam = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, ES_ServiceCode) };

				if (needsReferenceNumber)
				{
					eventParam.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, ES_References));
				}

				if (ShouldParentSpecifyEventParam)
				{
					var parentWithServiceEvents = Parent as IHaveEventsFromServices;
					eventParam.AddRange(parentWithServiceEvents.ReferenceParameters);
				}

				var eventDateWithOffset = new ZDateTimeOffset(eventDate); // This offset is inaccurate because the current branch may be different to ES_Booked or ES_Completed.
				var log = logs.CreateRecreateOrUpdateEventLog(addEvent, EstimateActual.Actual, eventDateWithOffset, ZString.Empty, eventParam.ToArray());
				if (log != null)
				{
					if (!LogsCreatedByThis.TryGetValue(logs, out var logsCreated))
					{
						logsCreated = new List<StmALog>();
						LogsCreatedByThis[logs] = logsCreated;
					}
					logsCreated.Add(log);
				}
			}
		}

		void CancelOrDeleteServiceEventLog(IEnumerable<Logs> logsCollection, string eventCode, bool needsReferenceNumber = false)
		{
			logsCollection?.ForEach(logs => CancelOrDeleteServiceEventLog(logs, eventCode, needsReferenceNumber));
		}

		void CancelOrDeleteServiceEventLog(Logs logs, string eventCode, bool needsReferenceNumber = false)
		{
			if (IsInDatabase && logs != null)
			{
				var eventParam = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, ES_ServiceCodeInfo.OriginalValue.ToString()) };
				if (needsReferenceNumber)
				{
					eventParam.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, ES_ReferencesInfo.OriginalValue.ToString()));
				}
				if (ShouldParentSpecifyEventParam)
				{
					var parentWithServiceEvents = Parent as IHaveEventsFromServices;
					eventParam.AddRange(parentWithServiceEvents.ReferenceParameters);
				}
				var reference = StmALog.GenerateEventReference(string.Empty, eventParam.ToArray());

				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
				query.AddToFilter(StmALogSchema.SL_Reference, reference);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

				var oldLogs = logs.Find(query);
				foreach (var log in oldLogs)
				{
					if (log.IsInDatabase)
					{
						log.Cancel();
					}
				}
			}

			if (LogsCreatedByThis.TryGetValue(logs, out var logsCreated))
			{
				var oldLog = logsCreated.FirstOrDefault(c => !c.IsDeleted && !c.IsInDatabase && c.SL_SE_NKEvent == eventCode);
				if (oldLog != null)
				{
					logsCreated.Remove(oldLog);
					oldLog.Delete();
				}
			}
		}

		bool ShouldHandleServiceEventsOnParent
		{
			get
			{
				return Parent != null && Parent.NeedsServiceEvents;
			}
		}

		bool ShouldParentSpecifyEventParam
		{
			get
			{
				return Parent != null && Parent is IHaveEventsFromServices && ((IHaveEventsFromServices)Parent).ReferenceParameters.Any();
			}
		}

		Logs GetServiceParentLogs()
		{
			if (Parent != null)
			{
				var logParent = Parent.ServiceParent as IStmALogParent;
				if (logParent != null)
				{
					return logParent.Logs;
				}
			}

			return null;
		}

		Dictionary<Logs, List<StmALog>> LogsCreatedByThis
		{
			get { return logsCreatedByThis ?? (logsCreatedByThis = new Dictionary<Logs, List<StmALog>>()); }
		}
		Dictionary<Logs, List<StmALog>> logsCreatedByThis;

		#endregion

		#region Property Overrides

		[LightValidationTestExempt]
		public override ZGuid ES_ParentID { get => base.ES_ParentID; set => base.ES_ParentID = value; }
		[LightValidationTestExempt]
		public override ZString ES_ParentTableCode { get => base.ES_ParentTableCode; set => base.ES_ParentTableCode = value; }

		[List("Lookups.JobServiceType_List")]
		public override ZString ES_ServiceCode
		{
			get { return base.ES_ServiceCode; }
			set
			{
				base.ES_ServiceCode = value;
				ES_DurationInfo.RefreshBinding();

				SetToDefaultContractor();
				if (MarkParentAsNeedingValidation)
				{
					RequestForServiceParent?.MarkAsNeedingValidation();
				}
			}
		}

		[List("Lookups.ServiceContractor_List")]
		public override ZGuid ES_OH_Contractor
		{
			get { return base.ES_OH_Contractor; }
			set { base.ES_OH_Contractor = value; }
		}

		public ZDateTime ES_Booked
		{
			get => ES_BookedCore;
			set
			{
				ES_BookedCore = value;
				ES_BookedInfo.RefreshBinding();
			}
		}

		protected virtual ZDateTime ES_BookedCore
		{
			get => base.ES_BookedDateTimeOffset.ToZDateTime();
			set
			{
				base.ES_BookedDateTimeOffset = value.ToDateTimeOffset(ServiceBranchHomePort);
				if (MarkParentAsNeedingValidation)
				{
					RequestForServiceParent?.MarkAsNeedingValidation();
				}
			}
		}

		public ZPropertyInfo ES_BookedInfo => GetZPropertyInfo(nameof(ES_Booked));

		public ZDateTime ES_Completed
		{
			get => ES_CompletedCore;
			set
			{
				ES_CompletedCore = value;
				ES_CompletedInfo.RefreshBinding();
			}
		}

		protected virtual ZDateTime ES_CompletedCore
		{
			get => base.ES_CompletedDateTimeOffset.ToZDateTime();
			set => base.ES_CompletedDateTimeOffset = value.ToDateTimeOffset(ServiceBranchHomePort);
		}

		public ZPropertyInfo ES_CompletedInfo => GetZPropertyInfo(nameof(ES_Completed));

		RefUNLOCO ServiceBranchHomePort => Factory.GetCachedValue(ES_ParentID, () => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, Parent?.ServiceBranch?.NKUNLOCO));

		[ZDateTimeDurationValue]
		public override ZDateTime ES_Duration
		{
			get { return new ZDateTime(base.ES_Duration, DateTimeKind.Unspecified); }
			set { base.ES_Duration = value.ConvertToDurationBasedDate(ES_DurationInfo); }
		}

		public override ZGuid ES_OA_Location
		{
			get { return base.ES_OA_Location; }
			set
			{
				if (base.ES_OA_Location != value)
				{
					base.ES_OA_Location = value;
					ES_RX_NKServiceRateCurrency = Location?.Country?.LocalCurrency?.RX_Code ?? ZString.Empty;
				}
			}
		}

		[ResourceStringData("E054A9D6-0AC5-4C95-A123-A447AEE76FA1", Caption = "Sub Location")]
		public override ZString ES_SubLocation { get => base.ES_SubLocation; set => base.ES_SubLocation = value; }

		[List("Lookups.MeasurementBasisList")]
		[MaxLength(3)]
		public override ZString ES_MeasurementBasis
		{
			get { return base.ES_MeasurementBasis; }
			set { base.ES_MeasurementBasis = value; }
		}

		#endregion

		#region Calculated Properties

		#region ES_Calc_LocationCode

		[BusinessObjectTestExclude]
		[List("Lookups.LocationAddress_List")]
		[MaxLength(25)]
		public virtual ZString ES_Calc_LocationCode
		{
			get
			{
				var address = Factory.Load<OrgAddress>(ES_OA_Location);

				return (address != null) ? address.OA_Code : ZString.Empty;
			}
			set
			{
				var filter = new ZQuery(OrgAddressSchema.OA_OH, ServiceProviderPK);
				filter.AddToFilter(OrgAddressSchema.OA_Code, value);

				var address = Factory.LoadTop1<OrgAddress>(filter);
				ES_OA_Location = (address != null) ? address.PK : ZGuid.Empty;

				ES_Calc_LocationCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ES_Calc_LocationCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ES_Calc_LocationCode)); }
		}

		#endregion

		#region ServiceProviderPK

		[List("Lookups.ServiceProvider")]
		public ZGuid ServiceProviderPK
		{
			get { return ServiceProvider != null ? ServiceProvider.PK : ZGuid.Empty; }
			set
			{
				var org = Factory.Load<OrgHeader>(value);
				ES_OA_Location = (org != null) ? org.MainAddress.PK : ZGuid.Empty;

				ServiceProviderPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ServiceProviderPKInfo
		{
			get { return GetZPropertyInfo(nameof(ServiceProviderPK)); }
		}

		public OrgHeader ServiceProvider
		{
			get { return Location != null ? Location.Header : null; }
		}

		#endregion

		#region ParentContextID

		[BusinessObjectTestExclude]
		[List("Lookups.ServiceContext_List")]
		public ZGuid ParentContextID
		{
			get
			{
				return ES_CurrentContextID.IsEmpty && Parent != null ? Parent.PK : base.ES_CurrentContextID;
			}
			set
			{
				var parentServicesWithContext = Parent as IHaveServicesWithContext;
				var tableCode = parentServicesWithContext?.GetContextTableCode(value) ?? ZString.Empty;
				ES_CurrentContextID = tableCode.IsEmpty ? ZGuid.Empty : value;
				ES_CurrentContextTableCode = tableCode;

				ParentContextIDInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ParentContextIDInfo
		{
			get { return GetZPropertyInfo(nameof(ParentContextID)); }
		}

		#endregion

		#region ES_Calc_Description

		[MaxLength(40)]
		public ZString ES_Calc_Description
		{
			get { return ES_ServiceCode.IsEmpty ? "" : Lookups.JobServiceType_List.GetDescriptionFromCode(ES_ServiceCode); }
		}

		public ZPropertyInfo ES_Calc_DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ES_Calc_Description)); }
		}

		#endregion

		internal TimeSpan ServiceDuration
		{
			get { return ES_Duration.IsValid ? ES_Duration.ToTimeSpan() : new TimeSpan(0); }
		}

		public bool MarkParentAsNeedingValidation;

		#endregion

		#region Default Contractors

		public void SetToDefaultContractor()
		{
			if (ES_ServiceCode == Core.Constants.FreightServiceType.Codes.Fumigation && ES_OH_Contractor.IsEmpty)
			{
				ES_OH_Contractor = DefaultFumigationContractor;
			}
		}

		protected ZGuid DefaultFumigationContractor
		{
			get
			{
				ZGuid result = ZGuid.Empty;

				if (Parent != null)
				{
					if (Parent.TransportMode == Core.Constants.TransportModes.Air)
					{
						result = FreightDataRegistry.Instance.AIRFumigationContractor.Value;
					}
					else if (Parent.ContainerMode == Core.Constants.ContainerModes.LCL)
					{
						result = FreightDataRegistry.Instance.LCLFumigationContractor.Value;
					}
					else if (Parent.ContainerMode == Core.Constants.ContainerModes.FCL)
					{
						result = FreightDataRegistry.Instance.FCLFumigationContractor.Value;
					}
				}

				return result;
			}
		}

		#endregion

		#region GetPropertiesToExcludeFromCloning

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				JobServiceSchema.Constants.ES_ServiceId,
				JobServiceSchema.Constants.ES_ExternalServiceId
			};
		}

		#endregion

		#region Implementation

		protected virtual Type GetTypeFromPrefix(string prefix)
		{
			if (PrefixToTypeHash == null)
			{
				PrefixToTypeHash = new Dictionary<string, Type>();
				PrefixToTypeHash[JobDocsAndCartageSchema.Constants.Prefix] = ObjectFactory.GetType<IJobDocsAndCartage>();
				PrefixToTypeHash[JobContainerSchema.Constants.Prefix] = ObjectFactory.GetType<ICommonContainer>();
				PrefixToTypeHash[CusContainerSchema.Constants.Prefix] = ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
				PrefixToTypeHash[DtbConsignmentRunSheetSchema.Constants.Prefix] = ObjectFactory.GetType<TransportConsignment.Integration.IDtbConsignmentRunSheet>();
				PrefixToTypeHash[WhsDocketSchema.Constants.Prefix] = ObjectFactory.GetType<Warehouse.Integration.IWhsDocket>();
				PrefixToTypeHash[WorkItemSchema.Constants.Prefix] = ObjectFactory.GetType<ProcessManagement.Integration.IWorkItem>();
				PrefixToTypeHash[JobBookedCtgMoveSchema.Constants.Prefix] = ObjectFactory.GetType<Freight.LocalCartage.Integration.ICommonBookedCtgMove>();
				PrefixToTypeHash[DtbBookingSchema.Constants.Prefix] = ObjectFactory.GetType<Enterprise.Integration.TransportConsignment.IDtbBookingConsignment>();
				PrefixToTypeHash[WhsAdHocServiceJobSchema.Constants.Prefix] = ObjectFactory.GetType<Warehouse.Integration.IWhsAdHocServiceJob>();
				PrefixToTypeHash[WhsVASOrderSchema.Constants.Prefix] = ObjectFactory.GetType<Warehouse.Integration.IWhsVASOrder>();
				PrefixToTypeHash[CusInBondHeaderSchema.Constants.Prefix] = ObjectFactory.GetType<Enterprise.Integration.Customs.ICusInBondHeader>();
				PrefixToTypeHash[WhsItemReceiveConsignmentSchema.Constants.Prefix] = ObjectFactory.GetType<Warehouse.Integration.IWhsItemReceiveConsignment>();
				PrefixToTypeHash[WhsItemDispatchConsignmentSchema.Constants.Prefix] = ObjectFactory.GetType<Warehouse.Integration.IWhsItemDispatchConsignment>();
				PrefixToTypeHash[DtbConsignmentSchema.Constants.Prefix] = ObjectFactory.GetType<Enterprise.Integration.LandTransport.IDtbConsignment>();
				PrefixToTypeHash[CYDAdHocServiceOrderSchema.Constants.Prefix] = ObjectFactory.GetType<Enterprise.Integration.ICYDAdHocServiceOrder>();

#if DEBUG
				PrefixToTypeHash["Z0"] = typeof(Testing.DummyWithServices);
				PrefixToTypeHash["Z1"] = typeof(Testing.DummyConsignmentWithServices);
#endif
			}

			try
			{
				return PrefixToTypeHash[prefix];
			}
			catch (KeyNotFoundException ex)
			{
				ErrorReporter.ReportOnce("8C8B5360-2D85-4176-B565-6A0598842588", string.Format(CultureInfo.InvariantCulture, "PrefixToTypeHash does not contain the prefix : {0}", prefix), ex);
				throw;
			}
		}

		protected Dictionary<string, Type> PrefixToTypeHash;

		#endregion

		#region IDocumentSupportable Members

		public virtual DocumentSupporter DocumentSupporter
		{
			get { return new JobServiceDocumentSupporter(this); }
		}

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new JobServiceProcessHandlingInfo(this); }
		}

		#endregion

		internal Enterprise.Integration.Customs.IJobServiceTypeProvider GetJobServiceTypeProvider(string country)
		{
			var provider = ObjectFactory.Get<Hashtable>("JobServiceTypeProviders")[country] as ObjectHandle;
			return provider?.GetObject() as Enterprise.Integration.Customs.IJobServiceTypeProvider;
		}

		public bool ServiceTypeNeedsToBeUnique => GetJobServiceTypeProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.ServiceTypeNeedsToBeUnique(ES_ServiceCode) ?? true;
	}
}

#region Test
#if DEBUG

namespace Enterprise.MasterFiles.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Integration;
	using Enterprise.BufferManagement.Integration;
	using Enterprise.Environment;
	using Enterprise.Integration;
	using Enterprise.Warehouse.Integration;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using static Enterprise.Integration.Customs;

	#region class DummyWithServices

	public class DummyWithServices : DummyBusinessObject, IHaveServices, IWorkflowProvider, IStmALogProvider, IStmALogParent
	{
		public DummyWithServices(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IHaveServices Members

		bool jobServiceDeletedCalled;

		public bool JobServiceDeletedCalled()
		{
			return jobServiceDeletedCalled;
		}

		public ZString TableCode
		{
			get { return TablePrefix; }
		}

		public JobServiceDependentCollection Services
		{
			get { return services ?? (services = new JobServiceDependentCollection(this, Factory)); }
		}

		public ZString TransportMode { get; set; }

		public ZString ContainerMode { get; set; }

		public IHaveServices[] DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		public BusinessObject ServiceParent
		{
			get { return this; }
		}

		public bool NeedsServiceEvents { get; set; }

		void IHaveServices.JobServiceDeleted(ZGuid servicePK)
		{
			jobServiceDeletedCalled = true;
		}

		IBranch IHaveServices.ServiceBranch => Env.CurrentBranch;

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
		DummyProcessTaskCollection workflowItems;

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

		#endregion

		#region IStmALogProvider

		Logs IStmALogProvider.Logs => logs ?? (logs = new Logs(this));
		Logs logs;

		BusinessObjectFactory IStmALogProvider.LogsFactory => Factory;

		#endregion

		#region IStmALogParent

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents => Array.Empty<BusinessObject>();

		ZGuid IStmALogParent.LogsParentPK => PK;

		string IStmALogParent.LogsParentTableName => TableName;

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow => false;

		#endregion

		JobServiceDependentCollection services;
	}

	#endregion

	#region class DummyWithServicesWithContext

	public class DummyWithServicesWithContext : DummyBusinessObject, IHaveServicesWithContext
	{
		public DummyWithServicesWithContext(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IHaveServicesWithContext Members

		public ZString TableCode
		{
			get { return TablePrefix; }
		}

		public JobServiceDependentCollection Services
		{
			get { return services ?? (services = new JobServiceDependentCollection(this, Factory)); }
		}

		public ZString TransportMode { get; set; }

		public ZString ContainerMode { get; set; }

		public IHaveServices[] DependentServiceParents
		{
			get { return Array.Empty<IHaveServices>(); }
		}

		public BusinessObject ServiceParent
		{
			get { return this; }
		}

		public CodeDescriptionPairList ServiceCurrentContextList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("DummyCode", "DummyDescription");

				return result;
			}
		}

		public ZString GetContextTableCode(ZGuid contextID)
		{
			return contextID.IsEmpty ? "" : "DUM";
		}

		public bool NeedsServiceEvents { get; set; }

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Env.CurrentBranch;

		#endregion

		JobServiceDependentCollection services;
	}

	#endregion

	#region DummyConsignmentWithServices

	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	public class DummyConsignmentWithServices : DummyWithServices, IConsignment
	{
		public DummyConsignmentWithServices(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string TablePrefix => "Z1";

		public IWhsWarehouse Warehouse => WarehouseForTest;
		public IJobDocAddress BookingPartyDocAddress => BookingPartyDocAddressForTest;
		public ZString MasterBillNumber => MasterBillNumberForTest;
		public ZString HouseBillNumber => HouseBillNumberForTest;
		public ZString JobID => JobIDForTest;
		public ZString ShipmentNumber => ShipmentNumberForTest;
		public ZString Direction => DirectionForTest;
		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers => AdditionalReferenceNumbersForTest;

		public IWhsWarehouse WarehouseForTest { private get; set; }
		public JobDocAddress BookingPartyDocAddressForTest { private get; set; }
		public ZString MasterBillNumberForTest { private get; set; }
		public ZString HouseBillNumberForTest { private get; set; }
		public ZString JobIDForTest { private get; set; }
		public ZString ShipmentNumberForTest { private get; set; }
		public ZString DirectionForTest { private get; set; }
		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbersForTest { private get; set; }
	}

	#endregion
}

#endif
#endregion

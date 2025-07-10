
#if DEBUG

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	#region DummyWithWorkflow

	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	[UserDefinedValues]
	public class DummyWithWorkflow : DummyEnterpriseBusinessObject, IDummyWithWorkflow, IWorkflowProviderIncludingRelated, ICancellable, IRelatedJob, IModuleToModule, IWorkflowProviderEvent, IValidateForCustomsMessagingSupporter, IJobHeaderParent, IWorkflowTriggerFieldChangeSource, IDocumentSupportable, IWorkflowTriggerEventSource, IRegisterStatusChangeContext, Enterprise.Integration.Customs.EU.NCTS.INctsHeaderWithAdditionalMessagingValidation, IAuditDetailsWithContext
	{
		public DummyWithWorkflow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Argument.NotNull(DummyWorkflowDescriptor.Instance, nameof(DummyWorkflowDescriptor));
		}

		public static Overridable<EnterpriseBusinessObject.AutologState> AutoLogState = new Overridable<EnterpriseBusinessObject.AutologState>(EnterpriseBusinessObject.AutologState.NotLogged);

		void IDummyWithWorkflow.TurnOnAutoLogging() => AutoLogState.Value = EnterpriseBusinessObject.AutologState.AutoLogged;
		void IDummyWithWorkflow.TurnOnAutoLoggingToQueueOnly() => AutoLogState.Value = EnterpriseBusinessObject.AutologState.AutoLoggedToQueueOnly;
		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => AutoLogState.Value;

		protected override ZString CustomLogReferenceSuffix
		{
			get { return fLogReferenceSuffix; }
		}
		ZString fLogReferenceSuffix;

		public void SetCustomLogReferenceSuffix(string logReferenceSuffix)
		{
			fLogReferenceSuffix = logReferenceSuffix;
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get
			{
				if (BusinessObjectsWithRelatedEvents_ReturnNull)
				{
					return null;
				}
				List<BusinessObject> result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEvents);
				if (RelatedDummyWithTasks != null)
				{
					result.Add(RelatedDummyWithTasks);
				}
				if (RelatedDummyWithTasks2 != null)
				{
					result.Add(RelatedDummyWithTasks2);
				}
				if (HaveNullAsRelatedObject)
				{
					result.Add(null);
				}
				return result.ToArray();
			}
		}

		public void InitRelatedDummyWithTasks()
		{
			RelatedDummyWithTasks = Factory.New<DummyWithWorkflow>();
			RelatedDummyWithTasks2 = Factory.New<DummyWithWorkflow>();
		}

		public virtual DummyWithWorkflow RelatedDummyWithTasks { get; set; }
		public bool HaveNullAsRelatedObject { get; set; }
		public bool BusinessObjectsWithRelatedEvents_ReturnNull { get; set; }

		public ZGuid RelatedDummyWithTasksClient
		{
			get { return RelatedDummyWithTasks?.Client ?? ZGuid.Empty; }
			set
			{
				if (RelatedDummyWithTasks != null)
				{
					RelatedDummyWithTasks.Client = value;
				}
			}
		}

		int trackedStringReadCount;

		public int TrackedStringReadCount => trackedStringReadCount;

		public ZString TrackedString
		{
			get
			{
				trackedStringReadCount++;
				return ZString.Empty;
			}
		}

		public Forwarding.IForwardingShipment GetChildShipment(ZGuid pk)
		{
			childShipments.TryGetValue(pk, out var result);
			return result;
		}

		public void AttachChildShipment(Forwarding.IForwardingShipment shipment)
		{
			childShipments[shipment.PK] = shipment;
			this.BusinessObjectRelationChanged((BusinessObject)shipment, BusinessObjectParentLocatorEvent.ParentAdded);
		}

		public void DetachChildShipment(Forwarding.IForwardingShipment shipment)
		{
			if (childShipments.Remove(shipment.PK))
			{
				this.BusinessObjectRelationChanged((BusinessObject)shipment, BusinessObjectParentLocatorEvent.ParentRemoved);
			}
		}

		readonly Dictionary<ZGuid, Forwarding.IForwardingShipment> childShipments = new Dictionary<ZGuid, Forwarding.IForwardingShipment>();

		public ZPropertyInfo RelatedDummyWithTasksClientInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(RelatedDummyWithTasksClient), x => RelatedDummyWithTasks?.ClientInfo); }
		}

		public DummyWithWorkflow RelatedDummyWithTasks2 { get; set; }

		public ZString SubType1
		{
			get { return subType1; }
			set { SetNonPersistentPropertyValue(SubType1Info, ref subType1, value); }
		}
		ZString subType1;

		public ZPropertyInfo SubType1Info
		{
			get { return GetZPropertyInfo(nameof(SubType1)); }
		}

		public ZString SubType1_Extra
		{
			get { return subType1_extra; }
			set { SetNonPersistentPropertyValue(SubType1_ExtraInfo, ref subType1_extra, value); }
		}
		ZString subType1_extra;

		public ZPropertyInfo SubType1_ExtraInfo
		{
			get { return GetZPropertyInfo(nameof(SubType1_Extra)); }
		}

		public ZGuid Client
		{
			get { return client; }
			set { SetNonPersistentPropertyValue(ClientInfo, ref client, value); }
		}
		ZGuid client;

		public ZPropertyInfo ClientInfo
		{
			get { return GetZPropertyInfo(nameof(Client)); }
		}

		public ZString LoadPort { get; set; }
		public ZString DischargePort { get; set; }
		public bool UseRealShipmentForInternalUniversalXMLSending;

		public ZString OverridenHumanReadableName { get; set; }

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return OverridenHumanReadableName.IsEmpty ? ZString.Format("Dummy Business Object {0}", Z0_Description) : OverridenHumanReadableName;
			}
		}

		public ZString ThrowingProperty => throw throwingPropertyExceptionSupplier?.Invoke() ?? new Exception();

		public void SetThrowingPropertyExceptionSupplier(Func<Exception> throwingPropertyExceptionSupplier)
		{
			this.throwingPropertyExceptionSupplier = throwingPropertyExceptionSupplier;
		}
		Func<Exception> throwingPropertyExceptionSupplier;

		public NotaZType BadType => new NotaZType(5);

		public struct NotaZType
		{
			public int a;

			public NotaZType(int a)
			{
				this.a = a;
			}
		}

		#region On Save

		public static Overridable<bool> ShouldApplyWorkflowTemplateOnSave { get; } = new Overridable<bool>(false);

		protected override void OnFactorySaving()
		{
			if (ShouldApplyWorkflowTemplateOnSave.Value)
			{
				this.ApplyWorkflowTemplates();
			}
			base.OnFactorySaving();
		}

		#endregion

		#region IWorkflowProvider

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return GetWorkflowType(); }
		}

		protected virtual string GetWorkflowType()
		{
			return "DUM";
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		public virtual DummyProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = GetNewWorkflowItems();
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		DummyProcessTaskCollection workflowItems;

		IEnumerable<IWorkflowProvider> IWorkflowProviderIncludingRelated.RelatedIWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();

				if (RelatedDummyWithTasks != null)
				{
					result.Add(RelatedDummyWithTasks);
				}
				if (RelatedDummyWithTasks2 != null)
				{
					result.Add(RelatedDummyWithTasks2);
				}
				if (GetRelatedWorkflowProviders_ForTest != null)
				{
					result.AddRange(GetRelatedWorkflowProviders_ForTest());
				}
				return result;
			}
		}

		public Func<IEnumerable<IWorkflowProvider>> GetRelatedWorkflowProviders_ForTest { get; set; }

		protected virtual DummyProcessTaskCollection GetNewWorkflowItems()
		{
			return this.GetOrCreateProcessTaskCollection(() => new DummyProcessTaskCollection(this));
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => WorkflowInformationProviderOverride;

		public IWorkflowInformationProvider WorkflowInformationProviderOverride { get; set; }

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, SubType1, SubType1_Extra, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, Z0_Code, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_LoadPortCountry, LoadPort, LoadPort.Left(2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_DischargePortCountry, DischargePort, DischargePort.Left(2), ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, Client, RelatedDummyWithTasksClient, null);
			return result;
		}

		#endregion

		#region ICancellable Members

		public bool PreventDelete
		{
			get { return true; }
		}

		public string CanCancel()
		{
			return null;
		}

		public string CanReactivate()
		{
			return null;
		}

		bool isCancelled;
		public virtual bool IsCancelled
		{
			get
			{
				return isCancelled;
			}
			set
			{
				isCancelled = value;
			}
		}

		public bool IsCancelledHasChanged
		{
			get { return false; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return Z0_Description; }
		}

		#endregion

		#region IControllerIDProvider Members

		public Guid BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		public ControllerID ControllerID
		{
			get { return DummyControllerIDs.Dummy; }
		}

		#endregion

		#region IRelatedJob Members

		public ZString JobDescription
		{
			get { return "Dummy Job Description"; }
		}

		ZString IRelatedJob.JobNumber
		{
			get { return "Dummy Job No."; }
		}

		public ZString JobStatus
		{
			get { return "Dummy Job Status"; }
		}

		#endregion

		protected override DummyBizoValidation GetNewValidation()
		{
			return ValidationForTesting ?? base.GetNewValidation();
		}

		public DummyBizoValidation ValidationForTesting;

		#region IModuleToModule Members

		void IModuleToModule.AddToRelatedJobs(BusinessObject loadedJob)
		{
			throw new NotImplementedException();
		}

		bool IModuleToModule.CanExportData(out ZString errorMessage)
		{
			errorMessage = ZString.Empty;
			return true;
		}

		BusinessObject IModuleToModule.GetRelatedObject()
		{
			throw new NotImplementedException();
		}

		IOrgHeader IModuleToModule.RecipientOrganisation
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region IWorkflowProviderEvent Members

		OrgHeader[] IWorkflowProviderEvent.RecipientOrganisations
		{
			get { return new OrgHeader[] { GlbCompany.CurrentCompany.OrgProxy }; }
		}

		#endregion

		#region IJobHeaderParent Members

		public void SetJobNumberFieldOnSaving()
		{
			throw new NotImplementedException();
		}

		public void OnJobCreating(JobHeader job)
		{
			throw new NotImplementedException();
		}

		public void OnJobCreated(JobHeader job)
		{
			throw new NotImplementedException();
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		public bool AllowInvoiceDeletion
		{
			get { throw new NotImplementedException(); }
		}

		#endregion

		#region IValidateForCustomsMessagingSupporter Members

		ZBool IValidateForCustomsMessagingSupporter.SupportValidateCustomsMessaging
		{
			get { return true; }
		}

		BusinessObject IValidateForCustomsMessagingSupporter.GetEntityToValidate(string triggerAction)
		{
			return this;
		}

		#endregion

		#region IDocumentSupportable

		public virtual DocumentSupporter DocumentSupporter => supporter ?? (supporter = new DummyDocumentSupporter(this));
		DocumentSupporter supporter;

		#endregion

		#region ChangeLogging

		public StmChangeLogCollection FieldChangeLogs
		{
			get
			{
				if (fieldChangeLogs == null)
				{
					fieldChangeLogs = new StmChangeLogCollection(this);
				}
				return fieldChangeLogs;
			}
		}
		StmChangeLogCollection fieldChangeLogs;

		public DummyWithWorkflow RelatedWorkflowProvider
		{
			get
			{
				if (relatedWorkflowProvider == null)
				{
					relatedWorkflowProvider = Factory.LoadTop1<DummyWithWorkflow>(new ZQuery(DummyBizoSchema.Z0_Guid, PK));
					if (relatedWorkflowProvider == null)
					{
						relatedWorkflowProvider = Factory.New<DummyWithWorkflow>();
						relatedWorkflowProvider.Z0_Guid = PK;
					}
				}
				return relatedWorkflowProvider;
			}
		}

		DummyWithWorkflow relatedWorkflowProvider;

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				var relatedProvider = Factory.LoadTop1<DummyWithWorkflow>(new ZQuery(DummyBizoSchema.Z0_Guid, PK));
				return relatedProvider == null ? Array.Empty<IWorkflowProvider>() : new IWorkflowProvider[] { relatedProvider };
			}
		}

		#endregion

		#region ProcessLog

		protected sealed override void ProcessLogCore(IStmALog log)
		{
			ProcessLog_Override.Value?.Invoke(this, log);
		}
		public readonly static Overridable<Action<DummyWithWorkflow, IStmALog>> ProcessLog_Override = new Overridable<Action<DummyWithWorkflow, IStmALog>>(null);

		#endregion

		#region IDummyWithWorflow

		public IProcessTask AddNewTrigger()
		{
			return ((IWorkflowProvider)this).WorkflowItems.Triggers.AddNew();
		}

		public IProcessTask AddNewMilestone()
		{
			return ((IWorkflowProvider)this).WorkflowItems.Milestones.AddNew();
		}

		public IProcessTask AddNewTask()
		{
			return ((IWorkflowProvider)this).WorkflowItems.Tasks.AddNew();
		}

		#endregion

		#region IWorkflowTriggerEventSource

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders => ParentWorkflowProviders ?? Array.Empty<IWorkflowProviderCore>();

		public IWorkflowProviderCore[] ParentWorkflowProviders { get; set; }

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany => GlbCompany.CurrentCompany;

		#endregion

		#region IRegisterStatusChangeContext

		public ZString GetDummyValueForMacro
		{
			get
			{
				return triggeringEvent != null
					? $"hello|{triggeringEvent.SL_SE_NKEvent}{triggeringEvent.SL_Reference}"
					: "hello";
			}
		}

		IDisposable IRegisterStatusChangeContext.TemporarilySetStatusChangedByTriggerEvent(IStmALog triggeringEvent)
		{
			this.triggeringEvent = triggeringEvent;

			if (triggeringEvent != null)
			{
				var dummy1 = Factory.New<DummyWithWorkflow>();
				dummy1.Z0_Description = $"{triggeringEvent.SL_SE_NKEvent}{triggeringEvent.SL_Reference}";
			}

			return new DisposableAction(() => { this.triggeringEvent = null; });
		}

		IStmALog triggeringEvent;

		#endregion

		public ZString Z0_SystemLastEditUser
		{
			get => systemLastEditUser;
			set => SetNonPersistentPropertyValue(Z0_SystemLastEditUserInfo, ref systemLastEditUser, value);
		}
		ZString systemLastEditUser;
		public ZPropertyInfo Z0_SystemLastEditUserInfo => GetZPropertyInfo(nameof(Z0_SystemLastEditUser));

		#region SystemCreateUser Macro Test Properties

		public List<int> List_SystemCreateUser { get; }

		#endregion

		public GlbStaff SystemCreateUser
		{
			get => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, Z0_SystemCreateUser);
		}

		public ZString Z0_SystemCreateUser
		{
			get => systemCreateUserCode;
			set => SetNonPersistentPropertyValue(Z0_SystemCreateUserInfo, ref systemCreateUserCode, value);
		}
		ZString systemCreateUserCode;
		public ZPropertyInfo Z0_SystemCreateUserInfo => GetZPropertyInfo(nameof(Z0_SystemCreateUser));

		public ZString Z0_SystemCreateBranch
		{
			get => systemCreateBranch;
			set => SetNonPersistentPropertyValue(Z0_SystemCreateBranchInfo, ref systemCreateBranch, value);
		}
		ZString systemCreateBranch;
		public ZPropertyInfo Z0_SystemCreateBranchInfo => GetZPropertyInfo(nameof(Z0_SystemCreateBranch));

		public ZString Z0_SystemCreateDepartment
		{
			get => systemCreateDepartment;
			set => SetNonPersistentPropertyValue(Z0_SystemCreateDepartmentInfo, ref systemCreateDepartment, value);
		}
		ZString systemCreateDepartment;
		public ZPropertyInfo Z0_SystemCreateDepartmentInfo => GetZPropertyInfo(nameof(Z0_SystemCreateDepartment));

		public ZDateTime Z0_SystemCreateTimeUtc
		{
			get => systemCreateTimeUtc;
			set => SetNonPersistentPropertyValue(Z0_SystemCreateTimeUtcInfo, ref systemCreateTimeUtc, value);
		}
		ZDateTime systemCreateTimeUtc;
		public ZPropertyInfo Z0_SystemCreateTimeUtcInfo => GetZPropertyInfo(nameof(Z0_SystemCreateTimeUtc));

		public ZDateTime Z0_SystemLastEditTimeUtc
		{
			get => systemLastEditTimeUtc;
			set => SetNonPersistentPropertyValue(Z0_SystemLastEditTimeUtcInfo, ref systemLastEditTimeUtc, value);
		}
		ZDateTime systemLastEditTimeUtc;
		public ZPropertyInfo Z0_SystemLastEditTimeUtcInfo => GetZPropertyInfo(nameof(Z0_SystemLastEditTimeUtc));

		public DummyWithWorkflow OtherDummy { get; set; }

		#region IAuditDetails

		ZDateTime IAuditDetails.SystemCreateTimeUtc
		{
			get { return Z0_SystemCreateTimeUtc; }
		}

		ZString IAuditDetails.SystemCreateUser
		{
			get { return Z0_SystemCreateUser; }
		}

		ZDateTime IAuditDetails.SystemLastEditTimeUtc
		{
			get { return Z0_SystemLastEditTimeUtc; }
		}

		ZString IAuditDetails.SystemLastEditUser
		{
			get { return Z0_SystemLastEditUser; }
		}

		#endregion

		#region IAuditDetailsWithContext

		ZString IAuditDetailsWithContext.SystemCreateBranch
		{
			get { return Z0_SystemCreateBranch; }
		}

		ZString IAuditDetailsWithContext.SystemCreateDepartment
		{
			get { return Z0_SystemCreateDepartment; }
		}

		#endregion

		public class ShipmentToDummyLocator : IBusinessObjectParentLocator
		{
			public bool TryLocateParentBusinessObjects(BusinessObject child, out IEnumerable<BusinessObject> parents)
			{
				var dummies = child.Factory.Load<DummyWithWorkflow>(new ZQuery());
				parents = GetMatchingDummies();
				return parents.Any();

				IEnumerable<BusinessObject> GetMatchingDummies()
				{
					foreach (var dummy in dummies)
					{
						if (dummy.GetChildShipment(child.PK) != null)
						{
							yield return dummy;
						}
					}
				}
			}
		}

		public string[] GetAdditionalValidationErrorMessages()
		{
			return additionalValidationErrorMessages;
		}
		string[] additionalValidationErrorMessages = [];

		public void SetAdditionalValidationErrorMessages(string[] errorMessages)
		{
			additionalValidationErrorMessages = errorMessages;
		}
	}

	#endregion

	#region DummyDocumentSupporter

	public class DummyDocumentSupporter : DocumentSupporter
	{
		public DummyDocumentSupporter(DummyWithWorkflow dummy)
		 : base(dummy)
		{
			this.dummy = dummy;
		}
		readonly DummyWithWorkflow dummy;

		public override string TransportMode => dummy.Z0_Code;

		public override BusinessContext BusinessContext => throw new NotImplementedException();

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => throw new NotImplementedException();
		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => throw new NotImplementedException();
		protected override Core.Constants.DataContext[] GetSupportedDataContexts() => throw new NotImplementedException();
	}

	#endregion

	#region DummyWithWorkflowCollection

	public class DummyWithWorkflowCollection : ActiveBusinessObjectCollection<DummyWithWorkflow>
	{
		public DummyWithWorkflowCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyWithWorkflowCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public DummyWithWorkflowCollection(BusinessObject master)
			: base(master)
		{
		}

		public DummyWithWorkflowCollection(BusinessObject master, ZQuery filter)
			: base(master, filter)
		{
		}

		public DummyWithWorkflowCollection(BusinessObjectFactory factory, BusinessObject master)
			: base(factory, master)
		{
		}

		public DummyWithWorkflowCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter)
			: base(factory, master, filter)
		{
		}

		protected internal DummyWithWorkflowCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter, SchemaColumn relationshipColumn)
			: base(factory, master, filter, relationshipColumn)
		{
		}

		protected internal DummyWithWorkflowCollection(BusinessObject master, Type pivotObjectType, ZQuery filter)
			: base(master, pivotObjectType, filter)
		{
		}

		protected internal DummyWithWorkflowCollection(BusinessObject master, Type pivotObjectType)
			: base(master, pivotObjectType)
		{
		}

		protected internal DummyWithWorkflowCollection(BusinessObject master, Type pivotObjectType, ZQuery filter, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements)
			: base(master, pivotObjectType, filter, pivotTableFKToMaster, pivotTableFKToElements)
		{
		}

		public DummyWithWorkflowCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}

	#endregion

	#region DummyWithWorkflowProcessTaskTemplateUpdatable

	public class DummyWithWorkflowProcessTaskTemplateUpdatable : DummyWithWorkflow, IProcessTaskTemplateUpdatable
	{
		public DummyWithWorkflowProcessTaskTemplateUpdatable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Action<IProcessTaskTemplate> ProcessTaskTemplateUpdateImplementation { get; set; }
		void IProcessTaskTemplateUpdatable.Update(IProcessTaskTemplate taskTemplate)
		{
			if (ProcessTaskTemplateUpdateImplementation != null)
			{
				ProcessTaskTemplateUpdateImplementation(taskTemplate);
			}
		}
	}

	#endregion

	#region DummyWithWorkflowAndJobNumberForWorkflow

	public class DummyWithWorkflowAndJobNumberForWorkflow : DummyWithWorkflow, IJobNumberForWorkflow
	{
		public DummyWithWorkflowAndJobNumberForWorkflow(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		string IJobNumberForWorkflow.JobNumber
		{
			get { return jobNumberForWorkflow; }
		}

		string jobNumberForWorkflow;

		public void SetJobNumberForWorkflow(string jobNum)
		{
			jobNumberForWorkflow = jobNum;
		}
	}

	#endregion

	#region DummyProcessTask
	public class DummyProcessTask : ProcessTask, IDummyProcessTask
	{
		public DummyProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			parentControllerID = DummyControllerIDs.Dummy;
		}

		public override ControllerID ParentControllerID
		{
			get { return parentControllerID; }
		}
		ControllerID parentControllerID;

		public void SetParentControllerID(ControllerID controllerID)
		{
			this.parentControllerID = controllerID;
		}

		protected internal override Type ParentType
		{
			get { return OverriddenParentTypeForTest ?? DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride ?? typeof(DummyWithWorkflow); }
		}

		public Type OverriddenParentTypeForTest { get; set; }

		public override ZString P9_GS_NKAssignedStaffMember
		{
			get => base.P9_GS_NKAssignedStaffMember;
			set
			{
				NumberOfTimesStaffMemberCalled++;

				base.P9_GS_NKAssignedStaffMember = value;
			}
		}

		public int NumberOfTimesStaffMemberCalled { get; set; }

		public bool CanChangeStatus = true;

		internal override bool DoStatusChangeRespondersAllowStatusChange(ZString status)
		{
			return CanChangeStatus && base.DoStatusChangeRespondersAllowStatusChange(status);
		}
	}

	public class DummyProcessTaskLoadStrategy : IDummyProcessTaskLoadStrategy
	{
		public static LazyOverridable<Action<WorkflowDescriptor, ZDBOnlySubQuery>> AdditionalParentFiltersAdder { get; } = new LazyOverridable<Action<WorkflowDescriptor, ZDBOnlySubQuery>>(() => (_, x_) => { });
		public static LazyOverridable<Type> OverriddenTypeForLoad { get; } = new LazyOverridable<Type>(() => typeof(DummyProcessTask));

		void IProcessTaskLoadStrategy.AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			AdditionalParentFiltersAdder.Value(workflowDescriptor, subQuery);
		}

		Type IProcessTaskLoadStrategy.GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			return OverriddenTypeForLoad.Value;
		}
	}

	#endregion

	#region ProcessTaskForTest

	public class ProcessTaskForTest : ProcessTask
	{
		public ProcessTaskForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void OnSetActualDateCore(ZDateTimeOffset value)
		{
			base.OnSetActualDateCore(value);
			P9_ActualDateOverriddenSetterCalled = true;
		}

		public bool P9_ActualDateOverriddenSetterCalled;

		protected override string WorkflowTypeCore
		{
			get { return "SHP"; }
		}

		public string StatusLogFormat { get; set; }
	}

	#endregion

	#region ProcessTaskForTestToTestParentCodeDescription

	public class ProcessTaskForTestToTestParentCodeDescription : ProcessTask
	{
		public ProcessTaskForTestToTestParentCodeDescription(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected internal override Type ParentType
		{
			get
			{
				return typeof(DummytWithWorkflowWithoutCodeDescription);
			}
		}
	}

	#endregion

	#region DummytWithWorkflowWithoutCodeDescription

	public class DummytWithWorkflowWithoutCodeDescription : EnterpriseBusinessObject, IWorkflowProvider
	{
		public DummytWithWorkflowWithoutCodeDescription(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IWorkflowProvider

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return "DUM"; }
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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (fWorkflowItems == null)
				{
					fWorkflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection(this));
					RegisterEditableChildObject(fWorkflowItems);
				}
				return fWorkflowItems;
			}
		}
		ProcessTaskCollection fWorkflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return null;
		}

		#endregion

		public override SchemaGuidColumn PKSchemaColumn
		{
			get { return DummyBizoSchema.PK; }
		}

		#region Schema

		public abstract class Schema
		{
			public const string TableName = "DummyBizo";
		}

		#endregion
	}

	#endregion

	#region Dummy With Custom Fields

	[UserDefinedValues]
	public class DummyWithCustomFields : DummyWithWorkflow, ICustomFieldProvider
	{
		public DummyWithCustomFields(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		CustomBusinessObject customBizo;

		public virtual CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			if (customBizo == null || shouldRefresh)
			{
				var customPropertyCollection = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBizo = new CustomBusinessObject(Factory, this, customPropertyCollection);
			}
			return customBizo;
		}
	}

	#endregion

	#region DummyWithWorkflowSpecifiedCompanyPK

	public class DummyWithWorkflowSpecifiedCompanyPK : DummyEnterpriseBusinessObject, IWorkflowProviderTemplateCriteria
	{
		public DummyWithWorkflowSpecifiedCompanyPK(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void SetCompany(GlbCompany company)
		{
			Company = company;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
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
				if (fWorkflowItems == null)
				{
					fWorkflowItems = this.GetOrCreateProcessTaskCollection(() => new DummyProcessTaskCollection(this));
					RegisterEditableChildObject(fWorkflowItems);
				}
				return fWorkflowItems;
			}
		}
		ProcessTaskCollection fWorkflowItems;

		GlbCompany Company { get; set; }

		public ZString WorkflowType
		{
			get { return "DM1"; }
		}

		ZGuid IWorkflowProviderTemplateCriteria.CompanyPK
		{
			get { return Company != null ? Company.PK : ZGuid.Empty; }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}
	}

	#endregion

	#region DummyWithWorkflowAndUniversalXmlManagement

	public class DummyWithWorkflowAndUniversalXmlManagement : DummyWithWorkflow
	{
		public DummyWithWorkflowAndUniversalXmlManagement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool ManagesShipments { get; set; } = true;
		public bool ManagesEvents { get; set; } = true;
		public bool ManagesTransactions { get; set; } = true;
		public bool ManagesSchedules { get; set; } = true;
		public bool ManagesActivities { get; set; } = true;
	}

	#endregion

	#region ExampleQueuedLog

	public class QueuedLogForTesting : IQueuedLog
	{
		public QueuedLogForTesting(IQueuedLog log)
		{
			Factory = log.Factory;
			PK = log.PK;
			SJ_ParentTableCode = log.SJ_ParentTableCode;
			SJ_ParentID = log.SJ_ParentID;
			SJ_TargetID = log.SJ_TargetID;
			SJ_SE_NKEvent = log.SJ_SE_NKEvent;
			SJ_EventTime = log.SJ_EventTime;
			SJ_EventTimeUtc = log.SJ_EventTimeUtc;
			SJ_Reference = log.SJ_Reference;
			SJ_PostedTimeUtc = log.SJ_PostedTimeUtc;
			SJ_GS_NKUser = log.SJ_GS_NKUser;
			SJ_Status = log.SJ_Status;
		}

		public QueuedLogForTesting(BusinessObjectFactory factory)
		{
			Factory = factory;
			SJ_GS_NKUser = GlbStaff.CurrentUser.GS_Code;
			SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			SJ_GB_NKBranch = GlbBranch.CurrentBranch?.GB_Code ?? ZString.Empty;
			SJ_GE_NKDepartment = GlbDepartment.CurrentDepartment?.GE_Code ?? ZString.Empty;
			SJ_Reference = ZString.Format("Test|{0}|{1}|{2}|{0}|{1}||", SJ_GB_NKBranch, SJ_GE_NKDepartment, GlbStaff.GetCurrentUser(factory)?.GS_Code);
		}

		public QueuedLogForTesting(IStmALogParent parent, ZString eventCode, ZDateTimeOffset? eventTimeOffset)
			: this(parent, eventCode, null, eventTimeOffset)
		{
		}
		public QueuedLogForTesting(IStmALogParent parent, ZString eventCode, ZString? reference = null, ZDateTimeOffset? eventTimeOffset = null)
		{
			PK = ZGuid.NewZGuid();
			Factory = parent.LogsFactory;
			SJ_ParentTableCode = ((BusinessObject)parent).TablePrefix;
			SJ_ParentID = parent.LogsParentPK;
			SJ_TargetID = parent.LogsParentPK;
			SJ_SE_NKEvent = eventCode;
			SJ_EventTime = eventTimeOffset.HasValue ? eventTimeOffset.Value.ToZDateTime() : ZDateTime.Now;
			SJ_EventTimeUtc = eventTimeOffset.HasValue ? eventTimeOffset.Value.ToUtcZDateTime() : ZDateTime.UtcNow;
			SJ_Reference = reference ?? ZString.Empty;
			SJ_PostedTimeUtc = ZDateTime.UtcNow;
			SJ_GS_NKUser = GlbStaff.CurrentUser?.GS_Code ?? ZString.Empty;
			SJ_Status = "QUE";
		}

		public QueuedLogForTesting(StmALog triggerLog, ProcessTask trigger)
		{
			Factory = trigger.Factory;
			PK = ZGuid.NewZGuid();

			SJ_ALogReference = triggerLog.PK;
			SJ_EventTime = triggerLog.SL_EventTime;
			SJ_EventTimeUtc = triggerLog.SL_EventTimeUtc;
			SJ_GS_NKUser = triggerLog.SL_GS_NKUser;
			SJ_IsEstimate = triggerLog.SL_IsEstimate;
			SJ_ParentID = trigger.PK;
			SJ_ParentTableCode = trigger.TablePrefix;
			SJ_Reference = triggerLog.SL_Reference;
			SJ_SE_NKEvent = triggerLog.SL_SE_NKEvent;
		}

		#region IQueuedLog Members

		public BusinessObjectFactory Factory { get; set; }
		public bool IsRetry => SJ_RetryCount > 1;
		public bool IsFieldChange { get; set; }
		public ZGuid PK { get; set; }
		public ZGuid SJ_ALogReference { get; set; }
		public ZDateTime SJ_EventTime { get; set; }
		public ZDateTime SJ_EventTimeUtc { get; set; }
		public ZString SJ_GS_NKUser { get; set; }
		public ZString SJ_GE_NKDepartment { get; }
		public ZString SJ_GB_NKBranch { get; set; }
		public ZString StaffCode => SJ_GS_NKUser;
		public ZString DepartmentCode => SJ_GE_NKDepartment;
		public ZString BranchCode => SJ_GB_NKBranch;
		public ZString CompanyCode => ZString.Empty;
		public ZBool SJ_IsEstimate { get; set; }
		public ZGuid SJ_ParentID { get; set; }
		public ZString SJ_ParentTableCode { get; set; }
		public ZString SJ_Reference { get; set; }
		public ZString SJ_SE_NKEvent { get; set; }
		public ZGuid SJ_TargetID { get; set; }
		public ZBool SJ_IsDelayFired { get; set; }
		public ZDateTime SJ_PostedTimeUtc { get; set; }
		public ZString SJ_Status { get; set; }
		public ZByte SJ_RetryCount { get; set; }
		public IEnumerable<IStmChangeLog> ChangeLogs
		{
			get => changeLogs ?? Enumerable.Empty<IStmChangeLog>();
			set => changeLogs = value;
		}

		ZGuid IWorkflowTriggerSource.ParentID => throw new NotImplementedException();

		ZDateTime IWorkflowTriggerSource.EventTime => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.SourceType => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.Reference => SJ_Reference;

		ZBool IWorkflowTriggerSource.IsEstimate => throw new NotImplementedException();

		ZDateTime IWorkflowTriggerSource.PostedTimeUtc => throw new NotImplementedException();

		ZDateTimeOffset IWorkflowTriggerSource.EventTimeOffset => StmALog.ToDateTimeOffset(Factory, SJ_EventTime, SJ_EventTimeUtc, SJ_GB_NKBranch);

		ZString IWorkflowTriggerSource.FriendlyTableName => throw new NotImplementedException();

		ZGuid IIdentified.Identifier => throw new NotImplementedException();

		IPropagationSettings IWorkflowTriggerSource.PropagationSettings => throw new NotImplementedException();

		IEnumerable<IStmChangeLog> changeLogs;

		ZDateTime IWorkflowTriggerSource.EventTimeUtc => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.Source => throw new NotImplementedException();

		ZBool IWorkflowTriggerSource.IsCancelled => throw new NotImplementedException();

		ZString IEventUserContextSource.UserCode => SJ_GS_NKUser;
		#endregion
	}

	#endregion

	#region CompanyTestProvider

	public class CompanyTestProvider
	{
		public CompanyTestProvider(BusinessObjectFactory factory, int number)
		{
			companies = Enumerable.Range(0, number).Select(_ => new CompanyGrabBag(factory)).ToList();
		}

		readonly List<CompanyGrabBag> companies;

		public IEnumerable<GlbCompany> Companies => companies.Select(c => c.Company);

		public class CompanyGrabBag
		{
			public CompanyGrabBag(BusinessObjectFactory factory)
			{
				Company = factory.NewWithValidTestData<GlbCompany>();
				Branch = factory.NewWithValidTestData<GlbBranch>();
				Branch.GB_GC = Company.PK;
			}

			public GlbBranch Branch { get; }
			public GlbCompany Company { get; }
		}

		public CompanyGrabBag this[int index]
		{
			get => companies[index];
		}

		public IDisposable WithCompany(int number)
		{
			return Env.SetTemporaryUserContext(Env.Instance.CurrentUserPK, companies[number].Branch.PK.ToGuid(), Env.CurrentDepartmentPK);
		}

		public void ForAllCompanies(Action<CompanyGrabBag> action)
		{
			for (int i = 0; i < companies.Count; i++)
			{
				using (WithCompany(i))
				{
					action(companies[i]);
				}
			}
		}

		public IWorkflowInformationProvider InformationProvider => new WorkflowInformationProvider(companies.Select(s => s.Company.PK).ToArray());

		class WorkflowInformationProvider : IWorkflowInformationProvider
		{
			public WorkflowInformationProvider(IEnumerable<ZGuid> companies)
			{
				Companies = companies;
			}

			public ZString Destination => ZString.Empty;

			public ZString Origin => ZString.Empty;

			public TrackingConstants.BusinessContext BusinessContext => default(TrackingConstants.BusinessContext);

			public IEnumerable<ZGuid> Companies { get; }
		}
	}

	#endregion

	public static class ProcessTaskCollectionTestExtensions
	{
		public static bool IsCondition2Met(this ProcessTaskCollection collection, string condition, string value)
		{
			var task = collection.Factory.New<ProcessTask>();
			task.TemplateConditions.TemplateCondition2 = condition;
			task.TemplateConditions.TemplateCondition2Value = value;
			return collection.IsCondition2Met(task);
		}
	}
}

#endif

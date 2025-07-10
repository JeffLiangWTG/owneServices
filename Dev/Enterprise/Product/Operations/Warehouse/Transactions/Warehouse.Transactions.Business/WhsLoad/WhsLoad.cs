using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[CodeProperty(WhsLoadSchema.Constants.WLO_JobID)]
	public class WhsLoad : AutoWhsLoad, IDocumentSupportable, IDocManagerSupport, IEDocsProvider, INumberFountainConsumer, IWorkflowProvider, ITaskPlanningJob
	{
		public WhsLoad(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region WLO_TaskPlanningStatus

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_TaskPlanningStatus
		{
			get => base.WLO_TaskPlanningStatus;
			set => base.WLO_TaskPlanningStatus = value;
		}

		#endregion

		#region WLO_CompleteTime

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTimeOffset WLO_CompleteTime
		{
			get => base.WLO_CompleteTime;
			set => base.WLO_CompleteTime = value;
		}

		#endregion

		#region WLO_CutoffTime

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTimeOffset WLO_CutoffTime
		{
			get => base.WLO_CutoffTime;
			set => base.WLO_CutoffTime = value;
		}

		#endregion

		#region WLO_DriversLicense

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_DriversLicense
		{
			get => base.WLO_DriversLicense;
			set => base.WLO_DriversLicense = value;
		}

		#endregion

		#region WLO_DriversName

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_DriversName
		{
			get => base.WLO_DriversName;
			set => base.WLO_DriversName = value;
		}

		#endregion

		#region WLO_ExpectedVolumeCapacity

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDecimal WLO_ExpectedVolumeCapacity
		{
			get => base.WLO_ExpectedVolumeCapacity;
			set => base.WLO_ExpectedVolumeCapacity = value;
		}

		#endregion

		#region WLO_ExpectedVolumeCapacityUnit

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_ExpectedVolumeCapacityUnit
		{
			get => base.WLO_ExpectedVolumeCapacityUnit;
			set => base.WLO_ExpectedVolumeCapacityUnit = value;
		}

		#endregion

		#region WLO_ExpectedWeightCapacity

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDecimal WLO_ExpectedWeightCapacity
		{
			get => base.WLO_ExpectedWeightCapacity;
			set => base.WLO_ExpectedWeightCapacity = value;
		}

		#endregion

		#region WLO_ExpectedWeightCapacityUnit

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_ExpectedWeightCapacityUnit
		{
			get => base.WLO_ExpectedWeightCapacityUnit;
			set => base.WLO_ExpectedWeightCapacityUnit = value;
		}

		#endregion

		#region WLO_GateOutTime

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTimeOffset WLO_GateOutTime
		{
			get => base.WLO_GateOutTime;
			set => base.WLO_GateOutTime = value;
		}

		#endregion

		#region WLO_IsHeld

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZBool WLO_IsHeld
		{
			get => base.WLO_IsHeld;
			set => base.WLO_IsHeld = value;
		}

		#endregion

		#region WLO_JobID

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_JobID
		{
			get => base.WLO_JobID;
			set => base.WLO_JobID = value;
		}

		#endregion

		#region WLO_OH_TransportCompany

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZGuid WLO_OH_TransportCompany
		{
			get => base.WLO_OH_TransportCompany;
			set => base.WLO_OH_TransportCompany = value;
		}

		#endregion

		#region WLO_PL_NKCarrierServiceLevel

		[ReadOnly(true)]
		public override ZString WLO_PL_NKCarrierServiceLevel
		{
			get => base.WLO_PL_NKCarrierServiceLevel;
			set => base.WLO_PL_NKCarrierServiceLevel = value;
		}

		#endregion

		#region WLO_RQ_TransportationUnit

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZGuid WLO_RQ_TransportationUnit
		{
			get => base.WLO_RQ_TransportationUnit;
			set => base.WLO_RQ_TransportationUnit = value;
		}

		#endregion

		#region WLO_ScheduledPickUpTime

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTimeOffset WLO_ScheduledPickUpTime
		{
			get => base.WLO_ScheduledPickUpTime;
			set => base.WLO_ScheduledPickUpTime = value;
		}

		#endregion

		#region WLO_SealNumber

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_SealNumber
		{
			get => base.WLO_SealNumber;
			set => base.WLO_SealNumber = value;
		}

		#endregion

		#region WLO_StartTime

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTimeOffset WLO_StartTime
		{
			get => base.WLO_StartTime;
			set => base.WLO_StartTime = value;
		}

		#endregion

		#region WLO_TransportationUnitNumber

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WLO_TransportationUnitNumber
		{
			get => base.WLO_TransportationUnitNumber;
			set => base.WLO_TransportationUnitNumber = value;
		}

		#endregion

		#region WLO_WL_PlannedDockDoor

		public WhsLocation PlannedDockDoor => Factory.Load<WhsLocation>(WLO_WL_PlannedDockDoor);

		[RelatedBusinessObject(nameof(PlannedDockDoor))]
		[ReadOnly(true)]
		public override ZGuid WLO_WL_PlannedDockDoor
		{
			get => base.WLO_WL_PlannedDockDoor;
			set => base.WLO_WL_PlannedDockDoor = value;
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
			=> WLO_JobID.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, WLO_JobID);

		ZString HumanReadableNameWithoutID => Res.GetString("fa8d32f7-48fb-4364-820e-dcec5a5db253", "Load Planning");

		#endregion

		#region CarrierServiceLevel

		public override OrgCarrierServiceLevel CarrierServiceLevel
			=> TransportCompany?.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().FirstOrDefault(s => s.PL_Code.EqualsIgnoringCase(WLO_PL_NKCarrierServiceLevel));

		#endregion

		#region eDocs

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new WhsLoadDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Enterprise.Core.Constants.DocManagerCodes.WarehouseLoad));
		DocManagerInfo docManagerInfo;

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
				return types;
			}
		}

		#endregion

		// interfaces

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WLO_JobID;
			set => WLO_JobID = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WarehouseLoadID;

		#endregion

		#region IWorkflowProvider

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsLoadProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.WhsLoadWorkflowDescriptorCode;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var selectionCriteria = new ColumnValueRanker();
			if (PlannedDockDoor != null)
			{
				selectionCriteria.Add(ProcessTaskTemplateSchema.P0_WW, PlannedDockDoor.WLV_WW_Whs, ZGuid.Empty);
			}
			return selectionCriteria;
		}

		#endregion

		#region ITaskPlanningJob

		ZGuid ITaskPlanningJob.PK => PK;

		ZGuid ITaskPlanningJob.WarehousePK => PlannedDockDoor.WLV_WW_Whs;

		ZString ITaskPlanningJob.TaskPlanningStatus
		{
			get => WLO_TaskPlanningStatus;
			set => WLO_TaskPlanningStatus = value;
		}

		string ITaskPlanningJob.JobID => WLO_JobID;

		ZString ITaskPlanningJob.HumanReadableNameWithoutID => HumanReadableNameWithoutID;

		bool ITaskPlanningJob.IsFinalisedOrCancelled => WLO_GateOutTime.IsValid;

		void ITaskPlanningJob.ClearFKForProcessTasks(ISet<ZGuid> processTaskPKs) { }

		string ITaskPlanningJob.SpecialCannotUpdateTaskPlanningStatusReason => WLO_GateOutTime.IsValid ? Res.GetString("f030ce20-1ed7-411b-aec1-82c5f6569fbc", "Cannot change Task Planning Status as the {0} is gate out.", HumanReadableNameWithoutID) : string.Empty;

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			WorkflowItems.RemoveAndDeleteAll();
		}

		#endregion

		//

		#region Test
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			// delete Location View object and replace with a WhsLocation Row 
			// to force the factory to save the Location first
			var location = PlannedDockDoor
				?? Factory.NewWithValidTestData<WhsLocation>();

			if (!location.IsInDatabase)
			{
				var row = location.WLV_WR;
				var area = location.WLV_WA_PickingArea;
				var locationType = location.WLV_WLT_LocationType;

				location.Delete();

				var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
				var newLocation = rowFactory.New(ZArchitecture.Schema.WhsLocationSchema.Constants.TableName);

				var locationPK = System.Guid.NewGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.PK] = locationPK;
				newLocation.Table.Rows.Add(newLocation);

				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WR] = row.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WA_PickingArea] = area.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WA_PutawayArea] = area.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_WLT_LocationType] = locationType.ToGuid();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_Column] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_Level] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_Tray] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_PutawayPathSequence] = 1;
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemCreateTimeUtc] = ZDateTime.UtcNow.SqlFormat.ToString();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemLastEditTimeUtc] = ZDateTime.UtcNow.SqlFormat.ToString();
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemCreateUser] = 'E';
				newLocation[ZArchitecture.Schema.WhsLocationSchema.Constants.WL_SystemLastEditUser] = 'E';
				WLO_WL_PlannedDockDoor = locationPK;
			}
		}
#endif
		#endregion
	}
}

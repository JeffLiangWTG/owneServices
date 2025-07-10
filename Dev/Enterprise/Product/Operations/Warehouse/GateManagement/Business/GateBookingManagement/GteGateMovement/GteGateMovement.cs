using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[UniversalDataContext(DataContextType.GateMovement)]
	public class GteGateMovement : AutoGteGateMovement, IGteGateMovement, IWorkflowProvider
	{
		public GteGateMovement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region GVM_GBM_Booking

		public GteGateMovementBooking GateMovementBooking => Factory.Load<GteGateMovementBooking>(GGM_GBM_MovementBooking);

		[RelatedBusinessObject("GateMovementBooking")]
		public override ZGuid GGM_GBM_MovementBooking
		{
			get => base.GGM_GBM_MovementBooking;
			set => base.GGM_GBM_MovementBooking = value;
		}

		#endregion

		#region GVM_GBM_Booking

		public GteVehicleMovement VehicleMovement => Factory.Load<GteVehicleMovement>(GGM_GVM_VehicleMovement);

		[RelatedBusinessObject("VehicleMovement")]
		public override ZGuid GGM_GVM_VehicleMovement
		{
			get => base.GGM_GVM_VehicleMovement;
			set => base.GGM_GVM_VehicleMovement = value;
		}

		#endregion

		#region GGM_WL_Dock

		public WhsLocation Dock => Factory.Load<WhsLocation>(GGM_WL_Dock);

		[RelatedBusinessObject("Dock")]
		public override ZGuid GGM_WL_Dock
		{
			get => base.GGM_WL_Dock;
			set => base.GGM_WL_Dock = value;
		}

		#endregion

		#region IWorkflowProvider Members

		[ChildEditable(true)]
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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GteGateMovementProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.GteGateMovementWorkflowDescriptorCode;

		public ZString WarehouseType => GateMovementBooking?.Booking?.Facility?.WW_WarehouseType ?? ZString.Empty;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			if (GateMovementBooking?.Booking != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_WW, GateMovementBooking.Booking.GBK_WW_Facility, ZGuid.Empty);
			}
			if (GateMovementBooking?.Booking?.Facility != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, WarehouseType, ZString.Empty);
			}
			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("9b81f9f3-2867-461c-9d45-f623ac614b15", "Gate Movement");
	}
}

using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[UniversalDataContext(DataContextType.GateVehicleMovement)]
	public class GteVehicleMovement : AutoGteVehicleMovement, IGteVehicleMovement, IWorkflowProvider
	{
		public GteVehicleMovement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region RelatedCollections

		[ChildEditable]
		public GteVehicleEntryCollection VehicleEntries
		{
			get
			{
				if (vehicleEntries == null)
				{
					vehicleEntries = new GteVehicleEntryCollection(this);
					RegisterEditableChildObject(vehicleEntries);
				}

				return vehicleEntries;
			}
		}
		GteVehicleEntryCollection vehicleEntries;

		[ChildEditable]
		public GteGateMovementCollection GateMovements
		{
			get
			{
				if (gateMovements == null)
				{
					gateMovements = new GteGateMovementCollection(this);
					RegisterEditableChildObject(gateMovements);
				}

				return gateMovements;
			}
		}
		GteGateMovementCollection gateMovements;

		#endregion

		public GteVehicleEntry GateInVehicleEntry => VehicleEntries.FirstOrDefault(x => x.GVE_IsIncoming && x.GVE_CancelledReason == ZString.Empty);

		public GteVehicleEntry GateOutVehicleEntry => VehicleEntries.FirstOrDefault(x => x.GVE_IsIncoming == false && x.GVE_CancelledReason == ZString.Empty);

		#region GVM_WL_Location

		public WhsLocation WarehouseLocation => Factory.Load<WhsLocation>(GVM_WL_Location);
		protected override ZString HumanReadableNameCore => Res.GetString("94df729a-d7a8-4b10-883a-22a659dc9068", "Vehicle Movement");

		[RelatedBusinessObject("WarehouseLocation")]
		public override ZGuid GVM_WL_Location
		{
			get => base.GVM_WL_Location;
			set => base.GVM_WL_Location = value;
		}

		#endregion

		#region GVM_GBK_MainBooking

		public GteBooking MainBooking => Factory.Load<GteBooking>(GVM_GBK_MainBooking);

		[RelatedBusinessObject("MainBooking")]
		public override ZGuid GVM_GBK_MainBooking
		{
			get => base.GVM_GBK_MainBooking;
			set => base.GVM_GBK_MainBooking = value;
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			GVM_VehicleRegistration = "REGO";
		}

#endif

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GteVehicleMovementProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.GteVehicleMovementWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();

			if (GateMovements.Any(g => g.WarehouseType == WarehouseTypes.Codes.ContainerYard))
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, WarehouseTypes.Codes.ContainerYard, ZString.Empty);
			}
			else if (GateMovements.Any(g => g.WarehouseType == WarehouseTypes.Codes.FreeTradeZone))
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, WarehouseTypes.Codes.FreeTradeZone, ZString.Empty);
			}
			else if (GateMovements.Any(g => g.WarehouseType == WarehouseTypes.Codes.Product))
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, WarehouseTypes.Codes.Product, ZString.Empty);
			}
			else if (GateMovements.Any(g => g.WarehouseType == WarehouseTypes.Codes.Transit))
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, WarehouseTypes.Codes.Transit, ZString.Empty);
			}

			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

		#endregion

		public string[] FacilityTypes
		{
			get {
				var types = GateMovements
					.Select(ggm => ggm.GateMovementBooking.Booking.Facility.WW_WarehouseType)
					.Distinct()
					.ToArray();

				return ZString.ToStringArray(types);
			}
		}

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
			VehicleEntries.DeleteAll();
			GateMovements.DeleteAll();
			base.Delete();
		}
	}
}

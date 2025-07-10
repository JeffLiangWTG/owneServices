using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteVehicleMovementModule : GteCommonModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GteVehicleMovement;

		protected override ControllerID ControllerID => ControllerIDs.GteVehicleMovement;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GteVehicleMovementFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GteVehicleMovementFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GteVehicleMovementCollection(Factory);

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.GteVehicleMovementWorkflowDescriptorCode;
	}
}

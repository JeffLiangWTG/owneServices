using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteGateMovementModule : GteCommonModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GteGateMovement;

		protected override ControllerID ControllerID => ControllerIDs.GteGateMovement;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GteGateMovementFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GteGateMovementFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GteGateMovementCollection(Factory);

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.GteGateMovementWorkflowDescriptorCode;
	}
}

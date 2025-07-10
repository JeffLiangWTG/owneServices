using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteGateMovementBookingModule : GteCommonModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GteGateMovementBooking;

		protected override ControllerID ControllerID => ControllerIDs.GteGateMovementBooking;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GteGateMovementBookingFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GteGateMovementBookingFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GteGateMovementBookingCollection(Factory);

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.GteGateMovementBookingWorkflowDescriptorCode;
	}
}

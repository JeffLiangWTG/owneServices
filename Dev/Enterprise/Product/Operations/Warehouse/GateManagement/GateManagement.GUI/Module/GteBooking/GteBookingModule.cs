using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteBookingModule : GteCommonModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GteBooking;

		protected override ControllerID ControllerID => ControllerIDs.GteBooking;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GteBookingFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GteBookingFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GteBookingCollection(Factory);

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.GteBookingWorkflowDescriptorCode;
	}
}

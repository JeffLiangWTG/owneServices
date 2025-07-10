using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class MNRWorkOrderModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.MNRWorkOrder;

		protected override ControllerID ControllerID => ControllerIDs.MNRWorkOrder;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new MNRWorkOrderFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new MNRWorkOrderFilterControl((MNRWorkOrderHeaderCollection)GridCollection, (MNRWorkOrderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new MNRWorkOrderHeaderCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.MNRWorkOrderHeaderWorkflowDescriptorCode;
	}
}

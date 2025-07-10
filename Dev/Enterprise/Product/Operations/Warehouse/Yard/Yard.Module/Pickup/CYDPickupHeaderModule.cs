using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDPickupHeaderModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDPickupHeader;

		protected override ControllerID ControllerID => ControllerIDs.CYDPickupHeader;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CYDPickupHeaderFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CYDPickupHeaderFilterControl((CYDPickupHeaderCollection)GridCollection, (CYDPickupHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CYDPickupHeaderCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CYDPickupHeaderWorkflowDescriptorCode;
	}
}

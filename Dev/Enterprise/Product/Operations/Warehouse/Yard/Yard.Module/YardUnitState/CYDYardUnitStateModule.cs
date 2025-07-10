using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDYardUnitStateModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDYardUnitState;

		protected override ControllerID ControllerID => ControllerIDs.CYDYardUnitState;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CYDYardUnitStateFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CYDYardUnitStateFilterControl((CYDYardUnitStateCollection)GridCollection, (CYDYardUnitStateFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CYDYardUnitStateCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CYDYardUnitStateWorkflowDescriptorCode;
	}
}

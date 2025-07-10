using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDTransportationUnitModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDTransportationUnit;

		protected override ControllerID ControllerID => ControllerIDs.CYDTransportationUnit;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CYDTransportationUnitFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CYDTransportationUnitFilterControl((CYDTransportationUnitCollection)GridCollection, (CYDTransportationUnitFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CYDTransportationUnitCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CYDTransportationUnitWorkflowDescriptorCode;
	}
}

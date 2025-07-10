using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDDeliveryHeaderModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDDeliveryHeader;

		protected override ControllerID ControllerID => ControllerIDs.CYDDeliveryHeader;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CYDDeliveryHeaderFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CYDDeliveryHeaderFilterControl((CYDDeliveryHeaderCollection)GridCollection, (CYDDeliveryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CYDDeliveryHeaderCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CYDDeliveryHeaderWorkflowDescriptorCode;
	}
}

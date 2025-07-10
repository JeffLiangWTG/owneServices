using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDAdHocServiceOrderModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDAdHocServiceOrder;

		protected override ControllerID ControllerID => ControllerIDs.CYDAdHocServiceOrder;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CYDAdHocServiceOrderFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CYDAdHocServiceOrderFilterControl((CYDAdHocServiceOrderCollection)GridCollection, (CYDAdHocServiceOrderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CYDAdHocServiceOrderCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CYDAdHocServiceOrderWorkflowDescriptorCode;
	}
}

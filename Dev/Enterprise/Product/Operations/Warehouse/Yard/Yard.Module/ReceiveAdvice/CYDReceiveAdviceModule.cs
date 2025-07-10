using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDReceiveAdviceModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDReceiveAdvice;

		protected override ControllerID ControllerID => ControllerIDs.CYDReceiveAdvice;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CYDReceiveAdviceFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CYDReceiveAdviceFilterControl((CYDReceiveAdviceCollection)GridCollection, (CYDReceiveAdviceFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CYDReceiveAdviceCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CYDReceiveAdviceWorkflowDescriptorCode;
	}
}

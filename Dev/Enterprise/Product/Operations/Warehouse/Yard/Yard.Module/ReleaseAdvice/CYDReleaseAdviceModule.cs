using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDReleaseAdviceModule : CYDModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CYDReleaseAdvice;

		protected override ControllerID ControllerID => ControllerIDs.CYDReleaseAdvice;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CYDReleaseAdviceFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CYDReleaseAdviceFilterControl((CYDReleaseAdviceCollection)GridCollection, (CYDReleaseAdviceFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CYDReleaseAdviceCollection(Factory);

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CYDReleaseAdviceWorkflowDescriptorCode;
	}
}

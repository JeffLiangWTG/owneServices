using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.Module
{
	class ProcessFieldChangeRuleModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ProcessFieldChangeRule;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.ProcessFieldChangeRule);

		protected override IFilterControl GetNewFilterControl() => new ProcessFieldChangeRuleFilterControl(GridCollection, (ProcessFieldChangeRuleFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ProcessFieldChangeRuleCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ProcessFieldChangeRuleFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ProcessFieldChangeRule;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;
	}
}

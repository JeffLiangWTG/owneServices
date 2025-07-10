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
	class ProcessCompanyLinkRuleModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ProcessCompanyLinkRule;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.ProcessCompanyLinkRule);

		protected override IFilterControl GetNewFilterControl() => new ProcessCompanyLinkRuleFilterControl(GridCollection, (ProcessCompanyLinkRuleFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ProcessCompanyLinkRuleCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ProcessCompanyLinkRuleFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ProcessCompanyLinkRule;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;
	}
}

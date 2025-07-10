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
	class ValidationRuleModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Messaging.UniversalValidationRule;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Messaging.UniversalValidationRule);

		protected override IFilterControl GetNewFilterControl() => new ValidationRuleFilterControl(GridCollection, (ValidationRuleFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new UniversalValidationRuleSetCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ValidationRuleFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.UniversalValidationRule;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;
	}
}

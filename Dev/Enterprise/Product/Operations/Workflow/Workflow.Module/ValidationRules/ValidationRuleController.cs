using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.Module
{
	class ValidationRuleController : ZController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Messaging.UniversalValidationRule;
		public override ControllerID ID => ControllerIDs.Messaging.UniversalValidationRule;
		public override Type TypeOfTopLevelBusinessObject => typeof(UniversalValidationRuleSet);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is UniversalValidationRuleSet ruleSet)
			{
				return new ValidationRuleForm(ruleSet);
			}
			else
			{
				throw new InvalidOperationException("Unknown type");
			}
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.UniversalValidationRuleView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.UniversalValidationRuleEdit;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.UniversalValidationRuleNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.UniversalValidationRuleDelete;

		#endregion
	}
}

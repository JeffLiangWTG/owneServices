using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationActionValidation : AutoProcessTemplateValidationActionValidation
	{
		public ProcessTemplateValidationActionValidation(AutoProcessTemplateValidationAction parent) : base(parent)
		{
		}

		new ProcessTemplateValidationAction Parent => (ProcessTemplateValidationAction)base.Parent;

		protected override void CheckP0A_ActionSource()
		{
			base.CheckP0A_ActionSource();
			ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.P0A_ActionSourceInfo);

			var validationRule = Parent.ValidationRule;
			if (validationRule != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.P0A_ActionSourceInfo, validationRule.ProcessTemplateValidationActions, Res.GetString("f91b6c36-058b-4d5b-81f6-2672f0e14793", "Action source must be unique for each validation rule."));
			}
		}
	}
}

using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business;

public sealed class NonPersistentValidationToolParent : NonPersistentBusinessObject, IObsoleteValidation, IValidationToolParent
{
	readonly IWorkflowProvider workflowProvider;

	public NonPersistentValidationToolParent(IWorkflowProvider workflowProvider)
	{
		this.workflowProvider = Argument.NotNull(workflowProvider, nameof(workflowProvider));
	}

	[ChildEditable(true)]
	public ProcessTemplateValidationCollection ProcessTemplateValidations
	{
		get
		{
			if (processTemplateValidations is null)
			{
				processTemplateValidations = new ProcessTemplateValidationCollection(workflowProvider);
				RegisterEditableChildObject(processTemplateValidations);
			}

			return processTemplateValidations;
		}
	}

	ProcessTemplateValidationCollection processTemplateValidations;
}

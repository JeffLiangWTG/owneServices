using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationActionLookups : AutoProcessTemplateValidationActionLookups
	{
		public ProcessTemplateValidationActionLookups(AutoProcessTemplateValidationAction parent) : base(parent)
		{
		}

		new ProcessTemplateValidationAction Parent => (ProcessTemplateValidationAction)base.Parent;

		public CodeDescriptionPairList ActionSourceList => Parent.WorkflowTemplate?.Lookups.ProcessTemplateValidationActionSourceList ?? new CodeDescriptionPairList();
	}
}

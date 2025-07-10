namespace Enterprise.Workflow.Business
{
	public interface IReapplyWorkflowTemplateConfiguration
	{
		bool DelayReapplyTemplatesToServiceTask { get; }
		bool ProcessAndSaveInNewFactory { get; }
	}
}

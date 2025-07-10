namespace Enterprise.Workflow.Business
{
	public class ReapplyWorkflowTemplateInServiceTaskConfiguration : IReapplyWorkflowTemplateConfiguration
	{
		public bool DelayReapplyTemplatesToServiceTask => true;

		public bool ProcessAndSaveInNewFactory => true;
	}
}

namespace Enterprise.Workflow.Business
{
	public class ReapplyWorkflowTemplateInGUIConfiguration : IReapplyWorkflowTemplateConfiguration
	{
		public bool DelayReapplyTemplatesToServiceTask => false;

		public bool ProcessAndSaveInNewFactory => false;
	}
}

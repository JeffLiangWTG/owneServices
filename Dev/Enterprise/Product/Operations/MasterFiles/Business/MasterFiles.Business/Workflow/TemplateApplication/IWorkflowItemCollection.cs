namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowItemCollection : IWorkflowTemplateApplicator, IWorkflowProviderCollection
	{
		TemplateEntityType TemplateEntityType { get; }
	}
}

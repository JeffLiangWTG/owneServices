namespace Enterprise.MasterFiles.Integration
{
	public interface IPropertiesThatAffectWorkflowProvider
	{
		string[] GetPropertiesThatAffectWorkflow(string code);
	}
}

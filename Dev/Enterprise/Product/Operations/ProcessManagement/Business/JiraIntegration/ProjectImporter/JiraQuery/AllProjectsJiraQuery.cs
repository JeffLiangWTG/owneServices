namespace Enterprise.ProcessManagement.Business
{
	public class AllProjectsJiraQuery : ProjectJiraQuery
	{
		public override string QueryString => AssembleQuery(ProjectQueryPart, ProjectExpansionQueryPart);
	}
}

namespace Enterprise.ProcessManagement.Business
{
	public class OneProjectJiraQuery : ProjectJiraQuery
	{
		public OneProjectJiraQuery(string projectKey)
		{
			ProjectKey = GetCorrectedProjectKey(projectKey);
		}

		string ProjectKey { get; }

		public override string QueryString => AssembleQuery(ProjectQueryPart, "/", ProjectKey, ProjectExpansionQueryPart);
	}
}

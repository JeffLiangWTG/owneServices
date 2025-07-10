namespace Enterprise.ProcessManagement.Business
{
	public abstract class ProjectJiraQuery : JiraQuery
	{
		internal static string GetCorrectedProjectKey(string originalKey)
		{
			return string.IsNullOrEmpty(originalKey) ? EmptyProjectQueryPart : originalKey;
		}

		#region SuppressResourceStringsCheckRegion

		protected const string ProjectQueryPart = "project";
		protected const string ProjectExpansionQueryPart = "/?expand=description,lead,issueTypes,url,projectKeys";
		protected const string EmptyProjectQueryPart = "NoProject";

		#endregion
	}
}

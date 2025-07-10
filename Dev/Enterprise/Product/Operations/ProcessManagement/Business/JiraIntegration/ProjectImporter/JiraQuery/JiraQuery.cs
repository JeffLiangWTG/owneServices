namespace Enterprise.ProcessManagement.Business
{
	public abstract class JiraQuery
	{
		public abstract string QueryString { get; }

		protected string AssembleQuery(params string[] queryParts)
		{
			return string.Join("", queryParts);
		}
	}
}

namespace Enterprise.ProcessManagement.Business
{
	public class OneUserJiraQuery : JiraQuery
	{
		public OneUserJiraQuery(string userKey)
		{
			UserKey = userKey;
		}

		string UserKey { get; }

		public override string QueryString => AssembleQuery(UserIDQueryPart, UserKey);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Query string")]
		const string UserIDQueryPart = "user?accountId=";
	}
}

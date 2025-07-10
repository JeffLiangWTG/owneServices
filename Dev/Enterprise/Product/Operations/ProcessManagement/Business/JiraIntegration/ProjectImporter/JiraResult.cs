namespace Enterprise.ProcessManagement.Business
{
	public class JiraResult
	{
		public JiraResult()
		{
		}

		public JiraResult(JiraResponseStatus status, string response)
		{
			Status = status;
			Response = response;
		}

		public JiraResult(JiraResponseStatus status, string response, bool wasDataSaved)
		{
			Status = status;
			Response = response;
			WasDataSaved = wasDataSaved;
		}

		public JiraResponseStatus Status { get; set; }
		public string Response { get; set; }
		public bool WasDataSaved { get; set; }
	}

	public enum JiraResponseStatus
	{
		Success = 0,
		GenericFailure = 1,
		InvalidRequest = 2,
		InvalidRequestWithCustomQuery = 3,
		BadCredentials = 4,
		InsufficientPermissions = 5,
		FailedServerCall = 6,
		Timeout = 7,
		NoProjectClient = 8,
	}
}

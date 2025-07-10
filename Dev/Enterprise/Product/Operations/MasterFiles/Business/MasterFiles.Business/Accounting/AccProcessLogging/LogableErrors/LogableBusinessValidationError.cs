namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	/// <summary>
	/// This class will be used to log any business rule violation error that is identified as part of running our validation related functions or any other functions that check whether certain conditions are met before committing a change (e.g., auto reconciliation, posting). 
	/// This is a tentative implementation. Will be correctly implemented  in a future workitem. Therefore, we will not write any unit tests at the moment.
	/// </summary>
	public class LogableBusinessError : ILogableError
	{
		public LogableBusinessError(string errorCode, string context, string messageTemplate, params object[] messageParameters)
		{
			this.errorCode = errorCode;
			this.messageTemplate = messageTemplate;
			//this.messageParameters = messageParameters;
			(this as ILogableError).ErrorContext = context;
		}

		readonly string errorCode;
		readonly string messageTemplate;
		//readonly object[] messageParameters;

		string ILogableError.Code => errorCode;

		string ILogableError.Message => messageTemplate;

		string ILogableError.ErrorContext { get; set; }

		string ILogableError.ErrorDetails => messageTemplate;
	}
}

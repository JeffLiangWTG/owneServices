namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	/// <summary>
	/// A common representation of the error/exception that we want to log.
	/// </summary>
	public interface ILogableError
	{
		string Code { get; }
		string Message { get; }
		string ErrorContext { get; set; }
		string ErrorDetails { get; }
	}
}

using System;

namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	/// <summary>
	/// This class will be used to log any unhandled system level exception while saving any changes to database. (For example: changes in a draft invoice or posting an invoice)
	/// This is a tentative implementation. Will be correctly implemented  in a future workitem. Therefore, we will not write any unit tests at the moment.
	/// </summary>
	public class LogableUnhandledSystemError : ILogableError
	{
		public LogableUnhandledSystemError(Exception exception)
		{
			this.exception = exception;
		}
		readonly Exception exception;

		string ILogableError.Code => "SYS";

		string ILogableError.Message => FormattableString.Invariant($"{exception.GetType()}: {exception.Message}");

		string ILogableError.ErrorContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		string ILogableError.ErrorDetails => throw new NotImplementedException();
	}
}

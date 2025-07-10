using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	/// <summary>
	/// This class will be used to log any accounting critical validation error while saving any changes to database. This can occur primarily when we try to post an ap invoice or a ap credit note that is got created from a draft invoice.
	/// This is a tentative implementation. Will be correctly implemented  in a future workitem. Therefore, we will not write any unit tests at the moment.
	/// </summary>
	public class LogableAccountingCriticalValidationError : ILogableError
	{
		public LogableAccountingCriticalValidationError(OnSavingCriticalCheckException criticalCheckException)
		{
			this.criticalCheckException = criticalCheckException;
		}
		readonly OnSavingCriticalCheckException criticalCheckException;

		string ILogableError.Code => "CRV";

		string ILogableError.Message => FormattableString.Invariant($"{criticalCheckException.ErrorType}-{criticalCheckException.Message}");

		string ILogableError.ErrorContext { get; set; }

		string ILogableError.ErrorDetails => criticalCheckException.GetFullMessage();
	}
}

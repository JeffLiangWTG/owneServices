using System;

namespace Enterprise.MasterFiles.Business.Accounting.ProcessLogging
{
	public static class ExtensionMethods
	{
		//This function will be called whenever a system exception occurs. This can be inside a catch block
		public static void LogException(this ISupportAccProcessLogging parent, Exception exception)
		{
			var error = new LogableUnhandledSystemError(exception);
			parent.LogError(error);
		}

		//This function will be called whenever a accounting critical validation exception occurs.
		public static void LogAccCriticalValidationException(this ISupportAccProcessLogging parent, OnSavingCriticalCheckException exception)
		{
			var error = new LogableAccountingCriticalValidationError(exception);
			parent.LogError(error);
		}

		//This function can be called from any place where we want to log an error.
		//For example:
		// - This can be in a PreSaveValidation if there are validation error
		// - Validation Error during auto reconciliation process.
		public static void LogError(this ISupportAccProcessLogging parent, ILogableError error)
		{
			if (parent.ShouldLog)
			{
				parent.Logger.Log(parent, error);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.ErrorReporting;

namespace CargoWise.RefDbRepo.Staging.Common.ErrorReporting
{
	public class ErrorReportingWrapper : IErrorReportingWrapper
	{
		readonly ErrorReportingClientWrapper errorReportingClientWrapper;

		StringBuilder keyBaseBuilder;
		StringBuilder descriptionBuilder;
		Dictionary<string, Exception> exceptionDic;

		int maxReportCount;
		bool isExceeded;

		public ErrorReportingWrapper(TimeSpan? timeOut = null)
			: this(null, null, timeOut)
		{
		}

		public ErrorReportingWrapper(IErrorReportBuilder errorReportBuilder, IErrorReportingClientProvider errorReportingClientProvider, TimeSpan? timeOut = null)
		{
			if (errorReportBuilder != null && errorReportingClientProvider != null)
			{
				errorReportingClientWrapper = new ErrorReportingClientWrapper(errorReportingClientProvider, errorReportBuilder, timeOut);
			}
			else
			{
				errorReportingClientWrapper = new ErrorReportingClientWrapper(timeOut);
			}
			keyBaseBuilder = new StringBuilder();
			descriptionBuilder = new StringBuilder();
			exceptionDic = new Dictionary<string, Exception>();
			maxReportCount = Application.ErrorReportMaxCount;
		}

		public void AppendBaseKey(string keyInfo)
		{
			keyBaseBuilder.AppendLine(keyInfo);
		}

		public void AppendDescription(string description)
		{
			if (maxReportCount > 0)
			{
				descriptionBuilder.AppendLine(description);
				maxReportCount--;
			}
			else if (!isExceeded)
			{
				descriptionBuilder.AppendLine("Excess error messages will be truncated, please see the log file for complete information");
				isExceeded = true;
			}
		}

		public void AddException(Exception ex)
		{
			var exKey = ex.StackTrace ?? ex.Message ?? ex.GetType().Name;
			if (!exceptionDic.ContainsKey(exKey))
			{
				exceptionDic.Add(exKey, ex);
			}
		}

		public bool HasErrorToReport => !string.IsNullOrWhiteSpace(descriptionBuilder.ToString()) || exceptionDic.Any();

		public void PostCrashReport()
		{
			var keyBase = keyBaseBuilder.ToString();
			var description = descriptionBuilder.ToString();

			if (exceptionDic.Any())
			{
				foreach (var ex in exceptionDic.Values)
				{
					PostCrashReport(ex, keyBase, description);
				}
			}
			else
			{
				PostCrashReport(new InvalidOperationException(description), keyBase, description);
			}
		}

		public void PostCrashReport(Exception ex, string keyBase, string description)
		{
			errorReportingClientWrapper.PostCrashReport(ex, keyBase, description);
		}

		public void Dispose()
		{
			if (errorReportingClientWrapper != null)
			{
				errorReportingClientWrapper.Dispose();
			}
		}
	}
}

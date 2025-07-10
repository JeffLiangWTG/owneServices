using System;

namespace CargoWise.RefDbRepo.Staging.Common.ErrorReporting
{
	public interface IErrorReportingWrapper : IDisposable
	{
		void PostCrashReport();
		void PostCrashReport(Exception ex, string keyBase, string description);
		void AppendBaseKey(string keyInfo);
		void AppendDescription(string description);
		void AddException(Exception ex);
		bool HasErrorToReport { get; }
	}
}

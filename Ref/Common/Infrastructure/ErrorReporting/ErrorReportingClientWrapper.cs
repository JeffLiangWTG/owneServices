using System;
using System.Net.Http;
using WTG.ErrorReporting;

namespace CargoWise.RefDbRepo.Common.ErrorReporting
{
	public class ErrorReportingClientWrapper : IDisposable
	{
		readonly IErrorReportingClient _errorReportingClient;
		readonly IErrorReportBuilder _errorReportBuilder;

		public ErrorReportingClientWrapper(TimeSpan? timeOut = null)
			: this(new ErrorReportingClientProvider(), new ErrorReportBuilder(), timeOut)
		{
		}

		public ErrorReportingClientWrapper(IErrorReportingClientProvider errorReportingClient, IErrorReportBuilder errorReportBuilder, TimeSpan? timeOut = null)
		{
			Argument.Argument.NotNull(errorReportingClient, nameof(errorReportingClient));
			Argument.Argument.NotNull(errorReportBuilder, nameof(errorReportBuilder));
			_errorReportBuilder = errorReportBuilder;
			_errorReportingClient = errorReportingClient.CreateClient(new Uri(WellKnownServiceUris.Production), timeOut);
		}

		public virtual void PostCrashReport(Exception ex, string keyBase, string description)
		{
			Argument.Argument.NotNull(ex, nameof(ex));

			var errorReport = _errorReportBuilder.BuildErrorReport(ex, keyBase, description, true);
			if (_errorReportingClient == null || errorReport == null)
			{
				return;
			}

			try
			{
				_errorReportingClient.PostCrashReportAsync(errorReport)?.GetAwaiter().GetResult();
			}
			catch (HttpRequestException errorReporterException)
			{
				Console.Error.WriteLine($@"An error occurred during the post of an error report.
Reporting Error message: {errorReporterException}
Actual error being reported: {ex}
");
			}
		}

		public void Dispose()
		{
			_errorReportingClient?.Dispose();
		}
	}
}

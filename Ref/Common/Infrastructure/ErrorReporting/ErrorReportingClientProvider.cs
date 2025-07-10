using System;
using WTG.ErrorReporting;

namespace CargoWise.RefDbRepo.Common.ErrorReporting
{
	class ErrorReportingClientProvider : IErrorReportingClientProvider
	{
		public IErrorReportingClient CreateClient(Uri uri, TimeSpan? timeout = null)
		{
			var errorReportingClient = new ErrorReportingClient(uri);
			if (timeout.HasValue)
			{
				errorReportingClient.Timeout = timeout.Value;
			}
			return errorReportingClient;
		}
	}
}

using System;
using WTG.ErrorReporting;

namespace CargoWise.RefDbRepo.Common.ErrorReporting
{
	public interface IErrorReportingClientProvider
	{
		IErrorReportingClient CreateClient(Uri uri, TimeSpan? timeout = null);
	}
}

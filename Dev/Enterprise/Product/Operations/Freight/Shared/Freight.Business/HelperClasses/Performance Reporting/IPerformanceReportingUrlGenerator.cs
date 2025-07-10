using System;
using System.Threading;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IPerformanceReportingUrlGenerator
	{
		IPerformanceReportingAuthTokenResult GetToken(string correlationId, OrgContact contact = null, CancellationToken ct = default);
		(Uri url, string errorMessage) Generate(string path, IPerformanceReportingAuthToken token, bool isLayoutHidden);
	}
}

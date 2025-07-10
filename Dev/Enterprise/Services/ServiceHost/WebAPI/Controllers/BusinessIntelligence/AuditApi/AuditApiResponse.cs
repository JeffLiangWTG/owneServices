using System.Collections.Generic;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.AuditApi
{
	public class AuditApiResponse
	{
		public string apiVersion;
		public object data;
		public AuditApiErrors? error;
	}

	public struct AuditApiErrors
	{
		public IEnumerable<AuditApiErrorMessage> errors;
	}

	public struct AuditApiErrorMessage
	{
		public string message;
	}
}

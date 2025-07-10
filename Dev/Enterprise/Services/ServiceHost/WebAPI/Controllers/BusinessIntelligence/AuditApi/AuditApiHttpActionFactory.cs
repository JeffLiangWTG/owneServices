using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.AuditApi;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.BusinessIntelligence.Audit
{
	public enum FormatType
	{
		JSON
	}

	public interface IAuditApiHttpActionResultFactory
	{
		IHttpActionResult GetResult(object data, FormatType format, HttpStatusCode status = HttpStatusCode.OK);
		IHttpActionResult GetErrors(IEnumerable<string> error, FormatType format, HttpStatusCode status = HttpStatusCode.BadRequest);
		IHttpActionResult GetError(string error, FormatType format, HttpStatusCode status = HttpStatusCode.BadRequest);
	}

	public class AuditApiHttpActionResultFactory : IAuditApiHttpActionResultFactory
	{
		public IHttpActionResult GetErrors(IEnumerable<string> errors, FormatType format, HttpStatusCode status)
		{
			var response = new AuditApiResponse
			{
				apiVersion = GetApiVersion(),
				error = new AuditApiErrors { errors = errors.Select(e => new AuditApiErrorMessage { message = e }) },
			};
			return GetResult(response, format, status);
		}

		public IHttpActionResult GetError(string error, FormatType format, HttpStatusCode status = HttpStatusCode.BadRequest) => GetErrors(new List<string>() { error }, format, status);

		public IHttpActionResult GetResult(object data, FormatType format, HttpStatusCode status = HttpStatusCode.OK)
		{
			var response = new AuditApiResponse
			{
				apiVersion = GetApiVersion(),
				data = data,
			};
			return GetResult(response, format, status);
		}

		string GetApiVersion()
		{
			return new EnterpriseInformationRetriever().VersionNumber;
		}

		IHttpActionResult GetResult(AuditApiResponse result, FormatType format, HttpStatusCode status)
		{
			switch (format)
			{
				case FormatType.JSON:
					return new JsonActionResult(status, result);
				default:
					throw new NotImplementedException();
			}
		}
	}
}

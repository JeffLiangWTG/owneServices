using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Enterprise.Services.ServiceHost
{
	public sealed class eAdaptorHttpResponseWriter : eAdaptorHttpResponseWriterBase
	{
		public override HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message)
		{
			return request.CreateErrorResponse(statusCode, message);
		}

		public override HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message, Exception exception)
		{
			return request.CreateErrorResponse(statusCode, message, exception);
		}

		public override HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, HttpError error)
		{
			return request.CreateErrorResponse(statusCode, error);
		}
	}
}

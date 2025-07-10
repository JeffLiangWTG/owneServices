using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Enterprise.Integration;

namespace Enterprise.Services.ServiceHost
{
	public abstract class eAdaptorHttpResponseWriterBase : IeAdaptorHttpResponseWriter
	{
		public abstract HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message, Exception exception);

		public abstract HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message);

		public abstract HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, HttpError error);

		public virtual HttpResponseMessage CreateResponse(IeAdaptorRequestProcessorResult data)
		{
			var response = new HttpResponseMessage(data.HttpStatus)
			{
				Content = new StreamContent(data.UniversalResponse.Copy())
			};
			if (data.CustomHeaders != null && data.CustomHeaders.Count > 0)
			{
				foreach (var pair in data.CustomHeaders)
				{
					response.Headers.Add(pair.Key, pair.Value);
				}
			}
			return response;
		}
	}
}

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Enterprise.DataTransfer.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.Messaging.Business;

namespace Enterprise.Services.ServiceHost
{
	public sealed class eAdaptorNextHttpResponseWriter : eAdaptorHttpResponseWriterBase
	{
		public override HttpResponseMessage CreateResponse(IeAdaptorRequestProcessorResult data)
		{
			var httpStatus = data.HttpStatus;
			switch (data.ProcessingStatus)
			{
				case EDIMessage.Status.Discarded:
					httpStatus = HttpStatusCode.BadRequest;
					break;
				case EDIMessage.Status.Rejected:
					httpStatus = HttpStatusCode.BadRequest;
					break;
				case EDIMessage.Status.Warning:
					httpStatus = HttpStatusCode.OK;
					break;
			}

			var response = new HttpResponseMessage(httpStatus)
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

		public override HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message)
		{
			var univeralResponse = UniversalResponseWriter.CreateResponse(UniversalResponseStatus.Error, null, new StringReader(message));
			using (univeralResponse)
			{
				univeralResponse.Position = 0;
				return CreateResponse(new eAdaptorRequestProcessorResult(statusCode, EDIMessage.Status.Error, univeralResponse));
			}
		}

		public override HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message, Exception exception)
		{
			return CreateErrorResponse(request, statusCode, message);
		}

		public override HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, HttpError error)
		{
			return CreateErrorResponse(request, statusCode, error.Message);
		}
	}
}

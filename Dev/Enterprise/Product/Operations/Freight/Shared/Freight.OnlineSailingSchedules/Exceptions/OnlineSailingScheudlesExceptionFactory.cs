using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Freight.OnlineSailingSchedules.ServiceRequestManager;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.OnlineSailingSchedules.Exceptions
{
	public static class OnlineSailingScheudlesExceptionFactory
	{
		public static async Task<Exception> Handle(HttpResponseMessage response)
		{
			if (response.StatusCode == HttpStatusCode.BadRequest)
			{
				return await GetBadRequest(response).ConfigureAwait(false);
			}

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				return new UnauthorizedException(response.ReasonPhrase);
			}

			if (response.StatusCode == HttpStatusCode.NotAcceptable && response.ReasonPhrase.Contains((NoResString)"The content length is more than"))
			{
				return new TooManyRecordsReturnedException();
			}

			// Include:
			// InternalServerError = 500, NotImplemented = 501, BadGateway = 502,
			// ServiceUnavailable = 503, GatewayTimeout = 504, HttpVersionNotSupported = 505
			if (response.StatusCode >= HttpStatusCode.InternalServerError)
			{
				return new InternalServerErrorException(response.Content != null ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : string.Empty);
			}

			try
			{
				response.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException e)
			{
				return e;
			}
			return null;
		}

		static async Task<Exception> GetBadRequest(HttpResponseMessage response)
		{
			var errorResult = new ErrorResult();
			var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			try
			{
				errorResult = JavaScriptSerializerHttpContentSerializer.Deserialize<ErrorResult>(content) ?? new ErrorResult();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				errorResult.Detail = ResString.GetMultilingualString("91E7DAE4-6DAC-4585-A0D8-62338236EF38",
					"{0} happened for {1}", e.Message, content);
			}
			return new BadRequestException(errorResult.ErrorMessageFull, errorResult.ProblemMessages, errorResult.TraceId);
		}
	}
}

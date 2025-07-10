using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.NewService.Extensions;
using Common.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.NewService.Logging
{
	public class ApiLogMiddleWare
	{
		readonly RequestDelegate _next;
		readonly ILog _log;
		int requestID;

		public ApiLogMiddleWare(RequestDelegate next, ILogWrapper logWrapper)
		{
			Argument.NotNull(next, nameof(next));
			Argument.NotNull(logWrapper, nameof(logWrapper));

			_next = next;
			_log = logWrapper.GetLog<ApiLogMiddleWare>();
		}

		public async Task Invoke(HttpContext context)
		{
			context.Request.EnableBuffering();
			var body = GetBodyFromRequest(context.Request);

			var requestLog = CreateApiLogRequest(context, body);
			_log.Info(requestLog);

			await _next.Invoke(context);

			var responseLog = CreateApiLogResponse(context, requestLog.Id);
			_log.Info(responseLog);
		}

		ApiLogRequest CreateApiLogRequest(HttpContext context, string body)
		{
			Argument.NotNull(context, nameof(context));

			requestID++;
			return new ApiLogRequest
			{
				Id = requestID,
				AuthType = context.GetAuthType(),
				UserId = context.GetUserId(),
				Uri = context.Request?.GetEncodedPathAndQuery(),
				Body = body
			};
		}

		static ApiLogResponse CreateApiLogResponse(HttpContext context, long id)
		{
			Argument.NotNull(context, nameof(context));

			return new ApiLogResponse
			{
				Id = id,
				AuthType = context.GetAuthType(),
				UserId = context.GetUserId(),
				Status = context.Response?.StatusCode.ToString(CultureInfo.InvariantCulture)
			};
		}

		string GetBodyFromRequest(HttpRequest request)
		{
			if (request.HasFormContentType && request.Form.Files.Any())
			{
				return "new file uploaded";
			}

			var body = DeserializeBodyForReportRequest(request.Path.ToString(), request.GetBody());
			return body;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "do not break the code if deserialize fails")]
		string DeserializeBodyForReportRequest(string requestPath, string body)
		{
			if (string.IsNullOrEmpty(body) || !requestPath.Contains("/Report", StringComparison.OrdinalIgnoreCase))
			{
				return body;
			}

			try
			{
				var deserializedBody = JsonConvert.DeserializeObject<Tuple<bool, JObject>>(body);
				body = deserializedBody.Item1 ? "update succeeded" : "update failed";
				var durationLog = deserializedBody.Item2.ToObject<UpdaterDurationLog>();
				if (durationLog != null && durationLog.DataSetName != null && durationLog.ClientId != null)
				{
					_log.Info(durationLog);
				}
			}
			catch
			{
				// old clients still send different objects so deserialize may fail, do not break the code even it fails.
			}
			return body;
		}
	}

	public static class ApiLogAppBuilderExtensions
	{
		public static IApplicationBuilder UseApiLog(this IApplicationBuilder app)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}

			return app.UseMiddleware<ApiLogMiddleWare>();
		}
	}
}

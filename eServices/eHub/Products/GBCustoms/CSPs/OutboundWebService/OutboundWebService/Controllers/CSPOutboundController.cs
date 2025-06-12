using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService.Controllers
{
	public class CSPOutboundController : ApiController
	{
		protected internal Func<Guid> InternalGuid = () => Guid.NewGuid();
		protected internal Func<ILog> InternalLog = () => LogManager.GetLogger<CSPOutboundController>();
		protected internal ILog Logger;
		protected internal HttpRequestMessage InternalRequestMessage => Request;
		readonly Guid ID;
		readonly string LogPrefix;

		public CSPOutboundController()
		{
			Logger = InternalLog();
			ID = InternalGuid();
			LogPrefix = $"[Service ID: {ID}] - ";
			Logger.Info($"{LogPrefix}Service Started.");
		}

		[HttpPost]
		public async Task<HttpResponseMessage> ReceiveNotification()
		{
			Logger.Info($"{LogPrefix}Received Outbound Notification.");

			if(!ValidateAuthHeader())
			{
				return HTTPHelper.CreateFailureResponse(Logger, LogPrefix, HttpStatusCode.Unauthorized, $"Not authorized");
			}

			HttpResponseMessage httpResponseMessage;
			string accountName;
			string uri;

			if (!GetAndRemoveHeader("AccountName", out accountName, out httpResponseMessage))
			{
				return httpResponseMessage;
			}

			if (!GetAndRemoveHeader("SendToURI", out uri, out httpResponseMessage))
			{
				return httpResponseMessage;
			}

			try
			{
				httpResponseMessage = await ThreadHandler.SendMessageThreadSafe(new MessageSender(), Logger, LogPrefix, Request, accountName, uri);
				var responseForLog = httpResponseMessage.Content.ReadAsStringAsync().Result ?? string.Empty;
				var responseForLogType = "Body";
				if (string.IsNullOrEmpty(responseForLog))
				{
					responseForLogType = "Headers";
					foreach (var header in httpResponseMessage.Headers)
					{
						responseForLog += Environment.NewLine;
						responseForLog += $"{header.Key} - {string.Join(",", header.Value)}";
					}
				}
				Logger.Debug($"{LogPrefix}Response received: Code[{httpResponseMessage.StatusCode}], Response {responseForLogType}: {responseForLog}");
			}
			catch (Exception ex)
			{
				httpResponseMessage = HTTPHelper.CreateFailureResponse(Logger, LogPrefix, HttpStatusCode.InternalServerError, $"{ex}");
			}

			Logger.Info($"{LogPrefix}Completed.");

			return httpResponseMessage;
		}

		public bool GetAndRemoveHeader(string headerName, out string headerValue, out HttpResponseMessage httpResponseMessage)
		{
			IEnumerable<string> headerList;

			if (Request.Headers.TryGetValues(headerName, out headerList))
			{
				headerValue = headerList.FirstOrDefault();
				if (string.IsNullOrEmpty(headerValue))
				{
					httpResponseMessage = HTTPHelper.CreateFailureResponse(Logger, LogPrefix, HttpStatusCode.InternalServerError, $"Invalid {headerName} header");
					return false;
				}
				Request.Headers.Remove(headerName);
				httpResponseMessage = null;
				return true;
			}
			else
			{
				httpResponseMessage = HTTPHelper.CreateFailureResponse(Logger, LogPrefix, HttpStatusCode.InternalServerError, $"No {headerName} header found");
				headerValue = null;
				return false;
			}
		}

		public bool ValidateAuthHeader()
		{
			string headerValue;

			if (GetAndRemoveHeader(GetSettings("AuthorisationHeaderKey"), out headerValue, out _))
			{
				return (headerValue ?? string.Empty) == GetSettings("AuthorisationHeaderValue");
			}

			return false;
		}

		protected internal virtual string GetSettings(string key)
		{
			var output = ConfigurationManager.AppSettings[key];
			if (string.IsNullOrWhiteSpace(output))
			{
				throw new ConfigurationErrorsException($"No AppSettings could be found for {key}");
			}
			return output;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;

namespace Enterprise.DeniedPartyScreening.Business
{
	public static class HttpDpsWebHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "url")]
		const string RescreenAdvicesUrlParameter = "/api/v4.4/rescreen-advices";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "url")]
		const string MatchDecisionsUrlParameter = "/api/v4.4/match-decisions";
		const string ScreenUrlParameter = "/api/v4.4/screen";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "log string")]
		const string WebErrorKeyMessage = "Web error occurred in DPS Service Task.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "url")]
		const string IdentifyDesyncedEntitiesUrlParameter = "/api/v4.4/identify-desynced-entities";

		public static void ConfirmRescreenAdvices(byte[] requestData)
		{
			Put(requestData, GetDpsUrlRestful(RescreenAdvicesUrlParameter).ToString());
		}

		public static List<DeniedPartyRescreenAdvice> GetRescreenAdvices(byte[] requestData)
		{
			var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-");

			return Post<List<DeniedPartyRescreenAdvice>>(requestData, GetDpsUrlRestful(RescreenAdvicesUrlParameter).ToString() + $"?licenseCode={licenseCode}");
		}

		public static void PostMatchDecisions(byte[] requestData)
		{
			Post<WebResponse>(requestData, GetDpsUrlRestful(MatchDecisionsUrlParameter).ToString());
		}

		public static DpsResponse PostScreenWithFailover(byte[] requestData)
		{
			var webServices = (OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.Value).Cast<DpsWebServiceItem>();
			var aggrExceptions = new List<Exception>();

			var isInternal = ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();

			if (isInternal)
			{
				return Post<DpsResponse>(requestData, webServices.Single(x => x.Role == RoleHelper.Code.Staging).WebServiceUrl.ToString().Trim('/') + ScreenUrlParameter);
			}
			else
			{
				foreach (var service in webServices.Where(x => x.Role != RoleHelper.Code.Staging))
				{
					try
					{
						return Post<DpsResponse>(requestData, service.WebServiceUrl.ToString().Trim('/') + ScreenUrlParameter);
					}
					catch (WebException webEx)
					{
						aggrExceptions.Add(webEx);
					}
				}

				throw new AggregateException(aggrExceptions);
			}
		}

		public static IEnumerable<Guid> PostIdentifyDesyncedEntities(byte[] requestData)
		{
			return Post<IEnumerable<Guid>>(requestData, GetDpsUrlRestful(IdentifyDesyncedEntitiesUrlParameter).ToString());
		}

		static Uri GetDpsUrlRestful(string urlParameter)
		{
			var registryValue = OrganisationsDataRegistry.Instance.DeniedPartyScreeningWebService.Value;
			var urlRows = registryValue.Cast<DpsWebServiceItem>();
			var isInternal = ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();

			return new Uri(urlRows.Single(urlRow => urlRow.Role == (isInternal ? RoleHelper.Code.Staging : RoleHelper.Code.Production)).WebServiceUrl.ToString().Trim('/') + urlParameter);
		}

		static TResponse Post<TResponse>(byte[] requestData, string urlParameter)
		{
			return RetryHandler(() =>
			{
#pragma warning disable SYSLIB0014 // Under net8.0 WebClient.WebClient()' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
				var request = (HttpWebRequest)WebRequest.Create(urlParameter);
#pragma warning restore SYSLIB0014
				request.Method = "POST";
				request.ContentType = "application/json";
				request.ContentLength = requestData.Length;
				AppendLicenceAndAuthorizationHeader(request);

				using (var dataStream = request.GetRequestStream())
				{
					dataStream.Write(requestData, 0, requestData.Length);
				}

				return JsonConvert.DeserializeObject<TResponse>(GetWebResponse(request));
			});
		}

		static bool Put(byte[] requestData, string urlParameter)
		{
			return RetryHandler(() =>
			{
#pragma warning disable SYSLIB0014 // Under net8.0 WebClient.WebClient()' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
				var request = (HttpWebRequest)WebRequest.Create(urlParameter);
#pragma warning restore SYSLIB0014
				request.Method = "PUT";
				request.ContentType = "application/json";
				request.ContentLength = requestData.Length;
				AppendLicenceAndAuthorizationHeader(request);

				using (var dataStream = request.GetRequestStream())
				{
					dataStream.Write(requestData, 0, requestData.Length);

					using (request.GetResponse())
					{
						return true;
					}
				}
			});
		}

		static void AppendLicenceAndAuthorizationHeader(HttpWebRequest request)
		{
			request.Headers.Add("LicenceCode", HMACSHA256Helper.GetComputedLicenceCode(GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-")));
			request.Headers.Add((NoResString)"Authorization", TokenManager.GetInstance(MDMProductCodes.DPS).GetAuthorizationHeaderValue().ToString());
		}

		static string GetWebResponse(WebRequest request)
		{
			using (var webResponse = request.GetResponse())
			using (var dataStream = webResponse.GetResponseStream())
			using (var reader = new StreamReader(dataStream))
			{
				return reader.ReadToEnd();
			}
		}

		static TResponse RetryHandler<TResponse>(Func<TResponse> action)
		{
			var tryCount = 3;
			do
			{
				try
				{
					return action();
				}
				catch (WebException)
				{
					if (--tryCount <= 0)
					{
						throw;
					}

					var delay = TimeSpan.FromSeconds(10);
#if DEBUG
					if (Globals.IsTest)
					{
						delay = TimeSpan.FromSeconds(0.5);
					}
#endif
					Thread.Sleep(delay);
				}
			}
			while (true);
		}

		public static void HandleWebException(WebException exception, ILogger logger, DpsServiceTaskHelper serviceTaskHelper = null )
		{
			if (exception.Response is HttpWebResponse response)
			{
				var descriptionFromServer = string.Empty;

				try
				{
					using (var responseStream = response.GetResponseStream())
					{
						if (responseStream != null)
						{
							using (var responseStreamReader = new StreamReader(responseStream))
							{
								descriptionFromServer = responseStreamReader.ReadToEnd();
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException()) { }

				var statusCode = (int)response.StatusCode;
				if (statusCode >= 400 && statusCode < 500)
				{
					LogAsError(logger, exception, descriptionFromServer, shouldReportOnce: false);
				}
				else if (statusCode >= 500)
				{
					if (response.StatusCode == HttpStatusCode.InternalServerError)
					{
						HandleWebExceptionInternalServerError(logger, exception, descriptionFromServer, serviceTaskHelper);
					}
					else if (response.StatusCode == HttpStatusCode.GatewayTimeout)
					{
						LogAsError(logger, exception, descriptionFromServer, shouldReportOnce: false);
					}
					else
					{
						LogAsWarning(logger, exception, descriptionFromServer, webError: true);
					}
				}
				else
				{
					LogAsWarning(logger, exception, descriptionFromServer, webError: true);
				}
			}
			else
			{
				LogAsWarning(logger, exception, (NoResString)"There was no Http response.", webError: false);
			}
		}

		static void LogAsError(ILogger logger, Exception exception, string message, bool shouldReportOnce)
		{
			if (!string.IsNullOrEmpty(message))
			{
				logger?.Log(LogType.Error, WebErrorKeyMessage + System.Environment.NewLine + message + System.Environment.NewLine, exception);
				if (shouldReportOnce)
				{
					ErrorReporter.ReportOnce(WebErrorKeyMessage, message, exception);
				}
			}
			else
			{
				logger?.Log(LogType.Error, WebErrorKeyMessage, exception);
				if (shouldReportOnce)
				{
					ErrorReporter.ReportOnce(WebErrorKeyMessage, exception);
				}
			}
		}

		static void LogAsWarning(ILogger logger, Exception exception, string message, bool webError)
		{
			if (webError)
			{
				message = string.IsNullOrEmpty(message) ? WebErrorKeyMessage : WebErrorKeyMessage + System.Environment.NewLine + message + System.Environment.NewLine;
			}

			logger?.Log(LogType.Warning, message, exception);
		}

		static void HandleWebExceptionInternalServerError(ILogger logger, Exception exception, string descriptionFromServer, DpsServiceTaskHelper serviceTaskHelper)
		{
			serviceTaskHelper?.IncreaseFailedToRunCount();

			if (serviceTaskHelper != null && serviceTaskHelper.IsLogAsWarning)
			{
				LogAsWarning(logger, exception, descriptionFromServer, webError: true);
			}
			else
			{
				LogAsError(logger, exception, descriptionFromServer, shouldReportOnce: true);
			}
		}
	}
}

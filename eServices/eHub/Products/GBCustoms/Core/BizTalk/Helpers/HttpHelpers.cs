using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers
{
	public static class HttpHelpers
	{
		public static HttpHeader[] CreateRegistrationHeaders(
			string userSystemCode,
			string username,
			string password,
			string badge,
			string eori)
		{
			return HttpHelpers
				.CreateSharedHeaders(userSystemCode, username, password, badge, eori)
				.Concat(new[] { HttpHeader.Create("Accept", "application/vnd.csp.1.0+xml") })
				.ToArray();
		}

		public static HttpHeader[] CreateSubmissionHeaders(string userSystemCode,
			string username,
			string password,
			string badge,
			string eori)
		{
			return HttpHelpers
				.CreateSharedHeaders(userSystemCode, username, password, badge, eori)
				.Concat(new[] { HttpHeader.Create("Accept", "application/vnd.hmrc.2.0+xml") })
				.ToArray();
		}

        public static HttpHeader[] CreateSubmissionHeaders(string userSystemCode,
            string username,
            string password,
            string badge,
            string eori,
            string version)
        {
            return HttpHelpers
                .CreateSharedHeaders(userSystemCode, username, password, badge, eori)
                .Concat(new[] { HttpHeader.Create("Accept", $"application/vnd.hmrc.{version}+xml") })
                .ToArray();
        }

        public static HttpHeader[] CreateSubmissionHeaders(string userSystemCode,
            string username,
            string password,
            string badge,
            string eori,
            string version,
            string account,
            string uri)
        {
            return HttpHelpers
                .CreateSharedHeaders(userSystemCode, username, password, badge, eori)
                .Concat(new[] { HttpHeader.Create("Accept", $"application/vnd.hmrc.{version}+xml") })
                .Concat(new[] { HttpHeader.Create("AccountName", account) })
				.Concat(new[] { HttpHeader.Create("SendToURI", uri) })
				.Concat(new[] { HttpHeader.Create("OutboundAuthorization", "CspOutboundValidation") })
				.ToArray();
        }

        private static IEnumerable<HttpHeader> CreateSharedHeaders(
			string userSystemCode,
			string username,
			string password,
			string badge,
			string eori)
		{
			if (string.IsNullOrEmpty(userSystemCode))
			{
				throw new ArgumentNullException("userSystemCode", "UserSystemCode was not supplied");
			}

			if (string.IsNullOrEmpty(username))
			{
				throw new ArgumentNullException("username", "Username was not supplied");
			}

			if (string.IsNullOrEmpty(password))
			{
				throw new ArgumentNullException("password", "Password was not supplied");
			}

			if (string.IsNullOrEmpty(badge))
			{
				throw new ArgumentNullException("badge", "Badge was not supplied");
			}

			var authorization = string.Format(
				"Basic {0}",
				Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(
					"{0}:{1}",
					username,
					password))));

			var userAgent = string.Format(
				"Vendor=WiseTech Global, Application=eHub, Version=3.0.0.0, Badge={0}, ClientID={1}",
				badge,
				userSystemCode);

			yield return HttpHeader.Create("Authorization", authorization);
			yield return HttpHeader.Create("X-Badge-ID", badge);
			if (!string.IsNullOrEmpty(eori))
			{
				yield return HttpHeader.Create("X-Submitter-Identifier", eori);
			}
			yield return HttpHeader.Create("User-Agent", userAgent);
		}

		public static string CreateRegistrationPayload(CallbackRegistration callbackRegistration)
		{
			if (callbackRegistration == null)
			{
				throw new ArgumentNullException("callbackRegistration");
			}

			return string.Format(
				"<consumer endpointUrl=\"{0}\" authorization=\"{1}\"></consumer>",
				callbackRegistration.Uri,
				callbackRegistration.Authorization);
		}

		public static string ExtractEORI(string credentialKey)
		{
			Regex rg = new Regex(@"\.(.*)\.");
			Match match = rg.Match(credentialKey);
			return match.Groups.Count > 1 ? match.Groups[1].Value : null;
		}

        public static string ExtractClientSystem(string credentialKey)
        {
            var parts = credentialKey.Split('.');
            return parts.Length > 1 ? parts[0] : null;
        }

        public static HttpHeader[] CreateHttpHeaders(params string[] headers)
        {
            List<HttpHeader> httpHeaders = new List<HttpHeader>();

            for (int i = 0; i < headers.GetLength(0); i++)
            {
                httpHeaders.Add(HttpHeader.Create(headers[i], headers[++i]));
            }

            return httpHeaders.ToArray();
        }

		public static void LogHeaders(HttpHeader[] httpHeaders, ILog logger, string logPrefix)
		{
			logger.Trace($"{logPrefix}Headers:");
			foreach(var header in httpHeaders)
			{
				logger.Trace($"{logPrefix}{header.Key} - {header.Value}");
			}
		}

		public static string EncodeGUID(string id)
		{
			try
			{
				var guid = new Guid(id);
				var bytes = guid.ToByteArray();
				return Convert.ToBase64String(bytes);
			}
			catch
			{
				return string.Empty;
			}
		}

		public static void CreateOverridingConfig(HttpRequest request)
		{
			var exceptionConfigs = new List<OverridingExceptionConfig>();
			foreach (var exception in RetryingExceptions)
			{
				foreach (var message in exception.Value)
				{
					var exceptionConfig = new OverridingExceptionConfig
					{
						ExceptionMessage = message,
						ExceptionType = exception.Key,
						ShouldRetry = true
					};
					exceptionConfigs.Add(exceptionConfig);
				}
			}

			request.OverridingExceptionConfigs = exceptionConfigs.ToArray();
		}

		static Dictionary<string, string[]> RetryingExceptions = new Dictionary<string, string[]>()
		{
			{ "Microsoft.XLANGs.Core.XlangSoapException", new string[]{ 
				"(System.Net.Http.HttpRequestException) The underlying connection was closed: An unexpected error occurred on a receive", 
				"(System.Net.Http.HttpRequestException) Unable to connect to the remote server", "(System.Threading.Tasks.TaskCanceledException) A task was canceled"}}
		};
    }
}

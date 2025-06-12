using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;

#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Configuration;
#else
using System.Configuration;
#endif

namespace CargoWise.eServices.Authentication.ServiceClient
{
	public class AuthWebServiceApi : IAuthWebserviceApi
	{
		static AuthWebServiceApi()
		{
			ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
		}

#if NETSTANDARD2_0_OR_GREATER
		public AuthWebServiceApi(IConfiguration configSection)
		{
			if (configSection == null) throw new ArgumentNullException($"{nameof(configSection)} cannot be null");

			CacheDurationOnServerDown = int.TryParse(configSection[nameof(CacheDurationOnServerDown)], out int serverDownduration) ? serverDownduration : DefaultServerDownduration;
			CacheDuration = int.TryParse(configSection[nameof(CacheDuration)], out int duration) ? duration : DefaultCacheDuration;
			CheckInterval = int.TryParse(configSection[nameof(CheckInterval)], out int interval) ? interval : DefaultCheckInterval;
			endPoint = configSection[nameof(EndPoint)];

			if (string.IsNullOrEmpty(endPoint))
			{
				throw new ArgumentNullException($"{nameof(EndPoint)} of {nameof(configSection)} cannot be null");
			}
		}
#else
		public AuthWebServiceApi()
		{
			CacheDurationOnServerDown = int.TryParse(ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.ServiceClient.CacheDurationOnServerDown"], out int serverDownduration) ? serverDownduration : DefaultServerDownduration;
			CacheDuration = int.TryParse(ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.ServiceClient.CacheDuration"], out int duration) ? duration : DefaultCacheDuration;
			CheckInterval = int.TryParse(ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.ServiceClient.CheckInterval"], out int interval) ? interval : DefaultCheckInterval;
		}
#endif

		private const int DefaultServerDownduration = 86400;
		private const int DefaultCacheDuration = 300;
		private const int DefaultCheckInterval = 300;

		public HttpStatusCode LastStatusCode { get; private set; }
		public string LastError { get; private set; }

		public string LastResult => $"Status {LastStatusCode}: {LastError}";

		public int ExistenceCachesCount => existenceCaches.Count;
		public int ValidationCachesCount => validationCaches.Count;

		private string endPoint;
		private static readonly ConcurrentDictionary<string, ResultCache> existenceCaches = new ConcurrentDictionary<string, ResultCache>();
		private static readonly ConcurrentDictionary<string, ResultCache> validationCaches = new ConcurrentDictionary<string, ResultCache>();
		private DateTimeWrapper dateTime;

		private int CacheDurationOnServerDown { get; }
		private int CacheDuration { get; }
		private int CheckInterval { get; }

		public bool CheckSystemExistence(string systemID)
		{
			ResultCache cache;
			if (!existenceCaches.TryGetValue(systemID, out cache))
			{
				var actionRequest = string.Format("\"{0}\"", systemID);
				cache = CacheResult(cache, MakeHttpsRequest("CheckSystemIDExistence", actionRequest));
				existenceCaches[systemID] = cache;
			}
			else
			{
				if (cache.ExpirationTime > DateTimeWrapper.UtcNow || cache.LastCheck.AddSeconds(CheckInterval) > DateTimeWrapper.UtcNow)
				{
					return cache.Result;
				}
				else
				{
					cache.LastCheck = DateTimeWrapper.UtcNow;
					var actionRequest = string.Format("\"{0}\"", systemID);
					var requestResult = MakeHttpsRequest("CheckSystemIDExistence", actionRequest, out var hasExceptionOnRequest);
					if (!hasExceptionOnRequest || cache.ExpirationTime.AddSeconds(CacheDurationOnServerDown - CacheDuration) <= DateTimeWrapper.UtcNow)
					{
						existenceCaches[systemID] = CacheResult(cache, requestResult);
					}
				}
			}
			return cache.Result;
		}

		public bool CheckSystemExistence(string enterpriseCode, string serverCode)
		{
			ResultCache cache;
			if (!existenceCaches.TryGetValue(enterpriseCode + serverCode, out cache))
			{
				var actionRequest = string.Format("[\"{0}\",\"{1}\"]", enterpriseCode, serverCode);
				cache = CacheResult(cache, MakeHttpsRequest("CheckCodeExistence", actionRequest));
				existenceCaches[enterpriseCode + serverCode] = cache;
			}
			else
			{
				if (cache.ExpirationTime > DateTimeWrapper.UtcNow || cache.LastCheck.AddSeconds(CheckInterval) > DateTimeWrapper.UtcNow)
				{
					return cache.Result;
				}
				else
				{
					cache.LastCheck = DateTimeWrapper.UtcNow;
					var actionRequest = string.Format("[\"{0}\",\"{1}\"]", enterpriseCode, serverCode);
					var requestResult = MakeHttpsRequest("CheckCodeExistence", actionRequest, out var hasExceptionOnRequest);
					if (!hasExceptionOnRequest || cache.ExpirationTime.AddSeconds(CacheDurationOnServerDown - CacheDuration) <= DateTimeWrapper.UtcNow)
					{
						existenceCaches[enterpriseCode + serverCode] = CacheResult(cache, requestResult);
					}
				}
			}
			return cache.Result;
		}

		public bool ValidateSystem(string systemID, string password)
		{
			ResultCache cache;
			if (!validationCaches.TryGetValue(systemID, out cache))
			{
				var actionRequest = string.Format("[\"{0}\",\"{1}\"]", systemID, password);
				cache = CacheResult(cache, MakeHttpsRequest("ValidateSystemIDAndPassword", actionRequest), password);
				validationCaches[systemID] = cache;
			}
			else
			{
				if ((cache.ExpirationTime > DateTimeWrapper.UtcNow || cache.LastCheck.AddSeconds(CheckInterval) > DateTimeWrapper.UtcNow) && cache.Password == password)
				{
					return cache.Result;
				}
				else
				{
					cache.LastCheck = DateTimeWrapper.UtcNow;
					var actionRequest = string.Format("[\"{0}\",\"{1}\"]", systemID, password);
					var requestResult = MakeHttpsRequest("ValidateSystemIDAndPassword", actionRequest, out var hasExceptionOnRequest);
					if (!hasExceptionOnRequest || cache.ExpirationTime.AddSeconds(CacheDurationOnServerDown - CacheDuration) <= DateTimeWrapper.UtcNow)
					{
						validationCaches[systemID] = CacheResult(cache, requestResult, password);
					}
				}
			}
			return cache.Result;
		}

		public bool ValidateSystem(string enterpriseCode, string serverCode, string password)
		{
			ResultCache cache;
			if (!validationCaches.TryGetValue(enterpriseCode + serverCode, out cache))
			{
				var actionRequest = string.Format("[\"{0}\",\"{1}\",\"{2}\"]", enterpriseCode, serverCode, password);
				cache = CacheResult(cache, MakeHttpsRequest("ValidateCodeAndPassword", actionRequest), password);
				validationCaches[enterpriseCode + serverCode] = cache;
			}
			else
			{
				if ((cache.ExpirationTime > DateTimeWrapper.UtcNow || cache.LastCheck.AddSeconds(CheckInterval) > DateTimeWrapper.UtcNow) && cache.Password == password)
				{
					return cache.Result;
				}
				else
				{
					cache.LastCheck = DateTimeWrapper.UtcNow;
					var actionRequest = string.Format("[\"{0}\",\"{1}\",\"{2}\"]", enterpriseCode, serverCode, password);
					var requestResult = MakeHttpsRequest("ValidateCodeAndPassword", actionRequest, out var hasExceptionOnRequest);
					if (!hasExceptionOnRequest || cache.ExpirationTime.AddSeconds(CacheDurationOnServerDown - CacheDuration) <= DateTimeWrapper.UtcNow)
					{
						validationCaches[enterpriseCode + serverCode] = CacheResult(cache, requestResult, password);
					}
				}
			}
			return cache.Result;
		}

		private bool MakeHttpsRequest(string action, string actionRequest) => MakeHttpsRequest(action, actionRequest, out _);

		private bool MakeHttpsRequest(string action, string actionRequest, out bool hasExceptionOnRequest)
		{
			LastStatusCode = MakeHttpsWebRequest(EndPoint, action, actionRequest, out hasExceptionOnRequest);

			if (LastStatusCode == HttpStatusCode.OK || LastStatusCode == HttpStatusCode.Found)
			{
				return true;
			}

			return false;
		}

		private HttpStatusCode MakeHttpsWebRequest(string endpoint, string action, string actionRequest, out bool hasExceptionOnRequest)
		{
			hasExceptionOnRequest = false;
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

			try
			{
				var uri = string.Concat(endpoint, action);
				var request = Request(uri, actionRequest);

				LastError = null;

				using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
				{
					return response.StatusCode;
				}
			}
			catch (Exception ex)
			{
				if (ex is WebException webException && webException.Response is HttpWebResponse response)
				{
					using (var stream = response.GetResponseStream())
					using (var reader = new StreamReader(stream))
					{
						LastError = reader.ReadToEnd();
					}
					return response.StatusCode;
				}

				hasExceptionOnRequest = true;
				return HttpStatusCode.Unauthorized;
			}
		}

		private ResultCache CacheResult(ResultCache cache, bool result, string password = null)
		{
			if (cache == null)
			{
				cache = new ResultCache();
			}
			var utcNow = DateTimeWrapper.UtcNow;
			cache.ExpirationTime = utcNow.AddSeconds(CacheDuration);
			cache.Result = result;
			cache.Password = password;
			cache.LastCheck = utcNow;
			return cache;
		}

		public virtual string EndPoint
		{
			get
			{
#if NETSTANDARD2_0_OR_GREATER
#else
				if (string.IsNullOrEmpty(endPoint))
				{
					endPoint = ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.ServiceClient.Endpoint"];
				}
#endif
				return endPoint;
			}
		}

		public bool Ping()
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

			try
			{
				var uri = string.Concat(EndPoint, "Ping");
				var request = HttpWebRequest.Create(uri);

				request.Credentials = CredentialCache.DefaultCredentials;
				using (var response = (HttpWebResponse)request.GetResponse())
				{
					return (response.StatusCode == HttpStatusCode.OK);
				}
			}
			catch
			{
				return false;
			}
		}

		public virtual IDateTime DateTimeWrapper
		{
			get
			{
				if (dateTime == null)
				{
					dateTime = new DateTimeWrapper();
				}
				return dateTime;
			}
		}

		internal virtual IRequest Request(string uri, string actionRequest)
		{
			var buffer = Encoding.ASCII.GetBytes(actionRequest);
			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.Timeout = 5000;
			request.Method = "POST";
			request.AllowAutoRedirect = false;
			request.Proxy = null;
			request.ContentType = "application/json";
			request.ContentLength = buffer.Length;
			using (var stream = request.GetRequestStream())
			{
				stream.Write(buffer, 0, buffer.Length);
			}

			return new HttpWebRequestWrapper(request);
		}

		public void ClearCaches()
		{
			existenceCaches.Clear();
			validationCaches.Clear();
		}
	}
}

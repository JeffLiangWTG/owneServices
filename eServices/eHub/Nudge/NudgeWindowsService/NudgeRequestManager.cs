using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Nudge
{
	public class NudgeRequestManager
    {
        public NudgeRequestManager(string systemCode)
        {
            this.SystemCode = systemCode;
            this.IsPendingRequest = false;
            this.ServiceStatus = 0;
            this.LastCallTimeUTC = DateTime.MinValue;
            this.QueueTimeUTC = DateTime.MinValue;
        }

        public static void RequestNudge(string systemCode)
        {
            NudgeRequestManager nudgeRequestManager = CacheManager.NudgeRequestControlCache.GetItemFromCacheOrCreateWhenNotExist(systemCode, delegate { return new NudgeRequestManager(systemCode); });
            nudgeRequestManager.ProcessNudgeRequest(systemCode);
        }

        public void ProcessNudgeRequest(string systemCode)
        {
            TimeSpan waitingTime = TimeSpan.MinValue;
            EHubClientSystem eHubClientSystem = CacheManager.SystemInfoCache.GetItemFromCache((string)systemCode);
            if (eHubClientSystem == null || string.IsNullOrWhiteSpace(eHubClientSystem.URL))
            {
                Logger.DebugFormat("Request to nudge system '{0}' wasn't queued because there is no valid nudge URL configured for that system", systemCode);
            }
            else if (IsPendingRequest)
            {
                Logger.DebugFormat("Request to nudge system '{0}' wasn't queued because there is a previous one pending which was queued on {1}", systemCode, QueueTimeUTC);
            }
            else if (ServiceStatus == SERVICE_STATUS_SUCCESS || IsCallTimeDue(out waitingTime))
			{
                IsPendingRequest = true;
                QueueTimeUTC = DateTime.UtcNow;
                Task.Run(() => DoNudge(SystemCode));
                Logger.DebugFormat("Request to nudge system '{0}' has been queued successfully on {1}", systemCode, QueueTimeUTC);
            }
            else
            {
                Logger.DebugFormat("Request to nudge system '{0}' wasn't queued because it has to wait {1} since last failed nudge before it can be queued again.", systemCode, waitingTime);
            }
        }

        bool IsCallTimeDue(out TimeSpan waitingTime)
        {
            TimeSpan[] nextIntervals = NudgeSettings.Instance.NEXT_INTERVALS;
            waitingTime = nextIntervals[Math.Min(ServiceStatus, nextIntervals.Length - 1)];
            ServiceStatus = Math.Min(ServiceStatus, nextIntervals.Length - 1);
            return DateTime.UtcNow >= LastCallTimeUTC + waitingTime;
        }

        protected virtual async Task DoNudge(object systemCode)
        {
            Logger.DebugFormat("Starting to nudge System '{0}'. This request was queued on {1} meaning a waiting time of {2} in the queue.", systemCode, QueueTimeUTC, DateTime.UtcNow - QueueTimeUTC);
            TimeSpan[] nextIntervals = NudgeSettings.Instance.NEXT_INTERVALS;
            TimeSpan waitingTime = nextIntervals[Math.Min(ServiceStatus, nextIntervals.Length - 1)];
            EHubClientSystem eHubClientSystem = CacheManager.SystemInfoCache.GetItemFromCache((string)systemCode);

            string endpoint = eHubClientSystem.SystemCode;

            var ccdsURL = NudgeSettings.Instance.NUDGE_CCDS_URL.Replace("{endpoint}", endpoint);

	        string nudgeURL = await CallCCDSServiceAsync(ccdsURL);

			if (string.IsNullOrWhiteSpace(nudgeURL))
			{
				Logger.WarnFormat("Failed to retrieve CCDS URL for system '{0}'. Skipping nudge.", systemCode);
			}

			else
            {
				int result = NudgeHttpUtil.Instance.MakeHttpRequest($"{nudgeURL}?code=EHI&key={NudgeSettings.Instance.AUTH_KEY}");
                if (result == 200)
                {
					Logger.InfoFormat("Successfully nudged System '{0}' via {1}.", systemCode, nudgeURL);
					ServiceStatus = 0;
				}
                else
                {
					Logger.WarnFormat("Failed to nudge System '{0}' with code {1}. URL: {2}.", systemCode, result, nudgeURL);
					ServiceStatus++;
				}
            }
            IsPendingRequest = false;
            LastCallTimeUTC = DateTime.UtcNow;
        }

		private static readonly HttpClient _httpClient = new HttpClient();

		static NudgeRequestManager()
		{
			string ccdsUrl = NudgeSettings.Instance.NUDGE_CCDS_URL;
			_httpClient.BaseAddress = new Uri(ccdsUrl.Split('?')[0]);
			ServicePoint servicePoint = ServicePointManager.FindServicePoint(_httpClient.BaseAddress);
			servicePoint.ConnectionLeaseTimeout = 60 * 1000;

		}

		protected virtual async Task<string> CallCCDSServiceAsync(string ccdsURL)
		{
			if (string.IsNullOrWhiteSpace(ccdsURL))
			{
				Logger.Warn("CCDS URL cannot be null or empty. Skipping call.");
				return string.Empty;
			}

			try
			{
				using (HttpResponseMessage response = await _httpClient.GetAsync(ccdsURL))
				{
					Logger.Debug($"CCDS response code: {response.StatusCode}");

					if (response.IsSuccessStatusCode)
					{
						string jsonResponse = await response.Content.ReadAsStringAsync();
						CCDSUrlResponse ccdsUrlResponse = JsonConvert.DeserializeObject<CCDSUrlResponse>(jsonResponse);
						return ccdsUrlResponse?.urls.Count > 0 ? ccdsUrlResponse.urls[0] : string.Empty;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error($"There was an error accessing CCDS service. Error: {ex.Message}", ex);
			}

			return string.Empty;
		}

		class CCDSUrlResponse
		{
			public List<string> urls { get; set; }
			public string keyvalue { get; set; }
		}

        public string SystemCode { get; set; }
        public int ServiceStatus { get; set; }
        public DateTime LastCallTimeUTC { get; set; }
        public bool IsPendingRequest { get; set; }
        public DateTime QueueTimeUTC { get; set; }

        static readonly ILog Logger = LogManager.GetLogger(typeof(NudgeRequestManager));
        const int SERVICE_STATUS_SUCCESS = 0;
    }
}

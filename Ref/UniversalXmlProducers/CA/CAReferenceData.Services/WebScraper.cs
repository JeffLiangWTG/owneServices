using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.CAReferenceData.Services
{
	public static class WebScraper
	{
		public static string GetHtmlWithWebDriver(string url)
		{
			string result = string.Empty;
			using (var webdriver = new WebDriverHelper())
			{
				result = webdriver.GetWebPage(url);
			}
			return result;
		}

		public static string GetHtml(string url)
		{
			var client = new HttpClientHelper();
			return client.GetWebPageAsync(url).Result;
		}

		public static bool DownloadFile(string url, string localPath)
		{
			var result = false;
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var client = new WebClient())
			{
				client.Headers.Add("user-agent", "WTG");
				client.DownloadFile(url, localPath);
				result = true;
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			return result;
		}
		
		public static string GetResponse(string url, Hashtable parameters)
		{
			string result = null;
			var apiUrl = url + "?" + ParsToString(parameters);
#pragma warning disable SYSLIB0014 // Type or member is obsolete
			using (var client = new WebClient())
			{
				client.Headers.Add("user-agent", "WTG");
				result = client.DownloadString(apiUrl);
			}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			return result;
		}

		public static string ParsToString(Hashtable pars)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string k in pars.Keys)
			{
				if (sb.Length > 0)
				{
					sb.Append('&');
				}
				sb.Append(HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(pars[k].ToString()));
			}
			return sb.ToString();
		}

#pragma warning disable CA1055 // URI-like return values should not be strings
		public static string GetUrlDownload(string url)
#pragma warning restore CA1055 // URI-like return values should not be strings
		{
			var html = string.Empty;
			string DownloadHtml()
			{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
				using (var client = new WebClient())
				{
					client.Headers.Add("user-agent", "WTG");
					return client.DownloadString(url);
				}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			}
			if (!string.IsNullOrEmpty(url))
			{
				var maxRetryTimes = Math.Max(ApplicationConfig.Instance.DownloadFileRetryTimes, 1);
				var retryTimes = 0;

				var result = false;

				while (!result && retryTimes < maxRetryTimes)
				{
					try
					{
						html = DownloadHtml();
						result = !string.IsNullOrWhiteSpace(html);
					}
					catch (Exception ex)
					{
						if (ex.Message.Contains("404"))
						{
							result = true;
						}
						if (retryTimes >= maxRetryTimes)
						{
							throw;
						}
					}
					finally
					{
						retryTimes++;
					}
				}
			}
			return html;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.USReferenceData.Services
{
	public class DownLoadService : IDownLoadService
	{
		public (bool successfullyParsed, DateTime dateTime) GetDateTimeFromHtmlNode(HtmlNode htmlNode)
		{
			var result = false;
			var dateTime = DateTime.MinValue;
			if (htmlNode != null)
			{
				var rgx = new Regex(@"(?i)(?<=\()(.*)(?=\))");
				var dateTimeAsString = rgx.Match(htmlNode.InnerText).Value;
				if (!string.IsNullOrEmpty(dateTimeAsString))
				{
					var formatStrings = new string[] { "MMM. dd, yyyy", "MMM. d, yyyy", "MMMM d, yyyy", "MMMM dd, yyyy" };
					result = DateTime.TryParseExact(dateTimeAsString, formatStrings, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
				}
			}
			return (result, dateTime);
		}

		public bool DownloadFile(string htmlString, string censusURL, string downloadFilePath)
		{
			var result = false;
			var match = new Regex(@"href=""([^""]*)").Match(htmlString);
			if (match.Success)
			{
				var url = match.Groups[1].Value;
				url = string.IsNullOrEmpty(censusURL) ? url : url.StartsWith(censusURL, StringComparison.OrdinalIgnoreCase) ? url : censusURL + url;
				result = DownloadFile(url, downloadFilePath);
			}
			return result;
		}

		public IEnumerable<HtmlNode> FindNodes(string url, Func<HtmlNode, bool> predicate)
		{
			var html = GetUrlDownload(url);
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			return doc.DocumentNode.Descendants().Where(predicate);
		}

		public HtmlNode FindNode(string url, Func<HtmlNode, bool> predicate)
		{
			var html = GetUrlDownload(url);
			var doc = new HtmlDocument();
			doc.LoadHtml(html);

			return doc.DocumentNode.Descendants().FirstOrDefault(predicate);
		}

		static string GetUrlDownload(string url)
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
					catch (Exception)
					{
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

		public byte[] DownloadData(string url)
		{
			byte[] result = null;

			Download(url, () =>
			{
#pragma warning disable SYSLIB0014 // Type or member is obsolete
				using (var client = new WebClient())
				{
					client.Headers.Add("user-agent", "WTG");
					result = client.DownloadData(new Uri(url));
					return true;
				}
#pragma warning restore SYSLIB0014 // Type or member is obsolete
			});

			return result;
		}

		public bool DownloadFile(string url, string localPath)
		{
			return Download(url, () =>
			{
				if (File.Exists(localPath))
				{
					File.Delete(localPath);
				}

#pragma warning disable SYSLIB0014 // Type or member is obsolete
				using (var client = new WebClient())
				{
					client.Headers.Add("user-agent", "WTG");
					client.DownloadFile(new Uri(url), localPath);
				}
#pragma warning disable SYSLIB0014 // Type or member is obsolete

				return File.Exists(localPath);
			});
		}

		static bool Download(string url, Func<bool> downloadFunc)
		{
			var result = false;

			if (!string.IsNullOrEmpty(url))
			{
				var maxRetryTimes = Math.Max(ApplicationConfig.Instance.DownloadFileRetryTimes, 1);
				var retryTimes = 0;

				while (!result && retryTimes < maxRetryTimes)
				{
					try
					{
						result = downloadFunc();
					}
					catch (Exception)
					{
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

			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public sealed class ErrorCodesDownloader : IErrorCodesDownloader
	{
		public ErrorCodesDownloader(IHttpClient client, IErrorCodeDownloadContextProvider contextProvider, int sleepInterval, int maxRetry, int codeSearchStart, int codeSearchEnd, ILogger logger)
		{
			this.client = client;
			this.contextProvider = contextProvider;
			this.sleepInterval = sleepInterval;
			this.maxRetry = maxRetry;
			this.codeSearchStart = codeSearchStart;
			this.codeSearchEnd = codeSearchEnd;
			this.logger = logger;
		}

		public List<ErrorCodeItem> GetErrorCodes(ErrorCodeType type)
		{
			logger.Log(LogType.Info, $"Downloading Error Codes for Type: {type}");
			var downloadContext = contextProvider.GetContext(type);
			FetchAndStoreCookie();

			var errorCodeItems = new List<ErrorCodeItem>();
			for (var index = codeSearchStart; index <= codeSearchEnd; index++)
			{
				errorCodeItems.AddRange(GetTableCodes(index, downloadContext));
			}
			return errorCodeItems;
		}

		List<ErrorCodeItem> GetTableCodes(int index, ErrorCodeDownloadContext downloadContext)
		{
			var code = index.ToString(TwoDigitFormat, CultureInfo.InvariantCulture);
			var subUrl = $"{downloadContext.SubUrl}?cth={code}&item=&submitbutton=Search";

			var errorCodeItems = new List<ErrorCodeItem>();
			var content = GetCodesResponse(subUrl);
			var codeType = downloadContext.CodeType;
			if (string.IsNullOrEmpty(content))
			{
				logger.Log(LogType.ReviewRequired, $"No response from source for code: {code}, Type: {codeType} and Url; {subUrl}");
				return errorCodeItems;
			}

			var doc = new HtmlDocument();
			doc.LoadHtml(content);
			var codesTable = doc.DocumentNode.SelectSingleNode(downloadContext.TableXpath);
			if (codesTable is null)
			{
				logger.Log(LogType.ReviewRequired, $"Table not found for code: {code} and Type: {codeType}");
				return errorCodeItems;
			}

			var rows = codesTable.SelectNodes(".//tr");
			for (int i = 2; i < rows.Count; i++)
			{
				var columns = rows[i].SelectNodes("td");
				if (columns == null || columns.Count != 3)
				{
					logger.Log(LogType.ReviewRequired, $"Unexpected number of columns in the table for code: {code} and Type: {codeType}");
					continue;
				}

				var item = new ErrorCodeItem
				{
					ErrorCode = columns[0].InnerText.Trim(),
					Description = columns[1].InnerText.Trim(),
					MessageType = columns[2].InnerText.Trim()
				};

				errorCodeItems.Add(item);
			}

			logger.Log(LogType.Info, $"Codes starts with:{code} and Count: {errorCodeItems.Count}");
			return errorCodeItems;
		}

		string GetCodesResponse(string searchUrl)
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					var searchResponse = client.Get(new Uri(baseUri, searchUrl));
					Helper.Assume(searchResponse.IsSuccessStatusCode, $"Search request failed with status: {searchResponse.StatusCode}");
					return searchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					if (++retryCount >= maxRetry)
					{
						var message = $"Failed to get data from the source after {retryCount} retries";
						logger.Log(LogType.ReviewRequired, message, ex);
						throw new UnhandledApplicationException(message, ex);
					}

					Thread.Sleep(sleepInterval);
					FetchAndStoreCookie();
				}
			}
		}

		void FetchAndStoreCookie()
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					var response = client.Get(new Uri(baseUri, "index.jsp"));
					Helper.Assume(response.IsSuccessStatusCode, $"Request failed with status: {response.StatusCode}");
					logger.Log(LogType.Info, "Cookie set");
					return;
				}
				catch (Exception ex)
				{
					if (++retryCount >= maxRetry)
					{
						var message = $"Failed to fetch cookie from the source after {retryCount} retries";
						logger.Log(LogType.ReviewRequired, message, ex);
						throw new UnhandledApplicationException(message, ex);
					}

					Thread.Sleep(sleepInterval);
				}
			}
		}

		readonly Uri baseUri = new Uri(AppConfig.ErrorCodes.BaseUrl);
		readonly ILogger logger;
		readonly IHttpClient client;
		readonly IErrorCodeDownloadContextProvider contextProvider;
		readonly int sleepInterval;
		readonly int maxRetry;
		readonly int codeSearchStart;
		readonly int codeSearchEnd;
		const string TwoDigitFormat = "D2";
	}
}

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.RefDbRepo.INReferenceData.Services.Tariff;
using CargoWise.RefDbRepo.INReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public class TariffPdfDownloader : ITariffPdfDownloader
	{
		public TariffPdfDownloader(IHttpClientHelper httpClientHelper, IFolderLocationProvider locationProvider, ILogger logger, int sleepInterval)
		{
			this.httpClientHelper = httpClientHelper;
			this.locationProvider = locationProvider;
			this.logger = logger;
			this.sleepInterval = sleepInterval;
		}

		public TariffPdfDownloader(IHttpClientHelper httpClientHelper, IFolderLocationProvider locationProvider, ILogger logger)
			: this(httpClientHelper, locationProvider, logger, AppConfig.Tariff.SleepInterval)
		{
		}

		public DateTime? GetLatestTariffDate(DateTime? lastProcessedDate)
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					return GetLatestDateToProcessTariffData(lastProcessedDate);
				}
				catch (Exception ex)
				{
					if (++retryCount >= AppConfig.Tariff.MaxRetry)
					{
						var message = $"Failed to get latest Tariff date from the source after {retryCount} retries";
						logger.Log(LogType.ReviewRequired, message, ex);
						throw new UnhandledApplicationException(message, ex);
					}

					Thread.Sleep(sleepInterval);
				}
			}
		}

		public string DownloadPdfFiles(DateTime date)
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					return DownloadPdfFilesCore(date);
				}
				catch (Exception ex)
				{
					if (++retryCount >= AppConfig.Tariff.MaxRetry)
					{
						var message = $"Failed to download Tariff files from the source after {retryCount} retries for date: {date}";
						logger.Log(LogType.ReviewRequired, message, ex);
						throw new UnhandledApplicationException(message, ex);
					}

					Thread.Sleep(sleepInterval);
				}
			}
		}

		#region Implementation

		DateTime? GetLatestDateToProcessTariffData(DateTime? lastProcessedDate)
		{
			var requestUri = new Uri(BaseUri, AppConfig.Tariff.TariffSubUrl);
			var response = GetResponse<TariffWebData>(requestUri);

			var selectedTariff = response?.childContentList?.Where(x => lastProcessedDate == null || x.GetTariffDate() > lastProcessedDate.Value).OrderByDescending(x => x.GetTariffDate()).FirstOrDefault();
			return selectedTariff?.GetTariffDate();
		}

		string DownloadPdfFilesCore(DateTime date)
		{
			var tariffId = GetSeletedTariffId(date);
			if (string.IsNullOrEmpty(tariffId))
			{
				logger.Log(LogType.Warning, "No tariffId found for the date", date);
				return null;
			}

			var partId = GetSelectedPartId(tariffId);
			if (string.IsNullOrEmpty(partId))
			{
				logger.Log(LogType.Warning, "No partId found for the tariffId", tariffId);
				return null;
			}

			var downloadFolder = locationProvider.GetPdfDownloadFolder(date);

			DownloadTariffPdfFiles(partId, downloadFolder);

			return downloadFolder;
		}

		string GetSeletedTariffId(DateTime date)
		{
			var requestUri = new Uri(BaseUri, AppConfig.Tariff.TariffSubUrl);
			var response = GetResponse<TariffWebData>(requestUri);

			var selectedTariff = response?.childContentList?.OrderByDescending(x => x.GetTariffDate()).FirstOrDefault(x => x.GetTariffDate() == date);
			return GetIdFromPath(selectedTariff?.path);
		}

		string GetSelectedPartId(string latestTariffId)
		{
			var requestUri = new Uri(BaseUri, latestTariffId);
			var response = GetResponse<TariffWebData>(requestUri);

			var selectedPart = response?.childContentList?.FirstOrDefault(x => Regex.IsMatch(x.titleEn, AppConfig.Tariff.PartToDownload));
			return GetIdFromPath(selectedPart?.path);
		}

		#region Download files

		void DownloadTariffPdfFiles(string partId, string downloadFolder)
		{
			var requestUri = new Uri(BaseUri, partId);
			var response = GetResponse<TariffWebData>(requestUri);
			if (response == null || response.childContentList.Length == 0)
			{
				logger.Log(LogType.Warning, "No sections found for the partId", partId);
				return;
			}

			foreach (var section in response.childContentList)
			{
				DownloadEachSection(GetIdFromPath(section.path), downloadFolder);
			}
		}

		void DownloadEachSection(string sectionId, string downloadFolder)
		{
			var requestUri = new Uri(BaseUri, sectionId);
			var response = GetResponse<TariffWebData>(requestUri);
			if (response == null || response.childContentList.Length == 0)
			{
				logger.Log(LogType.Warning, "No files found for the sectionId", sectionId);
				return;
			}

			foreach (var eachFile in response.childContentList)
			{
				DownloadEachFile(GetIdFromPath(eachFile.path), downloadFolder);
			}
		}

		void DownloadEachFile(string fileId, string downloadFolder)
		{
			var requestUri = new Uri(BaseUri, fileId);
			var response = GetResponse<TariffWebData>(requestUri);
			if (response == null || response.cbicDocMsts.Length == 0)
			{
				logger.Log(LogType.Warning, "No files found for the fileId", fileId);
				return;
			}

			foreach (var eachFile in response.cbicDocMsts)
			{
				DownloadFileFromPath(eachFile.filePathEn, downloadFolder);
			}
		}

		void DownloadFileFromPath(string filePathEn, string downloadFolder)
		{
			var requestUri = new Uri(ContentBaseUri, filePathEn);
			var response = GetResponse<TariffWebPdfData>(requestUri);

			if (response == null)
			{
				logger.Log(LogType.Warning, "No response received for the file path", filePathEn);
				return;
			}

			var path = Path.Combine(downloadFolder, response.fileName.Replace("/", "_"));
			var data = Convert.FromBase64String(response.data);
			File.WriteAllBytes(path, data);
		}

		#endregion

		T GetResponse<T>(Uri requestUri) where T : class
		{
			var retryCount = 0;
			while (true)
			{
				try
				{
					return GetResponseCore<T>(requestUri);
				}
				catch (Exception ex)
				{
					if (++retryCount >= AppConfig.Tariff.MaxRetry)
					{
						var message = $"Failed to GetResponse for {requestUri} after {retryCount} retries";
						logger.Log(LogType.ReviewRequired, message, ex);
						throw new UnhandledApplicationException(message, ex);
					}

					Thread.Sleep(sleepInterval);
				}
			}
		}

		T GetResponseCore<T>(Uri requestUri) where T : class
		{
			var requestUrl = requestUri.AbsoluteUri;
			logger.Log(LogType.Info, "Requesting data from", requestUrl);
			var response = httpClientHelper.GetAsync(requestUrl).GetAwaiter().GetResult();

			if (response is null)
			{
				return null;
			}

			using (var reader = new StreamReader(response, Encoding.UTF8))
			{
				string responseContent = reader.ReadToEnd();
				Helper.Assume(!string.IsNullOrEmpty(responseContent),
					"Failed to load data from the source, Response is empty");

				try
				{
					var data = JsonConvert.DeserializeObject<T>(responseContent);
					logger.Log(LogType.Info, "Data received from", requestUrl);
					return data;
				}
				catch (JsonSerializationException ex)
				{
					var message = $"Failed to deserialize the response content into {nameof(T)}";
					logger.Log(LogType.ReviewRequired, message, ex);
					throw new UnhandledApplicationException(message, ex);
				}
			}
		}

		string GetIdFromPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				logger.Log(LogType.Warning, "path is empty");
				return null;
			}
			var parts = path.Split("/");
			if (parts.Length == 0)
			{
				logger.Log(LogType.Warning, "path is not in correct format");
				return null;
			}

			return parts[parts.Length - 1];
		}

		#endregion

		Uri BaseUri => baseUri ?? (baseUri = new Uri(AppConfig.Tariff.BaseUrl));
		Uri baseUri;

		Uri ContentBaseUri => contentBaseUri ?? (contentBaseUri = new Uri(AppConfig.Tariff.ContentBaseUrl));
		Uri contentBaseUri;

		readonly ILogger logger;
		readonly int sleepInterval;
		readonly IFolderLocationProvider locationProvider;
		readonly IHttpClientHelper httpClientHelper;
	}
}

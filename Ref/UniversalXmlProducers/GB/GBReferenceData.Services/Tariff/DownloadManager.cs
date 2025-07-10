using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.GBReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Tariff
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
	public abstract class DownloadManager : IDownloadManager
	{
		protected DownloadManager(string workingFolder)
		{
			this.workingFolder = workingFolder;
		}
		readonly string workingFolder;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		public void RunDownloadProcess(string contentFolder)
		{
			for (int i = 0; i < RetryCount; i++)
			{
				try
				{
					var webClientWrapper = GetWebClientWrapper();
					GetAuthorisationDetails(webClientWrapper);
					ProcessUrls(webClientWrapper, workingFolder, contentFolder);
					break;
				}
				catch (Exception ex)
				{
					var errorMessage = $"The following processing errors occurred:{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex?.InnerException?.Message}";
					if (i == RetryCount - 1)
					{
						throw new ApplicationException(errorMessage, ex);
					}
					else
					{
						Console.WriteLine($"Retry {i + 1} - {errorMessage}");
						System.Threading.Thread.Sleep(RetryInterval);
					}
				}
			}
		}

		static void GetAuthorisationDetails(ITariffWebClientWrapper webClientWrapper)
		{
			var authHeader = GetAuthorisationHeader(webClientWrapper);

			if (!string.IsNullOrEmpty(authHeader))
			{
				webClientWrapper.SetAuthorisationHeader(authHeader);
			}
		}

		protected static string GetAuthorisationHeader(ITariffWebClientWrapper webClientWrapper)
		{
			var authHeader = string.Empty;

			try
			{
				var requestParams = new System.Collections.Specialized.NameValueCollection
				{
					{ "client_secret", ConfigurationProvider.TariffClientSecret },
					{ "client_id", ConfigurationProvider.TariffClientId },
					{ "grant_type", "client_credentials" },
					{ "scope", ConfigurationProvider.TariffClientScope }
				};

				var content = webClientWrapper.GetContentFromPost(ConfigurationProvider.TariffAuthorisationUrl, requestParams);

				if (!string.IsNullOrEmpty(content))
				{
					var result = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthorisationResponse>(content);

					authHeader = $"{result.token_type} {result.access_token}";
				}
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Failed to retrieve authorisation token", ex);
			}

			return authHeader;
		}

		static void ProcessUrls(ITariffWebClientWrapper webClientWrapper, string workingFolder, string contentFolder)
		{
			PrepareEnvironment(workingFolder, contentFolder);

			try
			{
				var fileList = GetFileList(webClientWrapper);

				RemoveUnexpectedFiles(new HashSet<string>(fileList.Select(x => x.Filename)), workingFolder);

				foreach (var fileDetail in fileList)
				{
					DownloadAndUnzipFile(webClientWrapper, workingFolder, contentFolder, fileDetail);
				}
			}
			catch (ApplicationException ex)
			{
				throw new ApplicationException($"Processing of files in {workingFolder} failed", ex);
			}
		}

		protected static List<FileDetails> GetFileList(ITariffWebClientWrapper webClientWrapper)
		{
			var fileList = new List<FileDetails>();

			foreach (var url in new string[] { ConfigurationProvider.TariffDailyUrl, ConfigurationProvider.TariffMonthlyUrl, ConfigurationProvider.TariffAnnualUrl })
			{
				var files = FileHelper.GetFileList(webClientWrapper, url);
				if (url == ConfigurationProvider.TariffAnnualUrl)
				{
					bool comparer(FileDetails x) => x.Filename.StartsWith(ConfigurationProvider.TariffUseSpecifiedAnnualFileOnly, StringComparison.OrdinalIgnoreCase);
					if (!string.IsNullOrEmpty(ConfigurationProvider.TariffUseSpecifiedAnnualFileOnly) && files.Any(comparer))
					{
						files = files.Where(comparer).ToList();
					}
					else if (ConfigurationProvider.TariffUseMostRecentAnnualFileOnly)
					{
						files = files.OrderByDescending(x => x.Filename).Take(1).ToList();
					}
				}
				fileList.AddRange(files);
			}

			return fileList.Where(x => !ConfigurationProvider.TariffFileExclusionList.Contains(x.Filename)).ToList();
		}

		static void RemoveUnexpectedFiles(HashSet<string> currentRequiredFiles, string workingFolder)
		{
			var files = Directory.GetFiles(workingFolder);
			foreach (var filepath in files.Where(x => !currentRequiredFiles.Contains(Path.GetFileName(x))))
			{
				File.Delete(filepath);
			}
		}

		static void DownloadAndUnzipFile(ITariffWebClientWrapper webClientWrapper, string workingFolder, string contentFolder, FileDetails fileDetail)
		{
			var downloadedFile = FileHelper.DownloadFile(webClientWrapper, fileDetail, workingFolder);
			FileHelper.UnzipFile(downloadedFile, contentFolder);
		}

		static void PrepareEnvironment(string workingFolder, string contentFolder)
		{
			try
			{
				Directory.CreateDirectory(workingFolder);
			}
			catch
			{
				throw new ApplicationException($"Could not create working folder '{workingFolder}'");
			}

			try
			{
				if (Directory.Exists(contentFolder))
				{
					Directory.Delete(contentFolder, true);

					//Wait for the operating system to delete (async call above)
					var stopWatch = new System.Diagnostics.Stopwatch();
					stopWatch.Start();
					while (Directory.Exists(contentFolder) && stopWatch.ElapsedMilliseconds < 30000)
					{
						System.Threading.Thread.Sleep(100);
					}
				}
				Directory.CreateDirectory(contentFolder);
			}
			catch
			{
				throw new ApplicationException($"Could not create content folder '{contentFolder}'");
			}
		}

		public abstract ITariffWebClientWrapper GetWebClientWrapper();

		protected virtual int RetryCount => 5;
		protected virtual TimeSpan RetryInterval => TimeSpan.FromMinutes(1);
	}

	internal class WebClientDownloadManager : DownloadManager
	{
		public WebClientDownloadManager() : base(GetWorkingFolder())
		{
		}

		public static string GetWorkingFolder() => Path.Combine(Path.GetTempPath(), "GBTariffData");

		public override ITariffWebClientWrapper GetWebClientWrapper() => new TariffWebClientWrapper();
	}
}

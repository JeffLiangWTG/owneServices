using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class DailyTariffFactory : IDailyTariffFactory
	{
		string _downloadFolderPath;
		public ICollection<RefCusTariff> TariffCollectionFromNomenclature { get; }

		public DailyTariffFactory(ICollection<RefCusTariff> tariffCollection, string downloadFolderPath = "")
		{
			TariffCollectionFromNomenclature = tariffCollection;
			_downloadFolderPath = downloadFolderPath;
		}

		public IFileDownloaderWrapper GetFileDownloaderWrapper() => new FileDownloaderWrapper(httpClient);

		public IRawRecord GetRawRecord(IDailyTariffRateParser dailyTariffRateParser, ISEMeasureParser seMeasureParser)
		{
			return new ImportRawRecord(dailyTariffRateParser, seMeasureParser);
		}

		public ITariffDataProducer GetDailyTariffDataProducer(IRawRecord dailyRawRecord, ITariffGenerator tariffGenerator, ITariffMerger tariffMerger)
		{
			return new TariffDataProducer(dailyRawRecord, tariffGenerator, tariffMerger);
		}

		public IXmlProducer<RefCusTariff> GetXmlProducer()
		{
			return new DailyTariffXmlProducer();
		}

		public IXmlProducer<RefCusTariff> GetSEXmlProducer()
		{
			return new SETariffXmlProducer();
		}

		public IEnumerable<IWebFileInfo> GetPreDownloadedFiles()
		{
			if (string.IsNullOrEmpty(_downloadFolderPath))
			{
				yield break;
			}
			var publishTime = DateTime.MinValue;
			var dailyFileName = Path.Combine(_downloadFolderPath, ApplicationConfig.DailyFileName);
			if (File.Exists(dailyFileName))
			{
				var fileLastWriteTime = File.GetLastWriteTime(dailyFileName);
				if (fileLastWriteTime > publishTime)
				{
					publishTime = fileLastWriteTime;
				}
			}
			ApplicationConfig.SetDownloadsFolder(_downloadFolderPath);
			ApplicationConfig.SetPublishTime(publishTime);
			foreach (var file in Directory.GetFiles(_downloadFolderPath))
			{
				var fileName = Path.GetFileName(file);
				var lastWriteTime = File.GetLastWriteTime(file);
				yield return new WebFileInfo(fileName, _downloadFolderPath, lastWriteTime);
			}
		}

		public IWebDriverHelper GetWebDriverHelper()
		{
			return new WebDriverHelper();
		}

		public DateTime ExtractDailyZipPublishTimeFromTaricWebSite(IWebDriverHelper webDriverHelper)
		{
			var pageHtml = webDriverHelper.GetWebPage(ApplicationConfig.SectionDetailsUrl);
			var match = Regex.Match(pageHtml, @"<span>Last TARIC update:.+\s.*\s.*\s.*([0-9]{2}-[0-9]{2}-[0-9]{4})\s");
			if (match != null && match.Success && match.Groups.Count > 1)
			{
				return DateTime.ParseExact(match.Groups[1].Value, "dd-MM-yyyy", CultureInfo.InvariantCulture);
			}
			throw new InvalidOperationException("Could not read the Daily publish time from the web site");
		}

		public IEnumerable<IWebFileInfo> GetOrDownloadMonthlyFiles(IProducer monthlyProducer)
		{
			var filesDontExist = false;
			var result = new List<IWebFileInfo>();
			foreach (var file in monthlyProducer.FilesToLocate)
			{
				var filePath = Path.Combine(ApplicationConfig.DownloadsFolder, file);
				if (!File.Exists(filePath))
				{
					filesDontExist = true;
					break;
				}
				result.Add(new WebFileInfo(file, filePath, File.GetLastWriteTime(filePath)));
			}
			var outOfDate = DailyFilesHelper.CheckFileLastTimeWrite(monthlyProducer.FilesToLocate);
			if (filesDontExist || outOfDate)
			{
				var monthlyWebFiles = monthlyProducer.LocateWebFiles();
				var filesDownloadedSuccessfully = monthlyProducer.DownloadFiles(monthlyWebFiles);
				if (!filesDownloadedSuccessfully)
				{
					throw new InvalidOperationException("Failure downloading files.");
				}
				return monthlyWebFiles;
			}
			return result;
		}

		public IEnumerable<IWebFileInfo> LocateWebFiles(IWebDriverHelper webDriverHelper)
		{
			var preDownloadedFiles = GetPreDownloadedFiles();
			if (preDownloadedFiles.Any())
			{
				return preDownloadedFiles;
			}
			Console.WriteLine(ApplicationConfig.DailyFileName);
			Console.WriteLine(ApplicationConfig.DownloadsFolder);

			if (!Directory.Exists(ApplicationConfig.DownloadsFolder))
			{
				Directory.CreateDirectory(ApplicationConfig.DownloadsFolder);
			}
			var downloadFilePath = Path.Combine(ApplicationConfig.DownloadsFolder, ApplicationConfig.DailyZipFileName);
			var result = new List<IWebFileInfo>();

			//get or download monthly files
			var monthlyImportProducer = new ImportTariffProducer();
			result.AddRange(GetOrDownloadMonthlyFiles(monthlyImportProducer));

			var fileDownloaderWrapper = GetFileDownloaderWrapper();
			var webPage = webDriverHelper.GetWebPage(ApplicationConfig.DailyTariffDownloadUrl);
			var downloadUrlAndPublicationDate = GetDownloadUrlAndPublicationDate(webPage);
			var files = fileDownloaderWrapper.DownloadAndExtract(downloadUrlAndPublicationDate.Item1, downloadFilePath, downloadUrlAndPublicationDate.Item2);
			var measureFile = MoveExtractedDailyFile(files);
			if (string.IsNullOrEmpty(measureFile))
			{
				result.Add(new WebFileInfo(ApplicationConfig.DailyFileName, new InvalidOperationException($"File {ApplicationConfig.DailyFileName} not found when reading {ApplicationConfig.DailyZipFileName}")));
				return result.ToArray();
			}
			ApplicationConfig.SetPublishTime(ExtractDailyZipPublishTimeFromTaricWebSite(webDriverHelper));
			result.Add(new WebFileInfo(ApplicationConfig.DailyFileName, ApplicationConfig.DownloadsFolder, downloadUrlAndPublicationDate.Item2));
			return result.ToArray();
		}

		public static Tuple<string, DateTime> GetDownloadUrlAndPublicationDate(string webPage)
		{
			var htmlDocument = new HtmlAgilityPack.HtmlDocument();
			htmlDocument.LoadHtml(webPage);
			var htmlNode = htmlDocument.DocumentNode.SelectSingleNode(@"//*[@id='outer-container']/div[@class='form-content']/form[@name='publications_form']/table[1]/tbody[1]/tr[1]");
			if (htmlNode == null)
			{
				throw new InvalidOperationException("Cannot find latest download url on TARIC Daily website.");
			}

			var downloadUrl = htmlNode.SelectSingleNode("td[3]/a")?.Attributes["href"].Value.Trim();
			var dateString = htmlNode.SelectSingleNode("td[1]")?.InnerText.Trim();
			var publicationDate = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
			return Tuple.Create(downloadUrl, publicationDate);
		}

		protected static string MoveExtractedDailyFile(string[] files)
		{
			var dailyFilePath = string.Empty;
			var dailyFileNameExtension = Path.GetExtension(ApplicationConfig.DailyFileName);
			var dailyFileNameWithoutExtension = Path.GetFileNameWithoutExtension(ApplicationConfig.DailyFileName);
			for (var i = 0; i < files.Length; i++)
			{
				var extension = Path.GetExtension(files[i]);
				var fileName = Path.GetFileNameWithoutExtension(files[i]);
				if (fileName.StartsWith(dailyFileNameWithoutExtension, StringComparison.Ordinal) && extension == dailyFileNameExtension)
				{
					fileName = Path.Combine(ApplicationConfig.DownloadsFolder, ApplicationConfig.DailyFileName);
					if (File.Exists(fileName))
					{
						File.Delete(fileName);
					}
					File.Move(files[i], fileName);
					dailyFilePath = fileName;
				}
				else
				{
					File.Delete(files[i]);
				}
			}
			return dailyFilePath;
		}

		public ISEMeasureParser GetSEMeasureParser()
		{
			return new SEMeasureParser(new SEMeasureDownloader());
		}

		static readonly HttpClient httpClient = FileDownloaderHttpClientFactory.CreateHttpClient();
	}
}

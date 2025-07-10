using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class DailyTariffUpdatesFileProvider : IDailyTariffUpdatesFileProvider
	{
		public ICollection<IRawNomenclatureDailyRecord> NomenclatureDailyRawRecordCollection => nomenclatureDailyRawRecordCollection ?? (nomenclatureDailyRawRecordCollection = GetNomenclatureDailyRawRecords());
		public ICollection<IRawRateDailyRecord> RateDailyRawRecordCollection => rateDailyRawRecordCollection ?? (rateDailyRawRecordCollection = GetRateDailyRawRecords());

		public DateTime? LastDailyPublishTime => lastDailyPublishTime ?? (lastDailyPublishTime = nomenclatureFileCollection.OrderBy(x => x.LastModificationTime).LastOrDefault()?.LastModificationTime);

		public void CleanAll()
		{
			ResetDailyPublishState();

			var downloadPath = DownloadPath;
			if (Directory.Exists(downloadPath))
			{
				Directory.Delete(downloadPath, true);
			}
			filesHaveBeenDownloaded = false;
			nomenclatureDailyRawRecordCollection = null;
			rateDailyRawRecordCollection = null;
			lastDailyPublishTime = null;
		}

		public void DownloadAndExtractDailyUpdates(DateTime referenceDate, DateTime monthlyPublishDate)
		{
			if (filesHaveBeenDownloaded)
			{
				return;
			}

			if (referenceDate < monthlyPublishDate)
			{
				return;
			}

			Directory.CreateDirectory(ApplicationConfig.DownloadsFolder);

			using (var webDriverHelper = GetWebDriverHelper())
			{
				var webPage = webDriverHelper.GetWebPage(ApplicationConfig.DailyTariffDownloadUrl);
				var packagesTableNode = GetListOfPackages(webPage);

				foreach (var rowNode in packagesTableNode.ChildNodes.Where(x => x.Name == "tr").Reverse())
				{
					var downloadUrl = rowNode.SelectSingleNode("td[3]/a")?.Attributes["href"].Value.Trim();
					var dateString = rowNode.SelectSingleNode("td[1]")?.InnerText.Trim();
					var publicationDateIsValid = DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var publicationDate);

					if (!IsValidPackage(referenceDate, downloadUrl, publicationDateIsValid, publicationDate, monthlyPublishDate))
					{
						continue;
					}

					var files = DownloadAndExtractPackage(downloadUrl, publicationDate);

					foreach (var file in files)
					{
						var fileName = Path.GetFileNameWithoutExtension(file);

						if (IsMeasureFile(fileName))
						{
							AddToCollectionIfNotExists(rateFileCollection, fileName, file, publicationDate);
						}
						else if (IsNomenclatureFile(fileName))
						{
							AddToCollectionIfNotExists(nomenclatureFileCollection, fileName, file, publicationDate);
						}
						else
						{
							File.Delete(file);
						}
					}
				}
			}

			filesHaveBeenDownloaded = true;
		}

		static void AddToCollectionIfNotExists(ICollection<IWebFileInfo> collection, string fileName, string filePath, DateTime publicationDate)
		{
			if (!collection.Select(x => x.FileName).Contains(fileName))
			{
				collection.Add(new WebFileInfo(fileName, filePath, publicationDate));
			}
		}

		void ResetDailyPublishState()
		{
			rateFileCollection.Clear();
			nomenclatureFileCollection.Clear();
			lastDailyPublishTime = null;
		}

		static bool IsNomenclatureFile(string fileName)
			=> fileName.StartsWith(GoodsNomenclatureFileNamePrefix, StringComparison.InvariantCulture);

		static bool IsMeasureFile(string fileName)
			=> fileName.StartsWith(MeasuresFileNamePrefix, StringComparison.InvariantCulture);

		static bool IsValidPackage(
			DateTime referenceDate,
			string downloadUrl,
			bool publicationDateIsValid,
			DateTime publicationDate,
			DateTime minDateTime)
		{
			return !string.IsNullOrWhiteSpace(downloadUrl)
				&& publicationDateIsValid
				&& publicationDate <= referenceDate
				&& publicationDate >= minDateTime;
		}

		string[] DownloadAndExtractPackage(string downloadUrl, DateTime publicationDate)
		{
			var packageFolderName = $"TARIC_{publicationDate:yyyyMMdd}";
			var packageDownloadFolderPath = Path.Combine(DownloadPath, packageFolderName);
			Directory.CreateDirectory(packageDownloadFolderPath);

			var packageDownloadFilePath = Path.Combine(packageDownloadFolderPath, $"{packageFolderName}.zip");
			var dailyPackageDownloader = GetDailyPackageDownloader();
			dailyPackageDownloader.DownloadAndExtract(downloadUrl, packageDownloadFilePath, packageDownloadFolderPath, deletePackage: true);

			return Directory.GetFiles(packageDownloadFolderPath);
		}

		static HtmlNode GetListOfPackages(string webPage)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(webPage);
			var packagesTableNode = htmlDocument.DocumentNode.SelectSingleNode(@"//*[@id='outer-container']/div[@class='form-content']/form[@name='publications_form']/table[1]/tbody[1]")
				?? throw new InvalidOperationException("Cannot find latest download url on TARIC Daily website.");
			return packagesTableNode;
		}

		string DownloadPath => downloadPath ?? (downloadPath = Path.Combine(ApplicationConfig.DownloadsFolder, DailyUpdatesDownloadFolder));
		string downloadPath;

		protected virtual IWebDriverHelper GetWebDriverHelper()
		{
			return new WebDriverHelper();
		}

		protected virtual IDailyPackageDownloader GetDailyPackageDownloader()
		{
			return new DailyPackageDownloader();
		}

		List<IRawNomenclatureDailyRecord> GetNomenclatureDailyRawRecords()
		{
			return nomenclatureFileCollection
				.SelectMany(x => new NomenclatureDailyParser(x.FileName).Parse(x.DownloadPath))
				.ToList();
		}

		List<IRawRateDailyRecord> GetRateDailyRawRecords()
		{
			return rateFileCollection
				.SelectMany(x => new RateDailyParser(x.FileName).Parse(x.DownloadPath))
				.ToList();
		}

		DateTime? lastDailyPublishTime;
		bool filesHaveBeenDownloaded;
		List<IRawNomenclatureDailyRecord> nomenclatureDailyRawRecordCollection;
		List<IRawRateDailyRecord> rateDailyRawRecordCollection;

		readonly List<IWebFileInfo> nomenclatureFileCollection = new List<IWebFileInfo>();
		readonly List<IWebFileInfo> rateFileCollection = new List<IWebFileInfo>();

		const string DailyUpdatesDownloadFolder = "DailyUpdates";
		const string MeasuresFileNamePrefix = "Measures_";
		const string GoodsNomenclatureFileNamePrefix = "Goods_Nomenclature_";
	}
}

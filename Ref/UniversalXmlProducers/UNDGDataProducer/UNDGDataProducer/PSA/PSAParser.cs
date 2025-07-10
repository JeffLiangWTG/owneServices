using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class PSAGroupParser
	{
		readonly string _zipFilePath;
		readonly IFileDownloaderWrapper _fileDownloaderWrapper;

		public PSAGroupParser(IFileDownloaderWrapper fileDownloaderWrapper, string zipFilePath)
		{
			_fileDownloaderWrapper = fileDownloaderWrapper;
			_zipFilePath = zipFilePath;
		}

		public async Task ParseAndSaveXmlAsync(string filePath)
		{
			var substances = new IMOParser(_fileDownloaderWrapper, _zipFilePath).ParseZipAndExtractUNDGSbustances();
			if (substances == null || !substances.Any())
			{
				Console.Error.WriteLine("Error extract IMO records from zip file");
				return;
			}

			await using var webScraper = await WebScraper.CreateAsync();
			var pSARecords = await GetPSARecordsAsync(substances, webScraper);

			var uNDGWithPSARecords = MergePSARecordsToUNDGSubstances(substances, pSARecords);
			XmlWriterHelper.ExportToXml(PSAXmlWriterConfiguration.GetXmlWriter(), uNDGWithPSARecords, filePath);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
		public static async Task<IEnumerable<PSARecord>> GetPSARecordsAsync(IEnumerable<UNDGSubstance> originalRecords, IWebScraper webScraper)
		{
			var results = new List<PSARecord>();
			var firstUNProcessedUNNOs = new Dictionary<string, string>();
			var numberOfRecordsProcessed = 0;
			SetDefaultConnectionLimitAndProtocol();
			var uNNOWithIndexToExtract = originalRecords.Select(x => x.DG_UNNO).Distinct().Select((unno, index) => new { unno, index });
			var totalNumberOfRecords = uNNOWithIndexToExtract.Count();
			var htmlForErrorUNNOs = new List<string>();

			foreach (var uNNOBatch in uNNOWithIndexToExtract.GroupBy(g => g.index / Constants.FetchPSAGroupBatchSize, i => i.unno))
			{
				await AddPSARecordsToListInBatchAsync(uNNOBatch, webScraper, results, firstUNProcessedUNNOs);
				numberOfRecordsProcessed += uNNOBatch.Count();
				DisplayProgress((numberOfRecordsProcessed - firstUNProcessedUNNOs.Count), totalNumberOfRecords);
			}
			if (firstUNProcessedUNNOs.Count == 0)
			{
				return results.Distinct();
			}
			Console.WriteLine("Re-Process failed " + firstUNProcessedUNNOs.Count + " records");
			Thread.Sleep(1000);

			var lastUNProcessedUNNOs = new Dictionary<string, string>();
			totalNumberOfRecords = firstUNProcessedUNNOs.Count;
			numberOfRecordsProcessed = 0;
			var unProcessedWithIndex = firstUNProcessedUNNOs.Select((unno, index) => new { unno = unno.Key, index });

			foreach (var uNNOBatch in unProcessedWithIndex.GroupBy(g => g.index / Constants.FetchPSAGroupBatchSize, i => i.unno))
			{
				await AddPSARecordsToListInBatchAsync(uNNOBatch, webScraper, results, lastUNProcessedUNNOs);
				numberOfRecordsProcessed += uNNOBatch.Count();
				DisplayProgress((numberOfRecordsProcessed - lastUNProcessedUNNOs.Count), totalNumberOfRecords);
			}

			if (lastUNProcessedUNNOs.Count == 0)
			{
				return results.Distinct();
			}
			Console.WriteLine("Single-Process remaining " + lastUNProcessedUNNOs.Count + " records");
			Thread.Sleep(1000);

			var remainingUNNOs = new Dictionary<string, string>();
			var errorUNNOS = new Dictionary<string, string>();
			foreach (var uNNOBatch in lastUNProcessedUNNOs)
			{
				await AddPSARecordsToListInBatchAsync(new List<string> { uNNOBatch.Key }, webScraper, results, remainingUNNOs);
			}

			if (remainingUNNOs.Count == 0)
			{
				return results.Distinct();
			}
			Thread.Sleep(1000);
			if (remainingUNNOs.Count != 0)
			{
				foreach (var uNNOBatch in remainingUNNOs)
				{
					await AddPSARecordsToListInBatchAsync(new List<string> { uNNOBatch.Key }, webScraper, results, errorUNNOS);
				}
			}
			if (errorUNNOS.Count != 0)
			{
				foreach(var unno in errorUNNOS)
				{
					htmlForErrorUNNOs.Add($"UNNO: {unno.Key}");
					htmlForErrorUNNOs.Add(unno.Value);
				}
				var htmlContent = string.Join(Environment.NewLine, htmlForErrorUNNOs);
				throw new Exception("Error processing UNNO: " + string.Join(",", errorUNNOS.Keys) + ". Please check" + Environment.NewLine + htmlContent);
			}

			return results.Distinct();
		}

		static async Task AddPSARecordsToListInBatchAsync(IEnumerable<string> uNNOList, IWebScraper webScraper, List<PSARecord> pSARecords, Dictionary<string, string> unProcessedUNNOs)
		{
			var tasks = uNNOList.Select(async unno =>
			{
				var delay = CommonHelper.RandomInteger(1000, 2000);
				await Task.Delay(delay);
				await AddPSARecordsToListByUNNOAsync(webScraper, unno, pSARecords, unProcessedUNNOs);
			});

			await Task.WhenAll(tasks);
		}

		static async Task AddPSARecordsToListByUNNOAsync(IWebScraper webScraper, string uNNO, List<PSARecord> pSARecords, Dictionary<string, string> unProcessedUNNOs)
		{
			var uri = new Uri(string.Format(CultureInfo.InvariantCulture, Constants.DataSources.PSAGroupURL, uNNO));
			var html = await webScraper.ScrapeWithRetriesAsync(uri);
			if (html != null && html.Contains("table"))
			{
				var doc = new HtmlDocument();
				doc.LoadHtml(WebUtility.HtmlDecode(html));
				var table = doc.DocumentNode.SelectSingleNode("//table[@class='altrows']")?
				.Descendants("tr")?
				.Skip(1)
				.Where(tr => tr.Elements("td").Count() > 1)
				.Select(tr => tr.Elements("td").Select(td => td.InnerText.Trim()).ToList())
				.ToList();
				if (table != null)
				{
					foreach (var item in table)
					{
						var record = new PSARecord();
						record.UNNO = item[0].Trim();
						record.IMOClass = item[1].Trim();
						record.PSN = item[2].Trim();
						record.FlashPointLower = string.IsNullOrEmpty(item[3]) ? string.Empty : item[3].Trim();
						record.FlashPointUpper = string.IsNullOrEmpty(item[4]) ? string.Empty : item[4].Trim();
						record.PackagingGroup = item[5].Trim();
						record.PSAGroup = item[6].Trim();
						pSARecords.Add(record);
					}
				}
			}
			else
			{
				unProcessedUNNOs[uNNO]=html;
			}
		}

		public static IEnumerable<UNDGSubstance> MergePSARecordsToUNDGSubstances(IEnumerable<UNDGSubstance> substances, IEnumerable<PSARecord> pSARecords)
		{
			foreach (var substance in substances)
			{
				var result = new UNDGSubstance { DG_UNNO = substance.DG_UNNO, DG_Variant = substance.DG_Variant };
				var references = new List<UNDGReference>();
				var matchedPSARecords = pSARecords.Where(x => x.UNNO == substance.DG_UNNO && x.IMOClass == substance.DG_Class && (x.PackagingGroup == substance.DG_PG || x.PackagingGroup == "-") && PSNFuzzyMatch(substance.DG_PSN, x.PSN));
				if (matchedPSARecords.Any() && matchedPSARecords.Any())
				{
					foreach (var item in matchedPSARecords)
					{
						if (item != null)
						{
							references.Add(new UNDGReference
							{
								DR_Code = item.PSAGroup,
								DR_HasFlashPointLower = !string.IsNullOrEmpty(item.FlashPointLower),
								DR_FlashPointLower = !string.IsNullOrEmpty(item.FlashPointLower) ? decimal.Parse(item.FlashPointLower, CultureInfo.InvariantCulture) : 0,
								DR_HasFlashPointUpper = !string.IsNullOrEmpty(item.FlashPointUpper),
								DR_FlashPointUpper = !string.IsNullOrEmpty(item.FlashPointUpper) ? decimal.Parse(item.FlashPointUpper, CultureInfo.InvariantCulture) : 0,
							});
						}
					}
					result.UNDGReferences = references.ToArray();
					yield return result;
				}
			}
		}

		static void SetDefaultConnectionLimitAndProtocol()
		{
			ServicePointManager.DefaultConnectionLimit = Constants.FetchPSAGroupBatchSize;
			ServicePointManager.UseNagleAlgorithm = false;
		}

		static bool PSNFuzzyMatch(string pSNIMO, string pSNPSA)
		{
			pSNIMO = pSNIMO.Trim().ToUpper(CultureInfo.InvariantCulture).Replace(",", " ").Replace("S ", string.Empty).Replace(" ", string.Empty);
			pSNPSA = pSNPSA.Trim().ToUpper(CultureInfo.InvariantCulture).Replace(",", " ").Replace("S ", string.Empty).Replace(" ", string.Empty);
			return pSNPSA.Contains(pSNIMO);
		}

		static void DisplayProgress(int numberOfRecordsProcessed, int totalNumberOfRecords)
		{
			if (numberOfRecordsProcessed % Constants.FetchPSAGroupBatchSize == 0)
			{
				Console.WriteLine($"{numberOfRecordsProcessed} of {totalNumberOfRecords} records processed");
			}
		}
	}
}

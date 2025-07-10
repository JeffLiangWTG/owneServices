using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using CargoWise.RefDbRepo.Staging.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer;
using ApplicationConfig = CargoWise.RefDbRepo.NLReferenceData.Services.ApplicationConfig;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class TariffProcessManager : Services.TariffProcessManager
	{
		public TariffProcessManager()
		{
			ErrorCollector = new StringBuilder();
		}

		public TariffProcessManager(StringBuilder errorCollector)
		{
			ErrorCollector = errorCollector;
		}

		public void RunProcess()
		{
			var fileList = DownloadFileList(DownloadUrlTariff);
			var downloadEelements = ReadTariffDownloadElements(fileList);
			DownloadZipFiles(downloadEelements);
			var tariffFilesToBeProcessed = GetTariffZipFilesToBeProcessed(DownloadDir);
			var extractedFiles = ExtractTariffZipFiles(tariffFilesToBeProcessed, TariffOutputDirectory);

			var measureProcessor = new MeasureProcessor();
			var allMeasures = new List<Measure>();

			var conditionProcessor = new MeasureConditionProcessor();
			var allConditionCodes = new List<MeasureCondition>();

			foreach (var extractedFile in extractedFiles)
			{
				measureProcessor.LoadData(extractedFile.FullName);
				measureProcessor.UpdateModels();
				allMeasures.AddRange(measureProcessor.DutiesModels);
				allMeasures.AddRange(measureProcessor.VATModels);

				if (allConditionCodes.Count > 0 && extractedFile.Name.StartsWith(MeasureConditionCodeFileNamePrefix, StringComparison.InvariantCulture))
				{
					allConditionCodes.Clear();
				}
				conditionProcessor.LoadData(extractedFile.FullName);
				allConditionCodes.AddRange(conditionProcessor.MeasureConditionCodes);
			}
			var tariffsBuilder = new TariffsBuilder(ErrorCollector);
			var content = tariffsBuilder.ConvertMeasuresToRefCusTariff(allMeasures);
			if (MergeWithEUN)
			{
				content = MergeImportTariffsWithEUNTarrifs(content);
			}
			TariffsBuilder.GenerateUniversalReferenceDataXml(content, PublicationTime, Path.GetFullPath(ApplicationConfig.OutputPath));

			GenerateUniversalReferenceDataXml_Conditions(tariffFilesToBeProcessed, allConditionCodes);

			Cleanup();
		}

		void GenerateUniversalReferenceDataXml_Conditions(List<FileInfo> tariffFilesToBeProcessed, List<MeasureCondition> allConditionCodes)
		{
			string dateTimeFormat = "yyyy_MM_dd_HH_mm_ss_fff";
			var fileDate = DateTime.Now;
			var conditionBuilder = new TariffMeasureConditionsBuilder(ErrorCollector);
			var conditionContent = conditionBuilder.ConvertMeasureConditionsToRefCusConditionCode(allConditionCodes);

			foreach (var file in tariffFilesToBeProcessed)
			{
				var fileNameSplit = file.Name.Split(new char[] { '-', '.' });
				var fileNameDate = fileNameSplit[1];
				if (DateTime.TryParseExact(fileNameDate, dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out fileDate))
				{
					if (fileDate.DayOfWeek == DayOfWeek.Saturday || fileDate.DayOfWeek == DayOfWeek.Sunday)
					{
						break;
					}
				};
			}

			TariffMeasureConditionsBuilder.GenerateUniversalReferenceDataXml(conditionContent, fileDate, Path.GetFullPath(ApplicationConfig.OutputPath));
		}

		static List<RefCusTariff> MergeImportTariffsWithEUNTarrifs(List<RefCusTariff> refCusTariffData, EUNTariffExpander expander = null)
		{
			var tariffExpander = expander ?? new EUNTariffExpander();
			return tariffExpander.MergeImportTariffsWithEUNTarrifs(refCusTariffData, false);
		}

		protected virtual XmlDocument DownloadFileList(string downloadUrlTariff)
		{
			try
			{
				DownloadManagerHelper.PrepareEnvironment(DownloadDir);

				using (var downloader = DownloadManager)
				{
					return downloader.DownloadFileAsXmlDocument(downloadUrlTariff);
				}
			}
			catch (Exception ex)
			{
				ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for '{downloadUrlTariff}' Exception: {ex.GetBaseException().Message}");
				throw;
			}
		}

		protected virtual List<string> DownloadZipFiles(List<TariffDownloadElement> downloadElements)
		{
			try
			{
				DownloadManagerHelper.PrepareEnvironment(DownloadDir);

				using (var downloader = DownloadManager)
				{
					return downloader.DownloadNewFiles(downloadElements, DownloadDir);
				}
			}
			catch (Exception ex)
			{
				ErrorCollector.AppendLine(CultureInfo.InvariantCulture, $"Processing failure for downloading zip files. Exception: {ex.GetBaseException().Message}");
				throw;
			}
		}

		static readonly DownloadManager DownloadManager = new DownloadManager(new WebClientWrapper(new HttpClient() { Timeout = new TimeSpan(0, 0, ApplicationConfig.DownloadTimeoutInSeconds) }, new HttpClientRetryHandler(ApplicationConfig.DownloadMaxRetryAttempts, (int)new TimeSpan(0, 0, ApplicationConfig.DownloadRetryIntervalInSeconds).TotalMilliseconds)));

		protected virtual string DownloadUrlTariff => ApplicationConfig.DownloadUrlTariff;

		protected virtual DateTime PublicationTime => DateTime.Now;

		protected virtual bool MergeWithEUN => true;

		readonly StringBuilder ErrorCollector;

		const string MeasureConditionCodeFileNamePrefix = "MeasureConditionCode_";
	}
}

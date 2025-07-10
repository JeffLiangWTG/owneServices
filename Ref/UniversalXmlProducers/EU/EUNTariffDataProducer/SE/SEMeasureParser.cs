using System.Collections.Generic;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE
{
	public class SEMeasureParser : ISEMeasureParser
	{
		readonly ISEMeasureDownloader _downloader;
		measure[] _downloadedMeasures;

		public SEMeasureParser(ISEMeasureDownloader downloader)
		{
			_downloader = downloader;
		}

		public bool CanDownload()
		{
			if (_downloadedMeasures != null)
			{
				return true;
			}
			var fileRepositoryUrl = ApplicationConfig.SEDailyFileBaseUrl;
			var temporaryFileDownload = ApplicationConfig.DownloadsFolder;
			_downloadedMeasures = _downloader.DownloadLatestIncrementalAndExtract(fileRepositoryUrl, temporaryFileDownload);
			if (_downloadedMeasures == null)
			{
				return false;
			}
			return true;
		}

		public void Parse(List<IRawMeasureConditionRecord> measureConditionRecords, List<IRawMeasureExclusionRecord> exclusionConditionRecords)
		{
			if (_downloadedMeasures == null)
			{
				return;
			}
			var converter = new SEMeasureToRawRecordConverter(_downloadedMeasures);
			converter.AddRawMeasureConditionRecordList(measureConditionRecords);
			converter.AddRawMeasureExclusionRecordList(exclusionConditionRecords);
		}
	}
}

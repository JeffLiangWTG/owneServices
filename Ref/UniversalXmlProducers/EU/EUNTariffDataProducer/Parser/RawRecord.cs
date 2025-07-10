using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.SE;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public abstract class RawRecord : IRawRecord
	{
		public IEnumerable<IRawRateRecord> RateRecords { get; set; }
		public IEnumerable<IRawMeasureExclusionRecord> MeasureExclusionRecords { get; set; }
		public IEnumerable<IRawMeasureConditionRecord> MeasureConditionRecords { get; set; }
		public IEnumerable<IRawRateRecord> UomRecords { get; set; }

		protected RawRecord()
		{ }

		protected RawRecord(IDailyTariffRateParser dailyTariffRateParser, ISEMeasureParser seMeasureParser)
		{
			Argument.NotNull(dailyTariffRateParser, nameof(dailyTariffRateParser));
			Argument.NotNull(seMeasureParser, nameof(seMeasureParser));

			_dailyTariffRateParser = dailyTariffRateParser;
			_seMeasureParser = seMeasureParser;
			_tariffHeaderToProcess = new List<string>();
		}
		readonly IDailyTariffRateParser _dailyTariffRateParser;
		readonly ISEMeasureParser _seMeasureParser;
		readonly List<string> _tariffHeaderToProcess;

		public void Parse(IEnumerable<IWebFileInfo> webFileInfos)
		{
			var measureExclusionParser = new MeasureExclusionParser();
			var measureConditionParser = new MeasureConditionParser();
			var rawRateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();
			var rawRateRecords = new List<IRawRateRecord>();
			var rawTariffUOMRecords = new List<IRawRateRecord>();
			var rawMeasureExclusionRecords = new List<IRawMeasureExclusionRecord>();
			var rawMeasureConditionRecords = new List<IRawMeasureConditionRecord>();
			var rawMeasureConditionFromRateRecords = new List<IRawRateRecord>();
			var downloadsFolder = ApplicationConfig.DownloadsFolder;

			var allValidMeasureTypeIds = ValidMeasureTypeIdsForMeasureConditionInRate
				.Concat(ApplicationConfig.ValidMeasureTypeIds())
				.Concat(ApplicationConfig.AntiDumpingMeasureTypeIds)
				.Concat(ApplicationConfig.TariffUOMValidMeasureTypeIds)
				.ToHashSet();

			foreach (var webFileInfo in webFileInfos)
			{
				var filePath = Path.Combine(downloadsFolder, webFileInfo.FileName);
				if (!File.Exists(filePath))
				{
					filePath = Path.Combine(webFileInfo.DownloadPath, webFileInfo.FileName);
				}

#pragma warning disable CA1308 // Normalize strings to uppercase
				var fileName = webFileInfo.FileName.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
				var isImport = fileName.Contains("import");
				var rateParser = new RateParser(isImport);
				if (IsDutyFile(fileName))
				{
					var allDutiesFileRecords = rateParser.Parse(filePath);

					var unsupportedMeasureTypes = allDutiesFileRecords
						.Select(x => x.MeasureTypeId)
						.Where(x => !string.IsNullOrEmpty(x) && !allValidMeasureTypeIds.Contains(x))
						.Distinct()
						.Order()
						.ToList();
					if (unsupportedMeasureTypes.Any())
					{
						Console.WriteLine($"WARNING: ({fileName}) Measure types are not supported: {string.Join(", ", unsupportedMeasureTypes)}");
					}

					allDutiesFileRecords = FinalizeDutyFileParsing(allDutiesFileRecords);
					IncludeRawRateRecordsIntoRawRateRecordsListsByMeasureType(allDutiesFileRecords, rawRateRecords, rawMeasureConditionFromRateRecords, rawTariffUOMRecords);
				}
				else if (fileName.Contains("exclusion"))
				{
					rawMeasureExclusionRecords.AddRange(measureExclusionParser.Parse(filePath));
				}
				else if (fileName.Contains("condition"))
				{
					rawMeasureConditionRecords.AddRange(measureConditionParser.Parse(filePath));
				}
				else if (_dailyTariffRateParser != null)
				{
					_dailyTariffRateParser.IsImport = isImport;
					_dailyTariffRateParser.Parse(filePath);
				}
			}

			foreach (var rawMeasureConditionFromRateRecord in rawMeasureConditionFromRateRecords)
			{
				rawMeasureConditionRecords.AddRange(rawRateToRawMeasureConditionConverter.Convert(rawMeasureConditionFromRateRecord));
			}

			rawRateRecords.AddRange(rawRateToRawMeasureConditionConverter.Convert(rawMeasureConditionRecords));

			RateRecords = rawRateRecords;
			MeasureConditionRecords = rawMeasureConditionRecords;
			MeasureExclusionRecords = rawMeasureExclusionRecords;
			UomRecords = rawTariffUOMRecords;
		}

		protected virtual IEnumerable<IRawRateRecord> FinalizeDutyFileParsing(IEnumerable<IRawRateRecord> records) => records;

		public void ParseSEFileAndAttachTaricDailyRecords()
		{
			var rawMeasureExclusionRecords = new List<IRawMeasureExclusionRecord>();
			var rawMeasureConditionRecords = new List<IRawMeasureConditionRecord>();
			var rawRateRecords = new List<IRawRateRecord>();
			var rawTariffUOMRecords = new List<IRawRateRecord>();
			var rawMeasureConditionFromRateRecords = new List<IRawRateRecord>();
			var rawRateToRawMeasureConditionConverter = new RawRateToRawMeasureConditionConverter();

			if (!_seMeasureParser.CanDownload())
			{
				return;
			}
			_seMeasureParser.Parse(rawMeasureConditionRecords, rawMeasureExclusionRecords);

			var dailyRawRateRecords = IncludeRawRateRecordsIntoRawRateRecordsListsByMeasureType(_dailyTariffRateParser.GetParsedRecords(), rawRateRecords, rawMeasureConditionFromRateRecords, rawTariffUOMRecords, true);
			_tariffHeaderToProcess.AddRange(dailyRawRateRecords.Select(x => x.TariffHeader));

			foreach (var rawMeasureConditionFromRateRecord in rawMeasureConditionFromRateRecords)
			{
				rawMeasureConditionRecords.AddRange(rawRateToRawMeasureConditionConverter.Convert(rawMeasureConditionFromRateRecord));
			}

			rawRateRecords.AddRange(rawRateToRawMeasureConditionConverter.Convert(rawMeasureConditionRecords));

			MeasureConditionRecords = rawMeasureConditionRecords;
			MeasureExclusionRecords = rawMeasureExclusionRecords;
			RateRecords = rawRateRecords;
			UomRecords = rawTariffUOMRecords;
		}

		List<IRawRateRecord> IncludeRawRateRecordsIntoRawRateRecordsListsByMeasureType(IEnumerable<IRawRateRecord> records, List<IRawRateRecord> rawRateRecords, List<IRawRateRecord> rawMeasureConditionFromRateRecords, List<IRawRateRecord> rawTariffUOMRecords, bool isDailyRecords = false)
		{
			Argument.NotNull(records, nameof(records));
			Argument.NotNull(rawRateRecords, nameof(rawRateRecords));
			Argument.NotNull(rawMeasureConditionFromRateRecords, nameof(rawMeasureConditionFromRateRecords));
			Argument.NotNull(rawTariffUOMRecords, nameof(rawTariffUOMRecords));
			var result = new List<IRawRateRecord>();

			foreach (var record in records)
			{
				var measureTypeId = record.MeasureTypeId;

				if (string.IsNullOrEmpty(measureTypeId))
				{
					continue;
				}

				if (ApplicationConfig.ValidMeasureTypeIds().Contains(measureTypeId)
					|| ApplicationConfig.AntiDumpingMeasureTypeIds.Contains(measureTypeId))
				{
					if (isDailyRecords)
					{
						var duplicatedRecord = rawRateRecords.FirstOrDefault(x => RawRateRecordComparer.AreEqual(x, record));
						if (duplicatedRecord != null)
						{
							rawRateRecords.Remove(duplicatedRecord);
						}
					}
					result.Add(record);
					rawRateRecords.Add(record);
				}
				if (IsValidMeasureTypeIdsForMeasureConditionRate(measureTypeId)
					|| ApplicationConfig.AntiDumpingMeasureTypeIds.Contains(measureTypeId))
				{
					if (isDailyRecords)
					{
						var duplicatedRecord = rawMeasureConditionFromRateRecords.FirstOrDefault(x => RawRateRecordComparer.AreEqual(x, record));
						if (duplicatedRecord != null)
						{
							rawMeasureConditionFromRateRecords.Remove(duplicatedRecord);
						}
					}
					result.Add(record);
					rawMeasureConditionFromRateRecords.Add(record);
				}
				if (ApplicationConfig.TariffUOMValidMeasureTypeIds.Contains(measureTypeId))
				{
					if (isDailyRecords)
					{
						var duplicatedRecord = rawRateRecords.FirstOrDefault(x => RawRateRecordComparer.AreEqual(x, record));
						if (duplicatedRecord != null)
						{
							rawTariffUOMRecords.Remove(duplicatedRecord);
						}
					}
					result.Add(record);
					rawTariffUOMRecords.Add(record);
				}
			}
			return result;
		}

		public List<string> GetDailyTariffHeadersToProcess()
		{
			return _tariffHeaderToProcess?.Distinct().Select(x => x).ToList();
		}

		protected abstract bool IsDutyFile(string fileName);

		bool IsValidMeasureTypeIdsForMeasureConditionRate(string measureTypeId)
		{
			return ValidMeasureTypeIdsForMeasureConditionInRate?.Contains(measureTypeId) ?? false;
		}

		protected abstract IEnumerable<string> ValidMeasureTypeIdsForMeasureConditionInRate { get; }
	}
}

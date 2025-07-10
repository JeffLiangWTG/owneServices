using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class TariffDataProducer : ITariffDataProducer
	{
		readonly IRawRecord parsedRecords;
		readonly ITariffGenerator tariffGenerator;
		readonly ITariffMerger tariffMerger;

		public TariffDataProducer(IRawRecord parsedRecords,
			ITariffGenerator tariffGenerator,
			ITariffMerger tariffMerger)
		{
			Argument.NotNull(parsedRecords, nameof(parsedRecords));
			Argument.NotNull(tariffGenerator, nameof(tariffGenerator));
			Argument.NotNull(tariffMerger, nameof(tariffMerger));

			this.parsedRecords = parsedRecords;
			this.tariffGenerator = tariffGenerator;
			this.tariffMerger = tariffMerger;
		}

		public IEnumerable<RefCusTariff> LoadAllTariffRates()
		{
			var rawRateRecords = parsedRecords.RateRecords;
			var rawMeasureExclusionRecords = parsedRecords.MeasureExclusionRecords;
			var rawMeasureConditionRecords = parsedRecords.MeasureConditionRecords;
			var rawUomRecords = parsedRecords.UomRecords;

			var rawTariffs = GetRawTariffs(rawRateRecords, rawMeasureExclusionRecords, rawMeasureConditionRecords, rawUomRecords);
			var actualTariffs = tariffMerger.ConvertToActualTariffs(rawTariffs, null, rawUomRecords);

			return actualTariffs;
		}

		public IEnumerable<RefCusTariff> LoadTariffRates(IEnumerable<IWebTariffHeader> tariffHeaders)
		{
			Argument.NotNull(tariffHeaders, nameof(tariffHeaders));
			var tariffHeadersOfApplicableRate = new List<string>();
			foreach (var tariffHeader in tariffHeaders)
			{
				var tariffCode = tariffHeader.TariffCode;
				tariffHeadersOfApplicableRate.AddRange(GetTariffHeadersOfApplicableRate(tariffCode));
			}

			var rawRateRecords = parsedRecords.RateRecords.Where(x => tariffHeadersOfApplicableRate.Contains(x.TariffHeader));
			var rawMeasureExclusionRecords = parsedRecords.MeasureExclusionRecords.Where(x => tariffHeadersOfApplicableRate.Contains(x.TariffHeader));
			var rawMeasureConditionRecords = parsedRecords.MeasureConditionRecords.Where(x => tariffHeadersOfApplicableRate.Contains(x.TariffHeader));
			var rawUomRecords = parsedRecords.UomRecords.Where(x => tariffHeadersOfApplicableRate.Contains(x.TariffHeader));

			var rawTariffs = GetRawTariffs(rawRateRecords, rawMeasureExclusionRecords, rawMeasureConditionRecords, rawUomRecords);
			var actualTariffs = tariffMerger.ConvertToActualTariffs(rawTariffs, tariffHeaders, rawUomRecords);

			return actualTariffs;
		}

		static IEnumerable<string> GetTariffHeadersOfApplicableRate(string tariffHeader)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));

			var tariffHeadersOfApplicableRate = new List<string>
			{
				tariffHeader
			};

			for (var lastIndexOf00 = 8; lastIndexOf00 > 0; lastIndexOf00 -= 2)
			{
				var lastIndex = lastIndexOf00;
				tariffHeader = tariffHeader.Substring(0, lastIndex) + "00";
				var normalizedCode = EUNUtils.NormalizeTariffCode(tariffHeader);

				if (!tariffHeadersOfApplicableRate.Contains(normalizedCode))
				{
					tariffHeadersOfApplicableRate.Add(normalizedCode);
				}
			}

			return tariffHeadersOfApplicableRate;
		}

		IEnumerable<RefCusTariff> GetRawTariffs(IEnumerable<IRawRateRecord> rawRateRecords, IEnumerable<IRawMeasureExclusionRecord> rawMeasureExclusionRecords, IEnumerable<IRawMeasureConditionRecord> rawMeasureConditionRecord, IEnumerable<IRawRateRecord> rawUomRecords)
		{
			Argument.NotNull(rawRateRecords, nameof(rawRateRecords));
			Argument.NotNull(rawMeasureExclusionRecords, nameof(rawMeasureExclusionRecords));
			Argument.NotNull(rawMeasureConditionRecord, nameof(rawMeasureConditionRecord));
			Argument.NotNull(rawUomRecords, nameof(rawUomRecords));

			var tariffRateRecords = rawRateRecords.GroupBy(x => x.TariffHeader);

			var rawTariffs = new List<RefCusTariff>();

			var unprocessed = rawMeasureConditionRecord.ToList();
			foreach (var tariffRateRecord in tariffRateRecords)
			{
				var measureExclusionRecords = rawMeasureExclusionRecords.Where(x => x.TariffHeader == tariffRateRecord.Key);
				var measureConditionRecords = rawMeasureConditionRecord.Where(x => x.TariffHeader == tariffRateRecord.Key).ToList();
				var uomRecords = rawUomRecords.Where(x => tariffRateRecord.Key.StartsWith(x.TariffHeader.TrimEnd('0'), StringComparison.Ordinal));

				foreach (var measureConditionRecord in measureConditionRecords)
				{
					unprocessed.Remove(measureConditionRecord);
				}

				var tariffHeader = tariffRateRecord?.Key;
				if (!string.IsNullOrEmpty(tariffHeader))
				{
					var rawTariff = tariffGenerator.GenerateRawTariff(tariffHeader, tariffRateRecord.ToList(), measureExclusionRecords, measureConditionRecords, uomRecords);
					rawTariffs.Add(rawTariff);
				}
			}

			if (unprocessed.Any())
			{
				var groupedMeasureConditionRecords = unprocessed.GroupBy(x => x.TariffHeader);
				rawTariffs.AddRange(groupedMeasureConditionRecords.Select(
					rawMeasureConditionRecords => tariffGenerator.GenerateRawTariffWithonlyMeasureConditions
					(
						rawMeasureConditionRecords.Key,
						rawMeasureConditionRecords,
						rawMeasureExclusionRecords.Where(y => y.TariffHeader == rawMeasureConditionRecords.Key),
						rawUomRecords.Where(y => rawMeasureConditionRecords.Key.StartsWith(y.TariffHeader.TrimEnd('0'), StringComparison.Ordinal))
					)));
			}

			return rawTariffs;
		}
	}
}

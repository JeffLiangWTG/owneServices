using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class TariffDataValidator
	{
		public TariffDataValidator(ILogger logger)
		{
			this.logger = logger;
		}

		public void ValidateChapter(Dictionary<string, List<RefCusTariff>> tariffsDictionary)
		{
			var missingChapters = new List<string>();
			for (var c = 1; c <= 98; c++)
			{
				var chapter = c.ToString("D2", CultureInfo.InvariantCulture);
				if (!tariffsDictionary.ContainsKey(chapter))
				{
					missingChapters.Add(chapter);
				}
			}
			if (missingChapters.Count > 0)
			{
				logger.Log(LogType.ReviewRequired, "Missing chapters in Tariff data", $"Chapters: {string.Join(", ", missingChapters)}");
			}
		}

		public void Validate(List<TariffDataItem> items, string fileName = "")
		{
			if (items.Count == 0)
			{
				logger.Log(LogType.ReviewRequired, "No data found in PDF");
				return;
			}

			foreach (var groupItems in items.GroupBy(x => GetTariffItemClean(x.TariffItem)).Where(x => !string.IsNullOrEmpty(x.Key) && x.Count() > 1))
			{
				logger.Log(LogType.ReviewRequired, "Duplicate TariffItem", $"Code: {groupItems.Key}, Count: {groupItems.Count()}");
			}

			var typoItems = items.Where(x => x.TariffItem.Length >= 2).GroupBy(x => x.TariffItem[..2]).OrderByDescending(g => g.Count()).Skip(1).SelectMany(g => g.ToList()).ToList();
			if (typoItems.Count != 0)
			{
				logger.Log(LogType.ReviewRequired, $"Possible typos in {fileName} with tariff codes {string.Join(", ", typoItems.Select(x => x.TariffItem).Order())}");
			}

			foreach (var row in items)
			{
				ValidateRow(row);
			}
		}

		void ValidateRow(TariffDataItem row)
		{
			logger.Log(LogType.Info, "Validating: Row data", row);
			var tariffItemClean = GetTariffItemClean(row.TariffItem);
			var invalidDataErrorBuilder = GetErrors(row, tariffItemClean);
			var warningsBuilder = GetWarnings(row, tariffItemClean);

			if (warningsBuilder.Length > 0)
			{
				logger.Log(LogType.Warning, "Possible Invalid data", warningsBuilder);
			}

			if (invalidDataErrorBuilder.Length > 0)
			{
				logger.Log(LogType.ReviewRequired, "Invalid data", invalidDataErrorBuilder);
			}

			if (row.Description.Contains("#omitted"))
			{
				logger.Log(LogType.ReviewRequired, "Description contains #omitted", $"TariffItem: {row.TariffItem}, Description: {row.Description}");
			}
		}

		static string GetTariffItemClean(string rawTariffItem)
		{
			return Regex.Replace(rawTariffItem, Constants.Tariff.Validation.Patterns.SymbolsInTariffItemPattern, "");
		}

		static StringBuilder GetWarnings(TariffDataItem row, string tariffItemClean)
		{
			var builder = new StringBuilder();
			if (!Regex.IsMatch(row.TariffItem, Constants.Tariff.Pdf.Patterns.TariffItemOptionalRawPattern))
			{
				builder.Append("TariffItem ");
			}
			if (!string.IsNullOrEmpty(row.Unit) && tariffItemClean.Length < Constants.Tariff.Validation.DeclarableTariffCodeLength)
			{
				builder.Append("Unit ");
			}
			if ((string.IsNullOrEmpty(row.StandardRate) && tariffItemClean.Length == Constants.Tariff.Validation.DeclarableTariffCodeLength) || (!string.IsNullOrEmpty(row.StandardRate) && tariffItemClean.Length < Constants.Tariff.Validation.DeclarableTariffCodeLength) || Regex.IsMatch(row.StandardRate, Constants.Tariff.Validation.Patterns.StandardRateWithHyphenPattern))
			{
				builder.Append("StandardRate ");
			}
			if ((string.IsNullOrEmpty(row.PreferentialRate) && tariffItemClean.Length == Constants.Tariff.Validation.DeclarableTariffCodeLength) || (!string.IsNullOrEmpty(row.PreferentialRate) && tariffItemClean.Length < Constants.Tariff.Validation.DeclarableTariffCodeLength))
			{
				builder.Append("PreferentialRate ");
			}

			return builder;
		}

		static StringBuilder GetErrors(TariffDataItem row, string tariffItemClean)
		{
			var builder = new StringBuilder();
			if (!Regex.IsMatch(tariffItemClean, Constants.Tariff.Validation.Patterns.TariffItemCleanPattern))
			{
				builder.Append("TariffItem ");
			}
			if (!Regex.IsMatch(row.Hyphens, Constants.Tariff.Validation.Patterns.HyphensPattern))
			{
				builder.Append("Hyphens ");
			}
			if (string.IsNullOrWhiteSpace(row.Description))
			{
				builder.Append("Description ");
			}
			if (string.IsNullOrEmpty(row.Unit) && tariffItemClean.Length == Constants.Tariff.Validation.DeclarableTariffCodeLength)
			{
				builder.Append("Unit ");
			}

			return builder;
		}

		readonly ILogger logger;
	}
}

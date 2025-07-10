using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public abstract class ExciseDutyRatePageReader
	{
		protected ExciseDutyRatePageReader(IApplicationConfig config)
		{
			Config = config;
		}

		public abstract string Url { get; }

		public abstract string Description { get; }

		public abstract RuntimeDataRecorder.Keys LogKey { get; }

		public abstract ExciseDutyRatePageData[] GetFromPageRow(ExtractedHtmlTable htmlTable);

		public abstract (bool Succeed, decimal Rate, string Formula) GetFormula(ExciseDutyRatePageData pageData);

		public DateTime PagePublishDate { get; private set; }

		protected IApplicationConfig Config { get; }

		protected static bool TryGetRate(string rateText, out decimal rate)
		{
			const string rateRegex = @"€(\d+(\.\d+)?)";
			rate = 0m;

			var rateRegexMatch = Regex.Match(rateText, rateRegex);
			var result = rateRegexMatch.Success && decimal.TryParse(rateRegexMatch.Groups[1].Value, out rate);

			return result;
		}

		protected static string ToRateString(decimal rate) =>
			(decimal.GetBits(rate)[3] >> 16) > 2 ? rate.ToString("0.0000", CultureInfo.InvariantCulture) : rate.ToString("0.00", CultureInfo.InvariantCulture);

		protected static DateTime? TryGetPublicationDate(HtmlNode documentNode)
		{
			var dateNode = documentNode.SelectSingleNode(ApplicationConfig.Instance.ExciseDuty_PublishDateXpath);
			if (dateNode != null)
			{
				var dateText = dateNode.InnerText.Trim().Replace("Published:", string.Empty);
				if(dateText.TryGetDate(out var dateTime))
				{
					return dateTime;
				}
			}
			return default;
		}

		protected static int GetValidLength(string[] row) => row.Count(cell => !string.IsNullOrWhiteSpace(cell));

		public static IReadOnlyCollection<ExciseDutyRatePageData> ReadAll(IApplicationConfig config)
		{
			var result = new List<ExciseDutyRatePageData>();

			var readers = new List<ExciseDutyRatePageReader>
			{
				new ExciseDutyRatePageReaderMineralOil(config),
				new ExciseDutyRatePageReaderAlcoholProducts(config),
				new ExciseDutyRatePageReaderTobaccoProducts(config),
			};

			foreach (var reader in readers)
			{
				var lastPublishDate = RuntimeDataRecorder.Read(reader.LogKey) is DateTime dateTime ? dateTime : DateTime.MinValue;

				var pageResult = HtmlTableLoader.Read(reader.Url, TryGetPublicationDate).GetAwaiter().GetResult();
				var pagePublishDate = pageResult.AdditionalResult;

				if (pagePublishDate.HasValue && pagePublishDate.Value.Date > lastPublishDate.Date)
				{
					foreach (var table in pageResult.ValueTables)
					{
						result.AddRange(reader.GetFromPageRow(table));
					}
					reader.PagePublishDate = pagePublishDate.Value;
				}
			}

			return result.ToArray();
		}
	}

	public class ExciseDutyRatePageData
	{
		public ExciseDutyRatePageData(string searchKey, string taxRate, string[] pageRow, ExciseDutyRatePageReader reader, decimal tolerableSimilarity)
		{

			SearchText = searchKey;
			TaxRate = taxRate;
			TolerableSimilarity = tolerableSimilarity;
			PageRow = pageRow;
			Reader = reader;
		}

		public ExciseDutyRatePageData(string searchKey, string taxRate, string[] pageRow, ExciseDutyRatePageReader reader) : this(searchKey, taxRate, pageRow, reader, 0.8m) { }

		public string SearchText { get; }
		public string TaxRate { get; }
		public decimal TolerableSimilarity { get; }
		public string[] PageRow { get; }

		public (bool Success, decimal Rate, string Formula) Formula => Reader.GetFormula(this);
		public ExciseDutyRatePageReader Reader { get; }
	}
}

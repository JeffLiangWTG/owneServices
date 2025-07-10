using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public class ExciseDutyRatePageReaderTobaccoProducts : ExciseDutyRatePageReader
	{
		public ExciseDutyRatePageReaderTobaccoProducts(IApplicationConfig config) : base(config)
		{
		}

		public override string Url => Config.ExciseDuty_Url_Tobacco_Products;

		public override string Description => Constants.ExciseDutyRate.AlcoholProductsTax;

		public override RuntimeDataRecorder.Keys LogKey => RuntimeDataRecorder.Keys.ExciseDutyRatePublicateDate_Tobacco_Products;

		public override (bool, decimal, string) GetFormula(ExciseDutyRatePageData pageData)
		{
			var text = pageData.PageRow[1];
			string unit = null;
			if (TryGetRate(text, out var rate))
			{
				if (text.Contains("per kilogram"))
				{
					unit = "[KGM]";
				}
				else if (text.Contains("per thousand"))
				{
					if (text.Contains("price at which the cigarettes are sold by retail"))
					{
						unit = "[RSP]";
					}
					else
					{
						unit = "[MIL]";
					}
				}

			}
			if (unit != null)
			{
				return (true, rate, $"{ToRateString(rate)} * {unit}");
			}
			else
			{
				return (false, rate, string.Join(" ", pageData.PageRow));
			}
		}

		public override ExciseDutyRatePageData[] GetFromPageRow(ExtractedHtmlTable htmlTable)
		{
			var result = new List<ExciseDutyRatePageData>();

			foreach (var row in htmlTable.Body.Select(row => row).Where(row => GetValidLength(row) > 1))
			{
				var description = Regex.Replace(row[1], @"[\r\n]", " ");
				var subTitlePattern = new Regex(@"(\(\w\)) ([^()]+)(?:\s*or\s*|$)");
				var ratePattern = new Regex(@"€\d+(\.\d+)?");

				var subTitles = subTitlePattern.Matches(description);
				if (subTitles.Any(m => m.Success))
				{
					result.AddRange(subTitles.Select(t => new ExciseDutyRatePageData(
						$"{row[0]} - {t.Groups[1].Value}",
						ratePattern.Match(t.Value).Value,
						row,
						this
					)));
				}
				else
				{
					result.Add(new ExciseDutyRatePageData(row[0], row[1], row, this));
				}
			}

			return result.ToArray();
		}
	}
}

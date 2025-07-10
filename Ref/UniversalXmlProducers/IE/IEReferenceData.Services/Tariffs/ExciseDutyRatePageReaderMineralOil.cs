using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public class ExciseDutyRatePageReaderMineralOil : ExciseDutyRatePageReader
	{
		public ExciseDutyRatePageReaderMineralOil(IApplicationConfig config) : base(config)
		{
		}

		public override string Url => Config.ExciseDuty_Url_Mineral_Oil;

		public override string Description => Constants.ExciseDutyRate.MineralOilTax;

		public override RuntimeDataRecorder.Keys LogKey => RuntimeDataRecorder.Keys.ExciseDutyRatePublicateDate_Mineral_Oil;

		public override (bool, decimal, string) GetFormula(ExciseDutyRatePageData pageData)
		{
			var rateText = pageData.SearchText.Contains("- Carbon") ? pageData.PageRow[3] : pageData.PageRow[2];
			if (TryGetRate(rateText, out var rate))
			{
				return (true, rate, $"{ToRateString(rate)} * [HLT]");
			}
			else
			{
				return (false, default, string.Join(" ", pageData.PageRow));
			}
		}

		public override ExciseDutyRatePageData[] GetFromPageRow(ExtractedHtmlTable htmlTable)
		{
			var result = new List<ExciseDutyRatePageData>();

			foreach (var row in htmlTable.Body.Select(row => row).Where(row => GetValidLength(row) > 4))
			{
				result.Add(new ExciseDutyRatePageData($"{row[0]} - {row[1]}", row[2], row, this));
				result.Add(new ExciseDutyRatePageData($"{row[0]} - {row[1]} - Carbon", row[3], row, this));
			}

			return result.ToArray();
		}
	}
}

using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public class ExciseDutyRatePageReaderAlcoholProducts : ExciseDutyRatePageReader
	{
		public ExciseDutyRatePageReaderAlcoholProducts(IApplicationConfig config) : base(config)
		{
		}

		public override string Url => Config.ExciseDuty_Url_Alcohol_Products;

		public override string Description => Constants.ExciseDutyRate.AlcoholProductsTax;

		public override RuntimeDataRecorder.Keys LogKey => RuntimeDataRecorder.Keys.ExciseDutyRatePublicateDate_Alcohol_Products;

		public override (bool, decimal, string) GetFormula(ExciseDutyRatePageData pageData)
		{
			var result = new List<string>();

			var rateText = pageData.PageRow[2];
			if (TryGetRate(rateText, out var rate))
			{
				rate /= 100;
				result.Add(ToRateString(rate));

				if (rateText.Contains("per litre"))
				{
					result.Add("[LPA]");
				}
				else if (rateText.Contains("per hectolitre"))
				{
					result.Add("[LTR]");
					if (rateText.Contains("per cent of alcohol"))
					{
						result.Add("[ASV%]");
					}
				}
			}

			return result.Any()
				? (true, rate, string.Join(" * ", result.ToArray()))
				: (false, rate, string.Join(" ", pageData.PageRow));
		}

		public override ExciseDutyRatePageData[] GetFromPageRow(ExtractedHtmlTable htmlTable)
		{
			var result = new List<ExciseDutyRatePageData>();
			var validRows = htmlTable.Body.Select(row => row).Where(row => GetValidLength(row) >= 2).ToArray();
			foreach (var row in validRows)
			{
				var searchKey = string.IsNullOrWhiteSpace(row[1]) ? row[0] : $"{row[0]} - {row[1]}";
				result.Add(new ExciseDutyRatePageData(searchKey, row[2], row, this));
			}
			return result.ToArray();
		}
	}
}

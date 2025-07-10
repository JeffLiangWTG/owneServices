using System;
using System.Collections.Generic;
using System.Text;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	internal class ConcessionToTariff
	{
		public ConcessionToTariff(string line)
		{
			var lineSplit = line.Split(Constants.TariffSplit);
			if (lineSplit.Length != 7)
			{
				throw new RefDataParseException($"Can't process data line. Line string: {line}");
			}

			Tariff = GetTariff(lineSplit);
			Code = GetCode(lineSplit);
			(Section, Chapters) = GetSectionAndChapters(lineSplit);
		}

		public string Tariff { get; }
		public string Code { get; }
		public int Section { get; }
		public string[] Chapters { get; }


		public static List<ConcessionToTariff> GetConcessionToTariffs(IEnumerable<string> concessionToTariffLines, ILogger logger)
		{
			var concessionRatesList = new List<ConcessionToTariff>();
			foreach (var concessionToTariffLine in concessionToTariffLines)
			{
				try
				{
					concessionRatesList.Add(new ConcessionToTariff(concessionToTariffLine));
				}
				catch (RefDataParseException e)
				{
					logger.LogError($"Error during parsing, ConcessionToTariff skipped: {e.Message}");
				}
			}

			return concessionRatesList;
		}

		static (int, string[]) GetSectionAndChapters(string[] lineSplit)
		{
			var sectionValue = lineSplit[6];

			if (int.TryParse(sectionValue, out var section))
			{
				if(Helper.SectionToChapters.TryGetValue(section, out var chapters))
				{
					return (section, chapters);
				}

				return (section, Array.Empty<string>());
			}

			return (0, Array.Empty<string>());
		}

		static string GetCode(string[] lineSplit) => lineSplit[0];

		static string GetTariff(string[] lineSplit)
		{
			var tariffBuilder = new StringBuilder();
			for (int i = 1; i < 6; i++)
			{
				var tariffLevelValue = lineSplit[i];
				if (tariffLevelValue == "**")
				{
					break;
				}

				tariffBuilder.Append(tariffLevelValue);
			}

			return tariffBuilder.ToString();
		}
	}
}

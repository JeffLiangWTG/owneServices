using System;
using System.Linq;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	class TariffRate
	{
		public string FactorA { get; }
		public string FactorB { get; }
		public string FormulaCode { get; }
		public string RateGroup { get; }
		public string TariffCode { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }


		public TariffRate(string line)
		{
			var parts = line.Trim().Split(Constants.TariffSplit);
			ValidateParts(parts, line);

			FactorA = parts[10];
			FactorB = parts[11];
			FormulaCode = parts[9];
			RateGroup = parts[5];
			TariffCode = string.Concat(parts.Take(5));

			if (!DateTime.TryParse(parts[6], out var startDate))
			{
				throw new RefDataParseException($"Failed to parse StartDate ('{parts[6]}')");
			}
			if (!DateTime.TryParse(parts[7], out var endDate))
			{
				throw new RefDataParseException($"Failed to parse EndDate ('{parts[7]}')");
			}

			StartDate = startDate < Constants.MinSmallDateTime ? Constants.MinSmallDateTime : startDate;
			EndDate = endDate > Constants.MaxSmallDateTime ? Constants.MaxSmallDateTime : endDate;
		}


		private static void ValidateParts(string[] parts, string source)
		{
			if (parts.Length != 16)
			{
				throw new RefDataParseException(
					$"Invalid line format: expected 16 fields but got {parts.Length}");
			}
		}
	}

}

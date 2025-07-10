using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	internal class ConcessionRates
	{
		public ConcessionRates(string line, ILogger logger = null)
		{
			Line = line;
			var lineSplit = line.Split(Constants.TariffSplit);
			if (lineSplit.Length != 11)
			{
				throw new RefDataParseException($"Can't process data line. Line string: {line}");
			}

			Code = GetCode(lineSplit);
			ExpiryDate = GetExpiryDate(lineSplit);
			RateGroup = GetRateGroup(lineSplit);

			try
			{
				(FormulaNumber, Formula) = GetFormula(lineSplit);
			}
			catch (RefDataParseException e) {
				FormulaNumber = 0;
				Formula = "0";
				logger?.LogError($"Error during parsing formula: {e.Message}");
			}
		}

		public DateTime ExpiryDate { get; }
		public string Code { get; }
		public string RateGroup { get; }
		public int FormulaNumber { get; }
		public string Formula{ get; }
		string Line { get; }

		public static ILookup<string, ConcessionRates> GetConcessionRates(IEnumerable<string> concessionRates, ILogger logger, IDateProvider dateProvider)
		{
			var dict = new Dictionary<string, ConcessionRates>();
			foreach (var concessionRate in concessionRates)
			{
				try
				{
					var rate = new ConcessionRates(concessionRate, logger);
					var codeWithTradeGroup = rate.Code + rate.RateGroup;
					if (dict.TryGetValue(codeWithTradeGroup, out var existingRate))
					{
						if (existingRate.ExpiryDate < rate.ExpiryDate)
						{
							dict[codeWithTradeGroup] = rate;
						}
					}
					else if (rate.ExpiryDate >= dateProvider.ActiveDate)
					{
						dict.Add(codeWithTradeGroup, rate);
					}
				}
				catch (RefDataParseException e)
				{
					logger.LogError($"Error during parsing, ConcessionRates skipped: {e.Message}");
				}
			}

			return dict.Values.ToLookup(x => x.Code, x => x);
		}

		(int, string) GetFormula(string[] lineSplit)
		{
			var formulaNumberString = lineSplit[4];
			var factorAString = lineSplit[5];
			var factorBString = lineSplit[6];
			if (!int.TryParse(formulaNumberString, out var formulaNumber))
			{
				throw new RefDataParseException($"Unable to parse formula number. Line string: {Line}");
			}

			string formula;
			switch (formulaNumber)
			{
				case 1:
				case 2:
					formula = "0";
					break;
				case 3:
					formula = $"{Helper.GetFactorString(factorAString, true, Line)}*VFD";
					break;
				case 4:
					formula = $"{Helper.GetFactorString(factorAString, false, Line)}*CU1";
					break;
				case 5:
					formula = $"({Helper.GetFactorString(factorAString, true, Line)}*VFD)+({Helper.GetFactorString(factorBString, false, Line)}*CU1)";
					break;
				case 6:
					formula = $"({Helper.GetFactorString(factorAString, false, Line)}*[{{0}}])-({Helper.GetFactorString(factorBString, true, Line)}*VFD)";
					break;
				default:
					throw new RefDataParseException($"Unsupported formula number. Line string: {Line}");
			}

			return (formulaNumber, formula);
		}

		static string GetRateGroup(string[] lineSplit) => lineSplit[1];

		static string GetCode(string[] lineSplit) => lineSplit[0];

		DateTime GetExpiryDate(string[] lineSplit)
		{
			var dateString = lineSplit[2];
			if (DateTime.TryParse(dateString, out var result))
			{
				return result > Constants.MaxSmallDateTime ? Constants.MaxSmallDateTime : result;
			}

			throw new RefDataParseException($"Unable to parse expiry date. Line string: {Line}");
		}
	}
}

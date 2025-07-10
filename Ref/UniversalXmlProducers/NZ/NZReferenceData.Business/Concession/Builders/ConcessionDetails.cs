using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	internal class ConcessionDetails
	{
		public ConcessionDetails(string line)
		{
			Line = line;
			var lineSplit = line.Split(Constants.TariffSplit);
			if (lineSplit.Length != 3)
			{
				throw new RefDataParseException($"Can't process data line. Line string: {line}");
			}

			Code = GetCode(lineSplit);
			StartDate = GetStartDate(lineSplit);
			EndDate = GetEndDate(lineSplit);
		}

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string Code { get; }
		string Line { get; }

		public static Dictionary<string, ConcessionDetails> GetConcessionDetails(IEnumerable<string> concessionDetails, ILogger logger, IDateProvider dateProvider)
		{
			var concessionRatesDictionary = new Dictionary<string, ConcessionDetails>();
			foreach (var concessionDetail in concessionDetails)
			{
				try
				{
					var concessionDetailsParsed = new ConcessionDetails(concessionDetail);
					if (concessionDetailsParsed.EndDate >= dateProvider.ActiveDate)
					{
						if (concessionRatesDictionary.ContainsKey(concessionDetailsParsed.Code))
						{
							logger.LogError($"Error during parsing, duplicate ConcessionDetails line: {concessionDetail}");
						}
						else
						{
							concessionRatesDictionary.Add(concessionDetailsParsed.Code, concessionDetailsParsed);
						}
					}
				}
				catch (RefDataParseException e)
				{
					logger.LogError($"Error during parsing, ConcessionDetails skipped: {e.Message}");
				}
			}

			return concessionRatesDictionary;
		}

		static string GetCode(string[] lineSplit) => lineSplit[0];

		DateTime GetStartDate(string[] lineSplit)
		{
			var dateString = lineSplit[1];
			if (DateTime.TryParse(dateString, out var result))
			{
				return result < Constants.MinSmallDateTime ? Constants.MinSmallDateTime : result;
			}

			throw new RefDataParseException($"Unable to parse start date. Line string: {Line}");
		}

		DateTime GetEndDate(string[] lineSplit)
		{
			var dateString = lineSplit[2];
			if (DateTime.TryParse(dateString, out var result))
			{
				return result > Constants.MaxSmallDateTime ? Constants.MaxSmallDateTime : result;
			}

			throw new RefDataParseException($"Unable to parse end date. Line string: {Line}");
		}
	}
}

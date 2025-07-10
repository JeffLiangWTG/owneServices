using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class DBKTariffValidator
	{
		public DBKTariffValidator(ILogger logger)
		{
			this.logger = logger;
		}
		ILogger logger;

		public List<DBKTariff> ValidateAndRemoveInvalidData(List<DBKTariff> items)
		{
			var result = new List<DBKTariff>();
			if (items.Count == 0)
			{
				logger.Log(LogType.ReviewRequired, "No data found in PDF");
			}

			foreach (var row in items)
			{
				var errorMessage = ValidateRow(row);
				if (!errorMessage.IsNullOrEmpty())
				{
					logger.Log(LogType.ReviewRequired, $@"Invalid TariffItem, Code: {row.TariffCode},
Error: {errorMessage}");
				}
				else
				{
					result.Add(row);
				}
			}
			return result;
		}

		static string ValidateRow(DBKTariff row)
		{
			var errorMessageBuilder = new StringBuilder();
			if (!Regex.IsMatch(row.TariffCode, DBKTariffXmlProducer.TariffCodeRegex))
			{
				errorMessageBuilder.AppendLine("The tariff number is not a four-digit or six-digit number.");
			}
			if (row.TariffDescription.IsNullOrEmpty())
			{
				errorMessageBuilder.AppendLine("The tariff description is empty.");
			}
			if (!row.Unit.IsNullOrEmpty())
			{
				var (matchingUnit, factor) = DBKTariffXmlProducer.GetMatchingUnitAndFactor(row.Unit);
				if (matchingUnit.IsNullOrEmpty())
				{
					errorMessageBuilder.AppendLine("No matching unit can be found.");
				}
			}
			return errorMessageBuilder.ToString();
		}
	}
}

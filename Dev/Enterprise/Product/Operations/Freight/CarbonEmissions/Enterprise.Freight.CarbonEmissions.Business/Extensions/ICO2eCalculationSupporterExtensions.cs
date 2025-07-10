using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business;

public static class ICO2eCalculationSupporterExtensions
{
	public static List<string> ValidateInputsWithAdditionalSupporter(this ICO2eCalculationSupporter supporter)
	{
		if (supporter.AdditionalCalculationSupporters.Length == 0)
		{
			return supporter.ValidateInputs();
		}

		var result = supporter.ValidateInputs().Select(s => ReasonWithBusinessObjectName(s, supporter)).ToList();
		result.AddRange(supporter.AdditionalCalculationSupporters
			.Select(x => x.Supporter)
			.SelectMany(additionalSupporter => additionalSupporter.ValidateInputs()
				.Select(s => ReasonWithBusinessObjectName(s, additionalSupporter))));

		return result;
	}

	static string ReasonWithBusinessObjectName(string reason, ICO2eCalculationSupporter supporter)
	{
		return supporter is IBusiness bizo
			? bizo.HumanReadableName + ": " + reason
			: reason;
	}

	public static bool HasBeenCalculated(this ICO2eCalculationSupporter supporter, string type = default)
	{
		var status = supporter.GetCO2eStatus(type);
		return status == CO2eStatusList.Codes.Current || status == CO2eStatusList.Codes.Rejected;
	}

	public static decimal GetTotalAdditionalSupportersEmission(this ICO2eCalculationSupporter supporter)
	{
		return supporter.AdditionalCalculationSupporters
			.Where(x => x.Supporter.GetCO2eStatus() == CO2eStatusList.Codes.Current)
			.Sum(x => x.GetApportionedValue());
	}

	public static void ApplyAdditionalCO2e(this ICO2eCalculationSupporter hostSupporter, ICO2eCalculationSupporter additionalSupporter)
	{
		var additionalCalculationSupporter = hostSupporter.AdditionalCalculationSupporters
			.FirstOrDefault(x => ((BusinessObject)x.Supporter).PK == ((BusinessObject)additionalSupporter).PK);
		if (additionalCalculationSupporter.Supporter is null)
		{
			return;
		}

		if (hostSupporter.GetCO2eStatus() != CO2eStatusList.Codes.Pending)
		{
			return;
		}

		var currentHostEmission = hostSupporter.GetTotalCO2e();
		if (currentHostEmission == 0m)
		{
			return;
		}

		if (hostSupporter.AdditionalCalculationSupporters.All(x => x.Supporter.HasBeenCalculated()))
		{
			var totalAdditionalEmission = hostSupporter.GetTotalAdditionalSupportersEmission();
			hostSupporter.SetTotalCO2e(currentHostEmission + totalAdditionalEmission);
			hostSupporter.SetCO2eStatus(CO2eStatusList.Codes.Current);
			hostSupporter.RecordLog(CO2eEventType.Updated, hostSupporter.GetTotalCO2e().ToString());
		}
	}
}

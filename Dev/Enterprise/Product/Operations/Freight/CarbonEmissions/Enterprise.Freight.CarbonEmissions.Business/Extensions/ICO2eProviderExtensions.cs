using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public static class ICO2eProviderExtensions
	{
		public static ZString GetTotalCO2eForBinding(this ICO2eProvider co2eProvider, ZString type = default)
		{
			if (!co2eProvider.JobCO2eExists(type))
			{
				return ZString.Empty;
			}

			var status = co2eProvider.GetCO2eStatus(type);

			if (status == CO2eStatusList.Codes.Pending)
			{
				return Res.GetString("9a2010af-81f9-42eb-a4fc-611552c8d4e5", "Pending");
			}

			var totalCO2e = co2eProvider is ICO2eLegProvider legProvider ? legProvider.DynamicTotalCO2e : co2eProvider.GetTotalCO2e(type);

			if (status == CO2eStatusList.Codes.NotCalculated || status == CO2eStatusList.Codes.Rejected || totalCO2e == 0)
			{
				return ZString.Empty;
			}

			return CO2eHelper.GetFormattedCO2e(totalCO2e);
		}

		public static void UpdateCO2eStatusToNotCurrent(this ICO2eProvider cO2eProvider, CO2eStatusChangedReason reason = default, bool skip = false, ZString type = default)
		{
			if (skip || !cO2eProvider.JobCO2eExists(type))
			{
				return;
			}

			var oldStatus = cO2eProvider.GetCO2eStatus(type);
			if (oldStatus == CO2eStatusList.Codes.NotCurrent)
			{
				return;
			}

			cO2eProvider.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent, type);
			cO2eProvider.LogSTUEvent(oldStatus, CO2eStatusList.Codes.NotCurrent, reason);
		}

		public static void CheckCO2eStatus(this ICO2eProvider parent, ZPropertyInfo property, ZString type = default)
		{
			if (parent.GetCO2eStatus(type) == CO2eStatusList.Codes.NotCurrent)
			{
				property.AddWarning(Res.GetString("e43f2c0d-e74e-4607-a946-a6b4923314aa", "Greenhouse gas emissions recalculation is required because the input data has changed."));
			}
			else if (parent.GetCO2eStatus(type) == CO2eStatusList.Codes.Rejected)
			{
				property.AddWarning(Res.GetString("fb326805-e721-4d6a-9edb-20932f2426af", "The greenhouse gas emissions value could not be calculated."));
			}
		}

		public static bool SkipCO2eStatusCheck(this ICO2eProvider provider) => !provider.JobCO2eExists() || provider.GetCO2eStatus() == CO2eStatusList.Codes.NotCurrent;
	}
}

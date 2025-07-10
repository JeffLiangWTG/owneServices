using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public static class TemperatureHelper
	{
		public static ZString CheckRequiredTemperatures(IRequiredTemperature temperatureProvider)
		{
			switch (temperatureProvider.RequiredTemperatureUnit)
			{
				case Constants.Temperature.Centigrade:
				case Constants.Temperature.Fahrenheit:
					var absoluteZero = Constants.Temperature.Convert(0, Constants.Temperature.Kelvin, temperatureProvider.RequiredTemperatureUnit);
					if (temperatureProvider.RequiredTemperatureMinimum < absoluteZero)
					{
						return Res.GetString("8599ce7d-6e6c-45e1-ac07-81e2313a4d21",
							"Minimum temperature ({0}°{2}) is below the minimum possible temperature of absolute zero ({1}°{2}).",
							temperatureProvider.RequiredTemperatureMinimum,
							absoluteZero,
							temperatureProvider.RequiredTemperatureUnit);
					}

					if (temperatureProvider.RequiredTemperatureMinimum > temperatureProvider.RequiredTemperatureMaximum)
					{
						return Res.GetString("80eb8362-bc4e-4b0a-ac20-cd99031abde2",
							"Minimum temperature ({0}°{2}) cannot be higher than maximum temperature ({1}°{2}).",
							temperatureProvider.RequiredTemperatureMinimum,
							temperatureProvider.RequiredTemperatureMaximum,
							temperatureProvider.RequiredTemperatureUnit);
					}

					return ZString.Empty;
				default:
					return Res.GetString("4700139a-56a5-4040-ac52-87ae551ca0fa",
						"Invalid temperature unit ({0}). Temperature must be set to C (Celsius) or F (Fahrenheit).",
						temperatureProvider.RequiredTemperatureUnit);
			}
		}
	}
}

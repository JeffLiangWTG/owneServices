using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public static class ITemperatureSettingsExtensions
	{
		#region Temperatures

		const decimal ZeroDegreesCentigrade = 0m;
		const decimal ZeroDegreesCentigradeInFahrenheit = 32m;

		const decimal IsChillerTemperatureCentigrade = 5m;
		const decimal IsChillerTemperatureFahrenheit = 41m;

		const decimal IsFrozenTemperatureCentigrade = -5m;
		const decimal IsFrozenTemperatureFahrenheit = 23m;

		#endregion

		#region IsChiller

		public static bool IsChiller(this ITemperatureSettings temperature)
		{
			return temperature.IsTemperatureControlled &&
				(
					(temperature.IsFahrenheit() && temperature.TemperatureMin >= ZeroDegreesCentigradeInFahrenheit) ||
					(!temperature.IsFahrenheit() && temperature.TemperatureMin >= ZeroDegreesCentigrade)
				);
		}

		public static void SetIsChiller(this ITemperatureSettings temperature, bool isChiller)
		{
			temperature.SetTemperatureFlag(isChiller, IsChillerTemperatureFahrenheit, IsChillerTemperatureCentigrade);
		}

		#endregion

		#region IsFreezer

		public static bool IsFreezer(this ITemperatureSettings temperature)
		{
			return temperature.IsTemperatureControlled &&
				(
					(temperature.IsFahrenheit() && temperature.TemperatureMin < ZeroDegreesCentigradeInFahrenheit) ||
					(!temperature.IsFahrenheit() && temperature.TemperatureMin < ZeroDegreesCentigrade)
				);
		}

		public static void SetIsFreezer(this ITemperatureSettings temperature, bool isFreezer)
		{
			temperature.SetTemperatureFlag(isFreezer, IsFrozenTemperatureFahrenheit, IsFrozenTemperatureCentigrade);
		}

		#endregion

		static void SetTemperatureFlag(this ITemperatureSettings temperature, bool isTemperatureControlled, ZDecimal fahrenheitTemperature, ZDecimal centigradeTemperature)
		{
			temperature.IsTemperatureControlled = isTemperatureControlled;
			if (isTemperatureControlled)
			{
				if (temperature.IsFahrenheit())
				{
					temperature.TemperatureMin = fahrenheitTemperature;
					temperature.TemperatureMax = fahrenheitTemperature;
				}
				else
				{
					temperature.TemperatureMin = centigradeTemperature;
					temperature.TemperatureMax = centigradeTemperature;
					temperature.TemperatureUnit = Core.Constants.Temperature.Centigrade;
				}
			}
		}

		static bool IsFahrenheit(this ITemperatureSettings temperature)
		{
			return temperature.TemperatureUnit.EqualsIgnoringCase(Core.Constants.Temperature.Fahrenheit);
		}
	}
}

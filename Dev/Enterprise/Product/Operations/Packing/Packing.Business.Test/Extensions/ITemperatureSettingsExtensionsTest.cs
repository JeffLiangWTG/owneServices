using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	public class ITemperatureSettingsExtensionsTest : TestCase
	{
		#region TestIsChillerAndSetIsChiller

		public void TestIsChillerAndSetIsChiller()
		{
			AssertIsChillerAndSetIsChiller(new DummyITemperatureSettings());
		}

		internal static void AssertIsChillerAndSetIsChiller(ITemperatureSettings temperatureSettings)
		{
			AssertEquals(false, temperatureSettings.IsChiller());

			// set temperature control to true

			temperatureSettings.IsTemperatureControlled = true;
			AssertEquals(true, temperatureSettings.IsChiller());

			// set a frozen temperature

			temperatureSettings.TemperatureMin = -1;
			AssertEquals("Temperature was below 0C, should not be a Chiller.", false, temperatureSettings.IsChiller());

			// test with Farhenheit

			temperatureSettings.TemperatureUnit = Core.Constants.Temperature.Fahrenheit;
			temperatureSettings.TemperatureMin = 31;
			AssertEquals(false, temperatureSettings.IsChiller());

			temperatureSettings.TemperatureMin = 32; // == 0 degrees celcius
			AssertEquals(true, temperatureSettings.IsChiller());

			// set IsChiller directly

			temperatureSettings.SetIsChiller(true);
			AssertEquals(true, temperatureSettings.IsChiller());
			AssertEquals("Setting IsChiller should set the Temperature to 41F (5C).", 41m, temperatureSettings.TemperatureMin);
			AssertEquals("Setting IsChiller should set the Temperature to 41F (5C).", 41m, temperatureSettings.TemperatureMax);
			AssertEquals("Setting IsChiller should not change the Temperature Unit.", Core.Constants.Temperature.Fahrenheit, temperatureSettings.TemperatureUnit);

			temperatureSettings.TemperatureUnit = "";
			temperatureSettings.SetIsChiller(true);
			AssertEquals("Setting IsChiller should default the Temperature Unit to C.", Core.Constants.Temperature.Centigrade, temperatureSettings.TemperatureUnit);

			temperatureSettings.SetIsChiller(false);
			AssertEquals(false, temperatureSettings.IsChiller());
		}

		#endregion

		#region TestIsFreezerAndSetIsFreezer

		public void TestIsFreezerAndSetIsFreezer()
		{
			AssertIsFreezerAndSetIsFreezer(new DummyITemperatureSettings());
		}

		internal static void AssertIsFreezerAndSetIsFreezer(ITemperatureSettings temperatureSettings)
		{
			AssertEquals(false, temperatureSettings.IsFreezer());

			// set temperature control to true

			temperatureSettings.IsTemperatureControlled = true;
			AssertEquals(false, temperatureSettings.IsFreezer());

			// set a frozen temperature

			temperatureSettings.TemperatureMin = -1;
			AssertEquals("Temperature was below 0C, should be Frozen.", true, temperatureSettings.IsFreezer());

			// set a non-frozen temperature

			temperatureSettings.TemperatureMin = 0;
			AssertEquals("Temperature was 0C or higher, should not be Frozen.", false, temperatureSettings.IsFreezer());

			// test with Farhenheit

			temperatureSettings.TemperatureUnit = Enterprise.Core.Constants.Temperature.Fahrenheit;
			temperatureSettings.TemperatureMin = 32;
			AssertEquals(false, temperatureSettings.IsFreezer());

			temperatureSettings.TemperatureMin = 31; // below 0 degrees celcius
			AssertEquals(true, temperatureSettings.IsFreezer());

			// set IsFreezer directly

			temperatureSettings.SetIsFreezer(true);
			AssertEquals(true, temperatureSettings.IsFreezer());
			AssertEquals("Setting IsFreezer should set the Temperature to 23F (-5C).", 23m, temperatureSettings.TemperatureMin);
			AssertEquals("Setting IsFreezer should set the Temperature to 23F (-5C).", 23m, temperatureSettings.TemperatureMax);
			AssertEquals("Setting IsFreezer should not change the Temperature Unit.", Enterprise.Core.Constants.Temperature.Fahrenheit, temperatureSettings.TemperatureUnit);

			temperatureSettings.TemperatureUnit = "";
			temperatureSettings.SetIsFreezer(true);
			AssertEquals("Setting IsFreezer should default the Temperature Unit to C.", Enterprise.Core.Constants.Temperature.Centigrade, temperatureSettings.TemperatureUnit);

			temperatureSettings.SetIsFreezer(false);
			AssertEquals(false, temperatureSettings.IsFreezer());
		}

		#endregion

		#region class DummyITemperatureSettings

		class DummyITemperatureSettings : ITemperatureSettings
		{
			public bool IsTemperatureControlled
			{
				get;
				set;
			}

			public ZDecimal TemperatureMin
			{
				get;
				set;
			}

			public ZDecimal TemperatureMax
			{
				get;
				set;
			}

			public ZString TemperatureUnit
			{
				get;
				set;
			}
		}

		#endregion
	}
}

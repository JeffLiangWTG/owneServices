using System.Collections.Generic;
using System.Linq;
using WTG.Glow.Data.Annotations;

namespace Enterprise.ProductionRules.Business.Testing
{
	class TemperatureConverterTest : UnitConverterTest
	{
		protected override IEnumerable<(decimal value, string fromUnit, string toUnit, decimal expected)> ConversionTestCases
		{
			get
			{
				yield return (10m, Core.Constants.Temperature.Centigrade, Core.Constants.Temperature.Fahrenheit, 50m);
				yield return (50m, Core.Constants.Temperature.Fahrenheit, Core.Constants.Temperature.Centigrade, 10m);
				yield return (0m, Core.Constants.Temperature.Centigrade, Core.Constants.Temperature.Fahrenheit, 32m);
				yield return (212m, Core.Constants.Temperature.Fahrenheit, Core.Constants.Temperature.Centigrade, 100m);
			}
		}

		protected override MeasurePropertyType MeasurePropertyType => MeasurePropertyType.Temperature;

		protected override decimal ConversionAssertionDelta => 0.000000000000000000000001m;

		protected override IEnumerable<string> ValidUnits => Core.Constants.Temperature.Codes;

		protected override IEnumerable<string> AdditionalInvalidUnits => Core.Constants.Volume.Codes.Concat(Core.Constants.Length.Codes);
	}
}

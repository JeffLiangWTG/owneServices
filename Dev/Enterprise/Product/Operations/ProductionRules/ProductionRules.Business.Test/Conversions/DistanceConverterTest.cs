using System.Collections.Generic;
using System.Linq;
using WTG.Glow.Data.Annotations;

namespace Enterprise.ProductionRules.Business.Testing
{
	sealed class DistanceConverterTest : UnitConverterTest
	{
		protected override IEnumerable<(decimal value, string fromUnit, string toUnit, decimal expected)> ConversionTestCases
		{
			get
			{
				yield return (1m, Core.Constants.Distance.Kilometres, Core.Constants.Distance.Miles, 0.621371192237334m);
				yield return (100m, Core.Constants.Distance.Miles, Core.Constants.Distance.Kilometres, 160.9344m);
				yield return (1m, Core.Constants.Distance.NauticalMiles, Core.Constants.Distance.Kilometres, 1.852m);
				yield return (1.234m, Core.Constants.Distance.NauticalMiles, Core.Constants.Distance.Miles, 1.42006183886105m);
				yield return (12345.678m, Core.Constants.Distance.Miles, Core.Constants.Distance.NauticalMiles, 10728.1008721555m);
				yield return (12345678m, Core.Constants.Distance.Kilometres, Core.Constants.Distance.NauticalMiles, 6666132.82937365m);
			}
		}
		protected override MeasurePropertyType MeasurePropertyType => MeasurePropertyType.Distance;

		protected override IEnumerable<string> ValidUnits => Core.Constants.Distance.Codes;

		protected override IEnumerable<string> AdditionalInvalidUnits => Core.Constants.Weight.Codes.Concat(Core.Constants.Volume.Codes);
	}
}

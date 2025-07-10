using System.Collections.Generic;
using System.Linq;
using WTG.Glow.Data.Annotations;

namespace Enterprise.ProductionRules.Business.Testing
{
	sealed class LengthConverterTest : UnitConverterTest
	{
		protected override IEnumerable<(decimal value, string fromUnit, string toUnit, decimal expected)> ConversionTestCases
		{
			get
			{
				yield return (1m, Core.Constants.Length.Metres, Core.Constants.Length.Centimetres, 100m);
				yield return (100m, Core.Constants.Length.Centimetres, Core.Constants.Length.Metres, 1m);
				yield return (1m, Core.Constants.Length.Metres, Core.Constants.Length.Millimetres, 1000m);
				yield return (1.234m, Core.Constants.Length.Metres, Core.Constants.Length.Millimetres, 1234m);
				yield return (12345.678m, Core.Constants.Length.Metres, Core.Constants.Length.Millimetres, 12345678m);
				yield return (12345678m, Core.Constants.Length.Millimetres, Core.Constants.Length.Metres, 12345.678m);
				yield return (1.2345678m, Core.Constants.Length.Millimetres, Core.Constants.Length.Metres, 0.0012345678m);
				yield return (1m, Core.Constants.Length.Inches, Core.Constants.Length.Centimetres, 2.54m);
				yield return (1m, Core.Constants.Length.Kilometres, Core.Constants.Length.Miles, 0.621371192237334m);
				yield return (100m, Core.Constants.Length.Miles, Core.Constants.Length.Kilometres, 160.9344m);
			}
		}
		protected override MeasurePropertyType MeasurePropertyType => MeasurePropertyType.Length;

		protected override IEnumerable<string> ValidUnits => Core.Constants.Length.Codes;

		protected override IEnumerable<string> AdditionalInvalidUnits => Core.Constants.Weight.Codes.Concat(Core.Constants.Volume.Codes);
	}
}

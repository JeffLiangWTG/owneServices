using System.Collections.Generic;
using System.Linq;
using WTG.Glow.Data.Annotations;

namespace Enterprise.ProductionRules.Business.Testing
{
	class VolumeConverterTest : UnitConverterTest
	{
		protected override IEnumerable<(decimal value, string fromUnit, string toUnit, decimal expected)> ConversionTestCases
		{
			get
			{
				yield return (1m, Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicDecimetres, 1000m);
				yield return (1000m, Core.Constants.Volume.CubicDecimetres, Core.Constants.Volume.CubicMetres, 1m);
				yield return (1m, Core.Constants.Volume.CubicYards, Core.Constants.Volume.CubicFeet, 26.9999999929371m);
				yield return (26.9999999929371m, Core.Constants.Volume.CubicFeet, Core.Constants.Volume.CubicYards, 1m);
				yield return (1m, Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicInches, 61023.7440947323m);
				yield return (3.14m, Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicDecimetres, 3140m);
				yield return (1m, Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicCentimeters, 1000000m);
			}
		}

		protected override MeasurePropertyType MeasurePropertyType => MeasurePropertyType.Volume;

		protected override IEnumerable<string> ValidUnits => Core.Constants.Volume.Codes;

		protected override IEnumerable<string> AdditionalInvalidUnits => Core.Constants.Weight.Codes.Concat(Core.Constants.Length.Codes);
	}
}

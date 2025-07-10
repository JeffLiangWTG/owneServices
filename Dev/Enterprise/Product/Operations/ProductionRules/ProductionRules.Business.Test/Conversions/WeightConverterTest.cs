using System.Collections.Generic;
using System.Linq;
using WTG.Glow.Data.Annotations;

namespace Enterprise.ProductionRules.Business.Testing
{
	class WeightConverterTest : UnitConverterTest
	{
		protected override IEnumerable<(decimal value, string fromUnit, string toUnit, decimal expected)> ConversionTestCases
		{
			get
			{
				yield return (1m, Core.Constants.Weight.Kilograms, Core.Constants.Weight.Grams, 1000m);
				yield return (1m, Core.Constants.Weight.MetricCarat, Core.Constants.Weight.Kilograms, 0.0002m);
				yield return (1m, Core.Constants.Weight.Pounds, Core.Constants.Weight.Grams, 453.59237m);
				yield return (1000m, Core.Constants.Weight.Grams, Core.Constants.Weight.Kilograms, 1m);
				yield return (10m, Core.Constants.Weight.Kilograms, Core.Constants.Weight.Pounds, 22.0462262184878m);
				yield return (2500000m, Core.Constants.Weight.Grams, Core.Constants.Weight.Tonnes, 2.5m);
				yield return (1234567m, Core.Constants.Weight.Grams, Core.Constants.Weight.Tonnes, 1.234567m);
				yield return (1m, Core.Constants.Weight.Ounces, Core.Constants.Weight.Kilograms, 0.0283495231m);
			}
		}

		protected override MeasurePropertyType MeasurePropertyType => MeasurePropertyType.Weight;

		protected override IEnumerable<string> ValidUnits => Core.Constants.Weight.Codes;

		protected override IEnumerable<string> AdditionalInvalidUnits => Core.Constants.Volume.Codes.Concat(Core.Constants.Length.Codes);
	}
}

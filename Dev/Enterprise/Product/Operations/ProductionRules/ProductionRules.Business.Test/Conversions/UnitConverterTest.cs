using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WTG.Glow.Data.Annotations;

namespace Enterprise.ProductionRules.Business.Testing
{
	abstract class UnitConverterTest : TestCase
	{
		public void TestIsValid_True()
		{
			var converter = new UnitConverter();
			foreach (var unit in ValidUnits)
			{
				AssertEquals($"{unit} should be valid.", true, converter.IsValidUnit(MeasurePropertyType, unit));
			}
		}

		public void TestIsValid_False()
		{
			var converter = new UnitConverter();
			foreach (var unit in new[] { null, string.Empty, "  ", "BLA", "BAD" }.Concat(AdditionalInvalidUnits))
			{
				AssertEquals($"{unit} should *not* be valid.", false, converter.IsValidUnit(MeasurePropertyType, unit));
			}
		}

		public void TestConvert()
		{
			var converter = new UnitConverter();
			foreach (var (value, fromUnit, toUnit, expected) in ConversionTestCases)
			{
				AssertEquals($"{value}{fromUnit} to {toUnit}.", expected, converter.Convert(MeasurePropertyType, value, fromUnit, toUnit), delta: ConversionAssertionDelta);
			}
		}

		protected virtual decimal ConversionAssertionDelta => 0m;

		public void TestConvert_Invalid()
		{
			var validUnit = ValidUnits.First();
			var invalidUnit = "BAD";

			var converter = new UnitConverter();
			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, string.Empty, string.Empty));
			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, null, null));
			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, invalidUnit, invalidUnit));

			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, validUnit, string.Empty));
			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, validUnit, null));
			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, validUnit, invalidUnit));

			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, string.Empty, validUnit));
			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, null, validUnit));
			AssertEquals("Invalid conversions should return 0m.", 0m, converter.Convert(MeasurePropertyType, 1m, invalidUnit, validUnit));
		}

		protected abstract MeasurePropertyType MeasurePropertyType { get; }
		protected abstract IEnumerable<(decimal value, string fromUnit, string toUnit, decimal expected)> ConversionTestCases { get; }
		protected abstract IEnumerable<string> ValidUnits { get; }
		protected abstract IEnumerable<string> AdditionalInvalidUnits { get; }
	}
}

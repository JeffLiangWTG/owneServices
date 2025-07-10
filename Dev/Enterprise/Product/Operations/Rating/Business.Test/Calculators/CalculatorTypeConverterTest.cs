using System;
using System.Linq;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Test
{
	public class CalculatorTypeConverterTest : TestCase
	{
		public void TestEnumToCode_CodeToEnum()
		{
			var values = Enum.GetValues(typeof(CalculatorType))
				.Cast<int>().ToArray();

			foreach (var codeGroup in values.GroupBy(x => CalculatorTypeConverter.EnumToCode((CalculatorType)x)))
			{
				var calcType = (CalculatorType)codeGroup.First();
				AssertEquals("code is unique " + codeGroup.Key, 1, codeGroup.Count());
				if (calcType != CalculatorType.None)
				{
					Assert("code is not empty for type " + Enum.GetName(typeof(CalculatorType), calcType), !string.IsNullOrEmpty(codeGroup.Key));
					AssertEquals("code converts back to same enum value", calcType, CalculatorTypeConverter.CodeToEnum(codeGroup.Key));
				}
			}

			AssertEquals("CalculatorType.None should be integer zero, the default value", 0, (int)(CalculatorType.None));

			AssertEquals(CalculatorType.None, CalculatorTypeConverter.CodeToEnum(null));
			AssertEquals(CalculatorType.None, CalculatorTypeConverter.CodeToEnum(string.Empty));
			AssertEquals(CalculatorType.None, CalculatorTypeConverter.CodeToEnum("???"));

			AssertNull("convert 0", CalculatorTypeConverter.EnumToCode(0));
			AssertNull("convert garbage", CalculatorTypeConverter.EnumToCode((CalculatorType)1000));
		}
	}
}

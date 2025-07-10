using System;
using System.Linq;
using System.Reflection;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	internal class ControlToCalculatorMapTest : TestCase
	{
		public void TestAllCalculatorsAreMapped()
		{
			var controlToCalculatorMap = new ControlToCalculatorMap();
			bool IsMissingCalculatorType(Type type)
			{
				var result = type.IsClass
					&& !type.IsAbstract
					&& type.IsSubclassOf(typeof(Calculator))
					&& type != typeof(NullCalculator)
					&& (controlToCalculatorMap.GetControlType(type) == null)
					&& !type.IsTestCalculator();

				return result;
			}

			var typesFromCalculatorAssembly = Assembly.GetAssembly(typeof(Calculator)).GetTypes();
			var missedCalculatorTypes = typesFromCalculatorAssembly.Where(IsMissingCalculatorType).ToArray();

			var errorMessage = "\r\nNo control has been defined for the following calculators\r\n"
				 + string.Join("\r\n", missedCalculatorTypes.Select(o => o.FullName));

			AssertEquals(errorMessage, 0, missedCalculatorTypes.Length);
		}

		public void TestRateCalculatorUserControlsAreMapped()
		{
			var controlToCalculatorMap = new ControlToCalculatorMap();
			bool IsMissingControl(Type type)
			{
				var result = type.IsClass
					&& !type.IsAbstract
					&& type.IsSubclassOf(typeof(RateCalculatorUserControl))
					&& (controlToCalculatorMap.GetCalculatorType(type) == null);

				return result;
			}

			var typesFromControl = Assembly.GetAssembly(typeof(RateCalculatorUserControl)).GetTypes();
			var missedControlTypes = typesFromControl.Where(IsMissingControl).ToArray();

			var errorMessage = "\r\nNo calculator has been defined for the following control\r\n"
				+ string.Join("\r\n", missedControlTypes.Select(o => o.FullName));

			AssertEquals(errorMessage, 0, missedControlTypes.Length);
		}
	}
}

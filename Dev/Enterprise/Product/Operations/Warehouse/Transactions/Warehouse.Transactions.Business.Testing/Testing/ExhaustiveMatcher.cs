using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class ExhaustiveMatcher
	{
		DynamicBusinessObject TestedObject { get; }
		readonly Dictionary<string, Action> AssertionDictionary = new Dictionary<string, Action>();

		public ExhaustiveMatcher(DynamicBusinessObject dynamicBizOUnderTest)
		{
			TestedObject = dynamicBizOUnderTest;
		}

		public void AddColumnMatcher(string columnName, object expectedValue)
		{
			AddColumnMatcher(columnName, columnName, expectedValue);
		}

		public void AddColumnMatcher(string message, string columnName, object expectedValue)
		{
			AddColumnMatcherCore(message, columnName, expectedValue, TestedObject[columnName]);
		}

		void AddColumnMatcherCore(string message, string columnName, object expectedValue, object actualValue)
		{
			AssertionDictionary[columnName] = () => Assertion.AssertEquals(message, expectedValue, actualValue);
		}

		public void AssertAll()
		{
			AssertionWithHtml.CombineAssertions("Assert all columns are asserted, and all values are matched.", () =>
			{
				foreach (var action in AssertionDictionary.Values)
				{
					action.Invoke();
				}
				Assertion.AssertContainsExactElementsInAnyOrder("All columns in the DynamicBusinessObject should be asserted.", TestedObject.PropertyNames, AssertionDictionary.Keys);
			});
		}
	}
}

using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

public static class DictionaryExtensions
{
	public static void AssertDictionaryValue(this IDictionary<string, decimal> dictionary, string key, decimal expectedValue)
	{
		if (dictionary.TryGetValue(key, out decimal actualValue))
		{
			Assertion.AssertEquals($"[{key}]", expectedValue, actualValue);
		}
		else
		{
			Assertion.Fail($"Key [{key}] not present in the dictionary");
		}
	}
}

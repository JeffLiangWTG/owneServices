using System;
using System.Collections.Generic;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AesRuleTestHelper : TestCase
{
	public static void TestR0089E(CusEntryInstruction entryInstruction, Func<object> getValue)
		=> CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._76;
			AssertNull("R0089E - 76 procedure code", getValue.Invoke());

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			AssertNotNull("R0089E - not 76 procedure code", getValue.Invoke());
		});

	public static void TestR0089E<T>(CusEntryInstruction entryInstruction, Func<IReadOnlyCollection<T>> getValue)
		where T : class
		=> CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._76;
			AssertEquals("R0089E - 76 procedure code", 0, getValue.Invoke().Count);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._48;
			AssertNotEquals("R0089E - not 76 procedure code", 0, getValue.Invoke().Count);
		});

	public static void TestItemsAreOrderedByRuleR0093E<T>(AesRuleHelper.RuleR0093E.Patterns pattern, T[] items, Action<T, string> setCode, Func<IEnumerable<string>> getActual) => CombineAssertions(() =>
	{
		setCode(items[0], "ABC");
		setCode(items[1], "DEF");
		AssertContainsExactElementsInExactOrder("The items have initial order.", new[] { "ABC", "DEF" }, getActual());

		var matchedCode = pattern switch
		{
			AesRuleHelper.RuleR0093E.Patterns.n1an2 => "1A2",
			AesRuleHelper.RuleR0093E.Patterns.n1an3 => "1A2B",
			AesRuleHelper.RuleR0093E.Patterns.a1an4 => "A1B2C",
			_ => throw new ArgumentException($"Invalid Value for {nameof(pattern)}: {pattern.ToString()}", nameof(pattern)),
		};
		setCode(items[0], matchedCode);
		AssertContainsExactElementsInExactOrder($"The order is changed by the rule R0093E using '{pattern}' format.", new[] { "DEF", matchedCode }, getActual());
	});
}

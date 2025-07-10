using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.AesRuleHelper;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AesRuleHelperTest : TestCaseWithFactory
{
	public void TestCheckRuleR0089E() => CombineAssertions(() =>
	{
		AssertEquals("Null passed", false, AesRuleHelper.CheckRuleR0089E(null));

		var entryInstruction = Factory.New<CusEntryInstruction>();
		AssertEquals("Procedure code is empty", false, AesRuleHelper.CheckRuleR0089E(entryInstruction));

		entryInstruction.CEI_Procedure = Constants.ProcedureCodes._76;
		AssertEquals("Procedure code is 76", true, AesRuleHelper.CheckRuleR0089E(entryInstruction));

		entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
		AssertEquals("Procedure code is not 76", false, AesRuleHelper.CheckRuleR0089E(entryInstruction));
	});

	public void TestHasSubStyleEqualsXorYorZ() => CombineAssertions(() =>
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_SubStyle = "A";
		AssertEquals("SubStyle is not X or Y or Z", false, entryInstruction.HasSubStyleEqualsXorYorZ());

		entryInstruction.CEI_SubStyle = "X";
		AssertEquals("SubStyle is X", true, entryInstruction.HasSubStyleEqualsXorYorZ());

		entryInstruction.CEI_SubStyle = "Y";
		AssertEquals("SubStyle is Y", true, entryInstruction.HasSubStyleEqualsXorYorZ());

		entryInstruction.CEI_SubStyle = "Z";
		AssertEquals("SubStyle is Z", true, entryInstruction.HasSubStyleEqualsXorYorZ());
	});

	public void TestHasSubStyleEqualsBorCorEorF() => CombineAssertions(() =>
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		entryInstruction.CEI_SubStyle = "A";
		AssertEquals("SubStyle is not B or C or E or F", false, entryInstruction.HasSubStyleEqualsBorCorEorF());

		entryInstruction.CEI_SubStyle = "B";
		AssertEquals("SubStyle is B", true, entryInstruction.HasSubStyleEqualsBorCorEorF());

		entryInstruction.CEI_SubStyle = "C";
		AssertEquals("SubStyle is C", true, entryInstruction.HasSubStyleEqualsBorCorEorF());

		entryInstruction.CEI_SubStyle = "E";
		AssertEquals("SubStyle is E", true, entryInstruction.HasSubStyleEqualsBorCorEorF());

		entryInstruction.CEI_SubStyle = "F";
		AssertEquals("SubStyle is F", true, entryInstruction.HasSubStyleEqualsBorCorEorF());
	});

	public void TestGetAdditionalInfosBySubtypeAndDistinctByCode() => CombineAssertions(() =>
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();

		var addtitionalInfo1 = entryInstruction.AdditionalInfos.AddNew();
		addtitionalInfo1.CSI_SubType = "REF";
		var addtitionalInfo2 = entryInstruction.AdditionalInfos.AddNew();
		addtitionalInfo2.CSI_SubType = "INF";
		addtitionalInfo2.CSI_Code = "CODE1";

		var items = entryInstruction.GetAdditionalInfosBySubtypeAndDistinctByCode("INF");
		AssertEquals("Filtered by subtype", true, items.Single().CSI_SubType == "INF");

		var additionalInfo3 = entryInstruction.AdditionalInfos.AddNew();
		additionalInfo3.CSI_SubType = "INF";
		additionalInfo3.CSI_Code = "CODE1";

		items = entryInstruction.GetAdditionalInfosBySubtypeAndDistinctByCode("INF");
		AssertEquals("Filtered by subtype but not distinct by code", 2, items.Count());

		items = entryInstruction.GetAdditionalInfosBySubtypeAndDistinctByCode("INF", new ZString[] { "CODE1" });
		AssertEquals("Filtered by subtype and distinct by code", 1, items.Count());
	});

	public void TestApplyC0050Rule() => CombineAssertions(() =>
	{
		AssertEquals("Identification number is not empty", null, ApplyC0050Rule("123", () => "Test String"));
		AssertEquals("Identification number is whitespace", "Test String", ApplyC0050Rule("   ", () => "Test String"));
		AssertEquals("Identification number is empty", "Test String", ApplyC0050Rule(string.Empty, () => "Test String"));
		AssertEquals("Identification number is null", "Test String", ApplyC0050Rule(null, () => "Test String"));
	});

	public void TestApplyE1104Rule() => CombineAssertions(() =>
	{
		const string longString = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
		AssertEquals("AES TP is off, lengthy string", longString, ApplyE1104Rule(longString, isAesTransitionPeriod: false));
		AssertEquals("AES TP is off, empty string", string.Empty, ApplyE1104Rule(string.Empty, isAesTransitionPeriod: false));
		AssertEquals("AES TP is off, null string", null, ApplyE1104Rule(null, isAesTransitionPeriod: false));
		AssertEquals("AES TP is on, lengthy string", "12345678901234567890123456789012345", ApplyE1104Rule(longString, isAesTransitionPeriod: true));
		AssertEquals("AES TP is on, empty string", null, ApplyE1104Rule(string.Empty, isAesTransitionPeriod: true));
		AssertEquals("AES TP is on, null string", null, ApplyE1104Rule(null, isAesTransitionPeriod: true));
	});

	public void TestOrderByRuleR0093E() => CombineAssertions(() =>
	{
		var pattern = RuleR0093E.Patterns.n1an2;
		var matchedCode1 = "1AB";
		var matchedCode2 = "123";
		AssertOrderBy("All items in the list have the original order.",
			initial: new[] { "ABC", "12", "1234" },
			expected: new[] { "ABC", "12", "1234" });
		AssertOrderBy("The matched item is moved to the end of the list.",
			initial: new[] { matchedCode1, "12", "1234" },
			expected: new[] { "12", "1234", matchedCode1 });
		AssertOrderBy("All matched items are moved to the end of the list",
			initial: new[] { matchedCode1, "12", matchedCode2, "1ABC" },
			expected: new[] { "12", "1ABC", matchedCode1, matchedCode2 });

		pattern = RuleR0093E.Patterns.n1an3;
		matchedCode1 = "1ABC";
		matchedCode2 = "2B11";
		AssertOrderBy("All items in the list have the original order.",
			initial: new[] { "AB11", "1AB", "1ABCD" },
			expected: new[] { "AB11", "1AB", "1ABCD" });
		AssertOrderBy("The matched item is moved to the end of the list.",
			initial: new[] { "AB11", matchedCode1, "1ABCD" },
			expected: new[] { "AB11", "1ABCD", matchedCode1 });
		AssertOrderBy("All matched items are moved to the end of the list",
			initial: new[] { "1AB", matchedCode1, "ABCD", matchedCode2, "1ABCD" },
			expected: new[] { "1AB", "ABCD", "1ABCD", matchedCode1, matchedCode2 });

		pattern = RuleR0093E.Patterns.a1an4;
		matchedCode1 = "A1B2C";
		matchedCode2 = "A12BC";
		AssertOrderBy("All items in the list have the original order.",
			initial: new[] { "AB12", "AB12AB", "1AB23" },
			expected: new[] { "AB12", "AB12AB", "1AB23" });
		AssertOrderBy("The matched item is moved to the end of the list.",
			initial: new[] { matchedCode1, "AB12AB", "1AB23" },
			expected: new[] { "AB12AB", "1AB23", matchedCode1 });
		AssertOrderBy("All matched items are moved to the end of the list",
			initial: new[] { "AB12", matchedCode1, matchedCode2, "1ABCD" },
			expected: new[] { "AB12", "1ABCD", matchedCode1, matchedCode2 });

		pattern = (RuleR0093E.Patterns)123;
		AssertOrderBy("All items in the list have the original order for wrong pattern.",
			initial: new[] { "1AB", "1ABC", "A1B23", "AB" },
			expected: new[] { "1AB", "1ABC", "A1B23", "AB" });

		void AssertOrderBy(string description, IEnumerable<string> initial, IEnumerable<string> expected)
		{
			var actual = initial.OrderBy(x => RuleR0093E.GetKeyEuLessThanPl(x, pattern));
			AssertContainsExactElementsInExactOrder($"Pattern '{pattern}': {description}", expected, actual);
		}
	});
}

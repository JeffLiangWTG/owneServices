using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class StringCheckerTest : TestCaseWithFactory
	{
		public void TestIsLettersAndNumbersAndSpaces()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Blank", true, StringChecker.IsLettersAndNumbersAndSpaces(""));
				AssertEquals("Space", true, StringChecker.IsLettersAndNumbersAndSpaces(" "));
				AssertEquals("Numbers", true, StringChecker.IsLettersAndNumbersAndSpaces("123"));
				AssertEquals("Letters", true, StringChecker.IsLettersAndNumbersAndSpaces("ASD"));
				AssertEquals("Wide Space", false, StringChecker.IsLettersAndNumbersAndSpaces("A　S　　D　W"));
				AssertEquals("Tab", false, StringChecker.IsLettersAndNumbersAndSpaces("AA\\tBB"));
				AssertEquals("Numbers and letters", true, StringChecker.IsLettersAndNumbersAndSpaces("456ASDZXC2"));
				AssertEquals("Space, Numbers and letters", true, StringChecker.IsLettersAndNumbersAndSpaces("456Z ASD 2"));
				AssertEquals("Symbols", false, StringChecker.IsLettersAndNumbersAndSpaces("!%"));
			});
		}

		public void TestReplaceSpecialCharactersWithSpaces()
		{
			CombineAssertions(() =>
			{
				AssertEquals("123", StringChecker.ReplaceSpecialCharactersWithSpaces("123"));
				AssertEquals("ASD", StringChecker.ReplaceSpecialCharactersWithSpaces("ASD"));
				AssertEquals("12 3", StringChecker.ReplaceSpecialCharactersWithSpaces("12!3"));
				AssertEquals("AS D 3", StringChecker.ReplaceSpecialCharactersWithSpaces("AS D!3"));
			});
		}

		public void TestIsValidCharactersForClassX()
		{
			CombineAssertions(() =>
			{
				AssertEquals(true, StringChecker.IsValidCharactersForClassX("123"));
				AssertEquals(true, StringChecker.IsValidCharactersForClassX("ABC"));
				AssertEquals(true, StringChecker.IsValidCharactersForClassX("ABC123"));
				AssertEquals(true, StringChecker.IsValidCharactersForClassX("123ABC"));
				AssertEquals(true, StringChecker.IsValidCharactersForClassX("12@!BC"));
			});
		}

		public void TestReplaceAsValidForClassX()
		{
			CombineAssertions(() =>
			{
				AssertEquals("1@23", StringChecker.ReplaceAsValidForClassX("1@23"));
				AssertEquals("AS#D", StringChecker.ReplaceAsValidForClassX("AS#D"));
				AssertEquals("12!3", StringChecker.ReplaceAsValidForClassX("12!3"));
			});
		}
	}
}

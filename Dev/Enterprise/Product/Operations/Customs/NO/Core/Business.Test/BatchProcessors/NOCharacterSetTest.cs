using Enterprise.Edifact;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOCharacterSet))]
sealed class NOCharacterSetTest : TestCase
{
	public void TestIsTypeOfUNOACharacterSet()
	{
		var isSubclass = typeof(NOCharacterSet).IsSubclassOf(typeof(UNOACharacterSet));
		AssertEquals("NOCharacterSet should inherit from UNOACharacterSet.", expected: true, isSubclass);
	}

	public void TestValidCharacters() => AssertEquals(
			"abcdefghijklmnopqrstuvwxyzæøåABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅàÀáÁäÄâÂèÈÊéêÉëËúÚüÜóÓöÖõÕ0123456789 .,-()/=\'+:=\\?!%&*;<>\"@#",
			NOCharacterSet.ValidCharacters);

	public void TestDelimiters() => CombineAssertions(() =>
	{
		var noCharSet = new NOCharacterSet();

		AssertEquals(":", noCharSet.SubElementDelimiter);
		AssertEquals('+', noCharSet.ElementDelimiterChar);
		AssertEquals('?', noCharSet.EscapeCharacterChar);
		AssertEquals('\'', noCharSet.SegmentDelimiterChar);
	});

	public void TestAllowsNonValidCharactersThatAreAcceptedByTheGateway()
	{
		var validCharacters = NOCharacterSet.ValidCharacters;
		var characterSet = new NOCharacterSet();

		CombineAssertions(() =>
		{
			foreach (var character in validCharacters)
			{
				var characterAsString = character.ToString();
				var formattedCharacter = characterSet.FormatElement(characterAsString);
				var expectedValue = IsAnEscapedCharacter(characterAsString)
					? "?" + characterAsString
					: characterAsString;

				AssertEquals($"Character: {characterAsString}", expectedValue, formattedCharacter);
			}
		});
	}

	static bool IsAnEscapedCharacter(string validCharacter) => (EscapedCharacters.IndexOf(validCharacter, System.StringComparison.OrdinalIgnoreCase) > -1);

	const string EscapedCharacters = @"'+:?";
}

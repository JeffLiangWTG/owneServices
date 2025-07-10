using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.BatchProcessor.Testing
{
	sealed class ZACharacterSetTest : TestCase
	{
		public void TestStripsInvalidChars()
		{
			string testInvalidCharactersareStrippedString = "\"!#$%^&(*)><,;[]\\/{}_-=~`.-'+:?@ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			AssertEquals("The Invalid Characters are not being stripped, special chars escaped", "\"!  % &(*)><,;   /   -=  .-?'?+?:?? ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", charSet.FormatElement(testInvalidCharactersareStrippedString));
		}

		public void TestValidChars()
		{
			AssertEquals("Valid chars", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,-()/='+:=?\"!%&*;<>", charSet.ValidCharacters_Exposed);
		}

		public void TestReplaceEscapedCharactersWithSpace()
		{
			AssertEquals("Should not replace with space", false, charSet.ReplaceEscapedCharactersWithSpace);
		}

		protected override void SetUp()
		{
			charSet = new ZACharacterSetForTest();
		}
		ZACharacterSetForTest charSet;
	}

	sealed class ZACharacterSetForTest : ZACharacterSet
	{
		public string ValidCharacters_Exposed => validCharacters;
	}
}

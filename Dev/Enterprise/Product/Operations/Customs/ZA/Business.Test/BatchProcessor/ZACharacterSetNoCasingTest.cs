using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.BatchProcessor.Testing
{
	sealed class ZACharacterSetNoCasingTest : TestCase
	{
		public void TestStripsInvalidChars()
		{
			string testInvalidCharactersareStrippedString = "\"!#$%^&(*)><,;[]\\/{}_-=~`.-'+:?@ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			AssertEquals("The Invalid Characters are not being stripped", "\"!  % &(*)><,;   /   -=  .-?'?+?:?? ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", charSet.FormatElement(testInvalidCharactersareStrippedString));
		}

		protected override void SetUp()
		{
			charSet = new ZACharacterSetNoCasing();
		}
		ZACharacterSetNoCasing charSet;
	}
}

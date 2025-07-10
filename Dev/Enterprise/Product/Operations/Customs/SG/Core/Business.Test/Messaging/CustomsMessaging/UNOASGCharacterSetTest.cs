using NUnit.Framework;

namespace Enterprise.Edifact.Testing
{
	public class UNOASGCharacterSetTest : TestCase
	{
		protected UNCharacterSet charSet;
		protected override void SetUp()
		{
			charSet = new UNOACharacterSet();
		}

		public void TestAllowsNonValidCharactersThatAreAcceptedByTheGateway()
		{
			string sGVAlidCharacters = UNOASGCharacterSet.SingaporeValidCharacters + ":+'?";
			for (int currentCharacterIndex = 0; currentCharacterIndex < sGVAlidCharacters.Length - 1; currentCharacterIndex++)
			{
				string aValidCharacter = sGVAlidCharacters[currentCharacterIndex].ToString();
				if (IsAnEscapedCharacter(aValidCharacter))
				{
					AssertEquals("Not allowing valid characters", "?" + aValidCharacter, new UNOASGCharacterSet().FormatElement(aValidCharacter));
				}
				else
				{
					AssertEquals("Not allowing valid characters", aValidCharacter, new UNOASGCharacterSet().FormatElement(aValidCharacter));
				}
			}
		}

		public void TestHashCharacterIsAccepted()
		{
			string sgAdditionalVAlidCharacters = UNOASGCharacterSet.SingaporeValidCharacters;
			Assert("Hashtag character '#' is acceptable to SingaporeCustoms", sgAdditionalVAlidCharacters.Contains("#"));
		}

		#region Implementation
		bool IsAnEscapedCharacter(string validCharacter)
		{
			bool escapedCharacter = false;
			if (EscapedCharacters.IndexOf(validCharacter) > -1)
			{
				escapedCharacter = true;
			}

			return escapedCharacter;
		}

		const string EscapedCharacters = @"'+:?";
		#endregion
	}
}

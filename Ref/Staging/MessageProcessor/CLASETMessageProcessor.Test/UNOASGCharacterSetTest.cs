using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor.Test
{
	[TestFixture]
	class UNOASGCharacterSetTest
	{
		[Test]
		public void TestAllowsNonValidCharactersThatAreAcceptedByTheGateway()
		{
			string sGVAlidCharacters = UNOASGCharacterSet.SingaporeValidCharacters + ":+'?";
			for (int currentCharacterIndex = 0; currentCharacterIndex < sGVAlidCharacters.Length - 1; currentCharacterIndex++)
			{
				string aValidCharacter = sGVAlidCharacters[currentCharacterIndex].ToString();
				if (IsAnEscapedCharacter(aValidCharacter))
				{
					Assert.AreEqual("?" + aValidCharacter, new UNOASGCharacterSet().FormatElement(aValidCharacter));
				}
				else
				{
					Assert.AreEqual(aValidCharacter, new UNOASGCharacterSet().FormatElement(aValidCharacter));
				}
			}
		}

		public void TestHashCharacterIsAccepted()
		{
			string sgAdditionalVAlidCharacters = UNOASGCharacterSet.SingaporeValidCharacters;
			Assert.AreEqual("Hashtag character '#' is acceptable to SingaporeCustoms", sgAdditionalVAlidCharacters.Contains("#"));
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

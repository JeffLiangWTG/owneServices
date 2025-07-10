using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using static Enterprise.MasterFiles.Business.ContactPasswordValidator;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ContactPasswordValidatorTest : TestCaseWithFactory
	{
		public void TestIsValidPasswordComplexity()
		{
			WebDataRegistry.Instance.WebPasswordMinLength.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 13);
			var (isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "1aA!56789012");
			Assert("Password should be too short", !isSuccess);
			AssertEquals("Password must be at least 13 characters long.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "1a!abacusabacus");
			Assert("Password is missing upper case letter", !isSuccess);
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "1A!ABACUSABACUS");
			Assert("Password is missing lower case letter", !isSuccess);
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa!abacusabacus");
			Assert("Password is missing numeral", !isSuccess);
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1abacusabacus");
			Assert("Password is missing special character", !isSuccess);
			AssertEquals("Password must contain at least one lower case letter, one upper case letter, one number and one special character.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abacusaba");
			Assert("Should be a valid password", isSuccess);
			AssertEquals("Password is valid.", message);
		}

		public void TestIsValidPassword_FullName()
		{
			var (isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abacusabacu", new List<string> { "wise tech" });
			Assert("Password should be valid", isSuccess);
			AssertEquals("Password is valid.", message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~wseicusetchcu");
			Assert("Password should be valid", isSuccess);
			AssertEquals("Password is valid.", message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~awiseusabacu", new List<string> { "wise tech" });
			Assert("Password contains a word from the user name", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaWISEusabacu", new List<string> { "wise tech" });
			Assert("Password contains a word from the user name (not case sensitive)", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaTECHusabacu", new List<string> { "wise tech" });
			Assert("Password contains a word from the user name", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			using (WebDataRegistry.Instance.EnableWebPasswordNameAndEmailValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~awiseusabacu", new List<string> { "wise tech" });
				Assert("Should not validate name when registry key is not enabled", isSuccess);
				AssertEquals("Password is valid.", message);
			}
		}

		public void TestIsValidPassword_Email()
		{
			var (isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abacusabacu", null, new List<string> { "wise.tech@email.com" });
			Assert("Password should be valid", isSuccess);
			AssertEquals("Password is valid.", message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abacusabacu");
			Assert("Password should be valid", isSuccess);
			AssertEquals("Password is valid.", message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~awiseusabacu", null, new List<string> { "wise.tech@email.com" });
			Assert("Password contains a word from the user email", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaWISEusabacu", null, new List<string> { "wise.tech@email.com" });
			Assert("Password contains a word from the user email (not case sensitive)", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaTECHusabacu", new List<string> { "wise tech" }, new List<string> { "wise.tech@email.com" });
			Assert("Password contains a word from the user email", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaTECHusabacu", null, new List<string> { "wise_tech@email.com" });
			Assert("Password contains a blacklisted string", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaTECHusabacu", null, new List<string> { "wise-tech@email.com" });
			Assert("Password contains a blacklisted string", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaTECHusabacu", null, new List<string> { "wise-tech2@email.com" });
			Assert("Password contains a blacklisted string", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaBADusabacu", null, new List<string> { "wise-bad.tech@email.com" });
			Assert("Password should be valid, words should have 3 characters or more", isSuccess);
			AssertEquals("Password is valid.", message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaTECHusabacu", null, new List<string> { "wise-tech(15)@email.com" });
			Assert("Password contains a blacklisted string", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			using (WebDataRegistry.Instance.EnableWebPasswordNameAndEmailValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abaTECHusabacu", null, new List<string> { "wise-tech(15)@email.com" });
				Assert("Should not validate name when registry key is not enabled", isSuccess);
				AssertEquals("Password is valid.", message);
			}
		}

		public void TestPasswordContainsNameOrEmail_ShouldRemoveSingleAndDoubleLetterWords()
		{
			var fullname = "Stephen A Ng Ing Alex";
			AssertEquals("Should contain names longer than 3 characters", true, ContactPasswordValidator.PasswordContainsNameOrEmail("Stephen", new List<string> { fullname }, null));
			AssertEquals("Should not contain names 3 characters or less", false, ContactPasswordValidator.PasswordContainsNameOrEmail("Ing", new List<string> { fullname }, null));
			AssertEquals("Should contain names longer than 3 characters", true, ContactPasswordValidator.PasswordContainsNameOrEmail("Alex", new List<string> { fullname }, null));
			AssertEquals("Should not contain names 3 characters or less", false, ContactPasswordValidator.PasswordContainsNameOrEmail("A", new List<string> { fullname }, null));
			AssertEquals("Should not contain names 3 characters or less", false, ContactPasswordValidator.PasswordContainsNameOrEmail("Ng", new List<string> { fullname }, null));
		}

		public void TestPasswordContainsNameOrEmail_ShouldRemoveNumbers()
		{
			var fullname = "Stephen (1) Alex";
			AssertEquals("Should contain names longer than 3 characters", true, ContactPasswordValidator.PasswordContainsNameOrEmail("Stephen", new List<string> { fullname }, null));
			AssertEquals("Should contain names longer than 3 characters", true, ContactPasswordValidator.PasswordContainsNameOrEmail("Alex", new List<string> { fullname }, null));
			AssertEquals("Should exclude number/special characters", false, ContactPasswordValidator.PasswordContainsNameOrEmail("(1)", new List<string> { fullname }, null));
		}

		public void TestExcludeList()
		{
			var excludeList = new List<string> { "WiSe", "Test", "bAd", "HoUse" };
			using (WebDataRegistry.Instance.PasswordBannedWordList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, excludeList.ToArray()))
			{
				var (isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abacusabacu");
				Assert("Password should be valid", isSuccess);
				AssertEquals("Password is valid.", message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~ababAdabacu");
				Assert("Password should not be valid", !isSuccess);
				AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abarhouseseabacu");
				Assert("Password should not be valid", !isSuccess);
				AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);
			}
		}

		public void TestIsValidPassword_MultipleNamesAndEmails()
		{
			var names = new List<string> { "Test Fullname", "Another Name" };
			var emails = new List<string> { "wise.tech@email.com", "some_thing@gmail.com" };

			var (isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~abacusabacu", names, emails);
			Assert("Password should be valid", isSuccess);
			AssertEquals("Password is valid.", message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~awiseusabacu", names, emails);
			Assert("Password contains a word from the user email", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~asomeusabacu", names, emails);
			Assert("Password contains a word from the user email", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~atestusabacu", names, emails);
			Assert("Password contains a word from the user name", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);

			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Aa1~aanotherusabacu", names, emails);
			Assert("Password contains a word from the user name", !isSuccess);
			AssertEquals(ContactPasswordValidator.ExcludedStringErrorMessage, message);
		}

		public void TestGetAllPasswordRequirements()
		{
			var result = ContactPasswordValidator.GetAllPasswordRequirements;
			var expected = new List<PasswordRequirements>()
			{
				PasswordRequirements.MinLength,
				PasswordRequirements.MaxLength,
				PasswordRequirements.ContainsLowerCaseLetter,
				PasswordRequirements.ContainsUpperCaseLetter,
				PasswordRequirements.ContainsNumber,
				PasswordRequirements.ContainsSpecialCharacter
			};
			AssertContainsExactElementsInAnyOrder(expected, result.Keys.ToList<PasswordRequirements>());
		}

		public void TestGetAllPasswordRequirements_EnableWebPasswordComplexityRules()
		{
			using (WebDataRegistry.Instance.EnableWebPasswordComplexityRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var result = ContactPasswordValidator.GetAllPasswordRequirements;
				var expected = new List<PasswordRequirements>()
				{
					PasswordRequirements.MinLength,
					PasswordRequirements.MaxLength,
					PasswordRequirements.ContainsAtLeastThreeOfTheFollowing
				};
				AssertContainsExactElementsInAnyOrder(expected, result.Keys.ToList<PasswordRequirements>());
			}
		}

		public void TestIsValidPasswordPasswordHistory()
		{
			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.SetHashedPassword("Aa1~abacusaba");
			Factory.Save();
			contact.SetHashedPassword("Aa1~abacusaba+");
			Factory.Save();
			contact.SetHashedPassword("Aa1~abacusaba++");
			Factory.Save();

			var (isSuccess, message) = ContactPasswordValidator.IsValidPassword(contact, "Aa1~abacusaba");
			Assert(!isSuccess);
			AssertEquals("The password entered has been used previously. Please enter a new password.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(contact, "Aa1~abacusaba+");
			Assert(!isSuccess);
			AssertEquals("The password entered has been used previously. Please enter a new password.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(contact, "Aa1~abacusaba++");
			Assert(!isSuccess);
			AssertEquals("The password entered has been used previously. Please enter a new password.", message);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(contact, "Aa1~abacusaba+++");
			Assert(isSuccess);
			AssertEquals("Password is valid.", message);

			WebDataRegistry.Instance.WebPasswordHistoryCount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			(isSuccess, message) = ContactPasswordValidator.IsValidPassword(contact, "Aa1~abacusaba");
			Assert(isSuccess);
			AssertEquals("Password is valid.", message);
		}

		public void TestComplexityRules()
		{
			/* 
			To satisfy password complexity, the password must contain characters from at least 3 of the 5 sets:
			- Uppercase letters (including those with diacritics and other cased alphabets like Greek and Cyrillic)
			- Lowercase letters (likewise)
			- Numeric characters (including Eastern Arabic digits, superscripts, fractions)
			- Symbols (including punctuation)
			- Letters that aren't upper or lower case (unicameral scripts, such as Chinese, Devanagari, Arabic, Hebrew)

			These are similar but not identical to Microsoft's complexity policy (https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/password-must-meet-complexity-requirements);
			for example, we consider currency signs to be symbols.

			Characters from beyond the Unicode Basic Multilingual Plane (such as most emoji) do not get counted.
			This is due to limitations with .NET Framework: Unicode property methods are on the char type, which only holds one UTF16 code unit; .NET Core has the Rune type for this.
			*/

			using (WebDataRegistry.Instance.WebPasswordMinLength.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (WebDataRegistry.Instance.EnableWebPasswordComplexityRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var (isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Testtttttttt");
				Assert("Password should not be valid", !isSuccess);
				AssertEquals(ContactPasswordValidator.PasswordComplexityRulesMessage, message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Test1tttttttt");
				Assert("Password should be valid", isSuccess);
				AssertEquals("Password is valid.", message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "te$ttttttttt");
				Assert("Password should not be valid", !isSuccess);
				AssertEquals(ContactPasswordValidator.PasswordComplexityRulesMessage, message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Te~ttttttttt");
				Assert("Password should be valid", isSuccess);
				AssertEquals("Password is valid.", message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "t&st1tttttttt");
				Assert("Password should be valid", isSuccess);
				AssertEquals("Password is valid.", message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "Tã1111111");
				Assert("Password should be valid", isSuccess);
				AssertEquals("Password is valid.", message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "testþtttttttt");
				Assert("Password should not be valid", !isSuccess);
				AssertEquals(ContactPasswordValidator.PasswordComplexityRulesMessage, message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "aaaaaa1111111א");
				Assert("Password should be valid", isSuccess);
				AssertEquals("Password is valid.", message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "111aaaaaa");
				Assert("Password should not be valid", !isSuccess);
				AssertEquals(ContactPasswordValidator.PasswordComplexityRulesMessage, message);

				(isSuccess, message) = ContactPasswordValidator.IsValidPassword(null, "111aaaaaa中文");
				Assert("Password should be valid", isSuccess);
				AssertEquals("Password is valid.", message);
			}
		}
	}
}

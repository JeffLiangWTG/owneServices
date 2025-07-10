using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Enterprise.Registry.Business;
using WTG.Foundation.Cryptography.UserSecrets;

namespace Enterprise.MasterFiles.Business
{
	public static class ContactPasswordValidator
	{
		public enum PasswordRequirements
		{
			MinLength,
			MaxLength,
			ContainsLowerCaseLetter,
			ContainsUpperCaseLetter,
			ContainsNumber,
			ContainsSpecialCharacter,
			ContainsAtLeastThreeOfTheFollowing
		}

		public static (bool isSuccess, string message) IsValidPassword(IGlbPasswordHistoryParent parent, string password, List<string> fullNames = null, List<string> emails = null)
		{
			var minimumPasswordLength = WebDataRegistry.Instance.WebPasswordMinLength.Value;
			if (password.Length < minimumPasswordLength)
			{
				return (false, Res.GetString("8b3c82a8-5363-4004-8f21-f3e7ac826863", "Password must be at least {0} characters long.", minimumPasswordLength));
			}

			if (PasswordContainsNameOrEmail(password, fullNames, emails) || PasswordContainsExcludeListWord(password))
			{
				return (false, ExcludedStringErrorMessage);
			}

			if (WebDataRegistry.Instance.EnableWebPasswordComplexityRules.Value)
			{
				if (!UserSecretsPolicyEnforcement.MeetsStandardComplexityRequirements(password))
				{
					return (false, PasswordComplexityRulesMessage);
				}
			}
			else
			{
				var regex = new Regex("^([^0-9]*|[^A-Z]*|[^a-z]*|[a-zA-Z0-9]*)$");
				if (regex.IsMatch(password))
				{
					return (false, Res.GetString("7803ac27-6ef9-4b07-82f7-a59c1eef2d1f", "Password must contain at least one lower case letter, one upper case letter, one number and one special character."));
				}
			}

			if (parent != null && parent.PasswordHistoryCount > 0 && PasswordHistoryHelper.HasPasswordBeenUsed(parent, password))
			{
				return (false, Res.GetString("988962ff-6959-4a16-a547-7688bd2c0ced", "The password entered has been used previously. Please enter a new password."));
			}

			return (true, Res.GetString("3ca88aaf-a082-4b48-ada9-da4e6c3050c4", "Password is valid."));
		}

		internal static bool PasswordContainsNameOrEmail(string password, List<string> fullNames, List<string> emails)
		{
			if (WebDataRegistry.Instance.EnableWebPasswordNameAndEmailValidation.Value)
			{
				if (fullNames != null)
				{
					foreach (var fullName in fullNames)
					{
						if (UserSecretsPolicyEnforcement.ContainsFullName(password.AsSpan(), fullName.AsSpan()))
						{
							return true;
						}
					}
				}

				if (emails != null)
				{
					foreach (var email in emails)
					{
						if (!string.IsNullOrEmpty(email) && email.Contains("@"))
						{
							if (UserSecretsPolicyEnforcement.ContainsEmailAddress(password.AsSpan(), email.AsSpan()))
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		static bool PasswordContainsExcludeListWord(string password)
		{
			var result = false;
			var excludeListString = WebDataRegistry.Instance.PasswordBannedWordList.Value;

			if (excludeListString?.Length > 0)
			{
				return UserSecretsPolicyEnforcement.ContainsBannedWord(password, excludeListString.ToImmutableArray());
			}

			return result;
		}

		public static Dictionary<PasswordRequirements, int> GetAllPasswordRequirements
		{
			get
			{
				var result = new Dictionary<PasswordRequirements, int>();

				result.Add(PasswordRequirements.MinLength, WebDataRegistry.Instance.WebPasswordMinLength.Value);
				result.Add(PasswordRequirements.MaxLength, OrgContact.PasswordMaxLength);

				if (WebDataRegistry.Instance.EnableWebPasswordComplexityRules.Value)
				{
					result.Add(PasswordRequirements.ContainsAtLeastThreeOfTheFollowing, 1);
				}
				else
				{
					result.Add(PasswordRequirements.ContainsLowerCaseLetter, 1);
					result.Add(PasswordRequirements.ContainsUpperCaseLetter, 1);
					result.Add(PasswordRequirements.ContainsNumber, 1);
					result.Add(PasswordRequirements.ContainsSpecialCharacter, 1);
				}

				return result;
			}
		}

		public static string PasswordComplexityRulesMessage => Res.GetString("92eb10fd-a0ec-4590-a49c-c86e7445c5e6", "Password must contain at least three of the following: uppercase letters, lowercase letters, numbers, symbols, and non-European alphabet characters.");
		public static string ExcludedStringErrorMessage => Res.GetString("6260a236-fc27-4a59-9620-5eec25a7203e", "The password appears to contain part of your name, email or a word which has been disallowed by your system administrator.");
	}
}

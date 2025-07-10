using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business
{
	public static class ABICharactersValidator
	{
		public static void ValidateCharacters(ZPropertyInfo info, bool isWarningOnly = false)
		{
			CharactersValidator.ValidateCharacters(info, isWarningOnly);
		}

		public static void ValidateCharacters(ZPropertyInfo info, ZString valueToValidate, bool isWarningOnly = false, string humanReadableName = "")
		{
			CharactersValidator.ValidateCharacters(info, valueToValidate, isWarningOnly, humanReadableName: humanReadableName);
		}
	}

	public static class AMSCharactersValidator
	{
		public static void ValidateCharacters(ZPropertyInfo info, bool isWarningOnly = false)
		{
			CharactersValidator.ValidateCharacters(info, isWarningOnly, CBPEDIInterchange.ApplicationCodes.AMS);
		}
	}

	static class CharactersValidator
	{
		public static void ValidateCharacters(ZPropertyInfo info, bool isWarningOnly = false, string applicationCode = "")
		{
			ValidateCharacters(info, (ZString)info.Value, isWarningOnly, applicationCode);
		}

		public static void ValidateCharacters(ZPropertyInfo info, ZString valueToValidate, bool isWarningOnly = false, string applicationCode = "", string humanReadableName = "")
		{
			var stringToBeValidated = valueToValidate.Replace(" ", "").Trim();
			if (stringToBeValidated.Length > 0)
			{
				var characterTypeToReplaceWith = MessageBlockStringAttribute.GetCharacterTypeToReplaceWith(applicationCode);
				var noOfInvalidChars = stringToBeValidated.Length - MessageBlockStringAttribute.ReplaceWithValidCharacters(stringToBeValidated, applicationCode, "").Length;
				humanReadableName = string.IsNullOrEmpty(humanReadableName) ? info.HumanReadableName : (ZString)humanReadableName;
				if (noOfInvalidChars == stringToBeValidated.Length && !isWarningOnly)
				{
					info.AddMessageError(ValidationConstants.Characters.InvalidCharacters(humanReadableName, characterTypeToReplaceWith));
				}
				else if (noOfInvalidChars > 0)
				{
					info.AddWarning(ValidationConstants.Characters.InvalidCharacters(humanReadableName, characterTypeToReplaceWith));
				}
			}
		}
	}
}

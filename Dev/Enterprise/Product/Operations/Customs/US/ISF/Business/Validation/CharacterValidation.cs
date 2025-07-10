using CargoWise.Common.Testing;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.ISF.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class CharacterValidation
	{
		public enum Type { AlphaNumeric, AlphaOnly, NumericOnly, Mix }

		public static void Validate(ZPropertyInfo info, Type validType)
		{
			ZPropertyInfoString infoToCheck = (ZPropertyInfoString)info;
			switch (validType)
			{
				case CharacterValidation.Type.AlphaNumeric:
					CheckOnlyAlphaNumericIsEntered(infoToCheck);
					break;
				case CharacterValidation.Type.NumericOnly:
					CheckOnlyNumericIsEntered(infoToCheck);
					break;
				case CharacterValidation.Type.AlphaOnly:
					CheckOnlyAlphaIsEntered(infoToCheck);
					break;
				default:
					// do nothing
					break;
			}
		}

		static void CheckOnlyAlphaIsEntered(ZPropertyInfoString info)
		{
			if (!info.Value.IsLettersOnlyOrEmpty)
			{
				info.AddMessageError(ValidationConstants.Character.OnlyAlphabeticCharactersAreAllowed);
			}
		}

		static void CheckOnlyNumericIsEntered(ZPropertyInfoString info)
		{
			if (!info.Value.IsNumbersOnlyOrEmpty)
			{
				info.AddMessageError(ValidationConstants.Character.OnlyNumericCharactersAreAllowed);
			}
		}

		static void CheckOnlyAlphaNumericIsEntered(ZPropertyInfoString info)
		{
			if (!info.Value.IsLettersAndNumbersOnlyOrEmpty)
			{
				info.AddMessageError(ValidationConstants.Character.OnlyAlphaNumericCharactersAreAllowed);
			}
		}
	}
}

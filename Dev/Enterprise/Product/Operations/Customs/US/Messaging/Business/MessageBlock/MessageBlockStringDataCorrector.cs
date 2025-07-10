using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface ICharacterTypeString
	{
		string Special { get; }
		string Alphanumeric { get; }
		string Alphabetic { get; }
		string Numeric { get; }
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("ABICharacterTypeString")]
	public class ABICharacterTypeString : ICharacterTypeString
	{
		public static class Constants
		{
			public const string Special = @"!""#$%&'*()+,-./0123456789:; <=>?@ABCDEFGHIJKLMNOPQ RSTUVWXYZ[\]^_`{|}~¢";
			public const string Alphanumeric = Alphabetic + Numeric;
			public const string Alphabetic = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
			public const string Numeric = "0123456789";
		}

		public string Alphabetic => Constants.Alphabetic;
		public string Alphanumeric => Constants.Alphanumeric;
		public string Numeric => Constants.Numeric;
		public string Special => Constants.Special;
	}
	[WTG.StaticAnalysis.Annotation.CodeAlive("AMSCharacterTypeString")]
	public class AMSCharacterTypeString : ICharacterTypeString
	{
		public static class Constants
		{
			public const string Special = @"!""#$%&'()+,-./0123456789:; <=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\\]^_`{|}~";
			public const string Alphanumeric = Alphabetic + Numeric;
			public const string Alphabetic = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
			public const string Numeric = "0123456789";
		}

		public string Alphabetic => Constants.Alphabetic;
		public string Alphanumeric => Constants.Alphanumeric;
		public string Numeric => Constants.Numeric;
		public string Special => Constants.Special;
	}

	public static class MessageBlockStringDataCorrector
	{
		public enum CharacterType { Alphabetic, Numeric, Alphanumeric, Special }

		public static ZString KeepOnlyValidCharacters(ZString value, CharacterType type, int length, ICharacterTypeString characterTypeString = null)
		{
			return KeepOnlyValidCharacters(value, GetCharactersToKeep(type, characterTypeString), length);
		}

		public static ZString KeepOnlyValidCharacters(ZString value, ZString validCharacters, int length)
		{
			return value.ToUpper().KeepChars(validCharacters).Left(length);
		}

		public static ZString ReplaceInvalidCharacters(ZString value, ZString validCharacters, char characterToReplaceWith)
		{
			var result = new ZStringBuilder();

			foreach (var character in value.ToUpperInvariant())
			{
				result.Append(new ZString(validCharacters.Contains(character) ? character : characterToReplaceWith));
			}
			return result.ToString();
		}

		static ZString GetCharactersToKeep(CharacterType type, ICharacterTypeString characterTypeString = null)
		{
			characterTypeString = characterTypeString ?? new ABICharacterTypeString();
			switch (type)
			{
				case CharacterType.Alphabetic:
					return characterTypeString.Alphabetic;
				case CharacterType.Alphanumeric:
					return characterTypeString.Alphanumeric;
				case CharacterType.Numeric:
					return characterTypeString.Numeric;
				default:
					return characterTypeString.Special;
			}
		}
	}
}

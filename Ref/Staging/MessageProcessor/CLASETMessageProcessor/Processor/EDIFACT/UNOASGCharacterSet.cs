using System.Globalization;
using Enterprise.Edifact;

namespace CargoWise.RefDbRepo.Staging.CLASETMessageProcessor
{
	public sealed class UNOASGCharacterSet : UNOACharacterSet
	{
		/// <summary>
		/// For use with Singapore TradeNet Version 4
		/// </summary>
		public const string SingaporeValidCharacters = ValidCharacters + @"@\$_#";

		public UNOASGCharacterSet()
		{
			SetStandardDelimiters();
			validCharacters = SingaporeValidCharacters + AllDelimiterCharacters();
			replaceEscapedCharactersWithSpace = false;
		}

		protected override string EnforceCase(string dataPiece)
		{
			return dataPiece.ToUpper(CultureInfo.InvariantCulture);
		}

		protected override string ReplaceIllegalCharacters(string element)
		{
			return KeepChars(element, validCharacters, " ");
		}
	}
}

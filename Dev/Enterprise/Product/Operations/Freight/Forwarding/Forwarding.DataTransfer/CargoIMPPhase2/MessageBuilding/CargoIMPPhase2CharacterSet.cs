using System;
using Enterprise.Edifact;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CargoIMPPhase2CharacterSet : UNCharacterSet
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Valid Characters")]
		protected internal const string ValidCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789~`!@#$%^&*()-_=+[{]}\|:""<.>/?";
		public static readonly char NotDefinedCharacter = Convert.ToChar(255);

		public CargoIMPPhase2CharacterSet()
			: base(NotDefinedCharacter, ',', NotDefinedCharacter, '\n')
		{
			validCharacters = ValidCharacters;
		}
	}
}

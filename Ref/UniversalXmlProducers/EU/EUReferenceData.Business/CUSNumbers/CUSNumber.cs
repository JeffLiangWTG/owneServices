using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public class CUSNumber
	{
		public CUSNumber(string code, string description, string cnCode, string casRn, string ecNumber, string unNumber, IEnumerable<CUSTranslatedName> translations)
		{
			Code = code;
			Description = description;
			CNCode = cnCode;
			CasRn = casRn;
			EcNumber = ecNumber;
			UnNumber = unNumber;
			Translations = translations;
		}

		public string Code { get; }

		public string Description { get; }

		// attributes
		public string CNCode { get; }

		public string CasRn { get; }

		public string EcNumber { get; }

		public string UnNumber { get; }

		public IEnumerable<CUSTranslatedName> Translations { get; }
	}
}
